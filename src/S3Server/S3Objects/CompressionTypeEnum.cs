namespace S3ServerLibrary.S3Objects
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// Compression type enum. 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CompressionTypeEnum
    {
        /// <summary>
        /// NONE.
        /// </summary>
        [EnumMember(Value = "NONE")]
        [XmlEnum(Name = "NONE")]
        NONE,
        /// <summary>
        /// GZIP.
        /// </summary>
        [EnumMember(Value = "GZIP")]
        [XmlEnum(Name = "GZIP")]
        GZIP,
        /// <summary>
        /// BZIP2.
        /// </summary>
        [EnumMember(Value = "BZIP2")]
        [XmlEnum(Name = "BZIP2")]
        BZIP2,
    }
}
