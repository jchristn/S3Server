namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Object metadata.
    /// </summary>
    [XmlRoot(ElementName = "Contents")]
    public class ObjectMetadata
    {
        #region Public-Members

        /// <summary>
        /// Object key.
        /// </summary>
        [XmlElement(ElementName = "Key")]
        public string Key { get; set; } = null;

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
        /// ETag of the resource.
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

        /// <summary>
        /// Content type.
        /// </summary>
        [XmlIgnore]
        public string ContentType { get; set; } = "application/octet-stream";

        /// <summary>
        /// The size in bytes of the resource.
        /// </summary>
        [XmlElement(ElementName = "Size")]
        public long Size
        {
            get
            {
                return _Size;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Size));
                _Size = value;
            }
        }

        /// <summary>
        /// The class of storage where the resource resides.
        /// </summary>
        [XmlElement(ElementName = "StorageClass")]
        public StorageClassEnum? StorageClass { get; set; } = StorageClassEnum.STANDARD;

        /// <summary>
        /// Object owner.
        /// </summary>
        [XmlElement(ElementName = "Owner")]
        public Owner Owner { get; set; } = new Owner();

        /// <summary>
        /// Restore status for archived objects, returned through the x-amz-restore response header.
        /// </summary>
        [XmlIgnore]
        public RestoreStatus RestoreStatus { get; set; } = null;

        #endregion

        #region Private-Members

        private long _Size = 0;
        private string _ETag = null;
        private DateTime _LastModified = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public ObjectMetadata()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="lastModified">Last modified.</param>
        /// <param name="eTag">ETag.</param>
        /// <param name="size">Size.</param>
        /// <param name="owner">Owner.</param>
        /// <param name="storageClass">Storage class.</param>
        public ObjectMetadata(string key, DateTime lastModified, string eTag, long size, Owner owner, StorageClassEnum storageClass = StorageClassEnum.STANDARD)
        {
            Key = key;
            LastModified = lastModified;
            ETag = eTag;
            Size = size;
            StorageClass = storageClass;
            Owner = owner;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Helper method for XML serialization.  Owner is omitted when null, and from ListObjectsV2 responses unless the
        /// request specified fetch-owner=true, as Amazon S3 does.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeOwner()
        {
            return Owner != null && !ResponseSerializationContext.SuppressListOwner;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
