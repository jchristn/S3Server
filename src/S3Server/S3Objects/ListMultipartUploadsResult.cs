namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// List multipart uploads.
    /// </summary>
    [XmlRoot(ElementName = "ListMultipartUploadsResult")]
    public class ListMultipartUploadsResult
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Bucket.
        /// </summary>
        [XmlElement(ElementName = "Bucket", IsNullable = false)]
        public string Bucket { get; set; } = null;

        /// <summary>
        /// Key marker.
        /// </summary>
        [XmlElement(ElementName = "KeyMarker")]
        public string KeyMarker { get; set; } = null;

        /// <summary>
        /// Upload ID marker.
        /// </summary>
        [XmlElement(ElementName = "UploadIdMarker")]
        public string UploadIdMarker { get; set; } = null;

        /// <summary>
        /// Next key marker.
        /// </summary>
        [XmlElement(ElementName = "NextKeyMarker")]
        public string NextKeyMarker { get; set; } = null;

        /// <summary>
        /// Prefix.
        /// </summary>
        [XmlElement(ElementName = "Prefix")]
        public string Prefix { get; set; } = null;

        /// <summary>
        /// Delimiter.
        /// </summary>
        [XmlElement(ElementName = "Delimiter")]
        public string Delimiter { get; set; } = null;

        /// <summary>
        /// Next upload ID marker.
        /// </summary>
        [XmlElement(ElementName = "NextUploadIdMarker")]
        public string NextUploadIdMarker { get; set; } = null;

        /// <summary>
        /// Max uploads.
        /// </summary>
        [XmlElement(ElementName = "MaxUploads", IsNullable = false)]
        public int MaxUploads
        {
            get
            {
                return _MaxUploads;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(MaxUploads));
                _MaxUploads = value;
            }
        }

        /// <summary>
        /// Flag indicating if the results are truncated.
        /// </summary>
        [XmlElement(ElementName = "IsTruncated", IsNullable = false)]
        public bool IsTruncated { get; set; } = false;

        /// <summary>
        /// Uploads.
        /// </summary>
        [XmlElement(ElementName = "Upload")]
        public List<Upload> Uploads
        {
            get
            {
                return _Uploads;
            }
            set
            {
                if (value == null) _Uploads = new List<Upload>();
                else _Uploads = value;
            }
        }

        /// <summary>
        /// Common prefixes.
        /// </summary>
        [XmlElement(ElementName = "CommonPrefixes")]
        public CommonPrefixes CommonPrefixes
        {
            get
            {
                return _CommonPrefixes;
            }
            set
            {
                _CommonPrefixes = value;
            }
        }

        /// <summary>
        /// Encoding type used to encode object key names in the XML response.
        /// Valid value is "url" for URL encoding.
        /// </summary>
        [XmlElement(ElementName = "EncodingType")]
        public string EncodingType { get; set; } = null;

        #endregion

        #region Private-Members

        private int _MaxUploads = 1000;
        private List<Upload> _Uploads = new List<Upload>();
        private CommonPrefixes _CommonPrefixes = new CommonPrefixes();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public ListMultipartUploadsResult()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="bucket">Bucket.</param>
        /// <param name="key">Key.</param>
        /// <param name="uploadId">Upload ID.</param>
        public ListMultipartUploadsResult(string bucket, string key, string uploadId)
        {
            if (String.IsNullOrEmpty(bucket)) throw new ArgumentNullException(nameof(bucket));
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (String.IsNullOrEmpty(uploadId)) throw new ArgumentNullException(nameof(uploadId));
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeUploads()
        {
            return _Uploads != null && _Uploads.Count > 0;
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeCommonPrefixes()
        {
            return _CommonPrefixes != null
                && _CommonPrefixes.Prefixes != null
                && _CommonPrefixes.Prefixes.Count > 0;
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeEncodingType()
        {
            return !String.IsNullOrEmpty(EncodingType);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
