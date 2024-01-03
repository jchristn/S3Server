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
        /// Checksum algorithm.
        /// </summary>
        [XmlElement(ElementName = "ChecksumAlgorithm", IsNullable = false)]
        public ChecksumAlgorithmEnum ChecksumAlgorithm { get; set; } = ChecksumAlgorithmEnum.CRC32;

        /// <summary>
        /// Timestamp from when the upload was initiated.
        /// </summary>
        [XmlElement(ElementName = "Initiated", IsNullable = false)]
        public DateTime Initiated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Key.
        /// </summary>
        [XmlElement(ElementName = "Key", IsNullable = false)]
        public string Key { get; set; } = null;

        /// <summary>
        /// Initiator of the upload.
        /// </summary>
        [XmlElement(ElementName = "Initiator", IsNullable = true)]
        public Owner Initiator { get; set; } = new Owner();

        /// <summary>
        /// Owner of the object.
        /// </summary>
        [XmlElement(ElementName = "Owner", IsNullable = true)]
        public Owner Owner { get; set; } = new Owner();

        /// <summary>
        /// Storage class.
        /// </summary>
        [XmlElement(ElementName = "StorageClass", IsNullable = false)]
        public StorageClassEnum StorageClass { get; set; } = StorageClassEnum.STANDARD;

        /// <summary>
        /// Upload ID.
        /// </summary>
        [XmlElement(ElementName = "UploadId", IsNullable = false)]
        public string UploadId { get; set; } = null;

        #endregion

        #region Private-Members

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

        #endregion

        #region Private-Methods

        #endregion
    }
}
