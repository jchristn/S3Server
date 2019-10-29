using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Amazon S3 access control list for a resource.
    /// </summary>
    [XmlRoot(ElementName = "AccessControlList", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class AccessControlList
    {
        /// <summary>
        /// Grant specifying to whom rights are provided to the resource.
        /// </summary>
        [XmlElement(ElementName = "Grant")]
        public List<Grant> Grant { get; set; }
    }
}
