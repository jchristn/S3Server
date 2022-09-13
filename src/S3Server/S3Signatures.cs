using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace S3ServerLibrary
{
    /// <summary>
    /// Internal class for validating S3 signatures.
    /// </summary>
    internal static class S3Signatures
    {
        // see https://docs.aws.amazon.com/AmazonS3/latest/API/sig-v4-header-based-auth.html

        internal static bool IsValidSignature(S3Context ctx, byte[] secretKey, Action<string> logger)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (secretKey == null || secretKey.Length < 1) throw new ArgumentNullException(nameof(secretKey));

            _S3Context = ctx;
            _SecretKey = secretKey;
            _Logger = logger;

            byte[] signingKey = null;
            string hmacSha1Signature = null;
            string hmacSha256Signature = null;
            string logMessage = null;

            if (_S3Context.Request.SignatureVersion == S3SignatureVersion.Version4)
            {
                #region V4

                signingKey = V4GenerateSigningKey(secretKey);

                hmacSha1Signature = Common.BytesToBase64(
                    Common.HmacSha1(Encoding.UTF8.GetBytes(_V4StringToSign), secretKey)
                    );

                hmacSha256Signature = Common.BytesToHex(
                    Common.HmacSha256(Encoding.UTF8.GetBytes(_V4StringToSign), signingKey)
                    )
                    .ToLower();

                logMessage =
                    _Header + "Signature V4 validation:" + Environment.NewLine + 
                    "  Canonical request" + Environment.NewLine +
                    "  -----------------" + Environment.NewLine +
                    _V4CanonicalRequest + Environment.NewLine +
                    Environment.NewLine +
                    "  String to sign" + Environment.NewLine +
                    "  --------------" + Environment.NewLine +
                    _V4StringToSign + Environment.NewLine +
                    Environment.NewLine +
                    "  Client-supplied signature : " + _S3Context.Request.Signature + Environment.NewLine +
                    "  Generated HMAC-SHA1       : " + hmacSha1Signature + Environment.NewLine +
                    "  Generated HMAC-SHA256     : " + hmacSha256Signature + Environment.NewLine +
                    "  Success                   : ";
                 
                if (hmacSha256Signature.Equals(_S3Context.Request.Signature) 
                    || hmacSha1Signature.Equals(_S3Context.Request.Signature))
                {
                    _Logger?.Invoke(logMessage + "true");
                    return true;
                }
                else
                {
                    _Logger?.Invoke(logMessage + "false");
                    return false;
                }

                #endregion
            }
            else
            {
                throw new InvalidOperationException("Unable to validate signature unless signature version is V4.");
            } 
        }

        #region Members

        private static S3Context _S3Context = null;
        private static byte[] _SecretKey = null;
        private static string _Header = "[S3Signature] ";
        private static Action<string> _Logger = null;
        private static string _AmazonTimestampFormat = "yyyyMMddTHHmmssZ";
        private static string _AmazonDateFormat = "yyyyMMdd";

        #endregion
         
        #region Signature-V4-Members

        private static string _V4StringToSign
        {
            get
            {
                string ret = "";

                ret += "AWS4-HMAC-SHA256" + "\n";
                ret += _S3Context.Request.TimestampUtc.ToString(_AmazonTimestampFormat) + "\n";
                ret += _S3Context.Request.TimestampUtc.ToString(_AmazonDateFormat) + "/" + _S3Context.Request.Region + "/s3/aws4_request" + "\n";
                ret += Common.BytesToHex(Common.Sha256(Encoding.UTF8.GetBytes(_V4CanonicalRequest))).ToLower();
                return ret;
            }
        }
        private static string _V4CanonicalRequest
        {
            get
            {
                string ret = "";

                ret += _S3Context.Http.Request.Method.ToString().ToUpper() + "\n";
                ret += _V4CanonicalUri + "\n";
                ret += _V4CanonicalQuerystring + "\n";

                // includes AWS and non-AWS headers
                ret += _V4CanonicalHeaders + "\n";
                ret += V4SignedHeadersToString() + "\n";

                if (!_S3Context.Request.Chunked) ret += _S3Context.Request.ContentSha256;
                else ret += "STREAMING-AWS4-HMAC-SHA256-PAYLOAD";

                return ret;
            }
        }
        private static string _V4CanonicalUri
        {
            get
            {
                string ret = "/";

                if (_S3Context.Request.RequestStyle == S3RequestStyle.PathStyle)
                {
                    if (!String.IsNullOrEmpty(_S3Context.Request.Bucket))
                    {
                        ret += WebUtility.UrlEncode(_S3Context.Request.Bucket);

                        if (!String.IsNullOrEmpty(_S3Context.Request.Key))
                        {
                            ret += "/" + _S3Context.Request.Key;
                        }
                    }
                }
                else if (_S3Context.Request.RequestStyle == S3RequestStyle.VirtualHostedStyle)
                {
                    if (!String.IsNullOrEmpty(_S3Context.Request.Key))
                    {
                        ret += _S3Context.Request.Key;
                    }
                }

                return ret;
            }
        }
        private static string _V4CanonicalQuerystring
        {
            get
            {
                string ret = "";

                foreach (KeyValuePair<string, string> query in _S3Context.Http.Request.Query.Elements)
                {
                    if (String.IsNullOrEmpty(query.Key)) continue;
                    if (!String.IsNullOrEmpty(ret)) ret += "&";
                    ret += WebUtility.UrlEncode(query.Key) + "=" + (!String.IsNullOrEmpty(query.Value) ? WebUtility.UrlEncode(query.Value) : "");
                } 

                return ret;
            }
        }
        private static string _V4CanonicalHeaders
        {
            get
            {
                string ret = "";

                if (_S3Context.Request.SignedHeaders != null && _S3Context.Request.SignedHeaders.Count > 0)
                {
                    foreach (string key in _S3Context.Request.SignedHeaders)
                    {
                        if (String.IsNullOrEmpty(key)) continue;
                        string val = _S3Context.Request.RetrieveHeaderValue(key);
                        ret += key.ToLower() + ":" + val + "\n";
                    }
                }

                return ret;
            }
        }
        private static List<string> _V4CanonicalQuerystringItems = new List<string>
        {
            "acl",
            "delete",
            "lifecycle",
            "location",
            "logging",
            "notification",
            "partNumber",
            "policy",
            "requestPayment",
            "torrent",
            "uploadId",
            "uploads",
            "versionId",
            "versioning",
            "versions",
            "website",
            "response-content-type",
            "response-content-language",
            "response-expires",
            "response-cache-control",
            "response-content-disposition",
            "response-content-encoding"
        };

        #endregion

        #region Signature-V4-Methods

        private static string V4SignedHeadersToString()
        {
            string ret = "";

            foreach (string header in _S3Context.Request.SignedHeaders)
            {
                if (String.IsNullOrEmpty(header)) continue;
                if (String.IsNullOrEmpty(ret)) ret += header.ToLower();
                else ret += ";" + header.ToLower();
            }

            return ret;
        }
         
        private static byte[] V4GenerateSigningKey(byte[] secretKey)
        {
            byte[] dateKey = Common.HmacSha256(
                Encoding.UTF8.GetBytes(_S3Context.Request.TimestampUtc.ToString(_AmazonDateFormat)),
                Encoding.UTF8.GetBytes("AWS4" + Encoding.UTF8.GetString(secretKey))
                );

            byte[] dateRegionKey = Common.HmacSha256(
                Encoding.UTF8.GetBytes(_S3Context.Request.Region),
                dateKey
                );

            byte[] dateRegionServiceKey = Common.HmacSha256(
                Encoding.UTF8.GetBytes("s3"),
                dateRegionKey
                );

            byte[] signingKey = Common.HmacSha256(
                Encoding.UTF8.GetBytes("aws4_request"),
                dateRegionServiceKey
                );

            return signingKey;
        }

        #endregion
    }
}
