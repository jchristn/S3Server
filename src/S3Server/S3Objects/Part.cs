namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// A part from a multipart upload.
    /// </summary>
    [XmlRoot(ElementName = "Part")]
    public class Part
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Checksum from CRC32.
        /// </summary>
        [XmlElement(ElementName = "ChecksumCRC32", IsNullable = true)]
        public string ChecksumCRC32 { get; set; } = null;

        /// <summary>
        /// Checksum from CRC32C.
        /// </summary>
        [XmlElement(ElementName = "ChecksumCRC32C", IsNullable = true)]
        public string ChecksumCRC32C { get; set; } = null;

        /// <summary>
        /// Checksum from SHA1.
        /// </summary>
        [XmlElement(ElementName = "ChecksumSHA1", IsNullable = true)]
        public string ChecksumSHA1 { get; set; } = null;

        /// <summary>
        /// Checksum from SHA256.
        /// </summary>
        [XmlElement(ElementName = "ChecksumSHA256", IsNullable = true)]
        public string ChecksumSHA256 { get; set; } = null;

        /// <summary>
        /// ETag.
        /// </summary>
        [XmlElement(ElementName = "ETag", IsNullable = true)]
        public string ETag { get; set; } = null;

        /// <summary>
        /// Timestamp from the last modification of the resource.
        /// </summary>
        [XmlElement(ElementName = "LastModified")]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Part number.
        /// </summary>
        [XmlElement(ElementName = "PartNumber", IsNullable = false)]
        public int PartNumber
        {
            get
            {
                return _PartNumber;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(PartNumber));
                _PartNumber = value;
            }
        }

        /// <summary>
        /// Size.
        /// </summary>
        [XmlElement(ElementName = "Size", IsNullable = false)]
        public int Size
        {
            get
            {
                return _Size;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Size));
                _Size = value;
            }
        }

        #endregion

        #region Private-Members

        private int _PartNumber = 0;
        private int _Size = 0;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public Part()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
