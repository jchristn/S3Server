namespace S3ServerLibrary.S3Objects
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// Encoding type enum. 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EncodingTypeEnum
    {
        /// <summary>
        /// Delimiter.
        /// </summary>
        [EnumMember(Value = "Delimiter")]
        [XmlEnum(Name = "Delimiter")]
        Delimiter,
        /// <summary>
        /// KeyMarker.
        /// </summary>
        [EnumMember(Value = "KeyMarker")]
        [XmlEnum(Name = "KeyMarker")]
        KeyMarker,
        /// <summary>
        /// Prefix.
        /// </summary>
        [EnumMember(Value = "Prefix")]
        [XmlEnum(Name = "Prefix")]
        Prefix,
        /// <summary>
        /// NextKeyMarker.
        /// </summary>
        [EnumMember(Value = "NextKeyMarker")]
        [XmlEnum(Name = "NextKeyMarker")]
        NextKeyMarker,
        /// <summary>
        /// Key.
        /// </summary>
        [EnumMember(Value = "Key")]
        [XmlEnum(Name = "Key")]
        Key,
    }
}
