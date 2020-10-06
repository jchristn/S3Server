using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Object.
    /// </summary>
    [XmlRoot(ElementName = "Object", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class Object
    {
        /// <summary>
        /// Object key.
        /// </summary>
        [XmlElement(ElementName = "Key", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string Key { get; set; }

        /// <summary>
        /// Version ID for the object.
        /// </summary>
        [XmlElement(ElementName = "VersionId", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string VersionId { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Object()
        {
            Key = null;
            VersionId = null;
        }
    }
}
