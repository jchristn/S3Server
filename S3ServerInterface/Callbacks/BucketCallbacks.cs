using System;
using System.Collections.Generic;
using System.Text;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3ServerInterface
{
    public class BucketCallbacks
    {
        #region Public-Members

        /// <summary>
        /// Retrieve location (region) constraint from the server for this bucket.
        /// Success: return an S3Response object with status 200 and LocationConstraint in the body.
        /// </summary>
        public Func<S3Request, S3Response> ReadLocation = null;

        /// <summary>
        /// Check for the existence of a bucket.
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response> Exists = null;

        /// <summary>
        /// Write a bucket.  
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response> Write = null;

        /// <summary>
        /// Write an ACL to a bucket, deleting the previous ACL.
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response> WriteAcl = null;

        /// <summary>
        /// Write tags to a bucket, deleting the previous tags.
        /// Success: return an S3Response object with status 204.
        /// </summary>
        public Func<S3Request, S3Response> WriteTags = null;

        /// <summary>
        /// Set a bucket's versioning policy.  
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response> WriteVersioning = null;
         
        /// <summary>
        /// Enumerate a bucket.  
        /// Success: return an S3Response object with status 200 and a ListBucketResult object as its data.
        /// </summary>
        public Func<S3Request, S3Response> Read = null;

        /// <summary>
        /// Read a bucket's access control policy.
        /// Success: return an S3Response object with status 200 and an AccessControlPolicy object as its data.
        /// </summary>
        public Func<S3Request, S3Response> ReadAcl = null;

        /// <summary>
        /// <summary>
        /// Read a bucket's tags. 
        /// Success: return an S3Response object with status 200 and a Tagging object as its data. 
        /// </summary>
        public Func<S3Request, S3Response> ReadTags = null;

        /// <summary>
        /// Get a list of object versions in the bucket. 
        /// Success: return an S3Response object with status 200 and a ListVersionsResult object as its data.
        /// </summary>
        public Func<S3Request, S3Response> ReadVersions = null;

        /// <summary>
        /// Get a bucket's versioning policy.  
        /// Success: return an S3Response object with status 200 and a VersioningConfiguration object as its data.
        /// </summary>
        public Func<S3Request, S3Response> ReadVersioning = null;

        /// Delete a bucket.  
        /// Success: return an S3Response object with status 204. 
        /// </summary>
        public Func<S3Request, S3Response> Delete = null;

        /// <summary>
        /// Delete a bucket's tags. 
        /// Success: return an S3Response object with status 204. 
        /// </summary>
        public Func<S3Request, S3Response> DeleteTags = null;

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
