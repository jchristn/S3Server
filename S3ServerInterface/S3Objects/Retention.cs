using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Retention status of a resource.
    /// </summary>
    [XmlRoot(ElementName = "Retention", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class Retention
    {
        /// <summary>
        /// Retention mode.
        /// </summary>
        [XmlElement(ElementName = "Mode", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string Mode { get; set; }
        /// <summary>
        /// Date upon which the resource shall no longer be retained.
        /// </summary>
        [XmlElement(ElementName = "RetainUntilDate", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public DateTime RetainUntilDate { get; set; }
    }
}
