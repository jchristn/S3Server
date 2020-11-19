using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WatsonWebserver; 
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace S3ServerInterface
{
    /// <summary>
    /// S3 request object.
    /// </summary>
    public class S3Request
    {
        #region Public-Members
         
        /// <summary>
        /// Indicates if the request includes the bucket name in the hostname or not.
        /// </summary>
        public S3RequestStyle RequestStyle { get; private set; } = S3RequestStyle.Unknown;

        /// <summary>
        /// Indicates the type of S3 request.
        /// </summary>
        public S3RequestType RequestType { get; private set; } = S3RequestType.Unknown;

        /// <summary>
        /// Time of creation in UTC.
        /// </summary>
        public DateTime TimestampUtc { get; private set; } = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// IP address of the client.
        /// </summary>
        public string SourceIp { get; private set; } = null;

        /// <summary>
        /// TCP port of the client.
        /// </summary>
        public int SourcePort { get; private set; } = 0;

        /// <summary>
        /// HTTP method (GET, PUT, POST, DELETE, etc).
        /// </summary>
        public HttpMethod Method { get; private set; } = HttpMethod.GET;

        /// <summary>
        /// Full URL.
        /// </summary>
        public string FullUrl { get; private set; } = null;

        /// <summary>
        /// The raw URL without querystring.
        /// </summary>
        public string RawUrl { get; private set; } = null;

        /// <summary>
        /// The length of the payload.
        /// </summary>
        public long ContentLength { get; private set; } = 0;

        /// <summary>
        /// The content type of the payload.
        /// </summary>
        public string ContentType { get; private set; } = null;

        /// <summary>
        /// Indicates if chunked transfer-encoding is in use.
        /// </summary>
        public bool Chunked { get; private set; } = false;

        /// <summary>
        /// AWS region.
        /// </summary>
        public string Region { get; private set; } = null;

        /// <summary>
        /// Hostname.
        /// </summary>
        public string Hostname { get; private set; } = null;

        /// <summary>
        /// Host header value.
        /// </summary>
        public string Host { get; private set; } = null;

        /// <summary>
        /// Base domain against which the hostname is evaluated to identify the bucket name.
        /// </summary>
        public string BaseDomain { get; private set; } = null;

        /// <summary>
        /// Bucket.
        /// </summary>
        public string Bucket { get; private set; } = null;

        /// <summary>
        /// Object key.
        /// </summary>
        public string Key { get; private set; } = null;

        /// <summary>
        /// Object key prefix.
        /// </summary>
        public string Prefix { get; private set; } = null;

        /// <summary>
        /// Delimiter.
        /// </summary>
        public string Delimiter { get; private set; } = null;

        /// <summary>
        /// Marker.
        /// </summary>
        public string Marker { get; private set; } = null;

        /// <summary>
        /// Maximum number of keys to retrieve in an enumeration.
        /// </summary>
        public long MaxKeys { get; private set; } = 0;

        /// <summary>
        /// Object version ID.
        /// </summary>
        public string VersionId { get; private set; } = null;

        /// <summary>
        /// Authorization header string, in full.
        /// </summary>
        public string Authorization { get; private set; } = null;

        /// <summary>
        /// Signature version from authorization header.
        /// </summary>
        public S3SignatureVersion SignatureVersion { get; private set; } = S3SignatureVersion.Unknown;

        /// <summary>
        /// Signature from authorization header.
        /// </summary>
        public string Signature { get; private set; } = null;

        /// <summary>
        /// Content MD-5 from request headers.
        /// </summary>
        public string ContentMd5 { get; private set; } = null;

        /// <summary>
        /// Content SHA-256 from request headers.
        /// </summary>
        public string ContentSha256 { get; private set; } = null;

        /// <summary>
        /// Date parameter.
        /// </summary>
        public string Date { get; private set; } = null;

        /// <summary>
        /// Expiration parameter from authorization header.
        /// </summary>
        public string Expires { get; private set; } = null;

        /// <summary>
        /// Access key, parsed from authorization header.
        /// </summary>
        public string AccessKey { get; private set; } = null;

        /// <summary>
        /// Start value from the Range header.
        /// </summary>
        public long? RangeStart { get; private set; } = null;

        /// <summary>
        /// End value from the Range header.
        /// </summary>
        public long? RangeEnd { get; private set; } = null;

        /// <summary>
        /// Continuation token.
        /// </summary>
        public string ContinuationToken { get; private set; } = null;

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
                    || RequestType == S3RequestType.ObjectWrite
                    || RequestType == S3RequestType.ObjectWriteAcl
                    || RequestType == S3RequestType.ObjectWriteLegalHold
                    || RequestType == S3RequestType.ObjectWriteRetention
                    || RequestType == S3RequestType.ObjectWriteTags)
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
                    case S3RequestType.ObjectWrite:
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
        [JsonProperty(Order = 990)]
        public List<string> SignedHeaders { get; private set; } = new List<string>();

        /// <summary>
        /// URL querystring.
        /// </summary>
        [JsonProperty(Order = 991)]
        public Dictionary<string, string> Querystring { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// Full set of HTTP headers.
        /// </summary>
        [JsonProperty(Order = 992)]
        public Dictionary<string, string> Headers { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// The individual elements in the raw URL.
        /// </summary>
        [JsonProperty(Order = 993)]
        public string[] RawUrlEntries { get; private set; } = null;

        /// <summary>
        /// User-defined metadata.
        /// </summary>
        [JsonProperty(Order = 998)]
        public Dictionary<object, object> UserMetadata
        {
            get
            {
                return _UserMetadata;
            }
            set
            {
                if (value == null)
                {
                    _UserMetadata = new Dictionary<object, object>();
                }
                else
                {
                    _UserMetadata = value;
                }
            }
        }

        /// <summary>
        /// HTTP context from which this response was created.
        /// </summary>
        [JsonProperty(Order = 999)]
        public HttpContext Http { get; private set; } = null;

        /// <summary>
        /// Stream containing the request body.
        /// </summary>
        [JsonIgnore]
        public Stream Data { get; private set; } = null;

        #endregion

        #region Private-Members

        private string _Header = "[S3Request] ";
        private Action<string> _Logger = null;

        private string _AmazonTimestampFormat = "yyyyMMddTHHmmssZ"; 
        private Dictionary<object, object> _UserMetadata = new Dictionary<object, object>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public S3Request()
        {
            TimestampUtc = DateTime.Now.ToUniversalTime();
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="baseDomain">Base domain against which the hostname should be evaluated to identify the bucket.  For instance, to resolve buckets for *.localhost, specify '.localhost'.</param>
        /// <param name="ctx">HttpContext.</param> 
        public S3Request(string baseDomain, HttpContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (ctx.Request == null) throw new ArgumentNullException(nameof(ctx.Request));
             
            ParseHttpContext(baseDomain, ctx);
        }

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        /// <param name="baseDomain">Base domain against which the hostname should be evaluated to identify the bucket.  For instance, to resolve buckets for *.localhost, specify '.localhost'.</param>
        /// <param name="ctx">HttpContext.</param> 
        /// <param name="logger">Method to invoke to send log messages.</param> 
        public S3Request(string baseDomain, HttpContext ctx, Action<string> logger)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (ctx.Request == null) throw new ArgumentNullException(nameof(ctx.Request));

            _Logger = logger;

            ParseHttpContext(baseDomain, ctx);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a JSON representation of the object.
        /// </summary>
        /// <param name="pretty">Pretty print.</param>
        /// <returns>JSON string.</returns>
        public string ToJson(bool pretty)
        {
            return Common.SerializeJson(this, pretty);
        }

        /// <summary>
        /// Populate members using an HttpContext.
        /// </summary>
        /// <param name="baseDomain">Base domain against which the hostname should be evaluated to identify the bucket.</param>
        /// <param name="ctx">HttpContext.</param>
        public void ParseHttpContext(string baseDomain, HttpContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            BaseDomain = baseDomain;
            Http = ctx;

            #region Initialize

            TimestampUtc = DateTime.Now.ToUniversalTime();
            Http = ctx;
            SourceIp = Http.Request.Source.IpAddress;
            SourcePort = Http.Request.Source.Port;
            Method = Http.Request.Method;
            FullUrl = Http.Request.Url.Full;
            RawUrl = Http.Request.Url.RawWithoutQuery;
            while (RawUrl.Contains("\\\\")) RawUrl.Replace("\\\\", "\\");

            RawUrlEntries = Http.Request.Url.Elements;
            ContentLength = Http.Request.ContentLength;
            ContentType = Http.Request.ContentType;
            Chunked = Http.Request.ChunkedTransfer;
            Querystring = Http.Request.Query.Elements;
            Headers = Http.Request.Headers;
            Region = null;
            BaseDomain = baseDomain;
            Hostname = Http.Request.Destination.Hostname;
            RequestType = S3RequestType.Unknown;
            RequestStyle = S3RequestStyle.Unknown;
            Bucket = null;
            Key = null;
            Authorization = null;
            AccessKey = null;
            Data = Http.Request.Data;

            #endregion

            #region Set-Parameters-from-Querystring

            if (Querystring != null && Querystring.Count > 0)
            {
                // sort ascending
                Querystring = Querystring.OrderBy(o => o.Key, StringComparer.OrdinalIgnoreCase).ToDictionary(o => o.Key, o => o.Value);

                VersionId = RetrieveQueryValue("versionid");
                Prefix = RetrieveQueryValue("prefix");
                Delimiter = RetrieveQueryValue("delimiter");
                Marker = RetrieveQueryValue("marker");
                ContinuationToken = RetrieveQueryValue("continuation-token");
                Expires = RetrieveQueryValue("expires");

                if (QuerystringExists("max-keys", false))
                {
                    long maxKeys = 0;
                    string maxKeysStr = RetrieveQueryValue("max-keys");
                    if (!String.IsNullOrEmpty(maxKeysStr))
                    {
                        if (Int64.TryParse(Querystring["max-keys"], out maxKeys))
                        {
                            MaxKeys = maxKeys;
                        }
                    }
                }

                if (QuerystringExists("awsaccesskeyid", false))
                    AccessKey = RetrieveQueryValue("awsaccesskeyid");

                if (QuerystringExists("signature", false))
                    Signature = RetrieveQueryValue("signature");
            }

            #endregion

            #region Set-Values-From-Headers

            if (Headers != null && Headers.Count > 0)
            {
                // sort ascending
                Headers = Headers.OrderBy(o => o.Key, StringComparer.OrdinalIgnoreCase).ToDictionary(o => o.Key, o => o.Value);

                if (HeaderExists("authorization", false))
                {
                    _Logger?.Invoke(_Header + "Processing Authorization header"); 
                    Authorization = RetrieveHeaderValue("authorization");
                    ParseAuthorizationHeader();
                }

                if (HeaderExists("range", false))
                {
                    string rangeHeaderValue = RetrieveHeaderValue("range");

                    if (!String.IsNullOrEmpty(rangeHeaderValue))
                    {
                        long start = 0;
                        long end = 0;
                        ParseRangeHeader(rangeHeaderValue, out start, out end);

                        RangeStart = start;
                        RangeEnd = end;
                    }
                }

                if (HeaderExists("content-md5", false))
                {
                    Host = RetrieveHeaderValue("content-md5");
                }

                if (HeaderExists("host", false))
                {
                    Host = RetrieveHeaderValue("host");
                }

                if (HeaderExists("x-amz-content-sha256", false))
                {
                    ContentSha256 = RetrieveHeaderValue("x-amz-content-sha256");
                }

                if (HeaderExists("date", false))
                {
                    Date = RetrieveHeaderValue("date");

                    if (DateTime.TryParseExact(Date, _AmazonTimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime dt))
                    {
                        TimestampUtc = dt;
                    }
                }

                if (HeaderExists("x-amz-date", false))
                {
                    Date = RetrieveHeaderValue("x-amz-date");

                    if (DateTime.TryParseExact(Date, _AmazonTimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime dt))
                    {
                        TimestampUtc = dt;
                    }
                }

                if (HeaderExists("x-amz-content-sha256", false))
                {
                    string sha256content = RetrieveHeaderValue("x-amz-content-sha256");
                    if (!String.IsNullOrEmpty(sha256content))
                    {
                        if (sha256content.Contains("STREAMING")) Chunked = true;
                    }
                }
            }

            #endregion

            #region Set-Region-Bucket-Style-and-Key

            if (!String.IsNullOrEmpty(Hostname) && !String.IsNullOrEmpty(RawUrl))
            { 
                ParseHostnameAndUrl(baseDomain, FullUrl, RawUrl);
            }

            #endregion

            #region Set-RequestType

            SetRequestType();

            #endregion 
        }

        /// <summary>
        /// Determine if a header exists.
        /// </summary>
        /// <param name="key">Header key.</param>
        /// <param name="caseSensitive">Specify whether a case sensitive search should be used.</param>
        /// <returns>True if exists.</returns>
        public bool HeaderExists(string key, bool caseSensitive)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            if (caseSensitive)
            {
                return Headers.ContainsKey(key);
            }
            else
            {
                if (Headers != null && Headers.Count > 0)
                {
                    foreach (KeyValuePair<string, string> header in Headers)
                    {
                        if (String.IsNullOrEmpty(header.Key)) continue;
                        if (header.Key.ToLower().Trim().Equals(key)) return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determine if a querystring entry exists.
        /// </summary>
        /// <param name="key">Querystring key.</param>
        /// <param name="caseSensitive">Specify whether a case sensitive search should be used.</param>
        /// <returns>True if exists.</returns>
        public bool QuerystringExists(string key, bool caseSensitive)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            if (Querystring != null && Querystring.Count > 0)
            {
                if (caseSensitive)
                {
                    return Querystring.ContainsKey(key);
                }
                else
                { 
                    foreach (KeyValuePair<string, string> queryElement in Querystring)
                    {
                        if (String.IsNullOrEmpty(queryElement.Key)) continue;
                        if (queryElement.Key.ToLower().Trim().Equals(key)) return true;
                    } 

                    return false;
                }
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
            if (Headers != null && Headers.Count > 0)
            {
                foreach (KeyValuePair<string, string> curr in Headers)
                {
                    if (String.IsNullOrEmpty(curr.Key)) continue;
                    if (String.Compare(curr.Key.ToLower(), key.ToLower()) == 0) return curr.Value;
                }
            }

            if (Querystring != null && Querystring.Count > 0)
            {
                foreach (KeyValuePair<string, string> curr in Querystring)
                {
                    if (String.IsNullOrEmpty(curr.Key)) continue;
                    if (String.Compare(curr.Key.ToLower(), key.ToLower()) == 0) return curr.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieve a querystring value.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Value.</returns>
        public string RetrieveQueryValue(string key)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key)); 

            if (Querystring != null && Querystring.Count > 0)
            {
                foreach (KeyValuePair<string, string> curr in Querystring)
                {
                    if (String.IsNullOrEmpty(curr.Key)) continue;
                    if (String.Compare(curr.Key.ToLower(), key.ToLower()) == 0) return curr.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Read a chunk from the request body.
        /// </summary>
        /// <returns>Chunk.</returns>
        public async Task<Chunk> ReadChunk()
        {
            return await Http.Request.ReadChunk().ConfigureAwait(false);
        }

        /// <summary>
        /// Determine if the signature was signed by the supplied secret key.
        /// </summary>
        /// <param name="secretKey">Base64 encoded secret key.</param>
        /// <returns>True if the signature was generated using the supplied secret key.</returns>
        public bool IsValidSignature(string secretKey)
        {
            if (String.IsNullOrEmpty(secretKey)) throw new ArgumentNullException(nameof(secretKey));
            if (!Common.IsBase64String(secretKey)) throw new ArgumentException("Supplied secret key is not base64 encoded.");
            return IsValidSignature(Encoding.UTF8.GetBytes(secretKey)); 
        }

        /// <summary>
        /// Determine if the signature was signed by the supplied secret key.
        /// </summary>
        /// <param name="secretKey">Secret key.</param>
        /// <returns>True if the signature was generated using the supplied secret key.</returns>
        public bool IsValidSignature(byte[] secretKey)
        {
            if (secretKey == null || secretKey.Length < 1) throw new ArgumentNullException(nameof(secretKey));
            return S3Signatures.IsValidSignature(this, secretKey, _Logger);
        }

        #endregion

        #region Private-Methods

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

        private void ParseHostnameAndUrl(string baseHostname, string fullUrl, string rawUrl)
        { 
            if (String.IsNullOrEmpty(fullUrl)) return;
            if (String.IsNullOrEmpty(rawUrl)) return;

            Uri uri = new Uri(fullUrl);
            Hostname = uri.Host;

            _Logger?.Invoke(_Header +
                "Parsing URL" + Environment.NewLine +
                "  Full URL      : " + fullUrl + Environment.NewLine +
                "  Raw URL       : " + rawUrl + Environment.NewLine + 
                "  Base Hostname : " + baseHostname);

            if (Common.IsIpAddress(Hostname))
            {
                RequestStyle = S3RequestStyle.BucketNotInHostname;
                _Logger?.Invoke(_Header + "Supplied hostname is an IP address");
            }
            else
            {
                if (String.IsNullOrEmpty(baseHostname))
                {
                    RequestStyle = S3RequestStyle.BucketNotInHostname;
                    _Logger?.Invoke(_Header + "Base hostname not supplied, request assumed to have bucket in URL");
                }
                else
                {
                    if (!Hostname.Contains(baseHostname))
                    {
                        RequestStyle = S3RequestStyle.BucketNotInHostname;
                        _Logger?.Invoke(_Header + "Base hostname not found in hostname [" + Hostname + "], assumed to have bucket in URL");
                    }
                    else
                    {
                        RequestStyle = S3RequestStyle.BucketInHostname;
                        Bucket = Common.ReplaceLastOccurrence(Hostname, baseHostname, "");
                        _Logger?.Invoke(_Header + "Bucket name [" + Bucket + "] found in hostname [" + Hostname + "]");
                    }
                }
            }

            while (rawUrl.StartsWith("/")) rawUrl = rawUrl.Substring(1);

            switch (RequestStyle)
            {
                case S3RequestStyle.BucketInHostname:
                    Key = WebUtility.UrlDecode(rawUrl);
                    break;

                case S3RequestStyle.BucketNotInHostname:
                    string[] valsInner = rawUrl.Split(new[] { '/' }, 2);
                    if (valsInner.Length > 0) Bucket = WebUtility.UrlDecode(valsInner[0]);
                    if (valsInner.Length > 1) Key = WebUtility.UrlDecode(valsInner[1]);
                    break;
            }

            _Logger?.Invoke(_Header + 
                "Parsed URL" + Environment.NewLine + 
                "  Full URL      : " + fullUrl + Environment.NewLine +
                "  Raw URL       : " + rawUrl + Environment.NewLine +
                "  Hostname      : " + Hostname + Environment.NewLine +
                "  Base Hostname : " + baseHostname + Environment.NewLine +
                "  Bucket name   : " + Bucket + Environment.NewLine +
                "  Style         : " + RequestStyle.ToString() + Environment.NewLine +
                "  Object key    : " + Key + Environment.NewLine +
                "  Region        : " + Region);
             
            return;
        }

        private void ParseRangeHeader(string header, out long start, out long end)
        {
            if (String.IsNullOrEmpty(header)) throw new ArgumentNullException(nameof(header));
            header = header.ToLower();
            if (header.StartsWith("bytes=")) header = header.Substring(6);
            string[] vals = header.Split('-');
            if (vals.Length != 2) throw new ArgumentException("Invalid range header: " + header);

            start = 0;
            end = 0;

            if (!String.IsNullOrEmpty(vals[0])) start = Convert.ToInt64(vals[0]);
            if (!String.IsNullOrEmpty(vals[1])) end = Convert.ToInt64(vals[1]);
        }
         
        private void SetRequestType()
        {
            switch (Method)
            {
                case HttpMethod.HEAD:
                    #region HEAD

                    if (!String.IsNullOrEmpty(Bucket) && String.IsNullOrEmpty(Key))
                        RequestType = RequestType = S3RequestType.BucketExists;
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
                        if (QuerystringExists("acl", false))
                            RequestType = S3RequestType.BucketReadAcl;
                        else if (QuerystringExists("location", false))
                            RequestType = S3RequestType.BucketReadLocation;
                        else if (QuerystringExists("logging", false))
                            RequestType = S3RequestType.BucketReadLogging;
                        else if (QuerystringExists("tagging", false))
                            RequestType = S3RequestType.BucketReadTags;
                        else if (QuerystringExists("versions", false))
                            RequestType = S3RequestType.BucketReadVersions;
                        else if (QuerystringExists("versioning", false))
                            RequestType = S3RequestType.BucketReadVersioning;
                        else if (QuerystringExists("website", false))
                            RequestType = S3RequestType.BucketReadWebsite;
                        else
                            RequestType = S3RequestType.BucketRead;
                    }
                    else if (!String.IsNullOrEmpty(Bucket) && !String.IsNullOrEmpty(Key))
                    {
                        if (QuerystringExists("range", false))
                        {
                            if (RangeEnd > 0) RequestType = S3RequestType.ObjectReadRange;
                            else RequestType = S3RequestType.ObjectRead;
                        }
                        else if (QuerystringExists("acl", false))
                            RequestType = S3RequestType.ObjectReadAcl;
                        else if (QuerystringExists("tagging", false))
                            RequestType = S3RequestType.ObjectReadTags;
                        else if (QuerystringExists("legal-hold", false))
                            RequestType = S3RequestType.ObjectReadLegalHold;
                        else if (QuerystringExists("retention", false))
                            RequestType = S3RequestType.ObjectReadRetention;
                        else
                            RequestType = S3RequestType.ObjectRead;
                    }

                    break;

                #endregion

                case HttpMethod.PUT:
                    #region PUT

                    if (!String.IsNullOrEmpty(Bucket) && String.IsNullOrEmpty(Key))
                    {
                        if (QuerystringExists("acl", false))
                            RequestType = S3RequestType.BucketWriteAcl;
                        else if (QuerystringExists("logging", false))
                            RequestType = S3RequestType.BucketWriteLogging;
                        else if (QuerystringExists("tagging", false))
                            RequestType = S3RequestType.BucketWriteTags;
                        else if (QuerystringExists("versioning", false))
                            RequestType = S3RequestType.BucketWriteVersioning;
                        else if (QuerystringExists("website", false))
                            RequestType = S3RequestType.BucketWriteWebsite;
                        else
                            RequestType = S3RequestType.BucketWrite;
                    }
                    else if (!String.IsNullOrEmpty(Bucket) && !String.IsNullOrEmpty(Key))
                    {
                        if (QuerystringExists("tagging", false))
                            RequestType = S3RequestType.ObjectWriteTags;
                        else if (QuerystringExists("acl", false))
                            RequestType = S3RequestType.ObjectWriteAcl;
                        else if (QuerystringExists("legal-hold", false))
                            RequestType = S3RequestType.ObjectWriteLegalHold;
                        else if (QuerystringExists("retention", false))
                            RequestType = S3RequestType.ObjectWriteRetention;
                        else
                            RequestType = S3RequestType.ObjectWrite;
                    }

                    break;

                #endregion

                case HttpMethod.POST:
                    #region POST

                    if (!String.IsNullOrEmpty(Bucket))
                    {
                        if (QuerystringExists("delete", false))
                            RequestType = S3RequestType.ObjectDeleteMultiple;
                    }

                    break;

                #endregion

                case HttpMethod.DELETE:
                    #region DELETE

                    if (!String.IsNullOrEmpty(Bucket) && String.IsNullOrEmpty(Key))
                    {
                        if (QuerystringExists("tagging", false))
                            RequestType = S3RequestType.BucketDeleteTags;
                        else if (QuerystringExists("website", false))
                            RequestType = S3RequestType.BucketDeleteWebsite;
                        else
                            RequestType = S3RequestType.BucketDelete;
                    }
                    else if (!String.IsNullOrEmpty(Bucket) && !String.IsNullOrEmpty(Key))
                    {
                        if (QuerystringExists("tagging", false))
                            RequestType = S3RequestType.ObjectDeleteTags;
                        else
                            RequestType = S3RequestType.ObjectDelete;
                    }

                    break;

                    #endregion
            } 
        }

        #endregion
    }
}
