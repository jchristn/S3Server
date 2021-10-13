using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Amazon S3 access control policy for a resource.
    /// </summary>
    [XmlRoot(ElementName = "AccessControlPolicy")]
    public class AccessControlPolicy
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Resource owner.
        /// </summary>
        [XmlElement(ElementName = "Owner", IsNullable = true)]
        public Owner Owner { get; set; } = null;

        /// <summary>
        /// Access control list for the resource.
        /// </summary>
        [XmlElement(ElementName = "AccessControlList", IsNullable = true)]
        public AccessControlList Acl { get; set; } = new AccessControlList();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public AccessControlPolicy()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="owner">Owner.</param>
        /// <param name="acl">ACL.</param>
        public AccessControlPolicy(Owner owner, AccessControlList acl)
        {
            Owner = owner;
            Acl = acl;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
