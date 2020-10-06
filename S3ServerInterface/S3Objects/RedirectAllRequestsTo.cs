using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Host and protocol to which all requests should be redirected.
    /// </summary>
	[XmlRoot(ElementName = "RedirectAllRequestsTo", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class RedirectAllRequestsTo
    {
        /// <summary>
        /// Redirect all requests to the specified hostname.
        /// </summary>
        [XmlElement(ElementName = "HostName")]
        public string HostName { get; set; }

        /// <summary>
        /// Redirect all requests to the specified hostname using the specified protocol.
        /// </summary>
        [XmlElement(ElementName = "Protocol")]
        public string Protocol { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public RedirectAllRequestsTo()
        {
            HostName = null;
            Protocol = null;
        }
    }
}
