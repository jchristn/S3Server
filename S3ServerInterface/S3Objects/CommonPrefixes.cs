using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Common prefixes.
    /// </summary>
    [XmlRoot(ElementName = "CommonPrefixes", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class CommonPrefixes
    {
        /// <summary>
        /// Prefix.
        /// </summary>
        [XmlElement(ElementName = "Prefix", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public List<string> Prefix { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public CommonPrefixes()
        {
            Prefix = new List<string>();
        }
    }
}
