namespace S3ServerLibrary.S3Objects
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// JSON type enum. 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum JsonTypeEnum
    {
        /// <summary>
        /// DOCUMENT.
        /// </summary>
        [EnumMember(Value = "DOCUMENT")]
        [XmlEnum(Name = "DOCUMENT")]
        DOCUMENT,
        /// <summary>
        /// LINES.
        /// </summary>
        [EnumMember(Value = "LINES")]
        [XmlEnum(Name = "LINES")]
        LINES,
    }
}
