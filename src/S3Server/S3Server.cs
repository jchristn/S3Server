namespace S3ServerLibrary
{
    using S3ServerLibrary.Callbacks;
    using S3ServerLibrary.S3Objects;
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using AWSSignatureGenerator;
    using WatsonWebserver;
    using WatsonWebserver.Core;
    using System.Net.NetworkInformation;
    using System.Globalization;

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
                return _Webserver.IsListening;
            }
        }

        /// <summary>
        /// Settings.
        /// </summary>
        public S3ServerSettings Settings
        {
            get
            {
                return _Settings;
            }
            set
            {
                if (value == null) _Settings = new S3ServerSettings();
                else _Settings = value;
            }
        }

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
        /// Access the underlying webserver.
        /// </summary>
        public Webserver Webserver
        {
            get
            {
                return _Webserver;
            }
        }

        #endregion

        #region Private-Members

        private string _Header = "[S3Server] ";
        private bool _Disposed = false;

        private Webserver _Webserver = null;
        private S3ServerSettings _Settings = new S3ServerSettings();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="settings">Settings.</param>
        public S3Server(S3ServerSettings settings)
        {
            if (settings == null) settings = new S3ServerSettings();
            _Settings = settings;
            _Webserver = new Webserver(_Settings.Webserver, RequestHandler);
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
            _Webserver.Start();
        }

        /// <summary>
        /// Stop accepting new connections.
        /// </summary>
        public void Stop()
        {
            _Webserver.Stop();
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
                _Settings.Logger?.Invoke(_Header + "dispose requested");

                if (_Webserver != null) _Webserver.Dispose();
            }

            _Webserver = null;
            _Disposed = true;
        }

        private async Task RequestHandler(HttpContextBase ctx)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

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
            InitiateMultipartUploadResult initiateMultipart = null;
            ListMultipartUploadsResult listMultipartUploads = null;
            ListPartsResult listParts = null;
            CompleteMultipartUpload completeMultipartRequest = null;
            CompleteMultipartUploadResult completeMultipartResult = null;
            SelectObjectContentRequest selectRequest = null;

            S3Context s3ctx = null;

            try
            {
                using (s3ctx = new S3Context(ctx, Service.FindMatchingBaseDomain, null, (_Settings.Logging.S3Requests ? _Settings.Logger : null)))
                {
                    s3ctx.Response.Headers.Add(Constants.HeaderRequestId, s3ctx.Request.RequestId);
                    s3ctx.Response.Headers.Add(Constants.HeaderTraceId, s3ctx.Request.TraceId);
                    s3ctx.Response.Headers.Add(Constants.HeaderConnection, "close");

                    if (_Settings.Logging.HttpRequests)
                        _Settings.Logger?.Invoke(_Header + "HTTP request: " + Environment.NewLine + SerializationHelper.SerializeJson(s3ctx.Http, true));

                    if (_Settings.Logging.S3Requests)
                        _Settings.Logger?.Invoke(_Header + "S3 request: " + Environment.NewLine + SerializationHelper.SerializeJson(s3ctx.Request, true));

                    if (_Settings.PreRequestHandler != null)
                    {
                        success = await _Settings.PreRequestHandler(s3ctx).ConfigureAwait(false);
                        if (success)
                        {
                            await s3ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                    }

                    if (_Settings.EnableSignatures)
                    {
                        if (Service.GetSecretKey != null)
                        {
                            string secretKey = Service.GetSecretKey(s3ctx);
                            if (String.IsNullOrEmpty(secretKey))
                            {
                                _Settings.Logger?.Invoke(_Header + "unable to retrieve secret key for signature " + s3ctx.Request.Signature);
                                throw new S3Exception(new Error(ErrorCode.AccessDenied));
                            }

                            if (s3ctx.Request.SignatureVersion == S3SignatureVersion.Version2)
                            {
                                _Settings.Logger?.Invoke(_Header + "invalid v2 signature '" + s3ctx.Request.Signature + "'");
                                throw new S3Exception(new Error(ErrorCode.SignatureDoesNotMatch));
                            }
                            else if (s3ctx.Request.SignatureVersion == S3SignatureVersion.Version4)
                            {
                                V4SignatureResult result = new V4SignatureResult(
                                    DateTime.UtcNow.ToString(Constants.AmazonTimestampFormatCompact),
                                    s3ctx.Http.Request.Method.ToString().ToUpper(),
                                    s3ctx.Http.Request.Url.Full,
                                    s3ctx.Request.AccessKey,
                                    secretKey,
                                    s3ctx.Request.Region,
                                    "s3",
                                    s3ctx.Http.Request.Headers,
                                    s3ctx.Http.Request.DataAsBytes);
                                 
                                if (!result.Signature.Equals(s3ctx.Request.Signature))
                                {
                                    _Settings.Logger?.Invoke(_Header + "invalid v4 signature '" + s3ctx.Request.Signature + "'");
                                    throw new S3Exception(new Error(ErrorCode.SignatureDoesNotMatch));
                                }
                            }
                            else
                            {
                                _Settings.Logger?.Invoke(_Header + "unknown signature version");
                                throw new S3Exception(new Error(ErrorCode.AccessDenied));
                            }
                        }
                    }

                    switch (s3ctx.Request.RequestType)
                    {
                        #region Service

                        case S3RequestType.ServiceExists:
                            if (Service.ServiceExists != null)
                            {
                                string region = await Service.ServiceExists(s3ctx).ConfigureAwait(false);
                                if (!String.IsNullOrEmpty(region)) s3ctx.Response.Headers.Add(Constants.HeaderBucketRegion, region);

                                s3ctx.Response.StatusCode = 200;
                                await s3ctx.Response.Send().ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.ListBuckets:
                            if (Service.ListBuckets != null)
                            {
                                buckets = await Service.ListBuckets(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(buckets)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        #endregion

                        #region Bucket

                        case S3RequestType.BucketDelete:
                            if (Bucket.Delete != null)
                            {
                                await Bucket.Delete(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 204;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.BucketDeleteAcl:
                            if (Bucket.DeleteAcl != null)
                            {
                                await Bucket.DeleteAcl(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 204;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.BucketDeleteTags:
                            if (Bucket.DeleteTagging != null)
                            {
                                await Bucket.DeleteTagging(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 204;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.BucketDeleteWebsite:
                            if (Bucket.DeleteWebsite != null)
                            {
                                await Bucket.DeleteWebsite(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 204;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.BucketExists:
                            if (Bucket.Exists != null)
                            {
                                exists = await Bucket.Exists(s3ctx).ConfigureAwait(false);
                                if (exists)
                                {
                                    s3ctx.Response.StatusCode = 200;
                                    s3ctx.Response.ContentType = Constants.ContentTypeText;
                                    await s3ctx.Response.Send().ConfigureAwait(false);
                                }
                                else
                                {
                                    error = new Error(ErrorCode.NoSuchBucket);
                                    s3ctx.Response.StatusCode = 404;
                                    s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                    await s3ctx.Response.Send(SerializationHelper.SerializeXml(error)).ConfigureAwait(false);
                                }
                                return;
                            }
                            break;

                        case S3RequestType.BucketRead:
                            if (Bucket.Read != null)
                            {
                                listBucketResult = await Bucket.Read(s3ctx).ConfigureAwait(false);
                                
                                if (!String.IsNullOrEmpty(listBucketResult.BucketRegion))
                                    s3ctx.Response.Headers.Add("x-amz-bucket-region", listBucketResult.BucketRegion);

                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(listBucketResult)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.BucketReadAcl:
                            if (Bucket.ReadAcl != null)
                            {
                                acp = await Bucket.ReadAcl(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(acp)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.BucketReadLocation:
                            if (Bucket.ReadLocation != null)
                            {
                                location = await Bucket.ReadLocation(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(location)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.BucketReadLogging:
                            if (Bucket.ReadLogging != null)
                            {
                                bucketLogging = await Bucket.ReadLogging(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(bucketLogging)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.BucketReadMultipartUploads:
                            if (Bucket.ReadMultipartUploads != null)
                            {
                                listMultipartUploads = await Bucket.ReadMultipartUploads(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(listMultipartUploads)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.BucketReadTags:
                            if (Bucket.ReadTagging != null)
                            {
                                tagging = await Bucket.ReadTagging(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(tagging)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.BucketReadVersioning:
                            if (Bucket.ReadVersioning != null)
                            {
                                versionConfig = await Bucket.ReadVersioning(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(versionConfig)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.BucketReadVersions:
                            if (Bucket.ReadVersions != null)
                            {
                                listVersionResult = await Bucket.ReadVersions(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(listVersionResult)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.BucketReadWebsite:
                            if (Bucket.ReadWebsite != null)
                            {
                                wc = await Bucket.ReadWebsite(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(wc)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.BucketWrite:
                            if (Bucket.Write != null)
                            {
                                await Bucket.Write(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
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
                                    _Settings.Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                    await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                    return;
                                }

                                await Bucket.WriteAcl(s3ctx, acp).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
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
                                    _Settings.Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                    await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                    return;
                                }

                                await Bucket.WriteLogging(s3ctx, bucketLogging).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
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
                                    _Settings.Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                    await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                    return;
                                }

                                await Bucket.WriteTagging(s3ctx, tagging).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
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
                                    _Settings.Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                    await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                    return;
                                }

                                await Bucket.WriteVersioning(s3ctx, versionConfig).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
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
                                    _Settings.Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                    await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                    return;
                                }

                                await Bucket.WriteWebsite(s3ctx, wc).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
                                return;
                            }
                            break;

                        #endregion

                        #region Object

                        case S3RequestType.ObjectAbortMultipartUpload:
                            if (Object.AbortMultipartUpload != null)
                            {
                                await Object.AbortMultipartUpload(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 204;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.ObjectCompleteMultipartUpload:
                            if (Object.DeleteMultiple != null)
                            {
                                try
                                {
                                    completeMultipartRequest = SerializationHelper.DeserializeXml<CompleteMultipartUpload>(s3ctx.Request.DataAsString);
                                }
                                catch (InvalidOperationException ioe)
                                {
                                    ioe.Data.Add("Context", s3ctx);
                                    ioe.Data.Add("RequestBody", s3ctx.Request.DataAsString);
                                    _Settings.Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                    await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                    return;
                                }

                                completeMultipartResult = await Object.CompleteMultipartUpload(s3ctx, completeMultipartRequest).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(completeMultipartResult)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.ObjectCreateMultipartUpload:
                            if (Object.CreateMultipartUpload != null)
                            {
                                initiateMultipart = await Object.CreateMultipartUpload(s3ctx);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(initiateMultipart)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.ObjectDelete:
                            if (Object.Delete != null)
                            {
                                await Object.Delete(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 204;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.ObjectDeleteAcl:
                            if (Object.DeleteAcl != null)
                            {
                                await Object.DeleteAcl(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 204;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
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
                                    _Settings.Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                    await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                    return;
                                }

                                delResult = await Object.DeleteMultiple(s3ctx, delMultiple).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(delResult)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.ObjectDeleteTags:
                            if (Object.DeleteTagging != null)
                            {
                                await Object.DeleteTagging(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 204;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.ObjectExists:
                            if (Object.Exists != null)
                            {
                                md = await Object.Exists(s3ctx).ConfigureAwait(false);
                                if (md != null)
                                {
                                    if (!String.IsNullOrEmpty(md.ETag)) s3ctx.Response.Headers.Add(Constants.HeaderETag, md.ETag);

                                    s3ctx.Response.Headers.Add(Constants.HeaderLastModified, md.LastModified.ToString(Constants.AmazonTimestampFormatVerbose, CultureInfo.InvariantCulture));
                                    s3ctx.Response.Headers.Add(Constants.HeaderStorageClass, md.StorageClass.ToString());
                                    s3ctx.Response.Headers.Add(Constants.HeaderAcceptRanges, "bytes");

                                    s3ctx.Response.StatusCode = 200;
                                    s3ctx.Response.ContentLength = md.Size;
                                    s3ctx.Response.ContentType = md.ContentType;
                                    await s3ctx.Response.Send().ConfigureAwait(false);
                                }
                                else
                                {
                                    error = new Error(ErrorCode.NoSuchKey);
                                    s3ctx.Response.StatusCode = 404;
                                    s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                    await s3ctx.Response.Send(SerializationHelper.SerializeXml(error)).ConfigureAwait(false);
                                }
                                return;
                            }
                            break;

                        case S3RequestType.ObjectRead:
                            if (Object.Read != null)
                            {
                                s3obj = await Object.Read(s3ctx).ConfigureAwait(false);

                                if (s3obj != null)
                                {
                                    if (!String.IsNullOrEmpty(s3obj.ETag)) s3ctx.Response.Headers.Add(Constants.HeaderETag, s3obj.ETag);

                                    s3ctx.Response.Headers.Add(Constants.HeaderLastModified, s3obj.LastModified.ToString(Constants.AmazonTimestampFormatVerbose, CultureInfo.InvariantCulture));
                                    s3ctx.Response.Headers.Add(Constants.HeaderStorageClass, s3obj.StorageClass.ToString());
                                    s3ctx.Response.Headers.Add(Constants.HeaderAcceptRanges, "bytes");

                                    s3ctx.Response.StatusCode = 200;
                                    s3ctx.Response.ContentType = s3obj.ContentType;
                                    s3ctx.Response.ContentLength = s3obj.Size;

                                    await s3ctx.Response.Send(s3obj.Size, s3obj.Data).ConfigureAwait(false);
                                }
                                else
                                {
                                    error = new Error(ErrorCode.NoSuchKey);
                                    s3ctx.Response.StatusCode = 404;
                                    s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                    await s3ctx.Response.Send(SerializationHelper.SerializeXml(error)).ConfigureAwait(false);
                                }
                                return;
                            }
                            break;

                        case S3RequestType.ObjectReadAcl:
                            if (Object.ReadAcl != null)
                            {
                                acp = await Object.ReadAcl(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(acp)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.ObjectReadLegalHold:
                            if (Object.ReadLegalHold != null)
                            {
                                legalHold = await Object.ReadLegalHold(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(legalHold)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.ObjectReadParts:
                            if (Object.ReadParts != null)
                            {
                                listParts = await Object.ReadParts(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(listParts)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.ObjectReadRange:
                            if (Object.ReadRange != null)
                            {
                                s3obj = await Object.ReadRange(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = s3obj.ContentType;
                                s3ctx.Response.ContentLength = s3obj.Size;
                                await s3ctx.Response.Send(s3obj.Size, s3obj.Data).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.ObjectReadRetention:
                            if (Object.ReadRetention != null)
                            {
                                retention = await Object.ReadRetention(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(retention)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.ObjectReadTags:
                            if (Object.ReadTagging != null)
                            {
                                tagging = await Object.ReadTagging(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3ctx.Response.Send(SerializationHelper.SerializeXml(tagging)).ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.ObjectSelectContent:
                            if (Object.WriteTagging != null)
                            {
                                try
                                {
                                    selectRequest = SerializationHelper.DeserializeXml<SelectObjectContentRequest>(s3ctx.Request.DataAsString);
                                }
                                catch (InvalidOperationException ioe)
                                {
                                    ioe.Data.Add("Context", s3ctx);
                                    ioe.Data.Add("RequestBody", s3ctx.Request.DataAsString);
                                    _Settings.Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                    await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                    return;
                                }

                                await Object.SelectContent(s3ctx, selectRequest).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.ObjectUploadPart:
                            if (Object.UploadPart != null)
                            {
                                await Object.UploadPart(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
                                return;
                            }
                            break;

                        case S3RequestType.ObjectWrite:
                            if (Object.Write != null)
                            {
                                if (s3ctx.Request.ContentLength > _Settings.OperationLimits.MaxPutObjectSize)
                                {
                                    error = new Error(ErrorCode.EntityTooLarge);
                                    s3ctx.Response.StatusCode = 400;
                                    s3ctx.Response.ContentType = Constants.ContentTypeXml;
                                    await s3ctx.Response.Send(SerializationHelper.SerializeXml(error)).ConfigureAwait(false);
                                }

                                await Object.Write(s3ctx).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
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
                                    _Settings.Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                    await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                    return;
                                }

                                await Object.WriteAcl(s3ctx, acp).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
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
                                    _Settings.Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                    await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                    return;
                                }

                                await Object.WriteLegalHold(s3ctx, legalHold).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
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
                                    _Settings.Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                    await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                    return;
                                }

                                await Object.WriteRetention(s3ctx, retention).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
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
                                    _Settings.Logger?.Invoke(_Header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                    await s3ctx.Response.Send(S3Objects.ErrorCode.MalformedXML).ConfigureAwait(false);
                                    return;
                                }

                                await Object.WriteTagging(s3ctx, tagging).ConfigureAwait(false);
                                s3ctx.Response.StatusCode = 200;
                                s3ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3ctx.Response.Send().ConfigureAwait(false);
                                return;
                            }
                            break;

                            #endregion
                    }

                    if (_Settings.DefaultRequestHandler != null)
                    {
                        await _Settings.DefaultRequestHandler(s3ctx).ConfigureAwait(false);
                        return;
                    }

                    await s3ctx.Response.Send(S3Objects.ErrorCode.InvalidRequest).ConfigureAwait(false);
                    return;
                }
            }
            catch (S3Exception s3e)
            {
                _Settings.Logger?.Invoke(_Header + "S3 exception:" + Environment.NewLine + s3e.ToString());

                if (s3ctx != null)
                {
                    s3ctx.Response.StatusCode = s3e.HttpStatusCode;
                    s3ctx.Response.ContentType = Constants.ContentTypeXml;
                    await s3ctx.Response.Send(s3e.Error).ConfigureAwait(false);
                }

                return;
            }
            catch (Exception e)
            {
                _Settings.Logger?.Invoke(_Header + "exception:" + Environment.NewLine + e.ToString());

                if (s3ctx != null)
                {
                    s3ctx.Response.StatusCode = 500;
                    s3ctx.Response.ContentType = Constants.ContentTypeXml;
                    await s3ctx.Response.Send(S3Objects.ErrorCode.InternalError).ConfigureAwait(false);
                }

                return;
            }
            finally
            {
                s3ctx.Timestamp.End = DateTime.UtcNow;
                if (_Settings.PostRequestHandler != null) await _Settings.PostRequestHandler(s3ctx).ConfigureAwait(false);
            }
        }

        #endregion
    }
}
