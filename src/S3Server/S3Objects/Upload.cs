namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Upload.
    /// </summary>
    [XmlRoot(ElementName = "Upload", IsNullable = true)]
    public class Upload
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Key.
        /// </summary>
        [XmlElement(ElementName = "Key", IsNullable = false)]
        public string Key { get; set; } = null;

        /// <summary>
        /// Upload ID.
        /// </summary>
        [XmlElement(ElementName = "UploadId", IsNullable = false)]
        public string UploadId { get; set; } = null;

        /// <summary>
        /// Initiator of the upload.
        /// </summary>
        [XmlElement(ElementName = "Initiator")]
        public Owner Initiator { get; set; } = new Owner();

        /// <summary>
        /// Owner of the object.
        /// </summary>
        [XmlElement(ElementName = "Owner")]
        public Owner Owner { get; set; } = new Owner();

        /// <summary>
        /// Storage class.
        /// </summary>
        [XmlElement(ElementName = "StorageClass", IsNullable = false)]
        public StorageClassEnum StorageClass { get; set; } = StorageClassEnum.STANDARD;

        /// <summary>
        /// Timestamp from when the upload was initiated.
        /// </summary>
        [XmlElement(ElementName = "Initiated", IsNullable = false)]
        public DateTime Initiated
        {
            get => _Initiated;
            set => _Initiated = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        /// <summary>
        /// Checksum algorithm.
        /// </summary>
        [XmlElement(ElementName = "ChecksumAlgorithm", IsNullable = false)]
        public ChecksumAlgorithmEnum ChecksumAlgorithm { get; set; } = ChecksumAlgorithmEnum.CRC32;

        /// <summary>
        /// Flag to control whether ChecksumAlgorithm is serialized.
        /// Default value is false.
        /// </summary>
        [XmlIgnore]
        public bool IncludeChecksumAlgorithm { get; set; } = false;

        #endregion

        #region Private-Members

        private DateTime _Initiated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public Upload()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeChecksumAlgorithm()
        {
            return IncludeChecksumAlgorithm;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
