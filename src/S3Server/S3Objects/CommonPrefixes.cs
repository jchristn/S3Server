namespace S3ServerLibrary.S3Objects
{
    using System.Xml.Serialization;

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
        public string Prefix { get; set; } = null;

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
        /// <param name="prefix">Prefix.</param>
        public CommonPrefixes(string prefix)
        {
            Prefix = prefix;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
