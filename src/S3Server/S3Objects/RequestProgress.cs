namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Request progress.
    /// </summary>
    [XmlRoot(ElementName = "RequestProgress")]
    public class RequestProgress
    {
        #region Public-Members

        /// <summary>
        /// Enabled.
        /// </summary>
        [XmlElement(ElementName = "Enabled")]
        public bool Enabled { get; set; } = false;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public RequestProgress()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
