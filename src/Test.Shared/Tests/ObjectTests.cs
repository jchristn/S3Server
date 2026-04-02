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
    using Amazon.S3;
    using Amazon.S3.Model;

    /// <summary>
    /// Object API tests (CRUD, ACL, tagging, retention, legal hold, range reads).
    /// </summary>
    public static class ObjectTests
    {
        /// <summary>
        /// Run all object API tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            Console.WriteLine();
            Console.WriteLine("--- Object API Tests ---");

            await runner.RunTestAsync("ObjectWrite uploads object", async (ct) =>
            {
                byte[] data = Encoding.UTF8.GetBytes("Hello, S3Server!");
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "test-object.txt",
                    InputStream = new MemoryStream(data),
                    ContentType = "text/plain"
                };
                PutObjectResponse response = await server.S3Client.PutObjectAsync(request, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutObject");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectExists returns metadata", async (ct) =>
            {
                GetObjectMetadataResponse response = await server.S3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = server.Bucket,
                    Key = "test-object.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "HeadObject");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectRead downloads object", async (ct) =>
            {
                GetObjectResponse response = await server.S3Client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "test-object.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "GetObject");
                AssertHelper.IsTrue(response.ContentLength > 0, "content length > 0");

                using (StreamReader reader = new StreamReader(response.ResponseStream))
                {
                    string content = await reader.ReadToEndAsync().ConfigureAwait(false);
                    AssertHelper.IsTrue(content.Length > 0, "content is not empty");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectReadRange downloads partial object", async (ct) =>
            {
                GetObjectResponse response = await server.S3Client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "test-object.txt",
                    ByteRange = new ByteRange(0, 4)
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "GetObject range");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectWriteAcl sets ACL", async (ct) =>
            {
                PutACLRequest request = new PutACLRequest
                {
                    BucketName = server.Bucket,
                    Key = "test-object.txt",
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
                PutACLResponse response = await server.S3Client.PutACLAsync(request, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutObjectACL");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectReadAcl returns ACL", async (ct) =>
            {
                GetACLResponse response = await server.S3Client.GetACLAsync(new GetACLRequest
                {
                    BucketName = server.Bucket,
                    Key = "test-object.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "GetObjectACL");
                AssertHelper.IsNotNull(response.AccessControlList, "ACL");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectWriteTagging sets tags", async (ct) =>
            {
                PutObjectTaggingRequest request = new PutObjectTaggingRequest
                {
                    BucketName = server.Bucket,
                    Key = "test-object.txt",
                    Tagging = new Amazon.S3.Model.Tagging
                    {
                        TagSet = new List<Amazon.S3.Model.Tag>
                        {
                            new Amazon.S3.Model.Tag { Key = "Type", Value = "TestData" }
                        }
                    }
                };
                PutObjectTaggingResponse response = await server.S3Client.PutObjectTaggingAsync(request, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutObjectTagging");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectReadTagging returns tags", async (ct) =>
            {
                GetObjectTaggingResponse response = await server.S3Client.GetObjectTaggingAsync(new GetObjectTaggingRequest
                {
                    BucketName = server.Bucket,
                    Key = "test-object.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "GetObjectTagging");
                AssertHelper.IsNotNull(response.Tagging, "Tagging");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectWriteRetention sets retention", async (ct) =>
            {
                PutObjectRetentionRequest request = new PutObjectRetentionRequest
                {
                    BucketName = server.Bucket,
                    Key = "test-object.txt",
                    Retention = new ObjectLockRetention
                    {
                        Mode = "GOVERNANCE",
                        RetainUntilDate = DateTime.UtcNow.AddDays(30)
                    }
                };
                PutObjectRetentionResponse response = await server.S3Client.PutObjectRetentionAsync(request, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutObjectRetention");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectReadRetention returns retention", async (ct) =>
            {
                GetObjectRetentionResponse response = await server.S3Client.GetObjectRetentionAsync(new GetObjectRetentionRequest
                {
                    BucketName = server.Bucket,
                    Key = "test-object.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "GetObjectRetention");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectWriteLegalHold sets legal hold", async (ct) =>
            {
                PutObjectLegalHoldRequest request = new PutObjectLegalHoldRequest
                {
                    BucketName = server.Bucket,
                    Key = "test-object.txt",
                    LegalHold = new ObjectLockLegalHold { Status = "ON" }
                };
                PutObjectLegalHoldResponse response = await server.S3Client.PutObjectLegalHoldAsync(request, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutObjectLegalHold");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectReadLegalHold returns legal hold", async (ct) =>
            {
                GetObjectLegalHoldResponse response = await server.S3Client.GetObjectLegalHoldAsync(new GetObjectLegalHoldRequest
                {
                    BucketName = server.Bucket,
                    Key = "test-object.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "GetObjectLegalHold");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectDeleteMultiple deletes objects", async (ct) =>
            {
                DeleteObjectsRequest request = new DeleteObjectsRequest
                {
                    BucketName = server.Bucket,
                    Objects = new List<KeyVersion>
                    {
                        new KeyVersion { Key = "test1.txt" },
                        new KeyVersion { Key = "test2.txt" }
                    }
                };
                DeleteObjectsResponse response = await server.S3Client.DeleteObjectsAsync(request, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "DeleteObjects");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectDeleteTagging removes tags", async (ct) =>
            {
                DeleteObjectTaggingResponse response = await server.S3Client.DeleteObjectTaggingAsync(new DeleteObjectTaggingRequest
                {
                    BucketName = server.Bucket,
                    Key = "test-object.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(204, (int)response.HttpStatusCode, "DeleteObjectTagging");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectDelete removes object", async (ct) =>
            {
                DeleteObjectResponse response = await server.S3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "delete-test-object.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(204, (int)response.HttpStatusCode, "DeleteObject");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectWrite exceeding max size returns EntityTooLarge", async (ct) =>
            {
                string url = $"{server.BaseUrl}/{server.Bucket}/oversized-object.bin";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);
                byte[] data = new byte[2048];
                request.Content = new ByteArrayContent(data);
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                try
                {
                    HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.BadRequest, response, "EntityTooLarge");
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    AssertHelper.StringContains(body, "EntityTooLarge", "error body");
                }
                catch (HttpRequestException)
                {
                    // TCP mode may reset connection for oversized requests
                }
            }, token).ConfigureAwait(false);
        }
    }
}
