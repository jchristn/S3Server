namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// XML serialization surface for one Error entry inside a DeleteResult.  Amazon S3 writes these entries as Key,
    /// VersionId, Code, and Message, while a standalone error body starts with Code, so a DeleteResult cannot reuse the
    /// Error element order.  Consumers should use DeleteResult.Errors rather than this type directly.
    /// Every property reads from and writes to the wrapped Error.
    /// Not thread-safe.
    /// </summary>
    public class DeleteError
    {
        #region Public-Members

        /// <summary>
        /// Object key.  Omitted from XML when null or empty.
        /// </summary>
        [XmlElement(ElementName = "Key")]
        public string Key
        {
            get { return _Error.Key; }
            set { _Error.Key = value; }
        }

        /// <summary>
        /// Version ID.  Omitted from XML when null or empty.
        /// </summary>
        [XmlElement(ElementName = "VersionId")]
        public string VersionId
        {
            get { return _Error.VersionId; }
            set { _Error.VersionId = value; }
        }

        /// <summary>
        /// Error code.
        /// </summary>
        [XmlElement(ElementName = "Code")]
        public ErrorCode Code
        {
            get { return _Error.Code; }
            set { _Error.Code = value; }
        }

        /// <summary>
        /// Message.  When not set, the default message for Code is used.
        /// </summary>
        [XmlElement(ElementName = "Message")]
        public string Message
        {
            get { return _Error.Message; }
            set { _Error.Message = value; }
        }

        /// <summary>
        /// The wrapped error.  Never null.
        /// </summary>
        [XmlIgnore]
        public Error Error
        {
            get { return _Error; }
        }

        #endregion

        #region Private-Members

        private Error _Error = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate with a new Error.
        /// </summary>
        public DeleteError()
        {
            _Error = new Error();
        }

        /// <summary>
        /// Instantiate around an existing Error.
        /// </summary>
        /// <param name="error">Error.  Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if error is null.</exception>
        public DeleteError(Error error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            _Error = error;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>True if Key is not null or empty.</returns>
        public bool ShouldSerializeKey()
        {
            return !String.IsNullOrEmpty(Key);
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>True if VersionId is not null or empty.</returns>
        public bool ShouldSerializeVersionId()
        {
            return !String.IsNullOrEmpty(VersionId);
        }

        #endregion
    }
}
