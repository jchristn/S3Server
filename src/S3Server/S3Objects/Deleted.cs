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
        #region Public-Members

        /// <summary>
        /// Object key.
        /// </summary>
        [XmlElement(ElementName = "Key", IsNullable = false)]
        public string Key { get; set; } = null;

        /// <summary>
        /// The version identifier for the resource.
        /// Null (the default) or empty omits the element, as Amazon S3 does for unversioned deletes.
        /// </summary>
        [XmlElement(ElementName = "VersionId", IsNullable = false)]
        public string VersionId { get; set; } = null;

        /// <summary>
        /// Indicates if the key represents a delete marker for the resource.
        /// The element is only serialized when the value is true.  Default is null.
        /// </summary>
        [XmlElement(ElementName = "DeleteMarker")]
        public bool? DeleteMarker { get; set; } = null;

        /// <summary>
        /// The version ID associated with the delete marker.
        /// Null (the default) or empty omits the element.
        /// </summary>
        [XmlElement(ElementName = "DeleteMarkerVersionId", IsNullable = false)]
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

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeVersionId()
        {
            return !String.IsNullOrEmpty(VersionId);
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeDeleteMarker()
        {
            return DeleteMarker == true;
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeDeleteMarkerVersionId()
        {
            return !String.IsNullOrEmpty(DeleteMarkerVersionId);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
