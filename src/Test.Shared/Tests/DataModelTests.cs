namespace Test.Shared.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using S3ServerLibrary;
    using S3ServerLibrary.S3Objects;

    /// <summary>
    /// Behavioral and serialization tests for the S3Object data holder and the response model
    /// classes.  These tests do not require a running server.
    /// </summary>
    public static class DataModelTests
    {
        /// <summary>
        /// Run all data model tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, CancellationToken token = default)
        {
            #region S3Object

            await runner.RunTestAsync("S3Object exposes byte data as bytes, stream, and string", (ct) =>
            {
                byte[] data = Encoding.UTF8.GetBytes("hello world");
                using (S3Object obj = new S3Object("k", null, true, DateTime.UtcNow, "etag", data.Length, new Owner("o", "O"), data))
                {
                    AssertHelper.AreEqual("hello world", obj.DataString, "data string");
                    AssertHelper.AreEqual(data.Length, obj.DataBytes.Length, "data bytes length");
                    AssertHelper.IsNotNull(obj.Data, "data stream");
                    AssertHelper.AreEqual("\"etag\"", obj.ETag, "etag normalized with quotes");
                }
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("S3Object stream constructor reads bytes and string", (ct) =>
            {
                byte[] data = Encoding.UTF8.GetBytes("stream-data");
                MemoryStream ms = new MemoryStream(data);
                using (S3Object obj = new S3Object("k", null, true, DateTime.UtcNow, "\"quoted\"", data.Length, new Owner(), ms))
                {
                    AssertHelper.AreEqual("stream-data", obj.DataString, "data string from stream");
                    AssertHelper.AreEqual(data.Length, obj.DataBytes.Length, "data bytes from stream");
                    AssertHelper.AreEqual("\"quoted\"", obj.ETag, "etag already quoted stays quoted");
                }
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("S3Object string constructor round-trips multibyte UTF-8 data", (ct) =>
            {
                string value = "café – über 日本語";
                byte[] expected = Encoding.UTF8.GetBytes(value);
                using (S3Object obj = new S3Object("k", null, true, DateTime.UtcNow, "etag", expected.Length, new Owner(), value))
                {
                    AssertHelper.AreEqual(value, obj.DataString, "multibyte data string round-trip");
                    AssertHelper.AreEqual(expected.Length, obj.DataBytes.Length, "multibyte byte length");
                }
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("S3Object Data setter accepts null and clears data", (ct) =>
            {
                S3Object obj = new S3Object();
                obj.DataString = "temp";
                AssertHelper.IsNotNull(obj.Data, "data present");
                obj.Data = null;
                AssertHelper.IsNull(obj.Data, "data cleared");
                AssertHelper.IsNull(obj.DataBytes, "data bytes cleared");
                AssertHelper.IsNull(obj.DataString, "data string cleared");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("S3Object rejects negative size and total size", (ct) =>
            {
                AssertThrows<ArgumentOutOfRangeException>(() => new S3Object { Size = -1 }, "negative size");
                AssertThrows<ArgumentOutOfRangeException>(() => new S3Object { TotalSize = -1 }, "negative total size");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("S3Object TotalSize defaults to null and accepts values", (ct) =>
            {
                S3Object obj = new S3Object();
                AssertHelper.IsNull(obj.TotalSize, "default total size null");
                obj.TotalSize = 1024;
                AssertHelper.AreEqual(1024L, obj.TotalSize.Value, "total size set");
                obj.Size = 100;
                AssertHelper.AreEqual(100L, obj.Size, "size set");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("S3Object Dispose is idempotent", (ct) =>
            {
                S3Object obj = new S3Object("k", null, true, DateTime.UtcNow, "etag", 4, new Owner(), new byte[] { 1, 2, 3, 4 });
                obj.Dispose();
                obj.Dispose();
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region List-Results

            await runner.RunTestAsync("ListBucketResult round-trips through XML", (ct) =>
            {
                ListBucketResult result = new ListBucketResult(
                    "my-bucket",
                    new List<ObjectMetadata>
                    {
                        new ObjectMetadata("file.txt", DateTime.UtcNow, "abc123", 42, new Owner("id", "name"), StorageClassEnum.STANDARD)
                    },
                    keyCount: 1,
                    maxKeys: 1000,
                    prefix: "docs/",
                    marker: "m",
                    delimiter: "/",
                    isTruncated: true,
                    nextToken: "next-1",
                    prefixes: new List<CommonPrefixes> { new CommonPrefixes("docs/2026/") });

                ListBucketResult copy = RoundTripXml(result);
                AssertHelper.AreEqual("my-bucket", copy.Name, "name");
                AssertHelper.AreEqual("docs/", copy.Prefix, "prefix");
                AssertHelper.AreEqual(1, copy.KeyCount, "key count");
                AssertHelper.AreEqual("/", copy.Delimiter, "delimiter");
                AssertHelper.IsTrue(copy.IsTruncated, "truncated");
                AssertHelper.AreEqual("next-1", copy.NextContinuationToken, "next token");
                AssertHelper.HasCount(copy.Contents, 1, "contents");
                AssertHelper.AreEqual("file.txt", copy.Contents[0].Key, "content key");
                AssertHelper.AreEqual("\"abc123\"", copy.Contents[0].ETag, "content etag normalized");
                AssertHelper.AreEqual(42L, copy.Contents[0].Size, "content size");
                AssertHelper.HasCount(copy.CommonPrefixes, 1, "common prefixes");
                AssertHelper.AreEqual("docs/2026/", copy.CommonPrefixes[0].Prefix, "common prefix value");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListBucketResult validation and ShouldSerialize helpers", (ct) =>
            {
                AssertThrows<ArgumentNullException>(() => new ListBucketResult(null, null, 0, 0), "null name");
                AssertThrows<ArgumentOutOfRangeException>(() => new ListBucketResult { KeyCount = -1 }, "negative key count");
                AssertThrows<ArgumentOutOfRangeException>(() => new ListBucketResult { MaxKeys = -1 }, "negative max keys");

                ListBucketResult empty = new ListBucketResult();
                AssertHelper.IsFalse(empty.ShouldSerializeMarker(), "empty marker");
                AssertHelper.IsFalse(empty.ShouldSerializeNextContinuationToken(), "empty next token");
                AssertHelper.IsFalse(empty.ShouldSerializeCommonPrefixes(), "empty common prefixes");
                AssertHelper.IsFalse(empty.ShouldSerializeDelimiter(), "empty delimiter");
                AssertHelper.IsFalse(empty.ShouldSerializeEncodingType(), "empty encoding type");
                AssertHelper.AreEqual("", empty.Prefix, "prefix normalizes null to empty");

                empty.Marker = "m";
                empty.NextContinuationToken = "n";
                empty.Delimiter = "/";
                empty.EncodingType = "url";
                empty.CommonPrefixes.Add(new CommonPrefixes("p/"));
                AssertHelper.IsTrue(empty.ShouldSerializeMarker(), "populated marker");
                AssertHelper.IsTrue(empty.ShouldSerializeNextContinuationToken(), "populated next token");
                AssertHelper.IsTrue(empty.ShouldSerializeCommonPrefixes(), "populated common prefixes");
                AssertHelper.IsTrue(empty.ShouldSerializeDelimiter(), "populated delimiter");
                AssertHelper.IsTrue(empty.ShouldSerializeEncodingType(), "populated encoding type");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListAllMyBucketsResult round-trips through XML", (ct) =>
            {
                ListAllMyBucketsResult result = new ListAllMyBucketsResult(
                    new Owner("owner-id", "Owner"),
                    new Buckets(new List<Bucket>
                    {
                        new Bucket("bucket-a", DateTime.UtcNow),
                        new Bucket("bucket-b", DateTime.UtcNow)
                    }));

                ListAllMyBucketsResult copy = RoundTripXml(result);
                AssertHelper.AreEqual("owner-id", copy.Owner.ID, "owner id");
                AssertHelper.HasCount(copy.Buckets.BucketList, 2, "bucket list");
                AssertHelper.AreEqual("bucket-a", copy.Buckets.BucketList[0].Name, "bucket a name");
                AssertHelper.AreEqual("bucket-b", copy.Buckets.BucketList[1].Name, "bucket b name");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Bucket rejects null name", (ct) =>
            {
                AssertThrows<ArgumentNullException>(() => new Bucket(null, DateTime.UtcNow), "null bucket name");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ObjectMetadata normalizes ETag and rejects negative size", (ct) =>
            {
                ObjectMetadata metadata = new ObjectMetadata("k", DateTime.UtcNow, "raw-etag", 10, new Owner());
                AssertHelper.AreEqual("\"raw-etag\"", metadata.ETag, "etag normalized");
                AssertThrows<ArgumentOutOfRangeException>(() => new ObjectMetadata { Size = -1 }, "negative size");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region Delete-Results

            await runner.RunTestAsync("DeleteResult round-trips deleted objects and errors through XML", (ct) =>
            {
                DeleteResult result = new DeleteResult(
                    new List<Deleted>
                    {
                        new Deleted("key1", "v1", true, "dm-v1"),
                        new Deleted("key2", null, false)
                    },
                    new List<Error>
                    {
                        new Error(ErrorCode.AccessDenied, "key3")
                    });

                DeleteResult copy = RoundTripXml(result);
                AssertHelper.HasCount(copy.DeletedObjects, 2, "deleted objects");
                AssertHelper.AreEqual("key1", copy.DeletedObjects[0].Key, "deleted 0 key");
                AssertHelper.AreEqual("dm-v1", copy.DeletedObjects[0].DeleteMarkerVersionId, "deleted 0 marker version");
                AssertHelper.IsTrue(copy.DeletedObjects[0].DeleteMarker.Value, "deleted 0 marker");
                AssertHelper.HasCount(copy.Errors, 1, "errors");
                AssertHelper.AreEqual(ErrorCode.AccessDenied, copy.Errors[0].Code, "error code");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Deleted rejects null key", (ct) =>
            {
                AssertThrows<ArgumentNullException>(() => new Deleted(null, null, false), "null deleted key");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region Multipart

            await runner.RunTestAsync("InitiateMultipartUploadResult round-trips through XML", (ct) =>
            {
                InitiateMultipartUploadResult result = new InitiateMultipartUploadResult("bucket", "key", "upload-1");
                InitiateMultipartUploadResult copy = RoundTripXml(result);
                AssertHelper.AreEqual("bucket", copy.Bucket, "bucket");
                AssertHelper.AreEqual("key", copy.Key, "key");
                AssertHelper.AreEqual("upload-1", copy.UploadId, "upload id");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("InitiateMultipartUploadResult validates required arguments", (ct) =>
            {
                AssertThrows<ArgumentNullException>(() => new InitiateMultipartUploadResult(null, "k", "u"), "null bucket");
                AssertThrows<ArgumentNullException>(() => new InitiateMultipartUploadResult("b", null, "u"), "null key");
                AssertThrows<ArgumentNullException>(() => new InitiateMultipartUploadResult("b", "k", null), "null upload id");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("CompleteMultipartUpload round-trips parts through XML", (ct) =>
            {
                CompleteMultipartUpload complete = new CompleteMultipartUpload();
                complete.Parts = new List<Part>
                {
                    new Part { PartNumber = 1, ETag = "etag1", Size = 100, ChecksumSHA256 = "sha" },
                    new Part { PartNumber = 2, ETag = "\"etag2\"", Size = 200 }
                };

                CompleteMultipartUpload copy = RoundTripXml(complete);
                AssertHelper.HasCount(copy.Parts, 2, "parts");
                AssertHelper.AreEqual(1, copy.Parts[0].PartNumber, "part 0 number");
                AssertHelper.AreEqual("\"etag1\"", copy.Parts[0].ETag, "part 0 etag normalized");
                AssertHelper.AreEqual("sha", copy.Parts[0].ChecksumSHA256, "part 0 checksum");
                AssertHelper.AreEqual(2, copy.Parts[1].PartNumber, "part 1 number");

                complete.Parts = null;
                AssertHelper.IsNotNull(complete.Parts, "parts null coalesces to empty list");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Part validates part number and size and ShouldSerialize checksums", (ct) =>
            {
                AssertThrows<ArgumentOutOfRangeException>(() => new Part { PartNumber = -1 }, "negative part number");
                AssertThrows<ArgumentOutOfRangeException>(() => new Part { Size = -1 }, "negative size");

                Part part = new Part();
                AssertHelper.IsFalse(part.ShouldSerializeChecksumCRC32(), "empty crc32");
                AssertHelper.IsFalse(part.ShouldSerializeChecksumCRC32C(), "empty crc32c");
                AssertHelper.IsFalse(part.ShouldSerializeChecksumSHA1(), "empty sha1");
                AssertHelper.IsFalse(part.ShouldSerializeChecksumSHA256(), "empty sha256");

                part.ChecksumCRC32 = "a";
                part.ChecksumCRC32C = "b";
                part.ChecksumSHA1 = "c";
                part.ChecksumSHA256 = "d";
                AssertHelper.IsTrue(part.ShouldSerializeChecksumCRC32(), "populated crc32");
                AssertHelper.IsTrue(part.ShouldSerializeChecksumCRC32C(), "populated crc32c");
                AssertHelper.IsTrue(part.ShouldSerializeChecksumSHA1(), "populated sha1");
                AssertHelper.IsTrue(part.ShouldSerializeChecksumSHA256(), "populated sha256");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region Target-Grants

            await runner.RunTestAsync("TargetGrants round-trips through XML", (ct) =>
            {
                TargetGrants grants = new TargetGrants(new List<Grant>
                {
                    new Grant(new CanonicalUser { ID = "u1", DisplayName = "User 1" }, PermissionEnum.Write)
                });

                TargetGrants copy = RoundTripXml(grants);
                AssertHelper.HasCount(copy.Grants, 1, "grants");
                AssertHelper.AreEqual(PermissionEnum.Write, copy.Grants[0].Permission, "grant permission");
                AssertHelper.AreEqual("u1", copy.Grants[0].Grantee.ID, "grantee id");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region Versions-And-Uploads

            await runner.RunTestAsync("ListVersionsResult round-trips versions and delete markers through XML", (ct) =>
            {
                ListVersionsResult result = new ListVersionsResult(
                    "bucket",
                    new List<ObjectVersion>
                    {
                        new ObjectVersion("k", "v1", true, DateTime.UtcNow, "etag", 128, new Owner("id", "name"))
                    },
                    new List<DeleteMarker>
                    {
                        new DeleteMarker("k", "v2", false, DateTime.UtcNow, new Owner("id", "name"))
                    },
                    maxKeys: 500,
                    prefix: "p/",
                    isTruncated: true,
                    nextKeyMarker: "nk",
                    nextVersionIdMarker: "nv",
                    delimiter: "/",
                    commonPrefixes: new List<CommonPrefixes> { new CommonPrefixes("p/2026/") });

                ListVersionsResult copy = RoundTripXml(result);
                AssertHelper.AreEqual("bucket", copy.Name, "name");
                AssertHelper.AreEqual(500, copy.MaxKeys, "max keys");
                AssertHelper.IsTrue(copy.IsTruncated, "truncated");
                AssertHelper.AreEqual("nk", copy.NextKeyMarker, "next key marker");
                AssertHelper.AreEqual("nv", copy.NextVersionIdMarker, "next version id marker");
                AssertHelper.HasCount(copy.Versions, 1, "versions");
                AssertHelper.AreEqual("v1", copy.Versions[0].VersionId, "version id");
                AssertHelper.AreEqual(128L, copy.Versions[0].Size.Value, "version size");
                AssertHelper.HasCount(copy.DeleteMarkers, 1, "delete markers");
                AssertHelper.AreEqual("v2", copy.DeleteMarkers[0].VersionId, "delete marker version id");
                AssertHelper.HasCount(copy.CommonPrefixes, 1, "common prefixes");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListVersionsResult validation and defaults", (ct) =>
            {
                AssertThrows<ArgumentNullException>(() => new ListVersionsResult(null, null, null, 0), "null name");
                AssertThrows<ArgumentOutOfRangeException>(() => new ListVersionsResult { MaxKeys = -1 }, "negative max keys");
                ListVersionsResult empty = new ListVersionsResult();
                AssertHelper.AreEqual("", empty.Prefix, "prefix null normalizes to empty");
                AssertHelper.IsFalse(empty.ShouldSerializeVersions(), "empty versions");
                AssertHelper.IsFalse(empty.ShouldSerializeDeleteMarkers(), "empty delete markers");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("DeleteMarker clears size and etag", (ct) =>
            {
                DeleteMarker marker = new DeleteMarker("k", "v", true, DateTime.UtcNow, new Owner());
                AssertHelper.IsNull(marker.Size, "delete marker size null");
                AssertHelper.IsNull(marker.ETag, "delete marker etag null");
                AssertHelper.IsFalse(marker.ShouldSerializeSize(), "should not serialize size");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListMultipartUploadsResult round-trips uploads through XML", (ct) =>
            {
                ListMultipartUploadsResult result = new ListMultipartUploadsResult();
                result.Bucket = "bucket";
                result.KeyMarker = "km";
                result.MaxUploads = 250;
                result.IsTruncated = true;
                result.NextKeyMarker = "nkm";
                result.Uploads = new List<Upload>
                {
                    new Upload { Key = "obj", UploadId = "u1", StorageClass = StorageClassEnum.STANDARD }
                };
                result.CommonPrefixes = new List<CommonPrefixes> { new CommonPrefixes("pre/") };

                ListMultipartUploadsResult copy = RoundTripXml(result);
                AssertHelper.AreEqual("bucket", copy.Bucket, "bucket");
                AssertHelper.AreEqual(250, copy.MaxUploads, "max uploads");
                AssertHelper.IsTrue(copy.IsTruncated, "truncated");
                AssertHelper.HasCount(copy.Uploads, 1, "uploads");
                AssertHelper.AreEqual("obj", copy.Uploads[0].Key, "upload key");
                AssertHelper.AreEqual("u1", copy.Uploads[0].UploadId, "upload id");
                AssertHelper.IsTrue(result.ShouldSerializeUploads(), "should serialize uploads");
                AssertHelper.IsTrue(result.ShouldSerializeCommonPrefixes(), "should serialize common prefixes");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListMultipartUploadsResult validates arguments and MaxUploads", (ct) =>
            {
                AssertThrows<ArgumentNullException>(() => new ListMultipartUploadsResult(null, "k", "u"), "null bucket");
                AssertThrows<ArgumentOutOfRangeException>(() => new ListMultipartUploadsResult { MaxUploads = 0 }, "zero max uploads");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ListPartsResult round-trips parts through XML", (ct) =>
            {
                ListPartsResult result = new ListPartsResult();
                result.Bucket = "bucket";
                result.Key = "key";
                result.UploadId = "u1";
                result.PartNumberMarker = 0;
                result.NextPartNumberMarker = 2;
                result.MaxParts = 100;
                result.IsTruncated = false;
                result.Parts = new List<Part>
                {
                    new Part { PartNumber = 1, ETag = "etag1", Size = 500 }
                };

                ListPartsResult copy = RoundTripXml(result);
                AssertHelper.AreEqual("bucket", copy.Bucket, "bucket");
                AssertHelper.AreEqual("u1", copy.UploadId, "upload id");
                AssertHelper.AreEqual(2, copy.NextPartNumberMarker, "next part number marker");
                AssertHelper.HasCount(copy.Parts, 1, "parts");
                AssertHelper.AreEqual(1, copy.Parts[0].PartNumber, "part number");
                AssertThrows<ArgumentOutOfRangeException>(() => new ListPartsResult { MaxParts = -1 }, "negative max parts");
                AssertThrows<ArgumentOutOfRangeException>(() => new ListPartsResult { PartNumberMarker = -1 }, "negative part number marker");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("CompleteMultipartUploadResult round-trips through XML", (ct) =>
            {
                CompleteMultipartUploadResult result = new CompleteMultipartUploadResult();
                result.Location = "http://bucket.s3.amazonaws.com/key";
                result.Bucket = "bucket";
                result.Key = "key";
                result.ETag = "final-etag";
                result.ChecksumSHA256 = "sha";

                CompleteMultipartUploadResult copy = RoundTripXml(result);
                AssertHelper.AreEqual("bucket", copy.Bucket, "bucket");
                AssertHelper.AreEqual("key", copy.Key, "key");
                AssertHelper.AreEqual("\"final-etag\"", copy.ETag, "etag normalized");
                AssertHelper.AreEqual("sha", copy.ChecksumSHA256, "checksum");
                AssertHelper.IsTrue(result.ShouldSerializeChecksumSHA256(), "should serialize sha256");
                AssertHelper.IsFalse(result.ShouldSerializeChecksumCRC32(), "should not serialize crc32");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion
        }

        private static T RoundTripXml<T>(T obj) where T : class
        {
            string xml = SerializationHelper.SerializeXml(obj);
            AssertHelper.IsTrue(!String.IsNullOrEmpty(xml), "serialized xml for " + typeof(T).Name);
            return SerializationHelper.DeserializeXml<T>(xml);
        }

        private static void AssertThrows<TException>(Action action, string name) where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }
            catch (Exception ex)
            {
                throw new Exception($"Expected {typeof(TException).Name} for {name}, but got {ex.GetType().Name}.");
            }

            throw new Exception($"Expected {typeof(TException).Name} for {name}, but no exception was thrown.");
        }
    }
}
