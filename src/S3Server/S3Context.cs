using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WatsonWebserver;

namespace S3ServerLibrary
{
    /// <summary>
    /// S3 context.
    /// </summary>
    public class S3Context
    {
        #region Public-Members

        /// <summary>
        /// S3 request.
        /// </summary>
        public S3Request Request { get; private set; } = null;

        /// <summary>
        /// S3 response.
        /// </summary>
        public S3Response Response
        {
            get
            {
                return _Response;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Response));
                _Response = value;
            }
        }

        /// <summary>
        /// User metadata, supplied by your application.
        /// </summary>
        public object Metadata { get; set; } = null;

        /// <summary>
        /// HTTP context from which the S3 context was created.
        /// </summary>
        [JsonPropertyOrder(999)]
        public HttpContext Http { get; private set; } = null;

        #endregion

        #region Private-Members

        private S3Response _Response = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public S3Context()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="ctx">HTTP context.</param>
        /// <param name="baseDomains">List of base domains, if any.</param>
        /// <param name="metadata">User metadata, provided by your application.</param>
        /// <param name="logger">Method to invoke to send log messages.</param>
        public S3Context(HttpContext ctx, List<string> baseDomains = null, object metadata = null, Action<string> logger = null)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
             
            Metadata = metadata;
            Http = ctx;
            Request = new S3Request(this, baseDomains, logger);
            Response = new S3Response(this);
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods
         
        #endregion
    }
}
