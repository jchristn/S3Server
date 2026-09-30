namespace S3ServerLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using PrettyId;
    using S3ServerLibrary.S3Objects;
    using WatsonWebserver.Core;

    /// <summary>
    /// S3 request.
    /// </summary>
    public class S3Request
    {
        #region Public-Members

        /// <summary>
        /// Request ID.
        /// </summary>
        public string RequestId { get; set; } = _IdGenerator.GenerateUrlSafe("req_", 32);

        /// <summary>
        /// Trace ID.
        /// </summary>
        public string TraceId { get; set; } = _IdGenerator.GenerateUrlSafe("trace_", 32);

        /// <summary>
        /// Indicates if the request includes the bucket name in the hostname or not.
        /// </summary>
        public S3RequestStyle RequestStyle { get; private set; } = S3RequestStyle.Unknown;

        /// <summary>
        /// Indicates the type of S3 request.
        /// </summary>
        public S3RequestType RequestType { get; private set; } = S3RequestType.Unknown;

        /// <summary>
        /// Indicates if chunked transfer-encoding is in use.
        /// </summary>
        public bool Chunked { get; set; } = false;

        /// <summary>
        /// AWS region.
        /// </summary>
        public string Region { get; set; } = null;

        /// <summary>
        /// Hostname.
        /// </summary>
        public string Hostname { get; set; } = null;

        /// <summary>
        /// Host header value.
        /// </summary>
        public string Host { get; set; } = null;

        /// <summary>
        /// Base domain identified in the hostname.
        /// </summary>
        public string BaseDomain { get; set; } = null;

        /// <summary>
        /// Bucket.
        /// </summary>
        public string Bucket { get; set; } = null;

        /// <summary>
        /// Object key.
        /// </summary>
        public string Key { get; set; } = null;

        /// <summary>
        /// Object key prefix.
        /// </summary>
        public string Prefix { get; set; } = null;

        /// <summary>
        /// Delimiter.
        /// </summary>
        public string Delimiter { get; set; } = null;

        /// <summary>
        /// Marker.
        /// </summary>
        public string Marker { get; set; } = null;

        /// <summary>
        /// Part number from a multipart upload.
        /// </summary>
        public int PartNumber
        {
            get
            {
                return _PartNumber;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(PartNumber));
                _PartNumber = value;
            }
        }

        /// <summary>
        /// Part number arker.
        /// </summary>
        public int PartNumberMarker
        {
            get
            {
                return _PartNumberMarker;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(PartNumberMarker));
                _PartNumberMarker = value;
            }
        }

        /// <summary>
        /// Maximum number of keys to retrieve in an enumeration.
        /// </summary>
        public int MaxKeys
        {
            get
            {
                return _MaxKeys;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(MaxKeys));
                _MaxKeys = value;
            }
        }

        /// <summary>
        /// Maximum number of parts to retrieve in an enumeration.
        /// </summary>
        public int MaxParts
        {
            get
            {
                return _MaxParts;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(MaxParts));
                _MaxParts = value;
            }
        }

        /// <summary>
        /// Object version ID.
        /// </summary>
        public string VersionId { get; set; } = null;

        /// <summary>
        /// Upload ID.
        /// </summary>
        public string UploadId { get; set; } = null;

        /// <summary>
        /// Authorization header string, in full.
        /// </summary>
        public string Authorization { get; set; } = null;

        /// <summary>
        /// Signature version from authorization header.
        /// </summary>
        public S3SignatureVersion SignatureVersion { get; set; } = S3SignatureVersion.Unknown;

        /// <summary>
        /// Signature from authorization header.
        /// </summary>
        public string Signature { get; set; } = null;

        /// <summary>
        /// Content type.
        /// </summary>
        public string ContentType { get; set; } = null;

        /// <summary>
        /// Content MD5 hash from request headers.
        /// </summary>
        public string ContentMd5 { get; set; } = null;

        /// <summary>
        /// Content SHA256 hash from request headers.
        /// </summary>
        public string ContentSha256 { get; set; } = null;

        /// <summary>
        /// Content length.
        /// </summary>
        public long ContentLength
        {
            get
            {
                if (_HttpRequest != null) return _HttpRequest.ContentLength;
                return 0;
            }
        }

        /// <summary>
        /// Date parameter.
        /// </summary>
        public string Date { get; set; } = null;

        /// <summary>
        /// Expiration parameter from authorization header.
        /// </summary>
        public string Expires { get; set; } = null;

        /// <summary>
        /// Access key, parsed from authorization header.
        /// </summary>
        public string AccessKey { get; set; } = null;

        /// <summary>
        /// Start value from the Range header.
        /// </summary>
        public long? RangeStart
        {
            get
            {
                return _RangeStart;
            }
            set
            {
                if (value != null && value.Value < 0)
                    throw new ArgumentOutOfRangeException(nameof(RangeStart));
                _RangeStart = value;
            }
        }

        /// <summary>
        /// End value from the Range header.
        /// </summary>
        public long? RangeEnd
        {
            get
            {
                return _RangeEnd;
            }
            set
            {
                if (value != null && value.Value < 0)
                    throw new ArgumentOutOfRangeException(nameof(RangeEnd));
                _RangeEnd = value;
            }
        }

        /// <summary>
        /// Suffix length from a suffix Range header (<c>bytes=-N</c>), meaning the last N bytes of the object.
        /// When set, RangeStart and RangeEnd are null.  Null when the Range header is absent, unparseable, or not a suffix range.
        /// Minimum value is 0.
        /// </summary>
        public long? RangeSuffixLength
        {
            get
            {
                return _RangeSuffixLength;
            }
            set
            {
                if (value != null && value.Value < 0)
                    throw new ArgumentOutOfRangeException(nameof(RangeSuffixLength));
                _RangeSuffixLength = value;
            }
        }

        /// <summary>
        /// Continuation token.
        /// </summary>
        public string ContinuationToken { get; set; } = null;

        /// <summary>
        /// Start-after key from a ListObjectsV2 request (the <c>start-after</c> querystring parameter), URL-decoded.
        /// Null when not supplied.
        /// </summary>
        public string StartAfter { get; set; } = null;

        /// <summary>
        /// Indicates whether a ListObjectsV2 request asked for owner information (<c>fetch-owner=true</c>).
        /// Default is false.
        /// </summary>
        public bool FetchOwner { get; set; } = false;

        /// <summary>
        /// Source bucket parsed from the x-amz-copy-source header, URL-decoded.
        /// Null unless the request is a copy (ObjectCopy or ObjectUploadPartCopy).
        /// </summary>
        public string CopySourceBucket { get; set; } = null;

        /// <summary>
        /// Source object key parsed from the x-amz-copy-source header, URL-decoded.
        /// Null unless the request is a copy (ObjectCopy or ObjectUploadPartCopy).
        /// </summary>
        public string CopySourceKey { get; set; } = null;

        /// <summary>
        /// Source version ID parsed from the <c>versionId</c> parameter of the x-amz-copy-source header.
        /// Null when the header does not name a version.
        /// </summary>
        public string CopySourceVersionId { get; set; } = null;

        /// <summary>
        /// First byte of the source range from the x-amz-copy-source-range header (UploadPartCopy).
        /// Null when the header is absent.  Minimum value is 0.
        /// </summary>
        public long? CopySourceRangeStart
        {
            get
            {
                return _CopySourceRangeStart;
            }
            set
            {
                if (value != null && value.Value < 0)
                    throw new ArgumentOutOfRangeException(nameof(CopySourceRangeStart));
                _CopySourceRangeStart = value;
            }
        }

        /// <summary>
        /// Last byte (inclusive) of the source range from the x-amz-copy-source-range header (UploadPartCopy).
        /// Null when the header is absent.  Minimum value is 0.
        /// </summary>
        public long? CopySourceRangeEnd
        {
            get
            {
                return _CopySourceRangeEnd;
            }
            set
            {
                if (value != null && value.Value < 0)
                    throw new ArgumentOutOfRangeException(nameof(CopySourceRangeEnd));
                _CopySourceRangeEnd = value;
            }
        }

        /// <summary>
        /// Entity tags from the If-Match header, exactly as sent (quotes and any <c>W/</c> prefix preserved), or a single <c>*</c>.
        /// Null when the header is absent.
        /// Evaluation is the responsibility of the callback.  Per RFC 9110 section 13.2.2, evaluate in this order:
        /// If-Match (failure: 412 PreconditionFailed), then If-Unmodified-Since only when If-Match is absent (failure: 412),
        /// then If-None-Match (failure: 304 NotModified for GET and HEAD, otherwise 412),
        /// then If-Modified-Since only when If-None-Match is absent and the method is GET or HEAD (failure: 304 NotModified).
        /// If-Match uses strong comparison; If-None-Match uses weak comparison.
        /// </summary>
        public List<string> IfMatch { get; set; } = null;

        /// <summary>
        /// Entity tags from the If-None-Match header, exactly as sent (quotes and any <c>W/</c> prefix preserved), or a single <c>*</c>.
        /// Null when the header is absent.  See IfMatch for the evaluation order.
        /// </summary>
        public List<string> IfNoneMatch { get; set; } = null;

        /// <summary>
        /// Timestamp (UTC) from the If-Modified-Since header.
        /// Null when the header is absent or not a valid HTTP date (RFC 9110 requires an invalid date to be ignored).
        /// See IfMatch for the evaluation order.
        /// </summary>
        public DateTime? IfModifiedSince { get; set; } = null;

        /// <summary>
        /// Timestamp (UTC) from the If-Unmodified-Since header.
        /// Null when the header is absent or not a valid HTTP date (RFC 9110 requires an invalid date to be ignored).
        /// See IfMatch for the evaluation order.
        /// </summary>
        public DateTime? IfUnmodifiedSince { get; set; } = null;

        /// <summary>
        /// Entity tags from the x-amz-copy-source-if-match header.  Null when absent.
        /// Applies to the copy source.  A failed condition should be answered with 412 PreconditionFailed.
        /// </summary>
        public List<string> CopySourceIfMatch { get; set; } = null;

        /// <summary>
        /// Entity tags from the x-amz-copy-source-if-none-match header.  Null when absent.
        /// Applies to the copy source.  A failed condition should be answered with 412 PreconditionFailed.
        /// </summary>
        public List<string> CopySourceIfNoneMatch { get; set; } = null;

        /// <summary>
        /// Timestamp (UTC) from the x-amz-copy-source-if-modified-since header.  Null when absent or not a valid HTTP date.
        /// Applies to the copy source.  A failed condition should be answered with 412 PreconditionFailed.
        /// </summary>
        public DateTime? CopySourceIfModifiedSince { get; set; } = null;

        /// <summary>
        /// Timestamp (UTC) from the x-amz-copy-source-if-unmodified-since header.  Null when absent or not a valid HTTP date.
        /// Applies to the copy source.  A failed condition should be answered with 412 PreconditionFailed.
        /// </summary>
        public DateTime? CopySourceIfUnmodifiedSince { get; set; } = null;

        /// <summary>
        /// Indicates if the request is a service request.
        /// </summary>
        public bool IsServiceRequest
        {
            get
            {
                if (RequestType == S3RequestType.ListBuckets)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Indicates if the request is a bucket request.
        /// </summary>
        public bool IsBucketRequest
        {
            get
            {
                if (RequestType == S3RequestType.BucketDelete
                    || RequestType == S3RequestType.BucketDeleteTags
                    || RequestType == S3RequestType.BucketDeleteWebsite
                    || RequestType == S3RequestType.BucketExists
                    || RequestType == S3RequestType.BucketRead
                    || RequestType == S3RequestType.BucketReadAcl
                    || RequestType == S3RequestType.BucketReadLocation
                    || RequestType == S3RequestType.BucketReadLogging
                    || RequestType == S3RequestType.BucketReadTags
                    || RequestType == S3RequestType.BucketReadVersioning
                    || RequestType == S3RequestType.BucketReadVersions
                    || RequestType == S3RequestType.BucketReadWebsite
                    || RequestType == S3RequestType.BucketWrite
                    || RequestType == S3RequestType.BucketWriteAcl
                    || RequestType == S3RequestType.BucketWriteLogging
                    || RequestType == S3RequestType.BucketWriteTags
                    || RequestType == S3RequestType.BucketWriteVersioning
                    || RequestType == S3RequestType.BucketWriteWebsite)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Indicates if the request is an object request.
        /// </summary>
        public bool IsObjectRequest
        {
            get
            {
                if (RequestType == S3RequestType.ObjectDelete
                    || RequestType == S3RequestType.ObjectDeleteMultiple
                    || RequestType == S3RequestType.ObjectDeleteTags
                    || RequestType == S3RequestType.ObjectExists
                    || RequestType == S3RequestType.ObjectRead
                    || RequestType == S3RequestType.ObjectReadAcl
                    || RequestType == S3RequestType.ObjectReadLegalHold
                    || RequestType == S3RequestType.ObjectReadRange
                    || RequestType == S3RequestType.ObjectReadRetention
                    || RequestType == S3RequestType.ObjectReadTags
                    || RequestType == S3RequestType.ObjectRestore
                    || RequestType == S3RequestType.ObjectWrite
                    || RequestType == S3RequestType.ObjectWriteAcl
                    || RequestType == S3RequestType.ObjectWriteLegalHold
                    || RequestType == S3RequestType.ObjectWriteRetention
                    || RequestType == S3RequestType.ObjectWriteTags
                    || RequestType == S3RequestType.ObjectCopy)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Indicates if the request is a multipart upload request.
        /// </summary>
        public bool IsMultipartUploadRequest
        {
            get
            {
                if (RequestType == S3RequestType.BucketReadMultipartUploads
                    || RequestType == S3RequestType.ObjectAbortMultipartUpload
                    || RequestType == S3RequestType.ObjectCompleteMultipartUpload
                    || RequestType == S3RequestType.ObjectCreateMultipartUpload
                    || RequestType == S3RequestType.ObjectDeleteMultiple
                    || RequestType == S3RequestType.ObjectReadParts
                    || RequestType == S3RequestType.ObjectUploadPart
                    || RequestType == S3RequestType.ObjectUploadPartCopy)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Lists the permission typically required for this type of request.
        /// See https://docs.aws.amazon.com/AmazonS3/latest/dev/acl-overview.html for details.
        /// </summary>
        public S3PermissionType PermissionsRequired
        {
            get
            {
                switch (RequestType)
                {
                    case S3RequestType.BucketDelete:
                    case S3RequestType.BucketDeleteTags:
                    case S3RequestType.BucketDeleteWebsite:
                    case S3RequestType.BucketWrite:
                    case S3RequestType.BucketWriteLogging:
                    case S3RequestType.BucketWriteTags:
                    case S3RequestType.BucketWriteVersioning:
                    case S3RequestType.BucketWriteWebsite:
                        return S3PermissionType.BucketWrite;

                    case S3RequestType.BucketExists:
                    case S3RequestType.BucketRead:
                    case S3RequestType.BucketReadLocation:
                    case S3RequestType.BucketReadLogging:
                    case S3RequestType.BucketReadTags:
                    case S3RequestType.BucketReadVersioning:
                    case S3RequestType.BucketReadVersions:
                    case S3RequestType.BucketReadWebsite:
                        return S3PermissionType.BucketRead;

                    case S3RequestType.BucketReadAcl:
                        return S3PermissionType.BucketReadAcp;

                    case S3RequestType.BucketWriteAcl:
                        return S3PermissionType.BucketWriteAcp;

                    case S3RequestType.ObjectExists:
                    case S3RequestType.ObjectRead:
                    case S3RequestType.ObjectReadLegalHold:
                    case S3RequestType.ObjectReadRange:
                    case S3RequestType.ObjectReadRetention:
                    case S3RequestType.ObjectReadTags:
                        return S3PermissionType.ObjectRead;

                    case S3RequestType.ObjectDelete:
                    case S3RequestType.ObjectDeleteMultiple:
                    case S3RequestType.ObjectDeleteTags:
                    case S3RequestType.ObjectRestore:
                    case S3RequestType.ObjectWrite:
                    case S3RequestType.ObjectCopy:
                    case S3RequestType.ObjectWriteLegalHold:
                    case S3RequestType.ObjectWriteRetention:
                    case S3RequestType.ObjectWriteTags:
                        return S3PermissionType.BucketWrite;

                    case S3RequestType.ObjectReadAcl:
                        return S3PermissionType.ObjectReadAcp;

                    case S3RequestType.ObjectWriteAcl:
                        return S3PermissionType.ObjectWriteAcp;

                    case S3RequestType.ListBuckets:
                    case S3RequestType.Unknown:
                    default:
                        return S3PermissionType.NotApplicable;
                }
            }
        }

        /// <summary>
        /// List of signed headers.
        /// </summary>
        [JsonPropertyOrder(998)]
        public List<string> SignedHeaders
        {
            get
            {
                return _SignedHeaders;
            }
            set
            {
                if (value == null) _SignedHeaders = new List<string>();
                else _SignedHeaders = value;
            }
        }

        /// <summary>
        /// Stream containing the request body.
        /// </summary>
        [JsonIgnore]
        public Stream Data { get; private set; } = null;

        /// <summary>
        /// Data stream as a string.  Fully reads the data stream.
        /// </summary>
        [JsonIgnore]
        public string DataAsString
        {
            get
            {
                if (_HttpRequest != null) return _HttpRequest.DataAsString;
                return null;
            }
        }

        /// <summary>
        /// Data stream as a byte array.  Fully reads the data stream.
        /// </summary>
        [JsonIgnore]
        public byte[] DataAsBytes
        {
            get
            {
                if (_HttpRequest != null) return _HttpRequest.DataAsBytes;
                return null;
            }
        }

        #endregion

        #region Private-Members

        private static IdGenerator _IdGenerator = new IdGenerator();

        private string _Header = "[S3Request] ";
        private HttpRequestBase _HttpRequest = null;
        private Action<string> _Logger = null;
        private List<string> _SignedHeaders = new List<string>();

        private Dictionary<object, object> _UserMetadata = new Dictionary<object, object>();

        private int _MaxKeys = 1000;
        private int _MaxParts = 1000;
        private int _PartNumber = 1;
        private int _PartNumberMarker = 1;
        private long? _RangeStart = null;
        private long? _RangeEnd = null;
        private long? _RangeSuffixLength = null;
        private long? _CopySourceRangeStart = null;
        private long? _CopySourceRangeEnd = null;

        private Func<string, string> _FindMatchingBaseDomain = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public S3Request()
        {
        }

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        /// <param name="ctx">S3 context.</param>
        /// <param name="baseDomainFinder">Callback to invoke to find a base domain for a given hostname, used with virtual hosted style URLs.</param> 
        /// <param name="logger">Method to invoke to send log messages.</param> 
        public S3Request(S3Context ctx, Func<string, string> baseDomainFinder = null, Action<string> logger = null)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (ctx.Http == null) throw new ArgumentNullException(nameof(ctx.Http));

            _HttpRequest = ctx.Http.Request;
            _Logger = logger;
            _FindMatchingBaseDomain = baseDomainFinder;

            ParseHttpContext();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Determine if a header exists.
        /// </summary>
        /// <param name="key">Header key.</param>
        /// <returns>True if exists.</returns>
        public bool HeaderExists(string key)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            if (_HttpRequest != null)
            {
                return _HttpRequest.HeaderExists(key);
            }

            return false;
        }

        /// <summary>
        /// Determine if a querystring entry exists.
        /// </summary>
        /// <param name="key">Querystring key.</param>
        /// <returns>True if exists.</returns>
        public bool QuerystringExists(string key)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            if (_HttpRequest != null)
            {
                return _HttpRequest.QuerystringExists(key);
            }

            return false;
        }

        /// <summary>
        /// Retrieve a header (or querystring) value.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Value.</returns>
        public string RetrieveHeaderValue(string key)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            if (_HttpRequest != null)
            {
                return _HttpRequest.RetrieveHeaderValue(key);
            }

            return null;
        }

        /// <summary>
        /// Retrieve a querystring value, URL-decoded (for example, <c>a%2Fb%20c</c> is returned as <c>a/b c</c>).
        /// Use this method rather than reading <c>ctx.Http.Request.Query.Elements</c>, which holds the raw, percent-encoded values.
        /// </summary>
        /// <param name="key">Key.  Cannot be null or empty.</param>
        /// <returns>Decoded value, or null if the key is not present.</returns>
        /// <exception cref="ArgumentNullException">Thrown if key is null or empty.</exception>
        public string RetrieveQueryValue(string key)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            if (_HttpRequest != null)
            {
                return _HttpRequest.RetrieveQueryValue(key);
            }

            return null;
        }

        private string RetrieveFirstQueryValue(params string[] keys)
        {
            if (keys == null || keys.Length < 1) return null;

            foreach (string key in keys)
            {
                if (String.IsNullOrEmpty(key)) continue;

                if (QuerystringExists(key))
                    return RetrieveQueryValue(key);
            }

            return null;
        }

        private bool QuerystringExistsAny(params string[] keys)
        {
            if (keys == null || keys.Length < 1) return false;

            foreach (string key in keys)
            {
                if (String.IsNullOrEmpty(key)) continue;
                if (QuerystringExists(key)) return true;
            }

            return false;
        }

        private bool TryRetrieveQueryInt(out int value, params string[] keys)
        {
            value = 0;
            string stringValue = RetrieveFirstQueryValue(keys);

            if (String.IsNullOrEmpty(stringValue))
                return false;

            if (!Int32.TryParse(stringValue, NumberStyles.None, CultureInfo.InvariantCulture, out value))
            {
                string name = keys[0];
                if (_HttpRequest != null && _HttpRequest.Query != null && _HttpRequest.Query.Elements != null)
                {
                    string sent = _HttpRequest.Query.Elements.AllKeys.FirstOrDefault(k => k != null && keys.Any(a => a.Equals(k, StringComparison.OrdinalIgnoreCase)));
                    if (!String.IsNullOrEmpty(sent)) name = sent;
                }

                throw InvalidArgument("Querystring parameter '" + name + "' must be a non-negative integer; received '" + stringValue + "'.");
            }

            return true;
        }

        /// <summary>
        /// Read a chunk from the request body.
        /// </summary>
        /// <returns>Chunk.</returns>
        public async Task<Chunk> ReadChunk()
        {
            return await _HttpRequest.ReadChunk().ConfigureAwait(false);
        }

        #endregion

        #region Private-Methods

        private void ParseHttpContext()
        {
            if (_HttpRequest == null) throw new InvalidOperationException("HTTP context not supplied in the S3 request object.");

            #region Initialize

            Chunked = _HttpRequest.ChunkedTransfer;
            Region = null;
            Hostname = _HttpRequest.Destination.Hostname;
            RequestType = S3RequestType.Unknown;
            RequestStyle = S3RequestStyle.Unknown;
            Bucket = null;
            Key = null;
            Authorization = null;
            AccessKey = null;
            Data = _HttpRequest.Data;

            #endregion

            #region Set-Parameters-from-Querystring

            if (_HttpRequest.Query != null && _HttpRequest.Query.Elements != null && _HttpRequest.Query.Elements.Count > 0)
            {
                AccessKey = RetrieveFirstQueryValue("awsaccesskeyid", "AWSAccessKeyId");
                ContinuationToken = RetrieveQueryValue("continuation-token");
                StartAfter = RetrieveQueryValue("start-after");
                FetchOwner = String.Equals(RetrieveQueryValue("fetch-owner"), "true", StringComparison.OrdinalIgnoreCase);
                Delimiter = RetrieveQueryValue("delimiter");
                Expires = RetrieveFirstQueryValue("expires", "Expires");
                Marker = RetrieveQueryValue("marker");
                Prefix = RetrieveQueryValue("prefix");
                Signature = RetrieveFirstQueryValue("signature", "Signature");
                UploadId = RetrieveFirstQueryValue("uploadid", "uploadId", "UploadId");
                VersionId = RetrieveFirstQueryValue("versionid", "versionId", "VersionId");

                if (TryRetrieveQueryInt(out int maxKeys, "max-keys"))
                {
                    MaxKeys = maxKeys;
                }

                if (TryRetrieveQueryInt(out int maxParts, "max-parts", "maxParts", "MaxParts"))
                {
                    MaxParts = maxParts;
                }

                if (TryRetrieveQueryInt(out int partNum, "partnumber", "partNumber", "PartNumber"))
                {
                    PartNumber = partNum;
                }

                if (TryRetrieveQueryInt(out int partNumMarker, "part-number-marker", "partNumberMarker", "PartNumberMarker"))
                {
                    PartNumberMarker = partNumMarker;
                }

                if (!String.IsNullOrEmpty(AccessKey)
                    && !String.IsNullOrEmpty(Expires)
                    && !String.IsNullOrEmpty(Signature))
                {
                    SignatureVersion = S3SignatureVersion.Version2;
                }
            }

            #endregion

            #region Set-Values-From-Headers

            if (_HttpRequest.Headers != null && _HttpRequest.Headers.Count > 0)
            {
                if (HeaderExists("authorization"))
                {
                    _Logger?.Invoke(_Header + "processing Authorization header");
                    Authorization = RetrieveHeaderValue("authorization");
                    ParseAuthorizationHeader();
                }

                if (HeaderExists("range"))
                {
                    string rangeHeaderValue = RetrieveHeaderValue("range");

                    if (!String.IsNullOrEmpty(rangeHeaderValue))
                    {
                        long? start = null;
                        long? end = null;
                        long? suffixLength = null;

                        if (TryParseRangeHeader(rangeHeaderValue, out start, out end, out suffixLength))
                        {
                            RangeStart = start;
                            RangeEnd = end;
                            RangeSuffixLength = suffixLength;
                        }
                        else
                        {
                            // RFC 9110 section 14.2: a server may ignore a Range header it cannot or will not satisfy.
                            // Amazon S3 serves the full object in this case, so the request routes as ObjectRead.
                            _Logger?.Invoke(_Header + "ignoring unsupported or malformed Range header: " + rangeHeaderValue);
                        }
                    }
                }

                if (_HttpRequest.Method == HttpMethod.PUT && HeaderExists("x-amz-copy-source"))
                {
                    ParseCopySourceHeader(RetrieveHeaderValue("x-amz-copy-source"));

                    if (HeaderExists("x-amz-copy-source-range"))
                        ParseCopySourceRangeHeader(RetrieveHeaderValue("x-amz-copy-source-range"));
                }

                IfMatch = ConditionalHeaderParser.ParseEntityTagList(RetrieveHeaderValueIfExists("if-match"));
                IfNoneMatch = ConditionalHeaderParser.ParseEntityTagList(RetrieveHeaderValueIfExists("if-none-match"));
                IfModifiedSince = ConditionalHeaderParser.ParseHttpDate(RetrieveHeaderValueIfExists("if-modified-since"));
                IfUnmodifiedSince = ConditionalHeaderParser.ParseHttpDate(RetrieveHeaderValueIfExists("if-unmodified-since"));
                CopySourceIfMatch = ConditionalHeaderParser.ParseEntityTagList(RetrieveHeaderValueIfExists("x-amz-copy-source-if-match"));
                CopySourceIfNoneMatch = ConditionalHeaderParser.ParseEntityTagList(RetrieveHeaderValueIfExists("x-amz-copy-source-if-none-match"));
                CopySourceIfModifiedSince = ConditionalHeaderParser.ParseHttpDate(RetrieveHeaderValueIfExists("x-amz-copy-source-if-modified-since"));
                CopySourceIfUnmodifiedSince = ConditionalHeaderParser.ParseHttpDate(RetrieveHeaderValueIfExists("x-amz-copy-source-if-unmodified-since"));

                if (HeaderExists("content-md5")) ContentMd5 = RetrieveHeaderValue("content-md5");
                if (HeaderExists("content-type")) ContentType = RetrieveHeaderValue("content-type");
                if (HeaderExists("host")) Host = RetrieveHeaderValue("host");

                if (HeaderExists("x-amz-content-sha256"))
                {
                    ContentSha256 = RetrieveHeaderValue("x-amz-content-sha256");
                    if (!String.IsNullOrEmpty(ContentSha256))
                    {
                        if (ContentSha256.IndexOf("streaming", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Chunked = true;
                            _HttpRequest.ChunkedTransfer = true;
                        }
                    }
                }

                if (HeaderExists("date"))
                    Date = RetrieveHeaderValue("date");

                if (HeaderExists("x-amz-date"))
                    Date = RetrieveHeaderValue("x-amz-date");

                if (HeaderExists("x-amz-request-id"))
                    RequestId = RetrieveHeaderValue("x-amz-request-id");

                if (HeaderExists("x-amz-id-2"))
                    TraceId = RetrieveHeaderValue("x-amz-id-2");
            }

            #endregion

            #region Set-Region-Bucket-Style-and-Key

            if (!String.IsNullOrEmpty(Hostname)
                && !String.IsNullOrEmpty(_HttpRequest.Url.RawWithoutQuery))
            {
                ParseHostnameAndUrl();
            }

            #endregion

            #region Set-RequestType

            SetRequestType();

            #endregion 
        }

        private void ParseAuthorizationHeader()
        {
            string logMessage = "";
            if (String.IsNullOrEmpty(Authorization)) return;
            string exceptionMsg = "Invalid authorization header format: " + Authorization;

            try
            {
                #region Retrieve-Outer-Values

                // [encryption] [values]
                string[] valsOuter = Authorization.Split(new[] { ' ' }, 2);
                if (valsOuter == null || valsOuter.Length < 2) throw new ArgumentException(exceptionMsg);

                logMessage += _Header + "Authorization header : " + Authorization + Environment.NewLine;
                logMessage += _Header + "Outer header values  :" + Environment.NewLine;
                for (int i = 0; i < valsOuter.Length; i++)
                {
                    logMessage += "  " + i + ": " + valsOuter[i].Trim() + Environment.NewLine;
                }

                #endregion

                if (valsOuter[0].Equals("AWS"))
                {
                    #region Signature-V2

                    // see https://docs.aws.amazon.com/AmazonS3/latest/dev/RESTAuthentication.html#ConstructingTheAuthenticationHeader
                    // Authorization: AWS AWSAccessKeyId:Signature

                    string[] valsInner = valsOuter[1].Split(':');

                    logMessage += _Header + "Inner header values" + Environment.NewLine;
                    for (int i = 0; i < valsInner.Length; i++)
                    {
                        logMessage += "  " + i + ": " + valsInner[i].Trim() + Environment.NewLine;
                    }

                    if (valsInner.Length != 2) throw new ArgumentException(exceptionMsg);
                    SignatureVersion = S3SignatureVersion.Version2;
                    AccessKey = valsInner[0].Trim();
                    Signature = valsInner[1].Trim();

                    logMessage +=
                        _Header + "Signature version    : " + SignatureVersion.ToString() + Environment.NewLine +
                        _Header + "Access key           : " + AccessKey + Environment.NewLine +
                        _Header + "Signature            : " + Signature;

                    return;

                    #endregion
                }
                else if (valsOuter[0].Equals("AWS4-HMAC-SHA256"))
                {
                    #region Signature-V4

                    // see https://docs.aws.amazon.com/AmazonS3/latest/API/sigv4-auth-using-authorization-header.html
                    // 
                    // AWS4-HMAC-SHA256 Credential=access/20190418/us-east-1/s3/aws4_request, SignedHeaders=content-length;content-type;host;user-agent;x-amz-content-sha256;x-amz-date;x-amz-decoded-content-length, Signature=66946e06895806f4e32d32217c1a02313b9d9235b759f3a690742c8f9971daa0
                    //
                    // valsOuter[0] AWS4-HMAC-SHA256
                    // valsOuter[1] everything else...

                    SignatureVersion = S3SignatureVersion.Version4;

                    string[] keyValuePairs = valsOuter[1].Split(',');
                    List<string> keyValuePairsTrimmed = new List<string>();

                    logMessage += _Header + "Inner header values" + Environment.NewLine;

                    for (int i = 0; i < keyValuePairs.Length; i++)
                    {
                        string currKey = keyValuePairs[i];
                        if (String.IsNullOrEmpty(currKey)) continue;

                        currKey = currKey.Trim();
                        keyValuePairsTrimmed.Add(currKey);

                        logMessage += i + ": " + keyValuePairs[i].Trim() + Environment.NewLine;
                    }

                    foreach (string currKey in keyValuePairsTrimmed)
                    {
                        if (currKey.StartsWith("Credential="))
                        {
                            #region Credentials

                            string credentialString = currKey.Replace("Credential=", "").Trim();
                            string[] credentialVals = credentialString.Split('/');
                            if (credentialVals.Length < 5) throw new ArgumentException(exceptionMsg);
                            AccessKey = credentialVals[0].Trim();
                            Region = credentialVals[2].Trim();

                            #endregion
                        }
                        else if (currKey.StartsWith("SignedHeaders="))
                        {
                            #region Signed-Headers

                            string signedHeadersString = currKey.Replace("SignedHeaders=", "").Trim();
                            string[] signedHeaderVals = signedHeadersString.Split(';');
                            if (signedHeaderVals != null && signedHeaderVals.Length > 0)
                            {
                                foreach (string currSignedHeader in signedHeaderVals)
                                {
                                    SignedHeaders.Add(currSignedHeader.Trim());
                                }

                                SignedHeaders.Sort();
                            }

                            #endregion
                        }
                        else if (currKey.StartsWith("Signature="))
                        {
                            #region Signature

                            Signature = currKey.Replace("Signature=", "").Trim();

                            #endregion
                        }
                        else if (currKey.StartsWith("Expires="))
                        {
                            #region Expires

                            Expires = currKey.Replace("Expires=", "").Trim();

                            #endregion
                        }
                    }

                    logMessage +=
                        _Header + "Signature version    : " + SignatureVersion.ToString() + Environment.NewLine +
                        _Header + "Access key           : " + AccessKey + Environment.NewLine +
                        _Header + "Region               : " + Region + Environment.NewLine +
                        _Header + "Signature            : " + Signature;

                    return;

                    #endregion
                }
                else
                {
                    throw new ArgumentException(exceptionMsg + Authorization);
                }
            }
            finally
            {
                _Logger?.Invoke(logMessage);
            }
        }

        private void ParseHostnameAndUrl()
        {
            string fullUrl = _HttpRequest.Url.Full;

            // When the server is bound to a wildcard hostname (*, +, 0.0.0.0),
            // the URL will contain the wildcard which is not a valid URI hostname.
            // Replace it with the Host header value from the actual HTTP request.
            if (!String.IsNullOrEmpty(fullUrl) && !String.IsNullOrEmpty(Host))
            {
                Uri tempUri;
                if (!Uri.TryCreate(fullUrl, UriKind.Absolute, out tempUri))
                {
                    string hostValue = Host.Contains(":") ? Host.Split(':')[0] : Host;
                    fullUrl = ReplaceWildcardHostname(fullUrl, hostValue);
                }
            }

            Uri uri = new Uri(fullUrl);
            Hostname = uri.Host;

            _Logger?.Invoke(_Header + "parsing URL " + _HttpRequest.Url.Full);

            if (IsIpAddress(Hostname))
            {
                RequestStyle = S3RequestStyle.PathStyle;
                _Logger?.Invoke(_Header + "supplied hostname is an IP address");
            }
            else
            {
                if (_FindMatchingBaseDomain == null)
                {
                    // assume path style
                    RequestStyle = S3RequestStyle.PathStyle;
                    _Logger?.Invoke(_Header + "no base domain finder specified, request assumed to have bucket in URL");
                }
                else
                {
                    // assume virtual hosted style
                    BaseDomain = _FindMatchingBaseDomain(Hostname);
                    if (String.IsNullOrEmpty(BaseDomain))
                    {
                        RequestStyle = S3RequestStyle.PathStyle;
                        _Logger?.Invoke(_Header + "base hostname not found, assumed to have bucket in URL");
                    }
                    else
                    {
                        RequestStyle = S3RequestStyle.VirtualHostedStyle;

                        string tempBaseDomain = BaseDomain.TrimStart('.');

                        string temp = ReplaceLastOccurrence(Hostname, tempBaseDomain, "").TrimEnd('.');

                        Bucket = temp;

                        _Logger?.Invoke(_Header + "bucket " + Bucket + " found in hostname " + Hostname);
                    }
                }
            }

            string rawUrl = _HttpRequest.Url.RawWithoutQuery;

            rawUrl = rawUrl.TrimStart('/');

            switch (RequestStyle)
            {
                case S3RequestStyle.VirtualHostedStyle:
                    Key = WebUtility.UrlDecode(rawUrl);
                    break;

                case S3RequestStyle.PathStyle:
                    string[] valsInner = rawUrl.Split(new[] { '/' }, 2);
                    if (valsInner.Length > 0) Bucket = WebUtility.UrlDecode(valsInner[0]);
                    if (valsInner.Length > 1) Key = WebUtility.UrlDecode(valsInner[1]);
                    break;
            }

            _Logger?.Invoke(_Header +
                "parsed URL:" + Environment.NewLine +
                "  Full URL      : " + _HttpRequest.Url.Full + Environment.NewLine +
                "  Raw URL       : " + _HttpRequest.Url.RawWithoutQuery + Environment.NewLine +
                "  Hostname      : " + Hostname + Environment.NewLine +
                "  Base domain   : " + BaseDomain + Environment.NewLine +
                "  Bucket name   : " + Bucket + Environment.NewLine +
                "  Style         : " + RequestStyle.ToString() + Environment.NewLine +
                "  Object key    : " + Key + Environment.NewLine +
                "  Region        : " + Region);

            return;
        }

        /// <summary>
        /// Reclassify a suffix range request (bytes=-N) as ObjectRead.
        /// Used when S3ServerSettings.RouteSuffixRangesToReadRange is false.
        /// </summary>
        internal void RouteSuffixRangeAsObjectRead()
        {
            if (RequestType == S3RequestType.ObjectReadRange
                && _RangeStart == null
                && _RangeSuffixLength != null)
            {
                RequestType = S3RequestType.ObjectRead;
            }
        }

        private string RetrieveHeaderValueIfExists(string key)
        {
            if (!HeaderExists(key)) return null;
            return RetrieveHeaderValue(key);
        }

        internal static bool TryParseRangeHeader(string header, out long? start, out long? end, out long? suffixLength)
        {
            start = null;
            end = null;
            suffixLength = null;

            if (String.IsNullOrWhiteSpace(header)) return false;

            string value = header.Trim();
            if (!value.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase)) return false;

            value = value.Substring(6).Trim();
            if (value.IndexOf(',') >= 0) return false;

            int dash = value.IndexOf('-');
            if (dash < 0 || dash != value.LastIndexOf('-')) return false;

            string first = value.Substring(0, dash).Trim();
            string last = value.Substring(dash + 1).Trim();

            if (first.Length == 0 && last.Length == 0) return false;

            if (first.Length == 0)
            {
                if (!Int64.TryParse(last, NumberStyles.None, CultureInfo.InvariantCulture, out long suffix)) return false;
                suffixLength = suffix;
                return true;
            }

            if (!Int64.TryParse(first, NumberStyles.None, CultureInfo.InvariantCulture, out long firstPos)) return false;

            if (last.Length == 0)
            {
                start = firstPos;
                return true;
            }

            if (!Int64.TryParse(last, NumberStyles.None, CultureInfo.InvariantCulture, out long lastPos)) return false;
            if (lastPos < firstPos) return false;

            start = firstPos;
            end = lastPos;
            return true;
        }

        private void ParseCopySourceHeader(string header)
        {
            if (String.IsNullOrWhiteSpace(header))
                throw InvalidArgument("The x-amz-copy-source header is empty.");

            string value = header.Trim();
            string query = null;

            // Split the query on the raw value, since a literal '?' in a key is sent percent-encoded.
            int queryIndex = value.IndexOf('?');
            if (queryIndex >= 0)
            {
                query = value.Substring(queryIndex + 1);
                value = value.Substring(0, queryIndex);
            }

            // Decode before splitting: some clients (including the AWS SDK for .NET) percent-encode the slash
            // between the bucket and the key.  Bucket names cannot contain '/', so the first '/' is the separator.
            value = Uri.UnescapeDataString(value).TrimStart('/');

            int slash = value.IndexOf('/');
            if (slash <= 0 || slash == value.Length - 1)
                throw InvalidArgument("The x-amz-copy-source header must be in the form 'bucket/key': '" + header + "'.");

            string bucket = value.Substring(0, slash);
            string key = value.Substring(slash + 1);

            CopySourceBucket = bucket;
            CopySourceKey = key;

            if (!String.IsNullOrEmpty(query))
            {
                foreach (string pair in query.Split('&'))
                {
                    if (String.IsNullOrEmpty(pair)) continue;

                    int equals = pair.IndexOf('=');
                    string name = equals >= 0 ? pair.Substring(0, equals) : pair;
                    string val = equals >= 0 ? pair.Substring(equals + 1) : null;

                    if (name.Equals("versionId", StringComparison.OrdinalIgnoreCase))
                    {
                        if (String.IsNullOrEmpty(val))
                            throw InvalidArgument("The x-amz-copy-source header has an empty versionId.");

                        CopySourceVersionId = Uri.UnescapeDataString(val);
                    }
                }
            }
        }

        private void ParseCopySourceRangeHeader(string header)
        {
            long? start;
            long? end;
            long? suffixLength;

            if (!TryParseRangeHeader(header, out start, out end, out suffixLength)
                || start == null
                || end == null)
            {
                throw InvalidArgument("The x-amz-copy-source-range header must be in the form 'bytes=first-last': '" + header + "'.");
            }

            CopySourceRangeStart = start;
            CopySourceRangeEnd = end;
        }

        private static S3Exception InvalidArgument(string message)
        {
            Error error = new Error(ErrorCode.InvalidArgument);
            error.Message = message;
            return new S3Exception(error);
        }

        private void SetRequestType()
        {
            switch (_HttpRequest.Method)
            {
                case HttpMethod.HEAD:
                    #region HEAD

                    if (String.IsNullOrEmpty(Bucket) && String.IsNullOrEmpty(Key))
                        RequestType = S3RequestType.ServiceExists;
                    if (!String.IsNullOrEmpty(Bucket) && String.IsNullOrEmpty(Key))
                        RequestType = S3RequestType.BucketExists;
                    else if (!String.IsNullOrEmpty(Bucket) && !String.IsNullOrEmpty(Key))
                        RequestType = S3RequestType.ObjectExists;
                    break;

                #endregion

                case HttpMethod.GET:
                    #region GET

                    if (String.IsNullOrEmpty(Bucket) && String.IsNullOrEmpty(Key))
                    {
                        RequestType = RequestType = S3RequestType.ListBuckets;
                    }
                    else if (!String.IsNullOrEmpty(Bucket) && String.IsNullOrEmpty(Key))
                    {
                        if (QuerystringExists("acl"))
                            RequestType = S3RequestType.BucketReadAcl;
                        else if (QuerystringExists("location"))
                            RequestType = S3RequestType.BucketReadLocation;
                        else if (QuerystringExists("logging"))
                            RequestType = S3RequestType.BucketReadLogging;
                        else if (QuerystringExists("tagging"))
                            RequestType = S3RequestType.BucketReadTags;
                        else if (QuerystringExists("uploads"))
                            RequestType = S3RequestType.BucketReadMultipartUploads;
                        else if (QuerystringExists("versions"))
                            RequestType = S3RequestType.BucketReadVersions;
                        else if (QuerystringExists("versioning"))
                            RequestType = S3RequestType.BucketReadVersioning;
                        else if (QuerystringExists("website"))
                            RequestType = S3RequestType.BucketReadWebsite;
                        else
                            RequestType = S3RequestType.BucketRead;
                    }
                    else if (!String.IsNullOrEmpty(Bucket) && !String.IsNullOrEmpty(Key))
                    {
                        if (HeaderExists("range") && (_RangeStart != null || _RangeSuffixLength != null))
                            RequestType = S3RequestType.ObjectReadRange;
                        else if (QuerystringExists("acl"))
                            RequestType = S3RequestType.ObjectReadAcl;
                        else if (QuerystringExists("legal-hold"))
                            RequestType = S3RequestType.ObjectReadLegalHold;
                        else if (QuerystringExistsAny("uploadid", "uploadId", "UploadId"))
                            RequestType = S3RequestType.ObjectReadParts;
                        else if (QuerystringExists("retention"))
                            RequestType = S3RequestType.ObjectReadRetention;
                        else if (QuerystringExists("tagging"))
                            RequestType = S3RequestType.ObjectReadTags;
                        else
                            RequestType = S3RequestType.ObjectRead;
                    }
                    break;

                #endregion

                case HttpMethod.PUT:
                    #region PUT

                    if (!String.IsNullOrEmpty(Bucket) && String.IsNullOrEmpty(Key))
                    {
                        if (QuerystringExists("acl"))
                            RequestType = S3RequestType.BucketWriteAcl;
                        else if (QuerystringExists("logging"))
                            RequestType = S3RequestType.BucketWriteLogging;
                        else if (QuerystringExists("tagging"))
                            RequestType = S3RequestType.BucketWriteTags;
                        else if (QuerystringExists("versioning"))
                            RequestType = S3RequestType.BucketWriteVersioning;
                        else if (QuerystringExists("website"))
                            RequestType = S3RequestType.BucketWriteWebsite;
                        else
                            RequestType = S3RequestType.BucketWrite;
                    }
                    else if (!String.IsNullOrEmpty(Bucket) && !String.IsNullOrEmpty(Key))
                    {
                        if (QuerystringExists("tagging"))
                            RequestType = S3RequestType.ObjectWriteTags;
                        else if (QuerystringExists("acl"))
                            RequestType = S3RequestType.ObjectWriteAcl;
                        else if (QuerystringExists("legal-hold"))
                            RequestType = S3RequestType.ObjectWriteLegalHold;
                        else if (QuerystringExists("retention"))
                            RequestType = S3RequestType.ObjectWriteRetention;
                        else if (QuerystringExistsAny("partnumber", "partNumber", "PartNumber") && QuerystringExistsAny("uploadid", "uploadId", "UploadId"))
                            RequestType = (CopySourceKey != null) ? S3RequestType.ObjectUploadPartCopy : S3RequestType.ObjectUploadPart;
                        else if (CopySourceKey != null)
                            RequestType = S3RequestType.ObjectCopy;
                        else
                            RequestType = S3RequestType.ObjectWrite;
                    }
                    break;

                #endregion

                case HttpMethod.POST:
                    #region POST

                    if (!String.IsNullOrEmpty(Bucket))
                    {
                        if (QuerystringExists("delete"))
                            RequestType = S3RequestType.ObjectDeleteMultiple;

                        if (!String.IsNullOrEmpty(Key))
                        {
                            if (QuerystringExists("restore"))
                                RequestType = S3RequestType.ObjectRestore;
                            if (QuerystringExists("select")
                                && QuerystringExists("select-type")
                                && RetrieveQueryValue("select-type").Equals("2"))
                            {
                                RequestType = S3RequestType.ObjectSelectContent;
                            }
                            if (QuerystringExistsAny("uploadid", "uploadId", "UploadId"))
                                RequestType = S3RequestType.ObjectCompleteMultipartUpload;
                            if (QuerystringExists("uploads"))
                                RequestType = S3RequestType.ObjectCreateMultipartUpload;
                        }
                    }
                    break;

                #endregion

                case HttpMethod.DELETE:
                    #region DELETE

                    if (!String.IsNullOrEmpty(Bucket) && String.IsNullOrEmpty(Key))
                    {
                        if (QuerystringExists("acl"))
                            RequestType = S3RequestType.BucketDeleteAcl;
                        else if (QuerystringExists("tagging"))
                            RequestType = S3RequestType.BucketDeleteTags;
                        else if (QuerystringExists("website"))
                            RequestType = S3RequestType.BucketDeleteWebsite;
                        else
                            RequestType = S3RequestType.BucketDelete;
                    }
                    else if (!String.IsNullOrEmpty(Bucket) && !String.IsNullOrEmpty(Key))
                    {
                        if (QuerystringExists("acl"))
                            RequestType = S3RequestType.ObjectDeleteAcl;
                        else if (QuerystringExists("tagging"))
                            RequestType = S3RequestType.ObjectDeleteTags;
                        else if (QuerystringExistsAny("uploadid", "uploadId", "UploadId"))
                            RequestType = S3RequestType.ObjectAbortMultipartUpload;
                        else
                            RequestType = S3RequestType.ObjectDelete;
                    }
                    break;

                    #endregion
            }
        }

        private static bool IsIpAddress(string val)
        {
            if (String.IsNullOrEmpty(val)) throw new ArgumentNullException(nameof(val));
            return IPAddress.TryParse(val, out _);
        }

        internal static string ReplaceWildcardHostname(string url, string replacement)
        {
            // Match protocol prefix then wildcard hostname
            foreach (string wildcard in new[] { "*", "+", "0.0.0.0" })
            {
                string pattern = "://" + wildcard;
                int idx = url.IndexOf(pattern);
                if (idx >= 0)
                {
                    return url.Substring(0, idx + 3) + replacement + url.Substring(idx + 3 + wildcard.Length);
                }
            }
            return url;
        }

        private static string ReplaceLastOccurrence(string src, string find, string replace)
        {
            int place = src.LastIndexOf(find);

            if (place == -1)
                return src;

            string result = src.Remove(place, find.Length).Insert(place, replace);
            return result;
        }

        #endregion
    }
}
