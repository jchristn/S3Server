using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
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
        /// HTTP request from which this response was created.
        /// </summary>
        public HttpRequest Http { get; set; }

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
        /// The length of the payload.
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// The content type of the payload.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// URL querystring.
        /// </summary>
        public Dictionary<string, string> Querystring { get; set; }

        /// <summary>
        /// Full set of HTTP headers.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// AWS region.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Hostname.
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Indicates if the request includes the bucket name in the hostname or not.
        /// </summary>
        public RequestStyle Style { get; set; }

        /// <summary>
        /// Bucket.
        /// </summary>
        public string Bucket { get; set; }

        /// <summary>
        /// Object key.
        /// </summary>
        public string Key { get; set; }

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
        /// Request body.
        /// </summary>
        public byte[] Data { get; set; }

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

        }

        /// <summary>
        /// Instantiate the object and populate from an HTTP request.
        /// </summary>
        /// <param name="req">HttpRequest.</param>
        public S3Request(HttpRequest req, bool debug)
        { 
            if (req == null) throw new ArgumentNullException(nameof(req));

            _Debug = debug;

            #region Initialize

            Http = req;
            SourceIp = req.SourceIp;
            SourcePort = req.SourcePort;
            Method = req.Method;
            FullUrl = req.FullUrl;
            RawUrl = req.RawUrlWithoutQuery;
            ContentLength = 0;
            ContentType = req.ContentType;
            Querystring = req.QuerystringEntries;
            Headers = req.Headers;
            Region = null;
            Hostname = req.DestHostname;
            Style = RequestStyle.Unknown;
            Bucket = null;
            Key = null;
            Authorization = null;
            AccessKey = null;
            Data = null;

            #endregion

            #region Set-Data-and-Content-Length

            if (req.Data != null && req.Data.Length > 0)
            {
                ContentLength = req.Data.Length;
                Data = new byte[req.Data.Length];
                Buffer.BlockCopy(req.Data, 0, Data, 0, req.Data.Length);
            }

            #endregion

            #region Set-Authorization-and-Access-Key
            
            if (Headers != null && Headers.Count > 0)
            {
                if (Headers.ContainsKey("Authorization"))
                {
                    if (_Debug) Console.WriteLine("Processing Authorization header");

                    Authorization = Headers["Authorization"]; 
                    string accessKey = null;
                    string signature = null;
                    string region = null;
                    ParseAuthorizationHeader(Authorization, out accessKey, out signature, out region);

                    if (!String.IsNullOrEmpty(accessKey)) AccessKey = accessKey;
                    if (!String.IsNullOrEmpty(region)) Region = region;
                    if (!String.IsNullOrEmpty(signature)) Signature = signature;
                }
            }

            #endregion

            #region Set-Region-Bucket-Style-and-Key
             
            if (!String.IsNullOrEmpty(Hostname) && !String.IsNullOrEmpty(RawUrl))
            {
                string bucketName = null;
                string region = null;
                RequestStyle style = RequestStyle.Unknown;
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
                if (style != RequestStyle.Unknown) Style = style;
                if (!String.IsNullOrEmpty(objectKey)) Key = objectKey;
            }  

            #endregion 

            return;
        }
         
        #endregion

        #region Public-Methods

        /// <summary>
        /// Returns a human-readable string with the object details.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            string ret = "---" + Environment.NewLine;
            ret += "  Source IP:Port : " + SourceIp + ":" + SourcePort + Environment.NewLine;
            ret += "  Method         : " + Method.ToString() + Environment.NewLine;
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
            ret += "  Hostname       : " + Hostname + Environment.NewLine;
            ret += "  Bucket         : " + Bucket + Environment.NewLine;
            ret += "  Key            : " + Key + Environment.NewLine;
            ret += "  Authorization  : " + Authorization + Environment.NewLine;
            ret += "  Signature      : " + Signature + Environment.NewLine;
            ret += "  AccessKey      : " + AccessKey + Environment.NewLine;

            ret += "  Data           : ";
            if (Data != null && Data.Length > 0)
            {
                ret += Data.Length + " bytes" + Environment.NewLine;
            }
            else
            {
                ret += "(none)" + Environment.NewLine;
            }

            return ret;
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

            string[] valsOuter = authHeader.Split(' ');
            if (valsOuter == null || valsOuter.Length < 2) throw new ArgumentException(exceptionMsg);

            if (_Debug) Console.WriteLine("Authorization header starts with: " + valsOuter[0]);

            if (valsOuter[0].Equals("AWS"))
            {
                #region Legacy-Auth-Header

                // see https://docs.aws.amazon.com/AmazonS3/latest/dev/RESTAuthentication.html#ConstructingTheAuthenticationHeader
                // Authorization: AWS AWSAccessKeyId:Signature

                string[] valsInner = valsOuter[1].Split(':');
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
                // valsOuter[0] AWS4-HMAC-SHA256 <space>
                // valsOuter[1] Credential=access/20190418/us-east-1/s3/aws4_request, <space>
                // valsOuter[2] SignedHeaders=content-length;content-type;host;user-agent;x-amz-content-sha256;x-amz-date;x-amz-decoded-content-length, <space>
                // valsOuter[3] Signature=66946e06895806f4e32d32217c1a02313b9d9235b759f3a690742c8f9971daa0

                // credentials
                string[] credentialVals = valsOuter[1].Split('/');
                if (credentialVals.Length != 5) throw new ArgumentException(exceptionMsg);
                accessKey = credentialVals[0];
                if (accessKey.StartsWith("Credential=")) accessKey = accessKey.Substring(11);
                region = credentialVals[2];
                 
                // Signature
                signature = valsOuter[3];
                if (signature.StartsWith("Signature=")) signature = signature.Substring(10);

                if (_Debug) Console.WriteLine("- Access key [" + accessKey + "] region [" + region + "] signature [" + signature + "]");

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
            out RequestStyle style,
            out string objectKey)
        {
            bucketName = null;
            region = null;
            style = RequestStyle.Unknown;
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
                style = RequestStyle.BucketInHostname;

                if (_Debug) Console.WriteLine("- Bucket name [" + bucketName + "] style [" + style.ToString() + "]");

                return;
            }
            else if (hostnameVals.Length == 3)
            {
                // s3-region.amazonaws.com
                // do not return, URL processing required for bucketname
                if (hostnameVals[0].Length > 3) region = hostnameVals[1].Substring(3);
                style = RequestStyle.BucketNotInHostname;

                if (_Debug) Console.WriteLine("- Region [" + region + "] style [" + style.ToString() + "]"); 
            }
            else
            {
                style = RequestStyle.BucketNotInHostname;
            }

            if (String.IsNullOrEmpty(bucketName))
            {
                if (!String.IsNullOrEmpty(rawUrl))
                {
                    if (rawUrl.StartsWith("/")) rawUrl = rawUrl.Substring(1);

                    string[] valsInner = rawUrl.Split('/');

                    switch (style)
                    {
                        case RequestStyle.BucketInHostname:
                            if (valsInner.Length > 0) objectKey = valsInner[0];
                            break;

                        case RequestStyle.BucketNotInHostname:
                            if (valsInner.Length > 0) bucketName = valsInner[0];
                            if (valsInner.Length > 1) objectKey = valsInner[1];
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

        #endregion
    }
}
