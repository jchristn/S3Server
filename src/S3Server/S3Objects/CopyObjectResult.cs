namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Result of a CopyObject operation (PUT with the x-amz-copy-source header).
    /// Returned by the Object.Copy callback and serialized as the response body.
    /// </summary>
    [XmlRoot(ElementName = "CopyObjectResult")]
    public class CopyObjectResult
    {
        #region Public-Members

        /// <summary>
        /// Timestamp from the last modification of the new object, in UTC.
        /// Default is the time of instantiation.
        /// </summary>
        [XmlElement(ElementName = "LastModified")]
        public DateTime LastModified
        {
            get => _LastModified;
            set => _LastModified = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        /// <summary>
        /// ETag of the new object.  Surrounding double quotes are added if not supplied.
        /// Null is permitted but most clients expect a value.
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
        public CopyObjectResult()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="eTag">ETag of the new object.</param>
        /// <param name="lastModified">Last modified timestamp of the new object.</param>
        public CopyObjectResult(string eTag, DateTime lastModified)
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
