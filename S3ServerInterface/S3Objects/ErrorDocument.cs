using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Object that should serve as the document to return should an error be encountered.
    /// </summary>
    [XmlRoot(ElementName = "ErrorDocument", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class ErrorDocument
    {
        /// <summary>
        /// The key of the object that should serve as the error document.
        /// </summary>
        [XmlElement(ElementName = "Key")]
        public string Key { get; set; }
    }
}
