namespace Test.Shared.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.S3;
    using Amazon.S3.Model;

    /// <summary>
    /// Bucket API tests (CRUD, ACL, tagging, versioning, logging, website).
    /// </summary>
    public static class BucketTests
    {
        /// <summary>
        /// Run all bucket API tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            Console.WriteLine();
            Console.WriteLine("--- Bucket API Tests ---");

            await runner.RunTestAsync("BucketWrite creates bucket", async (ct) =>
            {
                PutBucketResponse response = await server.S3Client.PutBucketAsync(new PutBucketRequest { BucketName = server.Bucket }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutBucket");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketExists returns location", async (ct) =>
            {
                GetBucketLocationResponse response = await server.S3Client.GetBucketLocationAsync(new GetBucketLocationRequest { BucketName = server.Bucket }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketRead lists objects", async (ct) =>
            {
                ListObjectsV2Response response = await server.S3Client.ListObjectsV2Async(new ListObjectsV2Request { BucketName = server.Bucket }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "ListObjectsV2");
                AssertHelper.IsNotNull(response.S3Objects, "S3Objects");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketReadAcl returns AccessControlPolicy", async (ct) =>
            {
                GetACLResponse response = await server.S3Client.GetACLAsync(new GetACLRequest { BucketName = server.Bucket }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "GetBucketACL");
                AssertHelper.IsNotNull(response.AccessControlList, "ACL");
                AssertHelper.IsNotNull(response.AccessControlList.Owner, "ACL Owner");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketWriteAcl sets AccessControlPolicy", async (ct) =>
            {
                PutACLRequest request = new PutACLRequest
                {
                    BucketName = server.Bucket,
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
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutBucketACL");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketReadLocation returns region", async (ct) =>
            {
                GetBucketLocationResponse response = await server.S3Client.GetBucketLocationAsync(new GetBucketLocationRequest { BucketName = server.Bucket }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "GetBucketLocation");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketWriteTagging sets tags", async (ct) =>
            {
                PutBucketTaggingRequest request = new PutBucketTaggingRequest
                {
                    BucketName = server.Bucket,
                    TagSet = new List<Amazon.S3.Model.Tag>
                    {
                        new Amazon.S3.Model.Tag { Key = "Environment", Value = "Test" },
                        new Amazon.S3.Model.Tag { Key = "Application", Value = "S3Server" }
                    }
                };
                PutBucketTaggingResponse response = await server.S3Client.PutBucketTaggingAsync(request, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutBucketTagging");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketReadTagging returns tags", async (ct) =>
            {
                GetBucketTaggingResponse response = await server.S3Client.GetBucketTaggingAsync(new GetBucketTaggingRequest { BucketName = server.Bucket }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "GetBucketTagging");
                AssertHelper.IsNotNull(response.TagSet, "TagSet");
                AssertHelper.IsNotEmpty(response.TagSet, "TagSet");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketWriteVersioning enables versioning", async (ct) =>
            {
                PutBucketVersioningRequest request = new PutBucketVersioningRequest
                {
                    BucketName = server.Bucket,
                    VersioningConfig = new S3BucketVersioningConfig { Status = VersionStatus.Enabled }
                };
                PutBucketVersioningResponse response = await server.S3Client.PutBucketVersioningAsync(request, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutBucketVersioning");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketReadVersioning returns status", async (ct) =>
            {
                GetBucketVersioningResponse response = await server.S3Client.GetBucketVersioningAsync(new GetBucketVersioningRequest { BucketName = server.Bucket }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "GetBucketVersioning");
                AssertHelper.IsNotNull(response.VersioningConfig, "VersioningConfig");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketReadVersions returns object versions", async (ct) =>
            {
                ListVersionsResponse response = await server.S3Client.ListVersionsAsync(new ListVersionsRequest { BucketName = server.Bucket }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "ListVersions");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketWriteLogging sets logging config", async (ct) =>
            {
                PutBucketLoggingRequest request = new PutBucketLoggingRequest
                {
                    BucketName = server.Bucket,
                    LoggingConfig = new S3BucketLoggingConfig { TargetBucketName = server.Bucket, TargetPrefix = "logs/" }
                };
                PutBucketLoggingResponse response = await server.S3Client.PutBucketLoggingAsync(request, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutBucketLogging");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketReadLogging returns logging config", async (ct) =>
            {
                GetBucketLoggingResponse response = await server.S3Client.GetBucketLoggingAsync(new GetBucketLoggingRequest { BucketName = server.Bucket }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "GetBucketLogging");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketWriteWebsite sets website config", async (ct) =>
            {
                PutBucketWebsiteRequest request = new PutBucketWebsiteRequest
                {
                    BucketName = server.Bucket,
                    WebsiteConfiguration = new Amazon.S3.Model.WebsiteConfiguration
                    {
                        IndexDocumentSuffix = "index.html",
                        ErrorDocument = "error.html"
                    }
                };
                PutBucketWebsiteResponse response = await server.S3Client.PutBucketWebsiteAsync(request, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutBucketWebsite");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketReadWebsite returns website config", async (ct) =>
            {
                GetBucketWebsiteResponse response = await server.S3Client.GetBucketWebsiteAsync(new GetBucketWebsiteRequest { BucketName = server.Bucket }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "GetBucketWebsite");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketDeleteWebsite removes website config", async (ct) =>
            {
                DeleteBucketWebsiteResponse response = await server.S3Client.DeleteBucketWebsiteAsync(new DeleteBucketWebsiteRequest { BucketName = server.Bucket }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(204, (int)response.HttpStatusCode, "DeleteBucketWebsite");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketDeleteTagging removes tags", async (ct) =>
            {
                DeleteBucketTaggingResponse response = await server.S3Client.DeleteBucketTaggingAsync(new DeleteBucketTaggingRequest { BucketName = server.Bucket }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(204, (int)response.HttpStatusCode, "DeleteBucketTagging");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketDelete removes bucket", async (ct) =>
            {
                DeleteBucketResponse response = await server.S3Client.DeleteBucketAsync(new DeleteBucketRequest { BucketName = "delete-test-bucket" }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(204, (int)response.HttpStatusCode, "DeleteBucket");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketReadMultipartUploads returns uploads", async (ct) =>
            {
                ListMultipartUploadsResponse response = await server.S3Client.ListMultipartUploadsAsync(new ListMultipartUploadsRequest { BucketName = server.Bucket }, ct).ConfigureAwait(false);
                AssertHelper.IsNotNull(response, "response");
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "ListMultipartUploads");
            }, token).ConfigureAwait(false);
        }
    }
}
