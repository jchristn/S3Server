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
        [XmlElement(ElementName = "Permission")]
        public PermissionEnum Permission { get; set; } = PermissionEnum.Read;

        #endregion

        #region Private-Members

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
        public Grant(Grantee grantee, PermissionEnum permission)
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
