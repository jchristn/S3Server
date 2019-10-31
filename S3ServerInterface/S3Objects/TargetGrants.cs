using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Recipients of the grants to access the resource.
    /// </summary>
    [XmlRoot(ElementName = "TargetGrants", Namespace = "http://doc.s3.amazonaws.com/2006-03-01")]
    public class TargetGrants
    {
        /// <summary>
        /// Grant specifying to whom rights are provided to the resource.
        /// </summary>
        [XmlElement(ElementName = "Grant", Namespace = "http://doc.s3.amazonaws.com/2006-03-01")]
        public List<Grant> Grant { get; set; }
    }
}
