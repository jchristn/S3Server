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
    public enum RequestStyle
    {
        [EnumMember(Value = "Unknown")]
        Unknown,
        [EnumMember(Value = "BucketNotInHostname")]
        BucketNotInHostname,
        [EnumMember(Value = "BucketInHostname")]
        BucketInHostname
    }
}
