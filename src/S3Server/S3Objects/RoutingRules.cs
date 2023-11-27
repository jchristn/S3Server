namespace S3ServerLibrary.S3Objects
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Routing rules for a bucket's website configuration.
    /// </summary>
    [XmlRoot(ElementName = "RoutingRules", IsNullable = true)]
    public class RoutingRules
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// List of routing rules.
        /// </summary>
        [XmlElement(ElementName = "RoutingRule", IsNullable = true)]
        public List<RoutingRule> Rules { get; set; } = new List<RoutingRule>();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public RoutingRules()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="rules">Rules.</param>
        public RoutingRules(List<RoutingRule> rules)
        {
            if (rules != null) Rules = rules;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
