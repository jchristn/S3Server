using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Permission types used by S3. 
    /// See https://docs.aws.amazon.com/AmazonS3/latest/dev/acl-overview.html for details.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VersioningStatusEnum
    {
        /// <summary>
        /// Enabled.
        /// </summary>
        [EnumMember(Value = "Enabled")]
        [XmlEnum(Name = "Enabled")]
        Enabled,
        /// <summary>
        /// Suspended.
        /// </summary>
        [EnumMember(Value = "Suspended")]
        [XmlEnum(Name = "Suspended")]
        Suspended
    }
}
