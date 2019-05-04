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
        public Func<S3Request, S3Response> Exists { get; set; }

        /// <summary>
        /// Write a bucket.  A successful S3Response should include the data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> Write { get; set; }

        /// <summary>
        /// Write tags to a bucket.  A successful S3Response should include no data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> WriteTags { get; set; }

        /// <summary>
        /// Enumerate a bucket.  A successful S3Response should include the data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> Read { get; set; }

        /// <summary>
        /// Read a bucket's tags.  A successful S3Response should include the data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> ReadTags { get; set; }

        /// <summary>
        /// Delete a bucket.  A successful S3Response should include no data and return an HTTP 204 status.
        /// </summary>
        public Func<S3Request, S3Response> Delete { get; set; }

        /// <summary>
        /// Delete a bucket's tags.  A successful S3Response should include no data and return an HTTP 204.
        /// </summary>
        public Func<S3Request, S3Response> DeleteTags { get; set; }

        /// <summary>
        /// Set a bucket's versioning policy.  A successful S3Response should include no data and return an HTTP 204.
        /// </summary>
        public Func<S3Request, S3Response> SetVersioning { get; set; }

        /// <summary>
        /// Get a bucket's versioning policy.  A successful S3Response should include no data and return an HTTP 204.
        /// </summary>
        public Func<S3Request, S3Response> GetVersioning { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public BucketCallbacks()
        {
            Write = null;
            Read = null;
            Delete = null;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
