using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace S3ServerInterface
{
    /// <summary>
    /// S3 signature version.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
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
