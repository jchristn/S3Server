using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    // built using https://xmltocsharp.azurewebsites.net/

    [XmlRoot(ElementName = "Error")]
    public class Error
    {
        [XmlElement(ElementName = "Code")]
        public ErrorCode Code { get; set; }
        [XmlElement(ElementName = "Message")]
        public string Message { get; set; }
        [XmlElement(ElementName = "Key")]
        public string Key { get; set; }
        [XmlElement(ElementName = "Resource")]
        public string Resource { get; set; }
        [XmlElement(ElementName = "RequestId")]
        public string RequestId { get; set; }
    }
}
