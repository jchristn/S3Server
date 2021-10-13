using System;
using System.Collections.Generic;
using System.Text;
using S3ServerLibrary.S3Objects;

namespace S3ServerLibrary
{
    /// <summary>
    /// S3 exception.  Throw an instance of this object if an S3 error response needs to be returned to the caller.
    /// </summary>
    public class S3Exception : Exception
    {
        #region Public-Members

        /// <summary>
        /// Error.
        /// </summary>
        public Error Error { get; set; } = null;

        /// <summary>
        /// Inner exception.
        /// </summary>
        public new Exception InnerException { get; set; } = null;

        /// <summary>
        /// JSON representation of the inner exception.
        /// </summary>
        public string InnerExceptionJson
        {
            get
            {
                if (InnerException != null) return Common.SerializeJson(InnerException, true);
                return null;
            }
        }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="error">Error.</param>
        /// <param name="inner">Inner exception.</param>
        public S3Exception(Error error = null, Exception inner = null)
        {
            Error = error;
            InnerException = inner;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
