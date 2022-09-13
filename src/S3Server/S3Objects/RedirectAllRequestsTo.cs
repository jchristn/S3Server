using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Host and protocol to which all requests should be redirected.
    /// </summary>
	[XmlRoot(ElementName = "RedirectAllRequestsTo", IsNullable = true)]
    public class RedirectAllRequestsTo
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Redirect all requests to the specified hostname.
        /// </summary>
        [XmlElement(ElementName = "HostName", IsNullable = true)]
        public string HostName { get; set; } = null;

        /// <summary>
        /// The protocol that should be used with the redirect.
        /// Valid values are http, https.
        /// </summary>
        [XmlElement(ElementName = "Protocol", IsNullable = true)]
        public string Protocol
        {
            get
            {
                return _Protocol;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) _Protocol = value;
                else
                {
                    if (!_ProtocolValidValues.Contains(value)) throw new ArgumentException("Unknown Protocol '" + value + "'.");
                    _Protocol = value;
                }
            }
        }

        #endregion

        #region Private-Members

        private string _Protocol = "http";
        private List<string> _ProtocolValidValues = new List<string>
        {
            "http",
            "https"
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public RedirectAllRequestsTo()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="hostname">Hostname.</param>
        /// <param name="protocol">Protocol.  Valid values are http, https.</param>
        public RedirectAllRequestsTo(string hostname, string protocol)
        {
            HostName = hostname;
            Protocol = protocol;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
