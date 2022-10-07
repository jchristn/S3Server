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
    /// Retention modes used by S3. 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RetentionModeEnum
    {
        /// <summary>
        /// Null.
        /// </summary>
        [EnumMember(Value = null)]
        [XmlEnum(Name = null)]
        None,
        /// <summary>
        /// Governance.
        /// </summary>
        [EnumMember(Value = "GOVERNANCE")]
        [XmlEnum(Name = "GOVERNANCE")]
        Governance,
        /// <summary>
        /// Compliance.
        /// </summary>
        [EnumMember(Value = "COMPLIANCE")]
        [XmlEnum(Name = "COMPLIANCE")]
        Compliance
    }
}
