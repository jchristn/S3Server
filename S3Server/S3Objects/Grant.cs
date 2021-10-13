using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// A grant allowing a grantee permissiont to a resource.
    /// </summary>
    [XmlRoot(ElementName = "Grant")]
    public class Grant
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// The recipient of the permission.
        /// </summary>
        [XmlElement(ElementName = "Grantee", IsNullable = true)]
        public Grantee Grantee { get; set; } = new Grantee();

        /// <summary>
        /// The permission given to the recipient.
        /// Valid values are FULL_CONTROL, WRITE, WRITE_ACP, READ, READ_ACP.
        /// </summary>
        [XmlElement(ElementName = "Permission", IsNullable = true)]
        public string Permission
        { 
            get
            {
                return _Permission;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Permission));
                if (!_PermissionValidValues.Contains(value)) throw new ArgumentException("Unknown Permission '" + value + "'.");
                _Permission = value;
            }
        }

        #endregion

        #region Private-Members

        private string _Permission = "READ";
        private List<string> _PermissionValidValues = new List<string>
        {
            "FULL_CONTROL",
            "WRITE",
            "WRITE_ACP",
            "READ",
            "READ_ACP"
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public Grant()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="grantee">Grantee.</param>
        /// <param name="permission">Permission.  Valid values are FULL_CONTROL, WRITE, WRITE_ACP, READ, READ_ACP.</param>
        public Grant(Grantee grantee, string permission)
        {
            Grantee = grantee;
            Permission = permission;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
