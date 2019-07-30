using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    [XmlRoot(ElementName = "AccessControlList", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class AccessControlList
    {
        [XmlElement(ElementName = "Grant")]
        public List<Grant> Grant { get; set; }
    }
}
