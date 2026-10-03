namespace S3ServerLibrary
{
    /// <summary>
    /// Stable names for every meter, activity source, instrument, span, attribute, and bounded attribute value S3Server emits.
    /// These strings are a public contract consumed by collectors, dashboards, and alerts; they do not change between minor versions.
    /// Instrument names are dotted (OpenTelemetry style).  A Prometheus exporter rewrites them to snake case and appends unit and
    /// type suffixes, for example s3server.request.duration becomes s3server_request_duration_seconds.
    /// Thread-safe: all members are constants.
    /// </summary>
    public static class S3ServerTelemetryNames
    {
        #region Sources

        /// <summary>
        /// Default name of the Meter and the ActivitySource: S3Server.
        /// Override with S3ServerTelemetrySettings.MeterName and ActivitySourceName.
        /// </summary>
        public const string DefaultSourceName = "S3Server";

        #endregion

        #region Metrics

        /// <summary>
        /// Counter of completed S3 requests ({request}).  Attributes: s3.operation, s3server.outcome, s3server.handler, error.type (on failures).
        /// </summary>
        public const string Requests = "s3server.requests";

        /// <summary>
        /// Histogram of end-to-end S3 request duration in seconds, from receipt by S3Server through the response send.  Attributes: s3.operation, s3server.outcome.
        /// </summary>
        public const string RequestDuration = "s3server.request.duration";

        /// <summary>
        /// Up-down counter of S3 requests currently being processed ({request}).  No attributes.
        /// </summary>
        public const string RequestsActive = "s3server.requests.active";

        /// <summary>
        /// Histogram of per-stage duration in seconds for each stage of the request pipeline.  Attributes: s3server.stage, s3server.outcome.
        /// </summary>
        public const string StageDuration = "s3server.stage.duration";

        /// <summary>
        /// Counter of application callback invocations ({invocation}).  Attributes: s3server.callback, s3server.outcome, error.type (on failures).
        /// </summary>
        public const string CallbackInvocations = "s3server.callback.invocations";

        /// <summary>
        /// Histogram of application callback duration in seconds.  Attributes: s3server.callback, s3server.outcome.
        /// </summary>
        public const string CallbackDuration = "s3server.callback.duration";

        /// <summary>
        /// Counter of signature validations ({validation}).  Attributes: s3server.signature.version, s3server.outcome.
        /// </summary>
        public const string SignatureValidations = "s3server.signature.validations";

        /// <summary>
        /// Histogram of signature validation duration in seconds, including Service.GetSecretKey and Service.IsAnonymousRequestAllowed.
        /// Attributes: s3server.signature.version, s3server.outcome.
        /// </summary>
        public const string SignatureDuration = "s3server.signature.duration";

        /// <summary>
        /// Counter of errors ({error}) by where they surfaced.  Attributes: error.type, s3server.stage.
        /// </summary>
        public const string Errors = "s3server.errors";

        /// <summary>
        /// Histogram of object payload sizes in bytes: the request Content-Length for writes and S3Object.Size for reads.  Attributes: s3.operation.
        /// </summary>
        public const string ObjectSize = "s3server.object.size";

        /// <summary>
        /// Counter of server lifecycle events ({event}).  Attributes: s3server.lifecycle.event.
        /// </summary>
        public const string LifecycleEvents = "s3server.lifecycle.events";

        /// <summary>
        /// Observable gauge, always 1, carrying the library version.  Attributes: s3server.version.
        /// </summary>
        public const string BuildInfo = "s3server.build.info";

        /// <summary>
        /// Observable gauge: 1 when signature validation is enabled, otherwise 0.
        /// </summary>
        public const string ConfigSignaturesEnabled = "s3server.config.signatures_enabled";

        /// <summary>
        /// Observable gauge: 1 when legacy signature V2 validation is enabled, otherwise 0.
        /// </summary>
        public const string ConfigSignatureV2Enabled = "s3server.config.signature_v2_enabled";

        /// <summary>
        /// Observable gauge: configured OperationLimits.MaxPutObjectSize in bytes.
        /// </summary>
        public const string ConfigMaxPutObjectSize = "s3server.config.max_put_object_size";

        /// <summary>
        /// Observable gauge: 1 while the server is listening, otherwise 0.
        /// </summary>
        public const string Listening = "s3server.listening";

        #endregion

        #region Spans

        /// <summary>
        /// Prefix of the per-request span name.  The full name is "S3 {operation}", for example "S3 ObjectRead".
        /// </summary>
        public const string RequestSpanPrefix = "S3 ";

        /// <summary>
        /// Prefix of per-stage span names.  The full name is "stage:{stage}", for example "stage:signature_validation".
        /// </summary>
        public const string StageSpanPrefix = "stage:";

        /// <summary>
        /// Prefix of application callback span names.  The full name is "callback {callback}", for example "callback Object.Read".
        /// </summary>
        public const string CallbackSpanPrefix = "callback ";

        #endregion

        #region Attributes

        /// <summary>
        /// S3 operation, the S3RequestType name (for example ObjectRead).  Bounded by the S3RequestType enumeration.
        /// </summary>
        public const string AttributeOperation = "s3.operation";

        /// <summary>
        /// Request outcome.  See the Outcome* constants for the values used by each instrument.
        /// </summary>
        public const string AttributeOutcome = "s3server.outcome";

        /// <summary>
        /// Which component produced the response.  See the Handler* constants.
        /// </summary>
        public const string AttributeHandler = "s3server.handler";

        /// <summary>
        /// Request pipeline stage.  See the Stage* constants.
        /// </summary>
        public const string AttributeStage = "s3server.stage";

        /// <summary>
        /// Application callback name, "{Service|Bucket|Object}.{Member}", for example Object.Write.  Bounded by the callback classes.
        /// </summary>
        public const string AttributeCallback = "s3server.callback";

        /// <summary>
        /// Signature version: v2, v4, or none.
        /// </summary>
        public const string AttributeSignatureVersion = "s3server.signature.version";

        /// <summary>
        /// Signature outcome on the request span (span only).  Same values as s3server.outcome on s3server.signature.validations.
        /// </summary>
        public const string AttributeSignatureOutcome = "s3server.signature.outcome";

        /// <summary>
        /// Lifecycle event: start, stop, or dispose.
        /// </summary>
        public const string AttributeLifecycleEvent = "s3server.lifecycle.event";

        /// <summary>
        /// Library version on the build info gauge.
        /// </summary>
        public const string AttributeVersion = "s3server.version";

        /// <summary>
        /// Error type (OpenTelemetry semantic convention): the S3 ErrorCode name for S3 errors, or the exception type's full name.
        /// </summary>
        public const string AttributeErrorType = "error.type";

        /// <summary>
        /// HTTP response status code (span only).
        /// </summary>
        public const string AttributeHttpStatusCode = "http.response.status_code";

        /// <summary>
        /// Request style: PathStyle, VirtualHostedStyle, or Unknown (span only).
        /// </summary>
        public const string AttributeRequestStyle = "s3.request_style";

        /// <summary>
        /// Bucket name (span only, OpenTelemetry semantic convention).  Omitted when S3ServerTelemetrySettings.IncludeBucketNames is false.
        /// </summary>
        public const string AttributeBucket = "aws.s3.bucket";

        /// <summary>
        /// Object key (span only, OpenTelemetry semantic convention).  Emitted only when S3ServerTelemetrySettings.IncludeObjectKeys is true.
        /// </summary>
        public const string AttributeKey = "aws.s3.key";

        /// <summary>
        /// Multipart upload ID (span only, OpenTelemetry semantic convention).
        /// </summary>
        public const string AttributeUploadId = "aws.s3.upload_id";

        /// <summary>
        /// Multipart part number (span only, OpenTelemetry semantic convention).
        /// </summary>
        public const string AttributePartNumber = "aws.s3.part_number";

        /// <summary>
        /// The x-amz-request-id returned to the client (span only, OpenTelemetry semantic convention), so a client-reported
        /// request ID can be found in the trace backend.
        /// </summary>
        public const string AttributeRequestId = "aws.request_id";

        #endregion

        #region Outcomes

        /// <summary>
        /// Outcome: the operation succeeded (HTTP status below 400, or a callback that returned normally).
        /// </summary>
        public const string OutcomeSuccess = "success";

        /// <summary>
        /// Outcome: the request failed with a 4xx response.
        /// </summary>
        public const string OutcomeClientError = "client_error";

        /// <summary>
        /// Outcome: the request failed with a 5xx response.
        /// </summary>
        public const string OutcomeServerError = "server_error";

        /// <summary>
        /// Outcome: a stage or callback failed.
        /// </summary>
        public const string OutcomeError = "error";

        /// <summary>
        /// Outcome: a callback threw an S3Exception (an S3 error such as NoSuchKey, answered to the client).
        /// </summary>
        public const string OutcomeS3Error = "s3_error";

        /// <summary>
        /// Outcome: a callback threw an exception other than S3Exception (answered as InternalError).
        /// </summary>
        public const string OutcomeException = "exception";

        /// <summary>
        /// Signature outcome: the signature matched.
        /// </summary>
        public const string OutcomeValid = "valid";

        /// <summary>
        /// Signature outcome: an unsigned request was permitted by Service.IsAnonymousRequestAllowed.
        /// </summary>
        public const string OutcomeAnonymous = "anonymous";

        /// <summary>
        /// Signature outcome: an unsigned request was rejected.
        /// </summary>
        public const string OutcomeUnsigned = "unsigned";

        /// <summary>
        /// Signature outcome: Service.GetSecretKey returned no secret for the access key.
        /// </summary>
        public const string OutcomeUnknownKey = "unknown_key";

        /// <summary>
        /// Signature outcome: the signature did not match.
        /// </summary>
        public const string OutcomeMismatch = "mismatch";

        /// <summary>
        /// Signature outcome: a signed URL had expired or carried an invalid expiration.
        /// </summary>
        public const string OutcomeExpired = "expired";

        /// <summary>
        /// Signature outcome: the signature version is disabled (V2 without EnableSignatureV2) or not recognized.
        /// </summary>
        public const string OutcomeUnsupported = "unsupported";

        /// <summary>
        /// Signature outcome: signature validation is enabled but Service.GetSecretKey is not set.
        /// </summary>
        public const string OutcomeMisconfigured = "misconfigured";

        #endregion

        #region Handlers

        /// <summary>
        /// Handler: an operation callback (Service, Bucket, or Object) produced the response.
        /// </summary>
        public const string HandlerCallback = "callback";

        /// <summary>
        /// Handler: S3ServerSettings.PreRequestHandler returned true and terminated the request.
        /// </summary>
        public const string HandlerPreRequest = "pre_request_handler";

        /// <summary>
        /// Handler: S3ServerSettings.AuthenticatedRequestHandler returned true and terminated the request.
        /// </summary>
        public const string HandlerAuthenticatedRequest = "authenticated_request_handler";

        /// <summary>
        /// Handler: S3ServerSettings.DefaultRequestHandler handled a request with no matching callback.
        /// </summary>
        public const string HandlerDefault = "default_request_handler";

        /// <summary>
        /// Handler: no callback was registered (NotImplemented) or the request was not recognized (InvalidRequest).
        /// </summary>
        public const string HandlerUnhandled = "unhandled";

        /// <summary>
        /// Handler: S3Server answered with an error before any handler ran (parse, validation, or signature failure).
        /// </summary>
        public const string HandlerRejected = "rejected";

        #endregion

        #region Stages

        /// <summary>
        /// Stage: parsing the HTTP request into an S3Context and S3Request, including FindMatchingBaseDomain and parameter validation.
        /// </summary>
        public const string StageParse = "parse";

        /// <summary>
        /// Stage: S3ServerSettings.PreRequestHandler.
        /// </summary>
        public const string StagePreRequestHandler = "pre_request_handler";

        /// <summary>
        /// Stage: signature validation.
        /// </summary>
        public const string StageSignatureValidation = "signature_validation";

        /// <summary>
        /// Stage: S3ServerSettings.AuthenticatedRequestHandler.
        /// </summary>
        public const string StageAuthenticatedRequestHandler = "authenticated_request_handler";

        /// <summary>
        /// Stage: deserializing an XML request body.
        /// </summary>
        public const string StageDeserialize = "deserialize";

        /// <summary>
        /// Stage: the application's operation callback.
        /// </summary>
        public const string StageCallback = "callback";

        /// <summary>
        /// Stage: serializing an XML response body.
        /// </summary>
        public const string StageSerialize = "serialize";

        /// <summary>
        /// Stage: writing the response to the client.  For object reads this includes reading the callback's stream.
        /// </summary>
        public const string StageSend = "send";

        /// <summary>
        /// Stage: S3ServerSettings.DefaultRequestHandler.
        /// </summary>
        public const string StageDefaultRequestHandler = "default_request_handler";

        /// <summary>
        /// Stage: S3ServerSettings.PostRequestHandler.
        /// </summary>
        public const string StagePostRequestHandler = "post_request_handler";

        /// <summary>
        /// Error stage: the failure surfaced while routing or in S3Server itself rather than in a timed stage.
        /// </summary>
        public const string StageInternal = "internal";

        #endregion

        #region Signature-Versions

        /// <summary>
        /// Signature version label for AWS Signature Version 2.
        /// </summary>
        public const string SignatureVersion2 = "v2";

        /// <summary>
        /// Signature version label for AWS Signature Version 4.
        /// </summary>
        public const string SignatureVersion4 = "v4";

        /// <summary>
        /// Signature version label for unsigned requests or an unrecognized version.
        /// </summary>
        public const string SignatureVersionNone = "none";

        #endregion

        #region Lifecycle

        /// <summary>
        /// Lifecycle event: Start was called.
        /// </summary>
        public const string LifecycleStart = "start";

        /// <summary>
        /// Lifecycle event: Stop was called.
        /// </summary>
        public const string LifecycleStop = "stop";

        /// <summary>
        /// Lifecycle event: the server was disposed.
        /// </summary>
        public const string LifecycleDispose = "dispose";

        #endregion
    }
}
