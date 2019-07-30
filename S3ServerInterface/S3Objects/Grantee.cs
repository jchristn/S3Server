using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    [XmlInclude(typeof(CanonicalUser))]
    [XmlInclude(typeof(Group))]
    [XmlRoot(ElementName = "Grantee", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class Grantee
    {
        [XmlElement(ElementName = "ID")]
        public string ID { get; set; }
        [XmlElement(ElementName = "DisplayName")]
        public string DisplayName { get; set; }
        [XmlElement(ElementName = "URI")]
        public string URI { get; set; }
    }

    [XmlType(TypeName = "CanonicalUser", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class CanonicalUser : Grantee
    {

    }

    [XmlType(TypeName = "Group", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class Group : Grantee
    {

    }
}
