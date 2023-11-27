namespace S3ServerLibrary
{
    using S3ServerLibrary.S3Objects;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Callback methods for bucket operations.
    /// </summary>
    public class BucketCallbacks
    {
        #region Public-Members

        /// <summary>
        /// Delete a bucket.
        /// </summary>
        public Func<S3Context, Task> Delete { get; set; } = null;

        /// <summary>
        /// Delete a bucket's ACL.
        /// </summary>
        public Func<S3Context, Task> DeleteAcl { get; set; } = null;

        /// <summary>
        /// Delete a bucket's tags.
        /// </summary>
        public Func<S3Context, Task> DeleteTagging { get; set; } = null;

        /// <summary>
        /// Delete a bucket's website configuration.
        /// </summary>
        public Func<S3Context, Task> DeleteWebsite { get; set; } = null;

        /// <summary>
        /// Check for the existence of a bucket. 
        /// Return true if it exists, false if it doesn't.
        /// </summary>
        public Func<S3Context, Task<bool>> Exists { get; set; } = null;

        /// <summary>
        /// Enumerate a bucket.
        /// </summary>
        public Func<S3Context, Task<ListBucketResult>> Read { get; set; } = null;

        /// <summary>
        /// Read a bucket's access control policy.
        /// </summary>
        public Func<S3Context, Task<AccessControlPolicy>> ReadAcl { get; set; } = null;

        /// <summary>
        /// Retrieve logging configuration for this bucket.
        /// </summary>
        public Func<S3Context, Task<BucketLoggingStatus>> ReadLogging { get; set; } = null;

        /// <summary>
        /// Retrieve location (region) constraint from the server for this bucket.
        /// </summary>
        public Func<S3Context, Task<LocationConstraint>> ReadLocation { get; set; } = null;

        /// <summary>
        /// Retrieve multipart uploads.
        /// </summary>
        public Func<S3Context, Task<ListMultipartUploadsResult>> ReadMultipartUploads { get; set; } = null;

        /// <summary>
        /// Read a bucket's tags.
        /// </summary>
        public Func<S3Context, Task<Tagging>> ReadTagging { get; set; } = null;

        /// <summary>
        /// Get a list of object versions in the bucket.
        /// </summary>
        public Func<S3Context, Task<ListVersionsResult>> ReadVersions { get; set; } = null;

        /// <summary>
        /// Get a bucket's versioning policy. 
        /// </summary>
        public Func<S3Context, Task<VersioningConfiguration>> ReadVersioning { get; set; } = null;

        /// <summary>
        /// Get a bucket's website configuration.
        /// </summary>
        public Func<S3Context, Task<WebsiteConfiguration>> ReadWebsite { get; set; } = null;

        /// <summary>
        /// Write a bucket.
        /// </summary>
        public Func<S3Context, Task> Write { get; set; } = null;

        /// <summary>
        /// Write an ACL to a bucket, deleting the previous ACL.
        /// </summary>
        public Func<S3Context, AccessControlPolicy, Task> WriteAcl { get; set; } = null;

        /// <summary>
        /// Write logging configuration to a bucket, deleting the previous configuration.
        /// </summary>
        public Func<S3Context, BucketLoggingStatus, Task> WriteLogging { get; set; } = null;

        /// <summary>
        /// Write tags to a bucket, deleting the previous tags.
        /// </summary>
        public Func<S3Context, Tagging, Task> WriteTagging { get; set; } = null;

        /// <summary>
        /// Set a bucket's versioning policy.
        /// </summary>
        public Func<S3Context, VersioningConfiguration, Task> WriteVersioning { get; set; } = null;

        /// <summary>
        /// Set a bucket's website configuration.
        /// </summary>
        public Func<S3Context, WebsiteConfiguration, Task> WriteWebsite { get; set; } = null;

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
