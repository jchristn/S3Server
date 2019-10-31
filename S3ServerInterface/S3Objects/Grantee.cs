using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// A permission recipient.
    /// </summary>
    [XmlInclude(typeof(CanonicalUser))]
    [XmlInclude(typeof(Group))]
    [XmlRoot(ElementName = "Grantee", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class Grantee
    {
        /// <summary>
        /// ID of the grantee.
        /// </summary>
        [XmlElement(ElementName = "ID")]
        public string ID { get; set; }
        /// <summary>
        /// Display name.
        /// </summary>
        [XmlElement(ElementName = "DisplayName")]
        public string DisplayName { get; set; }
        /// <summary>
        /// For a group, the URI of the group.
        /// </summary>
        [XmlElement(ElementName = "URI")]
        public string URI { get; set; }
        /// <summary>
        /// Type of grantee.
        /// </summary>
        [XmlElement(ElementName = "Type")]
        public string Type { get; set; }
        /// <summary>
        /// Email address of the grantee.
        /// </summary>
        [XmlElement(ElementName = "EmailAddress")]
        public string EmailAddress { get; set; }
    }

    /// <summary>
    /// Instantiate the object.
    /// </summary>
    [XmlType(TypeName = "CanonicalUser", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class CanonicalUser : Grantee
    {

    }

    /// <summary>
    /// Instantiate the object.
    /// </summary>
    [XmlType(TypeName = "Group", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class Group : Grantee
    {

    }
}
