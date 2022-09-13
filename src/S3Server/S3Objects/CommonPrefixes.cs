using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Common prefixes.
    /// </summary>
    [XmlRoot(ElementName = "CommonPrefixes", IsNullable = true)]
    public class CommonPrefixes
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Prefix.
        /// </summary>
        [XmlElement(ElementName = "Prefix", IsNullable = true)]
        public List<string> Prefixes { get; set; } = new List<string>();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public CommonPrefixes()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="prefixes">Prefixes.</param>
        public CommonPrefixes(List<string> prefixes)
        {
            if (prefixes != null) Prefixes = prefixes;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
