using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Amazon S3 access control list for a resource.
    /// </summary>
    [XmlRoot(ElementName = "AccessControlList")]
    public class AccessControlList
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Grant specifying to whom rights are provided to the resource.
        /// </summary>
        [XmlElement(ElementName = "Grant", IsNullable = true)]
        public List<Grant> Grants { get; set; } = new List<Grant>();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public AccessControlList()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="grants">Grants.</param>
        public AccessControlList(List<Grant> grants)
        {
            if (grants != null) Grants = grants;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
