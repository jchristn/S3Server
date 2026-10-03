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
        /// Telemetry settings for the S3Server meter and activity source (S3 operations, pipeline stages, callbacks, signatures).
        /// Read when the S3Server is constructed.  The webserver's own HTTP telemetry is configured under Webserver.Telemetry.
        /// Setting null restores the defaults.  See TELEMETRY.md.
        /// </summary>
        public S3ServerTelemetrySettings Telemetry
        {
            get
            {
                return _Telemetry;
            }
            set
            {
                if (value == null) _Telemetry = new S3ServerTelemetrySettings();
                else _Telemetry = value;
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
        /// Return true if you wish to terminate the request (the handler has sent the response), otherwise, return false, which will further route the request.
        /// SECURITY: with the default ValidateSignaturesBeforePreRequestHandler value of false, this handler runs BEFORE signature
        /// validation and therefore sees unauthenticated input.  It must not act on, modify, or disclose data.  Use it for logging,
        /// metrics, or populating ctx.Metadata for Service.GetSecretKey.  Use AuthenticatedRequestHandler for anything that
        /// answers requests.  Default is null.
        /// </summary>
        [JsonIgnore]
        public Func<S3Context, Task<bool>> PreRequestHandler = null;

        /// <summary>
        /// Callback method invoked after signature validation (when EnableSignatures is true) and before the request is routed
        /// to an operation callback.  Requests with a missing, forged, or unknown-key signature never reach this handler.
        /// Unsigned requests permitted by Service.IsAnonymousRequestAllowed do reach it.
        /// Return true if the handler has sent the response and the request should terminate, otherwise return false to continue routing.
        /// When EnableSignatures is false, this handler is invoked for every request, after PreRequestHandler.  Default is null.
        /// </summary>
        [JsonIgnore]
        public Func<S3Context, Task<bool>> AuthenticatedRequestHandler = null;

        /// <summary>
        /// When true, signature validation runs before PreRequestHandler, so PreRequestHandler only sees authenticated
        /// (or anonymously permitted) requests.  When false, PreRequestHandler runs first and sees unauthenticated input;
        /// this ordering is retained for consumers that populate ctx.Metadata in PreRequestHandler for Service.GetSecretKey.
        /// Default is false.  Has no effect when EnableSignatures is false.
        /// </summary>
        public bool ValidateSignaturesBeforePreRequestHandler { get; set; } = false;

        /// <summary>
        /// When true, suffix range requests (Range: bytes=-N) are routed to Object.ReadRange with S3Request.RangeSuffixLength set,
        /// and the Object.ReadRange callback must set S3Object.TotalSize so the Content-Range header can be computed.
        /// When false, suffix range requests are routed to Object.Read, as in versions prior to 7.4.0; use this if an existing
        /// Object.ReadRange callback assumes S3Request.RangeStart is always set.  Default is true.
        /// </summary>
        public bool RouteSuffixRangesToReadRange { get; set; } = true;

        /// <summary>
        /// When false, headers Amazon S3 does not send are removed from the webserver's default response headers when the
        /// server is constructed: the request-only headers Host, Accept, Accept-Language, and Accept-Charset, the blanket
        /// Cache-Control, the CORS Access-Control-* headers (see EmitCorsHeaders), and Connection when the webserver has
        /// keep-alive enabled.  When true, the webserver's default headers are sent unmodified on every response, as in
        /// versions prior to 7.4.0.  This value is only read when the S3Server is constructed.  Default is false.
        /// </summary>
        public bool PreserveWebserverDefaultHeaders { get; set; } = false;

        /// <summary>
        /// When true, the webserver's default CORS headers (Access-Control-*) are added to responses for requests that carry
        /// an Origin header, which is what browsers send for cross-origin requests.  When false, no CORS headers are sent,
        /// which matches Amazon S3 for a bucket without a CORS configuration.  Enable this for browser-based clients.
        /// Has no effect when PreserveWebserverDefaultHeaders is true (the CORS headers are then sent on every response).
        /// This value is only read when the S3Server is constructed and for each request.  Default is false.
        /// </summary>
        public bool EmitCorsHeaders { get; set; } = false;

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
        /// Enable or disable signature validation.  Dependent upon Service.GetSecretKey being set.
        /// AWS Signature V4 is validated when this value is true.
        /// </summary>
        public bool EnableSignatures { get; set; } = false;

        /// <summary>
        /// Enable or disable legacy AWS Signature V2 validation.
        /// This setting has no effect unless EnableSignatures is true.
        /// Signature V2 is deprecated by AWS and should only be enabled for legacy S3-compatible clients.
        /// </summary>
        public bool EnableSignatureV2 { get; set; } = false;

        #endregion

        #region Private-Members

        private LoggingSettings _Logging = new LoggingSettings();
        private WebserverSettings _Webserver = new WebserverSettings("localhost", 8000, false);
        private OperationLimitsSettings _Limits = new OperationLimitsSettings();
        private S3ServerTelemetrySettings _Telemetry = new S3ServerTelemetrySettings();

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
