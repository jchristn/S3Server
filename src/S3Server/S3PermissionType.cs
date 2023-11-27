namespace S3ServerLibrary
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Permission types used by S3. 
    /// See https://docs.aws.amazon.com/AmazonS3/latest/dev/acl-overview.html for details.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum S3PermissionType
    {
        /// <summary>
        /// Bucket read operations.
        /// </summary>
        [EnumMember(Value = "BucketRead")]
        BucketRead,
        /// <summary>
        /// Bucket write operations.
        /// </summary>
        [EnumMember(Value = "BucketWrite")]
        BucketWrite,
        /// <summary>
        /// Bucket read access control policy permissions.
        /// </summary>
        [EnumMember(Value = "BucketReadAcp")]
        BucketReadAcp,
        /// <summary>
        /// Bucket write access control policy permissions.
        /// </summary>
        [EnumMember(Value = "BucketWriteAcp")]
        BucketWriteAcp,
        /// <summary>
        /// Full control.
        /// </summary>
        [EnumMember(Value = "FullControl")]
        FullControl,
        /// <summary>
        /// Bucket read operations.
        /// </summary>
        [EnumMember(Value = "ObjectRead")]
        ObjectRead,
        /// <summary>
        /// Object write operations.
        /// This permission should not be used; refer to https://docs.aws.amazon.com/AmazonS3/latest/dev/acl-overview.html.
        /// </summary>
        [EnumMember(Value = "ObjectWrite")]
        ObjectWrite,
        /// <summary>
        /// Object read access control policy permissions.
        /// </summary>
        [EnumMember(Value = "ObjectReadAcp")]
        ObjectReadAcp,
        /// <summary>
        /// Object write access control policy permissions.
        /// </summary>
        [EnumMember(Value = "ObjectWriteAcp")]
        ObjectWriteAcp,
        /// <summary>
        /// No permission required or permission unknown.
        /// </summary>
        [EnumMember(Value = "NotApplicable")]
        NotApplicable
    }
}
