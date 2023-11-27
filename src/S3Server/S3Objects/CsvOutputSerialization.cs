namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// CSV output serialization.
    /// </summary>
    [XmlRoot(ElementName = "CSV")]
    public class CsvOutputSerialization
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Field delimiter.
        /// </summary>
        [XmlElement(ElementName = "FieldDelimiter", IsNullable = true)]
        public string FieldDelimiter { get; set; } = null;

        /// <summary>
        /// Quote character.
        /// </summary>
        [XmlElement(ElementName = "QuoteCharacter", IsNullable = true)]
        public string QuoteCharacter { get; set; } = null;

        /// <summary>
        /// Quote escape character.
        /// </summary>
        [XmlElement(ElementName = "QuoteEscapeCharacter", IsNullable = true)]
        public string QuoteEscapeCharacter { get; set; } = null;

        /// <summary>
        /// Quote fields.
        /// </summary>
        [XmlElement(ElementName = "QuoteFields", IsNullable = true)]
        public QuoteFieldsEnum QuoteFields { get; set; } = QuoteFieldsEnum.ALWAYS;

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
        public CsvOutputSerialization()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
