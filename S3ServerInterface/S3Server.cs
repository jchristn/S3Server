using System;
using System.Threading;
using System.Threading.Tasks;
using WatsonWebserver;

namespace S3ServerInterface
{
    /// <summary>
    /// S3 server.
    /// </summary>
    public class S3Server : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Function to call when an S3 request is received.  Provides you with a populated S3Request, expects an S3Response in return.
        /// </summary>
        public Func<S3Request, S3Response> RequestReceived = null;

        /// <summary>
        /// Enable or disable console debugging of HTTP requests.
        /// </summary>
        public bool DebugHttpRequests = false;

        /// <summary>
        /// Enable or disable console debugging of S3 request construction.
        /// </summary>
        public bool DebugS3RequestConstruction = false;

        #endregion

        #region Private-Members

        private bool _Disposed = false;

        private string _Hostname;
        private int _Port;
        private bool _Ssl;
        private Server _Server;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="hostname">The hostname on which to listen.</param>
        /// <param name="port">The TCP port number.</param>
        /// <param name="ssl">Enable or disable SSL.</param>
        public S3Server(string hostname, int port, bool ssl)
        {
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0 || port > 65535) throw new ArgumentException("Port must be between 0 and 65535.");

            _Hostname = hostname;
            _Port = port;
            _Ssl = ssl;

            _Server = new Server(_Hostname, _Port, _Ssl, RequestHandler);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Tear down the client and dispose of background workers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private-Methods

        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_Server != null) _Server.Dispose();
            }

            _Disposed = true;
        }

        private HttpResponse RequestHandler(HttpRequest req)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));
            DateTime startTime = DateTime.Now.ToUniversalTime();
            HttpResponse resp = new HttpResponse(req, true, 200, null, "text/plain", null, true);
            
            try
            {
                S3Request s3req = new S3Request(req, DebugS3RequestConstruction);

                if (DebugHttpRequests)
                {
                    Console.WriteLine(Common.SerializeJson(req, true));
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine(Common.SerializeJson(s3req, true));
                }

                S3Response s3resp = RequestReceived(s3req);
                resp = new HttpResponse(req, s3resp.Success, s3resp.StatusCode, s3resp.Headers, s3resp.ContentType, s3resp.Data, true);
                return resp;
            }
            catch (Exception e)
            {
                if (DebugHttpRequests)
                {
                    Console.WriteLine(Common.SerializeJson(e, true));
                }

                resp = new HttpResponse(req, false, 500, null, "text/plain", null, true);
                return resp;
            }
            finally
            {
                if (DebugHttpRequests)
                {
                    Console.WriteLine(
                        req.SourceIp + ":" + 
                        req.SourcePort + " " + 
                        req.Method.ToString() + " " + 
                        req.RawUrlWithoutQuery + " " + 
                        resp.StatusCode + 
                        " [" + Common.TotalMsFrom(startTime) + "]");
                }
            }
        }

        #endregion
    }
}
