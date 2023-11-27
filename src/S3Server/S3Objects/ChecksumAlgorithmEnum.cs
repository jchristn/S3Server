namespace S3ServerLibrary.S3Objects
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// Checksum algorithms. 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChecksumAlgorithmEnum
    {
        /// <summary>
        /// CRC32.
        /// </summary>
        [EnumMember(Value = "CRC32")]
        [XmlEnum(Name = "CRC32")]
        CRC32,
        /// <summary>
        /// CRC32C.
        /// </summary>
        [EnumMember(Value = "CRC32C")]
        [XmlEnum(Name = "CRC32C")]
        CRC32C,
        /// <summary>
        /// SHA1.
        /// </summary>
        [EnumMember(Value = "SHA1")]
        [XmlEnum(Name = "SHA1")]
        SHA1,
        /// <summary>
        /// SHA256.
        /// </summary>
        [EnumMember(Value = "SHA256")]
        [XmlEnum(Name = "SHA256")]
        SHA256,
    }
}
