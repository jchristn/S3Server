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
        /// Retrieve location (region) constraint from the server for this bucket.
        /// Success: return an S3Response object with status 200 and LocationConstraint in the body.
        /// </summary>
        public Func<S3Request, S3Response, Task> ReadLocation = null;

        /// <summary>
        /// Check for the existence of a bucket.
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response, Task> Exists = null;

        /// <summary>
        /// Write a bucket.  
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response, Task> Write = null;

        /// <summary>
        /// Write an ACL to a bucket, deleting the previous ACL.
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response, Task> WriteAcl = null;

        /// <summary>
        /// Write tags to a bucket, deleting the previous tags.
        /// Success: return an S3Response object with status 204.
        /// </summary>
        public Func<S3Request, S3Response, Task> WriteTags = null;

        /// <summary>
        /// Set a bucket's versioning policy.  
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response, Task> WriteVersioning = null;
         
        /// <summary>
        /// Enumerate a bucket.  
        /// Success: return an S3Response object with status 200 and a ListBucketResult object as its data.
        /// </summary>
        public Func<S3Request, S3Response, Task> Read = null;

        /// <summary>
        /// Read a bucket's access control policy.
        /// Success: return an S3Response object with status 200 and an AccessControlPolicy object as its data.
        /// </summary>
        public Func<S3Request, S3Response, Task> ReadAcl = null;

        /// <summary>
        /// <summary>
        /// Read a bucket's tags. 
        /// Success: return an S3Response object with status 200 and a Tagging object as its data. 
        /// </summary>
        public Func<S3Request, S3Response, Task> ReadTags = null;

        /// <summary>
        /// Get a list of object versions in the bucket. 
        /// Success: return an S3Response object with status 200 and a ListVersionsResult object as its data.
        /// </summary>
        public Func<S3Request, S3Response, Task> ReadVersions = null;

        /// <summary>
        /// Get a bucket's versioning policy.  
        /// Success: return an S3Response object with status 200 and a VersioningConfiguration object as its data.
        /// </summary>
        public Func<S3Request, S3Response, Task> ReadVersioning = null;

        /// Delete a bucket.  
        /// Success: return an S3Response object with status 204. 
        /// </summary>
        public Func<S3Request, S3Response, Task> Delete = null;

        /// <summary>
        /// Delete a bucket's tags. 
        /// Success: return an S3Response object with status 204. 
        /// </summary>
        public Func<S3Request, S3Response, Task> DeleteTags = null;

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
