namespace Test.Shared.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Amazon.S3;
    using Amazon.S3.Model;
    using S3ServerLibrary;
    using S3ServerLibrary.S3Objects;

    /// <summary>
    /// Positive and negative coverage for request parsing and routing fixes:
    /// invalid querystring integers, malformed and suffix ranges, CopyObject and UploadPartCopy routing,
    /// canned ACL writes, ListObjects pagination parameters, conditional headers, and decoded query values.
    /// </summary>
    public static class RequestParsingFixTests
    {
        /// <summary>
        /// Run all request parsing fix tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            #region Invalid-Querystring-Integers

            foreach (string query in new[] { "max-keys=-1", "max-keys=abc", "max-parts=-5", "part-number-marker=x", "partNumber=x", "max-keys=99999999999" })
            {
                string captured = query;

                await runner.RunTestAsync("Invalid querystring integer returns XML InvalidArgument: " + captured, async (ct) =>
                {
                    string url = server.BaseUrl + "/" + server.Bucket + "/test-object.txt?" + captured;
                    if (captured.StartsWith("max-keys", StringComparison.Ordinal)) url = server.BaseUrl + "/" + server.Bucket + "?" + captured;

                    HttpResponseMessage response = await server.HttpClient.GetAsync(url, ct).ConfigureAwait(false);
                    await AssertXmlError(response, HttpStatusCode.BadRequest, "InvalidArgument", captured).ConfigureAwait(false);

                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    string parameterName = captured.Split('=')[0];
                    AssertHelper.StringContains(body, parameterName, "error message names the parameter");
                }, token).ConfigureAwait(false);
            }

            await runner.RunTestAsync("Boundary max-keys values still route normally", async (ct) =>
            {
                foreach (int value in new[] { 0, 1000 })
                {
                    server.ClearObservedRequests();
                    HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "?max-keys=" + value, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "max-keys=" + value);
                    AssertHelper.IsNotNull(server.LastObservedRequest, "observed request");
                    AssertHelper.AreEqual(value, server.LastObservedRequest.MaxKeys, "parsed max-keys");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Programmatic S3Request setters keep ArgumentOutOfRangeException contract", async (ct) =>
            {
                S3Request request = new S3Request();
                await AssertHelper.ThrowsAsync<ArgumentOutOfRangeException>(() => { request.MaxKeys = -1; return Task.CompletedTask; }, "MaxKeys setter").ConfigureAwait(false);
                await AssertHelper.ThrowsAsync<ArgumentOutOfRangeException>(() => { request.MaxParts = -1; return Task.CompletedTask; }, "MaxParts setter").ConfigureAwait(false);
                await AssertHelper.ThrowsAsync<ArgumentOutOfRangeException>(() => { request.RangeSuffixLength = -1; return Task.CompletedTask; }, "RangeSuffixLength setter").ConfigureAwait(false);
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Request that fails parsing never returns the webserver HTML error page", async (ct) =>
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "?max-keys=-1", ct).ConfigureAwait(false);
                string contentType = response.Content.Headers.ContentType != null ? response.Content.Headers.ContentType.MediaType : null;
                AssertHelper.AreEqual("application/xml", contentType, "content type");

                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringDoesNotContain(body, "<html", "HTML error page");

                HttpResponseMessage healthy = await server.HttpClient.GetAsync(server.BaseUrl + "/", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, healthy, "server healthy afterward");
            }, token).ConfigureAwait(false);

            #endregion

            #region Range-Headers

            foreach (string range in new[] { "bytes=abc", "bytes=0-1,4-5", "items=0-1", "bytes=-", "bytes=1-2-3", "bytes=5-2" })
            {
                string captured = range;

                // RFC 9110 section 14.2 lets a server ignore a Range header it cannot or will not satisfy, and Amazon S3
                // serves the full object with 200 for multiple ranges, unknown units, and malformed values.  An inverted
                // range such as bytes=5-2 is syntactically invalid under RFC 9110 section 14.1.1 and is treated the same way.
                await runner.RunTestAsync("Unparseable Range header serves the full object: " + captured, async (ct) =>
                {
                    int readRangeBefore = server.ObjectReadRangeCount;
                    int readBefore = server.ObjectReadCount;
                    server.ClearObservedRequests();

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                    request.Headers.TryAddWithoutValidation("Range", captured);

                    HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "full object status");

                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    AssertHelper.AreEqual("hello", body, "full object body");

                    S3RequestObservation observed = server.LastObservedRequest;
                    AssertHelper.IsNotNull(observed, "observed request");
                    AssertHelper.AreEqual(S3RequestType.ObjectRead, observed.RequestType, "request type");
                    AssertHelper.IsNull(observed.RangeStart, "range start");
                    AssertHelper.IsNull(observed.RangeEnd, "range end");
                    AssertHelper.IsNull(observed.Request.RangeSuffixLength, "suffix length");
                    AssertHelper.AreEqual(readRangeBefore, server.ObjectReadRangeCount, "ReadRange not invoked");
                    AssertHelper.AreEqual(readBefore + 1, server.ObjectReadCount, "Read invoked once");
                }, token).ConfigureAwait(false);
            }

            await runner.RunTestAsync("Closed and open-ended ranges still route to ReadRange with 206", async (ct) =>
            {
                HttpRequestMessage closed = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                closed.Headers.TryAddWithoutValidation("Range", "bytes=0-2");
                HttpResponseMessage closedResponse = await server.HttpClient.SendAsync(closed, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.PartialContent, closedResponse, "closed range");
                AssertHelper.AreEqual("hel", await closedResponse.Content.ReadAsStringAsync().ConfigureAwait(false), "closed range body");
                AssertHelper.AreEqual("bytes 0-2/5", closedResponse.Content.Headers.GetValues("Content-Range").First(), "closed Content-Range");

                HttpRequestMessage open = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                open.Headers.TryAddWithoutValidation("Range", "bytes=3-");
                HttpResponseMessage openResponse = await server.HttpClient.SendAsync(open, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.PartialContent, openResponse, "open range");
                AssertHelper.AreEqual("lo", await openResponse.Content.ReadAsStringAsync().ConfigureAwait(false), "open range body");
                AssertHelper.AreEqual("bytes 3-4/5", openResponse.Content.Headers.GetValues("Content-Range").First(), "open Content-Range");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Suffix range returns 206 with the last bytes and a computed Content-Range", async (ct) =>
            {
                server.ClearObservedRequests();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                request.Headers.TryAddWithoutValidation("Range", "bytes=-3");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.PartialContent, response, "suffix range");
                AssertHelper.AreEqual("llo", await response.Content.ReadAsStringAsync().ConfigureAwait(false), "suffix body");
                AssertHelper.AreEqual("bytes 2-4/5", response.Content.Headers.GetValues("Content-Range").First(), "suffix Content-Range");

                S3RequestObservation observed = server.LastObservedRequest;
                AssertHelper.AreEqual(S3RequestType.ObjectReadRange, observed.RequestType, "request type");
                AssertHelper.AreEqual(3L, observed.Request.RangeSuffixLength.Value, "suffix length");
                AssertHelper.IsNull(observed.RangeStart, "range start is null for suffix");
                AssertHelper.IsNull(observed.RangeEnd, "range end is null for suffix");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Suffix range longer than the object is clamped and consistent", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                request.Headers.TryAddWithoutValidation("Range", "bytes=-50");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.PartialContent, response, "clamped suffix");
                AssertHelper.AreEqual("hello", await response.Content.ReadAsStringAsync().ConfigureAwait(false), "clamped body");
                AssertHelper.AreEqual("bytes 0-4/5", response.Content.Headers.GetValues("Content-Range").First(), "clamped Content-Range");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Zero-length suffix range is unsatisfiable", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                request.Headers.TryAddWithoutValidation("Range", "bytes=-0");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                await AssertXmlError(response, HttpStatusCode.RequestedRangeNotSatisfiable, "InvalidRange", "zero suffix").ConfigureAwait(false);
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Suffix range without TotalSize fails with InternalError", async (ct) =>
            {
                server.Server.Object.ReadRange = async (ctx) =>
                {
                    S3ServerLibrary.S3Object obj = new S3ServerLibrary.S3Object("k", "1", true, DateTime.UtcNow, "etag", 3, null, "llo", "text/plain");
                    return obj;
                };

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                request.Headers.TryAddWithoutValidation("Range", "bytes=-3");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                await AssertXmlError(response, HttpStatusCode.InternalServerError, "InternalError", "missing TotalSize").ConfigureAwait(false);
                AssertHelper.IsFalse(response.Content.Headers.Contains("Content-Range"), "no malformed Content-Range");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("RouteSuffixRangesToReadRange false routes suffix ranges to Object.Read", async (ct) =>
            {
                server.Server.Settings.RouteSuffixRangesToReadRange = false;
                int readRangeBefore = server.ObjectReadRangeCount;
                server.ClearObservedRequests();

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                request.Headers.TryAddWithoutValidation("Range", "bytes=-3");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "legacy suffix routing");
                AssertHelper.AreEqual(S3RequestType.ObjectRead, server.LastObservedRequest.RequestType, "request type");
                AssertHelper.AreEqual(3L, server.LastObservedRequest.Request.RangeSuffixLength.Value, "suffix length still parsed");
                AssertHelper.AreEqual(readRangeBefore, server.ObjectReadRangeCount, "ReadRange not invoked");
            }, token).ConfigureAwait(false);

            #endregion

            #region CopyObject

            await runner.RunTestAsync("SDK CopyObject reaches Object.Copy with the parsed source and reads the ETag", async (ct) =>
            {
                server.ClearObservedRequests();
                int writeBefore = server.ObjectWriteCount;
                string sourceKey = "source dir/file name+plus.txt";

                CopyObjectResponse response = await server.S3Client.CopyObjectAsync(new CopyObjectRequest
                {
                    SourceBucket = "source-bucket",
                    SourceKey = sourceKey,
                    DestinationBucket = server.Bucket,
                    DestinationKey = "copy-destination.txt"
                }, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "CopyObject status");
                AssertHelper.AreEqual("\"9b2c3e7a8d1f4e6b5c2a1d8f7e4b3c2a\"", response.ETag, "CopyObject ETag");

                S3RequestObservation observed = server.LastObservedRequest;
                AssertHelper.AreEqual(S3RequestType.ObjectCopy, observed.RequestType, "request type");
                AssertHelper.AreEqual("source-bucket", observed.Request.CopySourceBucket, "source bucket");
                AssertHelper.AreEqual(sourceKey, observed.Request.CopySourceKey, "source key");
                AssertHelper.IsNull(observed.Request.CopySourceVersionId, "source version");
                AssertHelper.AreEqual("copy-destination.txt", observed.Key, "destination key");
                AssertHelper.IsTrue(observed.IsObjectRequest, "object request");
                AssertHelper.AreEqual(writeBefore, server.ObjectWriteCount, "Object.Write not invoked");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Copy source with encoded slash space and versionId is parsed", async (ct) =>
            {
                server.ClearObservedRequests();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + "/" + server.Bucket + "/dest.txt");
                request.Headers.TryAddWithoutValidation("x-amz-copy-source", "/src-bucket/dir%2Fa%20b.txt?versionId=3");
                request.Headers.TryAddWithoutValidation("x-amz-copy-source-if-match", "\"abc\"");
                request.Headers.TryAddWithoutValidation("x-amz-copy-source-if-unmodified-since", "Sun, 06 Nov 1994 08:49:37 GMT");
                request.Content = new ByteArrayContent(Array.Empty<byte>());

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "copy status");

                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                XDocument doc = XDocument.Parse(body);
                AssertHelper.AreEqual("CopyObjectResult", doc.Root.Name.LocalName, "root element");
                AssertHelper.IsNotNull(doc.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "ETag"), "ETag element");
                AssertHelper.IsNotNull(doc.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "LastModified"), "LastModified element");

                S3Request parsed = server.LastObservedRequest.Request;
                AssertHelper.AreEqual("src-bucket", parsed.CopySourceBucket, "source bucket");
                AssertHelper.AreEqual("dir/a b.txt", parsed.CopySourceKey, "source key");
                AssertHelper.AreEqual("3", parsed.CopySourceVersionId, "source version");
                AssertHelper.HasCount(parsed.CopySourceIfMatch, 1, "copy-source-if-match count");
                AssertHelper.AreEqual("\"abc\"", parsed.CopySourceIfMatch[0], "copy-source-if-match");
                AssertHelper.AreEqual(new DateTime(1994, 11, 6, 8, 49, 37, DateTimeKind.Utc), parsed.CopySourceIfUnmodifiedSince.Value, "copy-source-if-unmodified-since");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("CopyObject without Object.Copy returns NotImplemented and never writes", async (ct) =>
            {
                server.Server.Object.Copy = null;
                server.Server.Settings.DefaultRequestHandler = null;
                int writeBefore = server.ObjectWriteCount;

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + "/" + server.Bucket + "/dest.txt");
                request.Headers.TryAddWithoutValidation("x-amz-copy-source", "src-bucket/src.txt");
                request.Content = new ByteArrayContent(Array.Empty<byte>());

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                await AssertXmlError(response, HttpStatusCode.NotImplemented, "NotImplemented", "unwired copy").ConfigureAwait(false);
                AssertHelper.AreEqual(writeBefore, server.ObjectWriteCount, "Object.Write not invoked");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Malformed copy source returns InvalidArgument", async (ct) =>
            {
                foreach (string source in new[] { "no-slash-here", "/bucket-only/", "/", "bucket/key?versionId=" })
                {
                    int writeBefore = server.ObjectWriteCount;
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + "/" + server.Bucket + "/dest.txt");
                    request.Headers.TryAddWithoutValidation("x-amz-copy-source", source);
                    request.Content = new ByteArrayContent(Array.Empty<byte>());

                    HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                    await AssertXmlError(response, HttpStatusCode.BadRequest, "InvalidArgument", "copy source " + source).ConfigureAwait(false);
                    AssertHelper.AreEqual(writeBefore, server.ObjectWriteCount, "Object.Write not invoked for " + source);
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Plain PUT without copy source still reaches Object.Write", async (ct) =>
            {
                server.ClearObservedRequests();
                int writeBefore = server.ObjectWriteCount;

                PutObjectResponse response = await server.S3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = server.Bucket,
                    Key = "plain-put.txt",
                    ContentBody = "plain"
                }, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "PutObject");
                AssertHelper.AreEqual(S3RequestType.ObjectWrite, server.LastObservedRequest.RequestType, "request type");
                AssertHelper.AreEqual(writeBefore + 1, server.ObjectWriteCount, "Object.Write invoked");
                AssertHelper.IsNull(server.LastObservedRequest.Request.CopySourceKey, "no copy source");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("SDK CopyPart reaches Object.UploadPartCopy with the parsed range", async (ct) =>
            {
                server.ClearObservedRequests();
                int uploadPartBefore = server.ObjectUploadPartCount;

                CopyPartResponse response = await server.S3Client.CopyPartAsync(new CopyPartRequest
                {
                    SourceBucket = "source-bucket",
                    SourceKey = "big source.bin",
                    DestinationBucket = server.Bucket,
                    DestinationKey = "assembled.bin",
                    UploadId = "upload-123",
                    PartNumber = 2,
                    FirstByte = 100,
                    LastByte = 199
                }, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "CopyPart status");
                AssertHelper.AreEqual("\"1b2c3e7a8d1f4e6b5c2a1d8f7e4b3c2b\"", response.ETag, "CopyPart ETag");

                S3RequestObservation observed = server.LastObservedRequest;
                AssertHelper.AreEqual(S3RequestType.ObjectUploadPartCopy, observed.RequestType, "request type");
                AssertHelper.AreEqual("source-bucket", observed.Request.CopySourceBucket, "source bucket");
                AssertHelper.AreEqual("big source.bin", observed.Request.CopySourceKey, "source key");
                AssertHelper.AreEqual(100L, observed.Request.CopySourceRangeStart.Value, "range start");
                AssertHelper.AreEqual(199L, observed.Request.CopySourceRangeEnd.Value, "range end");
                AssertHelper.AreEqual(2, observed.PartNumber, "part number");
                AssertHelper.IsTrue(observed.IsMultipartUploadRequest, "multipart request");
                AssertHelper.AreEqual(uploadPartBefore, server.ObjectUploadPartCount, "Object.UploadPart not invoked");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("UploadPartCopy without callback returns NotImplemented and never uploads", async (ct) =>
            {
                server.Server.Object.UploadPartCopy = null;
                server.Server.Settings.DefaultRequestHandler = null;
                int uploadPartBefore = server.ObjectUploadPartCount;

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + "/" + server.Bucket + "/assembled.bin?partNumber=1&uploadId=upload-123");
                request.Headers.TryAddWithoutValidation("x-amz-copy-source", "src-bucket/src.bin");
                request.Content = new ByteArrayContent(Array.Empty<byte>());

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                await AssertXmlError(response, HttpStatusCode.NotImplemented, "NotImplemented", "unwired part copy").ConfigureAwait(false);
                AssertHelper.AreEqual(uploadPartBefore, server.ObjectUploadPartCount, "Object.UploadPart not invoked");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Malformed copy source range returns InvalidArgument", async (ct) =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + "/" + server.Bucket + "/assembled.bin?partNumber=1&uploadId=upload-123");
                request.Headers.TryAddWithoutValidation("x-amz-copy-source", "src-bucket/src.bin");
                request.Headers.TryAddWithoutValidation("x-amz-copy-source-range", "bytes=10-");
                request.Content = new ByteArrayContent(Array.Empty<byte>());

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                await AssertXmlError(response, HttpStatusCode.BadRequest, "InvalidArgument", "open copy range").ConfigureAwait(false);
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Copy source header on GET is ignored", async (ct) =>
            {
                server.ClearObservedRequests();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                request.Headers.TryAddWithoutValidation("x-amz-copy-source", "not a valid source");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "GET with stray copy header");
                AssertHelper.AreEqual(S3RequestType.ObjectRead, server.LastObservedRequest.RequestType, "request type");
                AssertHelper.IsNull(server.LastObservedRequest.Request.CopySourceKey, "copy source not parsed");
            }, token).ConfigureAwait(false);

            #endregion

            #region Canned-ACLs

            await runner.RunTestAsync("Empty-body bucket PUT acl with x-amz-acl reaches WriteAcl with null policy", async (ct) =>
            {
                server.ClearObservedRequests();
                int aclBefore = server.WriteAclCount;

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + "/" + server.Bucket + "?acl");
                request.Headers.TryAddWithoutValidation("x-amz-acl", "public-read");
                request.Content = new ByteArrayContent(Array.Empty<byte>());

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "canned bucket ACL");
                AssertHelper.AreEqual(aclBefore + 1, server.WriteAclCount, "WriteAcl invoked");
                AssertHelper.IsNull(server.LastWriteAclPolicy, "policy is null");
                AssertHelper.AreEqual("public-read", server.LastObservedRequest.AclHeader, "x-amz-acl readable");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("SDK PutObjectAcl with canned ACL reaches Object.WriteAcl with null policy", async (ct) =>
            {
                int aclBefore = server.WriteAclCount;

                PutObjectAclResponse response = await server.S3Client.PutObjectAclAsync(new PutObjectAclRequest
                {
                    BucketName = server.Bucket,
                    Key = "acl-object.txt",
                    ACL = S3CannedACL.PublicRead
                }, ct).ConfigureAwait(false);

                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "canned object ACL");
                AssertHelper.AreEqual(aclBefore + 1, server.WriteAclCount, "WriteAcl invoked");
                AssertHelper.IsNull(server.LastWriteAclPolicy, "policy is null");
                AssertHelper.AreEqual("public-read", server.LastObservedRequest.AclHeader, "x-amz-acl readable");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Malformed ACL body still returns MalformedXML without invoking WriteAcl", async (ct) =>
            {
                foreach (string path in new[] { "/" + server.Bucket + "?acl", "/" + server.Bucket + "/acl-object.txt?acl" })
                {
                    int aclBefore = server.WriteAclCount;
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + path);
                    request.Content = new StringContent("<not-xml", Encoding.UTF8, "application/xml");

                    HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                    await AssertXmlError(response, HttpStatusCode.BadRequest, "MalformedXML", "malformed ACL " + path).ConfigureAwait(false);
                    AssertHelper.AreEqual(aclBefore, server.WriteAclCount, "WriteAcl not invoked for " + path);
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Well-formed ACL body still arrives populated", async (ct) =>
            {
                string xml =
                    "<AccessControlPolicy xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">"
                    + "<Owner><ID>owner-id</ID><DisplayName>Owner</DisplayName></Owner>"
                    + "<AccessControlList><Grant>"
                    + "<Grantee xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:type=\"CanonicalUser\"><ID>owner-id</ID><DisplayName>Owner</DisplayName></Grantee>"
                    + "<Permission>FULL_CONTROL</Permission>"
                    + "</Grant></AccessControlList>"
                    + "</AccessControlPolicy>";

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, server.BaseUrl + "/" + server.Bucket + "?acl");
                request.Content = new StringContent(xml, Encoding.UTF8, "application/xml");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "full ACL body");
                AssertHelper.IsNotNull(server.LastWriteAclPolicy, "policy populated");
                AssertHelper.AreEqual("owner-id", server.LastWriteAclPolicy.Owner.ID, "owner ID");
            }, token).ConfigureAwait(false);

            #endregion

            #region ListObjects-Pagination

            await runner.RunTestAsync("start-after and fetch-owner are parsed and decoded", async (ct) =>
            {
                server.ClearObservedRequests();
                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "?list-type=2&start-after=photos%2F2026%20a&fetch-owner=true", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "ListObjectsV2");
                AssertHelper.AreEqual("photos/2026 a", server.LastObservedRequest.Request.StartAfter, "start-after");
                AssertHelper.IsTrue(server.LastObservedRequest.Request.FetchOwner, "fetch-owner");

                server.ClearObservedRequests();
                response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "?list-type=2", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "ListObjectsV2 without parameters");
                AssertHelper.IsNull(server.LastObservedRequest.Request.StartAfter, "start-after absent");
                AssertHelper.IsFalse(server.LastObservedRequest.Request.FetchOwner, "fetch-owner absent");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("SDK ListObjectsV2 walks every page using continuation tokens", async (ct) =>
            {
                server.Server.Bucket.Read = async (ctx) =>
                {
                    ListBucketResult result = new ListBucketResult();
                    result.Name = ctx.Request.Bucket;
                    result.MaxKeys = 2;
                    result.ContinuationToken = ctx.Request.ContinuationToken;
                    result.StartAfter = ctx.Request.StartAfter;

                    if (String.IsNullOrEmpty(ctx.Request.ContinuationToken))
                    {
                        result.Contents.Add(new ObjectMetadata("a.txt", DateTime.UtcNow, "etag-a", 1, null));
                        result.Contents.Add(new ObjectMetadata("b.txt", DateTime.UtcNow, "etag-b", 1, null));
                        result.IsTruncated = true;
                        result.NextContinuationToken = "token-page-2";
                    }
                    else if (ctx.Request.ContinuationToken == "token-page-2")
                    {
                        result.Contents.Add(new ObjectMetadata("c.txt", DateTime.UtcNow, "etag-c", 1, null));
                        result.IsTruncated = false;
                    }
                    else
                    {
                        throw new S3Exception(new Error(ErrorCode.InvalidArgument));
                    }

                    result.KeyCount = result.Contents.Count;
                    return result;
                };

                List<string> keys = new List<string>();
                string continuation = null;
                int pages = 0;

                do
                {
                    ListObjectsV2Response page = await server.S3Client.ListObjectsV2Async(new ListObjectsV2Request
                    {
                        BucketName = server.Bucket,
                        ContinuationToken = continuation,
                        StartAfter = "0"
                    }, ct).ConfigureAwait(false);

                    pages++;
                    AssertHelper.AreEqual("0", page.StartAfter, "start-after echo on page " + pages);
                    if (continuation != null) AssertHelper.AreEqual(continuation, page.ContinuationToken, "continuation echo");
                    keys.AddRange(page.S3Objects.Select(o => o.Key));
                    continuation = page.IsTruncated == true ? page.NextContinuationToken : null;
                    AssertHelper.IsTrue(pages < 5, "pagination terminates");
                }
                while (continuation != null);

                AssertHelper.AreEqual(2, pages, "page count");
                AssertHelper.AreEqual("a.txt,b.txt,c.txt", String.Join(",", keys), "all keys");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("SDK ListObjects v1 with delimiter resumes from NextMarker", async (ct) =>
            {
                server.Server.Bucket.Read = async (ctx) =>
                {
                    ListBucketResult result = new ListBucketResult();
                    result.Name = ctx.Request.Bucket;
                    result.Delimiter = ctx.Request.Delimiter;
                    result.Marker = ctx.Request.Marker;

                    if (String.IsNullOrEmpty(ctx.Request.Marker))
                    {
                        result.Contents.Add(new ObjectMetadata("a.txt", DateTime.UtcNow, "etag-a", 1, null));
                        result.CommonPrefixes.Add(new CommonPrefixes("photos/"));
                        result.IsTruncated = true;
                        result.NextMarker = "photos/";
                    }
                    else
                    {
                        AssertHelper.AreEqual("photos/", ctx.Request.Marker, "resume marker");
                        result.Contents.Add(new ObjectMetadata("z.txt", DateTime.UtcNow, "etag-z", 1, null));
                        result.IsTruncated = false;
                    }

                    result.KeyCount = result.Contents.Count;
                    return result;
                };

                ListObjectsResponse first = await server.S3Client.ListObjectsAsync(new ListObjectsRequest
                {
                    BucketName = server.Bucket,
                    Delimiter = "/"
                }, ct).ConfigureAwait(false);

                AssertHelper.IsTrue(first.IsTruncated == true, "first page truncated");
                AssertHelper.AreEqual("photos/", first.NextMarker, "NextMarker");

                ListObjectsResponse second = await server.S3Client.ListObjectsAsync(new ListObjectsRequest
                {
                    BucketName = server.Bucket,
                    Delimiter = "/",
                    Marker = first.NextMarker
                }, ct).ConfigureAwait(false);

                AssertHelper.IsFalse(second.IsTruncated == true, "second page complete");
                AssertHelper.AreEqual("z.txt", second.S3Objects.Single().Key, "second page key");
            }, token).ConfigureAwait(false);

            #endregion

            #region Conditional-Headers

            await runner.RunTestAsync("Conditional request headers are parsed into typed properties", async (ct) =>
            {
                server.ClearObservedRequests();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                request.Headers.TryAddWithoutValidation("If-Match", "\"a\", \"b,c\"");
                request.Headers.TryAddWithoutValidation("If-None-Match", "W/\"weak\", \"strong\"");
                request.Headers.TryAddWithoutValidation("If-Modified-Since", "Sun, 06 Nov 1994 08:49:37 GMT");
                request.Headers.TryAddWithoutValidation("If-Unmodified-Since", "Sunday, 06-Nov-94 08:49:37 GMT");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "conditional GET");

                S3Request parsed = server.LastObservedRequest.Request;
                AssertHelper.AreEqual("\"a\"|\"b,c\"", String.Join("|", parsed.IfMatch), "If-Match list");
                AssertHelper.AreEqual("W/\"weak\"|\"strong\"", String.Join("|", parsed.IfNoneMatch), "If-None-Match list");
                AssertHelper.AreEqual(new DateTime(1994, 11, 6, 8, 49, 37, DateTimeKind.Utc), parsed.IfModifiedSince.Value, "If-Modified-Since");
                AssertHelper.AreEqual(new DateTime(1994, 11, 6, 8, 49, 37, DateTimeKind.Utc), parsed.IfUnmodifiedSince.Value, "If-Unmodified-Since");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Wildcard and garbage conditional headers parse safely", async (ct) =>
            {
                server.ClearObservedRequests();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/test-object.txt");
                request.Headers.TryAddWithoutValidation("If-None-Match", "*");
                request.Headers.TryAddWithoutValidation("If-Modified-Since", "not a date");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "garbage conditional GET");

                S3Request parsed = server.LastObservedRequest.Request;
                AssertHelper.AreEqual("*", String.Join("|", parsed.IfNoneMatch), "wildcard");
                AssertHelper.IsNull(parsed.IfModifiedSince, "garbage date ignored");
                AssertHelper.IsNull(parsed.IfMatch, "absent If-Match");
                AssertHelper.IsNull(parsed.IfUnmodifiedSince, "absent If-Unmodified-Since");
            }, token).ConfigureAwait(false);

            #endregion

            #region Decoded-Query-Values

            await runner.RunTestAsync("RetrieveQueryValue returns decoded values", async (ct) =>
            {
                string decoded = null;
                string plus = null;
                string absent = "sentinel";

                server.Server.Bucket.Read = async (ctx) =>
                {
                    decoded = ctx.Request.RetrieveQueryValue("x-test");
                    plus = ctx.Request.RetrieveQueryValue("x-plus");
                    absent = ctx.Request.RetrieveQueryValue("x-absent");
                    return new ListBucketResult();
                };

                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "?x-test=a%2Fb%20c&x-plus=1%2B1", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "list with custom query");
                AssertHelper.AreEqual("a/b c", decoded, "percent-decoded value");
                AssertHelper.AreEqual("1+1", plus, "encoded plus value");
                AssertHelper.IsNull(absent, "absent key");
            }, token).ConfigureAwait(false);

            #endregion
        }

        private static async Task AssertXmlError(HttpResponseMessage response, HttpStatusCode expectedStatus, string expectedCode, string name)
        {
            AssertHelper.StatusCodeEquals(expectedStatus, response, name);

            string contentType = response.Content.Headers.ContentType != null ? response.Content.Headers.ContentType.MediaType : null;
            AssertHelper.AreEqual("application/xml", contentType, name + " content type");

            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            XDocument doc = XDocument.Parse(body);
            AssertHelper.AreEqual("Error", doc.Root.Name.LocalName, name + " root element");
            XElement code = doc.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "Code");
            AssertHelper.IsNotNull(code, name + " Code element");
            AssertHelper.AreEqual(expectedCode, code.Value, name + " error code");
        }
    }
}
