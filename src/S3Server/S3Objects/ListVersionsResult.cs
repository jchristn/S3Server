namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Result from a ListVersions operation.
    /// </summary>
    [XmlRoot(ElementName = "ListVersionsResult", IsNullable = true)]
    public class ListVersionsResult
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Name of the bucket.
        /// </summary>
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; } = null;

        /// <summary>
        /// Prefix specified in the request.
        /// </summary>
        [XmlElement(ElementName = "Prefix")]
        public string Prefix
        {
            get
            {
                return _Prefix;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) _Prefix = "";
                else _Prefix = value;
            }
        }

        /// <summary>
        /// Key marker.
        /// </summary>
        [XmlElement(ElementName = "KeyMarker")]
        public string KeyMarker { get; set; } = null;

        /// <summary>
        /// Version ID marker.
        /// </summary>
        [XmlElement(ElementName = "VersionIdMarker")]
        public string VersionIdMarker { get; set; } = null;

        /// <summary>
        /// Maximum number of keys.
        /// </summary>
        [XmlElement(ElementName = "MaxKeys")]
        public int MaxKeys
        {
            get
            {
                return _MaxKeys;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(MaxKeys));
                _MaxKeys = value;
            }
        }

        /// <summary>
        /// Indicates if the response is truncated.
        /// </summary>
        [XmlElement(ElementName = "IsTruncated")]
        public bool IsTruncated { get; set; } = false;

        /// <summary>
        /// Object versions.
        /// </summary>
        [XmlElement(ElementName = "Version")]
        public List<ObjectVersion> Versions { get; set; } = new List<ObjectVersion>();

        /// <summary>
        /// Delete markers.
        /// </summary>
        [XmlElement(ElementName = "DeleteMarker")]
        public List<DeleteMarker> DeleteMarkers { get; set; } = new List<DeleteMarker>();

        /// <summary>
        /// Next key marker for pagination when results are truncated.
        /// </summary>
        [XmlElement(ElementName = "NextKeyMarker")]
        public string NextKeyMarker { get; set; } = null;

        /// <summary>
        /// Next version ID marker for pagination when results are truncated.
        /// </summary>
        [XmlElement(ElementName = "NextVersionIdMarker")]
        public string NextVersionIdMarker { get; set; } = null;

        /// <summary>
        /// Delimiter used to group keys.
        /// </summary>
        [XmlElement(ElementName = "Delimiter")]
        public string Delimiter { get; set; } = null;

        /// <summary>
        /// Common prefixes for grouped keys when using a delimiter.
        /// </summary>
        [XmlElement(ElementName = "CommonPrefixes")]
        public CommonPrefixes CommonPrefixes { get; set; } = new CommonPrefixes();

        /// <summary>
        /// Encoding type for object keys in the response.
        /// </summary>
        [XmlElement(ElementName = "EncodingType")]
        public string EncodingType { get; set; } = null;

        /// <summary>
        /// Bucket region string.  Not included in the XML, but rather as the HTTP header x-amz-bucket-region.
        /// </summary>
        [XmlIgnore]
        public string BucketRegion { get; set; } = "us-west-1";

        #endregion

        #region Private-Members

        private int _MaxKeys = 0;
        private string _Prefix = "";

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public ListVersionsResult()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="name">Bucket name.</param>
        /// <param name="versions">Versions.</param>
        /// <param name="maxKeys">Max keys.</param>
        /// <param name="prefix">Prefix.</param>
        /// <param name="keyMarker">Key marker.</param>
        /// <param name="versionIdMarker">Version ID marker.</param>
        /// <param name="isTruncated">Is truncated.</param>
        /// <param name="deleteMarkers">Delete markers.</param>
        /// <param name="nextKeyMarker">Next key marker for pagination.</param>
        /// <param name="nextVersionIdMarker">Next version ID marker for pagination.</param>
        /// <param name="delimiter">Delimiter for grouping keys.</param>
        /// <param name="commonPrefixes">Common prefixes.</param>
        /// <param name="encodingType">Encoding type.</param>
        /// <param name="bucketRegion">Bucket region.</param>
        public ListVersionsResult(
            string name,
            List<ObjectVersion> versions,
            List<DeleteMarker> deleteMarkers,
            int maxKeys,
            string prefix = null,
            string keyMarker = null,
            string versionIdMarker = null,
            bool isTruncated = false,
            string nextKeyMarker = null,
            string nextVersionIdMarker = null,
            string delimiter = null,
            CommonPrefixes commonPrefixes = null,
            string encodingType = null,
            string bucketRegion = "us-west-1")
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            Name = name;
            Prefix = prefix;
            KeyMarker = keyMarker;
            VersionIdMarker = versionIdMarker;
            MaxKeys = maxKeys;
            IsTruncated = isTruncated;
            if (versions != null) Versions = versions;
            if (deleteMarkers != null) DeleteMarkers = deleteMarkers;
            NextKeyMarker = nextKeyMarker;
            NextVersionIdMarker = nextVersionIdMarker;
            Delimiter = delimiter;
            if (commonPrefixes != null) CommonPrefixes = commonPrefixes;
            EncodingType = encodingType;
            BucketRegion = bucketRegion;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeVersions()
        {
            return Versions != null && Versions.Count > 0;
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeDeleteMarkers()
        {
            return DeleteMarkers != null && DeleteMarkers.Count > 0;
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeNextKeyMarker()
        {
            return !String.IsNullOrEmpty(NextKeyMarker);
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeNextVersionIdMarker()
        {
            return !String.IsNullOrEmpty(NextVersionIdMarker);
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeDelimiter()
        {
            return !String.IsNullOrEmpty(Delimiter);
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeCommonPrefixes()
        {
            return CommonPrefixes != null
                && CommonPrefixes.Prefixes != null
                && CommonPrefixes.Prefixes.Count > 0;
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
