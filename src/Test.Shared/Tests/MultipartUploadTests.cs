namespace Test.Shared.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.S3.Model;

    /// <summary>
    /// Multipart upload API tests.
    /// </summary>
    public static class MultipartUploadTests
    {
        /// <summary>
        /// Run all multipart upload tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            Console.WriteLine();
            Console.WriteLine("--- Multipart Upload Tests ---");

            await runner.RunTestAsync("InitiateMultipartUpload returns upload ID", async (ct) =>
            {
                InitiateMultipartUploadResponse response = await server.S3Client.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest
                {
                    BucketName = server.Bucket,
                    Key = "multipart-test.txt",
                    ContentType = "text/plain"
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "InitiateMultipartUpload");
                AssertHelper.IsNotNull(response.UploadId, "UploadId");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("UploadPart succeeds", async (ct) =>
            {
                UploadPartResponse response = await server.S3Client.UploadPartAsync(new UploadPartRequest
                {
                    BucketName = server.Bucket,
                    Key = "multipart-test.txt",
                    UploadId = "test-upload-id",
                    PartNumber = 1,
                    InputStream = new MemoryStream(Encoding.UTF8.GetBytes("Part 1 data"))
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "UploadPart");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListParts returns parts", async (ct) =>
            {
                ListPartsResponse response = await server.S3Client.ListPartsAsync(new ListPartsRequest
                {
                    BucketName = server.Bucket,
                    Key = "multipart-test.txt",
                    UploadId = "test-upload-id"
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "ListParts");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("CompleteMultipartUpload finishes upload", async (ct) =>
            {
                CompleteMultipartUploadResponse response = await server.S3Client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest
                {
                    BucketName = server.Bucket,
                    Key = "multipart-test.txt",
                    UploadId = "test-upload-id",
                    PartETags = new List<PartETag> { new PartETag(1, "etag1") }
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "CompleteMultipartUpload");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("AbortMultipartUpload cancels upload", async (ct) =>
            {
                AbortMultipartUploadResponse response = await server.S3Client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest
                {
                    BucketName = server.Bucket,
                    Key = "multipart-test.txt",
                    UploadId = "test-upload-id"
                }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(204, (int)response.HttpStatusCode, "AbortMultipartUpload");
            }, token).ConfigureAwait(false);
        }
    }
}
