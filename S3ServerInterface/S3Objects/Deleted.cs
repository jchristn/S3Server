using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    [XmlRoot(ElementName = "Deleted", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class Deleted
    {
        [XmlElement(ElementName = "Key", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string Key { get; set; }
        [XmlElement(ElementName = "VersionId", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string VersionId { get; set; }
        [XmlElement(ElementName = "DeleteMarker", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string DeleteMarker { get; set; }
        [XmlElement(ElementName = "DeleteMarkerVersionId", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string DeleteMarkerVersionId { get; set; }
    }
}
