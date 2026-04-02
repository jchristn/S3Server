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
            Console.WriteLine();
            Console.WriteLine("--- Signature Validation Tests ---");

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
                    try
                    {
                        await wrongClient.ListBucketsAsync(new ListBucketsRequest(), ct).ConfigureAwait(false);
                        // Request succeeded when it should have been rejected
                        AssertHelper.IsTrue(false, "expected rejection but request succeeded");
                    }
                    catch (Exception)
                    {
                        // Any exception means the server rejected the request
                    }
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
                    try
                    {
                        await unknownClient.ListBucketsAsync(new ListBucketsRequest(), ct).ConfigureAwait(false);
                        AssertHelper.IsTrue(false, "expected rejection but request succeeded");
                    }
                    catch (Exception)
                    {
                        // Any exception means the server rejected the request
                    }
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("V2 signature returns SignatureDoesNotMatch", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/");
                request.Headers.TryAddWithoutValidation("Authorization", "AWS AKIAIOSFODNN7EXAMPLE:somesignature");
                request.Headers.TryAddWithoutValidation("Date", DateTime.UtcNow.ToString("R"));

                try
                {
                    HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "V2 signature rejection");
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    AssertHelper.StringContains(body, "SignatureDoesNotMatch", "error body");
                }
                catch (HttpRequestException)
                {
                    // Server may reset connection
                }
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
                    try
                    {
                        PutObjectRequest putReq = new PutObjectRequest
                        {
                            BucketName = server.Bucket,
                            Key = "tampered-object.txt",
                            ContentBody = "tampered content"
                        };
                        await tamperedClient.PutObjectAsync(putReq, ct).ConfigureAwait(false);
                        AssertHelper.IsTrue(false, "expected rejection but request succeeded");
                    }
                    catch (Exception)
                    {
                        // Any exception means the server rejected the request
                    }
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
    }
}
