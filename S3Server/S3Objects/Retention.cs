using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Retention status of a resource.
    /// </summary>
    [XmlRoot(ElementName = "Retention", IsNullable = true)]
    public class Retention
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Retention mode.
        /// Valid values are null, GOVERNANCE, COMPLIANCE.
        /// </summary>
        [XmlElement(ElementName = "Mode", IsNullable = true)]
        public string Mode
        {
            get
            {
                return _Mode;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) _Mode = value;
                else
                {
                    if (!_ModeValidValues.Contains(value)) throw new ArgumentException("Unknown Mode '" + value + "'.");
                    _Mode = value;
                }
            }
        }

        /// <summary>
        /// Date upon which the resource shall no longer be retained.
        /// </summary>
        [XmlElement(ElementName = "RetainUntilDate", IsNullable = true)]
        public DateTime? RetainUntilDate { get; set; } = null;

        #endregion

        #region Private-Members

        private string _Mode = null;
        private List<string> _ModeValidValues = new List<string>
        {
            "GOVERNANCE",
            "COMPLIANCE"
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public Retention()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="mode">Mode.  Valid values are null, GOVERNANCE, COMPLIANCE.</param>
        /// <param name="retainUntilDate">Retain until.</param>
        public Retention(string mode, DateTime? retainUntilDate)
        {
            Mode = mode;
            RetainUntilDate = retainUntilDate;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
