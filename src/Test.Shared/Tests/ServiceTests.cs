namespace Test.Shared.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.S3;
    using Amazon.S3.Model;

    /// <summary>
    /// Service-level API tests (ListBuckets, ServiceExists).
    /// </summary>
    public static class ServiceTests
    {
        /// <summary>
        /// Run all service API tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            Console.WriteLine();
            Console.WriteLine("--- Service API Tests ---");

            await runner.RunTestAsync("ServiceExists returns region", async (ct) =>
            {
                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                {
                    BucketName = "",
                    Verb = HttpVerb.HEAD,
                    Expires = DateTime.UtcNow.AddHours(1)
                };

                string url = server.S3Client.GetPreSignedURL(request);
                AssertHelper.IsNotNull(url, "pre-signed URL");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListBuckets returns bucket list", async (ct) =>
            {
                ListBucketsRequest request = new ListBucketsRequest();
                ListBucketsResponse response = await server.S3Client.ListBucketsAsync(request, ct).ConfigureAwait(false);

                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "ListBuckets");
                AssertHelper.IsNotNull(response.Buckets, "buckets");
                AssertHelper.IsNotEmpty(response.Buckets, "buckets");
                AssertHelper.AreEqual("default", response.Buckets[0].BucketName, "first bucket name");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Service GET returns ListAllMyBucketsResult XML", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "Service GET");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "ListAllMyBucketsResult", "response body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Service HEAD returns 200", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, server.BaseUrl + "/");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "Service HEAD");
            }, token).ConfigureAwait(false);
        }
    }
}
