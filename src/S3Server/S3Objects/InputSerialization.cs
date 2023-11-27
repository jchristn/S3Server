namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Input serialization.
    /// </summary>
    [XmlRoot(ElementName = "InputSerialization")]
    public class InputSerialization
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Compression type.
        /// </summary>
        [XmlElement(ElementName = "CompressionType", IsNullable = false)]
        public CompressionTypeEnum CompressionType { get; set; } = CompressionTypeEnum.NONE;

        /// <summary>
        /// CSV input serialization.
        /// </summary>
        [XmlElement(ElementName = "CSV", IsNullable = true)]
        public CsvInputSerialization CSV { get; set; } = null;

        /// <summary>
        /// JSON input serialization.
        /// </summary>
        [XmlElement(ElementName = "JSON", IsNullable = true)]
        public JsonInputSerialization JSON { get; set; } = null;

        /// <summary>
        /// Parquet input serialization.
        /// </summary>
        [XmlElement(ElementName = "Parquet", IsNullable = true)]
        public ParquetInputSerialization Parquet { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public InputSerialization()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
