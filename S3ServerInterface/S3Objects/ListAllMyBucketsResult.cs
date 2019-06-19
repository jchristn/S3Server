using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    // built using https://xmltocsharp.azurewebsites.net/
     
    /*
     * Implemented in AccessControlPolicy
     * 
    [XmlRoot(ElementName = "Owner", Namespace = "http://s3.amazonaws.com/doc/2006-03-01")]
    public class Owner
    {
        [XmlElement(ElementName = "ID", Namespace = "http://s3.amazonaws.com/doc/2006-03-01")]
        public string ID { get; set; }
        [XmlElement(ElementName = "DisplayName", Namespace = "http://s3.amazonaws.com/doc/2006-03-01")]
        public string DisplayName { get; set; }
    }
     */

    [XmlRoot(ElementName = "Bucket", Namespace = "http://s3.amazonaws.com/doc/2006-03-01")]
    public class Bucket
    {
        [XmlElement(ElementName = "Name", Namespace = "http://s3.amazonaws.com/doc/2006-03-01")]
        public string Name { get; set; }
        [XmlElement(ElementName = "CreationDate", Namespace = "http://s3.amazonaws.com/doc/2006-03-01")]
        public string CreationDate { get; set; }
    }

    [XmlRoot(ElementName = "Buckets", Namespace = "http://s3.amazonaws.com/doc/2006-03-01")]
    public class Buckets
    {
        [XmlElement(ElementName = "Bucket", Namespace = "http://s3.amazonaws.com/doc/2006-03-01")]
        public List<Bucket> Bucket { get; set; }
    }

    [XmlRoot(ElementName = "ListAllMyBucketsResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01")]
    public class ListAllMyBucketsResult
    {
        [XmlElement(ElementName = "Owner", Namespace = "http://s3.amazonaws.com/doc/2006-03-01")]
        public Owner Owner { get; set; }
        [XmlElement(ElementName = "Buckets", Namespace = "http://s3.amazonaws.com/doc/2006-03-01")]
        public Buckets Buckets { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    } 
}
