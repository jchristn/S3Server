using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    // built using https://xmltocsharp.azurewebsites.net/

    [XmlRoot(ElementName = "Deleted")]
    public class Deleted
    {
        [XmlElement(ElementName = "Key")]
        public string Key { get; set; }
        [XmlElement(ElementName = "VersionId")]
        public string VersionId { get; set; }
        [XmlElement(ElementName = "DeleteMarker")]
        public string DeleteMarker { get; set; }
        [XmlElement(ElementName = "DeleteMarkerVersionId")]
        public string DeleteMarkerVersionId { get; set; }
    }
    
    [XmlRoot(ElementName = "DeleteResult")]
    public class DeleteResult
    {
        [XmlElement(ElementName = "Deleted")]
        public List<Deleted> Deleted { get; set; }
        [XmlElement(ElementName = "Error")]
        public List<Error> Error { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}
