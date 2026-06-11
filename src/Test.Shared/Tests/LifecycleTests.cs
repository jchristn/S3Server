namespace Test.Shared.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using S3ServerLibrary;

    /// <summary>
    /// Server lifecycle and concurrency tests.
    /// </summary>
    public static class LifecycleTests
    {
        /// <summary>
        /// Run all lifecycle and concurrency tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            await runner.RunTestAsync("Concurrent service requests return success", async (ct) =>
            {
                List<Task<HttpResponseMessage>> tasks = new List<Task<HttpResponseMessage>>();

                for (int i = 0; i < 20; i++)
                {
                    tasks.Add(server.HttpClient.GetAsync(server.BaseUrl + "/", ct));
                }

                HttpResponseMessage[] responses = await Task.WhenAll(tasks).ConfigureAwait(false);

                for (int i = 0; i < responses.Length; i++)
                {
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, responses[i], "concurrent service request " + i);
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Concurrent object reads return success", async (ct) =>
            {
                List<Task<HttpResponseMessage>> tasks = new List<Task<HttpResponseMessage>>();

                for (int i = 0; i < 12; i++)
                {
                    tasks.Add(server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/test-object-" + i + ".txt", ct));
                }

                HttpResponseMessage[] responses = await Task.WhenAll(tasks).ConfigureAwait(false);

                for (int i = 0; i < responses.Length; i++)
                {
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, responses[i], "concurrent object read " + i);
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("S3TestServer dispose is idempotent", async (ct) =>
            {
                using (S3TestServer inner = new S3TestServer())
                {
                    inner.Dispose();
                    inner.Dispose();
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Minimal server starts and stops listener state", async (ct) =>
            {
                int port = S3TestServer.GetAvailablePort();
                S3ServerSettings settings = new S3ServerSettings();
                settings.Webserver.Hostname = "127.0.0.1";
                settings.Webserver.Port = port;
                settings.Webserver.Ssl.Enable = false;

                using (S3Server minimal = new S3Server(settings))
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.ConnectionClose = true;

                    minimal.Service.ServiceExists = async (ctx) => "us-west-1";
                    minimal.Start();
                    AssertHelper.IsTrue(minimal.IsListening, "minimal server is listening after start");

                    HttpRequestMessage head = new HttpRequestMessage(HttpMethod.Head, "http://127.0.0.1:" + port + "/");
                    HttpResponseMessage response = await client.SendAsync(head, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "minimal server HEAD before stop");

                    minimal.Stop();
                    AssertHelper.IsFalse(minimal.IsListening, "minimal server is not listening after stop");
                }
            }, token).ConfigureAwait(false);
        }
    }
}
