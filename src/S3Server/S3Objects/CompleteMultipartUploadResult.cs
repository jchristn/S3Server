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
        [XmlElement(ElementName = "Location")]
        public string Location { get; set; } = null;

        /// <summary>
        /// Bucket.
        /// </summary>
        [XmlElement(ElementName = "Bucket")]
        public string Bucket { get; set; } = null;

        /// <summary>
        /// Key.
        /// </summary>
        [XmlElement(ElementName = "Key")]
        public string Key { get; set; } = null;

        /// <summary>
        /// Checksum from CRC32.
        /// </summary>
        [XmlElement(ElementName = "ChecksumCRC32")]
        public string ChecksumCRC32 { get; set; } = null;

        /// <summary>
        /// Checksum from CRC32C.
        /// </summary>
        [XmlElement(ElementName = "ChecksumCRC32C")]
        public string ChecksumCRC32C { get; set; } = null;

        /// <summary>
        /// Checksum from SHA1.
        /// </summary>
        [XmlElement(ElementName = "ChecksumSHA1")]
        public string ChecksumSHA1 { get; set; } = null;

        /// <summary>
        /// Checksum from SHA256.
        /// </summary>
        [XmlElement(ElementName = "ChecksumSHA256")]
        public string ChecksumSHA256 { get; set; } = null;

        /// <summary>
        /// ETag.
        /// </summary>
        [XmlElement(ElementName = "ETag")]
        public string ETag
        {
            get
            {
                return _ETag;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    value = value.Trim();
                    if (!value.StartsWith("\"")) value = "\"" + value;
                    if (!value.EndsWith("\"")) value = value + "\"";
                }

                _ETag = value;
            }
        }

        #endregion

        #region Private-Members

        private string _ETag = null;

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

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeChecksumCRC32()
        {
            return !String.IsNullOrEmpty(ChecksumCRC32);
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeChecksumCRC32C()
        {
            return !String.IsNullOrEmpty(ChecksumCRC32C);
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeChecksumSHA1()
        {
            return !String.IsNullOrEmpty(ChecksumSHA1);
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeChecksumSHA256()
        {
            return !String.IsNullOrEmpty(ChecksumSHA256);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
