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
    /// Storage classes used by S3. 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StorageClassEnum
    {
        /// <summary>
        /// Standard.
        /// </summary>
        [EnumMember(Value = "STANDARD")]
        [XmlEnum(Name = "STANDARD")]
        Standard,
        /// <summary>
        /// Reduced redundancy.
        /// </summary>
        [EnumMember(Value = "REDUCED_REDUNDANCY")]
        [XmlEnum(Name = "REDUCED_REDUNDANCY")]
        ReducedRedundancy,
        /// <summary>
        /// Glacier.
        /// </summary>
        [EnumMember(Value = "GLACIER")]
        [XmlEnum(Name = "GLACIER")]
        Glacier,
        /// <summary>
        /// Standard IA.
        /// </summary>
        [EnumMember(Value = "STANDARD_IA")]
        [XmlEnum(Name = "STANDARD_IA")]
        StandardIa,
        /// <summary>
        /// One zone IA.
        /// </summary>
        [EnumMember(Value = "ONEZONE_IA")]
        [XmlEnum(Name = "ONEZONE_IA")]
        OneZoneIa,
        /// <summary>
        /// Intelligent tiering.
        /// </summary>
        [EnumMember(Value = "INTELLIGENT_TIERING")]
        [XmlEnum(Name = "INTELLIGENT_TIERING")]
        IntelligentTiering,
        /// <summary>
        /// Deep archive.
        /// </summary>
        [EnumMember(Value = "DEEP_ARCHIVE")]
        [XmlEnum(Name = "DEEP_ARCHIVE")]
        DeepArchive,
        /// <summary>
        /// Outposts.
        /// </summary>
        [EnumMember(Value = "OUTPOSTS")]
        [XmlEnum(Name = "OUTPOSTS")]
        Outposts,
        /// <summary>
        /// None.
        /// </summary>
        [EnumMember(Value = null)]
        [XmlEnum(Name = null)]
        None
    }
}
