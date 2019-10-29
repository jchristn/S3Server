using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Bucket contents.
    /// </summary>
    [XmlRoot(ElementName = "Contents", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class Contents
    {
        /// <summary>
        /// Object key.
        /// </summary>
        [XmlElement(ElementName = "Key", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string Key { get; set; }
        /// <summary>
        /// Timestamp from the last modification of the resource.
        /// </summary>
        [XmlElement(ElementName = "LastModified", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public DateTime LastModified { get; set; }
        /// <summary>
        /// ETag of the resource.
        /// </summary>
        [XmlElement(ElementName = "ETag", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string ETag { get; set; }
        /// <summary>
        /// The size in bytes of the resource.
        /// </summary>
        [XmlElement(ElementName = "Size", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public long Size { get; set; }
        /// <summary>
        /// The class of storage where the resource resides.
        /// </summary>
        [XmlElement(ElementName = "StorageClass", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string StorageClass { get; set; }
    }
}
