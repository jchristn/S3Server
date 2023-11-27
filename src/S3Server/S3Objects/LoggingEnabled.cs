namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Logging status.
    /// </summary>
    [XmlRoot(ElementName = "LoggingEnabled")]
    public class LoggingEnabled
    {
        // Namespace = "http://doc.s3.amazonaws.com/2006-03-01"

        #region Public-Members

        /// <summary>
        /// The bucket where logs are stored.
        /// </summary>
        [XmlElement(ElementName = "TargetBucket", IsNullable = true)]
        public string TargetBucket { get; set; } = null;

        /// <summary>
        /// The prefix for objects used to store logging data.
        /// </summary>
        [XmlElement(ElementName = "TargetPrefix", IsNullable = true)]
        public string TargetPrefix { get; set; } = null;

        /// <summary>
        /// The grants allowing others to access logging data.
        /// </summary>
        [XmlElement(ElementName = "TargetGrants", IsNullable = true)]
        public TargetGrants TargetGrants { get; set; } = new TargetGrants();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public LoggingEnabled()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="targetBucket">Target bucket.</param>
        /// <param name="targetPrefix">Target prefix.</param>
        /// <param name="targetGrants">Target grants.</param>
        public LoggingEnabled(string targetBucket, string targetPrefix, TargetGrants targetGrants)
        {
            if (String.IsNullOrEmpty(targetBucket)) throw new ArgumentNullException(nameof(targetBucket));
            TargetBucket = targetBucket;
            TargetPrefix = targetPrefix;
            if (targetGrants != null) TargetGrants = targetGrants;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
