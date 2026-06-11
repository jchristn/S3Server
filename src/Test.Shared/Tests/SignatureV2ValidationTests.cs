namespace Test.Shared.Tests
{
    using System;
    using System.Collections.Specialized;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AWSSignatureGenerator;

    /// <summary>
    /// Legacy AWS Signature V2 validation tests.
    /// These tests run against a server with EnableSignatures and EnableSignatureV2 both enabled.
    /// </summary>
    public static class SignatureV2ValidationTests
    {
        /// <summary>
        /// Run all signature V2 validation tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server with V2 signatures enabled.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            await runner.RunTestAsync("Valid V2 header signature allows ListBuckets", async (ct) =>
            {
                HttpRequestMessage request = CreateV2HeaderRequest(server, HttpMethod.Get, server.BaseUrl + "/");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "V2 ListBuckets");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "ListAllMyBucketsResult", "response body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid V2 header signature allows GetObject", async (ct) =>
            {
                HttpRequestMessage request = CreateV2HeaderRequest(server, HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "V2 GetObject");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.AreEqual("hello", body, "object body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid V2 header signature allows PutObject with Content-MD5 and Content-Type", async (ct) =>
            {
                byte[] body = Encoding.UTF8.GetBytes("v2 upload body");
                string md5 = ComputeMd5Base64(body);
                string contentType = "text/plain";
                string url = server.BaseUrl + "/" + server.Bucket + "/v2-upload.txt";

                NameValueCollection headers = CreateDateHeaders();
                headers.Add("Content-MD5", md5);
                headers.Add("Content-Type", contentType);

                HttpRequestMessage request = CreateV2HeaderRequest(server, HttpMethod.Put, url, headers);
                request.Content = new ByteArrayContent(body);
                request.Content.Headers.ContentMD5 = Convert.FromBase64String(md5);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "V2 PutObject");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid V2 header signature allows x-amz-date without Date", async (ct) =>
            {
                string url = server.BaseUrl + "/" + server.Bucket + "/test-object.txt";
                NameValueCollection headers = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
                headers.Add("x-amz-date", DateTime.UtcNow.ToString("R"));

                HttpRequestMessage request = CreateV2HeaderRequest(server, HttpMethod.Get, url, headers);
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "V2 x-amz-date GetObject");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("V2 header signature passes parsed access key to credential lookup", async (ct) =>
            {
                server.ClearSecretKeyAccessKeys();

                HttpRequestMessage request = CreateV2HeaderRequest(server, HttpMethod.Get, server.BaseUrl + "/");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "V2 credential lookup header request");
                AssertHelper.IsTrue(server.SecretKeyAccessKeys.TryDequeue(out string accessKey), "credential lookup should be invoked");
                AssertHelper.AreEqual(server.AccessKey, accessKey, "credential lookup access key");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid V2 signed URL allows GetObject", async (ct) =>
            {
                string url = CreateV2SignedUrl(server, HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                HttpResponseMessage response = await server.HttpClient.GetAsync(url, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "V2 signed URL GetObject");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.AreEqual("hello", body, "object body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("V2 signed URL passes parsed access key to credential lookup", async (ct) =>
            {
                server.ClearSecretKeyAccessKeys();

                string url = CreateV2SignedUrl(server, HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                HttpResponseMessage response = await server.HttpClient.GetAsync(url, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "V2 credential lookup signed URL");
                AssertHelper.IsTrue(server.SecretKeyAccessKeys.TryDequeue(out string accessKey), "credential lookup should be invoked");
                AssertHelper.AreEqual(server.AccessKey, accessKey, "credential lookup access key");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid V2 signed URL allows bucket ACL subresource", async (ct) =>
            {
                string url = CreateV2SignedUrl(server, HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "?acl");
                HttpResponseMessage response = await server.HttpClient.GetAsync(url, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "V2 signed URL GetBucketAcl");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "AccessControlPolicy", "ACL body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Wrong V2 secret key is rejected", async (ct) =>
            {
                string url = server.BaseUrl + "/";
                NameValueCollection headers = CreateDateHeaders();
                using (V2SignatureResult result = new V2SignatureResult("GET", url, server.AccessKey, "wrong-secret", headers, null))
                {
                    HttpRequestMessage request = CreateRequestWithHeaders(HttpMethod.Get, url, headers);
                    request.Headers.TryAddWithoutValidation("Authorization", result.AuthorizationHeader);

                    HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "wrong V2 secret");
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    AssertHelper.StringContains(body, "SignatureDoesNotMatch", "error body");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Unknown V2 access key is rejected", async (ct) =>
            {
                string accessKey = "AKIAUNKNOWNV2KEY";
                string secretKey = "unknown-secret";
                string url = server.BaseUrl + "/";
                NameValueCollection headers = CreateDateHeaders();
                using (V2SignatureResult result = new V2SignatureResult("GET", url, accessKey, secretKey, headers, null))
                {
                    HttpRequestMessage request = CreateRequestWithHeaders(HttpMethod.Get, url, headers);
                    request.Headers.TryAddWithoutValidation("Authorization", result.AuthorizationHeader);

                    HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "unknown V2 access key");
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    AssertHelper.StringContains(body, "AccessDenied", "error body");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Expired V2 signed URL is rejected", async (ct) =>
            {
                long expires = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds();
                string url = CreateV2SignedUrl(server, HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt", expires);
                HttpResponseMessage response = await server.HttpClient.GetAsync(url, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "expired V2 signed URL");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "AccessDenied", "error body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Tampered V2 signed URL key is rejected", async (ct) =>
            {
                string signedUrl = CreateV2SignedUrl(server, HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                string tampered = signedUrl.Replace("/test-object.txt", "/tampered-object.txt");

                HttpResponseMessage response = await server.HttpClient.GetAsync(tampered, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "tampered V2 signed URL");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "SignatureDoesNotMatch", "error body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("V2 header signature without Date or x-amz-date is rejected", async (ct) =>
            {
                string url = server.BaseUrl + "/";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("Authorization", "AWS " + server.AccessKey + ":invalid");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "missing V2 date");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "SignatureDoesNotMatch", "error body");
            }, token).ConfigureAwait(false);
        }

        private static HttpRequestMessage CreateV2HeaderRequest(S3TestServer server, HttpMethod method, string url, NameValueCollection headers = null)
        {
            if (headers == null)
                headers = CreateDateHeaders();

            using (V2SignatureResult result = new V2SignatureResult(method.Method, url, server.AccessKey, server.SecretKey, headers, null))
            {
                HttpRequestMessage request = CreateRequestWithHeaders(method, url, headers);
                request.Headers.TryAddWithoutValidation("Authorization", result.AuthorizationHeader);
                return request;
            }
        }

        private static string CreateV2SignedUrl(S3TestServer server, HttpMethod method, string url, long? expires = null)
        {
            long expiresValue = expires ?? DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();

            using (V2SignedUrlResult result = new V2SignedUrlResult(method.Method, url, server.AccessKey, server.SecretKey, expiresValue, null, null))
            {
                return result.SignedUrl;
            }
        }

        private static HttpRequestMessage CreateRequestWithHeaders(HttpMethod method, string url, NameValueCollection headers)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, url);

            foreach (string key in headers.AllKeys)
            {
                if (String.Equals(key, "Content-MD5", StringComparison.OrdinalIgnoreCase)
                    || String.Equals(key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                request.Headers.TryAddWithoutValidation(key, headers[key]);
            }

            return request;
        }

        private static NameValueCollection CreateDateHeaders()
        {
            NameValueCollection headers = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
            headers.Add("Date", DateTime.UtcNow.ToString("R"));
            return headers;
        }

        private static string ComputeMd5Base64(byte[] data)
        {
            using (MD5 md5 = MD5.Create())
            {
                return Convert.ToBase64String(md5.ComputeHash(data));
            }
        }
    }
}
