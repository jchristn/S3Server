using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Legal hold status of a resource.
    /// </summary>
    [XmlRoot(ElementName = "LegalHold", IsNullable = true)]
    public class LegalHold
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Legal hold status.
        /// Valid values are null, ON, OFF.
        /// </summary>
        [XmlElement(ElementName = "Status", IsNullable = true)]
        public string Status
        {
            get
            {
                return _Status;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) _Status = value;
                else
                {
                    if (!_StatusValidValues.Contains(value)) throw new ArgumentException("Unknown Status '" + value + "'.");
                    _Status = value;
                }
            }
        }

        #endregion

        #region Private-Members

        private string _Status = "OFF";
        private List<string> _StatusValidValues = new List<string>
        {
            "ON",
            "OFF"
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public LegalHold()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="status">Status.  Valid values are null, ON, OFF.</param>
        public LegalHold(string status)
        {
            Status = status;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
