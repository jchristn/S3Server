namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Delete marker, serialized as a DeleteMarker element.
    /// As Amazon S3 does, a delete marker serializes only Key, VersionId, IsLatest, LastModified, and Owner.
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

        /// <summary>
        /// Determine whether to serialize StorageClass.  Always false, because Amazon S3 does not send StorageClass for
        /// a delete marker.
        /// </summary>
        /// <returns>False.</returns>
        public override bool ShouldSerializeStorageClass()
        {
            return false;
        }
    }
}
