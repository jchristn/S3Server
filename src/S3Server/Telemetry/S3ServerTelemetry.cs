namespace S3ServerLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Metrics;
    using System.Reflection;

    /// <summary>
    /// Owns the Meter, ActivitySource, and instruments for one S3Server instance.
    /// All recording methods are best-effort: they never throw.  Thread-safe.
    /// </summary>
    internal sealed class S3ServerTelemetry : IDisposable
    {
        #region Internal-Members

        internal S3ServerTelemetrySettings Settings
        {
            get { return _Settings; }
        }

        internal ActivitySource Source
        {
            get { return _Source; }
        }

        internal Meter Meter
        {
            get { return _Meter; }
        }

        internal static string LibraryVersion
        {
            get { return _LibraryVersion; }
        }

        #endregion

        #region Private-Members

        private static readonly string _LibraryVersion = ResolveLibraryVersion();

#if NET9_0_OR_GREATER
        private static readonly double[] _DurationBoundaries = new double[]
        {
            0.0005, 0.001, 0.0025, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10, 30, 60, 120
        };

        private static readonly long[] _SizeBoundaries = new long[]
        {
            0, 1024, 16384, 65536, 262144, 1048576, 4194304, 16777216, 67108864, 268435456, 1073741824, 5368709120
        };
#endif

        private readonly S3ServerTelemetrySettings _Settings;
        private readonly Meter _Meter = null;
        private readonly ActivitySource _Source = null;

        private readonly Counter<long> _Requests = null;
        private readonly Histogram<double> _RequestDuration = null;
        private readonly UpDownCounter<long> _RequestsActive = null;
        private readonly Histogram<double> _StageDuration = null;
        private readonly Counter<long> _CallbackInvocations = null;
        private readonly Histogram<double> _CallbackDuration = null;
        private readonly Counter<long> _SignatureValidations = null;
        private readonly Histogram<double> _SignatureDuration = null;
        private readonly Counter<long> _Errors = null;
        private readonly Histogram<long> _ObjectSize = null;
        private readonly Counter<long> _LifecycleEvents = null;

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        internal S3ServerTelemetry(S3ServerTelemetrySettings settings, Func<S3ServerSettings> serverSettings, Func<bool> isListening)
        {
            _Settings = settings ?? new S3ServerTelemetrySettings();
            if (!_Settings.Enable) return;

            if (_Settings.EnableTraces)
            {
                _Source = new ActivitySource(_Settings.ActivitySourceName, _LibraryVersion);
            }

            if (_Settings.EnableMetrics)
            {
                _Meter = new Meter(_Settings.MeterName, _LibraryVersion);

                _Requests = _Meter.CreateCounter<long>(S3ServerTelemetryNames.Requests, "{request}", "Completed S3 requests.");
                _RequestDuration = CreateDurationHistogram(S3ServerTelemetryNames.RequestDuration, "End-to-end S3 request duration.");
                _RequestsActive = _Meter.CreateUpDownCounter<long>(S3ServerTelemetryNames.RequestsActive, "{request}", "S3 requests currently being processed.");
                _StageDuration = CreateDurationHistogram(S3ServerTelemetryNames.StageDuration, "Duration of each request pipeline stage.");
                _CallbackInvocations = _Meter.CreateCounter<long>(S3ServerTelemetryNames.CallbackInvocations, "{invocation}", "Application callback invocations.");
                _CallbackDuration = CreateDurationHistogram(S3ServerTelemetryNames.CallbackDuration, "Application callback duration.");
                _SignatureValidations = _Meter.CreateCounter<long>(S3ServerTelemetryNames.SignatureValidations, "{validation}", "Signature validations by version and outcome.");
                _SignatureDuration = CreateDurationHistogram(S3ServerTelemetryNames.SignatureDuration, "Signature validation duration.");
                _Errors = _Meter.CreateCounter<long>(S3ServerTelemetryNames.Errors, "{error}", "Failed S3 requests by error type and the stage where the error surfaced.");
                _ObjectSize = CreateSizeHistogram(S3ServerTelemetryNames.ObjectSize, "Object payload size for reads and writes.");
                _LifecycleEvents = _Meter.CreateCounter<long>(S3ServerTelemetryNames.LifecycleEvents, "{event}", "Server lifecycle events.");

                _Meter.CreateObservableGauge<int>(
                    S3ServerTelemetryNames.BuildInfo,
                    () => new Measurement<int>(1, new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeVersion, _LibraryVersion)),
                    null,
                    "S3Server library version; the value is always 1.");

                _Meter.CreateObservableGauge<int>(
                    S3ServerTelemetryNames.ConfigSignaturesEnabled,
                    () => ObserveFlag(serverSettings, s => s.EnableSignatures),
                    null,
                    "1 when signature validation is enabled.");

                _Meter.CreateObservableGauge<int>(
                    S3ServerTelemetryNames.ConfigSignatureV2Enabled,
                    () => ObserveFlag(serverSettings, s => s.EnableSignatures && s.EnableSignatureV2),
                    null,
                    "1 when legacy signature V2 validation is enabled.");

                _Meter.CreateObservableGauge<long>(
                    S3ServerTelemetryNames.ConfigMaxPutObjectSize,
                    () => ObserveMaxPutObjectSize(serverSettings),
                    "By",
                    "Configured maximum PutObject size.");

                _Meter.CreateObservableGauge<int>(
                    S3ServerTelemetryNames.Listening,
                    () => ObserveListening(isListening),
                    null,
                    "1 while the server is listening.");
            }
        }

        #endregion

        #region Internal-Methods

        internal RequestTelemetry BeginRequest()
        {
            try
            {
                if (_Disposed) return null;

                bool metricsObserved = _Meter != null
                    && (_Requests.Enabled || _RequestDuration.Enabled || _StageDuration.Enabled || _CallbackDuration.Enabled
                        || _Errors.Enabled || _RequestsActive.Enabled || _SignatureValidations.Enabled || _ObjectSize.Enabled
                        || _CallbackInvocations.Enabled || _SignatureDuration.Enabled);

                bool tracesObserved = _Source != null && _Source.HasListeners();

                if (!metricsObserved && !tracesObserved) return null;

                return new RequestTelemetry(this, tracesObserved);
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal void RecordRequest(string operation, string outcome, string handler, string errorType, double seconds)
        {
            if (_Meter == null) return;

            try
            {
                KeyValuePair<string, object> op = new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeOperation, operation);
                KeyValuePair<string, object> oc = new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeOutcome, outcome);

                if (String.IsNullOrEmpty(errorType))
                {
                    _Requests.Add(1, op, oc, new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeHandler, handler));
                }
                else
                {
                    _Requests.Add(1, op, oc,
                        new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeHandler, handler),
                        new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeErrorType, errorType));
                }

                _RequestDuration.Record(seconds, op, oc);
            }
            catch (Exception)
            {
            }
        }

        internal bool AddActive(long delta)
        {
            if (_Meter == null || !_RequestsActive.Enabled) return false;

            try
            {
                _RequestsActive.Add(delta);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal void RecordStage(string stage, string outcome, double seconds)
        {
            if (_Meter == null) return;

            try
            {
                _StageDuration.Record(
                    seconds,
                    new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeStage, stage),
                    new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeOutcome, outcome));
            }
            catch (Exception)
            {
            }
        }

        internal void RecordCallback(string callback, string outcome, string errorType, double seconds)
        {
            if (_Meter == null) return;

            try
            {
                KeyValuePair<string, object> cb = new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeCallback, callback);
                KeyValuePair<string, object> oc = new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeOutcome, outcome);

                if (String.IsNullOrEmpty(errorType))
                    _CallbackInvocations.Add(1, cb, oc);
                else
                    _CallbackInvocations.Add(1, cb, oc, new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeErrorType, errorType));

                _CallbackDuration.Record(seconds, cb, oc);
            }
            catch (Exception)
            {
            }
        }

        internal void RecordSignature(string version, string outcome, double seconds)
        {
            if (_Meter == null) return;

            try
            {
                KeyValuePair<string, object> ver = new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeSignatureVersion, version);
                KeyValuePair<string, object> oc = new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeOutcome, outcome);
                _SignatureValidations.Add(1, ver, oc);
                _SignatureDuration.Record(seconds, ver, oc);
            }
            catch (Exception)
            {
            }
        }

        internal void RecordError(string errorType, string stage)
        {
            if (_Meter == null) return;

            try
            {
                _Errors.Add(
                    1,
                    new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeErrorType, errorType),
                    new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeStage, stage));
            }
            catch (Exception)
            {
            }
        }

        internal void RecordObjectSize(string operation, long bytes)
        {
            if (_Meter == null || bytes < 0) return;

            try
            {
                _ObjectSize.Record(bytes, new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeOperation, operation));
            }
            catch (Exception)
            {
            }
        }

        internal void RecordLifecycle(string lifecycleEvent)
        {
            if (_Meter == null) return;

            try
            {
                _LifecycleEvents.Add(1, new KeyValuePair<string, object>(S3ServerTelemetryNames.AttributeLifecycleEvent, lifecycleEvent));
            }
            catch (Exception)
            {
            }
        }

        internal static double ElapsedSeconds(long startTimestamp)
        {
            long elapsed = Stopwatch.GetTimestamp() - startTimestamp;
            if (elapsed < 0) return 0;
            return (double)elapsed / Stopwatch.Frequency;
        }

        internal static string ErrorTypeOf(Exception e)
        {
            if (e == null) return null;
            if (e is S3Exception s3e && s3e.Error != null) return s3e.Error.Code.ToString();
            return e.GetType().FullName;
        }

        /// <summary>
        /// Dispose of the meter and activity source.
        /// </summary>
        public void Dispose()
        {
            if (_Disposed) return;
            _Disposed = true;

            try
            {
                _Meter?.Dispose();
                _Source?.Dispose();
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Private-Methods

        private Histogram<double> CreateDurationHistogram(string name, string description)
        {
#if NET9_0_OR_GREATER
            return _Meter.CreateHistogram<double>(name, "s", description, null, new InstrumentAdvice<double> { HistogramBucketBoundaries = _DurationBoundaries });
#else
            return _Meter.CreateHistogram<double>(name, "s", description);
#endif
        }

        private Histogram<long> CreateSizeHistogram(string name, string description)
        {
#if NET9_0_OR_GREATER
            return _Meter.CreateHistogram<long>(name, "By", description, null, new InstrumentAdvice<long> { HistogramBucketBoundaries = _SizeBoundaries });
#else
            return _Meter.CreateHistogram<long>(name, "By", description);
#endif
        }

        private static IEnumerable<Measurement<int>> ObserveFlag(Func<S3ServerSettings> serverSettings, Func<S3ServerSettings, bool> selector)
        {
            S3ServerSettings settings = null;

            try
            {
                settings = serverSettings?.Invoke();
            }
            catch (Exception)
            {
            }

            if (settings == null) return Array.Empty<Measurement<int>>();
            return new Measurement<int>[] { new Measurement<int>(selector(settings) ? 1 : 0) };
        }

        private static IEnumerable<Measurement<long>> ObserveMaxPutObjectSize(Func<S3ServerSettings> serverSettings)
        {
            S3ServerSettings settings = null;

            try
            {
                settings = serverSettings?.Invoke();
            }
            catch (Exception)
            {
            }

            if (settings == null || settings.OperationLimits == null) return Array.Empty<Measurement<long>>();
            return new Measurement<long>[] { new Measurement<long>(settings.OperationLimits.MaxPutObjectSize) };
        }

        private static IEnumerable<Measurement<int>> ObserveListening(Func<bool> isListening)
        {
            bool listening = false;

            try
            {
                listening = isListening != null && isListening();
            }
            catch (Exception)
            {
            }

            return new Measurement<int>[] { new Measurement<int>(listening ? 1 : 0) };
        }

        private static string ResolveLibraryVersion()
        {
            try
            {
                Assembly assembly = typeof(S3ServerTelemetry).Assembly;
                AssemblyInformationalVersionAttribute info = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                string version = info?.InformationalVersion;

                if (String.IsNullOrEmpty(version)) version = assembly.GetName().Version?.ToString();
                if (String.IsNullOrEmpty(version)) return "unknown";

                int plus = version.IndexOf('+');
                if (plus > 0) version = version.Substring(0, plus);
                return version;
            }
            catch (Exception)
            {
                return "unknown";
            }
        }

        #endregion
    }
}
