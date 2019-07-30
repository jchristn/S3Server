using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    [XmlRoot(ElementName = "ListVersionsResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class ListVersionsResult
    {
        [XmlElement(ElementName = "Name", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string Name { get; set; }
        [XmlElement(ElementName = "Prefix", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string Prefix { get; set; }
        [XmlElement(ElementName = "KeyMarker", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string KeyMarker { get; set; }
        [XmlElement(ElementName = "VersionIdMarker", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string VersionIdMarker { get; set; }
        [XmlElement(ElementName = "MaxKeys", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public long MaxKeys { get; set; }
        [XmlElement(ElementName = "IsTruncated", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public bool IsTruncated { get; set; }
        [XmlElement(ElementName = "Version", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public List<Version> Version { get; set; }
        [XmlElement(ElementName = "DeleteMarker", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public List<DeleteMarker> DeleteMarker { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }

        public ListVersionsResult()
        {
            DeleteMarker = new List<DeleteMarker>();
            Version = new List<Version>();
        }
    }
}
