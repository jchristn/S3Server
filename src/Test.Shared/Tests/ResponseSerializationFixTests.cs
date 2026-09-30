namespace Test.Shared.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using S3ServerLibrary;
    using S3ServerLibrary.S3Objects;

    /// <summary>
    /// Serializer-level coverage for response body fixes: the S3 XML namespace, DeleteResult element omission,
    /// ListBucketResult pagination elements, interleaved ListVersionsResult entries, and the header parsers.
    /// These tests do not require a running server.
    /// </summary>
    public static class ResponseSerializationFixTests
    {
        private const string _S3Namespace = "http://s3.amazonaws.com/doc/2006-03-01/";
        private const string _LoggingNamespace = "http://doc.s3.amazonaws.com/2006-03-01";
        private const string _XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

        /// <summary>
        /// Run all response serialization fix tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, CancellationToken token = default)
        {
            #region Namespace

            foreach (KeyValuePair<string, object> sample in ResponseSamples())
            {
                KeyValuePair<string, object> captured = sample;

                await runner.RunTestAsync("Serialized response is fully in the S3 namespace and round-trips: " + captured.Key, (ct) =>
                {
                    string xml = SerializationHelper.SerializeXml(captured.Value);
                    XDocument doc = XDocument.Parse(xml);

                    AssertHelper.AreEqual(_S3Namespace, doc.Root.Name.NamespaceName, "root namespace");
                    foreach (XElement element in doc.Root.Descendants())
                        AssertHelper.AreEqual(_S3Namespace, element.Name.NamespaceName, "namespace of " + element.Name.LocalName);

                    object back = DeserializeAs(captured.Value.GetType(), xml);
                    AssertHelper.IsNotNull(back, "round-trip result");
                    AssertHelper.AreEqual(captured.Value.GetType(), back.GetType(), "round-trip type");
                    return Task.CompletedTask;
                }, token).ConfigureAwait(false);
            }

            await runner.RunTestAsync("Standalone Error has no namespace and no HttpStatusCode element", (ct) =>
            {
                string xml = SerializationHelper.SerializeXml(new Error(ErrorCode.NoSuchKey));
                XDocument doc = XDocument.Parse(xml);

                AssertHelper.AreEqual("", doc.Root.Name.NamespaceName, "error namespace");
                AssertHelper.IsNull(doc.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "HttpStatusCode"), "no HttpStatusCode element");
                AssertHelper.AreEqual(404, new Error(ErrorCode.NoSuchKey).HttpStatusCode, "NoSuchKey status");
                AssertHelper.AreEqual(403, new S3Exception(new Error(ErrorCode.AccessDenied)).HttpStatusCode, "AccessDenied status");
                AssertHelper.AreEqual(500, new S3Exception(new Error(ErrorCode.InternalError)).HttpStatusCode, "InternalError status");
                AssertHelper.AreEqual(304, new Error(ErrorCode.NotModified).HttpStatusCode, "NotModified status");

                Error back = SerializationHelper.DeserializeXml<Error>(xml);
                AssertHelper.AreEqual(ErrorCode.NoSuchKey, back.Code, "round-trip code");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Error inside DeleteResult is in the S3 namespace", (ct) =>
            {
                DeleteResult result = new DeleteResult(null, new List<Error> { new Error(ErrorCode.AccessDenied, "k") });
                XDocument doc = XDocument.Parse(SerializationHelper.SerializeXml(result));

                XElement error = doc.Root.Elements().Single(e => e.Name.LocalName == "Error");
                AssertHelper.AreEqual(_S3Namespace, error.Name.NamespaceName, "nested error namespace");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("BucketLoggingStatus uses the S3 logging namespace and round-trips", (ct) =>
            {
                BucketLoggingStatus status = new BucketLoggingStatus(new LoggingEnabled("logs", "prefix/", new TargetGrants()));
                string xml = SerializationHelper.SerializeXml(status);
                XDocument doc = XDocument.Parse(xml);

                AssertHelper.AreEqual(_LoggingNamespace, doc.Root.Name.NamespaceName, "logging namespace");
                BucketLoggingStatus back = SerializationHelper.DeserializeXml<BucketLoggingStatus>(xml);
                AssertHelper.AreEqual("logs", back.Enabled.TargetBucket, "round-trip target bucket");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Bare XML without a namespace still deserializes", (ct) =>
            {
                Tagging tagging = SerializationHelper.DeserializeXml<Tagging>("<Tagging><TagSet><Tag><Key>k</Key><Value>v</Value></Tag></TagSet></Tagging>");
                AssertHelper.AreEqual("k", tagging.Tags.Tags[0].Key, "bare XML tag key");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region DeleteResult

            await runner.RunTestAsync("Deleted with only a key serializes to exactly the Key element", (ct) =>
            {
                XElement deleted = SerializeDeleted(new Deleted("k", null, null));
                AssertHelper.AreEqual("Key", String.Join(",", deleted.Elements().Select(e => e.Name.LocalName)), "elements");
                AssertHelper.AreEqual("k", deleted.Elements().Single().Value, "key value");

                XElement falseMarker = SerializeDeleted(new Deleted("k", "", false, ""));
                AssertHelper.AreEqual("Key", String.Join(",", falseMarker.Elements().Select(e => e.Name.LocalName)), "false marker omitted");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Deleted with a delete marker emits DeleteMarker and DeleteMarkerVersionId", (ct) =>
            {
                XElement deleted = SerializeDeleted(new Deleted("k", "v1", true, "dm1"));
                AssertHelper.AreEqual("Key,VersionId,DeleteMarker,DeleteMarkerVersionId", String.Join(",", deleted.Elements().Select(e => e.Name.LocalName)), "elements");
                AssertHelper.AreEqual("true", deleted.Elements().Single(e => e.Name.LocalName == "DeleteMarker").Value, "marker value");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Serialized DeleteResult contains no xsi:nil and round-trips", (ct) =>
            {
                DeleteResult result = new DeleteResult(
                    new List<Deleted> { new Deleted("a", null, null), new Deleted("b", "2", true, "3") },
                    new List<Error> { new Error(ErrorCode.AccessDenied, "c") });

                string xml = SerializationHelper.SerializeXml(result);
                AssertHelper.StringDoesNotContain(xml, "nil", "no nil attributes");
                AssertHelper.StringDoesNotContain(xml, _XsiNamespace, "no xsi namespace");

                DeleteResult back = SerializationHelper.DeserializeXml<DeleteResult>(xml);
                AssertHelper.AreEqual(2, back.DeletedObjects.Count, "deleted count");
                AssertHelper.IsNull(back.DeletedObjects[0].DeleteMarker, "absent marker is null");
                AssertHelper.AreEqual(true, back.DeletedObjects[1].DeleteMarker, "marker round-trips");
                AssertHelper.AreEqual("3", back.DeletedObjects[1].DeleteMarkerVersionId, "marker version round-trips");
                AssertHelper.AreEqual(1, back.Errors.Count, "error count");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region ListBucketResult

            await runner.RunTestAsync("ListBucketResult pagination elements appear only when set", (ct) =>
            {
                ListBucketResult empty = new ListBucketResult();
                empty.Name = "b";
                XElement emptyRoot = XDocument.Parse(SerializationHelper.SerializeXml(empty)).Root;
                foreach (string name in new[] { "NextMarker", "StartAfter", "ContinuationToken" })
                    AssertHelper.IsNull(emptyRoot.Elements().FirstOrDefault(e => e.Name.LocalName == name), name + " omitted");

                ListBucketResult populated = new ListBucketResult();
                populated.Name = "b";
                populated.NextMarker = "photos/";
                populated.StartAfter = "a.txt";
                populated.ContinuationToken = "token-1";
                string xml = SerializationHelper.SerializeXml(populated);
                XElement root = XDocument.Parse(xml).Root;
                AssertHelper.AreEqual("photos/", root.Elements().Single(e => e.Name.LocalName == "NextMarker").Value, "NextMarker");
                AssertHelper.AreEqual("a.txt", root.Elements().Single(e => e.Name.LocalName == "StartAfter").Value, "StartAfter");
                AssertHelper.AreEqual("token-1", root.Elements().Single(e => e.Name.LocalName == "ContinuationToken").Value, "ContinuationToken");

                ListBucketResult back = SerializationHelper.DeserializeXml<ListBucketResult>(xml);
                AssertHelper.AreEqual("photos/", back.NextMarker, "NextMarker round-trips");
                AssertHelper.AreEqual("a.txt", back.StartAfter, "StartAfter round-trips");
                AssertHelper.AreEqual("token-1", back.ContinuationToken, "ContinuationToken round-trips");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region ListVersionsResult

            await runner.RunTestAsync("ListVersionsResult Entries serialize interleaved in list order and round-trip", (ct) =>
            {
                ListVersionsResult result = new ListVersionsResult();
                result.Name = "b";
                result.Entries.Add(new ObjectVersion("a.txt", "3", true, DateTime.UtcNow, "etag3", 3, null));
                result.Entries.Add(new DeleteMarker("a.txt", "2", false, DateTime.UtcNow, null));
                result.Entries.Add(new ObjectVersion("a.txt", "1", false, DateTime.UtcNow, "etag1", 1, null));

                string xml = SerializationHelper.SerializeXml(result);
                XElement root = XDocument.Parse(xml).Root;
                string order = String.Join(",", root.Elements()
                    .Where(e => e.Name.LocalName == "Version" || e.Name.LocalName == "DeleteMarker")
                    .Select(e => e.Name.LocalName + ":" + e.Elements().Single(c => c.Name.LocalName == "VersionId").Value));
                AssertHelper.AreEqual("Version:3,DeleteMarker:2,Version:1", order, "interleaved order");

                ListVersionsResult back = SerializationHelper.DeserializeXml<ListVersionsResult>(xml);
                AssertHelper.AreEqual(3, back.Entries.Count, "entries round-trip");
                AssertHelper.IsTrue(back.Entries[1] is DeleteMarker, "entry types preserved");
                AssertHelper.AreEqual(2, back.Versions.Count, "versions populated");
                AssertHelper.AreEqual(1, back.DeleteMarkers.Count, "delete markers populated");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListVersionsResult separate lists keep the previous output", (ct) =>
            {
                ListVersionsResult result = new ListVersionsResult(
                    "b",
                    new List<ObjectVersion>
                    {
                        new ObjectVersion("a.txt", "3", true, DateTime.UtcNow, "etag3", 3, null),
                        new ObjectVersion("b.txt", "1", true, DateTime.UtcNow, "etag1", 1, null)
                    },
                    new List<DeleteMarker> { new DeleteMarker("a.txt", "2", false, DateTime.UtcNow, null) },
                    1000);

                string xml = SerializationHelper.SerializeXml(result);
                XElement root = XDocument.Parse(xml).Root;
                string order = String.Join(",", root.Elements()
                    .Where(e => e.Name.LocalName == "Version" || e.Name.LocalName == "DeleteMarker")
                    .Select(e => e.Name.LocalName + ":" + e.Elements().Single(c => c.Name.LocalName == "VersionId").Value));
                AssertHelper.AreEqual("Version:3,Version:1,DeleteMarker:2", order, "versions then markers");
                AssertHelper.IsNull(root.Elements().FirstOrDefault(e => e.Name.LocalName == "XmlEntries" || e.Name.LocalName == "Entries"), "no wrapper element");

                ListVersionsResult back = SerializationHelper.DeserializeXml<ListVersionsResult>(xml);
                AssertHelper.AreEqual(2, back.Versions.Count, "versions round-trip");
                AssertHelper.AreEqual(1, back.DeleteMarkers.Count, "delete markers round-trip");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Empty ListVersionsResult has no Version or DeleteMarker elements", (ct) =>
            {
                ListVersionsResult result = new ListVersionsResult();
                result.Name = "b";
                XElement root = XDocument.Parse(SerializationHelper.SerializeXml(result)).Root;
                AssertHelper.IsNull(root.Elements().FirstOrDefault(e => e.Name.LocalName == "Version" || e.Name.LocalName == "DeleteMarker"), "no entries");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region Amazon-S3-Fidelity

            await runner.RunTestAsync("S3 URL encoding matches the Amazon S3 encoding-type=url rules", (ct) =>
            {
                AssertHelper.AreEqual("a+b%2Bc%26d%3De%3Ff%23g%25h%7E%C3%A9.txt", XmlWriterExtended.S3UrlEncode("a b+c&d=e?f#g%h~é.txt"), "mixed");
                AssertHelper.AreEqual("semi%3Bcolon%2Ccomma%3A%40%24%21*%27%28%29%5B%5D.txt", XmlWriterExtended.S3UrlEncode("semi;colon,comma:@$!*'()[].txt"), "punctuation");
                AssertHelper.AreEqual("dir/sub/A-z_0.9", XmlWriterExtended.S3UrlEncode("dir/sub/A-z_0.9"), "unreserved and slash");
                AssertHelper.AreEqual("tab%09key", XmlWriterExtended.S3UrlEncode("tab\tkey"), "control character");
                AssertHelper.AreEqual("", XmlWriterExtended.S3UrlEncode(""), "empty");
                AssertHelper.IsNull(XmlWriterExtended.S3UrlEncode(null), "null");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Timestamps serialize with exactly three fractional digits in UTC", (ct) =>
            {
                CopyObjectResult withTicks = new CopyObjectResult("e", new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc).AddTicks(1234567));
                AssertHelper.AreEqual("2026-01-02T03:04:05.123Z", Root(withTicks).Elements().Single(e => e.Name.LocalName == "LastModified").Value, "fractional");

                CopyObjectResult whole = new CopyObjectResult("e", new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc));
                AssertHelper.AreEqual("2026-01-02T03:04:05.000Z", Root(whole).Elements().Single(e => e.Name.LocalName == "LastModified").Value, "whole second");

                Upload upload = new Upload { Key = "k", UploadId = "u", Initiated = new DateTime(2026, 1, 2, 3, 4, 5, 678, DateTimeKind.Utc) };
                ListMultipartUploadsResult uploads = new ListMultipartUploadsResult();
                uploads.Bucket = "b";
                uploads.Uploads = new List<Upload> { upload };
                AssertHelper.AreEqual("2026-01-02T03:04:05.678Z", Root(uploads).Descendants().Single(e => e.Name.LocalName == "Initiated").Value, "Initiated");

                CopyObjectResult back = SerializationHelper.DeserializeXml<CopyObjectResult>(SerializationHelper.SerializeXml(withTicks));
                AssertHelper.AreEqual(new DateTime(2026, 1, 2, 3, 4, 5, 123, DateTimeKind.Utc), back.LastModified, "round-trip");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Error detail elements serialize only when set, before RequestId", (ct) =>
            {
                Error bare = new Error(ErrorCode.InvalidRange);
                XElement bareRoot = Root(bare);
                foreach (string name in new[] { "ArgumentName", "ArgumentValue", "Condition", "RangeRequested", "ActualObjectSize", "UploadId", "BucketName" })
                    AssertHelper.IsNull(bareRoot.Elements().FirstOrDefault(e => e.Name.LocalName == name), name + " omitted");

                Error detailed = new Error(ErrorCode.InvalidRange) { RangeRequested = "bytes=-0", ActualObjectSize = 10, RequestId = "r1", HostId = "h1" };
                AssertHelper.AreEqual("Code,Message,RangeRequested,ActualObjectSize,RequestId,HostId", String.Join(",", Root(detailed).Elements().Select(e => e.Name.LocalName)), "element order");

                Error emptyValue = new Error(ErrorCode.InvalidArgument) { ArgumentName = "x-amz-copy-source", ArgumentValue = "" };
                AssertHelper.IsNotNull(Root(emptyValue).Elements().FirstOrDefault(e => e.Name.LocalName == "ArgumentValue"), "empty ArgumentValue kept");

                AssertHelper.AreEqual("The requested range is not satisfiable", new Error(ErrorCode.InvalidRange).Message, "InvalidRange message");
                AssertHelper.AreEqual(400, new Error(ErrorCode.MalformedACLError).HttpStatusCode, "MalformedACLError status");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Direct serialization keeps the model-defined shape outside S3Server", (ct) =>
            {
                ListBucketResult list = new ListBucketResult();
                list.Name = "b";
                list.Contents.Add(new ObjectMetadata("k", DateTime.UtcNow, "e", 1, new Owner("o", "O")));
                XElement listRoot = Root(list);
                AssertHelper.IsNull(listRoot.Elements().FirstOrDefault(e => e.Name.LocalName == "Marker"), "null Marker omitted");
                AssertHelper.IsNotNull(listRoot.Elements().FirstOrDefault(e => e.Name.LocalName == "KeyCount"), "KeyCount kept");
                AssertHelper.IsNotNull(listRoot.Descendants().FirstOrDefault(e => e.Name.LocalName == "Owner"), "Owner kept");

                DeleteResult deletes = new DeleteResult(new List<Deleted> { new Deleted("k", null, null) }, null);
                AssertHelper.IsNotNull(Root(deletes).Elements().FirstOrDefault(e => e.Name.LocalName == "Deleted"), "Deleted kept");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("CopyObjectResult checksums serialize only when set", (ct) =>
            {
                CopyObjectResult plain = new CopyObjectResult("e", DateTime.UtcNow);
                AssertHelper.AreEqual("LastModified,ETag", String.Join(",", Root(plain).Elements().Select(e => e.Name.LocalName)), "plain");

                CopyPartResult withChecksum = new CopyPartResult("e", DateTime.UtcNow) { ChecksumCRC64NVME = "Ffmx7kz9nB0=", ChecksumType = "FULL_OBJECT" };
                AssertHelper.AreEqual("LastModified,ETag,ChecksumCRC64NVME,ChecksumType", String.Join(",", Root(withChecksum).Elements().Select(e => e.Name.LocalName)), "with checksum");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region Header-Parsers

            await runner.RunTestAsync("Range header parser accepts valid forms", (ct) =>
            {
                AssertRange("bytes=0-4", 0, 4, null);
                AssertRange("bytes=5-", 5, null, null);
                AssertRange("bytes=-3", null, null, 3);
                AssertRange("BYTES=1-1", 1, 1, null);
                AssertRange(" bytes = 2 - 3 ", null, null, null, false);
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Range header parser rejects invalid forms", (ct) =>
            {
                foreach (string header in new[] { null, "", "bytes=", "bytes=-", "bytes=abc", "bytes=0-1,4-5", "items=0-1", "bytes=5-2", "bytes=1-2-3", "bytes=-1-2", "bytes=+1-2", "bytes=99999999999999999999-" })
                    AssertRange(header, null, null, null, false);
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Entity tag list parser handles lists wildcards weak tags and quoted commas", (ct) =>
            {
                AssertHelper.IsNull(ConditionalHeaderParser.ParseEntityTagList(null), "null");
                AssertHelper.IsNull(ConditionalHeaderParser.ParseEntityTagList("  "), "whitespace");
                AssertHelper.IsNull(ConditionalHeaderParser.ParseEntityTagList(" , ,"), "only separators");
                AssertHelper.AreEqual("*", String.Join("|", ConditionalHeaderParser.ParseEntityTagList("*")), "wildcard");
                AssertHelper.AreEqual("\"a\"|W/\"b\"|\"c,d\"", String.Join("|", ConditionalHeaderParser.ParseEntityTagList("\"a\", W/\"b\" ,\"c,d\"")), "list");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("HTTP date parser accepts RFC 9110 formats and ignores garbage", (ct) =>
            {
                DateTime expected = new DateTime(1994, 11, 6, 8, 49, 37, DateTimeKind.Utc);
                AssertHelper.AreEqual(expected, ConditionalHeaderParser.ParseHttpDate("Sun, 06 Nov 1994 08:49:37 GMT").Value, "IMF-fixdate");
                AssertHelper.AreEqual(expected, ConditionalHeaderParser.ParseHttpDate("Sunday, 06-Nov-94 08:49:37 GMT").Value, "RFC 850");
                AssertHelper.AreEqual(expected, ConditionalHeaderParser.ParseHttpDate("Sun Nov  6 08:49:37 1994").Value, "asctime");
                AssertHelper.AreEqual(DateTimeKind.Utc, ConditionalHeaderParser.ParseHttpDate("Sun, 06 Nov 1994 08:49:37 GMT").Value.Kind, "UTC kind");
                AssertHelper.IsNull(ConditionalHeaderParser.ParseHttpDate(null), "null");
                AssertHelper.IsNull(ConditionalHeaderParser.ParseHttpDate("yesterday"), "garbage");
                AssertHelper.IsNull(ConditionalHeaderParser.ParseHttpDate("2026-13-45"), "invalid ISO");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion
        }

        private static List<KeyValuePair<string, object>> ResponseSamples()
        {
            Owner owner = new Owner("owner-id", "Owner");

            ListBucketResult listBucket = new ListBucketResult("b", new List<ObjectMetadata> { new ObjectMetadata("k", DateTime.UtcNow, "e", 1, owner) }, 1, 1000, "p", null, "/", false, null, new List<CommonPrefixes> { new CommonPrefixes("p/") });
            ListVersionsResult listVersions = new ListVersionsResult("b", new List<ObjectVersion> { new ObjectVersion("k", "1", true, DateTime.UtcNow, "e", 1, owner) }, new List<DeleteMarker> { new DeleteMarker("k", "2", false, DateTime.UtcNow, owner) }, 1000);

            ListAllMyBucketsResult buckets = new ListAllMyBucketsResult();
            buckets.Owner = owner;
            buckets.Buckets = new Buckets(new List<Bucket> { new Bucket("b", DateTime.UtcNow) });

            ListMultipartUploadsResult uploads = new ListMultipartUploadsResult();
            uploads.Bucket = "b";
            uploads.Uploads = new List<Upload> { new Upload { Key = "k", UploadId = "u", Owner = owner, Initiator = owner, Initiated = DateTime.UtcNow } };

            ListPartsResult parts = new ListPartsResult();
            parts.Bucket = "b";
            parts.Key = "k";
            parts.UploadId = "u";
            parts.Owner = owner;
            parts.Initiator = owner;
            parts.Parts = new List<Part> { new Part { PartNumber = 1, LastModified = DateTime.UtcNow, ETag = "e", Size = 1 } };

            CompleteMultipartUploadResult complete = new CompleteMultipartUploadResult();
            complete.Bucket = "b";
            complete.Key = "k";
            complete.ETag = "e";

            return new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("ListAllMyBucketsResult", buckets),
                new KeyValuePair<string, object>("ListBucketResult", listBucket),
                new KeyValuePair<string, object>("ListVersionsResult", listVersions),
                new KeyValuePair<string, object>("AccessControlPolicy", new AccessControlPolicy(owner, new AccessControlList(new List<Grant> { new Grant(new Grantee("owner-id", "Owner", null, "CanonicalUser", null), PermissionEnum.FullControl) }))),
                new KeyValuePair<string, object>("LocationConstraint", new LocationConstraint("us-west-1")),
                new KeyValuePair<string, object>("Tagging", new Tagging(new TagSet(new List<Tag> { new Tag("k", "v") }))),
                new KeyValuePair<string, object>("VersioningConfiguration", new VersioningConfiguration(VersioningStatusEnum.Enabled, MfaDeleteStatusEnum.Disabled)),
                new KeyValuePair<string, object>("WebsiteConfiguration", new WebsiteConfiguration { IndexDocument = new IndexDocument("index.html"), ErrorDocument = new ErrorDocument("error.html") }),
                new KeyValuePair<string, object>("InitiateMultipartUploadResult", new InitiateMultipartUploadResult("b", "k", "u")),
                new KeyValuePair<string, object>("ListMultipartUploadsResult", uploads),
                new KeyValuePair<string, object>("ListPartsResult", parts),
                new KeyValuePair<string, object>("CompleteMultipartUploadResult", complete),
                new KeyValuePair<string, object>("DeleteResult", new DeleteResult(new List<Deleted> { new Deleted("k", null, null) }, new List<Error> { new Error(ErrorCode.AccessDenied, "k2") })),
                new KeyValuePair<string, object>("LegalHold", new LegalHold("ON")),
                new KeyValuePair<string, object>("Retention", new Retention(RetentionModeEnum.Governance, DateTime.UtcNow.AddDays(1))),
                new KeyValuePair<string, object>("CopyObjectResult", new CopyObjectResult("e", DateTime.UtcNow)),
                new KeyValuePair<string, object>("CopyPartResult", new CopyPartResult("e", DateTime.UtcNow))
            };
        }

        private static object DeserializeAs(Type type, string xml)
        {
            System.Reflection.MethodInfo method = typeof(SerializationHelper)
                .GetMethods()
                .Single(m => m.Name == "DeserializeXml" && m.GetParameters()[0].ParameterType == typeof(string));

            return method.MakeGenericMethod(type).Invoke(null, new object[] { xml });
        }

        private static XElement Root(object obj)
        {
            return XDocument.Parse(SerializationHelper.SerializeXml(obj)).Root;
        }

        private static XElement SerializeDeleted(Deleted deleted)
        {
            DeleteResult result = new DeleteResult(new List<Deleted> { deleted }, null);
            return XDocument.Parse(SerializationHelper.SerializeXml(result)).Root.Elements().Single(e => e.Name.LocalName == "Deleted");
        }

        private static void AssertRange(string header, long? expectedStart, long? expectedEnd, long? expectedSuffix, bool expectedResult = true)
        {
            bool result = S3Request.TryParseRangeHeader(header, out long? start, out long? end, out long? suffix);
            AssertHelper.AreEqual(expectedResult, result, "parse result for '" + header + "'");
            AssertHelper.AreEqual(expectedStart, start, "start for '" + header + "'");
            AssertHelper.AreEqual(expectedEnd, end, "end for '" + header + "'");
            AssertHelper.AreEqual(expectedSuffix, suffix, "suffix for '" + header + "'");
        }
    }
}
