using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Result from a GetBucketWebsite request.
    /// </summary>
    [XmlRoot(ElementName = "WebsiteConfiguration", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class WebsiteConfiguration
    {
        /// <summary>
        /// Host and protocol to which all requests should be redirected.
        /// </summary>
        [XmlElement(ElementName = "RedirectAllRequestsTo")]
        public RedirectAllRequestsTo RedirectAllRequestsTo { get; set; }
        /// <summary>
        /// Parameters for the object that should serve as the index document for the bucket.
        /// </summary>
        [XmlElement(ElementName = "IndexDocument")]
        public IndexDocument IndexDocument { get; set; }
        /// <summary>
        /// Object that should serve as the document to return should an error be encountered.
        /// </summary>
        [XmlElement(ElementName = "ErrorDocument")]
        public ErrorDocument ErrorDocument { get; set; }
        /// <summary>
        /// Routing rules for a bucket's website configuration.
        /// </summary>
        [XmlElement(ElementName = "RoutingRules")]
        public RoutingRules RoutingRules { get; set; }
    }
}
