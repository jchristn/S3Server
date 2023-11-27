namespace S3ServerLibrary.S3Objects
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// Storage classes used by S3. 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StorageClassEnum
    {
        /// <summary>
        /// Standard.
        /// </summary>
        [EnumMember(Value = "STANDARD")]
        [XmlEnum(Name = "STANDARD")]
        STANDARD,
        /// <summary>
        /// Reduced redundancy.
        /// </summary>
        [EnumMember(Value = "REDUCED_REDUNDANCY")]
        [XmlEnum(Name = "REDUCED_REDUNDANCY")]
        REDUCED_REDUNDANCY,
        /// <summary>
        /// Glacier.
        /// </summary>
        [EnumMember(Value = "GLACIER")]
        [XmlEnum(Name = "GLACIER")]
        GLACIER,
        /// <summary>
        /// Standard IA.
        /// </summary>
        [EnumMember(Value = "STANDARD_IA")]
        [XmlEnum(Name = "STANDARD_IA")]
        STANDARD_IA,
        /// <summary>
        /// One zone IA.
        /// </summary>
        [EnumMember(Value = "ONEZONE_IA")]
        [XmlEnum(Name = "ONEZONE_IA")]
        ONEZONE_IA,
        /// <summary>
        /// Intelligent tiering.
        /// </summary>
        [EnumMember(Value = "INTELLIGENT_TIERING")]
        [XmlEnum(Name = "INTELLIGENT_TIERING")]
        INTELLIGENT_TIERING,
        /// <summary>
        /// Deep archive.
        /// </summary>
        [EnumMember(Value = "DEEP_ARCHIVE")]
        [XmlEnum(Name = "DEEP_ARCHIVE")]
        DEEP_ARCHIVE,
        /// <summary>
        /// Outposts.
        /// </summary>
        [EnumMember(Value = "OUTPOSTS")]
        [XmlEnum(Name = "OUTPOSTS")]
        OUTPOSTS
    }
}
