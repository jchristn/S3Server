using System;
using System.Collections.Generic;
using System.Text;

namespace S3ServerInterface
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

        #endregion

        #region Private-Members

        private S3Response _Response = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public S3Context()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="req">S3 request.</param>
        /// <param name="resp">S3 response.</param>
        /// <param name="metadata">User metadata, provided by your application.</param>
        public S3Context(S3Request req, S3Response resp, object metadata = null)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));
            if (resp == null) throw new ArgumentNullException(nameof(resp));

            Request = req;
            Response = resp;
            Metadata = metadata;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
