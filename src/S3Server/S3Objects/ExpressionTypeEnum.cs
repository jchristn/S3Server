namespace S3ServerLibrary.S3Objects
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// Expression type enum. 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ExpressionTypeEnum
    {
        /// <summary>
        /// SQL.
        /// </summary>
        [EnumMember(Value = "SQL")]
        [XmlEnum(Name = "SQL")]
        SQL,
    }
}
