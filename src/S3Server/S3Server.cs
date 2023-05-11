using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks; 
using WatsonWebserver;
using S3ServerLibrary.Callbacks;
using S3ServerLibrary.S3Objects;

namespace S3ServerLibrary
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
        public Func<S3Context, Task<bool>> PreRequestHandler = null;

        /// <summary>
        /// Callback method to call when no matching AWS S3 API callback could be found. 
        /// </summary>
        public Func<S3Context, Task> DefaultRequestHandler = null;

        /// <summary>
        /// Callback method to call after a response has been sent.
        /// </summary>
        public Func<S3Context, Task> PostRequestHandler = null;

        /// <summary>
        /// The list of base domains against which incoming requests should be compared to identify the bucket name.
        /// For instance, if you wish to identify the bucket 'bucket' from a request to host 'bucket.mys3server.com', set this value to '.mys3server.com'.
        /// If the list is empty, or, a base domain cannot be identified, S3Server will assume that the bucket name is in the URL.
        /// </summary>
        public List<string> BaseDomains
        {
            get
            {
                return _BaseDomains;
            }
            set
            {
                if (value == null) _BaseDomains = new List<string>();
                else _BaseDomains = value;
            }
        }

        #endregion

        #region Private-Members

        private string _Header = "[S3Server] ";
        private bool _Disposed = false;

        private string _ListenerHostname = null;
        private int _ListenerPort = 0;
        private bool _Ssl = false;
        private Server _Server = null;
        private List<string> _BaseDomains = new List<string>();
         
        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
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
        /// Instantiate.
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
            Func<S3Context, Task> defaultRequestHandler)
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
        /// Instantiate.
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
            Func<S3Context, Task<bool>> preRequestHandler,
            Func<S3Context, Task> defaultRequestHandler)
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
        /// Do not use the object after disposal.
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

            _Server = null;

            _Disposed = true;
        }

        private async Task RequestHandler(HttpContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            S3Context s3ctx = null;

            try
            {
                s3ctx = new S3Context(ctx, _BaseDomains, null, (Logging.S3Requests ? Logger : null));
                s3ctx.Response.Headers.Add("x-amz-request-id", s3ctx.Request.RequestId);
                s3ctx.Response.Headers.Add("x-amz-id-2", s3ctx.Request.RequestId);
            }
            catch (Exception e)
            {
                if (Logging.Exceptions)
                {
                    Logger?.Invoke(_Header + "Exception:" + Environment.NewLine + SerializationHelper.SerializeJson(e, true));
                }

                return;
            }

            byte[] bytes = null;
            bool success = false;
            bool exists = false;
            S3Object s3obj = null;
            ObjectMetadata md = null;
            AccessControlPolicy acp = null;
            LegalHold legalHold = null;
            Retention retention = null;
            Tagging tagging = null;
            ListAllMyBucketsResult buckets = null;
            ListBucketResult listBucketResult = null;
            ListVersionsResult listVersionResult = null;
            LocationConstraint location = null;
            BucketLoggingStatus bucketLogging = null;
            VersioningConfiguration versionConfig = null;
            WebsiteConfiguration wc = null;
            DeleteMultiple delMultiple = null;
            DeleteResult delResult = null;
            Error error = null;

            try
            {
                if (Logging.HttpRequests)
                    Logger?.Invoke(_Header + "HTTP request: " + Environment.NewLine + s3ctx.Http.ToJson(true));

                if (Logging.S3Requests)
                    Logger?.Invoke(_Header + "S3 request: " + Environment.NewLine + s3ctx.Request.ToJson(true));

                if (PreRequestHandler != null)
                {
                    success = await PreRequestHandler(s3ctx).ConfigureAwait(false);
                    if (success)
                    {
                        await s3ctx.Response.Send().ConfigureAwait(false);
                        return;
                    } 
                }

                switch (s3ctx.Request.RequestType)
                {
                    #region Service

                    case S3RequestType.ListBuckets:
                        if (Service.ListBuckets != null)
                        {
                            buckets = await Service.ListBuckets(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/xml";
                            await ctx.Response.Send(SerializationHelper.SerializeXml(buckets)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    #endregion

                    #region Bucket

                    case S3RequestType.BucketDelete:
                        if (Bucket.Delete != null)
                        {
                            await Bucket.Delete(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 204;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketDeleteAcl:
                        if (Bucket.DeleteAcl != null)
                        {
                            await Bucket.DeleteAcl(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 204;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketDeleteTags:
                        if (Bucket.DeleteTagging != null)
                        {
                            await Bucket.DeleteTagging(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 204;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketDeleteWebsite:
                        if (Bucket.DeleteWebsite != null)
                        {
                            await Bucket.DeleteWebsite(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 204;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketExists:
                        if (Bucket.Exists != null)
                        {
                            exists = await Bucket.Exists(s3ctx).ConfigureAwait(false);
                            if (exists)
                            {
                                ctx.Response.StatusCode = 200;
                                ctx.Response.ContentType = "text/plain";
                                await ctx.Response.Send().ConfigureAwait(false);
                            }
                            else
                            { 
                                error = new Error(ErrorCode.NoSuchBucket);
                                ctx.Response.StatusCode = 404;
                                ctx.Response.ContentType = "application/xml";
                                await ctx.Response.Send(SerializationHelper.SerializeXml(error)).ConfigureAwait(false);
                            }
                            return;
                        }
                        break;

                    case S3RequestType.BucketRead:
                        if (Bucket.Read != null)
                        {
                            listBucketResult = await Bucket.Read(s3ctx).ConfigureAwait(false);
                            bytes = Encoding.UTF8.GetBytes(SerializationHelper.SerializeXml(listBucketResult));

                            if (!String.IsNullOrEmpty(listBucketResult.BucketRegion))
                                ctx.Response.Headers.Add("x-amz-bucket-region", listBucketResult.BucketRegion);

                            ctx.Response.ChunkedTransfer = true;
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/xml";
                            await ctx.Response.SendFinalChunk(bytes, bytes.Length).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketReadAcl:
                        if (Bucket.ReadAcl != null)
                        {
                            acp = await Bucket.ReadAcl(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/xml";
                            await ctx.Response.Send(SerializationHelper.SerializeXml(acp)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketReadLocation:
                        if (Bucket.ReadLocation != null)
                        {
                            location = await Bucket.ReadLocation(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/xml";
                            await ctx.Response.Send(SerializationHelper.SerializeXml(location)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketReadLogging:
                        if (Bucket.ReadLogging != null)
                        {
                            bucketLogging = await Bucket.ReadLogging(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/xml";
                            await ctx.Response.Send(SerializationHelper.SerializeXml(bucketLogging)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketReadTags:
                        if (Bucket.ReadTagging != null)
                        {
                            tagging = await Bucket.ReadTagging(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/xml";
                            await ctx.Response.Send(SerializationHelper.SerializeXml(tagging)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketReadVersioning:
                        if (Bucket.ReadVersioning != null)
                        {
                            versionConfig = await Bucket.ReadVersioning(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/xml";
                            await ctx.Response.Send(SerializationHelper.SerializeXml(versionConfig)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketReadVersions:
                        if (Bucket.ReadVersions != null)
                        {
                            listVersionResult = await Bucket.ReadVersions(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/xml";
                            await ctx.Response.Send(SerializationHelper.SerializeXml(listVersionResult)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketReadWebsite:
                        if (Bucket.ReadWebsite != null)
                        {
                            wc = await Bucket.ReadWebsite(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/xml";
                            await ctx.Response.Send(SerializationHelper.SerializeXml(wc)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketWrite:
                        if (Bucket.Write != null)
                        {
                            await Bucket.Write(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketWriteAcl:
                        if (Bucket.WriteAcl != null)
                        {
                            try
                            {
                                acp = SerializationHelper.DeserializeXml<AccessControlPolicy>(s3ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3ctx);
                                ioe.Data.Add("RequestBody", s3ctx.Request.DataAsString);
                                Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + SerializationHelper.SerializeJson(ioe, true));
                                await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                return;
                            }

                            await Bucket.WriteAcl(s3ctx, acp).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketWriteLogging:
                        if (Bucket.WriteLogging != null)
                        {
                            try
                            {
                                bucketLogging = SerializationHelper.DeserializeXml<BucketLoggingStatus>(s3ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3ctx);
                                ioe.Data.Add("RequestBody", s3ctx.Request.DataAsString);
                                Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + SerializationHelper.SerializeJson(ioe, true));
                                await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                return;
                            }

                            await Bucket.WriteLogging(s3ctx, bucketLogging).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketWriteTags:
                        if (Bucket.WriteTagging != null)
                        {
                            try
                            {
                                tagging = SerializationHelper.DeserializeXml<Tagging>(s3ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3ctx);
                                ioe.Data.Add("RequestBody", s3ctx.Request.DataAsString);
                                Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + SerializationHelper.SerializeJson(ioe, true));
                                await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                return;
                            }

                            await Bucket.WriteTagging(s3ctx, tagging).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketWriteVersioning:
                        if (Bucket.WriteVersioning != null)
                        {
                            try
                            {
                                versionConfig = SerializationHelper.DeserializeXml<VersioningConfiguration>(s3ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3ctx);
                                ioe.Data.Add("RequestBody", s3ctx.Request.DataAsString);
                                Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + SerializationHelper.SerializeJson(ioe, true));
                                await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                return;
                            }

                            await Bucket.WriteVersioning(s3ctx, versionConfig).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketWriteWebsite:
                        if (Bucket.WriteWebsite != null)
                        {
                            try
                            {
                                wc = SerializationHelper.DeserializeXml<WebsiteConfiguration>(s3ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3ctx);
                                ioe.Data.Add("RequestBody", s3ctx.Request.DataAsString);
                                Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + SerializationHelper.SerializeJson(ioe, true));
                                await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                return;
                            }

                            await Bucket.WriteWebsite(s3ctx, wc).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    #endregion

                    #region Object

                    case S3RequestType.ObjectDelete:
                        if (Object.Delete != null)
                        {
                            await Object.Delete(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 204;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectDeleteAcl:
                        if (Object.DeleteAcl != null)
                        {
                            await Object.DeleteAcl(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 204;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectDeleteMultiple:
                        if (Object.DeleteMultiple != null)
                        {
                            try
                            {
                                delMultiple = SerializationHelper.DeserializeXml<DeleteMultiple>(s3ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3ctx);
                                ioe.Data.Add("RequestBody", s3ctx.Request.DataAsString);
                                Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + SerializationHelper.SerializeJson(ioe, true));
                                await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                return;
                            }

                            delResult = await Object.DeleteMultiple(s3ctx, delMultiple).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/xml";
                            await ctx.Response.Send(SerializationHelper.SerializeXml(delResult)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectDeleteTags:
                        if (Object.DeleteTagging != null)
                        {
                            await Object.DeleteTagging(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 204;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectExists:
                        if (Object.Exists != null)
                        {
                            md = await Object.Exists(s3ctx).ConfigureAwait(false);
                            if (md != null)
                            {
                                if (!String.IsNullOrEmpty(md.ETag)) ctx.Response.Headers.Add("ETag", md.ETag);
                                ctx.Response.Headers.Add("Last-Modified", md.LastModified.ToString());
                                ctx.Response.Headers.Add("x-amz-storage-class", md.StorageClass);
                                ctx.Response.StatusCode = 200;
                                ctx.Response.ContentLength = md.Size;
                                ctx.Response.ContentType = md.ContentType;
                                await ctx.Response.Send(md.Size).ConfigureAwait(false);
                            }
                            else
                            {
                                error = new Error(ErrorCode.NoSuchKey);
                                ctx.Response.StatusCode = 404;
                                ctx.Response.ContentType = "application/xml";
                                await ctx.Response.Send(SerializationHelper.SerializeXml(error)).ConfigureAwait(false);
                            }
                            return;
                        }
                        break;

                    case S3RequestType.ObjectRead:
                        if (Object.Read != null)
                        {
                            s3obj = await Object.Read(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = s3obj.ContentType;
                            ctx.Response.ContentLength = s3obj.Size;
                            await ctx.Response.Send(s3obj.Size, s3obj.Data).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectReadAcl:
                        if (Object.ReadAcl != null)
                        {
                            acp = await Object.ReadAcl(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/xml";
                            await ctx.Response.Send(SerializationHelper.SerializeXml(acp)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectReadLegalHold:
                        if (Object.ReadLegalHold != null)
                        {
                            legalHold = await Object.ReadLegalHold(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/xml";
                            await ctx.Response.Send(SerializationHelper.SerializeXml(legalHold)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectReadRange:
                        if (Object.ReadRange != null)
                        {
                            s3obj = await Object.ReadRange(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = s3obj.ContentType;
                            ctx.Response.ContentLength = s3obj.Size;
                            await ctx.Response.Send(s3obj.Size, s3obj.Data).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectReadRetention:
                        if (Object.ReadRetention != null)
                        {
                            retention = await Object.ReadRetention(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/xml";
                            await ctx.Response.Send(SerializationHelper.SerializeXml(retention)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectReadTags:
                        if (Object.ReadTagging != null)
                        {
                            tagging = await Object.ReadTagging(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/xml";
                            await ctx.Response.Send(SerializationHelper.SerializeXml(tagging)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectWrite:
                        if (Object.Write != null)
                        {
                            await Object.Write(s3ctx).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectWriteAcl:
                        if (Object.WriteAcl != null)
                        {
                            try
                            {
                                acp = SerializationHelper.DeserializeXml<AccessControlPolicy>(s3ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3ctx);
                                ioe.Data.Add("RequestBody", s3ctx.Request.DataAsString);
                                Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + SerializationHelper.SerializeJson(ioe, true));
                                await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                return;
                            }

                            await Object.WriteAcl(s3ctx, acp).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectWriteLegalHold:
                        if (Object.WriteLegalHold != null)
                        {
                            try
                            {
                                legalHold = SerializationHelper.DeserializeXml<LegalHold>(s3ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3ctx);
                                ioe.Data.Add("RequestBody", s3ctx.Request.DataAsString);
                                Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + SerializationHelper.SerializeJson(ioe, true));
                                await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                return;
                            }

                            await Object.WriteLegalHold(s3ctx, legalHold).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectWriteRetention:
                        if (Object.WriteRetention != null)
                        {
                            try
                            {
                                retention = SerializationHelper.DeserializeXml<Retention>(s3ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3ctx);
                                ioe.Data.Add("RequestBody", s3ctx.Request.DataAsString);
                                Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + SerializationHelper.SerializeJson(ioe, true));
                                await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                return;
                            }

                            await Object.WriteRetention(s3ctx, retention).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectWriteTags:
                        if (Object.WriteTagging != null)
                        {
                            try
                            {
                                tagging = SerializationHelper.DeserializeXml<Tagging>(s3ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3ctx);
                                ioe.Data.Add("RequestBody", s3ctx.Request.DataAsString);
                                Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + SerializationHelper.SerializeJson(ioe, true));
                                await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                return;
                            }

                            await Object.WriteTagging(s3ctx, tagging).ConfigureAwait(false);
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                        #endregion
                }

                if (DefaultRequestHandler != null)
                {
                    await DefaultRequestHandler(s3ctx).ConfigureAwait(false);
                    return;
                }

                await s3ctx.Response.Send(S3Objects.ErrorCode.InvalidRequest).ConfigureAwait(false);
                return;
            }
            catch (S3Exception s3e)
            {
                if (Logging.Exceptions) Logger?.Invoke(_Header + "S3 exception:" + Environment.NewLine + SerializationHelper.SerializeJson(s3e, true));

                s3ctx.Response.StatusCode = s3e.HttpStatusCode;
                s3ctx.Response.ContentType = "application/xml";
                await s3ctx.Response.Send(s3e.Error).ConfigureAwait(false);
                return;
            }
            catch (Exception e)
            {
                if (Logging.Exceptions) Logger?.Invoke(_Header + "exception:" + Environment.NewLine + SerializationHelper.SerializeJson(e, true));

                s3ctx.Response.StatusCode = 500;
                s3ctx.Response.ContentType = "application/xml";
                await s3ctx.Response.Send(S3Objects.ErrorCode.InternalError).ConfigureAwait(false);
                return;
            }
            finally
            {
                s3ctx.Timestamp.End = DateTime.UtcNow;

                if (Logging.HttpRequests)
                {
                    Logger?.Invoke(
                        _Header + 
                        "[" + 
                        ctx.Request.Source.IpAddress + ":" +
                        ctx.Request.Source.Port + 
                        "] " +
                        ctx.Request.Method.ToString() + " " +
                        ctx.Request.Url.RawWithoutQuery + " " +
                        s3ctx.Response.StatusCode + 
                        " [" + s3ctx.Timestamp.TotalMs + "ms]");
                }

                if (PostRequestHandler != null) await PostRequestHandler(s3ctx).ConfigureAwait(false);
            }
        }
         
        #endregion
    }
}
