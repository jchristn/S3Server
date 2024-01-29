namespace S3ServerLibrary
{
    using S3ServerLibrary.S3Objects;
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using WatsonWebserver.Core;

    /// <summary>
    /// S3 response.
    /// </summary>
    public class S3Response
    {
        #region Public-Members

        /// <summary>
        /// The HTTP status code to return to the requestor (client).
        /// </summary>
        public int StatusCode
        {
            get
            {
                return _HttpResponse.StatusCode;
            }
            set
            {
                _HttpResponse.StatusCode = value;
            }
        }

        /// <summary>
        /// User-supplied headers to include in the response.
        /// </summary>
        public NameValueCollection Headers
        {
            get
            {
                return _HttpResponse.Headers;
            }
            set
            {
                if (value == null) _HttpResponse.Headers = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
                else _HttpResponse.Headers = value;
            }
        }

        /// <summary>
        /// User-supplied content-type to include in the response.
        /// </summary>
        public string ContentType
        {
            get
            {
                return _HttpResponse.ContentType;
            }
            set
            {
                _HttpResponse.ContentType = value;
            }
        }

        /// <summary>
        /// The length of the data in the response stream.  This value must be set before assigning the stream.
        /// </summary>
        public long ContentLength
        {
            get
            {
                return _HttpResponse.ContentLength;
            }
            set
            {
                if (value < 0) throw new ArgumentException("Content length must be zero or greater.");
                _HttpResponse.ContentLength = value;
            }
        }

        /// <summary>
        /// Enable or disable chunked transfer-encoding.
        /// By default this parameter is set to the value of Chunked in the S3Request object.
        /// If Chunked is false, use Send() APIs.
        /// If Chunked is true, use SendChunk() or SendFinalChunk() APIs.
        /// The Send(ErrorCode) API is valid for both conditions.
        /// </summary>
        public bool ChunkedTransfer
        {
            get
            {
                return _HttpResponse.ChunkedTransfer;
            }
            set
            {
                _HttpResponse.ChunkedTransfer = value;
            }
        }

        /// <summary>
        /// The data to return to the requestor.  Set ContentLength before assigning the stream.
        /// </summary>
        [JsonIgnore]
        public Stream Data
        {
            get
            {
                return _HttpResponse.Data;
            }
        }

        /// <summary>
        /// Data stream as a string.  Fully reads the data stream.
        /// </summary>
        [JsonIgnore]
        public string DataAsString
        {
            get
            {
                return _HttpResponse.DataAsString;
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
                return _HttpResponse.DataAsBytes;
            }
        }

        #endregion

        #region Private-Members

        private HttpResponseBase _HttpResponse = null;
        private S3Request _S3Request = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
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

            _HttpResponse = ctx.Http.Response;
            _S3Request = ctx.Request;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Send the response with no data to the requestor and close the connection.
        /// </summary>
        /// <returns>True if successful.</returns>
        public async Task<bool> Send()
        {
            if (ChunkedTransfer) throw new IOException("Responses with chunked transfer-encoding enabled require use of SendChunk() and SendFinalChunk().");

            if (ContentLength > 0) _HttpResponse.ContentLength = ContentLength;

            SetResponseHeaders();

            return await _HttpResponse.Send().ConfigureAwait(false);
        }

        /// <summary>
        /// Send the response with the supplied data to the requestor and close the connection.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> Send(string data)
        {
            if (ChunkedTransfer) throw new IOException("Responses with chunked transfer-encoding enabled require use of SendChunk() and SendFinalChunk().");

            byte[] bytes = Array.Empty<byte>();
            if (!String.IsNullOrEmpty(data))
            {
                bytes = Encoding.UTF8.GetBytes(data);
                ContentLength = bytes.Length;
            }
            else
            {
                ContentLength = 0;
            }

            SetResponseHeaders();

            return await _HttpResponse.Send(bytes).ConfigureAwait(false);
        }

        /// <summary>
        /// Send the response with the supplied data to the requestor and close the connection.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> Send(byte[] data)
        {
            if (ChunkedTransfer) throw new IOException("Responses with chunked transfer-encoding enabled require use of SendChunk() and SendFinalChunk().");

            MemoryStream ms = null;
            ContentLength = 0;

            if (data != null && data.Length > 0)
            {
                ms = new MemoryStream();
                ms.Write(data, 0, data.Length);
                ContentLength = data.Length;
            }
            else
            {
                ms = new MemoryStream(Array.Empty<byte>());
            }

            ms.Seek(0, SeekOrigin.Begin);

            SetResponseHeaders();

            return await _HttpResponse.Send(ContentLength, ms).ConfigureAwait(false);
        }

        /// <summary>
        /// Send the response with the supplied stream to the requestor and close the connection.
        /// </summary>
        /// <param name="contentLength">Content length.</param>
        /// <param name="stream">Stream containing data.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> Send(long contentLength, Stream stream)
        {
            if (ChunkedTransfer) throw new IOException("Responses with chunked transfer-encoding enabled require use of SendChunk() and SendFinalChunk().");

            ContentLength = contentLength;

            SetResponseHeaders();

            if (stream != null && ContentLength > 0)
            {
                return await _HttpResponse.Send(ContentLength, stream).ConfigureAwait(false);
            }
            else
            {
                return await _HttpResponse.Send().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Send an error response to the requestor and close the connection.
        /// </summary>
        /// <param name="error">Error.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> Send(Error error)
        {
            ChunkedTransfer = false;

            byte[] bytes = Encoding.UTF8.GetBytes(SerializationHelper.SerializeXml(error));
            MemoryStream ms = new MemoryStream(bytes);
            ms.Seek(0, SeekOrigin.Begin);

            ContentLength = bytes.Length;
            StatusCode = error.HttpStatusCode;
            ContentType = Constants.ContentTypeXml;

            SetResponseHeaders();

            return await _HttpResponse.Send(ContentLength, ms).ConfigureAwait(false);
        }

        /// <summary>
        /// Send an error response to the requestor and close the connection.
        /// </summary>
        /// <param name="error">ErrorCode.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> Send(ErrorCode error)
        {
            ChunkedTransfer = false;

            Error errorBody = new Error(error);

            byte[] bytes = Encoding.UTF8.GetBytes(SerializationHelper.SerializeXml(errorBody));
            MemoryStream ms = new MemoryStream(bytes);
            ms.Seek(0, SeekOrigin.Begin);

            ContentLength = bytes.Length;
            StatusCode = errorBody.HttpStatusCode;
            ContentType = Constants.ContentTypeXml;

            SetResponseHeaders();

            return await _HttpResponse.Send(ContentLength, ms).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a chunk of data using chunked transfer-encoding to the requestor.
        /// </summary>
        /// <param name="data">Chunk of data.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> SendChunk(byte[] data)
        {
            if (!ChunkedTransfer) throw new IOException("Responses with chunked transfer-encoding disabled require use of Send().");

            SetResponseHeaders();

            if (data == null) data = Array.Empty<byte>();
            return await _HttpResponse.SendChunk(data).ConfigureAwait(false);
        }

        /// <summary>
        /// Send the final chunk of data using chunked transfer-encoding to the requestor and close the connection.
        /// </summary>
        /// <param name="data">Final chunk of data.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> SendFinalChunk(byte[] data)
        {
            if (!ChunkedTransfer) throw new IOException("Responses with chunked transfer-encoding disabled require use of Send().");

            SetResponseHeaders();

            if (data == null) data = Array.Empty<byte>();
            return await _HttpResponse.SendFinalChunk(data).ConfigureAwait(false);
        }

        #endregion

        #region Private-Methods

        private void SetResponseHeaders()
        {
            if (Headers == null) Headers = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);

            if (Headers.Get("X-Amz-Date") == null)
                Headers.Add("X-Amz-Date", DateTime.UtcNow.ToString(Constants.AmazonTimestampFormatVerbose, CultureInfo.InvariantCulture));

            if (Headers.Get("Host") == null)
                Headers.Add("Host", _S3Request.Hostname);

            if (Headers.Get("Server") == null)
                Headers.Add("Server", "S3Server");

            if (Headers.Get("Date") != null) Headers.Remove("Date");

            Headers.Add("Date", DateTime.UtcNow.ToString(Constants.AmazonTimestampFormatVerbose, CultureInfo.InvariantCulture));
        }

        #endregion
    }
}
