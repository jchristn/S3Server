using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace S3ServerInterface
{
    /// <summary>
    /// Internal class for validating S3 signatures.
    /// </summary>
    internal static class S3Signatures
    {
        internal static bool IsValidSignature(S3Request req, byte[] secretKey, Action<string> logger)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));
            if (secretKey == null || secretKey.Length < 1) throw new ArgumentNullException(nameof(secretKey));

            _Request = req;
            _SecretKey = secretKey;
            _Logger = logger;

            byte[] signingKey = null;
            string hmacSha1Signature = null;
            string hmacSha256Signature = null;
            string logMessage = null;

            if (_Request.SignatureVersion == S3SignatureVersion.Version2)
            {
                #region V2

                hmacSha1Signature = Common.BytesToBase64(
                    Common.HmacSha1(Encoding.UTF8.GetBytes(_V2StringToSign), secretKey)
                    );

                hmacSha256Signature = Common.BytesToBase64(
                    Common.HmacSha256(Encoding.UTF8.GetBytes(_V2StringToSign), secretKey)
                    );

                logMessage =
                    _Header + "Signature V2 validation:" + Environment.NewLine + 
                    "  String to sign" + Environment.NewLine +
                    "  --------------" + Environment.NewLine +
                    _V2StringToSign + Environment.NewLine +
                    Environment.NewLine +
                    "  Client-supplied signature : " + _Request.Signature + Environment.NewLine +
                    "  Generated HMAC-SHA1       : " + hmacSha1Signature + Environment.NewLine +
                    "  Generated HMAC-SHA256     : " + hmacSha256Signature + Environment.NewLine +
                    "  Success                   : ";

                if (hmacSha256Signature.Equals(_Request.Signature) 
                    || hmacSha1Signature.Equals(_Request.Signature))
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
            else if (_Request.SignatureVersion == S3SignatureVersion.Version4)
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
                    "  Client-supplied signature : " + _Request.Signature + Environment.NewLine +
                    "  Generated HMAC-SHA1       : " + hmacSha1Signature + Environment.NewLine +
                    "  Generated HMAC-SHA256     : " + hmacSha256Signature + Environment.NewLine +
                    "  Success                   : ";
                 
                if (hmacSha256Signature.Equals(_Request.Signature) 
                    || hmacSha1Signature.Equals(_Request.Signature))
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
                throw new InvalidOperationException("Unable to validate signature unless signature version is V2 or V4.");
            } 
        }

        #region Members

        private static S3Request _Request = null;
        private static byte[] _SecretKey = null;
        private static string _Header = "[S3Signature] ";
        private static Action<string> _Logger = null;
        private static string _AmazonTimestampFormat = "yyyyMMddTHHmmssZ";
        private static string _AmazonDateFormat = "yyyyMMdd";

        #endregion

        #region Signature-V2-Members

        private static string _V2StringToSign
        {
            get
            {
                string ret = "";

                /*
                 * Per https://docs.aws.amazon.com/AmazonS3/latest/dev/RESTAuthentication.html
                 * and https://github.com/aws/aws-sdk-net/blob/master/sdk/src/Core/Amazon.Runtime/Internal/Auth/S3Signer.cs
                 */
                ret += _Request.Method.ToString().ToUpper() + "\n";
                ret += _Request.ContentMd5 + "\n";
                ret += _Request.ContentType + "\n";

                if (!String.IsNullOrEmpty(_Request.Expires)) ret += _Request.Expires + "\n";
                else ret += _Request.Date + "\n";

                ret += _V2CanonicalHeaders;
                ret += _V2CanonicalUri; 
                ret += _V2CanonicalQuerystring;

                return ret;
            }
        }
        private static string _V2CanonicalHeaders
        {
            get
            {
                Dictionary<string, string> headersAndQuery = new Dictionary<string, string>();
                if (_Request.Headers != null && _Request.Headers.Count > 0)
                {
                    foreach (KeyValuePair<string, string> header in _Request.Headers)
                    {
                        headersAndQuery.Add(header.Key.ToLower(), header.Value);
                    }
                }

                if (_Request.Querystring != null && _Request.Querystring.Count > 0)
                {
                    foreach (KeyValuePair<string, string> query in _Request.Querystring)
                    {
                        if (!headersAndQuery.ContainsKey(query.Key.ToLower())) 
                            headersAndQuery.Add(query.Key.ToLower(), query.Value);
                    }
                }

                string ret = "";

                if (headersAndQuery != null && headersAndQuery.Count > 0)
                {
                    headersAndQuery = headersAndQuery.OrderBy(o => o.Key, StringComparer.OrdinalIgnoreCase).ToDictionary(o => o.Key, o => o.Value);

                    foreach (KeyValuePair<string, string> header in headersAndQuery)
                    {
                        if (String.IsNullOrEmpty(header.Key)) continue;
                        string key = header.Key.ToLower().Trim();
                        if (!key.StartsWith("x-amz-")) continue;
                        else
                        {
                            if (key.Equals("x-amz-date") && !String.IsNullOrEmpty(_Request.Date)) continue;
                            string val = _Request.RetrieveHeaderValue(header.Key);
                            ret += key.ToLower() + ":" + val + "\n";
                        }
                    }
                }
                 
                return ret;
            }
        }
        private static List<string> _V2CanonicalQuerystringItems = new List<string>
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
        private static string _V2CanonicalUri
        {
            get
            {
                string ret = "/";

                if (_Request.RequestStyle == S3RequestStyle.BucketNotInHostname)
                {
                    if (!String.IsNullOrEmpty(_Request.Bucket))
                    {
                        ret += WebUtility.UrlEncode(_Request.Bucket);

                        if (!String.IsNullOrEmpty(_Request.Key))
                        {
                            ret += "/" + _Request.Key;
                        }
                        else
                        {
                            ret += "/";
                        }
                    }
                    else
                    {
                        // do nothing
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(_Request.Key))
                    {
                        ret += _Request.Key;
                    }
                }

                return ret;
            }
        }
        private static string _V2CanonicalQuerystring
        {
            get
            {
                string ret = "";

                foreach (string key in _V2CanonicalQuerystringItems)
                {
                    if (_Request.QuerystringExists(key, false)) ret = V2AppendCanonicalizedResource(ret, key);
                }

                return ret;
            }
        }

        #endregion

        #region Signature-V2-Methods

        private static string V2AppendCanonicalizedResource(string original, string appendKey)
        {
            if (String.IsNullOrEmpty(appendKey)) return original;

            string val = _Request.RetrieveHeaderValue(appendKey);
            if (!String.IsNullOrEmpty(val))
            {
                val = WebUtility.UrlDecode(val);
                val = WebUtility.UrlEncode(val);
            }

            appendKey = WebUtility.UrlEncode(appendKey);

            if (String.IsNullOrEmpty(original))
            {
                if (String.IsNullOrEmpty(val))
                {
                    return "?" + appendKey;
                }
                else
                {
                    return "?" + appendKey + "=" + val;
                }
            }
            else
            {
                if (String.IsNullOrEmpty(val))
                {
                    return "&" + appendKey;
                }
                else
                {
                    return "&" + appendKey + "=" + val;
                }
            }
        }

        #endregion

        #region Signature-V4-Members

        private static string _V4StringToSign
        {
            get
            {
                string ret = "";

                ret += "AWS4-HMAC-SHA256" + "\n";
                ret += _Request.TimestampUtc.ToString(_AmazonTimestampFormat) + "\n";
                ret += _Request.TimestampUtc.ToString(_AmazonDateFormat) + "/" + _Request.Region + "/s3/aws4_request" + "\n";
                ret += Common.BytesToHex(Common.Sha256(Encoding.UTF8.GetBytes(_V4CanonicalRequest))).ToLower();
                return ret;
            }
        }
        private static string _V4CanonicalRequest
        {
            get
            {
                string ret = "";

                ret += _Request.Method.ToString().ToUpper() + "\n";
                ret += _V4CanonicalUri + "\n";
                ret += _V4CanonicalQuerystring + "\n";

                // includes AWS and non-AWS headers
                ret += _V4CanonicalHeaders + "\n";
                ret += V4SignedHeadersToString() + "\n";

                if (!_Request.Chunked) ret += _Request.ContentSha256;
                else ret += "STREAMING-AWS4-HMAC-SHA256-PAYLOAD";

                return ret;
            }
        }
        private static string _V4CanonicalUri
        {
            get
            {
                string ret = "/";

                if (_Request.RequestStyle == S3RequestStyle.BucketNotInHostname)
                {
                    if (!String.IsNullOrEmpty(_Request.Bucket))
                    {
                        ret += WebUtility.UrlEncode(_Request.Bucket);

                        if (!String.IsNullOrEmpty(_Request.Key))
                        {
                            ret += "/" + _Request.Key;
                        }
                        else
                        {
                            ret += "/";
                        }
                    }
                }
                else if (_Request.RequestStyle == S3RequestStyle.BucketInHostname)
                {
                    if (!String.IsNullOrEmpty(_Request.Key))
                    {
                        ret += _Request.Key;
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

                foreach (KeyValuePair<string, string> query in _Request.Querystring)
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

                if (_Request.SignedHeaders != null && _Request.SignedHeaders.Count > 0)
                {
                    foreach (string key in _Request.SignedHeaders)
                    {
                        if (String.IsNullOrEmpty(key)) continue;
                        string val = _Request.RetrieveHeaderValue(key);
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

            foreach (string header in _Request.SignedHeaders)
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
                Encoding.UTF8.GetBytes(_Request.TimestampUtc.ToString(_AmazonDateFormat)),
                Encoding.UTF8.GetBytes("AWS4" + Encoding.UTF8.GetString(secretKey))
                );

            byte[] dateRegionKey = Common.HmacSha256(
                Encoding.UTF8.GetBytes(_Request.Region),
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
