using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// A grant allowing a grantee permissiont to a resource.
    /// </summary>
    [XmlRoot(ElementName = "Grant", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class Grant
    {
        /// <summary>
        /// The recipient of the permission.
        /// </summary>
        [XmlElement(ElementName = "Grantee")]
        public Grantee Grantee { get; set; }

        /// <summary>
        /// The permission given to the recipient.
        /// </summary>
        [XmlElement(ElementName = "Permission")]
        public string Permission { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Grant()
        {
            Grantee = new Grantee();
            Permission = null;
        }
    }
}
