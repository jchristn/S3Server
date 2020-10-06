using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Metadata about a deleted resource.
    /// </summary>
    [XmlRoot(ElementName = "Deleted", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class Deleted
    {
        /// <summary>
        /// Object key.
        /// </summary>
        [XmlElement(ElementName = "Key", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string Key { get; set; }

        /// <summary>
        /// The version identifier for the resource.
        /// </summary>
        [XmlElement(ElementName = "VersionId", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string VersionId { get; set; }

        /// <summary>
        /// Indicates if the key represents a delete marker for the resource.
        /// </summary>
        [XmlElement(ElementName = "DeleteMarker", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string DeleteMarker { get; set; }

        /// <summary>
        /// The version ID associated with the delete marker.
        /// </summary>
        [XmlElement(ElementName = "DeleteMarkerVersionId", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string DeleteMarkerVersionId { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Deleted()
        {
            Key = null;
            VersionId = null;
            DeleteMarker = null;
            DeleteMarkerVersionId = null;
        }
    }
}
