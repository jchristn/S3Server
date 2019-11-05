using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
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
        public S3RequestStyle RequestStyle { get; set; }

        /// <summary>
        /// Indicates the type of S3 request.
        /// </summary>
        public S3RequestType RequestType { get; set; }

        /// <summary>
        /// Time of creation in UTC.
        /// </summary>
        public DateTime TimestampUtc = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// HTTP context from which this response was created.
        /// </summary>
        public HttpContext Http { get; set; } 

        /// <summary>
        /// IP address of the client.
        /// </summary>
        public string SourceIp { get; set; }

        /// <summary>
        /// TCP port of the client.
        /// </summary>
        public int SourcePort { get; set; }

        /// <summary>
        /// HTTP method (GET, PUT, POST, DELETE, etc).
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Full URL.
        /// </summary>
        public string FullUrl { get; set; }

        /// <summary>
        /// The raw URL without querystring.
        /// </summary>
        public string RawUrl { get; set; }

        /// <summary>
        /// The individual elements in the raw URL.
        /// </summary>
        public List<string> RawUrlEntries = new List<string>();

        /// <summary>
        /// The length of the payload.
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// The content type of the payload.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Indicates if chunked transfer-encoding is in use.
        /// </summary>
        public bool Chunked { get; set; }

        /// <summary>
        /// URL querystring.
        /// </summary>
        public Dictionary<string, string> Querystring = new Dictionary<string, string>();

        /// <summary>
        /// Full set of HTTP headers.
        /// </summary>
        public Dictionary<string, string> Headers = new Dictionary<string, string>();

        /// <summary>
        /// AWS region.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Hostname.
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Bucket.
        /// </summary>
        public string Bucket { get; set; }

        /// <summary>
        /// Object key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Object key prefix.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Maximum number of keys to retrieve in an enumeration.
        /// </summary>
        public long? MaxKeys = null;

        /// <summary>
        /// Object version ID.
        /// </summary>
        public string VersionId { get; set; }

        /// <summary>
        /// Authorization header string, in full.
        /// </summary>
        public string Authorization { get; set; }

        /// <summary>
        /// Signature from authorization header.
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// Access key, parsed from authorization header.
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// Start value from the Range header.
        /// </summary>
        public long? RangeStart = null;

        /// <summary>
        /// End value from the Range header.
        /// </summary>
        public long? RangeEnd = null;

        /// <summary>
        /// Stream containing the request body.
        /// </summary>
        [JsonIgnore]
        public Stream Data { get; set; }

        #endregion

        #region Private-Members

        private bool _Debug = false;  
        
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
        /// Instantiates the object.
        /// </summary>
        /// <param name="ctx">HttpContext.</param> 
        /// <param name="debug">Enable or disable console logging of construction.</param> 
        public S3Request(HttpContext ctx, bool debug)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (ctx.Request == null) throw new ArgumentNullException(nameof(ctx.Request));

            _Debug = debug;
             
            #region Initialize

            TimestampUtc = DateTime.Now.ToUniversalTime();
            Http = ctx;
            SourceIp = Http.Request.SourceIp;
            SourcePort = Http.Request.SourcePort;
            Method = Http.Request.Method;
            FullUrl = Http.Request.FullUrl;
            RawUrl = Http.Request.RawUrlWithoutQuery;
            while (RawUrl.Contains("\\\\")) RawUrl.Replace("\\\\", "\\");

            RawUrlEntries = Http.Request.RawUrlEntries;
            ContentLength = Http.Request.ContentLength;
            ContentType = Http.Request.ContentType;
            Chunked = Http.Request.ChunkedTransfer;
            Querystring = Http.Request.QuerystringEntries;
            Headers = Http.Request.Headers;
            Region = null;
            Hostname = Http.Request.DestHostname;
            RequestType = S3RequestType.Unknown;
            RequestStyle = S3RequestStyle.Unknown;
            Bucket = null;
            Key = null;
            Authorization = null;
            AccessKey = null;
            Data = Http.Request.Data;

            #endregion

            #region Set-Authorization-Access-Key-and-Streaming

            if (Headers != null && Headers.Count > 0)
            {
                if (Headers.ContainsKey("Authorization"))
                {
                    if (_Debug) Console.WriteLine("Processing Authorization header");

                    Authorization = Headers["Authorization"];
                    ParseAuthorizationHeader(Authorization, out string accessKey, out string signature, out string region);

                    if (!String.IsNullOrEmpty(accessKey)) AccessKey = accessKey;
                    if (!String.IsNullOrEmpty(region)) Region = region;
                    if (!String.IsNullOrEmpty(signature)) Signature = signature;
                }
            }
             
            string sha256content = Http.Request.RetrieveHeaderValue("X-Amz-Content-SHA256");
            if (!String.IsNullOrEmpty(sha256content))
                if (sha256content.Contains("STREAMING")) Chunked = true;

            #endregion

            #region Set-Region-Bucket-Style-and-Key

            if (!String.IsNullOrEmpty(Hostname) && !String.IsNullOrEmpty(RawUrl))
            {
                string bucketName = null;
                string region = null;
                S3RequestStyle style = S3RequestStyle.Unknown;
                string objectKey = null;

                ParseHostnameAndUrl(
                    FullUrl,
                    RawUrl,
                    out bucketName,
                    out region,
                    out style,
                    out objectKey);

                if (!String.IsNullOrEmpty(bucketName)) Bucket = bucketName;
                if (!String.IsNullOrEmpty(region)) Region = region;
                if (style != S3RequestStyle.Unknown) RequestStyle = style;
                if (!String.IsNullOrEmpty(objectKey)) Key = objectKey;
            }

            #endregion

            #region Set-Parameters-from-Querystring

            if (Http.Request.QuerystringEntries != null && Http.Request.QuerystringEntries.ContainsKey("versionId"))
            {
                VersionId = Http.Request.QuerystringEntries["versionId"];
            }

            if (Http.Request.QuerystringEntries != null && Http.Request.QuerystringEntries.ContainsKey("prefix"))
            {
                Prefix = Http.Request.QuerystringEntries["prefix"];
            }

            if (Http.Request.QuerystringEntries != null && Http.Request.QuerystringEntries.ContainsKey("max-keys"))
            {
                long maxKeys = 0;
                if (!Int64.TryParse(Http.Request.QuerystringEntries["max-keys"], out maxKeys))
                {
                    MaxKeys = 0;
                }
                else
                {
                    MaxKeys = maxKeys;
                }
            }

            #endregion

            #region Set-Parameters-from-Headers

            if (Headers != null)
            {
                if (Headers.ContainsKey("Range"))
                {
                    long start = 0;
                    long end = 0;
                    ParseRangeHeader(Headers["Range"], out start, out end);

                    RangeStart = start;
                    RangeEnd = end;
                }
            }

            #endregion

            #region Set-RequestType

            switch (Method)
            {
                case HttpMethod.HEAD:
                    #region HEAD

                    if (ctx.Request.RawUrlEntries.Count == 1)
                    {
                       RequestType = S3RequestType.BucketExists;
                    }
                    else if (ctx.Request.RawUrlEntries.Count >= 2)
                    {
                        RequestType = S3RequestType.ObjectExists;
                    }

                    break;

                #endregion

                case HttpMethod.GET:
                    #region GET

                    if (ctx.Request.RawUrlEntries.Count == 0)
                    {
                        RequestType = S3RequestType.ListBuckets;
                    }
                    else if (ctx.Request.RawUrlEntries.Count == 1)
                    {
                        if (ctx.Request.QuerystringEntries.ContainsKey("acl"))
                        {
                            RequestType = S3RequestType.BucketReadAcl;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("location"))
                        {
                            RequestType = S3RequestType.BucketReadLocation;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("logging"))
                        {
                            RequestType = S3RequestType.BucketReadLogging;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("tagging"))
                        {
                            RequestType = S3RequestType.BucketReadTags;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("versions"))
                        {
                            RequestType = S3RequestType.BucketReadVersions;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("versioning"))
                        {
                            RequestType = S3RequestType.BucketReadVersioning;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("website"))
                        {
                            RequestType = S3RequestType.BucketReadWebsite;
                        }
                        else
                        {
                            RequestType = S3RequestType.BucketRead;
                        }
                    }
                    else if (ctx.Request.RawUrlEntries.Count >= 2)
                    {
                        if (ctx.Request.Headers.ContainsKey("Range"))
                        {
                            if (RangeEnd > 0) RequestType = S3RequestType.ObjectReadRange;
                            else RequestType = S3RequestType.ObjectRead;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("acl"))
                        {
                            RequestType = S3RequestType.ObjectReadAcl;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("tagging"))
                        {
                            RequestType = S3RequestType.ObjectReadTags;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("legal-hold"))
                        {
                            RequestType = S3RequestType.ObjectReadLegalHold;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("retention"))
                        {
                            RequestType = S3RequestType.ObjectReadRetention;
                        }
                        else
                        {
                            RequestType = S3RequestType.ObjectRead;
                        }
                    }

                    break;

                #endregion

                case HttpMethod.PUT:
                    #region PUT

                    if (ctx.Request.RawUrlEntries.Count == 1)
                    {
                        if (ctx.Request.QuerystringEntries.ContainsKey("acl"))
                        {
                            RequestType = S3RequestType.BucketWriteAcl;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("logging"))
                        {
                            RequestType = S3RequestType.BucketWriteLogging;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("tagging"))
                        {
                            RequestType = S3RequestType.BucketWriteTags;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("versioning"))
                        {
                            RequestType = S3RequestType.BucketWriteVersioning;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("website"))
                        {
                            RequestType = S3RequestType.BucketWriteWebsite;
                        }
                        else
                        {
                            RequestType = S3RequestType.BucketWrite;
                        }
                    }
                    else if (ctx.Request.RawUrlEntries.Count >= 2)
                    {
                        if (ctx.Request.QuerystringEntries.ContainsKey("tagging"))
                        {
                            RequestType = S3RequestType.ObjectWriteTags;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("acl"))
                        {
                            RequestType = S3RequestType.ObjectWriteAcl;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("legal-hold"))
                        {
                            RequestType = S3RequestType.ObjectWriteLegalHold;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("retention"))
                        {
                            RequestType = S3RequestType.ObjectWriteRetention;
                        }
                        else
                        {
                            RequestType = S3RequestType.ObjectWrite;
                        }
                    }

                    break;

                #endregion

                case HttpMethod.POST:
                    #region POST

                    if (ctx.Request.RawUrlEntries.Count >= 1)
                    {
                        if (ctx.Request.QuerystringEntries.ContainsKey("delete"))
                        {
                            RequestType = S3RequestType.ObjectDeleteMultiple;
                        }
                    }

                    break;

                #endregion

                case HttpMethod.DELETE:
                    #region DELETE

                    if (ctx.Request.RawUrlEntries.Count == 1)
                    {
                        if (ctx.Request.QuerystringEntries.ContainsKey("tagging"))
                        {
                            RequestType = S3RequestType.BucketDeleteTags;
                        }
                        else if (ctx.Request.QuerystringEntries.ContainsKey("website"))
                        {
                            RequestType = S3RequestType.BucketDeleteWebsite;
                        }
                        else
                        {
                            RequestType = S3RequestType.BucketDelete;
                        }
                    }
                    else if (ctx.Request.RawUrlEntries.Count >= 2)
                    {
                        if (ctx.Request.QuerystringEntries.ContainsKey("tagging"))
                        {
                            RequestType = S3RequestType.ObjectDeleteTags;
                        }
                        else
                        {
                            RequestType = S3RequestType.ObjectDelete;
                        }
                    }

                    break;

                #endregion 
            }

            #endregion
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Returns a human-readable string with the object details.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            string ret = Environment.NewLine + "---" + Environment.NewLine;
            ret += "  Source IP:Port : " + SourceIp + ":" + SourcePort + Environment.NewLine;
            ret += "  Method         : " + Method.ToString() + Environment.NewLine;
            ret += "  Hostname       : " + Hostname + Environment.NewLine;
            ret += "  FullUrl        : " + FullUrl + Environment.NewLine;
            ret += "  RawUrl         : " + RawUrl + Environment.NewLine;
            ret += "  Content Length : " + ContentLength + " bytes " + Environment.NewLine;
            ret += "  Content Type   : " + ContentType + Environment.NewLine;

            ret += "  Querystring   : ";
            if (Querystring != null && Querystring.Count > 0)
            {
                ret += Environment.NewLine;
                foreach (KeyValuePair<string, string> curr in Querystring)
                {
                    if (String.IsNullOrEmpty(curr.Key)) continue;
                    ret += "    " + curr.Key + "=" + curr.Value + Environment.NewLine;
                }
            }
            else
            {
                ret += "(none)" + Environment.NewLine;
            }

            ret += "  Headers        : ";
            if (Headers != null && Headers.Count > 0)
            {
                ret += Environment.NewLine;
                foreach (KeyValuePair<string, string> curr in Headers)
                {
                    if (String.IsNullOrEmpty(curr.Key)) continue;
                    ret += "    " + curr.Key + "=" + curr.Value + Environment.NewLine;
                }
            }
            else
            {
                ret += "(none)" + Environment.NewLine;
            }

            ret += "  Region         : " + Region + Environment.NewLine;
            ret += "  Type           : " + RequestType.ToString() + Environment.NewLine;
            ret += "  Style          : " + RequestStyle.ToString() + Environment.NewLine;
            ret += "  Bucket         : " + Bucket + Environment.NewLine;
            ret += "  Key            : " + Key + Environment.NewLine;
            ret += "  Authorization  : " + Authorization + Environment.NewLine;
            ret += "  Signature      : " + Signature + Environment.NewLine;
            ret += "  AccessKey      : " + AccessKey + Environment.NewLine;

            ret += "  Data           : "; 
            if (Data != null)
            {
                ret += "(stream, " + ContentLength + " bytes)" + Environment.NewLine;
            }
            else
            {
                ret += "(none)" + Environment.NewLine;
            } 

            return ret;
        }

        /// <summary>
        /// Retrieve a header (or querystring) value.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Value.</returns>
        public string RetrieveHeaderValue(string key)
        {
            return Http.Request.RetrieveHeaderValue(key);
        }

        /// <summary>
        /// Read a chunk from the request body.
        /// </summary>
        /// <returns>Chunk.</returns>
        public async Task<Chunk> ReadChunk()
        {
            return await Http.Request.ReadChunk();
        }

        #endregion

        #region Private-Methods
         
        private void ParseAuthorizationHeader(
            string authHeader, 
            out string accessKey, 
            out string signature,
            out string region)
        {
            accessKey = null;
            signature = null;
            region = null;
            if (String.IsNullOrEmpty(authHeader)) return;
            string exceptionMsg = "Invalid authorization header format: " + authHeader;

            string[] valsOuter = authHeader.Split(new[] { ' ' }, 2);
            if (valsOuter == null || valsOuter.Length < 2) throw new ArgumentException(exceptionMsg);

            if (_Debug)
            {
                Console.WriteLine("Authorization header: " + authHeader);
                Console.WriteLine("Outer authorization header values:");
                for (int i = 0; i < valsOuter.Length; i++)
                {
                    Console.WriteLine("  " + i + ": " + valsOuter[i]);
                }
            }

            if (valsOuter[0].Equals("AWS"))
            {
                #region Legacy-Auth-Header

                // see https://docs.aws.amazon.com/AmazonS3/latest/dev/RESTAuthentication.html#ConstructingTheAuthenticationHeader
                // Authorization: AWS AWSAccessKeyId:Signature

                string[] valsInner = valsOuter[1].Split(':');

                if (_Debug)
                {
                    Console.WriteLine("Inner authorization header values:");
                    for (int i = 0; i < valsInner.Length; i++)
                    {
                        Console.WriteLine("  " + i + ": " + valsInner[i]);
                    }
                }

                if (valsInner.Length != 2) throw new ArgumentException(exceptionMsg);
                accessKey = valsInner[0];
                signature = valsInner[1];

                if (_Debug) Console.WriteLine("- Access key [" + accessKey + "] signature [" + signature + "]");

                return;

                #endregion
            }
            else if (valsOuter[0].Equals("AWS4-HMAC-SHA256"))
            {
                #region Modern-Auth-Header

                // see https://docs.aws.amazon.com/AmazonS3/latest/API/sigv4-auth-using-authorization-header.html
                // 
                // AWS4-HMAC-SHA256 Credential=access/20190418/us-east-1/s3/aws4_request, SignedHeaders=content-length;content-type;host;user-agent;x-amz-content-sha256;x-amz-date;x-amz-decoded-content-length, Signature=66946e06895806f4e32d32217c1a02313b9d9235b759f3a690742c8f9971daa0
                //
                // valsOuter[0] AWS4-HMAC-SHA256
                // valsOuter[1] everything else...
                 
                string[] keyValuePairs = valsOuter[1].Split(',');

                if (_Debug)
                {
                    Console.WriteLine("Inner authorization header values:");
                    for (int i = 0; i < keyValuePairs.Length; i++)
                    {
                        Console.WriteLine("  " + i + ": " + keyValuePairs[i]);
                    }
                }

                // keyValuePairs[0]: Credential=...
                // keyValuePairs[1]: SignedHeaders=...
                // keyValuePairs[3]: Signature=...

                // credentials
                string[] credentialVals = keyValuePairs[0].Split('/');
                if (credentialVals.Length < 5) throw new ArgumentException(exceptionMsg);
                accessKey = credentialVals[0].Trim();
                if (accessKey.StartsWith("Credential=")) accessKey = accessKey.Substring(11);
                region = credentialVals[2].Trim();
                 
                // Signature
                signature = keyValuePairs[2].Trim();
                if (signature.StartsWith("Signature=")) signature = signature.Substring(10);

                if (_Debug) Console.WriteLine("Access key [" + accessKey + "] region [" + region + "] signature [" + signature + "]");

                return;

                #endregion
            }
            else
            {
                throw new ArgumentException(exceptionMsg + authHeader);
            }
        }

        private void ParseHostnameAndUrl(
            string fullUrl,
            string rawUrl,
            out string bucketName,
            out string region,
            out S3RequestStyle style,
            out string objectKey)
        {
            bucketName = null;
            region = null;
            style = S3RequestStyle.Unknown;
            objectKey = null;
            if (String.IsNullOrEmpty(fullUrl)) return;
            if (String.IsNullOrEmpty(rawUrl)) return;

            Uri uri = new Uri(fullUrl);

            if (_Debug) Console.WriteLine("Parsing full URL " + fullUrl + " raw URL " + rawUrl);

            string[] hostnameVals = uri.Host.Split('.'); 

            if (hostnameVals.Length == 4)
            {
                // bucket.s3.amazonaws.com
                // bucket.s3-<region>.amazonaws.com
                bucketName = hostnameVals[0];
                if (hostnameVals[1].Length > 3) region = hostnameVals[1].Substring(3);
                style = S3RequestStyle.BucketInHostname;

                if (_Debug) Console.WriteLine("- Bucket name [" + bucketName + "] style [" + style.ToString() + "]");

                return;
            }
            else if (hostnameVals.Length == 3)
            {
                // s3-region.amazonaws.com
                // do not return, URL processing required for bucketname
                if (hostnameVals[0].Length > 3) region = hostnameVals[1].Substring(3);
                style = S3RequestStyle.BucketNotInHostname;

                if (_Debug) Console.WriteLine("- Region [" + region + "] style [" + style.ToString() + "]"); 
            }
            else
            {
                style = S3RequestStyle.BucketNotInHostname;
            }

            if (String.IsNullOrEmpty(bucketName))
            {
                if (!String.IsNullOrEmpty(rawUrl))
                {
                    if (rawUrl.StartsWith("/")) rawUrl = rawUrl.Substring(1);

                    switch (style)
                    {
                        case S3RequestStyle.BucketInHostname:
                            objectKey = WebUtility.UrlDecode(rawUrl);
                            break;

                        case S3RequestStyle.BucketNotInHostname:
                            string[] valsInner = rawUrl.Split(new[] { '/' }, 2); 
                            if (valsInner.Length > 0) bucketName = WebUtility.UrlDecode(valsInner[0]);
                            if (valsInner.Length > 1) objectKey = WebUtility.UrlDecode(valsInner[1]);
                            break;
                    }
                }
            }

            if (_Debug)
            {
                Console.WriteLine("- Returning:");
                Console.WriteLine("  - Bucket name   : " + bucketName);
                Console.WriteLine("  - Style         : " + style.ToString());
                Console.WriteLine("  - Object key    : " + objectKey);
                Console.WriteLine("  - Region        : " + region);
            }

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

        #endregion
    }
}
