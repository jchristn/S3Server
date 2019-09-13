using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        /// The HTTP status code to return to the requestor (client).
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// User-supplied headers to include in the response.
        /// </summary>
        public Dictionary<string, string> Headers = new Dictionary<string, string>();

        /// <summary>
        /// User-supplied content-type to include in the response.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The length of the data in the response stream.  This value must be set before assigning the stream.
        /// </summary>
        public long ContentLength { get; set; }
         
        /// <summary>
        /// The data to return to the requestor.  Set ContentLength before assigning the stream.
        /// </summary>
        public Stream Data { get; set; }

        /// <summary>
        /// Enable or disable chunked transfer-encoding.
        /// By default this parameter is set to the value of Chunked in the S3Request object.
        /// If Chunked is false, use Send() APIs.
        /// If Chunked is true, use SendChunk() or SendFinalChunk() APIs.
        /// The Send(ErrorCode) API is valid for both conditions.
        /// </summary>
        public bool Chunked { get; set; }

        #endregion

        #region Private-Members
         
        private S3Request _S3Request = null;

        #endregion

        #region Constructors-and-Factories
        
        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="s3request">S3Request.</param>
        public S3Response(S3Request s3request)
        {
            if (s3request == null) throw new ArgumentNullException(nameof(s3request));

            _S3Request = s3request;

            StatusCode = 200;
            ContentLength = 0;
            Chunked = s3request.Chunked;

            Headers = new Dictionary<string, string>();
        }

        /// <summary>
        /// Instantiate the object without supplying a stream.  Useful for HEAD responses.
        /// </summary>
        /// <param name="s3request">S3Request.</param>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="contentType">Content-type.</param>
        /// <param name="headers">HTTP headers.</param> 
        /// <param name="contentLength">Content length.</param>
        public S3Response(S3Request s3request, int statusCode, string contentType, Dictionary<string, string> headers, long contentLength)
        {
            if (s3request == null) throw new ArgumentNullException(nameof(s3request));

            _S3Request = s3request;

            StatusCode = 200;
            ContentLength = 0;

            StatusCode = statusCode;
            ContentType = contentType;
            ContentLength = contentLength;
            Chunked = s3request.Chunked;
            Headers = new Dictionary<string, string>();
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="s3request">S3Request.</param>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="contentType">Content-type.</param>
        /// <param name="headers">HTTP headers.</param>
        /// <param name="contentLength">Content length.</param>
        /// <param name="stream">Stream containing response data.</param>
        public S3Response(S3Request s3request, int statusCode, string contentType, Dictionary<string, string> headers, long contentLength, Stream stream)
        {
            if (s3request == null) throw new ArgumentNullException(nameof(s3request));

            _S3Request = s3request;

            StatusCode = statusCode;
            ContentType = contentType;
            ContentLength = contentLength;
            Chunked = s3request.Chunked;
            Headers = new Dictionary<string, string>();
            Data = stream;
        }
          
        #endregion

        #region Public-Methods

        /// <summary>
        /// Send the response to the requestor and close the connection.
        /// </summary>
        /// <returns>True if successful.</returns>
        public async Task<bool> Send()
        {
            if (Chunked) throw new IOException("Responses with chunked transfer-encoding enabled require use of SendChunk() and SendFinalChunk().");

            _S3Request.Http.Response.ChunkedTransfer = false;

            SetResponseHeaders();

            if (Data != null && ContentLength > 0)
            {
                return await _S3Request.Http.Response.Send(ContentLength, Data);
            }
            else
            {
                return await _S3Request.Http.Response.Send();
            } 
        }

        /// <summary>
        /// Send the response with the supplied data to the requestor and close the connection.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> Send(string data)
        {
            if (Chunked) throw new IOException("Responses with chunked transfer-encoding enabled require use of SendChunk() and SendFinalChunk()."); 
            _S3Request.Http.Response.ChunkedTransfer = false; 
            byte[] bytes = null;
            if (!String.IsNullOrEmpty(data)) bytes = Encoding.UTF8.GetBytes(data); 
            return await Send(bytes); 
        }

        /// <summary>
        /// Send the response with the supplied data to the requestor and close the connection.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> Send(byte[] data)
        {
            if (Chunked) throw new IOException("Responses with chunked transfer-encoding enabled require use of SendChunk() and SendFinalChunk().");

            _S3Request.Http.Response.ChunkedTransfer = false;

            MemoryStream ms = null;
            long contentLength = 0;

            if (data != null && data.Length > 0)
            {
                ms = new MemoryStream();
                ms.Write(data, 0, data.Length);
                contentLength = data.Length;
            }
            else
            {
                ms = new MemoryStream(new byte[0]);
            }

            ms.Seek(0, SeekOrigin.Begin); 
            return await Send(contentLength, ms);
        }

        /// <summary>
        /// Send the response with the supplied stream to the requestor and close the connection.
        /// </summary>
        /// <param name="contentLength">Content length.</param>
        /// <param name="stream">Stream containing data.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> Send(long contentLength, Stream stream)
        {
            if (Chunked) throw new IOException("Responses with chunked transfer-encoding enabled require use of SendChunk() and SendFinalChunk()."); 
            _S3Request.Http.Response.ChunkedTransfer = false; 
            ContentLength = contentLength;
            Data = stream;
            return await Send();
        }

        /// <summary>
        /// Send an error response to the requestor and close the connection.
        /// </summary>
        /// <param name="error">ErrorCode.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> Send(ErrorCode error)
        {
            Chunked = false;
            _S3Request.Http.Response.ChunkedTransfer = false; 

            Error errorBody = new Error(error);

            byte[] data = Encoding.UTF8.GetBytes(Common.SerializeXml(errorBody));
            Data = new MemoryStream(data);
            Data.Seek(0, SeekOrigin.Begin);

            ContentLength = data.Length;
            StatusCode = errorBody.HttpStatusCode;
            ContentType = "application/xml";
            Headers = new Dictionary<string, string>();

            return await Send();
        }
           
        /// <summary>
        /// Send a chunk of data using chunked transfer-encoding to the requestor.
        /// </summary>
        /// <param name="data">Chunk of data.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> SendChunk(byte[] data)
        {
            if (!Chunked) throw new IOException("Responses with chunked transfer-encoding disabled require use of Send().");

            _S3Request.Http.Response.ChunkedTransfer = true;

            SetResponseHeaders(); 
            return await _S3Request.Http.Response.SendChunk(data);
        }

        /// <summary>
        /// Send the final chunk of data using chunked transfer-encoding to the requestor and close the connection.
        /// </summary>
        /// <param name="data">Final chunk of data.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> SendFinalChunk(byte[] data)
        {
            if (!Chunked) throw new IOException("Responses with chunked transfer-encoding disabled require use of Send().");

            _S3Request.Http.Response.ChunkedTransfer = true;

            SetResponseHeaders();
            return await _S3Request.Http.Response.SendFinalChunk(data); 
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

        #endregion

        #region Private-Methods

        private void SetResponseHeaders()
        {
            _S3Request.Http.Response.StatusCode = StatusCode;
            _S3Request.Http.Response.ContentType = ContentType;
            _S3Request.Http.Response.ContentLength = ContentLength;

            if (Headers != null)
            {
                if (!Headers.ContainsKey("X-Amz-Date"))
                {
                    Headers.Add("X-Amz-Date", DateTime.Now.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
                }
                if (!Headers.ContainsKey("Host"))
                {
                    Headers.Add("Host", _S3Request.Hostname);
                }
                if (!Headers.ContainsKey("Server"))
                {
                    Headers.Add("Server", "Less3");
                }
            }
            else
            {
                Headers = new Dictionary<string, string>();
                Headers.Add("X-Amz-Date", DateTime.Now.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
                Headers.Add("Host", _S3Request.Hostname);
                Headers.Add("Server", "Less3");
            }

            _S3Request.Http.Response.Headers = Headers;
        }

        #endregion
    }
}
