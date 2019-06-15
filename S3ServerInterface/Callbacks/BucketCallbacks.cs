using System;
using System.Collections.Generic;
using System.Text;

namespace S3ServerInterface
{
    public class BucketCallbacks
    {
        #region Public-Members

        /// <summary>
        /// Check for the existence of a bucket.  A successful S3Response should include no data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> Exists = null;

        /// <summary>
        /// Write a bucket.  A successful S3Response should include the data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> Write = null;

        /// <summary>
        /// Write tags to a bucket.  A successful S3Response should include no data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> WriteTags = null;

        /// <summary>
        /// Enumerate a bucket.  A successful S3Response should include the data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> Read = null;

        /// <summary>
        /// Read a bucket's tags.  A successful S3Response should include the data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> ReadTags = null;

        /// <summary>
        /// Delete a bucket.  A successful S3Response should include no data and return an HTTP 204 status.
        /// </summary>
        public Func<S3Request, S3Response> Delete = null;

        /// <summary>
        /// Delete a bucket's tags.  A successful S3Response should include no data and return an HTTP 204.
        /// </summary>
        public Func<S3Request, S3Response> DeleteTags = null;

        /// <summary>
        /// Set a bucket's versioning policy.  A successful S3Response should include no data and return an HTTP 204.
        /// </summary>
        public Func<S3Request, S3Response> SetVersioning = null;

        /// <summary>
        /// Get a bucket's versioning policy.  A successful S3Response should include no data and return an HTTP 204.
        /// </summary>
        public Func<S3Request, S3Response> GetVersioning = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public BucketCallbacks()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
