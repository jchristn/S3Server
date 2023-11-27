namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /*
     * Removed Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"
     * From each XmlRoot attribute
     * 
     */

    /// <summary>
    /// Result from a ListBucket operation.
    /// </summary>
    [XmlRoot(ElementName = "ListBucketResult", IsNullable = true)]
    public class ListBucketResult
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Name of the bucket.
        /// </summary>
        [XmlElement(ElementName = "Name", IsNullable = true)]
        public string Name { get; set; } = null;

        /// <summary>
        /// Prefix specified in the request.
        /// </summary>
        [XmlElement(ElementName = "Prefix", IsNullable = true)]
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
        /// Marker.
        /// </summary>
        [XmlElement(ElementName = "Marker", IsNullable = true)]
        public string Marker { get; set; } = null;

        /// <summary>
        /// Number of keys.
        /// </summary>
        [XmlElement(ElementName = "KeyCount")]
        public int KeyCount
        {
            get
            {
                return _KeyCount;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(KeyCount));
                _KeyCount = value;
            }
        }

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
        /// Delimiter.
        /// </summary>
        [XmlElement(ElementName = "Delimiter", IsNullable = true)]
        public string Delimiter
        {
            get
            {
                return _Delimiter;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) _Delimiter = "/";
                else _Delimiter = value;
            }
        }

        /// <summary>
        /// Encoding type.
        /// </summary>
        [XmlElement(ElementName = "EncodingType")]
        public string EncodingType { get; set; } = "url";

        /// <summary>
        /// Indicates if the response is truncated.
        /// </summary>
        [XmlElement(ElementName = "IsTruncated")]
        public bool IsTruncated { get; set; } = false;

        /// <summary>
        /// The next continuation token to supply to continue the query.
        /// </summary>
        [XmlElement(ElementName = "NextContinuationToken", IsNullable = true)]
        public string NextContinuationToken { get; set; } = null;

        /// <summary>
        /// Bucket contents.
        /// </summary>
        [XmlElement(ElementName = "Contents", IsNullable = true)]
        public List<ObjectMetadata> Contents { get; set; } = new List<ObjectMetadata>();

        /// <summary>
        /// Common prefixes.
        /// </summary>
        [XmlElement(ElementName = "CommonPrefixes", IsNullable = true)]
        public CommonPrefixes CommonPrefixes { get; set; } = new CommonPrefixes();

        /// <summary>
        /// Bucket region string.  Not included in the XML, but rather as the HTTP header x-amz-bucket-region.
        /// </summary>
        public string BucketRegion { get; set; } = "us-west-1";

        #endregion

        #region Private-Members

        private int _KeyCount = 0;
        private int _MaxKeys = 1000;
        private string _Prefix = "";
        private string _Delimiter = "/";

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public ListBucketResult()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="contents">Contents.</param>
        /// <param name="keyCount">Key count.</param>
        /// <param name="maxKeys">Max keys.</param>
        /// <param name="prefix">Prefix.</param>
        /// <param name="marker">Marker.</param>
        /// <param name="delimiter">Delimiter.</param>
        /// <param name="isTruncated">Is truncated.</param>
        /// <param name="nextToken">Next continuation token.</param>
        /// <param name="prefixes">Prefixes</param>
        /// <param name="bucketRegion">Bucket region.</param>
        public ListBucketResult(
            string name,
            List<ObjectMetadata> contents,
            int keyCount,
            int maxKeys,
            string prefix = null,
            string marker = null,
            string delimiter = null,
            bool isTruncated = false,
            string nextToken = null,
            CommonPrefixes prefixes = null,
            string bucketRegion = "us-west-1")
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            Name = name;
            Prefix = prefix;
            Marker = marker;
            KeyCount = keyCount;
            MaxKeys = maxKeys;
            Delimiter = delimiter;
            IsTruncated = isTruncated;
            NextContinuationToken = nextToken;
            if (contents != null) Contents = contents;
            if (prefixes != null) CommonPrefixes = prefixes;
            BucketRegion = bucketRegion;
        }

        #endregion

        #region Public-Methods

        /*
         * See https://stackoverflow.com/a/51440611 for information on ShouldSerialize methods
         * and how they are used by XmlSerializer
         */

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean</returns>
    public bool ShouldSerializeMarker()
        {
            return !String.IsNullOrEmpty(Marker);
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean</returns>
        public bool ShouldSerializeNextContinuationToken()
        {
            return !String.IsNullOrEmpty(NextContinuationToken);
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean</returns>
        public bool ShouldSerializeCommonPrefixes()
        {
            return (
                CommonPrefixes != null
                && CommonPrefixes.Prefixes != null
                && CommonPrefixes.Prefixes.Count > 0);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
