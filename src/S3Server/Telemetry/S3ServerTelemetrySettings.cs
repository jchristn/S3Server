namespace S3ServerLibrary
{
    using System;

    /// <summary>
    /// Telemetry settings for S3Server.  S3Server emits metrics through a System.Diagnostics.Metrics.Meter and traces through a
    /// System.Diagnostics.ActivitySource, both named S3Server by default.  S3Server never exports anything itself: a host
    /// (OpenTelemetry SDK, Radiant, dotnet-counters) subscribes to those names.  When nothing subscribes, instrumentation costs a
    /// few listener checks per request and creates no telemetry state.
    /// The webserver's own telemetry (the Watson meter and activity source: HTTP metrics and a per-request server span) is
    /// configured separately under S3ServerSettings.Webserver.Telemetry.
    /// These values are read when the S3Server is constructed; change them before constructing the server.
    /// Not thread-safe for concurrent modification.
    /// </summary>
    public class S3ServerTelemetrySettings
    {
        #region Public-Members

        /// <summary>
        /// Master switch.  When false, S3Server creates no Meter or ActivitySource and records nothing.
        /// Default is true.
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Emit metrics on the Meter.  Has no effect when Enable is false.
        /// Default is true.
        /// </summary>
        public bool EnableMetrics { get; set; } = true;

        /// <summary>
        /// Emit spans on the ActivitySource.  Has no effect when Enable is false.
        /// Default is true.
        /// </summary>
        public bool EnableTraces { get; set; } = true;

        /// <summary>
        /// Name of the Meter a collector subscribes to.  Change it only to tell several S3Server instances in one process apart.
        /// Default is S3Server.  Cannot be null or empty.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when set to null or empty.</exception>
        public string MeterName
        {
            get
            {
                return _MeterName;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(MeterName));
                _MeterName = value;
            }
        }

        /// <summary>
        /// Name of the ActivitySource a collector subscribes to.  Change it only to tell several S3Server instances in one process apart.
        /// Default is S3Server.  Cannot be null or empty.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when set to null or empty.</exception>
        public string ActivitySourceName
        {
            get
            {
                return _ActivitySourceName;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(ActivitySourceName));
                _ActivitySourceName = value;
            }
        }

        /// <summary>
        /// Add the bucket name to spans as aws.s3.bucket.  Bucket names never appear on metrics.
        /// Default is true.
        /// </summary>
        public bool IncludeBucketNames { get; set; } = true;

        /// <summary>
        /// Add the object key to spans as aws.s3.key.  Object keys often carry user data (file names, user identifiers),
        /// so they are omitted unless explicitly enabled.  Object keys never appear on metrics.
        /// Default is false.
        /// </summary>
        public bool IncludeObjectKeys { get; set; } = false;

        #endregion

        #region Private-Members

        private string _MeterName = S3ServerTelemetryNames.DefaultSourceName;
        private string _ActivitySourceName = S3ServerTelemetryNames.DefaultSourceName;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public S3ServerTelemetrySettings()
        {

        }

        #endregion
    }
}
