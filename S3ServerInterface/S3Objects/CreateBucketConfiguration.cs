using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    // built using https://xmltocsharp.azurewebsites.net/

    [XmlRoot(ElementName = "CreateBucketConfiguration", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class CreateBucketConfiguration
    {
        [XmlElement(ElementName = "LocationConstraint", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string LocationConstraint { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}
