namespace S3ServerLibrary.S3Objects
{
    using System.Xml.Serialization;

    /// <summary>
    /// Bucket versioning configuration.
    /// </summary>
    [XmlRoot(ElementName = "VersioningConfiguration", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/", IsNullable = true)]
    public class VersioningConfiguration
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Status of the versioning configuration.
        /// Valid values are null, Enabled, Suspended.
        /// </summary>
        [XmlElement(ElementName = "Status")]
        public VersioningStatusEnum Status { get; set; } = VersioningStatusEnum.Suspended;

        /// <summary>
        /// Indicates if multi-factor authentication is enabled for delete operations.
        /// Valid values are null, Enabled, Disabled.
        /// </summary>
        [XmlElement(ElementName = "MfaDelete")]
        public MfaDeleteStatusEnum MfaDelete { get; set; } = MfaDeleteStatusEnum.Disabled;

        /// <summary>
        /// Flag to control whether Status is serialized.
        /// Default value is true.
        /// </summary>
        [XmlIgnore]
        public bool IncludeStatus { get; set; } = true;

        /// <summary>
        /// Flag to control whether MfaDelete is serialized.
        /// Default value is false.
        /// </summary>
        [XmlIgnore]
        public bool IncludeMfaDelete { get; set; } = false;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public VersioningConfiguration()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="status">Status.  Valid values are null, Enabled, Suspended.</param>
        /// <param name="mfaDelete">MFA delete setting.  Valid values are null, Enabled, Disabled.</param>
        public VersioningConfiguration(VersioningStatusEnum status, MfaDeleteStatusEnum mfaDelete)
        {
            Status = status;
            MfaDelete = mfaDelete;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeStatus()
        {
            return IncludeStatus;
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeMfaDelete()
        {
            return IncludeMfaDelete;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
