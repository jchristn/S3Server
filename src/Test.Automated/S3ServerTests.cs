namespace Test.Automated
{
    using Amazon;
    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.S3.Model;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;
    using S3ServerLibrary;
    using S3ServerLibrary.S3Objects;
    using S3Object = S3ServerLibrary.S3Object;

    /// <summary>
    /// Comprehensive automated tests for S3Server library.
    /// Tests all service, bucket, and object APIs with a mock S3 server.
    /// </summary>
    public class S3ServerTests : IDisposable
    {
#pragma warning disable S31000 // Property value too short
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Private-Members

        private S3Server _Server = null;
        private IAmazonS3 _Client = null;
        private string _Hostname = "localhost";
        private int _Port = 8001;
        private string _Bucket = "test-bucket";
        private string _AccessKey = "AKIAIOSFODNN7EXAMPLE";
        private string _SecretKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY";
        private S3ServerLibrary.S3Objects.Owner _Owner = new S3ServerLibrary.S3Objects.Owner("admin", "Administrator");
        private Grantee _Grantee = new Grantee("admin", "Administrator", null, "CanonicalUser", "admin@admin.com");

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Initialize test fixture.
        /// Sets up mock S3 server and AWS S3 client.
        /// </summary>
        public S3ServerTests()
        {
            InitializeServer();
            InitializeClient();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Cleanup resources.
        /// </summary>
        public void Dispose()
        {
            if (_Server != null)
            {
                _Server.Stop();
                _Server.Dispose();
            }

            if (_Client != null)
            {
                _Client.Dispose();
            }
        }

        #endregion

        #region Service-API-Tests

        /// <summary>
        /// Test service exists HEAD request.
        /// </summary>
        [Fact]
        public async Task ServiceExists_ShouldReturnRegion()
        {
            GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
            {
                BucketName = "",
                Verb = HttpVerb.HEAD,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            string url = _Client.GetPreSignedURL(request);
            Assert.NotNull(url);
        }

        /// <summary>
        /// Test list buckets operation.
        /// </summary>
        [Fact]
        public async Task ListBuckets_ShouldReturnBucketList()
        {
            ListBucketsRequest request = new ListBucketsRequest();
            ListBucketsResponse response = await _Client.ListBucketsAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
            Assert.NotNull(response.Buckets);
            Assert.NotEmpty(response.Buckets);
            Assert.Equal("default", response.Buckets[0].BucketName);
        }

        #endregion

        #region Bucket-API-Tests

        /// <summary>
        /// Test bucket write (create) operation.
        /// </summary>
        [Fact]
        public async Task BucketWrite_ShouldCreateBucket()
        {
            PutBucketRequest request = new PutBucketRequest
            {
                BucketName = _Bucket
            };

            PutBucketResponse response = await _Client.PutBucketAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test bucket exists HEAD request.
        /// </summary>
        [Fact]
        public async Task BucketExists_ShouldReturnTrue()
        {
            GetBucketLocationRequest request = new GetBucketLocationRequest
            {
                BucketName = _Bucket
            };

            GetBucketLocationResponse response = await _Client.GetBucketLocationAsync(request);
            Assert.NotNull(response);
        }

        /// <summary>
        /// Test bucket read (list objects) operation.
        /// </summary>
        [Fact]
        public async Task BucketRead_ShouldListObjects()
        {
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = _Bucket
            };

            ListObjectsV2Response response = await _Client.ListObjectsV2Async(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
            Assert.NotNull(response.S3Objects);
        }

        /// <summary>
        /// Test bucket read ACL operation.
        /// </summary>
        [Fact]
        public async Task BucketReadAcl_ShouldReturnAccessControlPolicy()
        {
            GetACLRequest request = new GetACLRequest
            {
                BucketName = _Bucket
            };

            GetACLResponse response = await _Client.GetACLAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
            Assert.NotNull(response.AccessControlList);
            Assert.NotNull(response.AccessControlList.Owner);
        }

        /// <summary>
        /// Test bucket write ACL operation.
        /// </summary>
        [Fact]
        public async Task BucketWriteAcl_ShouldSetAccessControlPolicy()
        {
            PutACLRequest request = new PutACLRequest
            {
                BucketName = _Bucket,
                AccessControlList = new S3AccessControlList
                {
                    Owner = new Amazon.S3.Model.Owner { DisplayName = "admin", Id = "admin" },
                    Grants = new List<S3Grant>
                    {
                        new S3Grant
                        {
                            Grantee = new S3Grantee
                            {
                                CanonicalUser = "admin",
                                DisplayName = "admin"
                            },
                            Permission = S3Permission.FULL_CONTROL
                        }
                    }
                }
            };

            PutACLResponse response = await _Client.PutACLAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test bucket read location operation.
        /// </summary>
        [Fact]
        public async Task BucketReadLocation_ShouldReturnRegion()
        {
            GetBucketLocationRequest request = new GetBucketLocationRequest
            {
                BucketName = _Bucket
            };

            GetBucketLocationResponse response = await _Client.GetBucketLocationAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test bucket write tags operation.
        /// </summary>
        [Fact]
        public async Task BucketWriteTagging_ShouldSetTags()
        {
            PutBucketTaggingRequest request = new PutBucketTaggingRequest
            {
                BucketName = _Bucket,
                TagSet = new List<Amazon.S3.Model.Tag>
                {
                    new Amazon.S3.Model.Tag { Key = "Environment", Value = "Test" },
                    new Amazon.S3.Model.Tag { Key = "Application", Value = "S3Server" }
                }
            };

            PutBucketTaggingResponse response = await _Client.PutBucketTaggingAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test bucket read tags operation.
        /// </summary>
        [Fact]
        public async Task BucketReadTagging_ShouldReturnTags()
        {
            GetBucketTaggingRequest request = new GetBucketTaggingRequest
            {
                BucketName = _Bucket
            };

            GetBucketTaggingResponse response = await _Client.GetBucketTaggingAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
            Assert.NotNull(response.TagSet);
            Assert.NotEmpty(response.TagSet);
        }

        /// <summary>
        /// Test bucket write versioning operation.
        /// </summary>
        [Fact]
        public async Task BucketWriteVersioning_ShouldEnableVersioning()
        {
            PutBucketVersioningRequest request = new PutBucketVersioningRequest
            {
                BucketName = _Bucket,
                VersioningConfig = new S3BucketVersioningConfig
                {
                    Status = VersionStatus.Enabled
                }
            };

            PutBucketVersioningResponse response = await _Client.PutBucketVersioningAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test bucket read versioning operation.
        /// </summary>
        [Fact]
        public async Task BucketReadVersioning_ShouldReturnVersioningStatus()
        {
            GetBucketVersioningRequest request = new GetBucketVersioningRequest
            {
                BucketName = _Bucket
            };

            GetBucketVersioningResponse response = await _Client.GetBucketVersioningAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
            Assert.NotNull(response.VersioningConfig);
        }

        /// <summary>
        /// Test bucket read versions operation.
        /// </summary>
        [Fact]
        public async Task BucketReadVersions_ShouldReturnObjectVersions()
        {
            ListVersionsRequest request = new ListVersionsRequest
            {
                BucketName = _Bucket
            };

            ListVersionsResponse response = await _Client.ListVersionsAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test bucket write logging operation.
        /// </summary>
        [Fact]
        public async Task BucketWriteLogging_ShouldSetLoggingConfiguration()
        {
            PutBucketLoggingRequest request = new PutBucketLoggingRequest
            {
                BucketName = _Bucket,
                LoggingConfig = new S3BucketLoggingConfig
                {
                    TargetBucketName = _Bucket,
                    TargetPrefix = "logs/"
                }
            };

            PutBucketLoggingResponse response = await _Client.PutBucketLoggingAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test bucket read logging operation.
        /// </summary>
        [Fact]
        public async Task BucketReadLogging_ShouldReturnLoggingConfiguration()
        {
            GetBucketLoggingRequest request = new GetBucketLoggingRequest
            {
                BucketName = _Bucket
            };

            GetBucketLoggingResponse response = await _Client.GetBucketLoggingAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test bucket write website operation.
        /// </summary>
        [Fact]
        public async Task BucketWriteWebsite_ShouldSetWebsiteConfiguration()
        {
            PutBucketWebsiteRequest request = new PutBucketWebsiteRequest
            {
                BucketName = _Bucket,
                WebsiteConfiguration = new Amazon.S3.Model.WebsiteConfiguration
                {
                    IndexDocumentSuffix = "index.html",
                    ErrorDocument = "error.html"
                }
            };

            PutBucketWebsiteResponse response = await _Client.PutBucketWebsiteAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test bucket read website operation.
        /// </summary>
        [Fact]
        public async Task BucketReadWebsite_ShouldReturnWebsiteConfiguration()
        {
            GetBucketWebsiteRequest request = new GetBucketWebsiteRequest
            {
                BucketName = _Bucket
            };

            GetBucketWebsiteResponse response = await _Client.GetBucketWebsiteAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test bucket delete website operation.
        /// </summary>
        [Fact]
        public async Task BucketDeleteWebsite_ShouldRemoveWebsiteConfiguration()
        {
            DeleteBucketWebsiteRequest request = new DeleteBucketWebsiteRequest
            {
                BucketName = _Bucket
            };

            DeleteBucketWebsiteResponse response = await _Client.DeleteBucketWebsiteAsync(request);

            Assert.NotNull(response);
            Assert.Equal(204, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test bucket delete tags operation.
        /// </summary>
        [Fact]
        public async Task BucketDeleteTagging_ShouldRemoveTags()
        {
            DeleteBucketTaggingRequest request = new DeleteBucketTaggingRequest
            {
                BucketName = _Bucket
            };

            DeleteBucketTaggingResponse response = await _Client.DeleteBucketTaggingAsync(request);

            Assert.NotNull(response);
            Assert.Equal(204, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test bucket delete operation.
        /// </summary>
        [Fact]
        public async Task BucketDelete_ShouldRemoveBucket()
        {
            DeleteBucketRequest request = new DeleteBucketRequest
            {
                BucketName = "delete-test-bucket"
            };

            DeleteBucketResponse response = await _Client.DeleteBucketAsync(request);

            Assert.NotNull(response);
            Assert.Equal(204, (int)response.HttpStatusCode);
        }

        #endregion

        #region Object-API-Tests

        /// <summary>
        /// Test object write (PutObject) operation.
        /// </summary>
        [Fact]
        public async Task ObjectWrite_ShouldUploadObject()
        {
            string key = "test-object.txt";
            string content = "Hello, S3Server!";
            byte[] data = Encoding.UTF8.GetBytes(content);

            PutObjectRequest request = new PutObjectRequest
            {
                BucketName = _Bucket,
                Key = key,
                InputStream = new MemoryStream(data),
                ContentType = "text/plain"
            };

            PutObjectResponse response = await _Client.PutObjectAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test object exists (HeadObject) operation.
        /// </summary>
        [Fact]
        public async Task ObjectExists_ShouldReturnMetadata()
        {
            string key = "test-object.txt";

            GetObjectMetadataRequest request = new GetObjectMetadataRequest
            {
                BucketName = _Bucket,
                Key = key
            };

            GetObjectMetadataResponse response = await _Client.GetObjectMetadataAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test object read (GetObject) operation.
        /// </summary>
        [Fact]
        public async Task ObjectRead_ShouldDownloadObject()
        {
            string key = "test-object.txt";

            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = _Bucket,
                Key = key
            };

            GetObjectResponse response = await _Client.GetObjectAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
            Assert.True(response.ContentLength > 0);

            using (StreamReader reader = new StreamReader(response.ResponseStream))
            {
                string content = await reader.ReadToEndAsync();
                Assert.NotEmpty(content);
            }
        }

        /// <summary>
        /// Test object read range operation.
        /// </summary>
        [Fact]
        public async Task ObjectReadRange_ShouldDownloadPartialObject()
        {
            string key = "test-object.txt";

            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = _Bucket,
                Key = key,
                ByteRange = new ByteRange(0, 4)
            };

            GetObjectResponse response = await _Client.GetObjectAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test object write ACL operation.
        /// </summary>
        [Fact]
        public async Task ObjectWriteAcl_ShouldSetAccessControlPolicy()
        {
            string key = "test-object.txt";

            PutACLRequest request = new PutACLRequest
            {
                BucketName = _Bucket,
                Key = key,
                AccessControlList = new S3AccessControlList
                {
                    Owner = new Amazon.S3.Model.Owner { DisplayName = "admin", Id = "admin" },
                    Grants = new List<S3Grant>
                    {
                        new S3Grant
                        {
                            Grantee = new S3Grantee { CanonicalUser = "admin", DisplayName = "admin" },
                            Permission = S3Permission.FULL_CONTROL
                        }
                    }
                }
            };

            PutACLResponse response = await _Client.PutACLAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test object read ACL operation.
        /// </summary>
        [Fact]
        public async Task ObjectReadAcl_ShouldReturnAccessControlPolicy()
        {
            string key = "test-object.txt";

            GetACLRequest request = new GetACLRequest
            {
                BucketName = _Bucket,
                Key = key
            };

            GetACLResponse response = await _Client.GetACLAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
            Assert.NotNull(response.AccessControlList);
        }

        /// <summary>
        /// Test object write tags operation.
        /// </summary>
        [Fact]
        public async Task ObjectWriteTagging_ShouldSetTags()
        {
            string key = "test-object.txt";

            PutObjectTaggingRequest request = new PutObjectTaggingRequest
            {
                BucketName = _Bucket,
                Key = key,
                Tagging = new Amazon.S3.Model.Tagging
                {
                    TagSet = new List<Amazon.S3.Model.Tag>
                    {
                        new Amazon.S3.Model.Tag { Key = "Type", Value = "TestData" }
                    }
                }
            };

            PutObjectTaggingResponse response = await _Client.PutObjectTaggingAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test object read tags operation.
        /// </summary>
        [Fact]
        public async Task ObjectReadTagging_ShouldReturnTags()
        {
            string key = "test-object.txt";

            GetObjectTaggingRequest request = new GetObjectTaggingRequest
            {
                BucketName = _Bucket,
                Key = key
            };

            GetObjectTaggingResponse response = await _Client.GetObjectTaggingAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
            Assert.NotNull(response.Tagging);
        }

        /// <summary>
        /// Test object write retention operation.
        /// </summary>
        [Fact]
        public async Task ObjectWriteRetention_ShouldSetRetention()
        {
            string key = "test-object.txt";

            PutObjectRetentionRequest request = new PutObjectRetentionRequest
            {
                BucketName = _Bucket,
                Key = key,
                Retention = new ObjectLockRetention
                {
                    Mode = "GOVERNANCE",
                    RetainUntilDate = DateTime.UtcNow.AddDays(30)
                }
            };

            PutObjectRetentionResponse response = await _Client.PutObjectRetentionAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test object read retention operation.
        /// </summary>
        [Fact]
        public async Task ObjectReadRetention_ShouldReturnRetention()
        {
            string key = "test-object.txt";

            GetObjectRetentionRequest request = new GetObjectRetentionRequest
            {
                BucketName = _Bucket,
                Key = key
            };

            GetObjectRetentionResponse response = await _Client.GetObjectRetentionAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test object write legal hold operation.
        /// </summary>
        [Fact]
        public async Task ObjectWriteLegalHold_ShouldSetLegalHold()
        {
            string key = "test-object.txt";

            PutObjectLegalHoldRequest request = new PutObjectLegalHoldRequest
            {
                BucketName = _Bucket,
                Key = key,
                LegalHold = new ObjectLockLegalHold
                {
                    Status = "ON"
                }
            };

            PutObjectLegalHoldResponse response = await _Client.PutObjectLegalHoldAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test object read legal hold operation.
        /// </summary>
        [Fact]
        public async Task ObjectReadLegalHold_ShouldReturnLegalHold()
        {
            string key = "test-object.txt";

            GetObjectLegalHoldRequest request = new GetObjectLegalHoldRequest
            {
                BucketName = _Bucket,
                Key = key
            };

            GetObjectLegalHoldResponse response = await _Client.GetObjectLegalHoldAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test object delete multiple operation.
        /// </summary>
        [Fact]
        public async Task ObjectDeleteMultiple_ShouldDeleteObjects()
        {
            DeleteObjectsRequest request = new DeleteObjectsRequest
            {
                BucketName = _Bucket,
                Objects = new List<KeyVersion>
                {
                    new KeyVersion { Key = "test1.txt" },
                    new KeyVersion { Key = "test2.txt" }
                }
            };

            DeleteObjectsResponse response = await _Client.DeleteObjectsAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test object delete tags operation.
        /// </summary>
        [Fact]
        public async Task ObjectDeleteTagging_ShouldRemoveTags()
        {
            string key = "test-object.txt";

            DeleteObjectTaggingRequest request = new DeleteObjectTaggingRequest
            {
                BucketName = _Bucket,
                Key = key
            };

            DeleteObjectTaggingResponse response = await _Client.DeleteObjectTaggingAsync(request);

            Assert.NotNull(response);
            Assert.Equal(204, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test object delete operation.
        /// </summary>
        [Fact]
        public async Task ObjectDelete_ShouldRemoveObject()
        {
            string key = "delete-test-object.txt";

            DeleteObjectRequest request = new DeleteObjectRequest
            {
                BucketName = _Bucket,
                Key = key
            };

            DeleteObjectResponse response = await _Client.DeleteObjectAsync(request);

            Assert.NotNull(response);
            Assert.Equal(204, (int)response.HttpStatusCode);
        }

        #endregion

        #region Multipart-Upload-Tests

        /// <summary>
        /// Test initiate multipart upload operation.
        /// </summary>
        [Fact]
        public async Task MultipartUpload_InitiateShouldReturnUploadId()
        {
            string key = "multipart-test.txt";

            InitiateMultipartUploadRequest request = new InitiateMultipartUploadRequest
            {
                BucketName = _Bucket,
                Key = key,
                ContentType = "text/plain"
            };

            InitiateMultipartUploadResponse response = await _Client.InitiateMultipartUploadAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
            Assert.NotNull(response.UploadId);
        }

        /// <summary>
        /// Test upload part operation.
        /// </summary>
        [Fact]
        public async Task MultipartUpload_UploadPartShouldSucceed()
        {
            string key = "multipart-test.txt";
            string uploadId = "test-upload-id";

            UploadPartRequest request = new UploadPartRequest
            {
                BucketName = _Bucket,
                Key = key,
                UploadId = uploadId,
                PartNumber = 1,
                InputStream = new MemoryStream(Encoding.UTF8.GetBytes("Part 1 data"))
            };

            UploadPartResponse response = await _Client.UploadPartAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test list parts operation.
        /// </summary>
        [Fact]
        public async Task MultipartUpload_ListPartsShouldReturnParts()
        {
            string key = "multipart-test.txt";
            string uploadId = "test-upload-id";

            ListPartsRequest request = new ListPartsRequest
            {
                BucketName = _Bucket,
                Key = key,
                UploadId = uploadId
            };

            ListPartsResponse response = await _Client.ListPartsAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test complete multipart upload operation.
        /// </summary>
        [Fact]
        public async Task MultipartUpload_CompleteShouldFinishUpload()
        {
            string key = "multipart-test.txt";
            string uploadId = "test-upload-id";

            CompleteMultipartUploadRequest request = new CompleteMultipartUploadRequest
            {
                BucketName = _Bucket,
                Key = key,
                UploadId = uploadId,
                PartETags = new List<PartETag>
                {
                    new PartETag(1, "etag1")
                }
            };

            CompleteMultipartUploadResponse response = await _Client.CompleteMultipartUploadAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test abort multipart upload operation.
        /// </summary>
        [Fact]
        public async Task MultipartUpload_AbortShouldCancelUpload()
        {
            string key = "multipart-test.txt";
            string uploadId = "test-upload-id";

            AbortMultipartUploadRequest request = new AbortMultipartUploadRequest
            {
                BucketName = _Bucket,
                Key = key,
                UploadId = uploadId
            };

            AbortMultipartUploadResponse response = await _Client.AbortMultipartUploadAsync(request);

            Assert.NotNull(response);
            Assert.Equal(204, (int)response.HttpStatusCode);
        }

        /// <summary>
        /// Test list multipart uploads operation.
        /// </summary>
        [Fact]
        public async Task MultipartUpload_ListUploadsShouldReturnUploads()
        {
            ListMultipartUploadsRequest request = new ListMultipartUploadsRequest
            {
                BucketName = _Bucket
            };

            ListMultipartUploadsResponse response = await _Client.ListMultipartUploadsAsync(request);

            Assert.NotNull(response);
            Assert.Equal(200, (int)response.HttpStatusCode);
        }

        #endregion

        #region Private-Methods

        private void InitializeServer()
        {
            S3ServerSettings settings = new S3ServerSettings();
            settings.Webserver.Hostname = _Hostname;
            settings.Webserver.Port = _Port;
            settings.Webserver.Ssl.Enable = false;
            settings.Logging.HttpRequests = false;
            settings.Logging.S3Requests = false;

            _Server = new S3Server(settings);

            SetupServiceCallbacks();
            SetupBucketCallbacks();
            SetupObjectCallbacks();

            _Server.Start();
        }

        private void InitializeClient()
        {
            BasicAWSCredentials credentials = new BasicAWSCredentials(_AccessKey, _SecretKey);

            AmazonS3Config config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USWest1,
                ServiceURL = $"http://{_Hostname}:{_Port}/",
                ForcePathStyle = true,
                UseHttp = true
            };

            _Client = new AmazonS3Client(credentials, config);
        }

        private void SetupServiceCallbacks()
        {
            _Server.Service.ServiceExists = async (ctx) =>
            {
                return "us-west-1";
            };

            _Server.Service.ListBuckets = async (ctx) =>
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
            _Server.Bucket.Write = async (ctx) => { };
            _Server.Bucket.Exists = async (ctx) => true;
            _Server.Bucket.Delete = async (ctx) => { };

            _Server.Bucket.Read = async (ctx) =>
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

            _Server.Bucket.ReadAcl = async (ctx) =>
            {
                AccessControlList acl = new AccessControlList(new List<Grant>
                {
                    new Grant(_Grantee, PermissionEnum.FullControl)
                });
                return new AccessControlPolicy(_Owner, acl);
            };

            _Server.Bucket.WriteAcl = async (ctx, acp) => { };

            _Server.Bucket.ReadLocation = async (ctx) => new LocationConstraint("us-west-1");

            _Server.Bucket.ReadTagging = async (ctx) =>
            {
                return new S3ServerLibrary.S3Objects.Tagging(new TagSet(new List<S3ServerLibrary.S3Objects.Tag>
                {
                    new S3ServerLibrary.S3Objects.Tag("key", "value")
                }));
            };

            _Server.Bucket.WriteTagging = async (ctx, tags) => { };
            _Server.Bucket.DeleteTagging = async (ctx) => { };

            _Server.Bucket.ReadVersioning = async (ctx) =>
            {
                return new VersioningConfiguration(VersioningStatusEnum.Enabled, MfaDeleteStatusEnum.Disabled);
            };

            _Server.Bucket.WriteVersioning = async (ctx, ver) => { };

            _Server.Bucket.ReadVersions = async (ctx) =>
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

            _Server.Bucket.ReadLogging = async (ctx) =>
            {
                return new BucketLoggingStatus(new LoggingEnabled("default", "logs/", new TargetGrants()));
            };

            _Server.Bucket.WriteLogging = async (ctx, logging) => { };

            _Server.Bucket.ReadWebsite = async (ctx) =>
            {
                S3ServerLibrary.S3Objects.WebsiteConfiguration website = new S3ServerLibrary.S3Objects.WebsiteConfiguration();
                website.ErrorDocument = new ErrorDocument("error.html");
                website.IndexDocument = new IndexDocument("index.html");
                return website;
            };

            _Server.Bucket.WriteWebsite = async (ctx, website) => { };
            _Server.Bucket.DeleteWebsite = async (ctx) => { };

            _Server.Bucket.ReadMultipartUploads = async (ctx) =>
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
            _Server.Object.Write = async (ctx) => { };

            _Server.Object.Exists = async (ctx) =>
            {
                return new S3ServerLibrary.S3Objects.ObjectMetadata(
                    "hello.txt",
                    DateTime.UtcNow,
                    "6cd3556deb0da54bca060b4c39479839",
                    13,
                    _Owner);
            };

            _Server.Object.Read = async (ctx) =>
            {
                return new S3Object(
                    "hello.txt",
                    "1",
                    true,
                    DateTime.UtcNow,
                    "5d41402abc4b2a76b9719d911017c592",
                    5,
                    _Owner,
                    "hello",
                    "text/plain");
            };

            _Server.Object.ReadRange = async (ctx) =>
            {
                string data = "hello";
                string rangeData = data.Substring((int)ctx.Request.RangeStart, (int)((int)ctx.Request.RangeEnd - (int)ctx.Request.RangeStart + 1));

                return new S3Object(
                    "hello.txt",
                    "1",
                    true,
                    DateTime.UtcNow,
                    "5d41402abc4b2a76b9719d911017c592",
                    rangeData.Length,
                    _Owner,
                    rangeData,
                    "text/plain");
            };

            _Server.Object.ReadAcl = async (ctx) =>
            {
                AccessControlList acl = new AccessControlList(new List<Grant>
                {
                    new Grant(_Grantee, PermissionEnum.FullControl)
                });
                return new AccessControlPolicy(_Owner, acl);
            };

            _Server.Object.WriteAcl = async (ctx, acp) => { };

            _Server.Object.ReadTagging = async (ctx) =>
            {
                return new S3ServerLibrary.S3Objects.Tagging(new TagSet(new List<S3ServerLibrary.S3Objects.Tag>
                {
                    new S3ServerLibrary.S3Objects.Tag("key", "value")
                }));
            };

            _Server.Object.WriteTagging = async (ctx, tags) => { };
            _Server.Object.DeleteTagging = async (ctx) => { };

            _Server.Object.ReadRetention = async (ctx) =>
            {
                return new S3ServerLibrary.S3Objects.Retention(RetentionModeEnum.Governance, DateTime.UtcNow.AddDays(30));
            };

            _Server.Object.WriteRetention = async (ctx, retention) => { };

            _Server.Object.ReadLegalHold = async (ctx) =>
            {
                return new LegalHold("ON");
            };

            _Server.Object.WriteLegalHold = async (ctx, legalHold) => { };

            _Server.Object.Delete = async (ctx) => { };

            _Server.Object.DeleteMultiple = async (ctx, del) =>
            {
                List<Deleted> deleted = new List<Deleted>();
                foreach (S3ServerLibrary.S3Objects.Object obj in del.Objects)
                {
                    deleted.Add(new Deleted(obj.Key, "1", false));
                }
                return new DeleteResult(deleted, null);
            };

            _Server.Object.CreateMultipartUpload = async (ctx) =>
            {
                return new InitiateMultipartUploadResult(
                    ctx.Request.Bucket,
                    ctx.Request.Key,
                    "upload-id-123");
            };

            _Server.Object.UploadPart = async (ctx) => { };

            _Server.Object.ReadParts = async (ctx) =>
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

            _Server.Object.CompleteMultipartUpload = async (ctx, complete) =>
            {
                CompleteMultipartUploadResult result = new CompleteMultipartUploadResult();
                result.Location = "http://localhost/";
                result.Bucket = ctx.Request.Bucket;
                result.Key = ctx.Request.Key;
                result.ETag = "9b2c3e7a8d1f4e6b5c2a1d8f7e4b3c2a";
                return result;
            };

            _Server.Object.AbortMultipartUpload = async (ctx) => { };
        }

        #endregion

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning restore S31000 // Property value too short
    }
}
