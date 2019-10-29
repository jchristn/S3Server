using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Amazon S3 access control policy for a resource.
    /// </summary>
    [XmlRoot(ElementName = "AccessControlPolicy", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class AccessControlPolicy
    {
        /// <summary>
        /// Resource owner.
        /// </summary>
        [XmlElement(ElementName = "Owner")]
        public Owner Owner { get; set; }
        /// <summary>
        /// Access control list for the resource.
        /// </summary>
        [XmlElement(ElementName = "AccessControlList")]
        public AccessControlList AccessControlList { get; set; }
    }
}
