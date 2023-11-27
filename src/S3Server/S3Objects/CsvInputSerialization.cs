namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// CSV input serialization.
    /// </summary>
    [XmlRoot(ElementName = "CSV")]
    public class CsvInputSerialization
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Allow quoted record delimiter.
        /// </summary>
        [XmlElement(ElementName = "AllowQuotedRecordDelimiter", IsNullable = true)]
        public bool AllowQuotedRecordDelimiter { get; set; } = true;

        /// <summary>
        /// Comments.
        /// </summary>
        [XmlElement(ElementName = "Comments", IsNullable = true)]
        public string Comments { get; set; } = null;

        /// <summary>
        /// Field delimiter.
        /// </summary>
        [XmlElement(ElementName = "FieldDelimiter", IsNullable = true)]
        public string FieldDelimiter { get; set; } = null;

        /// <summary>
        /// File header info.
        /// </summary>
        [XmlElement(ElementName = "FileHeaderInfo", IsNullable = true)]
        public FileHeaderInfoEnum FileHeaderInfo { get; set; } = FileHeaderInfoEnum.NONE;

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
        public CsvInputSerialization()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
