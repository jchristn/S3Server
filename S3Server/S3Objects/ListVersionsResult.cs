using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
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
        [XmlElement(ElementName = "Name", IsNullable = true)]
        public string Name { get; set; } = null;

        /// <summary>
        /// Prefix specified in the request.
        /// </summary>
        [XmlElement(ElementName = "Prefix", IsNullable = true)]
        public string Prefix { get; set; } = null;

        /// <summary>
        /// Key marker.
        /// </summary>
        [XmlElement(ElementName = "KeyMarker", IsNullable = true)]
        public string KeyMarker { get; set; } = null;

        /// <summary>
        /// Version ID marker.
        /// </summary>
        [XmlElement(ElementName = "VersionIdMarker", IsNullable = true)]
        public string VersionIdMarker { get; set; } = null;

        /// <summary>
        /// Maximum number of keys.
        /// </summary>
        [XmlElement(ElementName = "MaxKeys")]
        public long MaxKeys
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
        [XmlElement(ElementName = "Version", IsNullable = true)]
        public List<ObjectVersion> Versions { get; set; } = new List<ObjectVersion>();

        /// <summary>
        /// Delete markers.
        /// </summary>
        [XmlElement(ElementName = "DeleteMarker", IsNullable = true)]
        public List<DeleteMarker> DeleteMarkers { get; set; } = new List<DeleteMarker>();

        #endregion

        #region Private-Members

        private long _MaxKeys = 0;

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
        public ListVersionsResult(string name, List<ObjectVersion> versions, List<DeleteMarker> deleteMarkers, long maxKeys, string prefix = null, string keyMarker = null, string versionIdMarker = null, bool isTruncated = false)
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
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
