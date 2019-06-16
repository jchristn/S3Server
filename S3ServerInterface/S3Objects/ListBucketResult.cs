using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    // built using https://xmltocsharp.azurewebsites.net/

    [XmlRoot(ElementName = "Contents", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class Contents
    {
        [XmlElement(ElementName = "Key", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string Key { get; set; }
        [XmlElement(ElementName = "LastModified", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string LastModified { get; set; }
        [XmlElement(ElementName = "ETag", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string ETag { get; set; }
        [XmlElement(ElementName = "Size", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string Size { get; set; }
        [XmlElement(ElementName = "StorageClass", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string StorageClass { get; set; }
    }

    [XmlRoot(ElementName = "ListBucketResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class ListBucketResult
    {
        [XmlElement(ElementName = "Name", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string Name { get; set; }
        [XmlElement(ElementName = "Prefix", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string Prefix { get; set; }
        [XmlElement(ElementName = "NextContinuationToken", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string NextContinuationToken { get; set; }
        [XmlElement(ElementName = "KeyCount", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string KeyCount { get; set; }
        [XmlElement(ElementName = "MaxKeys", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string MaxKeys { get; set; }
        [XmlElement(ElementName = "IsTruncated", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string IsTruncated { get; set; }
        [XmlElement(ElementName = "Contents", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public List<Contents> Contents { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}
