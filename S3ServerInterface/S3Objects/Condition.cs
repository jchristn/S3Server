using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Condition that must be met in order to match a routing rule for a bucket website configuration.
    /// </summary>
    [XmlRoot(ElementName = "Condition", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class Condition
    {
        /// <summary>
        /// Assume a match if the HTTP error code returned matches the specified value.
        /// </summary>
        [XmlElement(ElementName = "HttpErrorCodeReturnedEquals")]
        public string HttpErrorCodeReturnedEquals { get; set; }
        /// <summary>
        /// Assume a match if the key prefix is equal to the specified value.
        /// </summary>
        [XmlElement(ElementName = "KeyPrefixEquals")]
        public string KeyPrefixEquals { get; set; }
    }
}
