using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerInterface.S3Objects
{
    /// <summary>
    /// Location constraint for a resource.
    /// </summary>
    [XmlRoot(ElementName = "LocationConstraint", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class LocationConstraint
    {
        /// <summary>
        /// XML namespace.
        /// </summary>
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }

        /// <summary>
        /// Text, i.e. the region.
        /// </summary>
        [XmlText]
        public string Text { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public LocationConstraint()
        {
            Text = null;
        }
    }
}
