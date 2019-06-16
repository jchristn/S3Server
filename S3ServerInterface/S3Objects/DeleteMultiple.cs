using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    // built using https://xmltocsharp.azurewebsites.net/

    [XmlRoot(ElementName = "Object")]
    public class Object
    {
        [XmlElement(ElementName = "Key")]
        public string Key { get; set; }
        [XmlElement(ElementName = "VersionId")]
        public string VersionId { get; set; }
    }

    [XmlRoot(ElementName = "Delete")]
    public class Delete
    {
        [XmlElement(ElementName = "Quiet")]
        public string Quiet { get; set; }
        [XmlElement(ElementName = "Object")]
        public List<Object> Object { get; set; }
    }
}
