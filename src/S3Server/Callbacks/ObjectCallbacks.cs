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
        /// For <c>bytes=first-last</c> and <c>bytes=first-</c>, ctx.Request.RangeStart is set (RangeEnd is null for an open-ended range).
        /// For a suffix range <c>bytes=-N</c>, ctx.Request.RangeSuffixLength is set and RangeStart and RangeEnd are null;
        /// return the last N bytes (or the whole object if it is shorter) and set S3Object.TotalSize, which is required to compute
        /// the Content-Range header.  A suffix range response without TotalSize fails with InternalError.
        /// Set S3Object.TotalSize for all ranges so the Content-Range header carries the real object size.
        /// </summary>
        public Func<S3Context, Task<S3Object>> ReadRange { get; set; } = null;

        /// <summary>
        /// Read an object's tags.
        /// </summary>
        public Func<S3Context, Task<Tagging>> ReadTagging { get; set; } = null;

        /// <summary>
        /// Restore an archived object.
        /// </summary>
        public Func<S3Context, RestoreRequest, Task<RestoreObjectResult>> Restore { get; set; } = null;

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
        /// Requests carrying the x-amz-copy-source header are routed to UploadPartCopy instead.
        /// </summary>
        public Func<S3Context, Task> UploadPart { get; set; } = null;

        /// <summary>
        /// Copy an object (PUT on an object key with the x-amz-copy-source header).
        /// The source is available in ctx.Request.CopySourceBucket, CopySourceKey, and CopySourceVersionId, and the
        /// copy-source conditions in ctx.Request.CopySourceIfMatch, CopySourceIfNoneMatch, CopySourceIfModifiedSince,
        /// and CopySourceIfUnmodifiedSince.  Other headers such as x-amz-metadata-directive are available through
        /// ctx.Request.RetrieveHeaderValue.  The returned CopyObjectResult is serialized as the response body with status 200.
        /// When this callback is null, the request falls through to DefaultRequestHandler or a NotImplemented error;
        /// it is never routed to Write.  Default is null.
        /// </summary>
        public Func<S3Context, Task<CopyObjectResult>> Copy { get; set; } = null;

        /// <summary>
        /// Upload a part by copying from an existing object (PUT with partNumber, uploadId, and the x-amz-copy-source header).
        /// The source is available in ctx.Request.CopySourceBucket, CopySourceKey, and CopySourceVersionId, and the optional
        /// byte range in ctx.Request.CopySourceRangeStart and CopySourceRangeEnd.  The returned CopyPartResult is serialized
        /// as the response body with status 200.  When this callback is null, the request falls through to
        /// DefaultRequestHandler or a NotImplemented error; it is never routed to UploadPart.  Default is null.
        /// </summary>
        public Func<S3Context, Task<CopyPartResult>> UploadPartCopy { get; set; } = null;

        /// <summary>
        /// Write an object.
        /// </summary>
        public Func<S3Context, Task> Write { get; set; } = null;

        /// <summary>
        /// Write an object's access control list, replacing the previous ACL.
        /// The AccessControlPolicy argument is null when the request has no body, which is the case for canned ACLs
        /// (the x-amz-acl header) and explicit grant headers (x-amz-grant-*); read those from ctx.Request.RetrieveHeaderValue.
        /// A body that is present but not valid XML is rejected with MalformedXML before this callback is invoked.
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
