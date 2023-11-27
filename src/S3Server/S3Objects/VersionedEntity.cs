namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Object version.
    /// </summary>
    [XmlInclude(typeof(ObjectVersion))]
    [XmlInclude(typeof(DeleteMarker))]
    [XmlRoot(ElementName = "Version", IsNullable = true)]
    public class VersionedEntity
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
        /// Indicates if this version is the latest version of the resource.
        /// </summary>
        [XmlElement(ElementName = "IsLatest", IsNullable = true)]
        public bool? IsLatest { get; set; } = false;

        /// <summary>
        /// Timestamp from the last modification of the resource.
        /// </summary>
        [XmlElement(ElementName = "LastModified")]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Object ETag.
        /// </summary>
        [XmlElement(ElementName = "ETag", IsNullable = true)]
        public string ETag { get; set; } = null;

        /// <summary>
        /// Content length of the object.
        /// </summary>
        [XmlElement(ElementName = "Size", IsNullable = true)]
        public long? Size
        {
            get
            {
                return _Size;
            }
            set
            {
                if (value != null && value < 0) throw new ArgumentOutOfRangeException(nameof(Size));
                _Size = value;
            }
        }

        /// <summary>
        /// The class of storage where the resource resides.
        /// Valid values are STANDARD, REDUCED_REDUNDANCY, GLACIER, STANDARD_IA, ONEZONE_IA, INTELLIGENT_TIERING, DEEP_ARCHIVE, OUTPOSTS.
        /// </summary>
        [XmlElement(ElementName = "StorageClass")]
        public StorageClassEnum StorageClass { get; set; } = StorageClassEnum.STANDARD;

        /// <summary>
        /// Object owner.
        /// </summary>
        [XmlElement(ElementName = "Owner", IsNullable = true)]
        public Owner Owner { get; set; } = null;

        #endregion

        #region Private-Members

        private long? _Size = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public VersionedEntity()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    [XmlType(TypeName = "Version")]
    public class ObjectVersion : VersionedEntity
    {
        /// <summary>
        /// Instantiate.
        /// </summary>
        public ObjectVersion()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="versionId">Version ID.</param>
        /// <param name="lastModified">Last modified.</param>
        /// <param name="isLatest">Is latest.</param>
        /// <param name="eTag">ETag.</param>
        /// <param name="size">Size.</param>
        /// <param name="owner">Owner.</param>
        /// <param name="storageClass">Storage class.  Valid values are STANDARD, REDUCED_REDUNDANCY, GLACIER, STANDARD_IA, ONEZONE_IA, INTELLIGENT_TIERING, DEEP_ARCHIVE, OUTPOSTS.</param>
        public ObjectVersion(string key, string versionId, bool isLatest, DateTime lastModified, string eTag, long? size, Owner owner, StorageClassEnum storageClass = StorageClassEnum.STANDARD)
        {
            base.Key = key;
            base.VersionId = versionId;
            base.IsLatest = isLatest;
            base.LastModified = lastModified;
            base.ETag = eTag;
            base.Size = size;
            base.StorageClass = storageClass;
            base.Owner = owner;
        }
    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    [XmlType(TypeName = "DeleteMarker")]
    public class DeleteMarker : VersionedEntity
    {
        /// <summary>
        /// Instantiate.
        /// </summary>
        public DeleteMarker()
        {
            base.Size = null;
            base.ETag = null;
        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="versionId">Version ID.</param>
        /// <param name="lastModified">Last modified.</param>
        /// <param name="isLatest">Is latest.</param>
        /// <param name="owner">Owner.</param>
        public DeleteMarker(string key, string versionId, bool isLatest, DateTime lastModified, Owner owner)
        {
            base.Key = key;
            base.VersionId = versionId;
            base.IsLatest = isLatest;
            base.LastModified = lastModified;
            base.ETag = null;
            base.Size = null;
            base.StorageClass = StorageClassEnum.STANDARD;
            base.Owner = owner;
        }
    }
}
