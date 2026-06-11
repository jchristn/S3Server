namespace Test.Shared
{
    using S3ServerLibrary;

    /// <summary>
    /// Immutable snapshot of parsed S3 request state captured by the shared test server.
    /// </summary>
    public sealed class S3RequestObservation
    {
        /// <summary>
        /// HTTP method.
        /// </summary>
        public string Method { get; private set; }

        /// <summary>
        /// Full request URL as seen by the webserver.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Parsed request type.
        /// </summary>
        public S3RequestType RequestType { get; private set; }

        /// <summary>
        /// Parsed request style.
        /// </summary>
        public S3RequestStyle RequestStyle { get; private set; }

        /// <summary>
        /// Parsed bucket.
        /// </summary>
        public string Bucket { get; private set; }

        /// <summary>
        /// Parsed object key.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Parsed prefix.
        /// </summary>
        public string Prefix { get; private set; }

        /// <summary>
        /// Parsed delimiter.
        /// </summary>
        public string Delimiter { get; private set; }

        /// <summary>
        /// Parsed marker.
        /// </summary>
        public string Marker { get; private set; }

        /// <summary>
        /// Parsed continuation token.
        /// </summary>
        public string ContinuationToken { get; private set; }

        /// <summary>
        /// Parsed upload ID.
        /// </summary>
        public string UploadId { get; private set; }

        /// <summary>
        /// Parsed version ID.
        /// </summary>
        public string VersionId { get; private set; }

        /// <summary>
        /// Parsed maximum key count.
        /// </summary>
        public int MaxKeys { get; private set; }

        /// <summary>
        /// Parsed maximum part count.
        /// </summary>
        public int MaxParts { get; private set; }

        /// <summary>
        /// Parsed part number.
        /// </summary>
        public int PartNumber { get; private set; }

        /// <summary>
        /// Parsed part-number marker.
        /// </summary>
        public int PartNumberMarker { get; private set; }

        /// <summary>
        /// Parsed range start.
        /// </summary>
        public long? RangeStart { get; private set; }

        /// <summary>
        /// Parsed range end.
        /// </summary>
        public long? RangeEnd { get; private set; }

        /// <summary>
        /// Parsed authorization header.
        /// </summary>
        public string Authorization { get; private set; }

        /// <summary>
        /// Parsed signature version.
        /// </summary>
        public S3SignatureVersion SignatureVersion { get; private set; }

        /// <summary>
        /// Parsed signature.
        /// </summary>
        public string Signature { get; private set; }

        /// <summary>
        /// Parsed access key.
        /// </summary>
        public string AccessKey { get; private set; }

        /// <summary>
        /// Parsed expiration value.
        /// </summary>
        public string Expires { get; private set; }

        /// <summary>
        /// Parsed date value.
        /// </summary>
        public string Date { get; private set; }

        /// <summary>
        /// Parsed content type.
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// Parsed content MD5.
        /// </summary>
        public string ContentMd5 { get; private set; }

        /// <summary>
        /// Parsed content SHA256.
        /// </summary>
        public string ContentSha256 { get; private set; }

        /// <summary>
        /// Parsed content length.
        /// </summary>
        public long ContentLength { get; private set; }

        /// <summary>
        /// Parsed Host header.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Parsed hostname.
        /// </summary>
        public string Hostname { get; private set; }

        /// <summary>
        /// True if the request is service-level.
        /// </summary>
        public bool IsServiceRequest { get; private set; }

        /// <summary>
        /// True if the request is bucket-level.
        /// </summary>
        public bool IsBucketRequest { get; private set; }

        /// <summary>
        /// True if the request is object-level.
        /// </summary>
        public bool IsObjectRequest { get; private set; }

        /// <summary>
        /// True if the request is multipart-related.
        /// </summary>
        public bool IsMultipartUploadRequest { get; private set; }

        /// <summary>
        /// Permissions the request normally requires.
        /// </summary>
        public S3PermissionType PermissionsRequired { get; private set; }

        /// <summary>
        /// Create an observation from a live context.
        /// </summary>
        /// <param name="ctx">S3 context.</param>
        /// <returns>Observation.</returns>
        public static S3RequestObservation From(S3Context ctx)
        {
            S3Request req = ctx.Request;

            return new S3RequestObservation
            {
                Method = ctx.Http.Request.Method.ToString(),
                Url = ctx.Http.Request.Url.Full,
                RequestType = req.RequestType,
                RequestStyle = req.RequestStyle,
                Bucket = req.Bucket,
                Key = req.Key,
                Prefix = req.Prefix,
                Delimiter = req.Delimiter,
                Marker = req.Marker,
                ContinuationToken = req.ContinuationToken,
                UploadId = req.UploadId,
                VersionId = req.VersionId,
                MaxKeys = req.MaxKeys,
                MaxParts = req.MaxParts,
                PartNumber = req.PartNumber,
                PartNumberMarker = req.PartNumberMarker,
                RangeStart = req.RangeStart,
                RangeEnd = req.RangeEnd,
                Authorization = req.Authorization,
                SignatureVersion = req.SignatureVersion,
                Signature = req.Signature,
                AccessKey = req.AccessKey,
                Expires = req.Expires,
                Date = req.Date,
                ContentType = req.ContentType,
                ContentMd5 = req.ContentMd5,
                ContentSha256 = req.ContentSha256,
                ContentLength = req.ContentLength,
                Host = req.Host,
                Hostname = req.Hostname,
                IsServiceRequest = req.IsServiceRequest,
                IsBucketRequest = req.IsBucketRequest,
                IsObjectRequest = req.IsObjectRequest,
                IsMultipartUploadRequest = req.IsMultipartUploadRequest,
                PermissionsRequired = req.PermissionsRequired
            };
        }
    }
}
