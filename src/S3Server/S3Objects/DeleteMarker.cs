namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Delete marker, serialized as a DeleteMarker element.
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
