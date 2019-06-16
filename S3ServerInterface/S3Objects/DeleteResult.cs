using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    // built using https://xmltocsharp.azurewebsites.net/

    [XmlRoot(ElementName = "Deleted", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class Deleted
    {
        [XmlElement(ElementName = "Key", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string Key { get; set; }
        [XmlElement(ElementName = "VersionId", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string VersionId { get; set; }
        [XmlElement(ElementName = "DeleteMarker", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string DeleteMarker { get; set; }
        [XmlElement(ElementName = "DeleteMarkerVersionId", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string DeleteMarkerVersionId { get; set; }
    }

    [XmlRoot(ElementName = "Error", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class Error
    {
        [XmlElement(ElementName = "Key", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string Key { get; set; }
        [XmlElement(ElementName = "Code", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string Code { get; set; }
        [XmlElement(ElementName = "Message", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string Message { get; set; }
    }

    [XmlRoot(ElementName = "DeleteResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class DeleteResult
    {
        [XmlElement(ElementName = "Deleted", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public List<Deleted> Deleted { get; set; }
        [XmlElement(ElementName = "Error", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public List<Error> Error { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}
