using System;
using System.Threading.Tasks; 
using WatsonWebserver;
using S3ServerInterface.Callbacks;

namespace S3ServerInterface
{
    /// <summary>
    /// S3 server.  
    /// Bucket names must not be in the hostname; they must be in the URL path. 
    /// </summary>
    public class S3Server : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Determine if the server is listening.
        /// </summary>
        public bool IsListening
        {
            get
            {
                return _Server.IsListening;
            }
        }

        /// <summary>
        /// Method to invoke when sending a log message.
        /// </summary>
        public Action<string> Logger = null;

        /// <summary>
        /// Enable or disable logging for various items.
        /// </summary>
        public LoggingSettings Logging = new LoggingSettings();

        /// <summary>
        /// Callback methods for requests received for service operations.
        /// </summary>
        public ServiceCallbacks Service = new ServiceCallbacks();

        /// <summary>
        /// Callback methods for requests received for bucket operations.
        /// </summary>
        public BucketCallbacks Bucket = new BucketCallbacks();

        /// <summary>
        /// Callback methods for requests received for object operations.
        /// </summary>
        public ObjectCallbacks Object = new ObjectCallbacks();

        /// <summary>
        /// Callback method to use prior to examining requests for AWS S3 APIs.
        /// Return true if you wish to terminate the request, otherwise, return false, which will further route the request.
        /// </summary>
        public Func<S3Request, S3Response, Task<bool>> PreRequestHandler = null;

        /// <summary>
        /// Callback method to call when no matching AWS S3 API callback could be found. 
        /// </summary>
        public Func<S3Request, S3Response, Task> DefaultRequestHandler = null;

        /// <summary>
        /// Callback method to call after a response has been sent.
        /// </summary>
        public Func<S3Request, S3Response, Task> PostRequestHandler = null;

        /// <summary>
        /// Callback method to retrieve the secret key for the supplied access key from your application.  
        /// This method is only used when AuthenticateSignatures is set to true.
        /// </summary>
        public Func<string, byte[]> GetSecretKey = null;

        /// <summary>
        /// Enable or disable authentication of signatures.  This does not validate signatures for transferred chunks, only authentication of the user.
        /// Signatures will only be authenticated if the GetSecretKey callback is set.
        /// Refer to the following links for how AWS signatures are derived.
        /// Version 2: https://docs.aws.amazon.com/AmazonS3/latest/dev/RESTAuthentication.html
        /// Version 4: https://docs.aws.amazon.com/AmazonS3/latest/API/sig-v4-header-based-auth.html
        /// </summary>
        public bool AuthenticateSignatures = false;

        /// <summary>
        /// Specify whether or not exceptions should be included in status 500 Internal Server Error messages.
        /// </summary>
        public bool SendExceptionsInResponses = true;

        /// <summary>
        /// The base domain against which incoming requests should be compared to identify the bucket name.
        /// For instance, if you wish to identify the bucket 'bucket' from a request to host 'bucket.mys3server.com', set this value to '.mys3server.com'.
        /// If you leave this value to null, S3Server will always assume that the bucket name is in the URL.
        /// </summary>
        public string BaseDomain = null;

        #endregion

        #region Private-Members

        private string _Header = "[S3Server] ";
        private bool _Disposed = false;

        private string _ListenerHostname = null;
        private int _ListenerPort = 0;
        private bool _Ssl = false;
        private Server _Server = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// Using this constructor results in no pre-request handler (your own API handler), and no custom default request handler (when an S3 API cannot be matched).
        /// </summary>
        /// <param name="hostname">The hostname on which to listen for HTTP requests.  If you listen on a wildcard such as '*', '+', or '0.0.0.0', you must run with elevated/administrative privileges.</param>
        /// <param name="port">The TCP port number.</param>
        /// <param name="ssl">Enable or disable SSL.</param> 
        public S3Server(
            string hostname,
            int port,
            bool ssl)
        {
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0 || port > 65535) throw new ArgumentException("Port must be between 0 and 65535.");

            _ListenerHostname = hostname;
            _ListenerPort = port;
            _Ssl = ssl;

            PreRequestHandler = null;
            DefaultRequestHandler = null;

