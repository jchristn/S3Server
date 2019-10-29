using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Legal hold status of a resource.
    /// </summary>
    [XmlRoot(ElementName = "LegalHold", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class LegalHold
    {
        /// <summary>
        /// Legal hold status.
        /// </summary>
        [XmlElement(ElementName = "Status", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string Status { get; set; }
    }
}
