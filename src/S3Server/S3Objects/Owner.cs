namespace S3ServerLibrary.S3Objects
{
    using System.Xml.Serialization;

    /// <summary>
    /// Resource owner.
    /// </summary>
    [XmlRoot(ElementName = "Owner")]
    public class Owner
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// ID of the owner.
        /// </summary>
        [XmlElement(ElementName = "ID")]
        public string ID { get; set; } = null;

        /// <summary>
        /// Display name of the owner.
        /// </summary>
        [XmlElement(ElementName = "DisplayName")]
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
