using System;
using System.Collections.Generic;
using System.Text;

namespace S3ServerInterface
{
    /// <summary>
    /// The type of S3 request, identified by parsing the request.
    /// </summary>
    public enum S3RequestType
    { 
        /// <summary>
        /// Unknown request type.
        /// </summary>
        Unknown, 
        /// <summary>
        /// List buckets owned by the user.
        /// </summary>
        ListBuckets,
        /// <summary>
        /// Delete a bucket.
        /// </summary>
        BucketDelete,
        /// <summary>
        /// Delete bucket tags.
        /// </summary>
        BucketDeleteTags,
        /// <summary>
        /// Check if a bucket exists.
        /// </summary>
        BucketExists,
        /// <summary>
        /// List objects in a bucket.
        /// </summary>
        BucketRead,
        /// <summary>
        /// Read a bucket's access control list.
        /// </summary>
        BucketReadAcl,
        /// <summary>
        /// Read a bucket's location, i.e. its region.
        /// </summary>
        BucketReadLocation,
        /// <summary>
        /// Read a bucket's logging configuration.
        /// </summary>
        BucketReadLogging,
        /// <summary>
        /// Read a bucket's tags.
        /// </summary>
        BucketReadTags,
        /// <summary>
        /// Read a bucket's versioning configuration.
        /// </summary>
        BucketReadVersioning,
        /// <summary>
        /// List object versions.
        /// </summary>
        BucketReadVersions,
        /// <summary>
        /// Create a bucket.
        /// </summary>
        BucketWrite,
        /// <summary>
        /// Write a bucket's access control list.
        /// </summary>
        BucketWriteAcl,
        /// <summary>
        /// Write a bucket's logging configuration.
        /// </summary>
        BucketWriteLogging,
        /// <summary>
        /// Write a bucket's tags.
        /// </summary>
        BucketWriteTags,
        /// <summary>
        /// Write a bucket's versioning policy.
        /// </summary>
        BucketWriteVersioning,
        /// <summary>
        /// Delete an object.
        /// </summary>
        ObjectDelete,
        /// <summary>
        /// Delete multiple objects.
        /// </summary>
        ObjectDeleteMultiple,
        /// <summary>
        /// Delete object tags.
        /// </summary>
        ObjectDeleteTags,
        /// <summary>
        /// Check if an object exists.
        /// </summary>
        ObjectExists,
        /// <summary>
        /// Read an object's contents.
        /// </summary>
        ObjectRead,
        /// <summary>
        /// Read an object's access control list.
        /// </summary>
        ObjectReadAcl,
        /// <summary>
        /// Read an object's legal hold status.
        /// </summary>
        ObjectReadLegalHold,
        /// <summary>
        /// Read a specific range of bytes from an object.
        /// </summary>
        ObjectReadRange,
        /// <summary>
        /// Read an object's retention policy.
        /// </summary>
        ObjectReadRetention,
        /// <summary>
        /// Read an object's tags.
        /// </summary>
        ObjectReadTags,
        /// <summary>
        /// Create an object.
        /// </summary>
        ObjectWrite,
        /// <summary>
        /// Write an object's access control list.
        /// </summary>
        ObjectWriteAcl,
        /// <summary>
        /// Write an object's legal hold status.
        /// </summary>
        ObjectWriteLegalHold,
        /// <summary>
        /// Write an object's retention policy.
        /// </summary>
        ObjectWriteRetention,
        /// <summary>
        /// Write tags to an object.
        /// </summary>
        ObjectWriteTags
    }
}
