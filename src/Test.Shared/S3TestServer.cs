namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.S3;
    using S3ServerLibrary;
    using S3ServerLibrary.S3Objects;

    /// <summary>
    /// Manages a test S3Server instance and provides clients for interacting with it.
    /// Encapsulates server setup, callback registration, and client creation.
    /// </summary>
    public class S3TestServer : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Hostname for the server.
        /// </summary>
        public string Hostname { get; private set; } = "127.0.0.1";

        /// <summary>
        /// Port for the server.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Test bucket name.
        /// </summary>
        public string Bucket { get; private set; } = "test-bucket";

        /// <summary>
        /// AWS access key.
        /// </summary>
        public string AccessKey { get; private set; } = "AKIAIOSFODNN7EXAMPLE";

        /// <summary>
        /// AWS secret key.
        /// </summary>
        public string SecretKey { get; private set; } = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY";

        /// <summary>
        /// Whether signature validation is enabled.
        /// </summary>
        public bool EnableSignatures { get; private set; }

        /// <summary>
        /// Base URL for raw HTTP requests.
        /// </summary>
        public string BaseUrl => $"http://{Hostname}:{Port}";

        /// <summary>
        /// The underlying S3Server instance.
        /// </summary>
        public S3Server Server { get; private set; }

        /// <summary>
        /// AWS S3 client configured for this server.
        /// </summary>
        public IAmazonS3 S3Client { get; private set; }

        /// <summary>
        /// Raw HTTP client for non-SDK requests.
        /// </summary>
        public HttpClient HttpClient { get; private set; }

        #endregion

        #region Private-Members

        private bool _Disposed = false;
        private Owner _Owner = new Owner("admin", "Administrator");
        private Grantee _Grantee = new Grantee("admin", "Administrator", null, "CanonicalUser", "admin@admin.com");

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="port">Port number. Default 8001.</param>
        /// <param name="enableSignatures">True to enable AWS signature validation.</param>
        public S3TestServer(int port = 8001, bool enableSignatures = false)
        {
            Port = port;
            EnableSignatures = enableSignatures;

            InitializeServer();
            InitializeClient();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose of resources.
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

            if (S3Client != null)
            {
                S3Client.Dispose();
            }

            if (HttpClient != null)
            {
                HttpClient.Dispose();
            }
        }

        #endregion

        #region Private-Methods

        private void InitializeServer()
        {
            S3ServerSettings settings = new S3ServerSettings();
            settings.Webserver.Hostname = Hostname;
            settings.Webserver.Port = Port;
            settings.Webserver.Ssl.Enable = false;
            settings.Logging.HttpRequests = false;
            settings.Logging.S3Requests = false;
            settings.OperationLimits.MaxPutObjectSize = 1024;
            settings.EnableSignatures = EnableSignatures;

            bool postHandlerExceptionTriggered = false;

            settings.PostRequestHandler = async (ctx) =>
            {
                if (ctx.Request.Bucket == "post-handler-exception-bucket" && !postHandlerExceptionTriggered)
                {
                    postHandlerExceptionTriggered = true;
                    throw new InvalidOperationException("Intentional PostRequestHandler exception for testing");
                }
            };

            settings.DefaultRequestHandler = async (ctx) =>
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "text/plain";
                await ctx.Response.Send("Handled by default handler");
            };

            Server = new S3Server(settings);

            SetupServiceCallbacks();
            SetupBucketCallbacks();
            SetupObjectCallbacks();

            if (EnableSignatures)
            {
                Server.Service.GetSecretKey = (ctx) =>
                {
                    if (ctx.Request.AccessKey == AccessKey)
                        return SecretKey;
                    return null;
                };
            }

            Server.Start();
        }

        private void InitializeClient()
        {
            BasicAWSCredentials credentials = new BasicAWSCredentials(AccessKey, SecretKey);

            AmazonS3Config config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USWest1,
                ServiceURL = $"http://{Hostname}:{Port}/",
                ForcePathStyle = true,
                UseHttp = true,
                MaxErrorRetry = 0,
                Timeout = TimeSpan.FromSeconds(5)
            };

            S3Client = new AmazonS3Client(credentials, config);

            HttpClient = new HttpClient();
            HttpClient.Timeout = TimeSpan.FromSeconds(5);
            HttpClient.DefaultRequestHeaders.ConnectionClose = true;
        }

        private void SetupServiceCallbacks()
        {
            Server.Service.ServiceExists = async (ctx) =>
            {
                return "us-west-1";
            };

            Server.Service.ListBuckets = async (ctx) =>
            {
                ListAllMyBucketsResult result = new ListAllMyBucketsResult();
                result.Owner = _Owner;
                result.Buckets = new Buckets(new List<S3ServerLibrary.S3Objects.Bucket>
                {
                    new S3ServerLibrary.S3Objects.Bucket("default", DateTime.UtcNow)
                });
                return result;
            };
        }

        private void SetupBucketCallbacks()
        {
            Server.Bucket.Write = async (ctx) =>
            {
                if (ctx.Request.Bucket == "access-denied-bucket")
                    throw new S3Exception(new Error(ErrorCode.AccessDenied));

                if (ctx.Request.Bucket == "exception-bucket")
                    throw new InvalidOperationException("Intentional test exception");
            };

            Server.Bucket.Exists = async (ctx) =>
            {
                if (ctx.Request.Bucket == "nonexistent-bucket-xyz")
                    return false;

                return true;
            };

            Server.Bucket.Delete = async (ctx) => { };
            Server.Bucket.DeleteAcl = async (ctx) => { };

            Server.Bucket.Read = async (ctx) =>
            {
                List<S3ServerLibrary.S3Objects.ObjectMetadata> contents = new List<S3ServerLibrary.S3Objects.ObjectMetadata>
                {
                    new S3ServerLibrary.S3Objects.ObjectMetadata("hello.txt", DateTime.UtcNow, "6cd3556deb0da54bca060b4c39479839", 13, _Owner)
                };

                return new ListBucketResult(
                    ctx.Request.Bucket,
                    contents,
                    1,
                    ctx.Request.MaxKeys,
                    ctx.Request.Prefix,
                    ctx.Request.Marker,
                    ctx.Request.Delimiter,
                    false,
                    null,
                    null,
                    "us-west-1");
            };

            Server.Bucket.ReadAcl = async (ctx) =>
            {
                AccessControlList acl = new AccessControlList(new List<Grant>
                {
                    new Grant(_Grantee, PermissionEnum.FullControl)
                });
                return new AccessControlPolicy(_Owner, acl);
            };

            Server.Bucket.WriteAcl = async (ctx, acp) => { };

            Server.Bucket.ReadLocation = async (ctx) => new LocationConstraint("us-west-1");

            Server.Bucket.ReadTagging = async (ctx) =>
            {
                return new S3ServerLibrary.S3Objects.Tagging(new TagSet(new List<S3ServerLibrary.S3Objects.Tag>
                {
                    new S3ServerLibrary.S3Objects.Tag("key", "value")
                }));
            };

            Server.Bucket.WriteTagging = async (ctx, tags) => { };
            Server.Bucket.DeleteTagging = async (ctx) => { };

            Server.Bucket.ReadVersioning = async (ctx) =>
            {
                return new VersioningConfiguration(VersioningStatusEnum.Enabled, MfaDeleteStatusEnum.Disabled);
            };

            Server.Bucket.WriteVersioning = async (ctx, ver) => { };

            Server.Bucket.ReadVersions = async (ctx) =>
            {
                List<ObjectVersion> versions = new List<ObjectVersion>
                {
                    new ObjectVersion("version1.key", "1", true, DateTime.UtcNow, "098f6bcd4621d373cade4e832627b4f6", 500, _Owner)
                };

                List<DeleteMarker> deleteMarkers = new List<DeleteMarker>
                {
                    new DeleteMarker("deleted1.key", "2", true, DateTime.UtcNow, _Owner)
                };

                return new ListVersionsResult(
                    ctx.Request.Bucket,
                    versions,
                    deleteMarkers,
                    ctx.Request.MaxKeys,
                    ctx.Request.Prefix,
                    ctx.Request.Marker,
                    null,
                    false,
                    "us-west-1");
            };

            Server.Bucket.ReadLogging = async (ctx) =>
            {
                return new BucketLoggingStatus(new LoggingEnabled("default", "logs/", new TargetGrants()));
            };

            Server.Bucket.WriteLogging = async (ctx, logging) => { };

            Server.Bucket.ReadWebsite = async (ctx) =>
            {
                S3ServerLibrary.S3Objects.WebsiteConfiguration website = new S3ServerLibrary.S3Objects.WebsiteConfiguration();
                website.ErrorDocument = new ErrorDocument("error.html");
                website.IndexDocument = new IndexDocument("index.html");
                return website;
            };

            Server.Bucket.WriteWebsite = async (ctx, website) => { };
            Server.Bucket.DeleteWebsite = async (ctx) => { };

            Server.Bucket.ReadMultipartUploads = async (ctx) =>
            {
                ListMultipartUploadsResult result = new ListMultipartUploadsResult();
                result.Bucket = ctx.Request.Bucket;
                result.Uploads = new List<S3ServerLibrary.S3Objects.Upload>
                {
                    new S3ServerLibrary.S3Objects.Upload
                    {
                        Key = "test-key",
                        UploadId = "upload-id-123",
                        Owner = _Owner,
                        Initiator = _Owner,
                        Initiated = DateTime.UtcNow,
                        StorageClass = StorageClassEnum.STANDARD
                    }
                };
                result.Prefix = ctx.Request.Prefix;
                result.Delimiter = ctx.Request.Delimiter;
                result.KeyMarker = ctx.Request.Marker;
                result.MaxUploads = ctx.Request.MaxKeys;
                result.IsTruncated = false;
                return result;
            };
        }

        private void SetupObjectCallbacks()
        {
            Server.Object.Write = async (ctx) => { };

            Server.Object.Exists = async (ctx) =>
            {
                if (ctx.Request.Key == "nonexistent-object-xyz.bin")
                    return null;

                return new S3ServerLibrary.S3Objects.ObjectMetadata(
                    ctx.Request.Key,
                    DateTime.UtcNow,
                    "6cd3556deb0da54bca060b4c39479839",
                    13,
                    _Owner);
            };

            Server.Object.Read = async (ctx) =>
            {
                if (ctx.Request.Key == "nonexistent-object-xyz.bin")
                    return null;

                return new S3ServerLibrary.S3Object(
                    ctx.Request.Key,
                    "1",
                    true,
                    DateTime.UtcNow,
                    "5d41402abc4b2a76b9719d911017c592",
                    5,
                    _Owner,
                    "hello",
                    "text/plain");
            };

            Server.Object.ReadRange = async (ctx) =>
            {
                string data = "hello";
                string rangeData = data.Substring((int)ctx.Request.RangeStart, (int)((int)ctx.Request.RangeEnd - (int)ctx.Request.RangeStart + 1));

                return new S3ServerLibrary.S3Object(
                    ctx.Request.Key,
                    "1",
                    true,
                    DateTime.UtcNow,
                    "5d41402abc4b2a76b9719d911017c592",
                    rangeData.Length,
                    _Owner,
                    rangeData,
                    "text/plain");
            };

            Server.Object.ReadAcl = async (ctx) =>
            {
                AccessControlList acl = new AccessControlList(new List<Grant>
                {
                    new Grant(_Grantee, PermissionEnum.FullControl)
                });
                return new AccessControlPolicy(_Owner, acl);
            };

            Server.Object.WriteAcl = async (ctx, acp) => { };
            Server.Object.DeleteAcl = async (ctx) => { };

            Server.Object.ReadTagging = async (ctx) =>
            {
                return new S3ServerLibrary.S3Objects.Tagging(new TagSet(new List<S3ServerLibrary.S3Objects.Tag>
                {
                    new S3ServerLibrary.S3Objects.Tag("key", "value")
                }));
            };

            Server.Object.WriteTagging = async (ctx, tags) => { };
            Server.Object.DeleteTagging = async (ctx) => { };

            Server.Object.ReadRetention = async (ctx) =>
            {
                return new S3ServerLibrary.S3Objects.Retention(RetentionModeEnum.Governance, DateTime.UtcNow.AddDays(30));
            };

            Server.Object.WriteRetention = async (ctx, retention) => { };

            Server.Object.ReadLegalHold = async (ctx) =>
            {
                return new LegalHold("ON");
            };

            Server.Object.WriteLegalHold = async (ctx, legalHold) => { };

            Server.Object.Delete = async (ctx) => { };

            Server.Object.DeleteMultiple = async (ctx, del) =>
            {
                List<Deleted> deleted = new List<Deleted>();
                foreach (S3ServerLibrary.S3Objects.Object obj in del.Objects)
                {
                    deleted.Add(new Deleted(obj.Key, "1", false));
                }
                return new DeleteResult(deleted, null);
            };

            Server.Object.CreateMultipartUpload = async (ctx) =>
            {
                return new InitiateMultipartUploadResult(
                    ctx.Request.Bucket,
                    ctx.Request.Key,
                    "upload-id-123");
            };

            Server.Object.UploadPart = async (ctx) => { };

            Server.Object.ReadParts = async (ctx) =>
            {
                ListPartsResult result = new ListPartsResult();
                result.Bucket = ctx.Request.Bucket;
                result.Key = ctx.Request.Key;
                result.UploadId = ctx.Request.UploadId;
                result.Owner = _Owner;
                result.Initiator = _Owner;
                result.StorageClass = StorageClassEnum.STANDARD;
                result.PartNumberMarker = 1;
                result.MaxParts = 1000;
                result.IsTruncated = false;
                result.Parts = new List<S3ServerLibrary.S3Objects.Part>
                {
                    new S3ServerLibrary.S3Objects.Part
                    {
                        PartNumber = 1,
                        LastModified = DateTime.UtcNow,
                        ETag = "5d41402abc4b2a76b9719d911017c592",
                        Size = 1024
                    }
                };
                return result;
            };

            Server.Object.CompleteMultipartUpload = async (ctx, complete) =>
            {
                CompleteMultipartUploadResult result = new CompleteMultipartUploadResult();
                result.Location = "http://localhost/";
                result.Bucket = ctx.Request.Bucket;
                result.Key = ctx.Request.Key;
                result.ETag = "9b2c3e7a8d1f4e6b5c2a1d8f7e4b3c2a";
                return result;
            };

            Server.Object.AbortMultipartUpload = async (ctx) => { };

            Server.Object.SelectContent = async (ctx, selectReq) => { };
        }

        #endregion
    }
}
