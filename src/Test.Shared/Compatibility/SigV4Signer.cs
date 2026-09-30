namespace Test.Shared.Compatibility
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Minimal AWS Signature Version 4 signer for raw S3 requests, independent of the AWS SDK and of the signature
    /// validator in S3Server, so the same signed request can be sent to Amazon S3 and to S3Server.
    /// Thread-safe; all members are stateless.
    /// </summary>
    public static class SigV4Signer
    {
        /// <summary>
        /// URI-encode a value per the AWS Signature Version 4 rules: unreserved characters (A-Z, a-z, 0-9, '-', '_', '.', '~')
        /// are kept, and every other UTF-8 byte becomes %XX with uppercase hex digits.
        /// </summary>
        /// <param name="value">Value.  Null is treated as empty.</param>
        /// <param name="keepSlash">True to keep '/' unencoded (object key paths).</param>
        /// <returns>Encoded value.</returns>
        public static string UriEncode(string value, bool keepSlash)
        {
            if (String.IsNullOrEmpty(value)) return "";

            StringBuilder sb = new StringBuilder();
            foreach (byte b in Encoding.UTF8.GetBytes(value))
            {
                char c = (char)b;
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '_' || c == '.' || c == '~')
                    sb.Append(c);
                else if (c == '/' && keepSlash)
                    sb.Append(c);
                else
                    sb.Append('%').Append(b.ToString("X2", CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Build the canonical (and sent) querystring from name and value pairs.
        /// </summary>
        /// <param name="query">Query pairs.  A null value is sent as an empty value.</param>
        /// <returns>Querystring without the leading '?', or empty.</returns>
        public static string CanonicalQuery(IEnumerable<KeyValuePair<string, string>> query)
        {
            if (query == null) return "";

            return String.Join("&", query
                .Select(p => UriEncode(p.Key, false) + "=" + UriEncode(p.Value, false))
                .OrderBy(s => s, StringComparer.Ordinal));
        }

        /// <summary>
        /// Sign a request by adding the x-amz-date, x-amz-content-sha256, and Authorization headers.
        /// Signs host, content-md5 and content-type when present, and every x-amz-* header.
        /// </summary>
        /// <param name="request">Request whose RequestUri already carries the encoded path and canonical query.</param>
        /// <param name="body">Request body.  Null is treated as empty.</param>
        /// <param name="accessKey">Access key.</param>
        /// <param name="secretKey">Secret key.</param>
        /// <param name="region">Region.</param>
        /// <param name="now">Signing time (UTC).</param>
        public static void Sign(HttpRequestMessage request, byte[] body, string accessKey, string secretKey, string region, DateTime now)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (body == null) body = Array.Empty<byte>();

            string amzDate = now.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
            string date = now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            string payloadHash = Hex(SHA256.HashData(body));

            request.Headers.Remove("x-amz-date");
            request.Headers.Remove("x-amz-content-sha256");
            request.Headers.TryAddWithoutValidation("x-amz-date", amzDate);
            request.Headers.TryAddWithoutValidation("x-amz-content-sha256", payloadHash);

            SortedDictionary<string, string> signed = new SortedDictionary<string, string>(StringComparer.Ordinal);
            signed["host"] = request.RequestUri.IsDefaultPort ? request.RequestUri.Host : request.RequestUri.Authority;

            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                string name = header.Key.ToLowerInvariant();
                if (name.StartsWith("x-amz-", StringComparison.Ordinal)) signed[name] = String.Join(",", header.Value).Trim();
            }

            if (request.Content != null)
            {
                foreach (KeyValuePair<string, IEnumerable<string>> header in request.Content.Headers)
                {
                    string name = header.Key.ToLowerInvariant();
                    if (name == "content-md5" || name == "content-type") signed[name] = String.Join(",", header.Value).Trim();
                }
            }

            string signedHeaders = String.Join(";", signed.Keys);
            string canonicalHeaders = String.Join("", signed.Select(h => h.Key + ":" + h.Value + "\n"));
            string query = request.RequestUri.Query.StartsWith("?", StringComparison.Ordinal) ? request.RequestUri.Query.Substring(1) : request.RequestUri.Query;

            string canonicalRequest =
                request.Method.Method + "\n"
                + request.RequestUri.AbsolutePath + "\n"
                + query + "\n"
                + canonicalHeaders + "\n"
                + signedHeaders + "\n"
                + payloadHash;

            string scope = date + "/" + region + "/s3/aws4_request";
            string stringToSign = "AWS4-HMAC-SHA256\n" + amzDate + "\n" + scope + "\n" + Hex(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest)));

            byte[] key = Hmac(Encoding.UTF8.GetBytes("AWS4" + secretKey), date);
            key = Hmac(key, region);
            key = Hmac(key, "s3");
            key = Hmac(key, "aws4_request");
            string signature = Hex(Hmac(key, stringToSign));

            request.Headers.TryAddWithoutValidation(
                "Authorization",
                "AWS4-HMAC-SHA256 Credential=" + accessKey + "/" + scope + ", SignedHeaders=" + signedHeaders + ", Signature=" + signature);
        }

        private static byte[] Hmac(byte[] key, string data)
        {
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            }
        }

        private static string Hex(byte[] bytes)
        {
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
