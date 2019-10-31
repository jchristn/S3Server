using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Logging status.
    /// </summary>
    [XmlRoot(ElementName = "LoggingEnabled", Namespace = "http://doc.s3.amazonaws.com/2006-03-01")]
    public class LoggingEnabled
    {
        /// <summary>
        /// The bucket where logs are stored.
        /// </summary>
        [XmlElement(ElementName = "TargetBucket", Namespace = "http://doc.s3.amazonaws.com/2006-03-01")]
        public string TargetBucket { get; set; }
        /// <summary>
        /// The prefix for objects used to store logging data.
        /// </summary>
        [XmlElement(ElementName = "TargetPrefix", Namespace = "http://doc.s3.amazonaws.com/2006-03-01")]
        public string TargetPrefix { get; set; }
        /// <summary>
        /// The grants allowing others to access logging data.
        /// </summary>
        [XmlElement(ElementName = "TargetGrants", Namespace = "http://doc.s3.amazonaws.com/2006-03-01")]
        public TargetGrants TargetGrants { get; set; }
    }
}
