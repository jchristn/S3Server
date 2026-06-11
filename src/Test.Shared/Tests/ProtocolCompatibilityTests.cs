namespace Test.Shared.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Protocol compatibility tests for response headers, status codes, and XML shape.
    /// </summary>
    public static class ProtocolCompatibilityTests
    {
        /// <summary>
        /// Run all protocol compatibility tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            await runner.RunTestAsync("ListBuckets returns XML content type and request identifiers", async (ct) =>
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "ListBuckets");
                AssertContentTypeContains(response, "xml", "ListBuckets content type");
                AssertHeaderPresent(response, "x-amz-request-id");
                AssertHeaderPresent(response, "x-amz-id-2");

                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "<ListAllMyBucketsResult", "ListBuckets XML root");
                AssertHelper.StringContains(body, "<Owner>", "ListBuckets owner");
                AssertHelper.StringContains(body, "<Buckets>", "ListBuckets buckets");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Error response returns S3 XML content type and request identifiers", async (ct) =>
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/nonexistent-object-xyz.bin", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.NotFound, response, "NoSuchKey response");
                AssertContentTypeContains(response, "xml", "error content type");
                AssertHeaderPresent(response, "x-amz-request-id");
                AssertHeaderPresent(response, "x-amz-id-2");

                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "<Error>", "error root");
                AssertHelper.StringContains(body, "<Code>NoSuchKey</Code>", "error code");
                AssertHelper.StringContains(body, "<RequestId>", "error request ID");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("HeadObject returns object metadata headers without body dependency", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "HeadObject");
                AssertHeaderPresent(response, "ETag");
                AssertHeaderPresent(response, "Last-Modified");
                AssertHeaderPresent(response, "x-amz-storage-class");
                AssertContentTypeContains(response, "text/plain", "HeadObject content type");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("GetObject returns body metadata headers and content", async (ct) =>
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/test-object.txt", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "GetObject");
                AssertHeaderPresent(response, "ETag");
                AssertHeaderPresent(response, "Last-Modified");
                AssertHeaderPresent(response, "x-amz-storage-class");
                AssertContentTypeContains(response, "text/plain", "GetObject content type");
                AssertHelper.IsTrue(response.Content.Headers.ContentLength == 5, "content length is 5");

                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.AreEqual("hello", body, "object body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListObjects response echoes prefix delimiter and max keys", async (ct) =>
            {
                string url = server.BaseUrl + "/" + server.Bucket + "?prefix=logs%2F&delimiter=%2F&max-keys=2";
                HttpResponseMessage response = await server.HttpClient.GetAsync(url, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "ListObjects");
                AssertContentTypeContains(response, "xml", "ListObjects content type");

                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "<ListBucketResult", "ListBucketResult XML root");
                AssertHelper.StringContains(body, "<Prefix>logs/</Prefix>", "prefix echo");
                AssertHelper.StringContains(body, "<Delimiter>/</Delimiter>", "delimiter echo");
                AssertHelper.StringContains(body, "<MaxKeys>2</MaxKeys>", "max keys echo");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Delete bucket response uses 204 with empty body", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, server.BaseUrl + "/compat-delete-bucket");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.NoContent, response, "DeleteBucket");

                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.AreEqual(string.Empty, body, "DeleteBucket body");
            }, token).ConfigureAwait(false);
        }

        private static void AssertHeaderPresent(HttpResponseMessage response, string headerName)
        {
            bool exists = ContainsHeader(response.Headers, headerName)
                || ContainsHeader(response.Content.Headers, headerName);

            AssertHelper.IsTrue(exists, "header present: " + headerName);
        }

        private static bool ContainsHeader(HttpHeaders headers, string headerName)
        {
            try
            {
                return headers.Contains(headerName);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private static void AssertContentTypeContains(HttpResponseMessage response, string expected, string name)
        {
            string contentType = response.Content.Headers.ContentType?.ToString();
            AssertHelper.IsNotNull(contentType, name);
            AssertHelper.StringContains(contentType, expected, name);
        }
    }
}
