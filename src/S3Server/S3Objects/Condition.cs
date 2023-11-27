namespace S3ServerLibrary.S3Objects
{
    using System.Xml.Serialization;

    /// <summary>
    /// Condition that must be met in order to match a routing rule for a bucket website configuration.
    /// </summary>
    [XmlRoot(ElementName = "Condition", IsNullable = true)]
    public class Condition
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Assume a match if the HTTP error code returned matches the specified value.
        /// </summary>
        [XmlElement(ElementName = "HttpErrorCodeReturnedEquals", IsNullable = true)]
        public string HttpErrorCodeReturnedEquals { get; set; } = null;

        /// <summary>
        /// Assume a match if the key prefix is equal to the specified value.
        /// </summary>
        [XmlElement(ElementName = "KeyPrefixEquals", IsNullable = true)]
        public string KeyPrefixEquals { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public Condition()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="httpErrorCode">HTTP error code.</param>
        /// <param name="keyPrefix">Key prefix.</param>
        public Condition(string httpErrorCode, string keyPrefix)
        {
            HttpErrorCodeReturnedEquals = httpErrorCode;
            KeyPrefixEquals = keyPrefix;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
