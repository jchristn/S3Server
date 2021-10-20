using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using S3ServerLibrary.S3Objects;

namespace S3ServerLibrary.Callbacks
{
    /// <summary>
    /// Callback methods for object operations.
    /// </summary>
    public class ObjectCallbacks
    {
        #region Public-Members

        /// <summary>
        /// Check for the existence of an object.
        /// </summary>
        public Func<S3Context, Task<ObjectMetadata>> Exists = null;

        /// <summary>
        /// Write an object.
        /// </summary>
        public Func<S3Context, Task> Write = null;

        /// <summary>
        /// Write an object's access control list, replacing the previous ACL.
        /// </summary>
        public Func<S3Context, AccessControlPolicy, Task> WriteAcl = null;

        /// <summary>
        /// Write tags to an object, replacing the previous tags.
        /// </summary>
        public Func<S3Context, Tagging, Task> WriteTagging = null;

        /// <summary>
        /// Write a legal hold status to an object.
        /// </summary>
        public Func<S3Context, LegalHold, Task> WriteLegalHold = null;

        /// <summary>
        /// Write a retention status to an object.
        /// </summary>
        public Func<S3Context, Retention, Task> WriteRetention = null;

        /// <summary>
        /// Read an object.
        /// </summary>
        public Func<S3Context, Task<S3Object>> Read = null;

        /// <summary>
        /// Read an object's access control list.
        /// </summary>
        public Func<S3Context, Task<AccessControlPolicy>> ReadAcl = null;

        /// <summary>
        /// Read a range of bytes from an object.
        /// </summary>
        public Func<S3Context, Task<S3Object>> ReadRange = null;

        /// <summary>
        /// Read an object's tags.
        /// </summary>
        public Func<S3Context, Task<Tagging>> ReadTagging = null;

        /// <summary>
        /// Read an object's legal hold status.
        /// </summary>
        public Func<S3Context, Task<LegalHold>> ReadLegalHold = null;

        /// <summary>
        /// Read an object's retention status.
        /// </summary>
        public Func<S3Context, Task<Retention>> ReadRetention = null;

        /// <summary>
        /// Delete an object.
        /// </summary>
        public Func<S3Context, Task> Delete = null;

        /// <summary>
        /// Delete an object's tags.
        /// </summary>
        public Func<S3Context, Task> DeleteTagging = null;

        /// <summary>
        /// Delete multiple objects.
        /// </summary>
        public Func<S3Context, DeleteMultiple, Task<DeleteResult>> DeleteMultiple = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
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
