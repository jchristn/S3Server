namespace Test.Shared.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Deterministic fuzz-style tests for key, prefix, and numeric parser inputs.
    /// </summary>
    public static class FuzzTests
    {
        /// <summary>
        /// Run all fuzz-style tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            string[] keys = new string[]
            {
                "folder/space key.txt",
                "plus+literal.txt",
                "percent%literal.txt",
                "parens (1).txt",
                "semi;colon.txt",
                "hash#literal.txt",
                "question?literal.txt",
                "equals=literal&safe.txt"
            };

            foreach (string key in keys)
            {
                await runner.RunTestAsync("Fuzz object key round-trips parser value: " + key, async (ct) =>
                {
                    server.ClearObservedRequests();
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + "/" + server.Bucket + "/" + EscapePathKey(key));
                    request.Content = new StringContent("fuzz", Encoding.UTF8, "text/plain");

                    HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "PUT fuzz key");

                    S3RequestObservation observed = RequireLastObservation(server);
                    AssertHelper.AreEqual(server.Bucket, observed.Bucket, "bucket");
                    AssertHelper.AreEqual(key, observed.Key, "key");
                }, token).ConfigureAwait(false);
            }

            string[] prefixes = new string[]
            {
                "logs/2026/06/",
                "space prefix/",
                "plus+prefix/",
                "percent%prefix/",
                "query?prefix/"
            };

            foreach (string prefix in prefixes)
            {
                await runner.RunTestAsync("Fuzz list prefix round-trips parser value: " + prefix, async (ct) =>
                {
                    server.ClearObservedRequests();
                    string url = server.BaseUrl
                        + "/"
                        + server.Bucket
                        + "?prefix="
                        + Uri.EscapeDataString(prefix)
                        + "&delimiter=%2F";

                    HttpResponseMessage response = await server.HttpClient.GetAsync(url, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "GET fuzz prefix");

                    S3RequestObservation observed = RequireLastObservation(server);
                    AssertHelper.AreEqual(prefix, observed.Prefix, "prefix");
                    AssertHelper.AreEqual("/", observed.Delimiter, "delimiter");
                }, token).ConfigureAwait(false);
            }

            int[] maxKeysValues = new int[] { 0, 1, 2, 999, 1000 };

            foreach (int maxKeys in maxKeysValues)
            {
                await runner.RunTestAsync("Fuzz max-keys parses boundary value: " + maxKeys, async (ct) =>
                {
                    server.ClearObservedRequests();
                    string url = server.BaseUrl + "/" + server.Bucket + "?max-keys=" + maxKeys;
                    HttpResponseMessage response = await server.HttpClient.GetAsync(url, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "GET max-keys");

                    S3RequestObservation observed = RequireLastObservation(server);
                    AssertHelper.AreEqual(maxKeys, observed.MaxKeys, "max keys");
                }, token).ConfigureAwait(false);
            }
        }

        private static S3RequestObservation RequireLastObservation(S3TestServer server)
        {
            AssertHelper.IsNotNull(server.LastObservedRequest, "last observed request");
            return server.LastObservedRequest;
        }

        private static string EscapePathKey(string key)
        {
            return Uri.EscapeDataString(key).Replace("%2F", "/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
