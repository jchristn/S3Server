namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// JSON output serialization.
    /// </summary>
    [XmlRoot(ElementName = "JSON")]
    public class JsonOutputSerialization
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Record delimiter.
        /// </summary>
        [XmlElement(ElementName = "RecordDelimiter", IsNullable = true)]
        public string RecordDelimiter { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public JsonOutputSerialization()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
