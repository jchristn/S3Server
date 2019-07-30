using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    [XmlRoot(ElementName = "Delete", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class DeleteMultiple
    {
        [XmlElement(ElementName = "Quiet", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
        public bool Quiet { get; set; }
        [XmlElement(ElementName = "Object", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
        public List<Object> Object { get; set; }
    }
}
