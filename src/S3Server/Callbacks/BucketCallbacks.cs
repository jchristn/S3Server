using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using S3ServerLibrary.S3Objects;
 
namespace S3ServerLibrary
{
    /// <summary>
    /// Callback methods for bucket operations.
    /// </summary>
    public class BucketCallbacks
    {
        #region Public-Members

        /// <summary>
        /// Delete a bucket.
        /// </summary>
        public Func<S3Context, Task> Delete = null;

        /// <summary>
        /// Delete a bucket's tags.
        /// </summary>
        public Func<S3Context, Task> DeleteTagging = null;

        /// <summary>
        /// Delete a bucket's website configuration.
        /// </summary>
        public Func<S3Context, Task> DeleteWebsite = null;

        /// <summary>
        /// Check for the existence of a bucket. 
        /// Return true if it exists, false if it doesn't.
        /// </summary>
        public Func<S3Context, Task<bool>> Exists = null;

        /// <summary>
        /// Enumerate a bucket.
        /// </summary>
        public Func<S3Context, Task<ListBucketResult>> Read = null;

        /// <summary>
        /// Read a bucket's access control policy.
        /// </summary>
        public Func<S3Context, Task<AccessControlPolicy>> ReadAcl = null;

        /// <summary>
        /// Retrieve logging configuration for this bucket.
        /// </summary>
        public Func<S3Context, Task<BucketLoggingStatus>> ReadLogging = null;

        /// <summary>
        /// Retrieve location (region) constraint from the server for this bucket.
        /// </summary>
        public Func<S3Context, Task<LocationConstraint>> ReadLocation = null;

        /// <summary>
        /// Read a bucket's tags.
        /// </summary>
        public Func<S3Context, Task<Tagging>> ReadTagging = null;

        /// <summary>
        /// Get a list of object versions in the bucket.
        /// </summary>
        public Func<S3Context, Task<ListVersionsResult>> ReadVersions = null;

        /// <summary>
        /// Get a bucket's versioning policy. 
        /// </summary>
        public Func<S3Context, Task<VersioningConfiguration>> ReadVersioning = null;

        /// <summary>
        /// Get a bucket's website configuration.
        /// </summary>
        public Func<S3Context, Task<WebsiteConfiguration>> ReadWebsite = null;

        /// <summary>
        /// Write a bucket.
        /// </summary>
        public Func<S3Context, Task> Write = null;

        /// <summary>
        /// Write an ACL to a bucket, deleting the previous ACL.
        /// </summary>
        public Func<S3Context, AccessControlPolicy, Task> WriteAcl = null;

        /// <summary>
        /// Write logging configuration to a bucket, deleting the previous configuration.
        /// </summary>
        public Func<S3Context, BucketLoggingStatus, Task> WriteLogging = null;

        /// <summary>
        /// Write tags to a bucket, deleting the previous tags.
        /// </summary>
        public Func<S3Context, Tagging, Task> WriteTagging = null;

        /// <summary>
        /// Set a bucket's versioning policy.
        /// </summary>
        public Func<S3Context, VersioningConfiguration, Task> WriteVersioning = null;

        /// <summary>
        /// Set a bucket's website configuration.
        /// </summary>
        public Func<S3Context, WebsiteConfiguration, Task> WriteWebsite = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
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
