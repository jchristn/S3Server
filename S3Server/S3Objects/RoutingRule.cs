using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Routing rule for a bucket's website configuration.
    /// </summary>
    [XmlRoot(ElementName = "RoutingRule", IsNullable = true)]
    public class RoutingRule
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Condition that must be met in order to match a routing rule for a bucket website configuration.
        /// </summary>
        [XmlElement(ElementName = "Condition", IsNullable = true)]
        public Condition Condition { get; set; } = null;

        /// <summary>
        /// Redirect rule for a bucket's website configuration.
        /// </summary>
        [XmlElement(ElementName = "Redirect", IsNullable = true)]
        public Redirect Redirect { get; set; } = null;


        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public RoutingRule()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="condition">Condition.</param>
        /// <param name="redirect">Redirect.</param>
        public RoutingRule(Condition condition, Redirect redirect)
        {
            Condition = condition;
            Redirect = redirect;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
