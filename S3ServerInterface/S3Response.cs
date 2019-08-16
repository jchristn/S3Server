using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using WatsonWebserver;

using S3ServerInterface.S3Objects;

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
        public Dictionary<string, string> Headers = new Dictionary<string, string>();

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
                    _DataStream = null;
                    _UseStream = false;
                }
            }
        }

        public Stream DataStream
        {
            get
            {
                return _DataStream;
            }
            set
            {
                if (value != null)
                {
                    _DataStream = value;
                    _UseStream = true;
                    _Data = null;
                }
            }
        }

        #endregion

        #region Private-Members

        private bool _UseStream = false;
        private byte[] _Data = null;
        private Stream _DataStream = null;
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
            Headers.Add("X-Amz-Date", DateTime.Now.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
            Headers.Add("Host", "localhost");
            Headers.Add("Server", "Less3");
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="s3request">S3Request.</param>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="contentType">Content-type.</param>
        /// <param name="headers">HTTP headers.</param>
        /// <param name="data">Data.</param>
        /// <param name="contentLength">ContentLength.</param>
        public S3Response(S3Request s3request, int statusCode, string contentType, Dictionary<string, string> headers, long contentLength)
        {
            if (s3request == null) throw new ArgumentNullException(nameof(s3request));

            TimestampUtc = DateTime.Now.ToUniversalTime();
            Request = s3request;
            StatusCode = statusCode;
            ContentType = contentType;
            ContentLength = contentLength;

            if (headers != null)
            {
                Headers = headers;
                if (!Headers.ContainsKey("X-Amz-Date"))
                {
                    Headers.Add("X-Amz-Date", DateTime.Now.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
                }
                if (!Headers.ContainsKey("Host"))
                {
                    Headers.Add("Host", s3request.Hostname);
                }
                if (!Headers.ContainsKey("Server"))
                {
                    Headers.Add("Server", "Less3");
                }
            }
            else
            {
                Headers.Add("X-Amz-Date", DateTime.Now.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
                Headers.Add("Host", s3request.Hostname);
                Headers.Add("Server", "Less3");
            }

            _UseStream = false;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="s3request">S3Request.</param>
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

            if (headers != null)
            {
                Headers = headers;
                if (!Headers.ContainsKey("X-Amz-Date"))
                {
                    Headers.Add("X-Amz-Date", DateTime.Now.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
                }
                if (!Headers.ContainsKey("Host"))
                {
                    Headers.Add("Host", s3request.Hostname);
                }
                if (!Headers.ContainsKey("Server"))
                {
                    Headers.Add("Server", "Less3");
                }
            }
            else
            {
                Headers.Add("X-Amz-Date", DateTime.Now.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
                Headers.Add("Host", s3request.Hostname);
                Headers.Add("Server", "Less3");
            }

            _UseStream = false;

            if (data != null && data.Length > 0)
            {
                _Data = new byte[data.Length];
                Buffer.BlockCopy(data, 0, _Data, 0, data.Length);
                ContentLength = data.Length;
            }
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="s3request">S3Request.</param>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="contentType">Content-type.</param>
        /// <param name="headers">HTTP headers.</param>
        /// <param name="contentLength">Content length of data in the stream.</param>
        /// <param name="stream">Stream containing data.</param>
        public S3Response(S3Request s3request, int statusCode, string contentType, Dictionary<string, string> headers, long contentLength, Stream stream)
        {
            if (s3request == null) throw new ArgumentNullException(nameof(s3request));

            TimestampUtc = DateTime.Now.ToUniversalTime();
            Request = s3request;
            StatusCode = statusCode;
            ContentType = contentType;

            if (headers != null)
            {
                Headers = headers;
                if (!Headers.ContainsKey("X-Amz-Date"))
                {
                    Headers.Add("X-Amz-Date", DateTime.Now.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
                }
                if (!Headers.ContainsKey("Host"))
                {
                    Headers.Add("Host", s3request.Hostname);
                }
                if (!Headers.ContainsKey("Server"))
                {
                    Headers.Add("Server", "Less3");
                }
            }
            else
            {
                Headers.Add("X-Amz-Date", DateTime.Now.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
                Headers.Add("Host", s3request.Hostname);
                Headers.Add("Server", "Less3");
            } 

            ContentLength = contentLength;
            DataStream = stream;
            _UseStream = true;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="s3request">S3Request.</param>
        /// <param name="error">ErrorCode.</param>
        public S3Response(S3Request s3request, ErrorCode error)
        {
            if (s3request == null) throw new ArgumentNullException(nameof(s3request));

            Error errorBody = new Error(error);

            TimestampUtc = DateTime.Now.ToUniversalTime();
            Request = s3request;
            StatusCode = errorBody.HttpStatusCode;
            ContentType = "application/xml";

            Data = Encoding.UTF8.GetBytes(Common.SerializeXml(errorBody));
            ContentLength = Data.Length;

            Headers.Add("X-Amz-Date", DateTime.Now.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
            Headers.Add("Host", s3request.Hostname);
            Headers.Add("Server", "Less3");

            _UseStream = false;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="s3request">S3Request.</param>
        /// <param name="error">Error.</param>
        public S3Response(S3Request s3request, Error error)
        {
            if (s3request == null) throw new ArgumentNullException(nameof(s3request));
            if (error == null) throw new ArgumentNullException(nameof(error));

            TimestampUtc = DateTime.Now.ToUniversalTime();
            Request = s3request;
            StatusCode = error.HttpStatusCode;
            ContentType = "application/xml";

            Data = Encoding.UTF8.GetBytes(Common.SerializeXml(error));
            ContentLength = Data.Length;

            Headers.Add("X-Amz-Date", DateTime.Now.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
            Headers.Add("Host", s3request.Hostname);
            Headers.Add("Server", "Less3");

            _UseStream = false;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create an HttpResponse from the S3Response.
        /// </summary>
        /// <returns></returns>
        public HttpResponse ToHttpResponse()
        { 
            if (_UseStream)
            {
                return new HttpResponse(
                    Request.Http,
                    StatusCode,
                    Headers,
                    ContentType,
                    ContentLength,
                    DataStream);
            }
            else
            {
                return new HttpResponse(
                    Request.Http,
                    StatusCode,
                    Headers,
                    ContentType,
                    Data);
            }
        }

        /// <summary>
        /// Create an HttpResponse from the S3Response for a HEAD response.
        /// </summary>
        /// <returns>HttpResponse.</returns>
        public HttpResponse ToHeadHttpResponse()
        { 
            return new HttpResponse(
                Request.Http,
                StatusCode,
                Headers, 
                ContentLength);
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
            if (_UseStream)
            {
                if (DataStream != null)
                {
                    ret += "(stream)" + Environment.NewLine;
                }
                else
                {
                    ret += "(none)" + Environment.NewLine;
                }
            }
            else
            {
                if (Data != null && Data.Length > 0)
                {
                    ret += Data.Length + " bytes" + Environment.NewLine;
                }
                else
                {
                    ret += "(none)" + Environment.NewLine;
                }
            }
             
            return ret;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
