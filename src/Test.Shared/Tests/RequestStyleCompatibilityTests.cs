namespace Test.Shared.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using S3ServerLibrary;

    /// <summary>
    /// Request style compatibility tests for path-style and virtual-hosted-style parsing.
    /// </summary>
    public static class RequestStyleCompatibilityTests
    {
        /// <summary>
        /// Run all request style compatibility tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">Unused shared server instance.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            await runner.RunTestAsync("Wildcard listener parses virtual-hosted-style object request", async (ct) =>
            {
                using (RequestStyleServer styleServer = new RequestStyleServer())
                {
                    string url = "http://test-bucket.s3.test:" + styleServer.Port + "/folder/object.txt";
                    HttpResponseMessage response = await styleServer.Client.GetAsync(url, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "virtual-hosted object request");

                    S3RequestObservation observed = styleServer.RequireObservation();
                    AssertHelper.AreEqual(S3RequestStyle.VirtualHostedStyle, observed.RequestStyle, "request style");
                    AssertHelper.AreEqual(S3RequestType.ObjectRead, observed.RequestType, "request type");
                    AssertHelper.AreEqual("test-bucket", observed.Bucket, "bucket");
                    AssertHelper.AreEqual("folder/object.txt", observed.Key, "key");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Wildcard listener parses virtual-hosted-style bucket request", async (ct) =>
            {
                using (RequestStyleServer styleServer = new RequestStyleServer())
                {
                    string url = "http://test-bucket.s3.test:" + styleServer.Port + "/";
                    HttpResponseMessage response = await styleServer.Client.GetAsync(url, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "virtual-hosted bucket request");

                    S3RequestObservation observed = styleServer.RequireObservation();
                    AssertHelper.AreEqual(S3RequestStyle.VirtualHostedStyle, observed.RequestStyle, "request style");
                    AssertHelper.AreEqual(S3RequestType.BucketRead, observed.RequestType, "request type");
                    AssertHelper.AreEqual("test-bucket", observed.Bucket, "bucket");
                    AssertHelper.IsTrue(String.IsNullOrEmpty(observed.Key), "key is empty");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Wildcard listener falls back to path-style when base domain does not match", async (ct) =>
            {
                using (RequestStyleServer styleServer = new RequestStyleServer())
                {
                    string url = "http://unknown.test:" + styleServer.Port + "/test-bucket/folder/object.txt";
                    HttpResponseMessage response = await styleServer.Client.GetAsync(url, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "path-style fallback request");

                    S3RequestObservation observed = styleServer.RequireObservation();
                    AssertHelper.AreEqual(S3RequestStyle.PathStyle, observed.RequestStyle, "request style");
                    AssertHelper.AreEqual(S3RequestType.ObjectRead, observed.RequestType, "request type");
                    AssertHelper.AreEqual("test-bucket", observed.Bucket, "bucket");
                    AssertHelper.AreEqual("folder/object.txt", observed.Key, "key");
                }
            }, token).ConfigureAwait(false);
        }

        private sealed class RequestStyleServer : IDisposable
        {
            private readonly S3Server _Server;

            public int Port { get; private set; }
            public HttpClient Client { get; private set; }
            public S3RequestObservation Observation { get; private set; }

            public RequestStyleServer()
            {
                Port = S3TestServer.GetAvailablePort();

                S3ServerSettings settings = new S3ServerSettings();
                settings.Webserver.Hostname = "*";
                settings.Webserver.Port = Port;
                settings.Webserver.Ssl.Enable = false;
                settings.Logging.HttpRequests = false;
                settings.Logging.S3Requests = false;
                settings.PreRequestHandler = async (ctx) =>
                {
                    Observation = S3RequestObservation.From(ctx);
                    ctx.Response.StatusCode = 200;
                    return true;
                };

                _Server = new S3Server(settings);
                _Server.Service.FindMatchingBaseDomain = FindMatchingBaseDomain;
                _Server.Start();

                SocketsHttpHandler handler = new SocketsHttpHandler();
                handler.ConnectCallback = async (context, cancellationToken) =>
                {
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        await socket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, Port), cancellationToken).ConfigureAwait(false);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                };

                Client = new HttpClient(handler);
                Client.Timeout = TimeSpan.FromSeconds(5);
                Client.DefaultRequestHeaders.ConnectionClose = true;
            }

            public S3RequestObservation RequireObservation()
            {
                AssertHelper.IsNotNull(Observation, "request observation");
                return Observation;
            }

            public void Dispose()
            {
                if (Client != null)
                    Client.Dispose();

                if (_Server != null)
                {
                    _Server.Stop();
                    _Server.Dispose();
                }
            }

            private static string FindMatchingBaseDomain(string hostname)
            {
                if (String.IsNullOrEmpty(hostname)) return null;

                if (hostname.Equals("s3.test", StringComparison.OrdinalIgnoreCase))
                    return "s3.test";

                if (hostname.EndsWith(".s3.test", StringComparison.OrdinalIgnoreCase))
                    return "s3.test";

                return null;
            }
        }
    }
}
