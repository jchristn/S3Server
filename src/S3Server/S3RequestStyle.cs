using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace S3ServerLibrary
{
    /// <summary>
    /// The type of request, specifically whether or not the bucket is included in the hostname.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum S3RequestStyle
    {
        /// <summary>
        /// Unknown request style.
        /// </summary>
        [EnumMember(Value = "Unknown")]
        Unknown,
        /// <summary>
        /// Path-style request, i.e. bucket is in the URL and not in the hostname, e.g. http://[hostname]/[bucket]/[key].
        /// </summary>
        [EnumMember(Value = "PathStyle")]
        PathStyle,
        /// <summary>
        /// Virtual-hosted style request, i.e. bucket is in the hostname and not in the URL, e.g. http://[bucket].[hostname]/[key].
        /// </summary>
        [EnumMember(Value = "VirtualHostedStyle")]
        VirtualHostedStyle
    }
}
