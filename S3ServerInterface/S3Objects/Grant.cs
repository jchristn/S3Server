using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    [XmlRoot(ElementName = "Grant", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class Grant
    {
        [XmlElement(ElementName = "Grantee")]
        public Grantee Grantee { get; set; }
        [XmlElement(ElementName = "Permission")]
        public string Permission { get; set; }
    }
}
