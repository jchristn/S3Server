using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Delete marker representing a deleted resource.
    /// </summary>
    [XmlRoot(ElementName = "DeleteMarker", IsNullable = true)]
    public class DeleteMarker
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
        public DateTime LastModified { get; set; } = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// Information about the resource owner.
        /// </summary>
        [XmlElement(ElementName = "Owner", IsNullable = true)]
        public Owner Owner { get; set; } = new Owner();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public DeleteMarker()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="versionId">Version ID.</param>
        /// <param name="isLatest">Is latest.</param>
        /// <param name="lastModified">Last modified.</param>
        /// <param name="owner">Owner.</param>
        public DeleteMarker(string key, string versionId, bool? isLatest, DateTime lastModified, Owner owner)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            Key = key;
            VersionId = versionId;
            IsLatest = isLatest;
            LastModified = lastModified;
            Owner = owner;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
