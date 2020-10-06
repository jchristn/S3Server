using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Delete marker representing a deleted resource.
    /// </summary>
    [XmlRoot(ElementName = "DeleteMarker", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class DeleteMarker
    {
        /// <summary>
        /// Object key.
        /// </summary>
        [XmlElement(ElementName = "Key", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string Key { get; set; }

        /// <summary>
        /// The version identifier for the resource.
        /// </summary>
        [XmlElement(ElementName = "VersionId", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public string VersionId { get; set; }

        /// <summary>
        /// Indicates if this version is the latest version of the resource.
        /// </summary>
        [XmlElement(ElementName = "IsLatest", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public bool IsLatest { get; set; }

        /// <summary>
        /// Timestamp from the last modification of the resource.
        /// </summary>
        [XmlElement(ElementName = "LastModified", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Information about the resource owner.
        /// </summary>
        [XmlElement(ElementName = "Owner", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public Owner Owner { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public DeleteMarker()
        {
            Key = null;
            VersionId = null;
            IsLatest = false;
            LastModified = DateTime.Now.ToUniversalTime();
            Owner = new Owner();
        }
    }
}
