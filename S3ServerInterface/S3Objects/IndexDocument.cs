using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Parameters for the object that should serve as the index document for the bucket.
    /// </summary>
	[XmlRoot(ElementName = "IndexDocument", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class IndexDocument
    {
        /// <summary>
        /// The suffix to use for the index document.
        /// </summary>
        [XmlElement(ElementName = "Suffix")]
        public string Suffix { get; set; }
    }
}
