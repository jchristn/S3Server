namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Object metadata.
    /// </summary>
    [XmlRoot(ElementName = "Contents", IsNullable = true)]
    public class ObjectMetadata
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Object key.
        /// </summary>
        [XmlElement(ElementName = "Key", IsNullable = true)]
        public string Key { get; set; } = null;

        /// <summary>
        /// Timestamp from the last modification of the resource.
        /// </summary>
        [XmlElement(ElementName = "LastModified")]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ETag of the resource.
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
        /// Content type.
        /// </summary>
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
        [XmlElement(ElementName = "StorageClass", IsNullable = true)]
        public StorageClassEnum? StorageClass { get; set; } = StorageClassEnum.STANDARD;

        /// <summary>
        /// Object owner.
        /// </summary>
        [XmlElement(ElementName = "Owner", IsNullable = true)]
        public Owner Owner { get; set; } = new Owner();

        #endregion

        #region Private-Members

        private long _Size = 0;
        private string _ETag = null;

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

        #endregion

        #region Private-Methods

        #endregion
    }
}
