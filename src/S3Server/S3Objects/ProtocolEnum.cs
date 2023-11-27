namespace S3ServerLibrary.S3Objects
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// Protocol types used by S3 for redirects. 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProtocolEnum
    {
        /// <summary>
        /// HTTP.
        /// </summary>
        [EnumMember(Value = "http")]
        [XmlEnum(Name = "http")]
        Http,
        /// <summary>
        /// HTTPS.
        /// </summary>
        [EnumMember(Value = "https")]
        [XmlEnum(Name = "https")]
        Https
    }
}
