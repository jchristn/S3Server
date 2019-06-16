using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{ 
    // built using https://xmltocsharp.azurewebsites.net/

    [XmlRoot(ElementName = "Tag")]
    public class Tag
    {
        [XmlElement(ElementName = "Key")]
        public string Key { get; set; }
        [XmlElement(ElementName = "Value")]
        public string Value { get; set; }
    }

    [XmlRoot(ElementName = "TagSet")]
    public class TagSet
    {
        [XmlElement(ElementName = "Tag")]
        public List<Tag> Tag { get; set; }
    }

    [XmlRoot(ElementName = "Tagging")]
    public class Tagging
    {
        [XmlElement(ElementName = "TagSet")]
        public TagSet TagSet { get; set; }
    } 
}
