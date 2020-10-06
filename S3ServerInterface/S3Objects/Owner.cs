using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Resource owner.
    /// </summary>
    [XmlRoot(ElementName = "Owner", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class Owner
    {
        /// <summary>
        /// ID of the owner.
        /// </summary>
        [XmlElement(ElementName = "ID", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string ID { get; set; }

        /// <summary>
        /// Display name of the owner.
        /// </summary>
        [XmlElement(ElementName = "DisplayName", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Owner()
        {
            ID = null;
            DisplayName = null;
        }
    }
}
