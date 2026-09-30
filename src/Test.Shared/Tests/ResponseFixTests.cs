namespace Test.Shared.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using S3ServerLibrary;
    using S3ServerLibrary.S3Objects;

    /// <summary>
    /// Positive and negative coverage for response fixes: the S3 XML namespace on the wire,
    /// 304 Not Modified responses, and removal of request-only default headers.
    /// </summary>
    public static class ResponseFixTests
    {
        private const string _S3Namespace = "http://s3.amazonaws.com/doc/2006-03-01/";

        /// <summary>
        /// Run all response fix tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            #region Namespace

            foreach (string path in new[] { "/", "/{bucket}?uploads", "/{bucket}?versioning", "/{bucket}?versions", "/{bucket}", "/{bucket}?acl", "/{bucket}?tagging" })
            {
                string captured = path;

                await runner.RunTestAsync("Response body is in the S3 namespace: " + captured, async (ct) =>
                {
                    HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + captured.Replace("{bucket}", server.Bucket), ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, captured);

                    XDocument doc = XDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    AssertHelper.AreEqual(_S3Namespace, doc.Root.Name.NamespaceName, "root namespace");

                    foreach (XElement element in doc.Root.Descendants())
                        AssertHelper.AreEqual(_S3Namespace, element.Name.NamespaceName, "namespace of " + element.Name.LocalName);
                }, token).ConfigureAwait(false);
            }

            await runner.RunTestAsync("Bucket logging response uses the S3 logging namespace", async (ct) =>
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "?logging", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "logging");

                XDocument doc = XDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                AssertHelper.AreEqual("http://doc.s3.amazonaws.com/2006-03-01", doc.Root.Name.NamespaceName, "logging namespace");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Error response body has no namespace", async (ct) =>
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/nonexistent-object-xyz.bin", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.NotFound, response, "missing object");

                XDocument doc = XDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                AssertHelper.AreEqual("Error", doc.Root.Name.LocalName, "root");
                AssertHelper.AreEqual("", doc.Root.Name.NamespaceName, "error namespace");
                AssertHelper.IsNull(doc.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "HttpStatusCode"), "no HttpStatusCode element");
            }, token).ConfigureAwait(false);

            #endregion

            #region Delete-Markers-And-Delete-Errors

            await runner.RunTestAsync("ListObjectVersions delete marker on the wire has only Key VersionId IsLatest LastModified Owner", async (ct) =>
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "?versions", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "ListObjectVersions");

                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringDoesNotContain(body, "nil", "no nil attributes");

                XElement root = XDocument.Parse(body).Root;
                XElement marker = root.Elements().Single(e => e.Name.LocalName == "DeleteMarker");
                AssertHelper.AreEqual("Key,VersionId,IsLatest,LastModified,Owner", String.Join(",", marker.Elements().Select(e => e.Name.LocalName)), "delete marker elements");

                XElement version = root.Elements().Single(e => e.Name.LocalName == "Version");
                AssertHelper.AreEqual("Key,VersionId,IsLatest,LastModified,ETag,Size,StorageClass,Owner", String.Join(",", version.Elements().Select(e => e.Name.LocalName)), "version elements");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("DeleteObjects per-key errors on the wire list Key and VersionId before Code and Message", async (ct) =>
            {
                Func<S3Context, DeleteMultiple, Task<DeleteResult>> original = server.Server.Object.DeleteMultiple;

                try
                {
                    server.Server.Object.DeleteMultiple = async (ctx, del) =>
                    {
                        Error denied = new Error(ErrorCode.AccessDenied, "denied.txt");
                        denied.Message = "Access Denied";
                        denied.RequestId = "not-per-key";
                        return new DeleteResult(
                            new List<Deleted> { new Deleted("ok.txt", null, null) },
                            new List<Error> { denied, new Error(ErrorCode.NoSuchVersion, "versioned.txt", "v1") });
                    };

                    string xml = "<Delete><Object><Key>ok.txt</Key></Object><Object><Key>denied.txt</Key></Object><Object><Key>versioned.txt</Key><VersionId>v1</VersionId></Object></Delete>";
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, server.BaseUrl + "/" + server.Bucket + "?delete");
                    request.Content = new StringContent(xml, System.Text.Encoding.UTF8, "application/xml");

                    HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "DeleteObjects");

                    XElement root = XDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false)).Root;
                    AssertHelper.AreEqual(_S3Namespace, root.Name.NamespaceName, "namespace");
                    XElement[] errors = root.Elements().Where(e => e.Name.LocalName == "Error").ToArray();
                    AssertHelper.AreEqual(2, errors.Length, "error count");
                    AssertHelper.AreEqual("Key,Code,Message", String.Join(",", errors[0].Elements().Select(e => e.Name.LocalName)), "first error elements");
                    AssertHelper.AreEqual("Access Denied", errors[0].Elements().Single(e => e.Name.LocalName == "Message").Value, "first error message");
                    AssertHelper.AreEqual("Key,VersionId,Code,Message", String.Join(",", errors[1].Elements().Select(e => e.Name.LocalName)), "second error elements");

                    Amazon.S3.Model.DeleteObjectsRequest sdkRequest = new Amazon.S3.Model.DeleteObjectsRequest { BucketName = server.Bucket };
                    sdkRequest.AddKey("denied.txt");
                    Amazon.S3.DeleteObjectsException thrown = null;
                    try
                    {
                        await server.S3Client.DeleteObjectsAsync(sdkRequest, ct).ConfigureAwait(false);
                    }
                    catch (Amazon.S3.DeleteObjectsException ex)
                    {
                        thrown = ex;
                    }

                    AssertHelper.IsNotNull(thrown, "AWS SDK reports per-key errors");
                    AssertHelper.AreEqual(2, thrown.Response.DeleteErrors.Count, "AWS SDK error count");
                    AssertHelper.AreEqual("denied.txt", thrown.Response.DeleteErrors[0].Key, "AWS SDK error key");
                    AssertHelper.AreEqual("AccessDenied", thrown.Response.DeleteErrors[0].Code, "AWS SDK error code");
                    AssertHelper.AreEqual("v1", thrown.Response.DeleteErrors[1].VersionId, "AWS SDK error version");
                }
                finally
                {
                    server.Server.Object.DeleteMultiple = original;
                }
            }, token).ConfigureAwait(false);

            #endregion

            #region Not-Modified

            await runner.RunTestAsync("NotModified from Object.Read returns 304 with no body and the ETag intact", async (ct) =>
            {
                server.Server.Object.Read = async (ctx) =>
                {
                    ctx.Response.Headers.Add("ETag", "\"abc\"");
                    ctx.Response.Headers.Add("Last-Modified", "Sun, 06 Nov 1994 08:49:37 GMT");
                    if (ctx.Request.IfNoneMatch != null && ctx.Request.IfNoneMatch.Contains("\"abc\""))
                        throw new S3Exception(new Error(ErrorCode.NotModified));
                    return new S3Object(ctx.Request.Key, "1", true, DateTime.UtcNow, "abc", 5, null, "hello", "text/plain");
                };

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/conditional.txt");
                request.Headers.TryAddWithoutValidation("If-None-Match", "\"abc\"");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.NotModified, response, "GET 304");

                byte[] body = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                AssertHelper.AreEqual(0, body.Length, "empty body");
                AssertHelper.IsNull(response.Content.Headers.ContentType, "no Content-Type");
                AssertHelper.AreEqual("\"abc\"", response.Headers.ETag != null ? response.Headers.ETag.Tag : null, "ETag preserved");
                AssertHelper.IsNotNull(response.Content.Headers.LastModified, "Last-Modified preserved");

                HttpRequestMessage unconditional = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/conditional.txt");
                HttpResponseMessage ok = await server.HttpClient.SendAsync(unconditional, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, ok, "unconditional GET");
                AssertHelper.AreEqual("hello", await ok.Content.ReadAsStringAsync().ConfigureAwait(false), "unconditional body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("NotModified from Object.Exists returns 304 for HEAD", async (ct) =>
            {
                server.Server.Object.Exists = async (ctx) =>
                {
                    ctx.Response.Headers.Add("ETag", "\"abc\"");
                    if (ctx.Request.IfModifiedSince != null)
                        throw new S3Exception(new Error(ErrorCode.NotModified));
                    return new ObjectMetadata(ctx.Request.Key, DateTime.UtcNow, "abc", 5, null);
                };

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, server.BaseUrl + "/" + server.Bucket + "/conditional.txt");
                request.Headers.TryAddWithoutValidation("If-Modified-Since", DateTime.UtcNow.ToString("r"));

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.NotModified, response, "HEAD 304");
                AssertHelper.AreEqual("\"abc\"", response.Headers.ETag != null ? response.Headers.ETag.Tag : null, "ETag preserved");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("PreconditionFailed still returns 412 with an XML body", async (ct) =>
            {
                server.Server.Object.Read = async (ctx) =>
                {
                    throw new S3Exception(new Error(ErrorCode.PreconditionFailed));
                };

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/conditional.txt");
                request.Headers.TryAddWithoutValidation("If-Match", "\"other\"");

                HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.PreconditionFailed, response, "412");
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AssertHelper.StringContains(body, "<Code>PreconditionFailed</Code>", "412 body");
            }, token).ConfigureAwait(false);

            #endregion

            #region Default-Headers

            await runner.RunTestAsync("GET and HEAD responses do not echo request-only headers", async (ct) =>
            {
                HttpResponseMessage get = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/test-object.txt", ct).ConfigureAwait(false);
                HttpResponseMessage head = await server.HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, server.BaseUrl + "/" + server.Bucket + "/test-object.txt"), ct).ConfigureAwait(false);

                foreach (HttpResponseMessage response in new[] { get, head })
                {
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "response status");
                    HashSet<string> names = HeaderNames(response);
                    foreach (string removed in new[] { "Host", "Accept", "Accept-Language", "Cache-Control" })
                        AssertHelper.IsFalse(names.Contains(removed), "response should not contain " + removed);

                    foreach (string removed in new[] { "Accept-Charset", "Access-Control-Allow-Origin", "Access-Control-Allow-Methods" })
                        AssertHelper.IsFalse(names.Contains(removed), "response should not contain " + removed);
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Callback Cache-Control appears exactly once", async (ct) =>
            {
                server.Server.Object.Read = async (ctx) =>
                {
                    ctx.Response.Headers.Add("Cache-Control", "max-age=60");
                    return new S3Object(ctx.Request.Key, "1", true, DateTime.UtcNow, "abc", 5, null, "hello", "text/plain");
                };

                HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/cached.txt", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "cached GET");

                List<string> values = HeaderValues(response, "Cache-Control");
                AssertHelper.AreEqual(1, values.Count, "Cache-Control count");
                AssertHelper.AreEqual("max-age=60", values[0], "Cache-Control value");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("PreserveWebserverDefaultHeaders restores the previous header set", async (ct) =>
            {
                int port = S3TestServer.GetAvailablePort();
                S3ServerSettings settings = new S3ServerSettings();
                settings.Webserver.Hostname = "127.0.0.1";
                settings.Webserver.Port = port;
                settings.Webserver.Ssl.Enable = false;
                settings.PreserveWebserverDefaultHeaders = true;

                using (S3Server legacy = new S3Server(settings))
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.ConnectionClose = true;
                    legacy.Service.ListBuckets = async (ctx) => new ListAllMyBucketsResult();
                    legacy.Start();

                    HttpResponseMessage response = await client.GetAsync("http://127.0.0.1:" + port + "/", ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "legacy ListBuckets");

                    HashSet<string> names = HeaderNames(response);
                    AssertHelper.IsTrue(names.Contains("Cache-Control"), "Cache-Control restored");
                    AssertHelper.IsTrue(names.Contains("Accept-Language"), "Accept-Language restored");

                    legacy.Stop();
                }
            }, token).ConfigureAwait(false);

            #endregion
        }

        private static HashSet<string> HeaderNames(HttpResponseMessage response)
        {
            HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers) names.Add(header.Key);
            foreach (KeyValuePair<string, IEnumerable<string>> header in response.Content.Headers) names.Add(header.Key);
            return names;
        }

        private static List<string> HeaderValues(HttpResponseMessage response, string name)
        {
            List<string> values = new List<string>();
            if (response.Headers.TryGetValues(name, out IEnumerable<string> a)) values.AddRange(a);
            if (response.Content.Headers.TryGetValues(name, out IEnumerable<string> b)) values.AddRange(b);
            return values;
        }
    }
}
