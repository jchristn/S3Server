using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Result from a ListBucket operation.
    /// </summary>
    [XmlRoot(ElementName = "ListBucketResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class ListBucketResult
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
        /// Number of keys.
        /// </summary>
        [XmlElement(ElementName = "KeyCount", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public long KeyCount { get; set; }
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
        /// The next continuation token to supply to continue the query.
        /// </summary>
        [XmlElement(ElementName = "NextContinuationToken", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string NextContinuationToken { get; set; }
        /// <summary>
        /// Bucket contents.
        /// </summary>
        [XmlElement(ElementName = "Contents", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public List<Contents> Contents { get; set; }
        /// <summary>
        /// XML namespace.
        /// </summary>
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}
