using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    // built using https://xmltocsharp.azurewebsites.net/

    [XmlRoot(ElementName = "VersioningConfiguration")]
    public class VersioningConfiguration
    {
        [XmlElement(ElementName = "Status")]
        public string Status { get; set; }
        [XmlElement(ElementName = "MfaDelete")]
        public string MfaDelete { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}
