namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Retention status of a resource.
    /// </summary>
    [XmlRoot(ElementName = "Retention", IsNullable = true)]
    public class Retention
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Retention mode.
        /// Valid values are null, GOVERNANCE, COMPLIANCE.
        /// </summary>
        [XmlElement(ElementName = "Mode")]
        public RetentionModeEnum Mode { get; set; } = RetentionModeEnum.None;

        /// <summary>
        /// Date upon which the resource shall no longer be retained.
        /// </summary>
        [XmlElement(ElementName = "RetainUntilDate", IsNullable = true)]
        public DateTime? RetainUntilDate { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public Retention()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="mode">Mode.  Valid values are null, GOVERNANCE, COMPLIANCE.</param>
        /// <param name="retainUntilDate">Retain until.</param>
        public Retention(RetentionModeEnum mode, DateTime? retainUntilDate)
        {
            Mode = mode;
            RetainUntilDate = retainUntilDate;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
