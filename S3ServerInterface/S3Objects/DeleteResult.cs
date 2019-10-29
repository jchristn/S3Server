using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Result from a delete operation.
    /// </summary>
    [XmlRoot(ElementName = "DeleteResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class DeleteResult
    {
        /// <summary>
        /// List of deleted resources.
        /// </summary>
        [XmlElement(ElementName = "Deleted", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public List<Deleted> Deleted { get; set; }
        /// <summary>
        /// List of errors encountered during the operation.
        /// </summary>
        [XmlElement(ElementName = "Error", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public List<Error> Error { get; set; }
        /// <summary>
        /// XML namespace attribute.
        /// </summary>
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public DeleteResult()
        {
            Deleted = new List<Deleted>();
            Error = new List<Error>();
        }
    }
}
