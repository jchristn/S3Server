using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Routing rules for a bucket's website configuration.
    /// </summary>
    [XmlRoot(ElementName = "RoutingRules", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class RoutingRules
    {
        /// <summary>
        /// List of routing rules.
        /// </summary>
        [XmlElement(ElementName = "RoutingRule")]
        public List<RoutingRule> RoutingRule { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public RoutingRules()
        {
            RoutingRule = new List<RoutingRule>();
        }
    }
}
