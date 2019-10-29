using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Amazon S3 bucket.
    /// </summary>
    [XmlRoot(ElementName = "Bucket", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class Bucket
    {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        [XmlElement(ElementName = "Name", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string Name { get; set; }
        /// <summary>
        /// The timestamp from bucket creation.
        /// </summary>
        [XmlElement(ElementName = "CreationDate", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public DateTime CreationDate { get; set; }
    }
}
