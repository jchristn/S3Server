using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
 
namespace S3ServerInterface
{
    /// <summary>
    /// Callback methods for bucket operations.
    /// </summary>
    public class BucketCallbacks
    {
        #region Public-Members

        /// <summary>
        /// Delete a bucket.  
        /// Success: send an S3Response with status 204. 
        /// </summary>
        public Func<S3Context, Task> Delete = null;

        /// <summary>
        /// Delete a bucket's tags. 
        /// Success: send an S3Response with status 204. 
        /// </summary>
        public Func<S3Context, Task> DeleteTags = null;

        /// <summary>
        /// Delete a bucket's website configuration. 
        /// Success: send an S3Response with status 204. 
        /// </summary>
        public Func<S3Context, Task> DeleteWebsite = null;

        /// <summary>
        /// Check for the existence of a bucket.
        /// Success: send an S3Response with status 200.
        /// </summary>
        public Func<S3Context, Task> Exists = null;

        /// <summary>
        /// Enumerate a bucket.  
        /// Success: send an S3Response with status 200 and a ListBucketResult object as its data.
        /// </summary>
        public Func<S3Context, Task> Read = null;

        /// <summary>
        /// Read a bucket's access control policy.
        /// Success: send an S3Response with status 200 and an AccessControlPolicy object as its data.
        /// </summary>
        public Func<S3Context, Task> ReadAcl = null;

        /// <summary>
        /// Retrieve logging configuration for this bucket.
        /// Success: send an S3Response with status 200 and BucketLoggingStatus in the body.
        /// </summary>
        public Func<S3Context, Task> ReadLogging = null;

        /// <summary>
        /// Retrieve location (region) constraint from the server for this bucket.
        /// Success: send an S3Response with status 200 and LocationConstraint in the body.
        /// </summary>
        public Func<S3Context, Task> ReadLocation = null;

        /// <summary>
        /// Read a bucket's tags. 
        /// Success: send an S3Response with status 200 and a Tagging object as its data. 
        /// </summary>
        public Func<S3Context, Task> ReadTags = null;

        /// <summary>
        /// Get a list of object versions in the bucket. 
        /// Success: send an S3Response with status 200 and a ListVersionsResult object as its data.
        /// </summary>
        public Func<S3Context, Task> ReadVersions = null;

        /// <summary>
        /// Get a bucket's versioning policy.  
        /// Success: send an S3Response with status 200 and a VersioningConfiguration object as its data.
        /// </summary>
        public Func<S3Context, Task> ReadVersioning = null;

        /// <summary>
        /// Get a bucket's website configuration.  
        /// Success: send an S3Response with status 200 and a WebsiteConfiguration object as its data.
        /// </summary>
        public Func<S3Context, Task> ReadWebsite = null;

        /// <summary>
        /// Write a bucket.  
        /// Success: send an S3Response with status 200.
        /// </summary>
        public Func<S3Context, Task> Write = null;

        /// <summary>
        /// Write an ACL to a bucket, deleting the previous ACL.
        /// Success: send an S3Response with status 200.
        /// </summary>
        public Func<S3Context, Task> WriteAcl = null;

        /// <summary>
        /// Write logging configuration to a bucket, deleting the previous configuration.
        /// Success: send an S3Response with status 200.
        /// </summary>
        public Func<S3Context, Task> WriteLogging = null;

        /// <summary>
        /// Write tags to a bucket, deleting the previous tags.
        /// Success: send an S3Response with status 204.
        /// </summary>
        public Func<S3Context, Task> WriteTags = null;

        /// <summary>
        /// Set a bucket's versioning policy.  
        /// Success: send an S3Response with status 200.
        /// </summary>
        public Func<S3Context, Task> WriteVersioning = null;

        /// <summary>
        /// Set a bucket's website configuration.  
        /// Success: send an S3Response with status 200.
        /// </summary>
        public Func<S3Context, Task> WriteWebsite = null;

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
