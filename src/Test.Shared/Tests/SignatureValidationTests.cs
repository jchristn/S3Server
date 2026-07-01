namespace Test.Shared.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.S3.Model;
    using S3ServerLibrary;
    using S3ServerLibrary.S3Objects;

    /// <summary>
    /// Signature validation tests.
    /// These tests run against a server instance with EnableSignatures = true.
    /// Tests use the AWSSDK 4.x S3 client which sends streaming V4 signatures.
    /// </summary>
    public static class SignatureValidationTests
    {
        /// <summary>
        /// Run all signature validation tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server with signatures enabled.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            await runner.RunTestAsync("Valid signature allows ListBuckets", async (ct) =>
            {
                ListBucketsResponse response = await server.S3Client.ListBucketsAsync(new ListBucketsRequest(), ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "ListBuckets");
                AssertHelper.IsNotNull(response.Buckets, "buckets");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid signature allows PutObject", async (ct) =>
            {
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "sig-test.txt",
                    ContentBody = "signature test content",
                    ContentType = "text/plain"
                };
                PutObjectResponse response = await server.S3Client.PutObjectAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutObject");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid signature allows GetObject", async (ct) =>
            {
                GetObjectResponse response = await server.S3Client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "sig-test.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "GetObject");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid signature allows HeadObject", async (ct) =>
            {
                GetObjectMetadataResponse response = await server.S3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = server.Bucket,
                    Key = "sig-test.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "HeadObject");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid signature allows RestoreObject", async (ct) =>
            {
                RestoreObjectResponse response = await server.S3Client.RestoreObjectAsync(new RestoreObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "archived-object.txt",
                    Days = 2
                }, ct).ConfigureAwait(false);

                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(202, (int)response.HttpStatusCode, "RestoreObject");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid signature allows DeleteObject", async (ct) =>
            {
                DeleteObjectResponse response = await server.S3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "sig-test.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(204, (int)response.HttpStatusCode, "DeleteObject");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid signature allows GetBucketACL", async (ct) =>
            {
                GetACLResponse response = await server.S3Client.GetACLAsync(new GetACLRequest
                {
                    BucketName = server.Bucket
                }, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "GetBucketACL");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid signature allows PutBucketTagging", async (ct) =>
            {
                PutBucketTaggingRequest request = new PutBucketTaggingRequest
                {
                    BucketName = server.Bucket,
                    TagSet = new List<Amazon.S3.Model.Tag>
                    {
                        new Amazon.S3.Model.Tag { Key = "SignTest", Value = "Valid" }
                    }
                };
                PutBucketTaggingResponse response = await server.S3Client.PutBucketTaggingAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutBucketTagging");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid signature allows InitiateMultipartUpload", async (ct) =>
            {
                InitiateMultipartUploadResponse response = await server.S3Client.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest
                {
                    BucketName = server.Bucket,
                    Key = "sig-multipart-test.txt",
                    ContentType = "text/plain"
                }, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "InitiateMultipartUpload");
                AssertHelper.IsNotNull(response.UploadId, "UploadId");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Wrong secret key is rejected", async (ct) =>
            {
                BasicAWSCredentials wrongCreds = new BasicAWSCredentials(server.AccessKey, "WRONG_SECRET_KEY_FOR_TESTING_1234567");
                AmazonS3Config config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.USWest1,
                    ServiceURL = server.BaseUrl + "/",
                    ForcePathStyle = true,
                    UseHttp = true,
                    MaxErrorRetry = 0,
                    Timeout = TimeSpan.FromSeconds(5)
                };

                using (IAmazonS3 wrongClient = new AmazonS3Client(wrongCreds, config))
                {
                    AmazonS3Exception s3e = await CaptureAmazonS3Exception(async () =>
                    {
                        await wrongClient.ListBucketsAsync(new ListBucketsRequest(), ct).ConfigureAwait(false);
                    }).ConfigureAwait(false);

                    AssertHelper.AreEqual(HttpStatusCode.Forbidden, s3e.StatusCode, "wrong secret status");
                    AssertHelper.AreEqual("SignatureDoesNotMatch", s3e.ErrorCode, "wrong secret error code");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Unknown access key is rejected", async (ct) =>
            {
                BasicAWSCredentials unknownCreds = new BasicAWSCredentials("AKIAUNKNOWNKEYEXAMPLE", "SomeRandomSecretKeyForTestPurposes123");
                AmazonS3Config config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.USWest1,
                    ServiceURL = server.BaseUrl + "/",
                    ForcePathStyle = true,
                    UseHttp = true,
                    MaxErrorRetry = 0,
                    Timeout = TimeSpan.FromSeconds(5)
                };

                using (IAmazonS3 unknownClient = new AmazonS3Client(unknownCreds, config))
                {
                    AmazonS3Exception s3e = await CaptureAmazonS3Exception(async () =>
                    {
                        await unknownClient.ListBucketsAsync(new ListBucketsRequest(), ct).ConfigureAwait(false);
                    }).ConfigureAwait(false);

                    AssertHelper.AreEqual(HttpStatusCode.Forbidden, s3e.StatusCode, "unknown key status");
                    AssertHelper.AreEqual("AccessDenied", s3e.ErrorCode, "unknown key error code");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Unsigned request is rejected when signatures are enabled", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "unsigned request rejection");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "AccessDenied", "unsigned request error body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Unsigned anonymous object read is allowed by callback", async (ct) =>
            {
                int port = S3TestServer.GetAvailablePort();
                bool anonymousCallbackCalled = false;
                bool secretLookupCalled = false;
                bool objectReadCalled = false;

                S3ServerSettings settings = new S3ServerSettings();
                settings.Webserver.Hostname = "127.0.0.1";
                settings.Webserver.Port = port;
                settings.Webserver.Ssl.Enable = false;
                settings.EnableSignatures = true;

                using (S3Server authServer = new S3Server(settings))
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.ConnectionClose = true;

                    authServer.Service.GetSecretKey = (ctx) =>
                    {
                        secretLookupCalled = true;
                        return server.SecretKey;
                    };
                    authServer.Service.IsAnonymousRequestAllowed = async (ctx) =>
                    {
                        anonymousCallbackCalled = true;
                        AssertHelper.AreEqual(S3PermissionType.ObjectRead, ctx.Request.PermissionsRequired, "anonymous read permission");
                        return true;
                    };
                    authServer.Object.Read = async (ctx) =>
                    {
                        objectReadCalled = true;
                        return new S3ServerLibrary.S3Object(
                            ctx.Request.Key,
                            "1",
                            true,
                            DateTime.UtcNow,
                            "etag-public-read",
                            5,
                            new S3ServerLibrary.S3Objects.Owner("admin", "Administrator"),
                            "hello",
                            "text/plain");
                    };

                    authServer.Start();

                    HttpResponseMessage response = await client.GetAsync("http://127.0.0.1:" + port + "/" + server.Bucket + "/public-read.txt", ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "anonymous object read");
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    AssertHelper.AreEqual("hello", body, "anonymous object body");
                    AssertHelper.IsTrue(anonymousCallbackCalled, "anonymous callback was called");
                    AssertHelper.IsFalse(secretLookupCalled, "secret lookup was not called for anonymous request");
                    AssertHelper.IsTrue(objectReadCalled, "object read callback was called");

                    authServer.Stop();
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Unsigned anonymous object write is allowed by callback", async (ct) =>
            {
                int port = S3TestServer.GetAvailablePort();
                bool anonymousCallbackCalled = false;
                bool secretLookupCalled = false;
                bool objectWriteCalled = false;

                S3ServerSettings settings = new S3ServerSettings();
                settings.Webserver.Hostname = "127.0.0.1";
                settings.Webserver.Port = port;
                settings.Webserver.Ssl.Enable = false;
                settings.EnableSignatures = true;

                using (S3Server authServer = new S3Server(settings))
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.ConnectionClose = true;

                    authServer.Service.GetSecretKey = (ctx) =>
                    {
                        secretLookupCalled = true;
                        return server.SecretKey;
                    };
                    authServer.Service.IsAnonymousRequestAllowed = async (ctx) =>
                    {
                        anonymousCallbackCalled = true;
                        AssertHelper.AreEqual(S3PermissionType.BucketWrite, ctx.Request.PermissionsRequired, "anonymous write permission");
                        return true;
                    };
                    authServer.Object.Write = async (ctx) =>
                    {
                        objectWriteCalled = true;
                    };

                    authServer.Start();

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "http://127.0.0.1:" + port + "/" + server.Bucket + "/public-write.txt");
                    request.Content = new StringContent("hello", Encoding.UTF8, "text/plain");
                    HttpResponseMessage response = await client.SendAsync(request, ct).ConfigureAwait(false);

                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "anonymous object write");
                    AssertHelper.IsTrue(anonymousCallbackCalled, "anonymous callback was called");
                    AssertHelper.IsFalse(secretLookupCalled, "secret lookup was not called for anonymous request");
                    AssertHelper.IsTrue(objectWriteCalled, "object write callback was called");

                    authServer.Stop();
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Unsigned anonymous request is rejected when callback denies", async (ct) =>
            {
                int port = S3TestServer.GetAvailablePort();
                bool anonymousCallbackCalled = false;
                bool objectReadCalled = false;

                S3ServerSettings settings = new S3ServerSettings();
                settings.Webserver.Hostname = "127.0.0.1";
                settings.Webserver.Port = port;
                settings.Webserver.Ssl.Enable = false;
                settings.EnableSignatures = true;

                using (S3Server authServer = new S3Server(settings))
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.ConnectionClose = true;

                    authServer.Service.GetSecretKey = (ctx) => server.SecretKey;
                    authServer.Service.IsAnonymousRequestAllowed = async (ctx) =>
                    {
                        anonymousCallbackCalled = true;
                        return false;
                    };
                    authServer.Object.Read = async (ctx) =>
                    {
                        objectReadCalled = true;
                        return new S3ServerLibrary.S3Object();
                    };

                    authServer.Start();

                    HttpResponseMessage response = await client.GetAsync("http://127.0.0.1:" + port + "/" + server.Bucket + "/private.txt", ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "denied anonymous request");
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    AssertHelper.StringContains(body, "AccessDenied", "denied anonymous error body");
                    AssertHelper.IsTrue(anonymousCallbackCalled, "anonymous callback was called");
                    AssertHelper.IsFalse(objectReadCalled, "object read callback was not called");

                    authServer.Stop();
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Invalid signed request does not use anonymous callback", async (ct) =>
            {
                int port = S3TestServer.GetAvailablePort();
                bool anonymousCallbackCalled = false;
                bool secretLookupCalled = false;
                bool objectReadCalled = false;

                S3ServerSettings settings = new S3ServerSettings();
                settings.Webserver.Hostname = "127.0.0.1";
                settings.Webserver.Port = port;
                settings.Webserver.Ssl.Enable = false;
                settings.EnableSignatures = true;

                using (S3Server authServer = new S3Server(settings))
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.ConnectionClose = true;

                    authServer.Service.GetSecretKey = (ctx) =>
                    {
                        secretLookupCalled = true;
                        return server.SecretKey;
                    };
                    authServer.Service.IsAnonymousRequestAllowed = async (ctx) =>
                    {
                        anonymousCallbackCalled = true;
                        return true;
                    };
                    authServer.Object.Read = async (ctx) =>
                    {
                        objectReadCalled = true;
                        return new S3ServerLibrary.S3Object();
                    };

                    authServer.Start();

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://127.0.0.1:" + port + "/" + server.Bucket + "/signed-public.txt");
                    request.Headers.TryAddWithoutValidation("Authorization", "AWS4-HMAC-SHA256 Credential=" + server.AccessKey + "/20260101/us-west-1/s3/aws4_request, SignedHeaders=host;x-amz-date, Signature=bad-signature");
                    request.Headers.TryAddWithoutValidation("x-amz-date", "20260101T000000Z");

                    HttpResponseMessage response = await client.SendAsync(request, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "invalid signed request");
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    AssertHelper.StringContains(body, "SignatureDoesNotMatch", "invalid signed request error body");
                    AssertHelper.IsFalse(anonymousCallbackCalled, "anonymous callback was not called");
                    AssertHelper.IsTrue(secretLookupCalled, "secret lookup was called for signed request");
                    AssertHelper.IsFalse(objectReadCalled, "object read callback was not called");

                    authServer.Stop();
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("V4 presigned URL material does not use anonymous callback", async (ct) =>
            {
                int port = S3TestServer.GetAvailablePort();
                bool anonymousCallbackCalled = false;
                bool objectReadCalled = false;

                S3ServerSettings settings = new S3ServerSettings();
                settings.Webserver.Hostname = "127.0.0.1";
                settings.Webserver.Port = port;
                settings.Webserver.Ssl.Enable = false;
                settings.EnableSignatures = true;

                using (S3Server authServer = new S3Server(settings))
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.ConnectionClose = true;

                    authServer.Service.GetSecretKey = (ctx) => server.SecretKey;
                    authServer.Service.IsAnonymousRequestAllowed = async (ctx) =>
                    {
                        anonymousCallbackCalled = true;
                        return true;
                    };
                    authServer.Object.Read = async (ctx) =>
                    {
                        objectReadCalled = true;
                        return new S3ServerLibrary.S3Object();
                    };

                    authServer.Start();

                    string url = "http://127.0.0.1:" + port + "/" + server.Bucket + "/presigned-public.txt"
                        + "?X-Amz-Algorithm=AWS4-HMAC-SHA256"
                        + "&X-Amz-Credential=" + WebUtility.UrlEncode(server.AccessKey + "/20260101/us-west-1/s3/aws4_request")
                        + "&X-Amz-Date=20260101T000000Z"
                        + "&X-Amz-Expires=60"
                        + "&X-Amz-SignedHeaders=host"
                        + "&X-Amz-Signature=bad";

                    HttpResponseMessage response = await client.GetAsync(url, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "V4 presigned query rejection");
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    AssertHelper.StringContains(body, "AccessDenied", "V4 presigned query error body");
                    AssertHelper.IsFalse(anonymousCallbackCalled, "anonymous callback was not called");
                    AssertHelper.IsFalse(objectReadCalled, "object read callback was not called");

                    authServer.Stop();
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Rejected unsigned request does not invoke operation callbacks", async (ct) =>
            {
                int port = S3TestServer.GetAvailablePort();
                bool listBucketsCalled = false;

                S3ServerSettings settings = new S3ServerSettings();
                settings.Webserver.Hostname = "127.0.0.1";
                settings.Webserver.Port = port;
                settings.Webserver.Ssl.Enable = false;
                settings.EnableSignatures = true;

                using (S3Server authServer = new S3Server(settings))
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.ConnectionClose = true;

                    authServer.Service.GetSecretKey = (ctx) => server.SecretKey;
                    authServer.Service.ListBuckets = async (ctx) =>
                    {
                        listBucketsCalled = true;
                        return new ListAllMyBucketsResult();
                    };

                    authServer.Start();

                    HttpResponseMessage response = await client.GetAsync("http://127.0.0.1:" + port + "/", ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "unsigned request rejection");
                    AssertHelper.IsFalse(listBucketsCalled, "ListBuckets callback was not invoked");

                    authServer.Stop();
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("V2 signature returns SignatureDoesNotMatch", async (ct) =>
            {
                server.ClearObservedRequests();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/");
                request.Headers.TryAddWithoutValidation("Authorization", "AWS AKIAIOSFODNN7EXAMPLE:somesignature");
                request.Headers.TryAddWithoutValidation("Date", DateTime.UtcNow.ToString("R"));

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "V2 signature rejection");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "SignatureDoesNotMatch", "error body");

                AssertHelper.IsNotNull(server.LastObservedRequest, "observed request");
                AssertHelper.AreEqual(S3SignatureVersion.Version2, server.LastObservedRequest.SignatureVersion, "signature version");
                AssertHelper.AreEqual(server.AccessKey, server.LastObservedRequest.AccessKey, "access key");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("V2 signed URL query parameters are rejected while unsupported", async (ct) =>
            {
                server.ClearObservedRequests();
                string expires = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds().ToString();
                string url = server.BaseUrl
                    + "/"
                    + server.Bucket
                    + "/hello.html?AWSAccessKeyId="
                    + WebUtility.UrlEncode(server.AccessKey)
                    + "&Expires="
                    + expires
                    + "&Signature="
                    + WebUtility.UrlEncode("is78k1u4bSulJPWbGYpzeFp6puo=");

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "V2 signed URL rejection");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "SignatureDoesNotMatch", "V2 signed URL error body");

                AssertHelper.IsNotNull(server.LastObservedRequest, "observed request");
                AssertHelper.AreEqual(server.AccessKey, server.LastObservedRequest.AccessKey, "access key");
                AssertHelper.AreEqual(expires, server.LastObservedRequest.Expires, "expires");
                AssertHelper.AreEqual("is78k1u4bSulJPWbGYpzeFp6puo=", server.LastObservedRequest.Signature, "signature");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Tampered credentials are rejected", async (ct) =>
            {
                BasicAWSCredentials tamperedCreds = new BasicAWSCredentials(server.AccessKey, "TamperedSecretKeyThatWillNotMatch123");
                AmazonS3Config config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.USWest1,
                    ServiceURL = server.BaseUrl + "/",
                    ForcePathStyle = true,
                    UseHttp = true,
                    MaxErrorRetry = 0,
                    Timeout = TimeSpan.FromSeconds(5)
                };

                using (IAmazonS3 tamperedClient = new AmazonS3Client(tamperedCreds, config))
                {
                    PutObjectRequest putReq = new PutObjectRequest
                    {
                        BucketName = server.Bucket,
                        Key = "tampered-object.txt",
                        ContentBody = "tampered content"
                    };

                    AmazonS3Exception s3e = await CaptureAmazonS3Exception(async () =>
                    {
                        await tamperedClient.PutObjectAsync(putReq, ct).ConfigureAwait(false);
                    }).ConfigureAwait(false);

                    AssertHelper.AreEqual(HttpStatusCode.Forbidden, s3e.StatusCode, "tampered credentials status");
                    AssertHelper.IsTrue(
                        s3e.ErrorCode == "SignatureDoesNotMatch" || s3e.ErrorCode == "Forbidden",
                        "tampered credentials error code is SignatureDoesNotMatch or SDK-level Forbidden");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid signature with binary upload", async (ct) =>
            {
                byte[] data = new byte[256];
                new Random(42).NextBytes(data);
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "sig-binary.bin",
                    InputStream = new MemoryStream(data),
                    ContentType = "application/octet-stream"
                };
                PutObjectResponse response = await server.S3Client.PutObjectAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutObject binary with signature");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid signature with empty body upload", async (ct) =>
            {
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "sig-empty.txt",
                    InputStream = new MemoryStream(Array.Empty<byte>()),
                    ContentType = "text/plain"
                };
                PutObjectResponse response = await server.S3Client.PutObjectAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutObject empty with signature");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Valid signature allows DeleteMultiple", async (ct) =>
            {
                DeleteObjectsRequest request = new DeleteObjectsRequest
                {
                    BucketName = server.Bucket,
                    Objects = new List<KeyVersion>
                    {
                        new KeyVersion { Key = "del1.txt" },
                        new KeyVersion { Key = "del2.txt" }
                    }
                };
                DeleteObjectsResponse response = await server.S3Client.DeleteObjectsAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "DeleteObjects with signature");
            }, token).ConfigureAwait(false);
        }

        private static async Task<AmazonS3Exception> CaptureAmazonS3Exception(Func<Task> action)
        {
            try
            {
                await action().ConfigureAwait(false);
            }
            catch (AmazonS3Exception s3e)
            {
                return s3e;
            }

            throw new Exception("Expected AmazonS3Exception.");
        }
    }
}
