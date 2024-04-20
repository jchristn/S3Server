namespace S3ServerLibrary
{
    using System;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using WatsonWebserver.Core;

    /// <summary>
    /// S3 server settings.
    /// </summary>
    public class S3ServerSettings
    {
        #region Public-Members

        /// <summary>
        /// Method to invoke when sending a log message.  This value can only be changed before the server has been started.  
        /// If you need to change the name after the server has been started, dispose and start again with the correct settings.
        /// </summary>
        [JsonIgnore]
        public Action<string> Logger { get; set; } = null;

        /// <summary>
        /// Enable or disable logging for various items.
        /// </summary>
        public LoggingSettings Logging
        {
            get
            {
                return _Logging;
            }
            set
            {
                if (value == null) _Logging = new LoggingSettings();
                else _Logging = value;
            }
        }

        /// <summary>
        /// Size limits for certain operations.
        /// </summary>
        public OperationLimitsSettings OperationLimits
        {
            get
            {
                return _Limits;
            }
            set
            {
                if (value == null) _Limits = new OperationLimitsSettings();
                else _Limits = value;
            }
        }

        /// <summary>
        /// Webserver settings.
        /// </summary>
        public WebserverSettings Webserver
        {
            get
            {
                return _Webserver;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Webserver));
                _Webserver = value;
            }
        }

        /// <summary>
        /// Callback method to use prior to examining requests for AWS S3 APIs.
        /// Return true if you wish to terminate the request, otherwise, return false, which will further route the request.
        /// </summary>
        [JsonIgnore]
        public Func<S3Context, Task<bool>> PreRequestHandler = null;

        /// <summary>
        /// Callback method to call when no matching AWS S3 API callback could be found. 
        /// </summary>
        [JsonIgnore]
        public Func<S3Context, Task> DefaultRequestHandler = null;

        /// <summary>
        /// Callback method to call after a response has been sent.
        /// </summary>
        [JsonIgnore]
        public Func<S3Context, Task> PostRequestHandler = null;

        /// <summary>
        /// Enable or disable support for signature V4.  Dependent upon EnableSignatureValidation being set to true and the Service.GetSecretKey being set.
        /// </summary>
        public bool EnableSignatures { get; set; } = false;

        #endregion

        #region Private-Members

        private LoggingSettings _Logging = new LoggingSettings();
        private WebserverSettings _Webserver = new WebserverSettings("localhost", 8000, false);
        private OperationLimitsSettings _Limits = new OperationLimitsSettings();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public S3ServerSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
