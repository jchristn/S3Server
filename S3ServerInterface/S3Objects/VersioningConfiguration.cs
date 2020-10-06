using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Bucket versioning configuration.
    /// </summary>
    [XmlRoot(ElementName = "VersioningConfiguration", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class VersioningConfiguration
    {
        /// <summary>
        /// Status of the versioning configuration.
        /// </summary>
        [XmlElement(ElementName = "Status", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string Status { get; set; }

        /// <summary>
        /// Indicates if multi-factor authentication is enabled for delete operations.
        /// </summary>
        [XmlElement(ElementName = "MfaDelete", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string MfaDelete { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public VersioningConfiguration()
        {
            Status = null;
            MfaDelete = null;
        }
    }
}
