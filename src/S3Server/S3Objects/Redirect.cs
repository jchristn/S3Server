namespace S3ServerLibrary.S3Objects
{
    using System.Xml.Serialization;

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
        [XmlElement(ElementName = "HttpRedirectCode")]
        public int HttpRedirectCode { get; set; } = 301;

        /// <summary>
        /// The protocol that should be used with the redirect.
        /// Valid values are http, https.
        /// </summary>
        [XmlElement(ElementName = "Protocol")]
        public ProtocolEnum Protocol { get; set; } = ProtocolEnum.Http;

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
        /// <param name="httpRedirectCode">HTTP redirect code.</param>
        /// <param name="protocol">Protocol.  Valid values are http, https.</param>
        /// <param name="replaceKeyPrefixWith">Replace key prefix with.</param>
        /// <param name="replaceKeyWith">Replace key with.</param>
        public Redirect(string hostname, int httpRedirectCode, ProtocolEnum protocol, string replaceKeyPrefixWith, string replaceKeyWith)
        {
            HostName = hostname;
            HttpRedirectCode = httpRedirectCode;
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
