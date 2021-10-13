using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Logging status for a bucket.
    /// </summary>
    [XmlRoot(ElementName = "BucketLoggingStatus")]
    public class BucketLoggingStatus
    {
        // Namespace = "http://doc.s3.amazonaws.com/2006-03-01"

        #region Public-Members

        /// <summary>
        /// Logging configuration for a bucket.
        /// </summary>
        [XmlElement(ElementName = "LoggingEnabled", IsNullable = true)]
        public LoggingEnabled Enabled { get; set; } = new LoggingEnabled();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public BucketLoggingStatus()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="loggingEnabled">Logging enabled.</param>
        public BucketLoggingStatus(LoggingEnabled loggingEnabled)
        {
            Enabled = loggingEnabled;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
