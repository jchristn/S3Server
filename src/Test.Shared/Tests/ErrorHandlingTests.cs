namespace Test.Shared.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using S3ServerLibrary;

    /// <summary>
    /// Error handling tests (404s, S3Exception, malformed XML, default handler).
    /// </summary>
    public static class ErrorHandlingTests
    {
        /// <summary>
        /// Run all error handling tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            Console.WriteLine();
            Console.WriteLine("--- Error Handling Tests ---");

            await runner.RunTestAsync("Nonexistent bucket HEAD returns 404", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, server.BaseUrl + "/nonexistent-bucket-xyz");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.NotFound, response, "HEAD nonexistent bucket");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Nonexistent object GET returns 404 with NoSuchKey", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/nonexistent-object-xyz.bin");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.NotFound, response, "GET nonexistent object");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "NoSuchKey", "error body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Nonexistent object HEAD returns 404", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, server.BaseUrl + "/" + server.Bucket + "/nonexistent-object-xyz.bin");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.NotFound, response, "HEAD nonexistent object");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("S3Exception AccessDenied returns 403", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + "/access-denied-bucket");
                try
                {
                    HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "AccessDenied");
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    AssertHelper.StringContains(body, "AccessDenied", "error body");
                }
                catch (HttpRequestException)
                {
                    // TCP mode may reset connection
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Unhandled exception returns 500 InternalError", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + "/exception-bucket");
                try
                {
                    HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.InternalServerError, response, "InternalError");
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    AssertHelper.StringContains(body, "InternalError", "error body");
                }
                catch (HttpRequestException)
                {
                    // TCP mode may reset connection
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("PostRequestHandler exception does not crash server", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + "/post-handler-exception-bucket");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "post-handler bucket write");

                HttpRequestMessage followUp = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/");
                HttpResponseMessage followUpResponse = await server.HttpClient.SendAsync(followUp, ct).ConfigureAwait(false);
                AssertHelper.IsTrue(followUpResponse.IsSuccessStatusCode, "server still operational after PostRequestHandler exception");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("OPTIONS request handled by default handler", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Options, server.BaseUrl + "/" + server.Bucket + "/some-object");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
            }, token).ConfigureAwait(false);

            // Malformed XML tests
            string[] malformedXmlEndpoints = new string[]
            {
                "/" + server.Bucket + "?tagging",
                "/" + server.Bucket + "?acl",
                "/" + server.Bucket + "?versioning",
                "/" + server.Bucket + "?website",
                "/" + server.Bucket + "?logging",
                "/" + server.Bucket + "/test-object.txt?acl",
                "/" + server.Bucket + "/test-object.txt?legal-hold",
                "/" + server.Bucket + "/test-object.txt?retention",
                "/" + server.Bucket + "/test-object.txt?tagging"
            };

            foreach (string endpoint in malformedXmlEndpoints)
            {
                string testName = "MalformedXML returns 400 for PUT " + endpoint;
                await runner.RunTestAsync(testName, async (ct) =>
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + endpoint);
                    request.Content = new StringContent("this is not valid xml <><>!!!", Encoding.UTF8, "application/xml");

                    HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.BadRequest, response, "MalformedXML");
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    AssertHelper.StringContains(body, "MalformedXML", "error body");
                }, token).ConfigureAwait(false);
            }

            await runner.RunTestAsync("Unwired recognized operation returns 501 NotImplemented", async (ct) =>
            {
                S3ServerSettings settings = new S3ServerSettings();
                settings.Webserver.Hostname = "127.0.0.1";
                settings.Webserver.Port = server.Port + 100;
                settings.Webserver.Ssl.Enable = false;

                using (S3Server minimalServer = new S3Server(settings))
                {
                    minimalServer.Service.ServiceExists = async (ctx) => { return "us-west-1"; };
                    minimalServer.Bucket.Exists = async (ctx) => { return true; };
                    // Intentionally do NOT wire Bucket.ReadAcl

                    minimalServer.Start();

                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(5);
                        client.DefaultRequestHeaders.ConnectionClose = true;
                        string url = $"http://127.0.0.1:{server.Port + 100}/test-bucket?acl";
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                        HttpResponseMessage response = await client.SendAsync(request, ct).ConfigureAwait(false);
                        AssertHelper.StatusCodeEquals(HttpStatusCode.NotImplemented, response, "unwired BucketReadAcl");
                        string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        AssertHelper.StringContains(body, "NotImplemented", "error body");
                    }

                    minimalServer.Stop();
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("EnableSignatures without GetSecretKey throws at startup", async (ct) =>
            {
                S3ServerSettings settings = new S3ServerSettings();
                settings.Webserver.Hostname = "127.0.0.1";
                settings.Webserver.Port = server.Port + 101;
                settings.Webserver.Ssl.Enable = false;
                settings.EnableSignatures = true;

                using (S3Server sigServer = new S3Server(settings))
                {
                    // Do NOT set Service.GetSecretKey
                    await AssertHelper.ThrowsAsync<InvalidOperationException>(async () =>
                    {
                        sigServer.Start();
                    }, "Start() should throw when EnableSignatures is true but GetSecretKey is null").ConfigureAwait(false);
                }
            }, token).ConfigureAwait(false);
        }
    }
}
