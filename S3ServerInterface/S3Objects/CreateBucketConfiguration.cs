using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    // built using https://xmltocsharp.azurewebsites.net/

    [XmlRoot(ElementName = "CreateBucketConfiguration")]
    public class CreateBucketConfiguration
    {
        [XmlElement(ElementName = "LocationConstraint")]
        public string LocationConstraint { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}
