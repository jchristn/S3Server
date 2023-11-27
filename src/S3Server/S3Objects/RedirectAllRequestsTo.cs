namespace S3ServerLibrary.S3Objects
{
    using System.Xml.Serialization;

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
        [XmlElement(ElementName = "Protocol")]
        public ProtocolEnum Protocol { get; set; } = ProtocolEnum.Http;

        #endregion

        #region Private-Members

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
        public RedirectAllRequestsTo(string hostname, ProtocolEnum protocol)
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
