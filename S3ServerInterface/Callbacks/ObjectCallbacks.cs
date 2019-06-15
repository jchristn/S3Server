using System;
using System.Collections.Generic;
using System.Text;

namespace S3ServerInterface.Callbacks
{
    public class ObjectCallbacks
    {
        #region Public-Members

        /// <summary>
        /// Check for the existence of an object.  A successful S3Response should include no data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> Exists = null;

        /// <summary>
        /// Write an object.  A successful S3Response should include no data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> Write = null;

        /// <summary>
        /// Write an object's access control list.  A successful S3Response should include no data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> WriteAcl = null;

        /// <summary>
        /// Write tags to an object.  A successful S3Response should include no data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> WriteTags = null;

        /// <summary>
        /// Write a legal hold status to an object.  A successful S3Response should include no data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> WriteLegalHold = null;

        /// <summary>
        /// Write a retention status to an object.  A successful S3Response should include no data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> WriteRetention = null;

        /// <summary>
        /// Read an object.  A successful S3Response should include the data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> Read = null;

        /// <summary>
        /// Read an object's access control list.  A successful S3Response should include an XML document containing the access control policy and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> ReadAcl = null;

        /// <summary>
        /// Read a range of bytes from an object.  A successful S3Response should include the data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> ReadRange = null;

        /// <summary>
        /// Read an object's tags.  A successful S3Response should include the data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> ReadTags = null;

        /// <summary>
        /// Read an object's legal hold status.  A successful S3Response should include the data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> ReadLegalHold = null;

        /// <summary>
        /// Read an object's retention status.  A successful S3Response should include the data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> ReadRetention = null;

        /// <summary>
        /// Delete an object.  A successful S3Response should include no data and return an HTTP 200.
        /// </summary>
        public Func<S3Request, S3Response> Delete = null;

        /// <summary>
        /// Delete an object's tags.  A successful S3Response should include no data and return an HTTP 204.
        /// </summary>
        public Func<S3Request, S3Response> DeleteTags = null;

        /// <summary>
        /// Delete multiple objects.  A successful S3Response should include an XML document containing the delete result and an HTTP 200.
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
