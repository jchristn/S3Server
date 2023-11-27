namespace S3ServerLibrary.S3Objects
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// File header info. 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FileHeaderInfoEnum
    {
        /// <summary>
        /// USE.
        /// </summary>
        [EnumMember(Value = "USE")]
        [XmlEnum(Name = "USE")]
        USE,
        /// <summary>
        /// IGNORE.
        /// </summary>
        [EnumMember(Value = "IGNORE")]
        [XmlEnum(Name = "IGNORE")]
        IGNORE,
        /// <summary>
        /// NONE.
        /// </summary>
        [EnumMember(Value = "NONE")]
        [XmlEnum(Name = "NONE")]
        NONE
    }
}
