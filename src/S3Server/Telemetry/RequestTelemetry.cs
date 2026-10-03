namespace S3ServerLibrary
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using S3ServerLibrary.S3Objects;

    /// <summary>
    /// Telemetry state for one S3 request: the request span, timing, and the outcome details recorded when it completes.
    /// Exists only when a collector observes the S3Server meter or activity source; every call site uses null-conditional access.
    /// Flows with the request through an AsyncLocal so the response and serialization code can attribute stages to it.
    /// All methods are best-effort and never throw.  Not intended to be shared across requests.
    /// </summary>
    internal sealed class RequestTelemetry
    {
        #region Internal-Members

        internal static RequestTelemetry Current
        {
            get { return _Current.Value; }
            set { _Current.Value = value; }
        }

        internal S3ServerTelemetry Telemetry
        {
            get { return _Telemetry; }
        }

        internal Activity Activity
        {
            get { return _Activity; }
        }

        internal string Operation
        {
            get { return _Operation; }
        }

        internal string Handler { get; set; } = S3ServerTelemetryNames.HandlerRejected;

        internal string SignatureOutcome { get; set; } = null;

        #endregion

        #region Private-Members

        private static readonly AsyncLocal<RequestTelemetry> _Current = new AsyncLocal<RequestTelemetry>();

        private readonly S3ServerTelemetry _Telemetry;
        private readonly long _StartTimestamp;
        private readonly bool _ActiveCounted;
        private Activity _Activity = null;
        private string _Operation = S3RequestType.Unknown.ToString();
        private string _ErrorType = null;
        private string _ErrorStage = null;
        private Exception _Exception = null;
        private int _Completed = 0;

        #endregion

        #region Constructors-and-Factories

        internal RequestTelemetry(S3ServerTelemetry telemetry, bool startActivity)
        {
            _Telemetry = telemetry;
            _StartTimestamp = Stopwatch.GetTimestamp();
            _ActiveCounted = telemetry.AddActive(1);

            if (startActivity)
            {
                try
                {
                    ActivityKind kind = Activity.Current == null ? ActivityKind.Server : ActivityKind.Internal;
                    _Activity = telemetry.Source.StartActivity(S3ServerTelemetryNames.RequestSpanPrefix + _Operation, kind);
                }
                catch (Exception)
                {
                    _Activity = null;
                }
            }
        }

        #endregion

        #region Internal-Methods

        internal static TelemetryStage StartCurrentStage(string stage)
        {
            RequestTelemetry current = _Current.Value;
            if (current == null) return null;
            return current.StartStage(stage);
        }

        internal TelemetryStage StartStage(string stage)
        {
            try
            {
                return new TelemetryStage(this, stage, S3ServerTelemetryNames.StageSpanPrefix + stage, null);
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal TelemetryStage StartCallback(string callback)
        {
            try
            {
                return new TelemetryStage(this, S3ServerTelemetryNames.StageCallback, S3ServerTelemetryNames.CallbackSpanPrefix + callback, callback);
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal void Describe(S3Context ctx)
        {
            try
            {
                if (ctx == null || ctx.Request == null) return;

                S3Request req = ctx.Request;
                _Operation = req.RequestType.ToString();

                if (_Activity == null) return;

                S3ServerTelemetrySettings settings = _Telemetry.Settings;
                _Activity.DisplayName = S3ServerTelemetryNames.RequestSpanPrefix + _Operation;
                _Activity.SetTag(S3ServerTelemetryNames.AttributeOperation, _Operation);
                _Activity.SetTag(S3ServerTelemetryNames.AttributeRequestStyle, req.RequestStyle.ToString());
                _Activity.SetTag(S3ServerTelemetryNames.AttributeRequestId, req.RequestId);
                _Activity.SetTag(S3ServerTelemetryNames.AttributeSignatureVersion, SignatureVersionOf(req));

                if (settings.IncludeBucketNames && !String.IsNullOrEmpty(req.Bucket))
                    _Activity.SetTag(S3ServerTelemetryNames.AttributeBucket, req.Bucket);

                if (settings.IncludeObjectKeys && !String.IsNullOrEmpty(req.Key))
                    _Activity.SetTag(S3ServerTelemetryNames.AttributeKey, req.Key);

                if (!String.IsNullOrEmpty(req.UploadId))
                    _Activity.SetTag(S3ServerTelemetryNames.AttributeUploadId, req.UploadId);

                if (req.PartNumber > 0)
                    _Activity.SetTag(S3ServerTelemetryNames.AttributePartNumber, req.PartNumber);
            }
            catch (Exception)
            {
            }
        }

        internal void MarkError(string errorType, string stage)
        {
            if (!String.IsNullOrEmpty(errorType)) _ErrorType = errorType;
            if (_ErrorStage == null && !String.IsNullOrEmpty(stage)) _ErrorStage = stage;
        }

        internal void MarkResponseError(ErrorCode code)
        {
            if (code == ErrorCode.NotModified) return;

            // An unexpected exception is more diagnostic than the InternalError code it is answered with.
            if (code == ErrorCode.InternalError && _Exception != null && !(_Exception is S3Exception)) return;
            _ErrorType = code.ToString();
        }

        internal void MarkException(Exception e, string stage)
        {
            if (e == null) return;
            if (_Exception == null) _Exception = e;
            MarkError(S3ServerTelemetry.ErrorTypeOf(e), stage);
        }

        internal void RecordObjectSize(long bytes)
        {
            _Telemetry.RecordObjectSize(_Operation, bytes);
        }

        internal void Complete(int statusCode)
        {
            if (Interlocked.Exchange(ref _Completed, 1) == 1) return;

            try
            {
                double seconds = S3ServerTelemetry.ElapsedSeconds(_StartTimestamp);

                string outcome;
                if (statusCode >= 500) outcome = S3ServerTelemetryNames.OutcomeServerError;
                else if (statusCode >= 400) outcome = S3ServerTelemetryNames.OutcomeClientError;
                else outcome = S3ServerTelemetryNames.OutcomeSuccess;

                bool failed = statusCode >= 400;
                string errorType = null;

                if (failed)
                {
                    errorType = _ErrorType ?? statusCode.ToString();
                    _Telemetry.RecordError(errorType, _ErrorStage ?? S3ServerTelemetryNames.StageInternal);
                }

                _Telemetry.RecordRequest(_Operation, outcome, Handler, errorType, seconds);

                if (_ActiveCounted) _Telemetry.AddActive(-1);

                if (_Activity != null)
                {
                    _Activity.SetTag(S3ServerTelemetryNames.AttributeHttpStatusCode, statusCode);
                    _Activity.SetTag(S3ServerTelemetryNames.AttributeHandler, Handler);
                    _Activity.SetTag(S3ServerTelemetryNames.AttributeOutcome, outcome);

                    if (failed)
                    {
                        _Activity.SetTag(S3ServerTelemetryNames.AttributeErrorType, errorType);
                        if (_ErrorStage != null) _Activity.SetTag(S3ServerTelemetryNames.AttributeStage, _ErrorStage);
                    }

                    if (statusCode >= 500)
                    {
                        if (_Exception != null) TelemetryStage.AddExceptionEvent(_Activity, _Exception);
                        _Activity.SetStatus(ActivityStatusCode.Error, errorType);
                    }

                    _Activity.Stop();
                }
            }
            catch (Exception)
            {
            }
        }

        internal static string SignatureVersionOf(S3Request req)
        {
            if (req == null) return S3ServerTelemetryNames.SignatureVersionNone;
            if (req.SignatureVersion == S3SignatureVersion.Version4) return S3ServerTelemetryNames.SignatureVersion4;
            if (req.SignatureVersion == S3SignatureVersion.Version2) return S3ServerTelemetryNames.SignatureVersion2;
            return S3ServerTelemetryNames.SignatureVersionNone;
        }

        #endregion
    }
}
