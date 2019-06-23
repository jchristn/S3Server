﻿using System;
using System.Collections.Generic;
using System.Text;

using S3ServerInterface.S3Objects;

namespace S3ServerInterface
{
    public class BucketCallbacks
    {
        #region Public-Members

        /// <summary>
        /// Check for the existence of a bucket.
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response> Exists = null;

        /// <summary>
        /// Write a bucket.  
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, CreateBucketConfiguration, S3Response> Write = null;

        /// <summary>
        /// Write tags to a bucket.  
        /// Success: return an S3Response object with status 204.
        /// </summary>
        public Func<S3Request, Tagging, S3Response> WriteTags = null;

        /// <summary>
        /// Set a bucket's versioning policy.  
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, VersioningConfiguration, S3Response> WriteVersioning = null;

        /// <summary>
        /// Set a bucket's access control policy.
        /// Sucess: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, AccessControlPolicy, S3Response> WriteAcl = null;

        /// <summary>
        /// Enumerate a bucket.  
        /// Success: return an S3Response object with status 200 and a ListBucketResult object as its data.
        /// </summary>
        public Func<S3Request, S3Response> Read = null;

        /// <summary>
        /// Read a bucket's tags. 
        /// Success: return an S3Response object with status 200 and a Tagging object as its data. 
        /// </summary>
        public Func<S3Request, S3Response> ReadTags = null;

        /// <summary>
        /// Get a bucket's versioning policy.  
        /// Success: return an S3Response object with status 200 and a VersioningConfiguration object as its data.
        /// </summary>
        public Func<S3Request, S3Response> ReadVersioning = null;

        /// <summary>
        /// Read a bucket's access control policy.
        /// Success: return an S3Response object with status 200 and an AccessControlPolicy object as its data.
        /// </summary>
        public Func<S3Request, S3Response> ReadAcl = null;

        /// <summary>
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
