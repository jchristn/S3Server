using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using WatsonWebserver;

namespace S3ServerLibrary
{
    /// <summary>
    /// S3 request.
    /// </summary>
    public class S3Request
    {
        #region Public-Members

        /// <summary>
        /// Time of creation in UTC.
        /// </summary>
        public DateTime TimestampUtc { get; private set; } = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// Request ID.
        /// </summary>
        public string RequestId { get; set; } = Guid.NewGuid().ToString();

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
        /// Maximum number of keys to retrieve in an enumeration.
        /// </summary>
        public long MaxKeys { get; set; } = 0;

        /// <summary>
        /// Object version ID.
        /// </summary>
        public string VersionId { get; set; } = null;

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
        public long? RangeStart { get; set; } = null;

        /// <summary>
        /// End value from the Range header.
        /// </summary>
        public long? RangeEnd { get; set; } = null;

        /// <summary>
        /// Continuation token.
        /// </summary>
        public string ContinuationToken { get; set; } = null;

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
        [JsonPropertyOrder(998)]
        public List<string> SignedHeaders { get; set; } = new List<string>();
         
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

        private string _Header = "[S3Request] ";
        private HttpRequest _HttpRequest = null;
        private Action<string> _Logger = null;
        private List<string> _BaseDomains = new List<string>();

        private string _AmazonTimestampFormat = "yyyyMMddTHHmmssZ"; 
        private Dictionary<object, object> _UserMetadata = new Dictionary<object, object>();

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
        /// <param name="baseDomains">List of base domains against which the hostname should be evaluated to identify the bucket.  For instance, to resolve buckets for *.localhost, specify '.localhost'.</param> 
        /// <param name="logger">Method to invoke to send log messages.</param> 
        public S3Request(S3Context ctx, List<string> baseDomains = null, Action<string> logger = null)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (ctx.Http == null) throw new ArgumentNullException(nameof(ctx.Http));
            if (baseDomains != null) _BaseDomains = baseDomains;

            _HttpRequest = ctx.Http.Request;
            _Logger = logger;
            _BaseDomains = baseDomains;

            ParseHttpContext();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Populate members using an HttpContext.
        /// </summary>
        public void ParseHttpContext()
        {
            if (_HttpRequest == null) throw new InvalidOperationException("HTTP context not supplied in the S3 request object.");

            #region Initialize

            TimestampUtc = DateTime.Now.ToUniversalTime();
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

            if (_HttpRequest.Query.Elements != null && _HttpRequest.Query.Elements.Count > 0)
            {
                VersionId = RetrieveQueryValue("versionid");
                Prefix = RetrieveQueryValue("prefix");
                Delimiter = RetrieveQueryValue("delimiter");
                Marker = RetrieveQueryValue("marker");
                ContinuationToken = RetrieveQueryValue("continuation-token");
                Expires = RetrieveQueryValue("expires");
                AccessKey = RetrieveQueryValue("awsaccesskeyid");
                Signature = RetrieveQueryValue("signature");

                if (QuerystringExists("max-keys", false))
                {
                    long maxKeys = 0;
                    string maxKeysStr = RetrieveQueryValue("max-keys");
                    if (!String.IsNullOrEmpty(maxKeysStr))
                    {
                        if (Int64.TryParse(_HttpRequest.Query.Elements["max-keys"], out maxKeys))
                        {
                            MaxKeys = maxKeys;
                        }
                    }
                }
            }

            #endregion

            #region Set-Values-From-Headers

            if (_HttpRequest.Headers != null && _HttpRequest.Headers.Count > 0)
            { 
                if (HeaderExists("authorization", false))
                {
                    _Logger?.Invoke(_Header + "processing Authorization header"); 
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

                if (HeaderExists("content-md5", false)) ContentMd5 = RetrieveHeaderValue("content-md5");
                if (HeaderExists("content-type", false)) ContentType = RetrieveHeaderValue("content-type");
                if (HeaderExists("host", false)) Host = RetrieveHeaderValue("host");
                if (HeaderExists("x-amz-content-sha256", false))
                {
                    ContentSha256 = RetrieveHeaderValue("x-amz-content-sha256");
                    if (!String.IsNullOrEmpty(ContentSha256))
                    {
                        if (ContentSha256.ToLower().Contains("streaming"))
                        {
                            Chunked = true;
                            _HttpRequest.ChunkedTransfer = true;
                        }
                    }
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

        /// <summary>
        /// Determine if a header exists.
        /// </summary>
        /// <param name="key">Header key.</param>
        /// <param name="caseSensitive">Specify whether a case sensitive search should be used.</param>
        /// <returns>True if exists.</returns>
        public bool HeaderExists(string key, bool caseSensitive)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            return _HttpRequest.HeaderExists(key, caseSensitive);
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
            return _HttpRequest.QuerystringExists(key, caseSensitive);
        }

        /// <summary>
        /// Retrieve a header (or querystring) value.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Value.</returns>
        public string RetrieveHeaderValue(string key)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            return _HttpRequest.RetrieveHeaderValue(key);
        }

        /// <summary>
        /// Retrieve a querystring value.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Value.</returns>
        public string RetrieveQueryValue(string key)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key)); 

