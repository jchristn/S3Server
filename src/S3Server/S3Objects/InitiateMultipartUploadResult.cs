namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Initiate multipart upload result.
    /// </summary>
    [XmlRoot(ElementName = "InitiateMultipartUploadResult")]
    public class InitiateMultipartUploadResult
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Bucket.
        /// </summary>
        [XmlElement(ElementName = "Bucket", IsNullable = false)]
        public string Bucket { get; set; } = null;

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

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public InitiateMultipartUploadResult()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="bucket">Bucket.</param>
        /// <param name="key">Key.</param>
        /// <param name="uploadId">Upload ID.</param>
        public InitiateMultipartUploadResult(string bucket, string key, string uploadId)
        {
            if (String.IsNullOrEmpty(bucket)) throw new ArgumentNullException(nameof(bucket));
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (String.IsNullOrEmpty(uploadId)) throw new ArgumentNullException(nameof(uploadId));
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
