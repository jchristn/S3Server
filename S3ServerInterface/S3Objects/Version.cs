using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Object version.
    /// </summary>
    [XmlRoot(ElementName = "Version", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class Version
    {
        /// <summary>
        /// Object key.
        /// </summary>
        [XmlElement(ElementName = "Key", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string Key { get; set; }
        /// <summary>
        /// Version identifier.
        /// </summary>
        [XmlElement(ElementName = "VersionId", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string VersionId { get; set; }
        /// <summary>
        /// Indicates if this version is the latest version for the object.
        /// </summary>
        [XmlElement(ElementName = "IsLatest", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public bool IsLatest { get; set; }
        /// <summary>
        /// Timestamp from the last modification.
        /// </summary>
        [XmlElement(ElementName = "LastModified", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public DateTime LastModified { get; set; }
        /// <summary>
        /// Object ETag.
        /// </summary>
        [XmlElement(ElementName = "ETag", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string ETag { get; set; }
        /// <summary>
        /// Content length of the object.
        /// </summary>
        [XmlElement(ElementName = "Size", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public long Size { get; set; }
        /// <summary>
        /// Storage class for the object.
        /// </summary>
        [XmlElement(ElementName = "StorageClass", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string StorageClass { get; set; }
        /// <summary>
        /// Object owner.
        /// </summary>
        [XmlElement(ElementName = "Owner", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public Owner Owner { get; set; }
    }
}
