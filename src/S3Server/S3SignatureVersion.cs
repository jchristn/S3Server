using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;

namespace S3ServerLibrary
{
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
