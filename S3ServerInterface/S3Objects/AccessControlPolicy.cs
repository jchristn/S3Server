using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    [XmlRoot(ElementName = "AccessControlPolicy", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class AccessControlPolicy
    {
        [XmlElement(ElementName = "Owner")]
        public Owner Owner { get; set; }
        [XmlElement(ElementName = "AccessControlList")]
        public AccessControlList AccessControlList { get; set; }
    }
}
