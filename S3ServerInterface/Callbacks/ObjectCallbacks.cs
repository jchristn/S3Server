using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace S3ServerInterface.Callbacks
{
    /// <summary>
    /// Callback methods for object operations.
    /// </summary>
    public class ObjectCallbacks
    {
        #region Public-Members

        /// <summary>
        /// Check for the existence of an object.  
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response, Task> Exists = null;

        /// <summary>
        /// Write an object.
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response, Task> Write = null;

        /// <summary>
        /// Write an object's access control list, replacing the previous ACL.  
        /// Success: return an S3Response object with status 200. 
        /// </summary>
        public Func<S3Request, S3Response, Task> WriteAcl = null;

        /// <summary>
        /// Write tags to an object, replacing the previous tags.
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response, Task> WriteTags = null;

        /// <summary>
        /// Write a legal hold status to an object. 
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response, Task> WriteLegalHold = null;

        /// <summary>
        /// Write a retention status to an object.
        /// Success: return an S3Response object with status 200. 
        /// </summary>
        public Func<S3Request, S3Response, Task> WriteRetention = null;

        /// <summary>
        /// Read an object.  
        /// Success: return an S3Response object with status 200 and the object data.
        /// </summary>
        public Func<S3Request, S3Response, Task> Read = null;

        /// <summary>
        /// Read an object's access control list.  
        /// Success: return an S3Response object with status 200 and an AccessControlPolicy as its data.
        /// </summary>
        public Func<S3Request, S3Response, Task> ReadAcl = null;

        /// <summary>
        /// Read a range of bytes from an object.  
        /// Success: return an S3Response object with status 200 and the range of bytes as its data. 
        /// </summary>
        public Func<S3Request, S3Response, Task> ReadRange = null;

        /// <summary>
        /// Read an object's tags.  
        /// Success: return an S3Response object with status 200 and a Tagging object as its data.
        /// </summary>
        public Func<S3Request, S3Response, Task> ReadTags = null;

        /// <summary>
        /// Read an object's legal hold status.
        /// Success: return an S3Response object with status 200 and a LegalHold object as its data. 
        /// </summary>
        public Func<S3Request, S3Response, Task> ReadLegalHold = null;

        /// <summary>
        /// Read an object's retention status.  
        /// Success: return an S3Response object with status 200 and a Retention object as its data.
        /// </summary>
        public Func<S3Request, S3Response, Task> ReadRetention = null;

        /// <summary>
        /// Delete an object. 
        /// Success: return an S3Response object with status 204.
        /// </summary>
        public Func<S3Request, S3Response, Task> Delete = null;

        /// <summary>
        /// Delete an object's tags.  
        /// Success: return an S3Response object with status 204.
        /// </summary>
        public Func<S3Request, S3Response, Task> DeleteTags = null;

        /// <summary>
        /// Delete multiple objects.  
        /// Success: return an S3Response object with status 200 and a DeleteResult as its data.
        /// </summary>
        public Func<S3Request, S3Response, Task> DeleteMultiple = null;

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
