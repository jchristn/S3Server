using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    [XmlRoot(ElementName = "VersioningConfiguration", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class VersioningConfiguration
    {
        [XmlElement(ElementName = "Status", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string Status { get; set; }
        [XmlElement(ElementName = "MfaDelete", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public string MfaDelete { get; set; }

        public VersioningConfiguration()
        {

        }
    }
}
