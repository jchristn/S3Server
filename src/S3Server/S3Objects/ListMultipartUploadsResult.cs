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
        [XmlElement(ElementName = "KeyMarker", IsNullable = true)]
        public string KeyMarker { get; set; } = null;

        /// <summary>
        /// Upload ID marker.
        /// </summary>
        [XmlElement(ElementName = "UploadIdMarker", IsNullable = true)]
        public string UploadIdMarker { get; set; } = null;

        /// <summary>
        /// Next key marker.
        /// </summary>
        [XmlElement(ElementName = "NextKeyMarker", IsNullable = true)]
        public string NextKeyMarker { get; set; } = null;

        /// <summary>
        /// Prefix.
        /// </summary>
        [XmlElement(ElementName = "Prefix", IsNullable = true)]
        public string Prefix { get; set; } = null;

        /// <summary>
        /// Delimiter.
        /// </summary>
        [XmlElement(ElementName = "Delimiter", IsNullable = true)]
        public string Delimiter { get; set; } = null;

        /// <summary>
        /// Next upload ID marker.
        /// </summary>
        [XmlElement(ElementName = "NextUploadIdMarker", IsNullable = true)]
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
        [XmlElement(ElementName = "Upload", IsNullable = true)]
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
        [XmlElement(ElementName = "CommonPrefixes", IsNullable = true)]
        public CommonPrefixes CommonPrefixes
        {
            get
            {
                return _CommonPrefixes;
            }
            set
            {
                if (value == null) _CommonPrefixes = new CommonPrefixes();
                else _CommonPrefixes = value;
            }
        }

        /// <summary>
        /// Encoding type.
        /// </summary>
        [XmlElement(ElementName = "EncodingType", IsNullable = false)]
        public EncodingTypeEnum EncodingType { get; set; } = EncodingTypeEnum.Key;

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

        #endregion

        #region Private-Methods

        #endregion
    }
}
