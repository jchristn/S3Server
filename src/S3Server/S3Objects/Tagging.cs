namespace S3ServerLibrary.S3Objects
{
    using System.Xml.Serialization;

    /// <summary>
    /// Tag metadata.
    /// </summary>
    [XmlRoot(ElementName = "Tagging")]
    public class Tagging
    {
        #region Public-Members

        /// <summary>
        /// Tag set.
        /// </summary>
        [XmlElement(ElementName = "TagSet", IsNullable = true)]
        public TagSet Tags { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public Tagging()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="tags">Tags.</param>
        public Tagging(TagSet tags)
        {
            if (tags != null) Tags = tags;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
