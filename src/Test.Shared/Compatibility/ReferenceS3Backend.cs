namespace Test.Shared.Compatibility
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using S3ServerLibrary;
    using S3ServerLibrary.S3Objects;
    using WatsonWebserver.Core;

    /// <summary>
    /// In-memory S3 storage wired to an S3Server's callbacks, used as the reference implementation for the compatibility
    /// scenarios and by Test.Compatibility --serve.  It is intentionally simple: protocol behavior (range semantics,
    /// parameter validation, XML shape, encoding-type) is left to S3Server so the scenarios exercise the library.
    /// Conditional requests are evaluated in the order the S3Request property documentation describes.
    /// Thread-safe for concurrent requests.
    /// </summary>
    public class ReferenceS3Backend
    {
        #region Public-Members

        /// <summary>
        /// Owner reported in listings and ACLs.
        /// </summary>
        public Owner Owner { get; set; } = new Owner("reference-owner-id", "reference");

        #endregion

        #region Private-Members

        private ConcurrentDictionary<string, ConcurrentDictionary<string, ReferenceObject>> _Buckets =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, ReferenceObject>>(StringComparer.Ordinal);

        private ConcurrentDictionary<string, ReferenceUpload> _Uploads = new ConcurrentDictionary<string, ReferenceUpload>(StringComparer.Ordinal);

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a bucket if it does not exist.
        /// </summary>
        /// <param name="bucket">Bucket name.</param>
        public void CreateBucket(string bucket)
        {
            _Buckets.TryAdd(bucket, new ConcurrentDictionary<string, ReferenceObject>(StringComparer.Ordinal));
        }

        /// <summary>
        /// Attach the backend to an S3Server's callbacks.
        /// </summary>
        /// <param name="server">Server.  Cannot be null.</param>
        public void Attach(S3Server server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            server.Service.ListBuckets = async (ctx) =>
            {
                ListAllMyBucketsResult r = new ListAllMyBucketsResult();
                r.Owner = Owner;
                r.Buckets = new Buckets(_Buckets.Keys.OrderBy(k => k, StringComparer.Ordinal).Select(k => new Bucket(k, DateTime.UtcNow)).ToList());
                return r;
            };

            server.Bucket.Write = async (ctx) => CreateBucket(ctx.Request.Bucket);
            server.Bucket.Exists = async (ctx) => _Buckets.ContainsKey(ctx.Request.Bucket);
            server.Bucket.ReadLocation = async (ctx) => { GetBucket(ctx.Request.Bucket); return new LocationConstraint("us-west-1"); };
            server.Bucket.Read = async (ctx) => List(ctx);
            server.Bucket.ReadVersions = async (ctx) => ListVersions(ctx);
            server.Bucket.ReadMultipartUploads = async (ctx) => ListUploads(ctx);

            server.Object.Write = async (ctx) =>
            {
                ConcurrentDictionary<string, ReferenceObject> b = GetBucket(ctx.Request.Bucket);
                byte[] data = await ReadBody(ctx).ConfigureAwait(false);
                ReferenceObject o = new ReferenceObject
                {
                    Data = data,
                    ETag = Md5ETag(data),
                    LastModified = DateTime.UtcNow,
                    ContentType = String.IsNullOrEmpty(ctx.Request.ContentType) ? "binary/octet-stream" : ctx.Request.ContentType
                };
                b[ctx.Request.Key] = o;
                ctx.Response.Headers.Add("ETag", o.ETag);
            };

            server.Object.Exists = async (ctx) =>
            {
                if (!GetBucket(ctx.Request.Bucket).TryGetValue(ctx.Request.Key, out ReferenceObject o)) return null;
                EvaluateConditions(ctx, o);
                ObjectMetadata md = new ObjectMetadata(ctx.Request.Key, o.LastModified, o.ETag, o.Data.Length, Owner);
                md.ContentType = o.ContentType;
                return md;
            };

            server.Object.Read = async (ctx) =>
            {
                ReferenceObject o = GetObject(ctx.Request.Bucket, ctx.Request.Key);
                EvaluateConditions(ctx, o);
                return ToS3Object(ctx.Request.Key, o, o.Data);
            };

            server.Object.ReadRange = async (ctx) =>
            {
                ReferenceObject o = GetObject(ctx.Request.Bucket, ctx.Request.Key);
                EvaluateConditions(ctx, o);

                long total = o.Data.Length;
                long start;
                long end;

                if (ctx.Request.RangeSuffixLength != null)
                {
                    long n = Math.Min(ctx.Request.RangeSuffixLength.Value, total);
                    start = total - n;
                    end = total - 1;
                }
                else
                {
                    start = Math.Min(ctx.Request.RangeStart.Value, total);
                    end = Math.Min(ctx.Request.RangeEnd ?? (total - 1), total - 1);
                }

                byte[] slice = end >= start ? o.Data.Skip((int)start).Take((int)(end - start + 1)).ToArray() : Array.Empty<byte>();
                return ToS3Object(ctx.Request.Key, o, slice);
            };

            server.Object.Delete = async (ctx) => { GetBucket(ctx.Request.Bucket).TryRemove(ctx.Request.Key, out _); };

            server.Object.DeleteMultiple = async (ctx, del) =>
            {
                ConcurrentDictionary<string, ReferenceObject> b = GetBucket(ctx.Request.Bucket);
                List<Deleted> deleted = new List<Deleted>();
                foreach (S3ServerLibrary.S3Objects.Object item in del.Objects)
                {
                    b.TryRemove(item.Key, out _);
                    deleted.Add(new Deleted(item.Key, null, null));
                }

                return new DeleteResult(deleted, null);
            };

            server.Object.Copy = async (ctx) =>
            {
                ReferenceObject src = GetObject(ctx.Request.CopySourceBucket, ctx.Request.CopySourceKey);
                if (ctx.Request.CopySourceIfMatch != null && !ctx.Request.CopySourceIfMatch.Contains("*") && !ctx.Request.CopySourceIfMatch.Contains(src.ETag))
                    throw new S3Exception(new Error(ErrorCode.PreconditionFailed) { Condition = "x-amz-copy-source-If-Match" });

                ReferenceObject copy = new ReferenceObject { Data = src.Data.ToArray(), ETag = src.ETag, LastModified = DateTime.UtcNow, ContentType = src.ContentType };
                GetBucket(ctx.Request.Bucket)[ctx.Request.Key] = copy;
                return new CopyObjectResult(copy.ETag, copy.LastModified);
            };

            server.Object.ReadAcl = async (ctx) =>
            {
                GetObject(ctx.Request.Bucket, ctx.Request.Key);
                return new AccessControlPolicy(Owner, new AccessControlList(new List<Grant>
                {
                    new Grant(new Grantee(Owner.ID, Owner.DisplayName, null, "CanonicalUser", null), PermissionEnum.FullControl)
                }));
            };

            server.Object.WriteAcl = async (ctx, acp) => { GetObject(ctx.Request.Bucket, ctx.Request.Key); };
            server.Object.ReadTagging = async (ctx) => { GetObject(ctx.Request.Bucket, ctx.Request.Key); return new Tagging(new TagSet(new List<Tag>())); };

            server.Object.CreateMultipartUpload = async (ctx) =>
            {
                GetBucket(ctx.Request.Bucket);
                ReferenceUpload upload = new ReferenceUpload { Bucket = ctx.Request.Bucket, Key = ctx.Request.Key, UploadId = Guid.NewGuid().ToString("N"), Initiated = DateTime.UtcNow };
                _Uploads[upload.UploadId] = upload;
                return new InitiateMultipartUploadResult(ctx.Request.Bucket, ctx.Request.Key, upload.UploadId);
            };

            server.Object.UploadPart = async (ctx) =>
            {
                ReferenceUpload upload = GetUpload(ctx.Request.UploadId);
                byte[] data = await ReadBody(ctx).ConfigureAwait(false);
                ReferenceObject part = new ReferenceObject { Data = data, ETag = Md5ETag(data), LastModified = DateTime.UtcNow };
                upload.Parts[ctx.Request.PartNumber] = part;
                ctx.Response.Headers.Add("ETag", part.ETag);
            };

            server.Object.UploadPartCopy = async (ctx) =>
            {
                ReferenceUpload upload = GetUpload(ctx.Request.UploadId);
                ReferenceObject src = GetObject(ctx.Request.CopySourceBucket, ctx.Request.CopySourceKey);
                byte[] data = src.Data;
                if (ctx.Request.CopySourceRangeStart != null)
                {
                    long start = ctx.Request.CopySourceRangeStart.Value;
                    long end = ctx.Request.CopySourceRangeEnd.Value;
                    if (start >= data.Length || end >= data.Length) throw new S3Exception(new Error(ErrorCode.InvalidRange));
                    data = data.Skip((int)start).Take((int)(end - start + 1)).ToArray();
                }

                ReferenceObject part = new ReferenceObject { Data = data, ETag = Md5ETag(data), LastModified = DateTime.UtcNow };
                upload.Parts[ctx.Request.PartNumber] = part;
                return new CopyPartResult(part.ETag, part.LastModified);
            };

            server.Object.ReadParts = async (ctx) =>
            {
                ReferenceUpload upload = GetUpload(ctx.Request.UploadId);
                List<int> numbers = upload.Parts.Keys.Where(n => n > ctx.Request.PartNumberMarker).OrderBy(n => n).ToList();
                List<int> page = numbers.Take(ctx.Request.MaxParts).ToList();

                ListPartsResult r = new ListPartsResult();
                r.Bucket = upload.Bucket;
                r.Key = upload.Key;
                r.UploadId = upload.UploadId;
                r.Owner = Owner;
                r.Initiator = Owner;
                r.StorageClass = StorageClassEnum.STANDARD;
                r.PartNumberMarker = ctx.Request.PartNumberMarker;
                r.MaxParts = ctx.Request.MaxParts;
                r.IsTruncated = numbers.Count > page.Count;
                if (page.Count > 0) r.NextPartNumberMarker = page.Last();
                r.Parts = page.Select(n => new Part { PartNumber = n, LastModified = upload.Parts[n].LastModified, ETag = upload.Parts[n].ETag, Size = upload.Parts[n].Data.Length }).ToList();
                return r;
            };

            server.Object.CompleteMultipartUpload = async (ctx, complete) =>
            {
                ReferenceUpload upload = GetUpload(ctx.Request.UploadId);
                byte[] data = upload.Parts.OrderBy(p => p.Key).SelectMany(p => p.Value.Data).ToArray();
                ReferenceObject o = new ReferenceObject { Data = data, ETag = "\"" + Md5ETag(data).Trim('"') + "-" + upload.Parts.Count + "\"", LastModified = DateTime.UtcNow };
                GetBucket(upload.Bucket)[upload.Key] = o;
                _Uploads.TryRemove(upload.UploadId, out _);

                CompleteMultipartUploadResult r = new CompleteMultipartUploadResult();
                r.Bucket = upload.Bucket;
                r.Key = upload.Key;
                r.ETag = o.ETag;
                r.Location = "/" + upload.Bucket + "/" + upload.Key;
                return r;
            };

            server.Object.AbortMultipartUpload = async (ctx) =>
            {
                GetUpload(ctx.Request.UploadId);
                _Uploads.TryRemove(ctx.Request.UploadId, out _);
            };
        }

        #endregion

        #region Private-Methods

        private ConcurrentDictionary<string, ReferenceObject> GetBucket(string bucket)
        {
            if (bucket == null || !_Buckets.TryGetValue(bucket, out ConcurrentDictionary<string, ReferenceObject> b))
                throw new S3Exception(new Error(ErrorCode.NoSuchBucket));
            return b;
        }

        private ReferenceObject GetObject(string bucket, string key)
        {
            if (key == null || !GetBucket(bucket).TryGetValue(key, out ReferenceObject o))
                throw new S3Exception(new Error(ErrorCode.NoSuchKey, key));
            return o;
        }

        private ReferenceUpload GetUpload(string uploadId)
        {
            if (uploadId == null || !_Uploads.TryGetValue(uploadId, out ReferenceUpload upload))
                throw new S3Exception(new Error(ErrorCode.NoSuchUpload));
            return upload;
        }

        private S3ServerLibrary.S3Object ToS3Object(string key, ReferenceObject o, byte[] data)
        {
            S3ServerLibrary.S3Object obj = new S3ServerLibrary.S3Object(key, null, true, o.LastModified, o.ETag, data.Length, Owner, data, o.ContentType);
            obj.TotalSize = o.Data.Length;
            return obj;
        }

        private void EvaluateConditions(S3Context ctx, ReferenceObject o)
        {
            S3Request r = ctx.Request;
            DateTime modified = o.LastModified.AddTicks(-(o.LastModified.Ticks % TimeSpan.TicksPerSecond));
            bool readMethod = ctx.Http.Request.Method == HttpMethod.GET || ctx.Http.Request.Method == HttpMethod.HEAD;

            if (r.IfMatch != null)
            {
                if (!r.IfMatch.Contains("*") && !r.IfMatch.Contains(o.ETag))
                    throw new S3Exception(new Error(ErrorCode.PreconditionFailed) { Condition = "If-Match" });
            }
            else if (r.IfUnmodifiedSince != null && modified > r.IfUnmodifiedSince.Value)
            {
                throw new S3Exception(new Error(ErrorCode.PreconditionFailed) { Condition = "If-Unmodified-Since" });
            }

            if (r.IfNoneMatch != null)
            {
                if (r.IfNoneMatch.Contains("*") || r.IfNoneMatch.Select(t => t.StartsWith("W/", StringComparison.Ordinal) ? t.Substring(2) : t).Contains(o.ETag))
                {
                    if (!readMethod) throw new S3Exception(new Error(ErrorCode.PreconditionFailed) { Condition = "If-None-Match" });
                    NotModified(ctx, o);
                }
            }
            else if (r.IfModifiedSince != null && readMethod && modified <= r.IfModifiedSince.Value)
            {
                NotModified(ctx, o);
            }
        }

        private static void NotModified(S3Context ctx, ReferenceObject o)
        {
            ctx.Response.Headers.Add("ETag", o.ETag);
            ctx.Response.Headers.Add("Last-Modified", o.LastModified.ToString("r"));
            throw new S3Exception(new Error(ErrorCode.NotModified));
        }

        private ListBucketResult List(S3Context ctx)
        {
            ConcurrentDictionary<string, ReferenceObject> b = GetBucket(ctx.Request.Bucket);
            bool v2 = ctx.Request.RetrieveQueryValue("list-type") == "2";
            string prefix = ctx.Request.Prefix ?? "";
            string delimiter = ctx.Request.Delimiter;
            string after = v2 ? (ctx.Request.ContinuationToken ?? ctx.Request.StartAfter) : ctx.Request.Marker;
            int max = ctx.Request.MaxKeys;

            ListBucketResult r = new ListBucketResult();
            r.Name = ctx.Request.Bucket;
            r.Prefix = prefix;
            r.Delimiter = delimiter;
            r.MaxKeys = max;
            r.Marker = v2 ? null : ctx.Request.Marker;
            r.ContinuationToken = v2 ? ctx.Request.ContinuationToken : null;
            r.StartAfter = v2 ? ctx.Request.StartAfter : null;

            string last = null;
            HashSet<string> seenPrefixes = new HashSet<string>(StringComparer.Ordinal);

            foreach (string key in b.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).OrderBy(k => k, StringComparer.Ordinal))
            {
                string entry = key;
                bool isPrefix = false;

                if (!String.IsNullOrEmpty(delimiter))
                {
                    int i = key.IndexOf(delimiter, prefix.Length, StringComparison.Ordinal);
                    if (i >= 0)
                    {
                        entry = key.Substring(0, i + delimiter.Length);
                        isPrefix = true;
                    }
                }

                if (after != null && String.CompareOrdinal(entry, after) <= 0) continue;
                if (isPrefix && seenPrefixes.Contains(entry)) continue;

                if (r.Contents.Count + seenPrefixes.Count >= max)
                {
                    r.IsTruncated = true;
                    break;
                }

                if (isPrefix)
                {
                    seenPrefixes.Add(entry);
                    r.CommonPrefixes.Add(new CommonPrefixes(entry));
                }
                else
                {
                    ReferenceObject o = b[key];
                    r.Contents.Add(new ObjectMetadata(key, o.LastModified, o.ETag, o.Data.Length, Owner));
                }

                last = entry;
            }

            r.KeyCount = r.Contents.Count + r.CommonPrefixes.Count;
            if (r.IsTruncated)
            {
                if (v2) r.NextContinuationToken = last;
                else if (!String.IsNullOrEmpty(delimiter)) r.NextMarker = last;
            }

            return r;
        }

        private ListVersionsResult ListVersions(S3Context ctx)
        {
            ConcurrentDictionary<string, ReferenceObject> b = GetBucket(ctx.Request.Bucket);
            string prefix = ctx.Request.Prefix ?? "";
            ListVersionsResult r = new ListVersionsResult();
            r.Name = ctx.Request.Bucket;
            r.Prefix = prefix;
            r.KeyMarker = ctx.Request.KeyMarker ?? "";
            r.VersionIdMarker = ctx.Request.VersionIdMarker ?? "";
            r.MaxKeys = ctx.Request.MaxKeys;

            foreach (string key in b.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).OrderBy(k => k, StringComparer.Ordinal))
            {
                if (ctx.Request.KeyMarker != null && String.CompareOrdinal(key, ctx.Request.KeyMarker) <= 0) continue;
                if (r.Entries.Count >= r.MaxKeys)
                {
                    r.IsTruncated = true;
                    break;
                }

                ReferenceObject o = b[key];
                r.Entries.Add(new ObjectVersion(key, "null", true, o.LastModified, o.ETag, o.Data.Length, Owner));
                r.NextKeyMarker = key;
                r.NextVersionIdMarker = "null";
            }

            if (!r.IsTruncated)
            {
                r.NextKeyMarker = null;
                r.NextVersionIdMarker = null;
            }

            return r;
        }

        private ListMultipartUploadsResult ListUploads(S3Context ctx)
        {
            GetBucket(ctx.Request.Bucket);
            string prefix = ctx.Request.Prefix ?? "";

            ListMultipartUploadsResult r = new ListMultipartUploadsResult();
            r.Bucket = ctx.Request.Bucket;
            r.Prefix = prefix;
            r.KeyMarker = ctx.Request.KeyMarker ?? "";
            r.UploadIdMarker = ctx.Request.UploadIdMarker ?? "";
            r.MaxUploads = ctx.Request.MaxUploads;
            r.Uploads = _Uploads.Values
                .Where(u => u.Bucket == ctx.Request.Bucket && u.Key.StartsWith(prefix, StringComparison.Ordinal))
                .OrderBy(u => u.Key, StringComparer.Ordinal)
                .ThenBy(u => u.Initiated)
                .Take(r.MaxUploads)
                .Select(u => new Upload { Key = u.Key, UploadId = u.UploadId, Owner = Owner, Initiator = Owner, Initiated = u.Initiated, StorageClass = StorageClassEnum.STANDARD })
                .ToList();
            return r;
        }

        private static async Task<byte[]> ReadBody(S3Context ctx)
        {
            if (ctx.Request.Chunked)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    while (true)
                    {
                        Chunk chunk = await ctx.Request.ReadChunk().ConfigureAwait(false);
                        if (chunk == null) break;
                        if (chunk.Data != null && chunk.Length > 0) ms.Write(chunk.Data, 0, chunk.Length);
                        if (chunk.IsFinal) break;
                    }

                    return ms.ToArray();
                }
            }

            if (ctx.Request.ContentLength < 1) return Array.Empty<byte>();
            return ctx.Request.DataAsBytes ?? Array.Empty<byte>();
        }

        private static string Md5ETag(byte[] data)
        {
            return "\"" + Convert.ToHexString(MD5.HashData(data)).ToLowerInvariant() + "\"";
        }

        #endregion
    }
}
