using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Logging status for a bucket.
    /// </summary>
    [XmlRoot(ElementName = "BucketLoggingStatus", Namespace = "http://doc.s3.amazonaws.com/2006-03-01")]
    public class BucketLoggingStatus
    {
        /// <summary>
        /// Logging configuration for a bucket.
        /// </summary>
        [XmlElement(ElementName = "LoggingEnabled", Namespace = "http://doc.s3.amazonaws.com/2006-03-01")]
        public LoggingEnabled LoggingEnabled { get; set; }
        /// <summary>
        /// XML namespace.
        /// </summary>
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}
