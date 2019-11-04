using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Routing rule for a bucket's website configuration.
    /// </summary>
    [XmlRoot(ElementName = "RoutingRule", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class RoutingRule
    {
        /// <summary>
        /// Condition that must be met in order to match a routing rule for a bucket website configuration.
        /// </summary>
        [XmlElement(ElementName = "Condition")]
        public Condition Condition { get; set; }
        /// <summary>
        /// Redirect rule for a bucket's website configuration.
        /// </summary>
        [XmlElement(ElementName = "Redirect")]
        public Redirect Redirect { get; set; }
    }
}
