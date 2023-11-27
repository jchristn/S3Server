namespace S3ServerLibrary
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    /// <summary>
    /// S3 signature version.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum S3SignatureVersion
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        [EnumMember(Value = "Unknown")]
        Unknown,
        /// <summary>
        /// Version2.
        /// </summary>
        [EnumMember(Value = "Version2")]
        Version2,
        /// <summary>
        /// Version4.
        /// </summary>
        [EnumMember(Value = "Version4")]
        Version4
    }
}
