using System;
using System.Collections.Generic;
using System.Text;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using S3ServerInterface;

namespace S3ServerInterface.Callbacks
{
    public class ObjectCallbacks
    {
        #region Public-Members

        /// <summary>
        /// Check for the existence of an object.  
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response> Exists = null;

        /// <summary>
        /// Write an object.
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response> Write = null;

        /// <summary>
        /// Write an object's access control list.  
        /// Success: return an S3Response object with status 200. 
        /// </summary>
        public Func<S3Request, S3Response> WriteAcl = null;

        /// <summary>
        /// Write tags to an object.
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response> WriteTags = null;

        /// <summary>
        /// Write a legal hold status to an object. 
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response> WriteLegalHold = null;

        /// <summary>
        /// Write a retention status to an object.
        /// Success: return an S3Response object with status 200. 
        /// </summary>
        public Func<S3Request, S3Response> WriteRetention = null;

        /// <summary>
        /// Read an object.  
        /// Success: return an S3Response object with status 200 and the object data.
        /// </summary>
        public Func<S3Request, S3Response> Read = null;

        /// <summary>
        /// Read an object's access control list.  
        /// Success: return an S3Response object with status 200 and an AccessControlPolicy as its data.
        /// </summary>
        public Func<S3Request, S3Response> ReadAcl = null;

        /// <summary>
        /// Read a range of bytes from an object.  
        /// Success: return an S3Response object with status 200 and the range of bytes as its data. 
        /// </summary>
        public Func<S3Request, S3Response> ReadRange = null;

        /// <summary>
        /// Read an object's tags.  
        /// Success: return an S3Response object with status 200 and a Tagging object as its data.
        /// </summary>
        public Func<S3Request, S3Response> ReadTags = null;

        /// <summary>
        /// Read an object's legal hold status.
        /// Success: return an S3Response object with status 200 and a LegalHold object as its data. 
        /// </summary>
        public Func<S3Request, S3Response> ReadLegalHold = null;

        /// <summary>
        /// Read an object's retention status.  
        /// Success: return an S3Response object with status 200 and a Retention object as its data.
        /// </summary>
        public Func<S3Request, S3Response> ReadRetention = null;

        /// <summary>
        /// Delete an object. 
        /// Success: return an S3Response object with status 204.
        /// </summary>
        public Func<S3Request, S3Response> Delete = null;

        /// <summary>
        /// Delete an object's tags.  
        /// Success: return an S3Response object with status 204.
        /// </summary>
        public Func<S3Request, S3Response> DeleteTags = null;

        /// <summary>
        /// Delete multiple objects.  
        /// Success: return an S3Response object with status 200 and a DeleteResult as its data.
        /// </summary>
        public Func<S3Request, S3Response> DeleteMultiple = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public ObjectCallbacks()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
