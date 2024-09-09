namespace S3ServerLibrary
{
    /// <summary>
    /// Logging settings.
    /// </summary>
    public class LoggingSettings
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable debugging for HTTP requests.
        /// </summary>
        public bool HttpRequests { get; set; } = false;

        /// <summary>
        /// Enable or disable debugging for S3 request construction.
        /// </summary>
        public bool S3Requests { get; set; } = false;

        /// <summary>
        /// Enable or disable debugging for signature validation for version 4 signatures.
        /// </summary>
        public bool SignatureV4Validation { get; set; } = false;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public LoggingSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
