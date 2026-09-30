namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Object version, serialized as a Version element.
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
}
