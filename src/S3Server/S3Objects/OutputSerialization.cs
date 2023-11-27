namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Output serialization.
    /// </summary>
    [XmlRoot(ElementName = "OutputSerialization")]
    public class OutputSerialization
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// CSV output serialization.
        /// </summary>
        [XmlElement(ElementName = "CSV", IsNullable = true)]
        public CsvOutputSerialization CSV { get; set; } = null;

        /// <summary>
        /// JSON output serialization.
        /// </summary>
        [XmlElement(ElementName = "JSON", IsNullable = true)]
        public JsonOutputSerialization JSON { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public OutputSerialization()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
