using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    // built using https://xmltocsharp.azurewebsites.net/

    [XmlRoot(ElementName = "Owner", Namespace = "http://s3.amazonaws.com/doc/2006-03-01")]
    public class Owner
    {
        [XmlElement(ElementName = "ID", Namespace = "http://s3.amazonaws.com/doc/2006-03-01")]
        public string ID { get; set; }
        [XmlElement(ElementName = "DisplayName", Namespace = "http://s3.amazonaws.com/doc/2006-03-01")]
        public string DisplayName { get; set; }
    }
}
