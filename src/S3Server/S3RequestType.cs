using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;

namespace S3ServerLibrary
{
    /// <summary>
    /// The type of S3 request, identified by parsing the request.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum S3RequestType
    {
        /// <summary>
        /// Unknown request type.
        /// </summary>
        [EnumMember(Value = "Unknown")]
        Unknown,
        /// <summary>
        /// List buckets owned by the user.
        /// </summary>
        [EnumMember(Value = "ListBuckets")]
        ListBuckets,
        /// <summary>
        /// Service exists.
        /// </summary>
        [EnumMember(Value = "ServiceExists")]
        ServiceExists,
        /// <summary>
        /// Delete a bucket.
        /// </summary>
        [EnumMember(Value = "BucketDelete")]
        BucketDelete,
        /// <summary>
        /// Delete bucket ACL.
        /// </summary>
        [EnumMember(Value = "BucketDeleteAcl")]
        BucketDeleteAcl,
        /// <summary>
        /// Delete bucket tags.
        /// </summary>
        [EnumMember(Value = "BucketDeleteTags")]
        BucketDeleteTags,
        /// <summary>
        /// Delete a bucket's website configuration.
        /// </summary>
        [EnumMember(Value = "BucketDeleteWebsite")]
        BucketDeleteWebsite,
        /// <summary>
        /// Check if a bucket exists.
        /// </summary>
        [EnumMember(Value = "BucketExists")]
        BucketExists,
        /// <summary>
        /// List objects in a bucket.
        /// </summary>
        [EnumMember(Value = "BucketRead")]
        BucketRead,
        /// <summary>
        /// Read a bucket's access control list.
        /// </summary>
        [EnumMember(Value = "BucketReadAcl")]
        BucketReadAcl,
        /// <summary>
        /// Read a bucket's location, i.e. its region.
        /// </summary>
        [EnumMember(Value = "BucketReadLocation")]
        BucketReadLocation,
        /// <summary>
        /// Read a bucket's logging configuration.
        /// </summary>
        [EnumMember(Value = "BucketReadLogging")]
        BucketReadLogging,
        /// <summary>
        /// Read a bucket's tags.
        /// </summary>
        [EnumMember(Value = "BucketReadTags")]
        BucketReadTags,
        /// <summary>
        /// Read a bucket's versioning configuration.
        /// </summary>
        [EnumMember(Value = "BucketReadVersioning")]
        BucketReadVersioning,
        /// <summary>
        /// List object versions.
        /// </summary>
        [EnumMember(Value = "BucketReadVersions")]
        BucketReadVersions,
        /// <summary>
        /// Read a bucket's website configuration.
        /// </summary>
        [EnumMember(Value = "BucketReadWebsite")]
        BucketReadWebsite,
        /// <summary>
        /// Create a bucket.
        /// </summary>
        [EnumMember(Value = "BucketWrite")]
        BucketWrite,
        /// <summary>
        /// Write a bucket's access control list.
        /// </summary>
        [EnumMember(Value = "BucketWriteAcl")]
        BucketWriteAcl,
        /// <summary>
        /// Write a bucket's logging configuration.
        /// </summary>
        [EnumMember(Value = "BucketWriteLogging")]
        BucketWriteLogging,
        /// <summary>
        /// Write a bucket's tags.
        /// </summary>
        [EnumMember(Value = "BucketWriteTags")]
        BucketWriteTags,
        /// <summary>
        /// Write a bucket's versioning policy.
        /// </summary>
        [EnumMember(Value = "BucketWriteVersioning")]
        BucketWriteVersioning,
        /// <summary>
        /// Write a bucket's website configuration.
        /// </summary>
        [EnumMember(Value = "BucketWriteWebsite")]
        BucketWriteWebsite,
        /// <summary>
        /// Delete an object.
        /// </summary>
        [EnumMember(Value = "ObjectDelete")]
        ObjectDelete,
        /// <summary>
        /// Delete an object's ACLs.
        /// </summary>
        [EnumMember(Value = "ObjectDeleteAcl")]
        ObjectDeleteAcl,
        /// <summary>
        /// Delete multiple objects.
        /// </summary>
        [EnumMember(Value = "ObjectDeleteMultiple")]
        ObjectDeleteMultiple,
        /// <summary>
        /// Delete object tags.
        /// </summary>
        [EnumMember(Value = "ObjectDeleteTags")]
        ObjectDeleteTags,
        /// <summary>
        /// Check if an object exists.
        /// </summary>
        [EnumMember(Value = "ObjectExists")]
        ObjectExists,
        /// <summary>
        /// Read an object's contents.
        /// </summary>
        [EnumMember(Value = "ObjectRead")]
        ObjectRead,
        /// <summary>
        /// Read an object's access control list.
        /// </summary>
        [EnumMember(Value = "ObjectReadAcl")]
        ObjectReadAcl,
        /// <summary>
        /// Read an object's legal hold status.
        /// </summary>
        [EnumMember(Value = "ObjectReadLegalHold")]
        ObjectReadLegalHold,
        /// <summary>
        /// Read a specific range of bytes from an object.
        /// </summary>
        [EnumMember(Value = "ObjectReadRange")]
        ObjectReadRange,
        /// <summary>
        /// Read an object's retention policy.
        /// </summary>
        [EnumMember(Value = "ObjectReadRetention")]
        ObjectReadRetention,
        /// <summary>
        /// Read an object's tags.
        /// </summary>
        [EnumMember(Value = "ObjectReadTags")]
        ObjectReadTags,
        /// <summary>
        /// Create an object.
        /// </summary>
        [EnumMember(Value = "ObjectWrite")]
        ObjectWrite,
        /// <summary>
        /// Write an object's access control list.
        /// </summary>
        [EnumMember(Value = "ObjectWriteAcl")]
        ObjectWriteAcl,
        /// <summary>
        /// Write an object's legal hold status.
        /// </summary>
        [EnumMember(Value = "ObjectWriteLegalHold")]
        ObjectWriteLegalHold,
        /// <summary>
        /// Write an object's retention policy.
        /// </summary>
        [EnumMember(Value = "ObjectWriteRetention")]
        ObjectWriteRetention,
        /// <summary>
        /// Write tags to an object.
        /// </summary>
        [EnumMember(Value = "ObjectWriteTags")]
        ObjectWriteTags
    }
}
