namespace Test.Shared.Compatibility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An S3 endpoint the compatibility scenarios run against: Amazon S3 or an S3Server instance.
    /// Every object the scenarios create lives under Prefix, and cleanup removes only objects under Prefix,
    /// so existing data in the bucket is never modified.
    /// </summary>
    public class CompatTarget : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Endpoint, for example https://s3.us-west-1.amazonaws.com or http://127.0.0.1:8000.  Path-style requests are used.
        /// </summary>
        public string Endpoint { get; private set; }

        /// <summary>
        /// Bucket.
        /// </summary>
        public string Bucket { get; private set; }

        /// <summary>
        /// Region used for signing.
        /// </summary>
        public string Region { get; private set; }

        /// <summary>
        /// Key prefix for every object the scenarios create.  Ends with '/'.
        /// </summary>
        public string Prefix { get; private set; }

        /// <summary>
        /// True when the target is Amazon S3.
        /// </summary>
        public bool IsAmazon { get; private set; }

        #endregion

        #region Private-Members

        private string _AccessKey;
        private string _SecretKey;
        private HttpClient _Client;
        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="endpoint">Endpoint without a trailing slash.</param>
        /// <param name="bucket">Bucket.</param>
        /// <param name="region">Region.</param>
        /// <param name="accessKey">Access key.</param>
        /// <param name="secretKey">Secret key.</param>
        /// <param name="isAmazon">True when the target is Amazon S3.</param>
        /// <param name="prefix">Key prefix; null generates a unique s3server-compat-* prefix.</param>
        public CompatTarget(string endpoint, string bucket, string region, string accessKey, string secretKey, bool isAmazon, string prefix = null)
        {
            if (String.IsNullOrEmpty(endpoint)) throw new ArgumentNullException(nameof(endpoint));
            if (String.IsNullOrEmpty(bucket)) throw new ArgumentNullException(nameof(bucket));

            Endpoint = endpoint.TrimEnd('/');
            Bucket = bucket;
            Region = region ?? "us-west-1";
            _AccessKey = accessKey;
            _SecretKey = secretKey;
            IsAmazon = isAmazon;
            Prefix = prefix ?? ("s3server-compat-" + Guid.NewGuid().ToString("N").Substring(0, 10) + "/");
            if (!Prefix.EndsWith("/", StringComparison.Ordinal)) Prefix += "/";

            _Client = new HttpClient();
            _Client.Timeout = TimeSpan.FromSeconds(30);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Full key for a scenario object.
        /// </summary>
        /// <param name="name">Name under the prefix.</param>
        /// <returns>Key.</returns>
        public string Key(string name)
        {
            return Prefix + name;
        }

        /// <summary>
        /// Build a query list from alternating names and values.  A null value sends an empty value.
        /// </summary>
        /// <param name="nameValues">Alternating names and values.</param>
        /// <returns>Query pairs.</returns>
        public static List<KeyValuePair<string, string>> Query(params string[] nameValues)
        {
            List<KeyValuePair<string, string>> ret = new List<KeyValuePair<string, string>>();
            for (int i = 0; i + 1 < nameValues.Length; i += 2) ret.Add(new KeyValuePair<string, string>(nameValues[i], nameValues[i + 1]));
            if (nameValues.Length % 2 == 1) ret.Add(new KeyValuePair<string, string>(nameValues[nameValues.Length - 1], null));
            return ret;
        }

        /// <summary>
        /// Send a signed request.
        /// </summary>
        /// <param name="method">HTTP method.</param>
        /// <param name="key">Object key, or null for a bucket request.</param>
        /// <param name="query">Query pairs, or null.</param>
        /// <param name="headers">Additional headers, or null.  Content-Type and Content-MD5 are applied to the content.</param>
        /// <param name="body">Body, or null.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Response.</returns>
        public async Task<CompatResponse> SendAsync(
            HttpMethod method,
            string key,
            List<KeyValuePair<string, string>> query = null,
            Dictionary<string, string> headers = null,
            byte[] body = null,
            CancellationToken token = default)
        {
            string path = "/" + Bucket + (key != null ? "/" + SigV4Signer.UriEncode(key, true) : "");
            string qs = SigV4Signer.CanonicalQuery(query);
            string url = Endpoint + path + (qs.Length > 0 ? "?" + qs : "");

            using (HttpRequestMessage request = new HttpRequestMessage(method, url))
            {
                bool hasBody = body != null || method == HttpMethod.Put || method == HttpMethod.Post;
                if (hasBody)
                {
                    request.Content = new ByteArrayContent(body ?? Array.Empty<byte>());
                    request.Content.Headers.ContentType = null;
                }

                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> h in headers)
                    {
                        if (h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) && request.Content != null)
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(h.Value);
                        else if (h.Key.Equals("Content-MD5", StringComparison.OrdinalIgnoreCase) && request.Content != null)
                            request.Content.Headers.TryAddWithoutValidation("Content-MD5", h.Value);
                        else
                            request.Headers.TryAddWithoutValidation(h.Key, h.Value);
                    }
                }

                SigV4Signer.Sign(request, body, _AccessKey, _SecretKey, Region, DateTime.UtcNow);

                using (HttpResponseMessage response = await _Client.SendAsync(request, token).ConfigureAwait(false))
                {
                    Dictionary<string, string> captured = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (KeyValuePair<string, IEnumerable<string>> h in response.Headers) captured[h.Key] = String.Join(", ", h.Value);
                    foreach (KeyValuePair<string, IEnumerable<string>> h in response.Content.Headers) captured[h.Key] = String.Join(", ", h.Value);

                    byte[] responseBody = await response.Content.ReadAsByteArrayAsync(token).ConfigureAwait(false);
                    return new CompatResponse((int)response.StatusCode, captured, responseBody);
                }
            }
        }

        /// <summary>
        /// Delete every object under Prefix, including incomplete multipart uploads.  Objects outside Prefix are never touched.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Number of objects deleted.</returns>
        public async Task<int> CleanupAsync(CancellationToken token = default)
        {
            int deleted = 0;

            for (int page = 0; page < 100; page++)
            {
                CompatResponse list = await SendAsync(HttpMethod.Get, null, Query("list-type", "2", "prefix", Prefix), token: token).ConfigureAwait(false);
                if (list.StatusCode != 200) break;

                List<string> keys = list.Elements("Key", "Contents").Where(k => k.StartsWith(Prefix, StringComparison.Ordinal)).ToList();
                if (keys.Count < 1) break;

                foreach (string k in keys)
                {
                    CompatResponse del = await SendAsync(HttpMethod.Delete, k, token: token).ConfigureAwait(false);
                    if (del.StatusCode == 204 || del.StatusCode == 200) deleted++;
                }
            }

            CompatResponse uploads = await SendAsync(HttpMethod.Get, null, Query("uploads", null, "prefix", Prefix), token: token).ConfigureAwait(false);
            if (uploads.StatusCode == 200 && uploads.Xml != null)
            {
                foreach (System.Xml.Linq.XElement upload in uploads.Xml.Descendants().Where(e => e.Name.LocalName == "Upload"))
                {
                    string k = upload.Elements().First(e => e.Name.LocalName == "Key").Value;
                    string id = upload.Elements().First(e => e.Name.LocalName == "UploadId").Value;
                    if (k.StartsWith(Prefix, StringComparison.Ordinal))
                        await SendAsync(HttpMethod.Delete, k, Query("uploadId", id), token: token).ConfigureAwait(false);
                }
            }

            return deleted;
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            if (_Disposed) return;
            _Disposed = true;
            _Client?.Dispose();
        }

        #endregion
    }
}
