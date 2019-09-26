using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace S3ServerInterface
{
    /// <summary>
    /// The type of request, specifically whether or not the bucket is included in the hostname.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum S3RequestStyle
    {
        /// <summary>
        /// Unknown request style.
        /// </summary>
        [EnumMember(Value = "Unknown")]
        Unknown,
        /// <summary>
        /// Bucket is not in the hostname, rather, it is in the URL.
        /// </summary>
        [EnumMember(Value = "BucketNotInHostname")]
        BucketNotInHostname,
        /// <summary>
        /// Bucket is in the hostname and not in the URL.
        /// </summary>
        [EnumMember(Value = "BucketInHostname")]
        BucketInHostname
    }
}
