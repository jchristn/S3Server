namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// JSON input serialization.
    /// </summary>
    [XmlRoot(ElementName = "JSON")]
    public class JsonInputSerialization
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Allow quoted record delimiter.
        /// </summary>
        [XmlElement(ElementName = "Type", IsNullable = true)]
        public JsonTypeEnum Type { get; set; } = JsonTypeEnum.DOCUMENT;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public JsonInputSerialization()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
