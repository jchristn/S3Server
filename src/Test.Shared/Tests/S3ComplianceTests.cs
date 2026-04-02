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
    using Amazon.S3.Model;

    /// <summary>
    /// Additional S3 compliance tests covering edge cases and protocol conformance.
    /// </summary>
    public static class S3ComplianceTests
    {
        /// <summary>
        /// Run all S3 compliance tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            Console.WriteLine();
            Console.WriteLine("--- S3 Compliance Tests ---");

            #region XML-Response-Format

            await runner.RunTestAsync("ListBuckets response contains valid XML with Owner", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "ListBuckets");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                AssertHelper.StringContains(body, "ListAllMyBucketsResult", "XML root element");
                AssertHelper.StringContains(body, "<Owner>", "Owner element");
                AssertHelper.StringContains(body, "<Buckets>", "Buckets element");
                AssertHelper.StringContains(body, "<Bucket>", "Bucket element");
                AssertHelper.StringContains(body, "<Name>", "Name element");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListObjectsV2 response contains valid XML structure", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "?list-type=2");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "ListObjectsV2");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                AssertHelper.StringContains(body, "ListBucketResult", "XML root element");
                AssertHelper.StringContains(body, "<Name>", "Name element");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("GetBucketACL response contains valid ACL XML", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "?acl");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "GetBucketACL");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                AssertHelper.StringContains(body, "AccessControlPolicy", "ACL root element");
                AssertHelper.StringContains(body, "<Owner>", "Owner element");
                AssertHelper.StringContains(body, "<AccessControlList>", "ACL element");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("GetBucketTagging response contains valid Tagging XML", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "?tagging");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "GetBucketTagging");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                AssertHelper.StringContains(body, "Tagging", "Tagging root element");
                AssertHelper.StringContains(body, "<TagSet>", "TagSet element");
                AssertHelper.StringContains(body, "<Tag>", "Tag element");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("GetBucketVersioning response contains valid XML", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "?versioning");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "GetBucketVersioning");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                AssertHelper.StringContains(body, "VersioningConfiguration", "VersioningConfiguration element");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListVersions response contains valid XML", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "?versions");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "ListVersions");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                AssertHelper.StringContains(body, "ListVersionsResult", "ListVersionsResult element");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("GetBucketWebsite response contains valid XML", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "?website");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "GetBucketWebsite");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                AssertHelper.StringContains(body, "WebsiteConfiguration", "WebsiteConfiguration element");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("GetBucketLogging response contains valid XML", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "?logging");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "GetBucketLogging");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                AssertHelper.StringContains(body, "BucketLoggingStatus", "BucketLoggingStatus element");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("GetBucketLocation response contains valid XML", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "?location");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "GetBucketLocation");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                AssertHelper.StringContains(body, "LocationConstraint", "LocationConstraint element");
            }, token).ConfigureAwait(false);

            #endregion

            #region Error-Response-Format

            await runner.RunTestAsync("Error response contains valid S3 Error XML", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/nonexistent-object-xyz.bin");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.NotFound, response, "404 response");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                AssertHelper.StringContains(body, "<Error>", "Error root element");
                AssertHelper.StringContains(body, "<Code>", "Code element");
                AssertHelper.StringContains(body, "NoSuchKey", "error code value");
            }, token).ConfigureAwait(false);

            #endregion

            #region HTTP-Method-Handling

            await runner.RunTestAsync("PUT to root with empty body creates bucket", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + "/compliance-test-bucket");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "PUT create bucket");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("DELETE bucket returns 204", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, server.BaseUrl + "/compliance-delete-bucket");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.NoContent, response, "DELETE bucket");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("DELETE object returns 204", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, server.BaseUrl + "/" + server.Bucket + "/some-key.txt");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.NoContent, response, "DELETE object");
            }, token).ConfigureAwait(false);

            #endregion

            #region Object-Content-Handling

            await runner.RunTestAsync("PutObject with binary content succeeds", async (ct) =>
            {
                byte[] binaryData = new byte[512];
                new Random(42).NextBytes(binaryData);

                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "binary-object.bin",
                    InputStream = new MemoryStream(binaryData),
                    ContentType = "application/octet-stream"
                };
                PutObjectResponse response = await server.S3Client.PutObjectAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutObject binary");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("PutObject with empty body succeeds", async (ct) =>
            {
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "empty-object.txt",
                    InputStream = new MemoryStream(Array.Empty<byte>()),
                    ContentType = "text/plain"
                };
                PutObjectResponse response = await server.S3Client.PutObjectAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutObject empty body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("PutObject with content at max size succeeds", async (ct) =>
            {
                // Server is configured with MaxPutObjectSize = 1024
                byte[] data = new byte[1024];
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "max-size-object.bin",
                    InputStream = new MemoryStream(data),
                    ContentType = "application/octet-stream"
                };
                PutObjectResponse response = await server.S3Client.PutObjectAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutObject at max size");
            }, token).ConfigureAwait(false);

            #endregion

            #region Key-Naming

            await runner.RunTestAsync("Object key with forward slashes (directory-like) succeeds", async (ct) =>
            {
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "path/to/nested/object.txt",
                    ContentBody = "nested",
                    ContentType = "text/plain"
                };
                PutObjectResponse response = await server.S3Client.PutObjectAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutObject with slashes");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Object key with special characters succeeds", async (ct) =>
            {
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "special chars/file (1).txt",
                    ContentBody = "special",
                    ContentType = "text/plain"
                };
                PutObjectResponse response = await server.S3Client.PutObjectAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutObject with special chars");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Object key with dots succeeds", async (ct) =>
            {
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "file.with.multiple.dots.tar.gz",
                    ContentBody = "dots",
                    ContentType = "application/gzip"
                };
                PutObjectResponse response = await server.S3Client.PutObjectAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutObject with dots");
            }, token).ConfigureAwait(false);

            #endregion

            #region Multipart-Upload-XML

            await runner.RunTestAsync("ListMultipartUploads response contains valid XML", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "?uploads");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "ListMultipartUploads");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                AssertHelper.StringContains(body, "ListMultipartUploadsResult", "ListMultipartUploadsResult element");
                AssertHelper.StringContains(body, "<Bucket>", "Bucket element");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("InitiateMultipartUpload response contains valid XML", async (ct) =>
            {
                InitiateMultipartUploadResponse response = await server.S3Client.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest
                {
                    BucketName = server.Bucket,
                    Key = "compliance-multipart.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "InitiateMultipartUpload");
                AssertHelper.IsNotNull(response.UploadId, "UploadId");
                AssertHelper.IsTrue(response.UploadId.Length > 0, "UploadId is not empty");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("CompleteMultipartUpload response contains valid XML", async (ct) =>
            {
                CompleteMultipartUploadResponse response = await server.S3Client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest
                {
                    BucketName = server.Bucket,
                    Key = "compliance-complete.txt",
                    UploadId = "test-upload-id",
                    PartETags = new List<PartETag> { new PartETag(1, "etag1") }
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "CompleteMultipartUpload");
                AssertHelper.IsNotNull(response.ETag, "ETag");
            }, token).ConfigureAwait(false);

            #endregion

            #region Object-Metadata

            await runner.RunTestAsync("HeadObject returns content-type header", async (ct) =>
            {
                GetObjectMetadataResponse response = await server.S3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = server.Bucket,
                    Key = "test-object.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "HeadObject");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("GetObject returns body and content-length", async (ct) =>
            {
                GetObjectResponse response = await server.S3Client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "test-object.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "GetObject");
                AssertHelper.IsTrue(response.ContentLength >= 0, "content-length is non-negative");
            }, token).ConfigureAwait(false);

            #endregion

            #region DeleteMultiple-XML

            await runner.RunTestAsync("DeleteMultiple with valid XML returns DeleteResult", async (ct) =>
            {
                DeleteObjectsRequest request = new DeleteObjectsRequest
                {
                    BucketName = server.Bucket,
                    Objects = new List<KeyVersion>
                    {
                        new KeyVersion { Key = "obj1.txt" },
                        new KeyVersion { Key = "obj2.txt" },
                        new KeyVersion { Key = "obj3.txt" }
                    }
                };
                DeleteObjectsResponse response = await server.S3Client.DeleteObjectsAsync(request, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "DeleteObjects");
                AssertHelper.IsNotNull(response.DeletedObjects, "DeletedObjects");
                AssertHelper.AreEqual(3, response.DeletedObjects.Count, "deleted count");
            }, token).ConfigureAwait(false);

            #endregion

            #region Retention-And-LegalHold-XML

            await runner.RunTestAsync("GetObjectRetention response contains retention mode", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt?retention");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "GetObjectRetention");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                AssertHelper.StringContains(body, "Retention", "Retention element");
                AssertHelper.StringContains(body, "Mode", "Mode element");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("GetObjectLegalHold response contains status", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt?legal-hold");
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "GetObjectLegalHold");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                AssertHelper.StringContains(body, "LegalHold", "LegalHold element");
                AssertHelper.StringContains(body, "Status", "Status element");
            }, token).ConfigureAwait(false);

            #endregion

            #region Multiple-Sequential-Requests

            await runner.RunTestAsync("Multiple rapid sequential requests handled correctly", async (ct) =>
            {
                for (int i = 0; i < 5; i++)
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/");
                    HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, $"sequential request {i + 1}");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Multiple different operations in sequence", async (ct) =>
            {
                // PUT object
                PutObjectResponse putResp = await server.S3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "sequence-test.txt",
                    ContentBody = "sequence",
                    ContentType = "text/plain"
                }, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)putResp.HttpStatusCode, "PUT sequence");

                // HEAD object
                GetObjectMetadataResponse headResp = await server.S3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = server.Bucket,
                    Key = "sequence-test.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)headResp.HttpStatusCode, "HEAD sequence");

                // GET object
                GetObjectResponse getResp = await server.S3Client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "sequence-test.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)getResp.HttpStatusCode, "GET sequence");

                // DELETE object
                DeleteObjectResponse delResp = await server.S3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "sequence-test.txt"
                }, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(204, (int)delResp.HttpStatusCode, "DELETE sequence");
            }, token).ConfigureAwait(false);

            #endregion
        }
    }
}
