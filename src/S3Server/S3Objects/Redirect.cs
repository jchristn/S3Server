using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Redirect rule for a bucket's website configuration.
    /// </summary>
    [XmlRoot(ElementName = "Redirect", IsNullable = true)]
    public class Redirect
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// The hostname to which the request should be redirected.
        /// </summary>
        [XmlElement(ElementName = "HostName", IsNullable = true)]
        public string HostName { get; set; } = null;

        /// <summary>
        /// The HTTP redirect code to use to perform the redirect.
        /// </summary>
        [XmlElement(ElementName = "HttpRedirectCode", IsNullable = true)]
        public string HttpRedirectCode { get; set; } = null;

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

        /// <summary>
        /// Replace the key prefix as specified when the redirect rule matches.
        /// </summary>
        [XmlElement(ElementName = "ReplaceKeyPrefixWith", IsNullable = true)]
        public string ReplaceKeyPrefixWith { get; set; } = null;

        /// <summary>
        /// Replace the entire key as specified when the redirect rule matches.
        /// </summary>
        [XmlElement(ElementName = "ReplaceKeyWith", IsNullable = true)]
        public string ReplaceKeyWith { get; set; } = null;

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
        public Redirect()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="hostname">Hostname.</param>
        /// <param name="httpRedirectcode">HTTP redirect code.</param>
        /// <param name="protocol">Protocol.  Valid values are http, https.</param>
        /// <param name="replaceKeyPrefixWith">Replace key prefix with.</param>
        /// <param name="replaceKeyWith">Replace key with.</param>
        public Redirect(string hostname, string httpRedirectcode, string protocol, string replaceKeyPrefixWith, string replaceKeyWith)
        {
            HostName = hostname;
            HttpRedirectCode = httpRedirectcode;
            Protocol = protocol;
            ReplaceKeyPrefixWith = replaceKeyPrefixWith;
            ReplaceKeyWith = replaceKeyWith;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
