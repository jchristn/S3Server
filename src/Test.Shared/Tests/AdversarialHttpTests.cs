namespace Test.Shared.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Negative and adversarial HTTP tests.
    /// </summary>
    public static class AdversarialHttpTests
    {
        /// <summary>
        /// Run all adversarial HTTP tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            await runner.RunTestAsync("Non-numeric max-keys returns InvalidArgument", async (ct) =>
            {
                server.ClearObservedRequests();
                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "?max-keys=not-a-number", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.BadRequest, response, "invalid max-keys request");

                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "<Code>InvalidArgument</Code>", "error code");
                AssertHelper.IsNull(server.LastObservedRequest, "request never reached routing");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Negative max-keys returns InvalidArgument", async (ct) =>
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "?max-keys=-1", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.BadRequest, response, "negative max-keys");

                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "<Code>InvalidArgument</Code>", "error code");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Malformed range header is ignored and the full object is served", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                request.Headers.TryAddWithoutValidation("Range", "bytes=abc-def");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "malformed range");
                AssertHelper.AreEqual("hello", await response.Content.ReadAsStringAsync().ConfigureAwait(false), "full body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Inverted range header is ignored and the full object is served", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                request.Headers.TryAddWithoutValidation("Range", "bytes=4-1");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "inverted range");
                AssertHelper.AreEqual("hello", await response.Content.ReadAsStringAsync().ConfigureAwait(false), "full body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Oversized decoded content length is rejected", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + "/" + server.Bucket + "/decoded-too-large.bin");
                request.Headers.TryAddWithoutValidation("x-amz-decoded-content-length", "2048");
                request.Content = new StringContent("small-body", Encoding.UTF8, "application/octet-stream");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.BadRequest, response, "decoded length rejection");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "EntityTooLarge", "error body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Duplicate query parameters do not crash routing", async (ct) =>
            {
                server.ClearObservedRequests();
                string url = server.BaseUrl + "/" + server.Bucket + "?prefix=a&prefix=b&max-keys=2";
                HttpResponseMessage response = await server.HttpClient.GetAsync(url, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "duplicate query parameters");
                AssertHelper.IsNotNull(server.LastObservedRequest, "observed request");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("OPTIONS request returns default handler status and body", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Options, server.BaseUrl + "/" + server.Bucket + "/some-object");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "OPTIONS response");

                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "Handled by default handler", "OPTIONS body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Server remains usable after adversarial parse failure", async (ct) =>
            {
                await AssertRejectedOrDisconnected(async () =>
                {
                    return await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "?max-parts=nope", ct).ConfigureAwait(false);
                }, "bad query setup").ConfigureAwait(false);

                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "post-failure health check");
            }, token).ConfigureAwait(false);
        }

        private static S3RequestObservation RequireLastObservation(S3TestServer server)
        {
            AssertHelper.IsNotNull(server.LastObservedRequest, "last observed request");
            return server.LastObservedRequest;
        }

        private static async Task AssertRejectedOrDisconnected(Func<Task<HttpResponseMessage>> sendAsync, string name)
        {
            try
            {
                HttpResponseMessage response = await sendAsync().ConfigureAwait(false);
                AssertHelper.IsFalse(response.IsSuccessStatusCode, name + " should not succeed");
            }
            catch (HttpRequestException)
            {
                // Some low-level parse failures close the TCP connection before an S3 XML response can be written.
            }
        }
    }
}
