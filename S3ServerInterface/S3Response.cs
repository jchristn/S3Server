using System;
using System.Collections.Generic;
using System.Text;

using WatsonWebserver;

namespace S3ServerInterface
{
    /// <summary>
    /// S3 response.
    /// </summary>
    public class S3Response
    {
        #region Public-Members

        /// <summary>
        /// Time of creation in UTC.
        /// </summary>
        public DateTime TimestampUtc { get; set; }

        /// <summary>
        /// The original request to which this response is being sent.
        /// </summary>
        public S3Request Request;
         
        /// <summary>
        /// The HTTP status code to return to the requestor (client).
        /// </summary>
        public int StatusCode;
          
        /// <summary>
        /// User-supplied headers to include in the response.
        /// </summary>
        public Dictionary<string, string> Headers;

        /// <summary>
        /// User-supplied content-type to include in the response.
        /// </summary>
        public string ContentType;

        /// <summary>
        /// The length of the supplied response data.  Value automatically set when setting Data.
        /// </summary>
        public long ContentLength
        {
            get
            {
                return _ContentLength;
            }
            private set
            {
                _ContentLength = value;
            }
        }

        /// <summary>
        /// The data to return to the requestor in the response body.  Setting this will also update the ContentLength field.
        /// </summary>
        public byte[] Data
        {
            get
            {
                return _Data;
            }
            set
            {
                if (value != null && value.Length > 0)
                {
                    _Data = new byte[value.Length];
                    Buffer.BlockCopy(value, 0, _Data, 0, value.Length);
                    _ContentLength = value.Length;
                }
            }
        }

        #endregion

        #region Private-Members

        private byte[] _Data = null;
        private long _ContentLength = 0;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public S3Response()
        {
            TimestampUtc = DateTime.Now.ToUniversalTime();
            Headers = new Dictionary<string, string>();
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="contentType">Content-type.</param>
        /// <param name="headers">HTTP headers.</param>
        /// <param name="data">Data.</param>
        public S3Response(S3Request s3request, int statusCode, string contentType, Dictionary<string, string> headers, byte[] data)
        {
            if (s3request == null) throw new ArgumentNullException(nameof(s3request));

            TimestampUtc = DateTime.Now.ToUniversalTime();
            Request = s3request; 
            StatusCode = statusCode;
            ContentType = contentType;
            Headers = headers;

            if (data != null && data.Length > 0)
            {
                _Data = new byte[data.Length];
                Buffer.BlockCopy(data, 0, _Data, 0, data.Length);
                ContentLength = data.Length;
            }
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create an HttpResponse from the S3Response.
        /// </summary>
        /// <returns></returns>
        public HttpResponse ToHttpResponse()
        {
            HttpResponse resp = new HttpResponse(
                Request.Http,
                StatusCode,
                Headers,
                ContentType,
                Data);

            return resp;
        }

        /// <summary>
        /// Returns a human-readable string with the object details.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            string ret = "---" + Environment.NewLine;
            ret += "  Status Code    : " + StatusCode + Environment.NewLine;
            ret += "  Content Type   : " + ContentType + Environment.NewLine;
            ret += "  Content Length : " + ContentLength + Environment.NewLine;
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

        #endregion
    }
}
