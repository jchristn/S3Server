namespace S3ServerLibrary.S3Objects
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// Permission types used by S3. 
    /// See https://docs.aws.amazon.com/AmazonS3/latest/dev/acl-overview.html for details.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PermissionEnum
    {
        /// <summary>
        /// Full control.
        /// </summary>
        [EnumMember(Value = "FULL_CONTROL")]
        [XmlEnum(Name = "FULL_CONTROL")]
        FullControl,
        /// <summary>
        /// Write.
        /// </summary>
        [EnumMember(Value = "WRITE")]
        [XmlEnum(Name = "WRITE")]
        Write,
        /// <summary>
        /// Write access control permissions.
        /// </summary>
        [EnumMember(Value = "WRITE_ACP")]
        [XmlEnum(Name = "WRITE_ACP")]
        WriteAcp,
        /// <summary>
        /// Read.
        /// </summary>
        [EnumMember(Value = "READ")]
        [XmlEnum(Name = "READ")]
        Read,
        /// <summary>
        /// Read access control permissions.
        /// </summary>
        [EnumMember(Value = "READ_ACP")]
        [XmlEnum(Name = "READ_ACP")]
        ReadAcp,
    }
}
