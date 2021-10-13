using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Resource owner.
    /// </summary>
    [XmlRoot(ElementName = "Owner", IsNullable = true)]
    public class Owner
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// ID of the owner.
        /// </summary>
        [XmlElement(ElementName = "ID", IsNullable = true)]
        public string ID { get; set; } = null;

        /// <summary>
        /// Display name of the owner.
        /// </summary>
        [XmlElement(ElementName = "DisplayName", IsNullable = true)]
        public string DisplayName { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public Owner()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="id">ID.</param>
        /// <param name="displayName">Display name.</param>
        public Owner(string id, string displayName)
        {
            ID = id;
            DisplayName = displayName;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
