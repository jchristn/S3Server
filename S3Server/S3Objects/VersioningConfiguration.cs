using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Bucket versioning configuration.
    /// </summary>
    [XmlRoot(ElementName = "VersioningConfiguration", IsNullable = true)]
    public class VersioningConfiguration
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Status of the versioning configuration.
        /// Valid values are null, Enabled, Suspended.
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
                if (String.IsNullOrEmpty(value)) _Status = null;
                else
                {
                    if (!_StatusValidValues.Contains(value)) throw new ArgumentException("Unknown Status '" + value + "'.");
                    _Status = value;
                }
            }
        }

        /// <summary>
        /// Indicates if multi-factor authentication is enabled for delete operations.
        /// Valid values are null, Enabled, Disabled.
        /// </summary>
        [XmlElement(ElementName = "MfaDelete", IsNullable = true)]
        public string MfaDelete
        {
            get
            {
                return _MfaDelete;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) _MfaDelete = null;
                else
                {
                    if (!_MfaDeleteValidValues.Contains(value)) throw new ArgumentException("Unknown MfaDelete '" + value + "'.");
                    _MfaDelete = value;
                }
            }
        }

        #endregion

        #region Private-Members

        private string _Status = null;
        private List<string> _StatusValidValues = new List<string>
        {
            "Enabled",
            "Suspended"
        };
        private string _MfaDelete = null;
        private List<string> _MfaDeleteValidValues = new List<string>
        {
            "Enabled",
            "Disabled"
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public VersioningConfiguration()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="status">Status.  Valid values are null, Enabled, Suspended.</param>
        /// <param name="mfaDelete">MFA delete setting.  Valid values are null, Enabled, Disabled.</param>
        public VersioningConfiguration(string status, string mfaDelete)
        {
            Status = status;
            MfaDelete = mfaDelete;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
