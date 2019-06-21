using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    // built using https://xmltocsharp.azurewebsites.net/

    [XmlRoot(ElementName = "Contents")]
    public class Contents
    {
        [XmlElement(ElementName = "Key")]
        public string Key { get; set; }
        [XmlElement(ElementName = "LastModified")]
        public string LastModified { get; set; }
        [XmlElement(ElementName = "ETag")]
        public string ETag { get; set; }
        [XmlElement(ElementName = "Size")]
        public string Size { get; set; }
        [XmlElement(ElementName = "StorageClass")]
        public string StorageClass { get; set; }
    }

    [XmlRoot(ElementName = "ListBucketResult")]
    public class ListBucketResult
    {
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "Prefix")]
        public string Prefix { get; set; }
        [XmlElement(ElementName = "NextContinuationToken")]
        public string NextContinuationToken { get; set; }
        [XmlElement(ElementName = "KeyCount")]
        public string KeyCount { get; set; }
        [XmlElement(ElementName = "MaxKeys")]
        public string MaxKeys { get; set; }
        [XmlElement(ElementName = "IsTruncated")]
        public string IsTruncated { get; set; }
        [XmlElement(ElementName = "Contents")]
        public List<Contents> Contents { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}
