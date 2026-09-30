namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Tag.
    /// </summary>
    [XmlRoot(ElementName = "Tag")]
    public class Tag
    {
        #region Public-Members

        /// <summary>
        /// Key.
        /// </summary>
        [XmlElement(ElementName = "Key", IsNullable = true)]
        public string Key
        {
            get
            {
                return _Key;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Key));
                _Key = value;
            }
        }

        /// <summary>
        /// Value.
        /// </summary>
        [XmlElement(ElementName = "Value", IsNullable = true)]
        public string Value { get; set; } = null;

        #endregion

        #region Private-Members

        private string _Key = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public Tag()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="val">Value.</param>
        public Tag(string key, string val)
        {
            Key = key;
            Value = val;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
