using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using S3ServerInterface.S3Objects;
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
        /// The HTTP status code to return to the requestor (client).
        /// </summary>
        public int StatusCode { get; set; } = 200;

        /// <summary>
        /// User-supplied headers to include in the response.
        /// </summary>
        public Dictionary<string, string> Headers 
        { 
            get
            {
                return _Headers;
            }
            set
            {
                if (value == null) _Headers = new Dictionary<string, string>();
                else _Headers = value;
            }
        }

        /// <summary>
        /// User-supplied content-type to include in the response.
        /// </summary>
        public string ContentType { get; set; } = null;

        /// <summary>
        /// The length of the data in the response stream.  This value must be set before assigning the stream.
        /// </summary>
        public long ContentLength { get; set; } = 0;

        /// <summary>
        /// The data to return to the requestor.  Set ContentLength before assigning the stream.
        /// </summary>
        public Stream Data { get; set; } = null;

        /// <summary>
        /// Enable or disable chunked transfer-encoding.
        /// By default this parameter is set to the value of Chunked in the S3Request object.
        /// If Chunked is false, use Send() APIs.
        /// If Chunked is true, use SendChunk() or SendFinalChunk() APIs.
        /// The Send(ErrorCode) API is valid for both conditions.
        /// </summary>
        public bool Chunked { get; set; } = false;

        #endregion

        #region Private-Members

        private HttpContext _Http = null;
        private S3Request _Request = null;
        private Dictionary<string, string> _Headers = new Dictionary<string, string>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public S3Response()
        {

        }

        /// <summary>
        /// Instantiate the object without supplying a stream.  Useful for HEAD responses.
        /// </summary>
        /// <param name="ctx">S3 context.</param>
        public S3Response(S3Context ctx)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            _Http = ctx.Http;
            _Request = ctx.Request;
        }

        /// <summary>
        /// Instantiate the object without supplying a stream.  Useful for HEAD responses.
        /// </summary>
        /// <param name="ctx">S3 context.</param>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="contentType">Content-type.</param>
        /// <param name="headers">HTTP headers.</param> 
        /// <param name="contentLength">Content length.</param>
        public S3Response(S3Context ctx, int statusCode = 200, string contentType = "text/plain", Dictionary<string, string> headers = null, long contentLength = 0)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            _Http = ctx.Http;
            _Request = ctx.Request;

            StatusCode = statusCode;
            ContentType = contentType;
            ContentLength = contentLength;
            Chunked = _Request.Chunked;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="ctx">S3 context.</param>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="contentType">Content-type.</param>
        /// <param name="headers">HTTP headers.</param>
        /// <param name="contentLength">Content length.</param>
        /// <param name="stream">Stream containing response data.</param>
        public S3Response(S3Context ctx, int statusCode = 200, string contentType = "text/plain", Dictionary<string, string> headers = null, long contentLength = 0, Stream stream = null)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            _Http = ctx.Http;
            _Request = ctx.Request;

            StatusCode = statusCode;
            ContentType = contentType;
            ContentLength = contentLength;
            Chunked = _Request.Chunked;
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

            _Http.Response.ChunkedTransfer = false;

            SetResponseHeaders();

            if (Data != null && ContentLength > 0)
            {
                return await _Http.Response.Send(ContentLength, Data).ConfigureAwait(false);
            }
            else
            {
                return await _Http.Response.Send().ConfigureAwait(false);
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

            _Http.Response.ChunkedTransfer = false; 

            byte[] bytes = null;
            if (!String.IsNullOrEmpty(data)) bytes = Encoding.UTF8.GetBytes(data); 
            return await Send(bytes).ConfigureAwait(false); 
        }

        /// <summary>
        /// Send the response with the supplied data to the requestor and close the connection.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> Send(byte[] data)
        {
            if (Chunked) throw new IOException("Responses with chunked transfer-encoding enabled require use of SendChunk() and SendFinalChunk().");

            _Http.Response.ChunkedTransfer = false;

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
            return await Send(contentLength, ms).ConfigureAwait(false);
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

            _Http.Response.ChunkedTransfer = false; 

            ContentLength = contentLength;
            Data = stream;
            return await Send().ConfigureAwait(false);
        }

        /// <summary>
        /// Send an error response to the requestor and close the connection.
        /// </summary>
        /// <param name="error">ErrorCode.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> Send(ErrorCode error)
        {
            Chunked = false;

            _Http.Response.ChunkedTransfer = false; 

            Error errorBody = new Error(error);

            byte[] data = Encoding.UTF8.GetBytes(Common.SerializeXml(errorBody));
            Data = new MemoryStream(data);
            Data.Seek(0, SeekOrigin.Begin);

            ContentLength = data.Length;
            StatusCode = errorBody.HttpStatusCode;
            ContentType = "application/xml";
            Headers = new Dictionary<string, string>();

            return await Send().ConfigureAwait(false);
        }
           
        /// <summary>
        /// Send a chunk of data using chunked transfer-encoding to the requestor.
        /// </summary>
        /// <param name="data">Chunk of data.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> SendChunk(byte[] data)
        {
            if (!Chunked) throw new IOException("Responses with chunked transfer-encoding disabled require use of Send().");

            _Http.Response.ChunkedTransfer = true;

            SetResponseHeaders(); 

            return await _Http.Response.SendChunk(data).ConfigureAwait(false);
        }

        /// <summary>
        /// Send the final chunk of data using chunked transfer-encoding to the requestor and close the connection.
        /// </summary>
        /// <param name="data">Final chunk of data.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> SendFinalChunk(byte[] data)
        {
            if (!Chunked) throw new IOException("Responses with chunked transfer-encoding disabled require use of Send().");

            _Http.Response.ChunkedTransfer = true;

            SetResponseHeaders();

            return await _Http.Response.SendFinalChunk(data).ConfigureAwait(false); 
        }

        #endregion

        #region Private-Methods

        private void SetResponseHeaders()
        {
            _Http.Response.StatusCode = StatusCode;
            _Http.Response.ContentType = ContentType;
            _Http.Response.ContentLength = ContentLength;

            if (Headers != null)
            {
                if (!Headers.ContainsKey("X-Amz-Date"))
                {
                    Headers.Add("X-Amz-Date", DateTime.Now.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
                }
                if (!Headers.ContainsKey("Host"))
                {
                    Headers.Add("Host", _Request.Hostname);
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
                Headers.Add("Host", _Request.Hostname);
            }

            _Http.Response.Headers = Headers;
        }

        #endregion
    }
}
