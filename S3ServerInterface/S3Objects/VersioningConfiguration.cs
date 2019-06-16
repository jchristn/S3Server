using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    // built using https://xmltocsharp.azurewebsites.net/

    [XmlRoot(ElementName = "VersioningConfiguration", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class VersioningConfiguration
    {
        [XmlElement(ElementName = "Status", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string Status { get; set; }
        [XmlElement(ElementName = "MfaDelete", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string MfaDelete { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}
