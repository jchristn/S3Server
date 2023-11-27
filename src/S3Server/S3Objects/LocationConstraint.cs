namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Location constraint for a resource.
    /// </summary>
    [XmlRoot(ElementName = "LocationConstraint")]
    public class LocationConstraint
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Text, i.e. the region.
        /// Valid values are valid S3 regions, i.e. us-west-1.
        /// </summary>
        [XmlText]
        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Text));
                if (!_TextValidValues.Contains(value)) throw new ArgumentException("Unknown Region '" + value + "'.");
                _Text = value;
            }
        }

        #endregion

        #region Private-Members

        private string _Text = "us-west-1";
        private List<string> _TextValidValues = new List<string>
        {
            "af-south-1",
            "ap-east-1",
            "ap-northeast-1",
            "ap-northeast-2",
            "ap-northeast-3",
            "ap-south-1",
            "ap-southeast-1",
            "ap-southeast-2",
            "ca-central-1",
            "cn-north-1",
            "cn-northwest-1",
            "EU",
            "eu-central-1",
            "eu-north-1",
            "eu-south-1",
            "eu-west-1",
            "eu-west-2",
            "eu-west-3",
            "me-south-1",
            "sa-east-1",
            "us-east-2",
            "us-gov-east-1",
            "us-gov-west-1",
            "us-west-1",
            "us-west-2"
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public LocationConstraint()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="region">Region.  Valid values are valid S3 regions, i.e. us-west-1.</param>
        public LocationConstraint(string region)
        {
            Text = region;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
