using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Redirect rule for a bucket's website configuration.
    /// </summary>
    [XmlRoot(ElementName = "Redirect", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class Redirect
    {
        /// <summary>
        /// The hostname to which the request should be redirected.
        /// </summary>
        [XmlElement(ElementName = "HostName")]
        public string HostName { get; set; }
        /// <summary>
        /// The HTTP redirect code to use to perform the redirect.
        /// </summary>
        [XmlElement(ElementName = "HttpRedirectCode")]
        public string HttpRedirectCode { get; set; }
        /// <summary>
        /// The protocol that should be used with the redirect.
        /// </summary>
        [XmlElement(ElementName = "Protocol")]
        public string Protocol { get; set; }
        /// <summary>
        /// Replace the key prefix as specified when the redirect rule matches.
        /// </summary>
        [XmlElement(ElementName = "ReplaceKeyPrefixWith")]
        public string ReplaceKeyPrefixWith { get; set; }
        /// <summary>
        /// Replace the entire key as specified when the redirect rule matches.
        /// </summary>
        [XmlElement(ElementName = "ReplaceKeyWith")]
        public string ReplaceKeyWith { get; set; }
    }
}
