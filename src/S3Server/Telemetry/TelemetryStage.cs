namespace S3ServerLibrary
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Times one request pipeline stage or application callback, owns its child span, and records the stage (and callback)
    /// metrics when disposed.  Call Fail from an exception filter so the failure is captured before the scope is disposed.
    /// Best-effort: never throws.  Not thread-safe; use from the flow that started it.
    /// </summary>
    internal sealed class TelemetryStage : IDisposable
    {
        #region Private-Members

        private readonly RequestTelemetry _Request;
        private readonly string _Stage;
        private readonly string _Callback;
        private readonly long _StartTimestamp;
        private readonly Activity _Activity = null;
        private Exception _Exception = null;
        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        internal TelemetryStage(RequestTelemetry request, string stage, string spanName, string callback)
        {
            _Request = request;
            _Stage = stage;
            _Callback = callback;
            _StartTimestamp = Stopwatch.GetTimestamp();

            ActivitySource source = request.Telemetry.Source;
            if (source != null && request.Activity != null)
            {
                // Parent implicitly on Activity.Current so Stop restores it; fall back to the request span if the flow lost it.
                if (Activity.Current != null)
                    _Activity = source.StartActivity(spanName, ActivityKind.Internal);
                else
                    _Activity = source.StartActivity(spanName, ActivityKind.Internal, request.Activity.Context);

                if (_Activity != null)
                {
                    _Activity.SetTag(S3ServerTelemetryNames.AttributeStage, stage);
                    if (callback != null)
                    {
                        _Activity.SetTag(S3ServerTelemetryNames.AttributeCallback, callback);
                        _Activity.SetTag(S3ServerTelemetryNames.AttributeOperation, request.Operation);
                    }
                }
            }
        }

        #endregion

        #region Internal-Methods

        /// <summary>
        /// Record a failure for the stage.  Always returns false so it can be used as an exception filter.
        /// </summary>
        /// <param name="stage">Stage scope, may be null.</param>
        /// <param name="e">Exception.</param>
        /// <returns>False.</returns>
        internal static bool Fail(TelemetryStage stage, Exception e)
        {
            try
            {
                if (stage != null && e != null && stage._Exception == null)
                {
                    stage._Exception = e;

                    // PostRequestHandler runs after the response is sent, so its failure must not rewrite the request's outcome.
                    if (stage._Stage != S3ServerTelemetryNames.StagePostRequestHandler)
                        stage._Request.MarkException(e, stage._Stage);
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        internal static void AddExceptionEvent(Activity activity, Exception e)
        {
            if (activity == null || e == null) return;

            try
            {
                ActivityTagsCollection tags = new ActivityTagsCollection();
                tags.Add("exception.type", e.GetType().FullName);
                tags.Add("exception.message", e.Message);
                tags.Add("exception.stacktrace", e.ToString());
                activity.AddEvent(new ActivityEvent("exception", default(DateTimeOffset), tags));
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Stop the span and record the stage metrics.
        /// </summary>
        public void Dispose()
        {
            if (_Disposed) return;
            _Disposed = true;

            try
            {
                double seconds = S3ServerTelemetry.ElapsedSeconds(_StartTimestamp);
                bool failed = _Exception != null;
                S3ServerTelemetry telemetry = _Request.Telemetry;

                telemetry.RecordStage(_Stage, failed ? S3ServerTelemetryNames.OutcomeError : S3ServerTelemetryNames.OutcomeSuccess, seconds);

                string errorType = failed ? S3ServerTelemetry.ErrorTypeOf(_Exception) : null;
                bool serverFault = failed && (!(_Exception is S3Exception s3e) || s3e.HttpStatusCode >= 500);

                if (_Callback != null)
                {
                    string outcome = S3ServerTelemetryNames.OutcomeSuccess;
                    if (failed) outcome = (_Exception is S3Exception) ? S3ServerTelemetryNames.OutcomeS3Error : S3ServerTelemetryNames.OutcomeException;
                    telemetry.RecordCallback(_Callback, outcome, errorType, seconds);
                }

                if (_Activity != null)
                {
                    if (failed)
                    {
                        _Activity.SetTag(S3ServerTelemetryNames.AttributeErrorType, errorType);

                        if (serverFault)
                        {
                            AddExceptionEvent(_Activity, _Exception);
                            _Activity.SetStatus(ActivityStatusCode.Error, errorType);
                        }
                    }

                    _Activity.Stop();
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
