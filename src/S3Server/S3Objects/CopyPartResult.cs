namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Result of an UploadPartCopy operation (PUT with partNumber, uploadId, and the x-amz-copy-source header).
    /// Returned by the Object.UploadPartCopy callback and serialized as the response body.
    /// </summary>
    [XmlRoot(ElementName = "CopyPartResult")]
    public class CopyPartResult
    {
        #region Public-Members

        /// <summary>
        /// Timestamp from the last modification of the part, in UTC.
        /// Default is the time of instantiation.
        /// </summary>
        [XmlElement(ElementName = "LastModified")]
        public DateTime LastModified
        {
            get => _LastModified;
            set => _LastModified = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        /// <summary>
        /// ETag of the part.  Surrounding double quotes are added if not supplied.
        /// Clients use this value when completing the multipart upload.
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
        private DateTime _LastModified = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public CopyPartResult()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="eTag">ETag of the part.</param>
        /// <param name="lastModified">Last modified timestamp of the part.</param>
        public CopyPartResult(string eTag, DateTime lastModified)
        {
            ETag = eTag;
            LastModified = lastModified;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeETag()
        {
            return !String.IsNullOrEmpty(ETag);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
