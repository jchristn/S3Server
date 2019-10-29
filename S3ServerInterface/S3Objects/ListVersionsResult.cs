using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Result from a ListVersions operation.
    /// </summary>
    [XmlRoot(ElementName = "ListVersionsResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class ListVersionsResult
    {
        /// <summary>
        /// Name of the bucket.
        /// </summary>
        [XmlElement(ElementName = "Name", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string Name { get; set; }
        /// <summary>
        /// Prefix specified in the request.
        /// </summary>
        [XmlElement(ElementName = "Prefix", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string Prefix { get; set; }
        /// <summary>
        /// Key marker.
        /// </summary>
        [XmlElement(ElementName = "KeyMarker", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string KeyMarker { get; set; }
        /// <summary>
        /// Version ID marker.
        /// </summary>
        [XmlElement(ElementName = "VersionIdMarker", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string VersionIdMarker { get; set; }
        /// <summary>
        /// Maximum number of keys.
        /// </summary>
        [XmlElement(ElementName = "MaxKeys", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public long MaxKeys { get; set; }
        /// <summary>
        /// Indicates if the response is truncated.
        /// </summary>
        [XmlElement(ElementName = "IsTruncated", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public bool IsTruncated { get; set; }
        /// <summary>
        /// Object versions.
        /// </summary>
        [XmlElement(ElementName = "Version", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public List<Version> Version { get; set; }
        /// <summary>
        /// Delete markers.
        /// </summary>
        [XmlElement(ElementName = "DeleteMarker", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public List<DeleteMarker> DeleteMarker { get; set; }
        /// <summary>
        /// XML namespace.
        /// </summary>
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public ListVersionsResult()
        {
            DeleteMarker = new List<DeleteMarker>();
            Version = new List<Version>();
        }
    }
}
