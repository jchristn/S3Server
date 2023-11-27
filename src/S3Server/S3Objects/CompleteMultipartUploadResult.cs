namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Complete multipart upload result.
    /// </summary>
    [XmlRoot(ElementName = "CompleteMultipartUploadResult")]
    public class CompleteMultipartUploadResult
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Location.
        /// </summary>
        [XmlElement(ElementName = "Location", IsNullable = true)]
        public string Location { get; set; } = null;

        /// <summary>
        /// Bucket.
        /// </summary>
        [XmlElement(ElementName = "Bucket", IsNullable = true)]
        public string Bucket { get; set; } = null;

        /// <summary>
        /// Key.
        /// </summary>
        [XmlElement(ElementName = "Key", IsNullable = true)]
        public string Key { get; set; } = null;

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

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public CompleteMultipartUploadResult()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
