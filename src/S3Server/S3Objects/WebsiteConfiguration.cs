namespace S3ServerLibrary.S3Objects
{
    using System.Xml.Serialization;

    /// <summary>
    /// Result from a GetBucketWebsite request.
    /// </summary>
    [XmlRoot(ElementName = "WebsiteConfiguration", IsNullable = true)]
    public class WebsiteConfiguration
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Host and protocol to which all requests should be redirected.
        /// </summary>
        [XmlElement(ElementName = "RedirectAllRequestsTo", IsNullable = true)]
        public RedirectAllRequestsTo RedirectAllRequestsTo { get; set; } = null;

        /// <summary>
        /// Parameters for the object that should serve as the index document for the bucket.
        /// </summary>
        [XmlElement(ElementName = "IndexDocument", IsNullable = true)]
        public IndexDocument IndexDocument { get; set; } = null;

        /// <summary>
        /// Object that should serve as the document to return should an error be encountered.
        /// </summary>
        [XmlElement(ElementName = "ErrorDocument", IsNullable = true)]
        public ErrorDocument ErrorDocument { get; set; } = null;

        /// <summary>
        /// Routing rules for a bucket's website configuration.
        /// </summary>
        [XmlElement(ElementName = "RoutingRules", IsNullable = true)]
        public RoutingRules RoutingRules { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public WebsiteConfiguration()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="redirectTo">Redirect all requests to.</param>
        /// <param name="indexDoc">Index document.</param>
        /// <param name="errorDoc">Error document.</param>
        /// <param name="routingRules">Routing rules.</param>
        public WebsiteConfiguration(RedirectAllRequestsTo redirectTo, IndexDocument indexDoc, ErrorDocument errorDoc, RoutingRules routingRules)
        {
            RedirectAllRequestsTo = redirectTo;
            IndexDocument = indexDoc;
            ErrorDocument = errorDoc;
            RoutingRules = routingRules;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
