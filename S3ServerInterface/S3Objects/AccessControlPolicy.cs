using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    // built using https://xmltocsharp.azurewebsites.net/

    // Original namespacing: [XmlRoot(ElementName = "VersioningConfiguration", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]

    [XmlRoot(ElementName = "Grantee")]
    public class Grantee
    {
        [XmlElement(ElementName = "ID")]
        public string ID { get; set; }
        [XmlElement(ElementName = "DisplayName")]
        public string DisplayName { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "type", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "Grant")]
    public class Grant
    {
        [XmlElement(ElementName = "Grantee")]
        public Grantee Grantee { get; set; }
        [XmlElement(ElementName = "Permission")]
        public string Permission { get; set; }
    }

    [XmlRoot(ElementName = "AccessControlList")]
    public class AccessControlList
    {
        [XmlElement(ElementName = "Grant")]
        public List<Grant> Grant { get; set; }
    }

    [XmlRoot(ElementName = "AccessControlPolicy")]
    public class AccessControlPolicy
    {
        [XmlElement(ElementName = "Owner")]
        public Owner Owner { get; set; }
        [XmlElement(ElementName = "AccessControlList")]
        public AccessControlList AccessControlList { get; set; }
    } 
}
