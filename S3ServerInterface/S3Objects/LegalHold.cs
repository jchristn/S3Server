using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    // built using https://xmltocsharp.azurewebsites.net/

    [XmlRoot(ElementName = "LegalHold")]
    public class LegalHold
    {
        [XmlElement(ElementName = "Status")]
        public string Status { get; set; }
    }
}