            _Server = new Server(_ListenerHostname, _ListenerPort, _Ssl, RequestHandler); 
        }

        /// <summary>
        /// Instantiate the object.
        /// Using this constructor results in no pre-request handler (your own API handler), but (if not null) allows a custom default request handler (when an S3 API cannot be matched).
        /// </summary>
        /// <param name="hostname">The hostname on which to listen for HTTP requests.  If you listen on a wildcard such as '*', '+', or '0.0.0.0', you must run with elevated/administrative privileges.</param>
        /// <param name="port">The TCP port number.</param>
        /// <param name="ssl">Enable or disable SSL.</param> 
        /// <param name="defaultRequestHandler">Default request handler used when no other callbacks can be found.</param>
        public S3Server(
            string hostname,
            int port,
            bool ssl,
            Func<S3Request, S3Response, Task> defaultRequestHandler)
        {
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0 || port > 65535) throw new ArgumentException("Port must be between 0 and 65535.");

            _ListenerHostname = hostname;
            _ListenerPort = port;
            _Ssl = ssl;
            PreRequestHandler = null;
            DefaultRequestHandler = defaultRequestHandler;

            _Server = new Server(_ListenerHostname, _ListenerPort, _Ssl, RequestHandler); 
        }

        /// <summary>
        /// Instantiate the object.
        /// Using this constructor results in (if not null) a pre-request handler (your own API handler), and (if not null) a custom default request handler (when an S3 API cannot be matched).
        /// </summary>
        /// <param name="hostname">The hostname on which to listen for HTTP requests.  If you listen on a wildcard such as '*', '+', or '0.0.0.0', you must run with elevated/administrative privileges.</param>
        /// <param name="port">The TCP port number.</param>
        /// <param name="ssl">Enable or disable SSL.</param>
        /// <param name="preRequestHandler">Request handler to call prior to evaluating for S3 requests, can be null.</param>
        /// <param name="defaultRequestHandler">Default request handler used when no other callbacks can be found.</param>
        public S3Server(
            string hostname,
            int port,
            bool ssl,
            Func<S3Request, S3Response, Task<bool>> preRequestHandler,
            Func<S3Request, S3Response, Task> defaultRequestHandler)
        {
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0 || port > 65535) throw new ArgumentException("Port must be between 0 and 65535."); 

            _ListenerHostname = hostname;
            _ListenerPort = port;
            _Ssl = ssl;

            PreRequestHandler = preRequestHandler;
            DefaultRequestHandler = defaultRequestHandler;

            _Server = new Server(_ListenerHostname, _ListenerPort, _Ssl, RequestHandler); 
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
         
        /// <summary>
        /// Start accepting new connections.
        /// </summary>
        public void Start()
        {
            _Server.Start();
        }

        /// <summary>
        /// Stop accepting new connections.
        /// </summary>
        public void Stop()
        {
            _Server.Stop();
        }

        #endregion

        #region Private-Methods

        /// <summary>
        /// Dispose of the server.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed)
            {
                return;
            }

            if (disposing)
            {
                Logger?.Invoke(_Header + "Dispose requested");

                if (_Server != null) _Server.Dispose();
            }

            _Disposed = true;
        }

        private async Task RequestHandler(HttpContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            DateTime startTime = DateTime.Now;

            S3Request s3req = new S3Request(BaseDomain, ctx, (Logging.S3Requests ? Logger : null));
            S3Response s3resp = new S3Response(s3req);
            bool success = false;

            try
            { 
                if (Logging.HttpRequests)
                {
                    Logger?.Invoke(_Header + "HTTP request: " + Environment.NewLine + Common.SerializeJson(s3req, true));
                }

                if (PreRequestHandler != null)
                {
                    success = await PreRequestHandler(s3req, s3resp);
                    if (success)
                    {
                        await s3resp.Send();
                        return;
                    } 
                }

                if (AuthenticateSignatures && GetSecretKey != null)
                {
                    if (!String.IsNullOrEmpty(s3req.AccessKey))
                    {
                        byte[] secretKey = GetSecretKey(s3req.AccessKey);
                        if (secretKey == null || secretKey.Length < 1)
                        {
                            await s3resp.Send(S3Objects.ErrorCode.InvalidRequest);
                            return;
                        }

                        if (!s3req.IsValidSignature(secretKey))
                        {
                            await s3resp.Send(S3Objects.ErrorCode.SignatureDoesNotMatch);
                            return;
                        }
                    }
                }

                switch (s3req.RequestType)
                {
                    case S3RequestType.ListBuckets:
                        if (Service.ListBuckets != null)
                        {
                            await Service.ListBuckets(s3req, s3resp);
                            return;
                        }
                        break;
                         
                    case S3RequestType.BucketDelete:
                        if (Bucket.Delete != null)
                        {
                            await Bucket.Delete(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketDeleteTags:
                        if (Bucket.DeleteTags != null)
                        {
                            await Bucket.DeleteTags(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketDeleteWebsite:
                        if (Bucket.DeleteWebsite != null)
                        {
                            await Bucket.DeleteWebsite(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketExists:
                        if (Bucket.Exists != null)
                        {
                            await Bucket.Exists(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketRead:
                        if (Bucket.Read != null)
                        {
                            await Bucket.Read(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketReadAcl:
                        if (Bucket.ReadAcl != null)
                        {
                            await Bucket.ReadAcl(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketReadLocation:
                        if (Bucket.ReadLocation != null)
                        {
                            await Bucket.ReadLocation(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketReadLogging:
                        if (Bucket.ReadLogging != null)
                        {
                            await Bucket.ReadLogging(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketReadTags:
                        if (Bucket.ReadTags != null)
                        {
                            await Bucket.ReadTags(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketReadVersioning:
                        if (Bucket.ReadVersioning != null)
                        {
                            await Bucket.ReadVersioning(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketReadVersions:
                        if (Bucket.ReadVersions != null)
                        {
                            await Bucket.ReadVersions(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketReadWebsite:
                        if (Bucket.ReadWebsite != null)
                        {
                            await Bucket.ReadWebsite(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketWrite:
                        if (Bucket.Write != null)
                        {
                            await Bucket.Write(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketWriteAcl:
                        if (Bucket.WriteAcl != null)
                        {
                            await Bucket.WriteAcl(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketWriteLogging:
                        if (Bucket.WriteLogging != null)
                        {
                            await Bucket.WriteLogging(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketWriteTags:
                        if (Bucket.WriteTags != null)
                        {
                            await Bucket.WriteTags(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketWriteVersioning:
                        if (Bucket.WriteVersioning != null)
                        {
                            await Bucket.WriteVersioning(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.BucketWriteWebsite:
                        if (Bucket.WriteWebsite != null)
                        {
                            await Bucket.WriteWebsite(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.ObjectDelete:
                        if (Object.Delete!= null)
                        {
                            await Object.Delete(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.ObjectDeleteMultiple:
                        if (Object.DeleteMultiple != null)
                        {
                            await Object.DeleteMultiple(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.ObjectDeleteTags:
                        if (Object.DeleteTags != null)
                        {
                            await Object.DeleteTags(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.ObjectExists:
                        if (Object.Exists != null)
                        {
                            await Object.Exists(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.ObjectRead:
                        if (Object.Read != null)
                        {
                            await Object.Read(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.ObjectReadAcl:
                        if (Object.ReadAcl != null)
                        {
                            await Object.ReadAcl(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.ObjectReadLegalHold:
                        if (Object.ReadLegalHold != null)
                        {
                            await Object.ReadLegalHold(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.ObjectReadRange:
                        if (Object.ReadRange != null)
                        {
                            await Object.ReadRange(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.ObjectReadRetention:
                        if (Object.ReadRetention != null)
                        {
                            await Object.ReadRetention(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.ObjectReadTags:
                        if (Object.ReadTags != null)
                        {
                            await Object.ReadTags(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.ObjectWrite:
                        if (Object.Write != null)
                        {
                            await Object.Write(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.ObjectWriteAcl:
                        if (Object.WriteAcl != null)
                        {
                            await Object.WriteAcl(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.ObjectWriteLegalHold:
                        if (Object.WriteLegalHold != null)
                        {
                            await Object.WriteLegalHold(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.ObjectWriteRetention:
                        if (Object.WriteRetention != null)
                        {
                            await Object.WriteRetention(s3req, s3resp);
                            return;
                        }
                        break;
                    case S3RequestType.ObjectWriteTags:
                        if (Object.WriteTags != null)
                        {
                            await Object.WriteTags(s3req, s3resp);
                            return;
                        }
                        break;
                }

                if (DefaultRequestHandler != null)
                {
                    await DefaultRequestHandler(s3req, s3resp);
                    return;
                }

                await s3resp.Send(S3Objects.ErrorCode.InvalidRequest);
                return;
            }
            catch (Exception e)
            {
                if (Logging.Exceptions)
                {
                    Logger?.Invoke(_Header + "Exception:" + Environment.NewLine + Common.SerializeJson(e, true));
                }

                await s3resp.Send(S3Objects.ErrorCode.InternalError);
                return;
            }
            finally
            { 
                if (Logging.HttpRequests)
                {
                    Logger?.Invoke(
                        _Header + 
                        "[" + 
                        ctx.Request.SourceIp + ":" +
                        ctx.Request.SourcePort + 
                        "] " +
                        ctx.Request.Method.ToString() + " " +
                        ctx.Request.RawUrlWithoutQuery + " " +
                        s3resp.StatusCode + 
                        " [" + Common.TotalMsFrom(startTime) + "ms]");
                }

                if (PostRequestHandler != null) await PostRequestHandler(s3req, s3resp);
            }
        }
         
        #endregion
    }
}
