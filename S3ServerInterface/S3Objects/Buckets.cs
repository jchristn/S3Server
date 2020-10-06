using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// A collection of buckets.
    /// </summary>
    [XmlRoot(ElementName = "Buckets", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class Buckets
    {
        /// <summary>
        /// A list of individual buckets.
        /// </summary>
        [XmlElement(ElementName = "Bucket", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public List<Bucket> Bucket { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Buckets()
        {
            Bucket = new List<Bucket>();
        }
    }
}
