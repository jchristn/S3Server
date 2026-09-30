namespace Test.Shared.Compatibility
{
    using System;
    using System.Net.Sockets;
    using S3ServerLibrary;

    /// <summary>
    /// S3Server with the reference backend and signature validation enabled, listening on 127.0.0.1.
    /// </summary>
    public class ReferenceS3Server : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Port.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Endpoint, for example http://127.0.0.1:8000.
        /// </summary>
        public string Endpoint => "http://127.0.0.1:" + Port;

        /// <summary>
        /// Bucket created at startup.
        /// </summary>
        public string Bucket { get; private set; }

        /// <summary>
        /// Access key accepted by the server.
        /// </summary>
        public string AccessKey { get; private set; } = "AKIAREFERENCEEXAMPLE";

        /// <summary>
        /// Secret key for AccessKey.
        /// </summary>
        public string SecretKey { get; private set; } = "referenceSecretKeyForCompatibilityTests1";

        /// <summary>
        /// Underlying server.
        /// </summary>
        public S3Server Server { get; private set; }

        /// <summary>
        /// Storage backend.
        /// </summary>
        public ReferenceS3Backend Backend { get; private set; } = new ReferenceS3Backend();

        #endregion

        #region Private-Members

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate and start.
        /// </summary>
        /// <param name="port">Port; 0 selects an available port.</param>
        /// <param name="bucket">Bucket to create.</param>
        /// <param name="enableSignatures">True to validate AWS signatures.</param>
        /// <param name="logger">Optional logger.</param>
        public ReferenceS3Server(int port = 0, string bucket = "compat-bucket", bool enableSignatures = true, Action<string> logger = null)
        {
            Bucket = bucket;
            Backend.CreateBucket(bucket);

            // An automatically selected port can be claimed by another test process between selection and bind,
            // so retry with a new port when the bind fails.
            for (int attempt = 1; ; attempt++)
            {
                Port = port == 0 ? S3TestServer.GetAvailablePort() : port;

                S3ServerSettings settings = new S3ServerSettings();
                settings.Webserver.Hostname = "127.0.0.1";
                settings.Webserver.Port = Port;
                settings.Webserver.Ssl.Enable = false;
                settings.EnableSignatures = enableSignatures;
                settings.Logger = logger;

                Server = new S3Server(settings);
                Server.Service.GetSecretKey = (ctx) => ctx.Request.AccessKey == AccessKey ? SecretKey : null;
                Backend.Attach(Server);

                try
                {
                    Server.Start();
                    return;
                }
                catch (SocketException) when (port == 0 && attempt < 5)
                {
                    Server.Dispose();
                    Server = null;
                }
            }
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            if (_Disposed) return;
            _Disposed = true;

            if (Server != null)
            {
                Server.Stop();
                Server.Dispose();
            }
        }

        #endregion
    }
}