            if (_HttpRequest.Query.Elements != null && _HttpRequest.Query.Elements.Count > 0)
            {
                foreach (KeyValuePair<string, string> curr in _HttpRequest.Query.Elements)
                {
                    if (String.IsNullOrEmpty(curr.Key)) continue;
                    if (String.Compare(curr.Key.ToLower(), key.ToLower()) == 0)
                    {
                        return WebUtility.UrlDecode(curr.Value);
                    }
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
            return await _HttpRequest.ReadChunk().ConfigureAwait(false);
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

        private void ParseHostnameAndUrl()
        {
            Uri uri = new Uri(_HttpRequest.Url.Full);
            Hostname = uri.Host;

            _Logger?.Invoke(_Header + "parsing URL " + _HttpRequest.Url.Full);

            if (Common.IsIpAddress(Hostname))
            {
                RequestStyle = S3RequestStyle.PathStyle;
                _Logger?.Invoke(_Header + "supplied hostname is an IP address");
            }
            else
            {
                if (_BaseDomains == null || _BaseDomains.Count < 1)
                {
                    RequestStyle = S3RequestStyle.PathStyle;
                    _Logger?.Invoke(_Header + "no base hostnames, request assumed to have bucket in URL");
                }
                else
                {
                    // currently looking for exact matches and Hostname has the bucket name in it
                    // need to look to see if any of the base domains are contained WITHIN the hostname
                    BaseDomain = GetMatchingBaseDomain(Hostname);
                    if (String.IsNullOrEmpty(BaseDomain))
                    {
                        RequestStyle = S3RequestStyle.PathStyle;
                        _Logger?.Invoke(_Header + "base hostname not found, assumed to have bucket in URL");
                    }
                    else
                    {
                        RequestStyle = S3RequestStyle.VirtualHostedStyle;

                        string tempBaseDomain = BaseDomain;
                        while (tempBaseDomain.StartsWith(".")) tempBaseDomain = tempBaseDomain.Substring(1);

                        string temp = Common.ReplaceLastOccurrence(Hostname, tempBaseDomain, "");
                        while (temp.EndsWith(".")) temp = temp.Substring(0, (temp.Length - 1));

                        Bucket = temp;

                        _Logger?.Invoke(_Header + "bucket " + Bucket + " found in hostname " + Hostname);
                    }
                }
            }

            string rawUrl = _HttpRequest.Url.RawWithoutQuery;

            while (rawUrl.StartsWith("/")) rawUrl = rawUrl.Substring(1);

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
            switch (_HttpRequest.Method)
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

        private string GetMatchingBaseDomain(string hostname)
        {
            string eval = "." + hostname;

            List<string> matches = new List<string>();

            foreach (string baseDomain in _BaseDomains)
            {
                if (eval.Equals(baseDomain)) return baseDomain;
                if (eval.EndsWith(baseDomain)) matches.Add(baseDomain);
            }

            if (matches.Count > 0)
            {
                return matches.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);
            }

            return null;
        }

        #endregion
    }
}
