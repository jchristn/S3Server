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
        public DateTime LastModified
        {
            get => _LastModified;
            set => _LastModified = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        /// <summary>
        /// Object ETag.
        /// </summary>
        [XmlElement(ElementName = "ETag", IsNullable = true)]
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

        /// <summary>
        /// Content length of the object.
        /// </summary>
        [XmlElement(ElementName = "Size")]
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
        /// Determine whether to serialize Size.
        /// </summary>
        public bool ShouldSerializeSize()
        {
            return Size.HasValue;
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
        private string _ETag = null;
        private DateTime _LastModified = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

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
}
