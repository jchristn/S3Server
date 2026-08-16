namespace Test.Shared.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using S3ServerLibrary;
    using S3ServerLibrary.S3Objects;

    /// <summary>
    /// S3 Select (SelectObjectContent) end-to-end routing tests.  These exercise the HTTP path
    /// (POST /bucket/key?select&amp;select-type=2) through request parsing, XML deserialization,
    /// and Object.SelectContent callback invocation.  The serialization suite covers the request
    /// model in isolation; these cover the wire-to-callback contract.
    /// </summary>
    public static class SelectContentTests
    {
        private const string ValidSelectBody =
            "<SelectObjectContentRequest xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">"
            + "<Expression>SELECT * FROM S3Object</Expression>"
            + "<ExpressionType>SQL</ExpressionType>"
            + "<InputSerialization><CSV><FileHeaderInfo>USE</FileHeaderInfo></CSV></InputSerialization>"
            + "<OutputSerialization><CSV></CSV></OutputSerialization>"
            + "</SelectObjectContentRequest>";

        /// <summary>
        /// Run all S3 Select tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            await runner.RunTestAsync("S3 Select POST routes to ObjectSelectContent and invokes callback", async (ct) =>
            {
                server.ClearObservedRequests();

                string url = server.BaseUrl + "/" + server.Bucket + "/data.csv?select&select-type=2";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(ValidSelectBody, Encoding.UTF8, "application/xml");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "select POST");

                S3RequestObservation observed = server.LastObservedRequest;
                AssertHelper.IsNotNull(observed, "last observed request");
                AssertHelper.AreEqual(S3RequestType.ObjectSelectContent, observed.RequestType, "request type");
                AssertHelper.AreEqual(server.Bucket, observed.Bucket, "bucket");
                AssertHelper.AreEqual("data.csv", observed.Key, "key");

                AssertHelper.IsNotNull(server.LastSelectRequest, "callback received select request");
                AssertHelper.AreEqual("SELECT * FROM S3Object", server.LastSelectRequest.Expression, "expression");
                AssertHelper.AreEqual(ExpressionTypeEnum.SQL, server.LastSelectRequest.ExpressionType, "expression type");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("S3 Select with malformed XML returns 400 MalformedXML", async (ct) =>
            {
                string url = server.BaseUrl + "/" + server.Bucket + "/data.csv?select&select-type=2";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent("this is not valid xml <><>!!!", Encoding.UTF8, "application/xml");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.BadRequest, response, "select malformed XML");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "MalformedXML", "error body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Unwired S3 Select returns 501 NotImplemented", async (ct) =>
            {
                int port = GetAvailablePort();
                S3ServerSettings settings = new S3ServerSettings();
                settings.Webserver.Hostname = "127.0.0.1";
                settings.Webserver.Port = port;
                settings.Webserver.Ssl.Enable = false;

                using (S3Server minimalServer = new S3Server(settings))
                {
                    minimalServer.Service.ServiceExists = async (ctx) => { return "us-west-1"; };
                    minimalServer.Bucket.Exists = async (ctx) => { return true; };
                    // Intentionally do NOT wire Object.SelectContent
                    minimalServer.Start();

                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(5);
                        client.DefaultRequestHeaders.ConnectionClose = true;
                        string url = $"http://127.0.0.1:{port}/test-bucket/data.csv?select&select-type=2";
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                        request.Content = new StringContent(ValidSelectBody, Encoding.UTF8, "application/xml");

                        HttpResponseMessage response = await client.SendAsync(request, ct).ConfigureAwait(false);
                        AssertHelper.StatusCodeEquals(HttpStatusCode.NotImplemented, response, "unwired ObjectSelectContent");
                        string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        AssertHelper.StringContains(body, "NotImplemented", "error body");
                    }

                    minimalServer.Stop();
                }
            }, token).ConfigureAwait(false);
        }

        private static int GetAvailablePort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
