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
            await runner.RunTestAsync("Invalid max-keys keeps default parser value", async (ct) =>
            {
                server.ClearObservedRequests();
                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "?max-keys=not-a-number", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "invalid max-keys request");

                S3RequestObservation observed = RequireLastObservation(server);
                AssertHelper.AreEqual(1000, observed.MaxKeys, "default max keys");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Negative max-keys fails closed", async (ct) =>
            {
                await AssertRejectedOrDisconnected(async () =>
                {
                    return await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "?max-keys=-1", ct).ConfigureAwait(false);
                }, "negative max-keys").ConfigureAwait(false);
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Malformed range header fails closed", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                request.Headers.TryAddWithoutValidation("Range", "bytes=abc-def");

                await AssertRejectedOrDisconnected(async () =>
                {
                    return await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                }, "malformed range").ConfigureAwait(false);
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Inverted range header fails closed", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                request.Headers.TryAddWithoutValidation("Range", "bytes=4-1");

                await AssertRejectedOrDisconnected(async () =>
                {
                    return await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                }, "inverted range").ConfigureAwait(false);
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
                HttpRequestMessage bad = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                bad.Headers.TryAddWithoutValidation("Range", "bytes=nope");

                await AssertRejectedOrDisconnected(async () =>
                {
                    return await server.HttpClient.SendAsync(bad, ct).ConfigureAwait(false);
                }, "bad range setup").ConfigureAwait(false);

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
