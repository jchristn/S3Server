namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Metadata about a deleted resource.
    /// </summary>
    [XmlRoot(ElementName = "Deleted", IsNullable = true)]
    public class Deleted
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Object key.
        /// </summary>
        [XmlElement(ElementName = "Key", IsNullable = true)]
        public string Key { get; set; } = null;

        /// <summary>
        /// The version identifier for the resource.
        /// </summary>
        [XmlElement(ElementName = "VersionId", IsNullable = true)]
        public string VersionId { get; set; } = null;

        /// <summary>
        /// Indicates if the key represents a delete marker for the resource.
        /// </summary>
        [XmlElement(ElementName = "DeleteMarker", IsNullable = true)]
        public bool? DeleteMarker { get; set; } = false;

        /// <summary>
        /// The version ID associated with the delete marker.
        /// </summary>
        [XmlElement(ElementName = "DeleteMarkerVersionId", IsNullable = true)]
        public string DeleteMarkerVersionId { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public Deleted()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="versionId">Version ID.</param>
        /// <param name="deleteMarker">Delete marker.</param>
        /// <param name="deleteMarkerVersionId">Delete marker version ID.</param>
        public Deleted(string key, string versionId, bool? deleteMarker, string deleteMarkerVersionId = null)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            Key = key;
            VersionId = versionId;
            DeleteMarker = deleteMarker;
            DeleteMarkerVersionId = deleteMarkerVersionId;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
