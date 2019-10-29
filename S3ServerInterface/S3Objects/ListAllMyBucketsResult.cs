using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Result from a ListAllMyBuckets request.
    /// </summary>
    [XmlRoot(ElementName = "ListAllMyBucketsResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class ListAllMyBucketsResult
    {
        /// <summary>
        /// Bucket owner.
        /// </summary>
        [XmlElement(ElementName = "Owner", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public Owner Owner { get; set; }
        /// <summary>
        /// Buckets owned by the user.
        /// </summary>
        [XmlElement(ElementName = "Buckets", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public Buckets Buckets { get; set; }
        /// <summary>
        /// XML namespace.
        /// </summary>
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}
