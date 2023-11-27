namespace S3ServerLibrary.Callbacks
{
    using S3ServerLibrary.S3Objects;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Callback methods for object operations.
    /// </summary>
    public class ObjectCallbacks
    {
        #region Public-Members

        /// <summary>
        /// Abort a multipart upload.
        /// </summary>
        public Func<S3Context, Task> AbortMultipartUpload { get; set; } = null;

        /// <summary>
        /// Complete multipart upload.
        /// </summary>
        public Func<S3Context, CompleteMultipartUpload, Task<CompleteMultipartUploadResult>> CompleteMultipartUpload { get; set; } = null;

        /// <summary>
        /// Create multipart upload.
        /// </summary>
        public Func<S3Context, Task<InitiateMultipartUploadResult>> CreateMultipartUpload { get; set; } = null;

        /// <summary>
        /// Delete an object.
        /// </summary>
        public Func<S3Context, Task> Delete { get; set; } = null;

        /// <summary>
        /// Delete an object's ACL.
        /// </summary>
        public Func<S3Context, Task> DeleteAcl { get; set; } = null;

        /// <summary>
        /// Delete an object's tags.
        /// </summary>
        public Func<S3Context, Task> DeleteTagging { get; set; } = null;

        /// <summary>
        /// Delete multiple objects.
        /// </summary>
        public Func<S3Context, DeleteMultiple, Task<DeleteResult>> DeleteMultiple { get; set; } = null;

        /// <summary>
        /// Check for the existence of an object.
        /// Return the ObjectMetadata if it exists, null if it doesn't.
        /// </summary>
        public Func<S3Context, Task<ObjectMetadata>> Exists { get; set; } = null;

        /// <summary>
        /// Read an object.
        /// </summary>
        public Func<S3Context, Task<S3Object>> Read { get; set; } = null;

        /// <summary>
        /// Read an object's access control list.
        /// </summary>
        public Func<S3Context, Task<AccessControlPolicy>> ReadAcl { get; set; } = null;

        /// <summary>
        /// Read the parts associated with a multipart upload.
        /// </summary>
        public Func<S3Context, Task<ListPartsResult>> ReadParts { get; set; } = null;

        /// <summary>
        /// Read a range of bytes from an object.
        /// </summary>
        public Func<S3Context, Task<S3Object>> ReadRange { get; set; } = null;

        /// <summary>
        /// Read an object's tags.
        /// </summary>
        public Func<S3Context, Task<Tagging>> ReadTagging { get; set; } = null;

        /// <summary>
        /// Read an object's legal hold status.
        /// </summary>
        public Func<S3Context, Task<LegalHold>> ReadLegalHold { get; set; } = null;

        /// <summary>
        /// Read an object's retention status.
        /// </summary>
        public Func<S3Context, Task<Retention>> ReadRetention { get; set; } = null;

        /// <summary>
        /// Select content from an object.
        /// </summary>
        public Func<S3Context, SelectObjectContentRequest, Task> SelectContent { get; set; } = null;

        /// <summary>
        /// Upload part.
        /// </summary>
        public Func<S3Context, Task> UploadPart { get; set; } = null;

        /// <summary>
        /// Write an object.
        /// </summary>
        public Func<S3Context, Task> Write { get; set; } = null;

        /// <summary>
        /// Write an object's access control list, replacing the previous ACL.
        /// </summary>
        public Func<S3Context, AccessControlPolicy, Task> WriteAcl { get; set; } = null;

        /// <summary>
        /// Write tags to an object, replacing the previous tags.
        /// </summary>
        public Func<S3Context, Tagging, Task> WriteTagging { get; set; } = null;

        /// <summary>
        /// Write a legal hold status to an object.
        /// </summary>
        public Func<S3Context, LegalHold, Task> WriteLegalHold { get; set; } = null;

        /// <summary>
        /// Write a retention status to an object.
        /// </summary>
        public Func<S3Context, Retention, Task> WriteRetention { get; set; } = null;

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
