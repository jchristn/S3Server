namespace Test.Shared.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using S3ServerLibrary;

    /// <summary>
    /// Parser and routing tests that assert parsed S3Request state through real HTTP requests.
    /// </summary>
    public static class ParserRoutingTests
    {
        /// <summary>
        /// Run all parser and routing tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            await runner.RunTestAsync("Path-style service GET parses ListBuckets", async (ct) =>
            {
                server.ClearObservedRequests();
                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "service GET");

                S3RequestObservation request = RequireLastObservation(server);
                AssertHelper.AreEqual("GET", request.Method, "method");
                AssertHelper.AreEqual(S3RequestType.ListBuckets, request.RequestType, "request type");
                AssertHelper.AreEqual(S3RequestStyle.PathStyle, request.RequestStyle, "request style");
                AssertHelper.IsTrue(String.IsNullOrEmpty(request.Bucket), "bucket is empty");
                AssertHelper.IsTrue(String.IsNullOrEmpty(request.Key), "key is empty");
                AssertHelper.IsTrue(request.IsServiceRequest, "service request");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Path-style bucket HEAD parses BucketExists", async (ct) =>
            {
                server.ClearObservedRequests();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, server.BaseUrl + "/" + server.Bucket);
                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "bucket HEAD");

                S3RequestObservation observed = RequireLastObservation(server);
                AssertHelper.AreEqual("HEAD", observed.Method, "method");
                AssertHelper.AreEqual(S3RequestType.BucketExists, observed.RequestType, "request type");
                AssertHelper.AreEqual(server.Bucket, observed.Bucket, "bucket");
                AssertHelper.IsNull(observed.Key, "key");
                AssertHelper.IsTrue(observed.IsBucketRequest, "bucket request");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListObjectsV2 parses prefix delimiter marker max keys and continuation token", async (ct) =>
            {
                server.ClearObservedRequests();
                string url = server.BaseUrl
                    + "/"
                    + server.Bucket
                    + "?list-type=2&prefix=photos%2F2026%2F&delimiter=%2F&marker=start-here&max-keys=37&continuation-token=token-123";

                HttpResponseMessage response = await server.HttpClient.GetAsync(url, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "ListObjectsV2");

                S3RequestObservation request = RequireLastObservation(server);
                AssertHelper.AreEqual(S3RequestType.BucketRead, request.RequestType, "request type");
                AssertHelper.AreEqual(server.Bucket, request.Bucket, "bucket");
                AssertHelper.AreEqual("photos/2026/", request.Prefix, "prefix");
                AssertHelper.AreEqual("/", request.Delimiter, "delimiter");
                AssertHelper.AreEqual("start-here", request.Marker, "marker");
                AssertHelper.AreEqual("token-123", request.ContinuationToken, "continuation token");
                AssertHelper.AreEqual(37, request.MaxKeys, "max keys");
                AssertHelper.AreEqual(S3PermissionType.BucketRead, request.PermissionsRequired, "permissions");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Object key path is URL-decoded", async (ct) =>
            {
                server.ClearObservedRequests();
                string key = "folder one/file+name (1).txt";
                string url = server.BaseUrl + "/" + server.Bucket + "/" + EscapeKey(key);

                HttpResponseMessage response = await server.HttpClient.GetAsync(url, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "object GET");

                S3RequestObservation request = RequireLastObservation(server);
                AssertHelper.AreEqual(S3RequestType.ObjectRead, request.RequestType, "request type");
                AssertHelper.AreEqual(server.Bucket, request.Bucket, "bucket");
                AssertHelper.AreEqual(key, request.Key, "key");
                AssertHelper.IsTrue(request.IsObjectRequest, "object request");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Range header parses byte offsets and ObjectReadRange", async (ct) =>
            {
                server.ClearObservedRequests();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                request.Headers.TryAddWithoutValidation("Range", "bytes=1-3");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.PartialContent, response, "range GET");

                S3RequestObservation observed = RequireLastObservation(server);
                AssertHelper.AreEqual(S3RequestType.ObjectReadRange, observed.RequestType, "request type");
                AssertHelper.AreEqual(1, observed.RangeStart.Value, "range start");
                AssertHelper.AreEqual(3, observed.RangeEnd.Value, "range end");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Multipart upload part parses part number and upload ID", async (ct) =>
            {
                server.ClearObservedRequests();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + "/" + server.Bucket + "/multipart.bin?partNumber=7&uploadId=upload-123");
                request.Content = new StringContent("part-data", Encoding.UTF8, "application/octet-stream");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "upload part");

                S3RequestObservation observed = RequireLastObservation(server);
                AssertHelper.AreEqual(S3RequestType.ObjectUploadPart, observed.RequestType, "request type");
                AssertHelper.AreEqual("multipart.bin", observed.Key, "key");
                AssertHelper.AreEqual(7, observed.PartNumber, "part number");
                AssertHelper.AreEqual("upload-123", observed.UploadId, "upload ID");
                AssertHelper.IsTrue(observed.IsMultipartUploadRequest, "multipart request");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Object ACL subresource routes before object read", async (ct) =>
            {
                server.ClearObservedRequests();
                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/test-object.txt?acl", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "object ACL");

                S3RequestObservation observed = RequireLastObservation(server);
                AssertHelper.AreEqual(S3RequestType.ObjectReadAcl, observed.RequestType, "request type");
                AssertHelper.AreEqual(S3PermissionType.ObjectReadAcp, observed.PermissionsRequired, "permissions");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("V2 Authorization header parses access key and signature", async (ct) =>
            {
                server.ClearObservedRequests();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/");
                request.Headers.TryAddWithoutValidation("Authorization", "AWS " + server.AccessKey + ":abc123signature");
                request.Headers.TryAddWithoutValidation("Date", DateTime.UtcNow.ToString("R"));

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "V2 parse request");

                S3RequestObservation observed = RequireLastObservation(server);
                AssertHelper.AreEqual(S3SignatureVersion.Version2, observed.SignatureVersion, "signature version");
                AssertHelper.AreEqual(server.AccessKey, observed.AccessKey, "access key");
                AssertHelper.AreEqual("abc123signature", observed.Signature, "signature");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("V2 signed URL query parses access key expiration and signature", async (ct) =>
            {
                server.ClearObservedRequests();
                string expires = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds().ToString();
                string url = server.BaseUrl
                    + "/"
                    + server.Bucket
                    + "/hello.txt?AWSAccessKeyId="
                    + WebUtility.UrlEncode(server.AccessKey)
                    + "&Expires="
                    + WebUtility.UrlEncode(expires)
                    + "&Signature="
                    + WebUtility.UrlEncode("abc123=");

                HttpResponseMessage response = await server.HttpClient.GetAsync(url, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "V2 signed URL parse request");

                S3RequestObservation observed = RequireLastObservation(server);
                AssertHelper.AreEqual(server.AccessKey, observed.AccessKey, "access key");
                AssertHelper.AreEqual(expires, observed.Expires, "expires");
                AssertHelper.AreEqual("abc123=", observed.Signature, "signature");
            }, token).ConfigureAwait(false);
        }

        private static S3RequestObservation RequireLastObservation(S3TestServer server)
        {
            AssertHelper.IsNotNull(server.LastObservedRequest, "last observed request");
            return server.LastObservedRequest;
        }

        private static string EscapeKey(string key)
        {
            return Uri.EscapeDataString(key).Replace("%2F", "/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
