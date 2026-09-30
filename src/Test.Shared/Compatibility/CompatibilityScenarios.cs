namespace Test.Shared.Compatibility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Wire-level S3 compatibility scenarios.  Every expectation was recorded from Amazon S3, so the same scenarios can run
    /// against Amazon S3 (Test.Compatibility --target s3) to confirm the expectations, and against S3Server with the
    /// reference backend (the Compatibility suite) to confirm S3Server matches.
    /// Scenarios only create, modify, and delete objects under the target's prefix; bucket settings are never changed.
    /// </summary>
    public static class CompatibilityScenarios
    {
        #region Private-Members

        private const string _S3Namespace = "http://s3.amazonaws.com/doc/2006-03-01/";
        private const string _Ten = "0123456789";
        private static readonly Regex _S3Timestamp = new Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$", RegexOptions.Compiled);

        private static readonly string[] _ListKeys = new[] { "list/a.txt", "list/b.txt", "list/sub/c.txt", "list/sub/d.txt", "list/z.txt" };

        // Key names under enc/ and the form Amazon S3 returns for each with encoding-type=url.
        private static readonly Dictionary<string, string> _EncodedKeys = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "enc/a b+c&d=e?f#g%h~é.txt", "enc/a+b%2Bc%26d%3De%3Ff%23g%25h%7E%C3%A9.txt" },
            { "enc/semi;colon,comma:@$!*'()[].txt", "enc/semi%3Bcolon%2Ccomma%3A%40%24%21*%27%28%29%5B%5D.txt" },
            { "enc/tab\tkey.txt", "enc/tab%09key.txt" },
            { "enc/sp ace/x.txt", "enc/sp+ace/x.txt" }
        };

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create fixtures, run every scenario, and remove everything under the target's prefix.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="target">Target.  Cannot be null.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public static async Task RunAllAsync(TestRunner runner, CompatTarget target, CancellationToken token = default)
        {
            if (runner == null) throw new ArgumentNullException(nameof(runner));
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (!runner.DiscoveryOnly) await CreateFixtures(target, token).ConfigureAwait(false);

            try
            {
                await Ranges(runner, target, token).ConfigureAwait(false);
                await Parameters(runner, target, token).ConfigureAwait(false);
                await Listings(runner, target, token).ConfigureAwait(false);
                await Encoding(runner, target, token).ConfigureAwait(false);
                await Conditionals(runner, target, token).ConfigureAwait(false);
                await Headers(runner, target, token).ConfigureAwait(false);
                await Copies(runner, target, token).ConfigureAwait(false);
                await Multipart(runner, target, token).ConfigureAwait(false);
                await Deletes(runner, target, token).ConfigureAwait(false);
                await AclsTagsAndErrors(runner, target, token).ConfigureAwait(false);
            }
            finally
            {
                if (!runner.DiscoveryOnly) await target.CleanupAsync(CancellationToken.None).ConfigureAwait(false);
            }
        }

        #endregion

        #region Scenario-Groups

        private static async Task Ranges(TestRunner runner, CompatTarget t, CancellationToken token)
        {
            // Range, expected status, expected Content-Range (null when absent), expected body.
            string[][] cases = new[]
            {
                new[] { "bytes=-3", "206", "bytes 7-9/10", "789" },
                new[] { "bytes=2-4", "206", "bytes 2-4/10", "234" },
                new[] { "bytes=9-", "206", "bytes 9-9/10", "9" },
                new[] { "bytes=-50", "206", "bytes 0-9/10", _Ten },
                new[] { "bytes=0-100", "206", "bytes 0-9/10", _Ten },
                new[] { "bytes=5-2", "200", null, _Ten },
                new[] { "bytes=abc", "200", null, _Ten },
                new[] { "bytes=0-1,4-5", "200", null, _Ten },
                new[] { "items=0-1", "200", null, _Ten },
                new[] { "bytes=-", "200", null, _Ten }
            };

            foreach (string[] c in cases)
            {
                string[] captured = c;
                await runner.RunTestAsync("GET Range " + captured[0] + " returns " + captured[1], async (ct) =>
                {
                    CompatResponse r = await Get(t, t.Key("ten.txt"), ct, "Range", captured[0]).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(Int32.Parse(captured[1]), r.StatusCode, "status");
                    AssertHelper.AreEqual(captured[2], r.Header("Content-Range"), "Content-Range");
                    AssertHelper.AreEqual(captured[3], r.Text, "body");
                    AssertHelper.AreEqual(captured[3].Length.ToString(), r.Header("Content-Length"), "Content-Length");
                }, token).ConfigureAwait(false);
            }

            foreach (string range in new[] { "bytes=-0", "bytes=100-200", "bytes=10-" })
            {
                string captured = range;
                await runner.RunTestAsync("GET Range " + captured + " returns 416 InvalidRange with details", async (ct) =>
                {
                    CompatResponse r = await Get(t, t.Key("ten.txt"), ct, "Range", captured).ConfigureAwait(false);
                    ExpectError(r, 416, "InvalidRange");
                    AssertHelper.AreEqual(captured, r.Element("RangeRequested"), "RangeRequested");
                    AssertHelper.AreEqual("10", r.Element("ActualObjectSize"), "ActualObjectSize");
                }, token).ConfigureAwait(false);
            }

            await runner.RunTestAsync("GET suffix Range on an empty object returns 200 with an empty body", async (ct) =>
            {
                CompatResponse r = await Get(t, t.Key("empty.txt"), ct, "Range", "bytes=-3").ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "status");
                AssertHelper.IsNull(r.Header("Content-Range"), "Content-Range");
                AssertHelper.AreEqual(0, r.Body.Length, "body length");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("GET Range bytes=0-0 on an empty object returns 416", async (ct) =>
            {
                CompatResponse r = await Get(t, t.Key("empty.txt"), ct, "Range", "bytes=0-0").ConfigureAwait(false);
                ExpectError(r, 416, "InvalidRange");
                AssertHelper.AreEqual("0", r.Element("ActualObjectSize"), "ActualObjectSize");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("HEAD Range returns 206 with Content-Range and range length", async (ct) =>
            {
                CompatResponse r = await Send(t, HttpMethod.Head, t.Key("ten.txt"), ct, "Range", "bytes=2-4").ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(206, r.StatusCode, "status");
                AssertHelper.AreEqual("bytes 2-4/10", r.Header("Content-Range"), "Content-Range");
                AssertHelper.AreEqual("3", r.Header("Content-Length"), "Content-Length");

                r = await Send(t, HttpMethod.Head, t.Key("ten.txt"), ct, "Range", "bytes=-3").ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(206, r.StatusCode, "suffix status");
                AssertHelper.AreEqual("bytes 7-9/10", r.Header("Content-Range"), "suffix Content-Range");
                AssertHelper.AreEqual("3", r.Header("Content-Length"), "suffix Content-Length");

                r = await Send(t, HttpMethod.Head, t.Key("ten.txt"), ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "no range status");
                AssertHelper.AreEqual("10", r.Header("Content-Length"), "no range Content-Length");
                AssertHelper.IsNull(r.Header("Content-Range"), "no range Content-Range");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("HEAD unsatisfiable Range returns 416 with no body", async (ct) =>
            {
                CompatResponse r = await Send(t, HttpMethod.Head, t.Key("ten.txt"), ct, "Range", "bytes=50-60").ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(416, r.StatusCode, "status");
                AssertHelper.AreEqual(0, r.Body.Length, "body length");
            }, token).ConfigureAwait(false);
        }

        private static async Task Parameters(TestRunner runner, CompatTarget t, CancellationToken token)
        {
            foreach (string q in new[] { "max-keys=abc", "max-keys=-1", "max-parts=abc", "part-number-marker=x" })
            {
                string captured = q;
                await runner.RunTestAsync("GET object ignores listing parameter " + captured, async (ct) =>
                {
                    string[] pair = captured.Split('=');
                    CompatResponse r = await t.SendAsync(HttpMethod.Get, t.Key("ten.txt"), CompatTarget.Query(pair[0], pair[1]), token: ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(200, r.StatusCode, "status");
                    AssertHelper.AreEqual(_Ten, r.Text, "body");
                }, token).ConfigureAwait(false);
            }

            foreach (string value in new[] { "x", "0", "10001", "-1" })
            {
                string captured = value;
                await runner.RunTestAsync("GET object partNumber=" + captured + " returns InvalidArgument", async (ct) =>
                {
                    CompatResponse r = await t.SendAsync(HttpMethod.Get, t.Key("ten.txt"), CompatTarget.Query("partNumber", captured), token: ct).ConfigureAwait(false);
                    ExpectError(r, 400, "InvalidArgument", "partNumber", captured);
                    AssertHelper.AreEqual("Part number must be an integer between 1 and 10000, inclusive", r.Element("Message"), "message");
                }, token).ConfigureAwait(false);
            }

            // Query pairs, expected status, expected ArgumentName, expected ArgumentValue.
            string[][] listCases = new[]
            {
                new[] { "max-keys", "-1", "400", "maxKeys", "-1" },
                new[] { "max-keys", "abc", "400", "max-keys", "abc" },
                new[] { "max-keys", "99999999999", "400", "max-keys", "99999999999" },
                new[] { "max-keys", "", "200", null, null },
                new[] { "encoding-type", "xyz", "400", "encoding-type", "xyz" }
            };

            foreach (string[] c in listCases)
            {
                string[] captured = c;
                await runner.RunTestAsync("ListObjects " + captured[0] + "=" + captured[1] + " returns " + captured[2], async (ct) =>
                {
                    CompatResponse r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("prefix", t.Prefix, captured[0], captured[1]), token: ct).ConfigureAwait(false);
                    if (captured[2] == "200") AssertHelper.StatusCodeEquals(200, r.StatusCode, "status");
                    else ExpectError(r, 400, "InvalidArgument", captured[3], captured[4]);
                }, token).ConfigureAwait(false);
            }

            await runner.RunTestAsync("ListObjectsV2, ListObjectVersions, and ListMultipartUploads validate their numeric parameters", async (ct) =>
            {
                CompatResponse r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("list-type", "2", "prefix", t.Prefix, "max-keys", "abc"), token: ct).ConfigureAwait(false);
                ExpectError(r, 400, "InvalidArgument", "max-keys", "abc");

                r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("versions", null, "prefix", t.Prefix, "max-keys", "abc"), token: ct).ConfigureAwait(false);
                ExpectError(r, 400, "InvalidArgument", "max-keys", "abc");

                r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("uploads", null, "prefix", t.Prefix, "max-uploads", "abc"), token: ct).ConfigureAwait(false);
                ExpectError(r, 400, "InvalidArgument", "max-uploads", "abc");

                r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("versions", null, "prefix", t.Prefix, "max-keys", "-1"), token: ct).ConfigureAwait(false);
                ExpectError(r, 400, "InvalidArgument", "max-keys");
                AssertHelper.AreEqual("max-keys cannot be negative", r.Element("Message"), "versions message");
                AssertHelper.IsNull(r.Element("ArgumentValue"), "versions ArgumentValue");

                r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("uploads", null, "prefix", t.Prefix, "max-uploads", "-1"), token: ct).ConfigureAwait(false);
                ExpectError(r, 400, "InvalidArgument", "max-uploads", "-1");
                AssertHelper.AreEqual("Argument max-uploads must be an integer between 0 and 2147483647", r.Element("Message"), "max-uploads message");

                r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("uploads", null, "prefix", t.Prefix, "max-uploads", "0"), token: ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "max-uploads=0 accepted");
            }, token).ConfigureAwait(false);
        }

        private static async Task Listings(TestRunner runner, CompatTarget t, CancellationToken token)
        {
            await runner.RunTestAsync("ListObjects v1 shape: empty Marker, no KeyCount, Owner, millisecond timestamps", async (ct) =>
            {
                CompatResponse r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("prefix", t.Key("list/"), "max-keys", "1"), token: ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "status");
                AssertHelper.AreEqual(_S3Namespace, r.Xml.Root.Name.NamespaceName, "namespace");
                AssertHelper.IsTrue(r.HasRootChild("Marker"), "Marker present");
                AssertHelper.AreEqual("", r.Element("Marker"), "Marker empty");
                AssertHelper.IsFalse(r.HasRootChild("KeyCount"), "no KeyCount");
                AssertHelper.IsFalse(r.HasRootChild("NextMarker"), "no NextMarker without delimiter");
                AssertHelper.AreEqual("true", r.Element("IsTruncated"), "IsTruncated");
                AssertHelper.AreEqual(1, r.Elements("Owner", "Contents").Count, "Owner in Contents");
                AssertHelper.IsTrue(_S3Timestamp.IsMatch(r.Element("LastModified")), "LastModified format " + r.Element("LastModified"));
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListObjectsV2 shape: KeyCount, no Marker, no Owner without fetch-owner", async (ct) =>
            {
                CompatResponse r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("list-type", "2", "prefix", t.Key("list/"), "max-keys", "1"), token: ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "status");
                AssertHelper.AreEqual("1", r.Element("KeyCount"), "KeyCount");
                AssertHelper.IsFalse(r.HasRootChild("Marker"), "no Marker");
                AssertHelper.IsTrue(r.HasRootChild("NextContinuationToken"), "NextContinuationToken");
                AssertHelper.AreEqual(0, r.Elements("Owner", "Contents").Count, "no Owner");

                r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("list-type", "2", "prefix", t.Key("list/"), "max-keys", "1", "fetch-owner", "true"), token: ct).ConfigureAwait(false);
                AssertHelper.AreEqual(1, r.Elements("Owner", "Contents").Count, "Owner with fetch-owner");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListObjectsV2 start-after is echoed and honored", async (ct) =>
            {
                CompatResponse r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("list-type", "2", "prefix", t.Key("list/"), "start-after", t.Key("list/a.txt")), token: ct).ConfigureAwait(false);
                AssertHelper.AreEqual(t.Key("list/a.txt"), r.Element("StartAfter"), "StartAfter");
                AssertHelper.AreEqual(t.Key("list/b.txt"), r.Elements("Key", "Contents").First(), "first key");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListObjects v1 with delimiter pages with NextMarker across a common prefix", async (ct) =>
            {
                CompatResponse first = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("prefix", t.Key("list/"), "delimiter", "/", "max-keys", "2"), token: ct).ConfigureAwait(false);
                AssertHelper.AreEqual(Join(t.Key("list/a.txt"), t.Key("list/b.txt")), Join(first.Elements("Key", "Contents").ToArray()), "first page keys");
                AssertHelper.AreEqual("true", first.Element("IsTruncated"), "first page truncated");
                AssertHelper.AreEqual(t.Key("list/b.txt"), first.Element("NextMarker"), "NextMarker");

                CompatResponse second = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("prefix", t.Key("list/"), "delimiter", "/", "max-keys", "2", "marker", first.Element("NextMarker")), token: ct).ConfigureAwait(false);
                AssertHelper.AreEqual(t.Key("list/b.txt"), second.Element("Marker"), "Marker echo");
                AssertHelper.AreEqual(t.Key("list/sub/"), Join(second.Elements("Prefix", "CommonPrefixes").ToArray()), "common prefixes");
                AssertHelper.AreEqual(t.Key("list/z.txt"), Join(second.Elements("Key", "Contents").ToArray()), "second page keys");
                AssertHelper.AreEqual("false", second.Element("IsTruncated"), "second page complete");
                AssertHelper.IsFalse(second.HasRootChild("NextMarker"), "no NextMarker on last page");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListObjectsV2 continuation tokens walk every key in order", async (ct) =>
            {
                List<string> keys = new List<string>();
                string continuation = null;
                for (int page = 0; page < 10; page++)
                {
                    List<KeyValuePair<string, string>> q = CompatTarget.Query("list-type", "2", "prefix", t.Key("list/"), "max-keys", "2");
                    if (continuation != null) q.Add(new KeyValuePair<string, string>("continuation-token", continuation));
                    CompatResponse r = await t.SendAsync(HttpMethod.Get, null, q, token: ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(200, r.StatusCode, "page status");
                    if (continuation != null) AssertHelper.AreEqual(continuation, r.Element("ContinuationToken"), "ContinuationToken echo");
                    keys.AddRange(r.Elements("Key", "Contents"));
                    continuation = r.Element("IsTruncated") == "true" ? r.Element("NextContinuationToken") : null;
                    if (continuation == null) break;
                }

                AssertHelper.AreEqual(Join(_ListKeys.Select(k => t.Key(k)).ToArray()), Join(keys.ToArray()), "all keys");
            }, token).ConfigureAwait(false);
        }

        private static async Task Encoding(TestRunner runner, CompatTarget t, CancellationToken token)
        {
            await runner.RunTestAsync("encoding-type=url encodes keys the way Amazon S3 does", async (ct) =>
            {
                CompatResponse r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("prefix", t.Key("enc/"), "encoding-type", "url"), token: ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "status");
                AssertHelper.AreEqual("url", r.Element("EncodingType"), "EncodingType");
                string expected = Join(_EncodedKeys.Values.Select(v => t.Prefix + v).OrderBy(v => v, StringComparer.Ordinal).ToArray());
                AssertHelper.AreEqual(expected, Join(r.Elements("Key", "Contents").OrderBy(v => v, StringComparer.Ordinal).ToArray()), "encoded keys");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("encoding-type=url encodes Delimiter, CommonPrefixes, and NextMarker", async (ct) =>
            {
                CompatResponse r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("prefix", t.Key("enc/"), "delimiter", " ", "encoding-type", "url"), token: ct).ConfigureAwait(false);
                AssertHelper.AreEqual("+", r.Element("Delimiter"), "Delimiter");
                AssertHelper.IsTrue(r.Elements("Prefix", "CommonPrefixes").Contains(t.Key("enc/a+")), "encoded common prefix");

                r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("prefix", t.Key("enc/"), "delimiter", "/", "max-keys", "1", "encoding-type", "url"), token: ct).ConfigureAwait(false);
                AssertHelper.AreEqual(t.Prefix + _EncodedKeys["enc/a b+c&d=e?f#g%h~é.txt"], r.Element("NextMarker"), "encoded NextMarker");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListObjectsV2 encoding-type=url encodes StartAfter", async (ct) =>
            {
                CompatResponse r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("list-type", "2", "prefix", t.Key("enc/"), "start-after", t.Key("enc/a b"), "encoding-type", "url"), token: ct).ConfigureAwait(false);
                AssertHelper.AreEqual(t.Key("enc/a+b"), r.Element("StartAfter"), "StartAfter");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Listing without encoding-type returns raw keys", async (ct) =>
            {
                CompatResponse r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("prefix", t.Key("enc/")), token: ct).ConfigureAwait(false);
                AssertHelper.IsFalse(r.HasRootChild("EncodingType"), "no EncodingType");
                string expected = Join(_EncodedKeys.Keys.Select(k => t.Key(k)).OrderBy(v => v, StringComparer.Ordinal).ToArray());
                AssertHelper.AreEqual(expected, Join(r.Elements("Key", "Contents").OrderBy(v => v, StringComparer.Ordinal).ToArray()), "raw keys");
            }, token).ConfigureAwait(false);
        }

        private static async Task Conditionals(TestRunner runner, CompatTarget t, CancellationToken token)
        {
            await runner.RunTestAsync("If-None-Match with the current ETag returns 304 without a body", async (ct) =>
            {
                string etag = (await Send(t, HttpMethod.Head, t.Key("ten.txt"), ct).ConfigureAwait(false)).Header("ETag");
                CompatResponse r = await Get(t, t.Key("ten.txt"), ct, "If-None-Match", etag).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(304, r.StatusCode, "GET status");
                AssertHelper.AreEqual(0, r.Body.Length, "body length");
                AssertHelper.AreEqual(etag, r.Header("ETag"), "ETag");
                AssertHelper.IsNotNull(r.Header("Last-Modified"), "Last-Modified");
                AssertHelper.IsNull(r.Header("Content-Type"), "no Content-Type");

                r = await Send(t, HttpMethod.Head, t.Key("ten.txt"), ct, "If-None-Match", etag).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(304, r.StatusCode, "HEAD status");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Failed If-Match returns 412 PreconditionFailed naming the condition", async (ct) =>
            {
                CompatResponse r = await Get(t, t.Key("ten.txt"), ct, "If-Match", "\"nope\"").ConfigureAwait(false);
                ExpectError(r, 412, "PreconditionFailed");
                AssertHelper.AreEqual("If-Match", r.Element("Condition"), "Condition");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Matching If-Match overrides a failed If-Unmodified-Since", async (ct) =>
            {
                string etag = (await Send(t, HttpMethod.Head, t.Key("ten.txt"), ct).ConfigureAwait(false)).Header("ETag");
                CompatResponse r = await Get(t, t.Key("ten.txt"), ct, "If-Match", etag, "If-Unmodified-Since", "Sun, 06 Nov 1994 08:49:37 GMT").ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "status");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Non-matching If-None-Match overrides If-Modified-Since", async (ct) =>
            {
                CompatResponse r = await Get(t, t.Key("ten.txt"), ct, "If-None-Match", "\"nope\"", "If-Modified-Since", "Sun, 06 Nov 2094 08:49:37 GMT").ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "status");
            }, token).ConfigureAwait(false);
        }

        private static async Task Headers(TestRunner runner, CompatTarget t, CancellationToken token)
        {
            await runner.RunTestAsync("GET response carries S3 headers and no request-only or CORS headers", async (ct) =>
            {
                CompatResponse r = await Get(t, t.Key("ten.txt"), ct, "Origin", "http://example.com").ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "status");
                AssertHelper.IsNotNull(r.Header("x-amz-request-id"), "x-amz-request-id");
                AssertHelper.IsNotNull(r.Header("ETag"), "ETag");
                AssertHelper.IsNotNull(r.Header("Last-Modified"), "Last-Modified");
                AssertHelper.AreEqual("bytes", r.Header("Accept-Ranges"), "Accept-Ranges");
                AssertHelper.AreEqual("10", r.Header("Content-Length"), "Content-Length");

                foreach (string absent in new[] { "Host", "Accept", "Accept-Language", "Accept-Charset", "Cache-Control", "Access-Control-Allow-Origin", "Access-Control-Allow-Methods" })
                    AssertHelper.IsNull(r.Header(absent), "absent " + absent);
            }, token).ConfigureAwait(false);
        }

        private static async Task Copies(TestRunner runner, CompatTarget t, CancellationToken token)
        {
            await runner.RunTestAsync("CopyObject returns a namespaced CopyObjectResult and copies the data", async (ct) =>
            {
                string etag = (await Send(t, HttpMethod.Head, t.Key("ten.txt"), ct).ConfigureAwait(false)).Header("ETag");
                CompatResponse r = await Send(t, HttpMethod.Put, t.Key("copy1.txt"), ct, "x-amz-copy-source", "/" + t.Bucket + "/" + SigV4Signer.UriEncode(t.Key("ten.txt"), true)).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "status");
                AssertHelper.AreEqual("CopyObjectResult", r.Xml.Root.Name.LocalName, "root");
                AssertHelper.AreEqual(_S3Namespace, r.Xml.Root.Name.NamespaceName, "namespace");
                AssertHelper.AreEqual(etag, r.Element("ETag"), "ETag");
                AssertHelper.IsTrue(_S3Timestamp.IsMatch(r.Element("LastModified")), "LastModified format " + r.Element("LastModified"));
                AssertHelper.AreEqual(_Ten, (await Get(t, t.Key("copy1.txt"), ct).ConfigureAwait(false)).Text, "copied data");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("CopyObject accepts a percent-encoded separator slash", async (ct) =>
            {
                CompatResponse r = await Send(t, HttpMethod.Put, t.Key("copy2.txt"), ct, "x-amz-copy-source", t.Bucket + "%2F" + SigV4Signer.UriEncode(t.Key("ten.txt"), true)).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "status");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("CopyObject rejects malformed sources with InvalidArgument", async (ct) =>
            {
                CompatResponse r = await Send(t, HttpMethod.Put, t.Key("copy3.txt"), ct, "x-amz-copy-source", "no-slash").ConfigureAwait(false);
                ExpectError(r, 400, "InvalidArgument", "x-amz-copy-source");

                r = await Send(t, HttpMethod.Put, t.Key("copy3.txt"), ct, "x-amz-copy-source", t.Bucket + "/" + SigV4Signer.UriEncode(t.Key("ten.txt"), true) + "?versionId=").ConfigureAwait(false);
                ExpectError(r, 400, "InvalidArgument", "x-amz-copy-source");
                AssertHelper.AreEqual("Version id cannot be the empty string", r.Element("Message"), "message");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("CopyObject from a missing source returns NoSuchKey", async (ct) =>
            {
                CompatResponse r = await Send(t, HttpMethod.Put, t.Key("copy4.txt"), ct, "x-amz-copy-source", t.Bucket + "/" + SigV4Signer.UriEncode(t.Key("missing.txt"), true)).ConfigureAwait(false);
                ExpectError(r, 404, "NoSuchKey");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("CopyObject with a failed x-amz-copy-source-if-match returns 412", async (ct) =>
            {
                CompatResponse r = await Send(t, HttpMethod.Put, t.Key("copy5.txt"), ct,
                    "x-amz-copy-source", t.Bucket + "/" + SigV4Signer.UriEncode(t.Key("ten.txt"), true),
                    "x-amz-copy-source-if-match", "\"nope\"").ConfigureAwait(false);
                ExpectError(r, 412, "PreconditionFailed");
            }, token).ConfigureAwait(false);
        }

        private static async Task Multipart(TestRunner runner, CompatTarget t, CancellationToken token)
        {
            await runner.RunTestAsync("Multipart upload parts, part copy, listing, and abort", async (ct) =>
            {
                string key = t.Key("mp-abort.bin");
                string uploadId = await CreateUpload(t, key, ct).ConfigureAwait(false);

                CompatResponse r = await t.SendAsync(HttpMethod.Put, key, CompatTarget.Query("partNumber", "1", "uploadId", uploadId), body: System.Text.Encoding.ASCII.GetBytes("AAAAAAAAAA"), token: ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "UploadPart status");
                AssertHelper.IsNotNull(r.Header("ETag"), "UploadPart ETag");

                r = await t.SendAsync(HttpMethod.Put, key, CompatTarget.Query("partNumber", "2", "uploadId", uploadId),
                    Headers("x-amz-copy-source", t.Bucket + "/" + SigV4Signer.UriEncode(t.Key("ten.txt"), true), "x-amz-copy-source-range", "bytes=0-4"), token: ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "UploadPartCopy status");
                AssertHelper.AreEqual("CopyPartResult", r.Xml.Root.Name.LocalName, "CopyPartResult root");
                AssertHelper.IsNotNull(r.Element("ETag"), "CopyPartResult ETag");

                r = await t.SendAsync(HttpMethod.Get, key, CompatTarget.Query("uploadId", uploadId), token: ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "ListParts status");
                AssertHelper.AreEqual("0", r.Element("PartNumberMarker"), "PartNumberMarker default");
                AssertHelper.AreEqual("1,2", String.Join(",", r.Elements("PartNumber", "Part")), "part numbers");

                r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("uploads", null, "prefix", t.Key("mp-abort")), token: ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "ListMultipartUploads status");
                AssertHelper.IsTrue(r.Elements("UploadId", "Upload").Contains(uploadId), "upload listed");

                r = await t.SendAsync(HttpMethod.Delete, key, CompatTarget.Query("uploadId", uploadId), token: ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(204, r.StatusCode, "Abort status");

                r = await t.SendAsync(HttpMethod.Get, key, CompatTarget.Query("uploadId", uploadId), token: ct).ConfigureAwait(false);
                ExpectError(r, 404, "NoSuchUpload");
                AssertHelper.AreEqual(uploadId, r.Element("UploadId"), "UploadId detail");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Multipart parameter validation matches Amazon S3", async (ct) =>
            {
                string key = t.Key("mp-invalid.bin");
                string uploadId = await CreateUpload(t, key, ct).ConfigureAwait(false);

                try
                {
                    foreach (string partNumber in new[] { "0", "10001" })
                    {
                        CompatResponse bad = await t.SendAsync(HttpMethod.Put, key, CompatTarget.Query("partNumber", partNumber, "uploadId", uploadId), body: new byte[] { 1 }, token: ct).ConfigureAwait(false);
                        ExpectError(bad, 400, "InvalidArgument", "partNumber", partNumber);
                    }

                    CompatResponse r = await t.SendAsync(HttpMethod.Put, key, CompatTarget.Query("partNumber", "1", "uploadId", uploadId),
                        Headers("x-amz-copy-source", t.Bucket + "/" + SigV4Signer.UriEncode(t.Key("ten.txt"), true), "x-amz-copy-source-range", "bytes=5-"), token: ct).ConfigureAwait(false);
                    ExpectError(r, 400, "InvalidArgument", "x-amz-copy-source-range", "bytes=5-");

                    r = await t.SendAsync(HttpMethod.Get, key, CompatTarget.Query("uploadId", uploadId, "max-parts", "abc"), token: ct).ConfigureAwait(false);
                    ExpectError(r, 400, "InvalidArgument", "max-parts", "abc");

                    r = await t.SendAsync(HttpMethod.Get, key, CompatTarget.Query("uploadId", uploadId, "part-number-marker", "abc"), token: ct).ConfigureAwait(false);
                    ExpectError(r, 400, "InvalidArgument", "part-number-marker", "abc");

                    r = await t.SendAsync(HttpMethod.Get, key, CompatTarget.Query("uploadId", uploadId, "max-parts", "-5"), token: ct).ConfigureAwait(false);
                    ExpectError(r, 400, "InvalidArgument", "max-parts", "-5");
                    AssertHelper.AreEqual("Argument max-parts must be an integer between 0 and 2147483647", r.Element("Message"), "max-parts message");

                    r = await t.SendAsync(HttpMethod.Get, key, CompatTarget.Query("uploadId", uploadId, "part-number-marker", "-1"), token: ct).ConfigureAwait(false);
                    ExpectError(r, 400, "InvalidArgument", "part-number-marker", "-1");
                }
                finally
                {
                    await t.SendAsync(HttpMethod.Delete, key, CompatTarget.Query("uploadId", uploadId), token: CancellationToken.None).ConfigureAwait(false);
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Multipart upload completes with a single part", async (ct) =>
            {
                string key = t.Key("mp-complete.bin");
                string uploadId = await CreateUpload(t, key, ct).ConfigureAwait(false);
                CompatResponse part = await t.SendAsync(HttpMethod.Put, key, CompatTarget.Query("partNumber", "1", "uploadId", uploadId), body: System.Text.Encoding.ASCII.GetBytes("multipart-data"), token: ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, part.StatusCode, "UploadPart status");

                string complete = "<CompleteMultipartUpload><Part><PartNumber>1</PartNumber><ETag>" + part.Header("ETag") + "</ETag></Part></CompleteMultipartUpload>";
                CompatResponse r = await t.SendAsync(HttpMethod.Post, key, CompatTarget.Query("uploadId", uploadId), Headers("Content-Type", "application/xml"), System.Text.Encoding.UTF8.GetBytes(complete), ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "Complete status");
                AssertHelper.AreEqual("CompleteMultipartUploadResult", r.Xml.Root.Name.LocalName, "Complete root");
                AssertHelper.AreEqual("multipart-data", (await Get(t, key, ct).ConfigureAwait(false)).Text, "assembled data");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListParts for an unknown upload returns NoSuchUpload with the UploadId", async (ct) =>
            {
                CompatResponse r = await t.SendAsync(HttpMethod.Get, t.Key("ten.txt"), CompatTarget.Query("uploadId", "bogus"), token: ct).ConfigureAwait(false);
                ExpectError(r, 404, "NoSuchUpload");
                AssertHelper.AreEqual("bogus", r.Element("UploadId"), "UploadId detail");
            }, token).ConfigureAwait(false);
        }

        private static async Task Deletes(TestRunner runner, CompatTarget t, CancellationToken token)
        {
            await runner.RunTestAsync("DeleteObjects reports each key with only a Key element", async (ct) =>
            {
                await Put(t, t.Key("del1.txt"), "x", ct).ConfigureAwait(false);
                CompatResponse r = await DeleteObjects(t, false, ct, t.Key("del1.txt"), t.Key("never-existed.txt")).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "status");
                AssertHelper.AreEqual(_S3Namespace, r.Xml.Root.Name.NamespaceName, "namespace");
                AssertHelper.AreEqual(Join(t.Key("del1.txt"), t.Key("never-existed.txt")), Join(r.Elements("Key", "Deleted").OrderBy(k => k, StringComparer.Ordinal).ToArray()), "deleted keys (Amazon S3 does not order them)");
                AssertHelper.AreEqual(0, r.Elements("VersionId").Count + r.Elements("DeleteMarker").Count, "no version elements");
                AssertHelper.StatusCodeEquals(404, (await Send(t, HttpMethod.Head, t.Key("del1.txt"), ct).ConfigureAwait(false)).StatusCode, "object gone");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("DeleteObjects in quiet mode returns an empty DeleteResult", async (ct) =>
            {
                await Put(t, t.Key("del2.txt"), "x", ct).ConfigureAwait(false);
                CompatResponse r = await DeleteObjects(t, true, ct, t.Key("del2.txt")).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "status");
                AssertHelper.AreEqual("DeleteResult", r.Xml.Root.Name.LocalName, "root");
                AssertHelper.AreEqual(0, r.Elements("Deleted").Count, "no Deleted entries");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("DELETE object returns 204 whether or not the object exists", async (ct) =>
            {
                await Put(t, t.Key("del3.txt"), "x", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(204, (await Send(t, HttpMethod.Delete, t.Key("del3.txt"), ct).ConfigureAwait(false)).StatusCode, "existing");
                AssertHelper.StatusCodeEquals(204, (await Send(t, HttpMethod.Delete, t.Key("del3.txt"), ct).ConfigureAwait(false)).StatusCode, "missing");
            }, token).ConfigureAwait(false);
        }

        private static async Task AclsTagsAndErrors(TestRunner runner, CompatTarget t, CancellationToken token)
        {
            await runner.RunTestAsync("Canned object ACL with no body is accepted", async (ct) =>
            {
                CompatResponse r = await t.SendAsync(HttpMethod.Put, t.Key("ten.txt"), CompatTarget.Query("acl", null), Headers("x-amz-acl", "private"), token: ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "status");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Malformed ACL body returns MalformedACLError", async (ct) =>
            {
                CompatResponse r = await t.SendAsync(HttpMethod.Put, t.Key("ten.txt"), CompatTarget.Query("acl", null), body: System.Text.Encoding.UTF8.GetBytes("<not-xml"), token: ct).ConfigureAwait(false);
                ExpectError(r, 400, "MalformedACLError");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("GetObjectAcl and GetObjectTagging return namespaced documents", async (ct) =>
            {
                CompatResponse r = await t.SendAsync(HttpMethod.Get, t.Key("ten.txt"), CompatTarget.Query("acl", null), token: ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "acl status");
                AssertHelper.AreEqual("AccessControlPolicy", r.Xml.Root.Name.LocalName, "acl root");
                AssertHelper.AreEqual(_S3Namespace, r.Xml.Root.Name.NamespaceName, "acl namespace");
                AssertHelper.AreEqual("FULL_CONTROL", r.Element("Permission"), "permission");

                r = await t.SendAsync(HttpMethod.Get, t.Key("ten.txt"), CompatTarget.Query("tagging", null), token: ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, r.StatusCode, "tagging status");
                AssertHelper.AreEqual("Tagging", r.Xml.Root.Name.LocalName, "tagging root");
                AssertHelper.IsTrue(r.HasRootChild("TagSet"), "TagSet");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("NoSuchKey error names the key and carries request identifiers", async (ct) =>
            {
                CompatResponse r = await Get(t, t.Key("missing.txt"), ct).ConfigureAwait(false);
                ExpectError(r, 404, "NoSuchKey");
                AssertHelper.AreEqual("", r.Xml.Root.Name.NamespaceName, "error namespace");
                AssertHelper.AreEqual(t.Key("missing.txt"), r.Element("Key"), "Key");
                AssertHelper.IsNotNull(r.Element("RequestId"), "RequestId");
                AssertHelper.IsNotNull(r.Element("HostId"), "HostId");
                AssertHelper.IsNotNull(r.Header("x-amz-request-id"), "x-amz-request-id");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("HEAD of a missing object returns 404 with no body", async (ct) =>
            {
                CompatResponse r = await Send(t, HttpMethod.Head, t.Key("missing.txt"), ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(404, r.StatusCode, "status");
                AssertHelper.AreEqual(0, r.Body.Length, "body length");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("InvalidArgument errors carry request identifiers", async (ct) =>
            {
                CompatResponse r = await t.SendAsync(HttpMethod.Get, null, CompatTarget.Query("prefix", t.Prefix, "max-keys", "-1"), token: ct).ConfigureAwait(false);
                ExpectError(r, 400, "InvalidArgument");
                AssertHelper.IsNotNull(r.Element("RequestId"), "RequestId");
                AssertHelper.IsNotNull(r.Header("x-amz-request-id"), "x-amz-request-id");
            }, token).ConfigureAwait(false);
        }

        #endregion

        #region Helpers

        private static async Task CreateFixtures(CompatTarget t, CancellationToken token)
        {
            await Put(t, t.Key("ten.txt"), _Ten, token).ConfigureAwait(false);
            await Put(t, t.Key("empty.txt"), "", token).ConfigureAwait(false);
            foreach (string k in _ListKeys) await Put(t, t.Key(k), "x", token).ConfigureAwait(false);
            foreach (string k in _EncodedKeys.Keys) await Put(t, t.Key(k), "x", token).ConfigureAwait(false);
        }

        private static async Task Put(CompatTarget t, string key, string data, CancellationToken token)
        {
            CompatResponse r = await t.SendAsync(HttpMethod.Put, key, body: System.Text.Encoding.UTF8.GetBytes(data), token: token).ConfigureAwait(false);
            if (r.StatusCode != 200) throw new InvalidOperationException("Fixture PUT " + key + " failed: " + r);
        }

        private static async Task<string> CreateUpload(CompatTarget t, string key, CancellationToken token)
        {
            CompatResponse r = await t.SendAsync(HttpMethod.Post, key, CompatTarget.Query("uploads", null), token: token).ConfigureAwait(false);
            AssertHelper.StatusCodeEquals(200, r.StatusCode, "CreateMultipartUpload status");
            AssertHelper.AreEqual(_S3Namespace, r.Xml.Root.Name.NamespaceName, "CreateMultipartUpload namespace");
            return r.Element("UploadId");
        }

        private static async Task<CompatResponse> DeleteObjects(CompatTarget t, bool quiet, CancellationToken token, params string[] keys)
        {
            StringBuilder xml = new StringBuilder("<Delete>");
            if (quiet) xml.Append("<Quiet>true</Quiet>");
            foreach (string k in keys) xml.Append("<Object><Key>").Append(System.Security.SecurityElement.Escape(k)).Append("</Key></Object>");
            xml.Append("</Delete>");

            byte[] body = System.Text.Encoding.UTF8.GetBytes(xml.ToString());
            string md5 = Convert.ToBase64String(MD5.HashData(body));
            return await t.SendAsync(HttpMethod.Post, null, CompatTarget.Query("delete", null), Headers("Content-MD5", md5, "Content-Type", "application/xml"), body, token).ConfigureAwait(false);
        }

        private static Task<CompatResponse> Get(CompatTarget t, string key, CancellationToken token, params string[] headerPairs)
        {
            return Send(t, HttpMethod.Get, key, token, headerPairs);
        }

        private static Task<CompatResponse> Send(CompatTarget t, HttpMethod method, string key, CancellationToken token, params string[] headerPairs)
        {
            return t.SendAsync(method, key, null, Headers(headerPairs), null, token);
        }

        private static Dictionary<string, string> Headers(params string[] pairs)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i + 1 < pairs.Length; i += 2) ret[pairs[i]] = pairs[i + 1];
            return ret;
        }

        private static void ExpectError(CompatResponse r, int status, string code, string argumentName = null, string argumentValue = null)
        {
            AssertHelper.StatusCodeEquals(status, r.StatusCode, "status (" + r + ")");
            AssertHelper.AreEqual(code, r.ErrorCode, "error code");
            if (argumentName != null) AssertHelper.AreEqual(argumentName, r.Element("ArgumentName"), "ArgumentName");
            if (argumentValue != null) AssertHelper.AreEqual(argumentValue, r.Element("ArgumentValue"), "ArgumentValue");
        }

        private static string Join(params string[] values)
        {
            return String.Join("|", values);
        }

        #endregion
    }
}
