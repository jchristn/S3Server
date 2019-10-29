using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Request to delete multiple resources.
    /// </summary>
    [XmlRoot(ElementName = "Delete", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class DeleteMultiple
    {
        /// <summary>
        /// Enable or disable quiet deletion.
        /// </summary>
        [XmlElement(ElementName = "Quiet", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public bool Quiet { get; set; }
        /// <summary>
        /// List of objects to delete.
        /// </summary>
        [XmlElement(ElementName = "Object", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public List<Object> Object { get; set; }
    }
}
