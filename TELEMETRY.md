# S3Server Telemetry

S3Server measures the S3 layer and hands the numbers to whatever your host already runs. It emits metrics through a `System.Diagnostics.Metrics.Meter` and traces through a `System.Diagnostics.ActivitySource`, both named **`S3Server`**, using OpenTelemetry-shaped names. S3Server is a library: it takes no dependency on any exporter or SDK and never opens a connection to a backend. Your host subscribes to the names; when nothing subscribes, instrumentation costs a few listener checks per request and creates no telemetry state.

The goal is operational. Using only dashboards and traces, an on-call engineer can tell **where the time went** (signature validation, your storage callback, XML serialization, or sending the body) and **what failed** (which S3 operation, which error code, which callback, which stage), without reading the source or attaching a debugger.

## Contents

- [Two layers: Watson and S3Server](#two-layers-watson-and-s3server)
- [Enabling and configuring](#enabling-and-configuring)
- [Subscribing a collector](#subscribing-a-collector)
- [Metrics catalog](#metrics-catalog)
- [Attribute values](#attribute-values)
- [Spans catalog](#spans-catalog)
- [Request pipeline and stages](#request-pipeline-and-stages)
- [Histogram buckets](#histogram-buckets)
- [Logs and correlation](#logs-and-correlation)
- [Grafana dashboard](#grafana-dashboard)
- [Recommended alerts (PromQL)](#recommended-alerts-promql)
- [Cardinality and privacy](#cardinality-and-privacy)
- [Cost and failure behavior](#cost-and-failure-behavior)

## Two layers: Watson and S3Server

S3Server runs on [Watson](https://github.com/jchristn/WatsonWebserver) 7.1, which measures the HTTP layer itself: the `Watson` meter and activity source emit `http.server.request.duration`, active requests, body sizes, connection and byte counters, and one `Server` span per request. S3Server does not duplicate any of that.

Watson cannot see S3 semantics. S3Server routes every request through a single default route, so Watson's metrics have no route label and its span is named only by the HTTP method. The `S3Server` meter and source cover everything behind the route:

| Question | Signal |
| --- | --- |
| Which S3 operations are slow or failing? | `s3server.requests`, `s3server.request.duration` by `s3.operation` |
| Is the time in my storage backend or in S3Server? | `s3server.callback.duration`, `s3server.stage.duration` |
| Are clients failing authentication, and why? | `s3server.signature.validations` by outcome |
| What errors are clients getting, and where did they come from? | `s3server.errors` by `error.type` and `s3server.stage` |
| Why was this one request slow? | The `S3 {operation}` span and its `stage:*` and `callback *` children, nested under Watson's server span |

Subscribe to **both** `Watson` and `S3Server`.

## Enabling and configuring

Telemetry is on by default. Settings live under `S3ServerSettings.Telemetry` (type `S3ServerTelemetrySettings`) and are read when the `S3Server` is constructed.

| Setting | Default | Effect |
| --- | --- | --- |
| `Telemetry.Enable` | `true` | Master switch. When false, no Meter or ActivitySource is created. |
| `Telemetry.EnableMetrics` | `true` | Emit metrics on the meter. |
| `Telemetry.EnableTraces` | `true` | Emit spans on the activity source. |
| `Telemetry.MeterName` | `S3Server` | Meter name a collector subscribes to. Change only to tell several servers in one process apart. |
| `Telemetry.ActivitySourceName` | `S3Server` | Activity source name a collector subscribes to. |
| `Telemetry.IncludeBucketNames` | `true` | Add `aws.s3.bucket` to spans. Never on metrics. |
| `Telemetry.IncludeObjectKeys` | `false` | Add `aws.s3.key` to spans. Keys often carry user data, so they are off unless you opt in. Never on metrics. |

Watson's HTTP telemetry is configured separately under `S3ServerSettings.Webserver.Telemetry` (`Enable`, `EnableMetrics`, `EnableTraces`, `PropagateContext`, all `true` by default). Leave `PropagateContext` on so an inbound W3C `traceparent` header is adopted; S3Server's spans then join the caller's trace.

```csharp
S3ServerSettings settings = new S3ServerSettings();
settings.Telemetry.Enable = true;              // default
settings.Telemetry.IncludeObjectKeys = false;  // default; keys can be user data
settings.Webserver.Telemetry.Enable = true;    // Watson HTTP telemetry, default
settings.Webserver.Telemetry.PropagateContext = true;

S3Server server = new S3Server(settings);
```

The meter and activity source names are also available as constants: `S3ServerTelemetryNames.DefaultSourceName`. Every instrument, span, attribute, and bounded attribute value is a constant on `S3ServerTelemetryNames`.

## Subscribing a collector

### Radiant

[Radiant](https://www.nuget.org/packages/Radiant) owns the host side (OpenTelemetry providers, OTLP export, Prometheus endpoint, runtime metrics). In the application that hosts S3Server:

```csharp
RadiantSettings settings = new RadiantSettings("my-s3-service");
settings.Sources.AddMeter("Watson");
settings.Sources.AddActivitySource("Watson");
settings.Sources.AddMeter("S3Server");
settings.Sources.AddActivitySource("S3Server");

using (RadiantHost host = RadiantHost.Start(settings))
{
    using (S3Server server = new S3Server(s3Settings))
    {
        server.Start();
        // run until shutdown
    }
}
```

### OpenTelemetry SDK

```csharp
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

MeterProvider meters = Sdk.CreateMeterProviderBuilder()
    .AddMeter("Watson", "S3Server")
    .AddRuntimeInstrumentation()
    .AddPrometheusHttpListener(o => o.UriPrefixes = new[] { "http://127.0.0.1:9464/" })
    .Build();

TracerProvider traces = Sdk.CreateTracerProviderBuilder()
    .AddSource("Watson", "S3Server")
    .AddOtlpExporter(o => o.Endpoint = new Uri("http://127.0.0.1:4317"))
    .Build();
```

Dispose both providers on shutdown. Runtime metrics (GC, thread pool, exceptions) are a host concern: use Radiant's or `OpenTelemetry.Instrumentation.Runtime`.

### Ad hoc

`dotnet-counters monitor -n <process> --counters S3Server,Watson` shows live values with no code change. `dotnet-trace` can collect the activity sources.

## Metrics catalog

Instrument names are dotted. A Prometheus exporter rewrites them to snake case and appends unit and type suffixes; the last column shows the resulting series name. Attribute keys are rewritten the same way (`s3.operation` becomes `s3_operation`).

| Instrument | Type | Unit | Attributes | Description | Prometheus name |
| --- | --- | --- | --- | --- | --- |
| `s3server.requests` | Counter | `{request}` | `s3.operation`, `s3server.outcome`, `s3server.handler`, `error.type` (failures only) | Completed S3 requests. | `s3server_requests_total` |
| `s3server.request.duration` | Histogram | `s` | `s3.operation`, `s3server.outcome` | End-to-end duration from receipt by S3Server through the response send and PostRequestHandler. | `s3server_request_duration_seconds` |
| `s3server.requests.active` | UpDownCounter | `{request}` | none | Requests currently being processed. | `s3server_requests_active` |
| `s3server.stage.duration` | Histogram | `s` | `s3server.stage`, `s3server.outcome` | Duration of each request pipeline stage. Its `_count` is the per-stage event counter. | `s3server_stage_duration_seconds` |
| `s3server.callback.invocations` | Counter | `{invocation}` | `s3server.callback`, `s3server.outcome`, `error.type` (failures only) | Application callback invocations (your storage integration). | `s3server_callback_invocations_total` |
| `s3server.callback.duration` | Histogram | `s` | `s3server.callback`, `s3server.outcome` | Application callback duration. | `s3server_callback_duration_seconds` |
| `s3server.signature.validations` | Counter | `{validation}` | `s3server.signature.version`, `s3server.outcome` | Signature validations (only when `EnableSignatures` is true). | `s3server_signature_validations_total` |
| `s3server.signature.duration` | Histogram | `s` | `s3server.signature.version`, `s3server.outcome` | Signature validation duration, including `Service.GetSecretKey` and `Service.IsAnonymousRequestAllowed`. | `s3server_signature_duration_seconds` |
| `s3server.errors` | Counter | `{error}` | `error.type`, `s3server.stage` | Failed requests (status 400 and above, plus PostRequestHandler exceptions) by error type and the stage where the error surfaced. | `s3server_errors_total` |
| `s3server.object.size` | Histogram | `By` | `s3.operation` | Object payload size: request `Content-Length` (or `x-amz-decoded-content-length`) for `ObjectWrite` and `ObjectUploadPart`, `S3Object.Size` for `ObjectRead` and `ObjectReadRange`. Its `_sum` rate is object throughput. | `s3server_object_size_bytes` |
| `s3server.lifecycle.events` | Counter | `{event}` | `s3server.lifecycle.event` | `Start`, `Stop`, and disposal. | `s3server_lifecycle_events_total` |
| `s3server.build.info` | ObservableGauge | none | `s3server.version` | Always 1; carries the library version. | `s3server_build_info` |
| `s3server.config.signatures_enabled` | ObservableGauge | none | none | 1 when `EnableSignatures` is true. | `s3server_config_signatures_enabled` |
| `s3server.config.signature_v2_enabled` | ObservableGauge | none | none | 1 when signature V2 validation is active. | `s3server_config_signature_v2_enabled` |
| `s3server.config.max_put_object_size` | ObservableGauge | `By` | none | `OperationLimits.MaxPutObjectSize`. | `s3server_config_max_put_object_size_bytes` |
| `s3server.listening` | ObservableGauge | none | none | 1 while the server is listening. | `s3server_listening` |

The OpenTelemetry Prometheus exporter also adds `otel_scope_name="S3Server"` to every series.

## Attribute values

Every metric attribute is bounded.

| Attribute | Values |
| --- | --- |
| `s3.operation` | An `S3RequestType` name, for example `ObjectRead`, `ObjectWrite`, `BucketRead`, `ListBuckets`, `ObjectUploadPart`, `Unknown`. About 50 values. |
| `s3server.outcome` on requests | `success` (status below 400), `client_error` (4xx), `server_error` (5xx) |
| `s3server.outcome` on stages | `success`, `error` |
| `s3server.outcome` on callbacks | `success`, `s3_error` (callback threw `S3Exception`), `exception` (any other exception; answered as `InternalError`) |
| `s3server.outcome` on signatures | `valid`, `anonymous` (unsigned, permitted by `IsAnonymousRequestAllowed`), `unsigned` (rejected), `unknown_key` (`GetSecretKey` returned nothing), `mismatch`, `expired` (V2 signed URL), `unsupported` (V2 disabled or unknown version), `misconfigured` (`GetSecretKey` not set), `error` |
| `s3server.handler` | `callback`, `pre_request_handler`, `authenticated_request_handler`, `default_request_handler`, `unhandled` (no callback: `NotImplemented`, or `InvalidRequest`), `rejected` (S3Server answered before any handler: parse, validation, or signature failure) |
| `s3server.stage` | `parse`, `pre_request_handler`, `signature_validation`, `authenticated_request_handler`, `deserialize`, `callback`, `serialize`, `send`, `default_request_handler`, `post_request_handler`; on `s3server.errors` also `internal` |
| `s3server.callback` | `{Service,Bucket,Object}.{Member}`, for example `Object.Read`, `Bucket.Write`, `Object.CompleteMultipartUpload`. 47 values. |
| `s3server.signature.version` | `v4`, `v2`, `none` |
| `s3server.lifecycle.event` | `start`, `stop`, `dispose` |
| `error.type` | The S3 `ErrorCode` name (`NoSuchKey`, `AccessDenied`, `SignatureDoesNotMatch`, `InvalidArgument`, `MalformedXML`, ...), or for an unexpected exception the exception type's full name (`System.IO.IOException`). |

`error.type` on `s3server.errors` and `s3server.requests`: when a callback throws an unexpected exception, the exception type is reported rather than the `InternalError` code it is answered with, because the type says what actually broke. When a handled failure is answered with a specific code (for example a body that fails to deserialize, answered with `MalformedXML`), the S3 code is reported and `s3server.stage` shows where it happened (`deserialize`).

## Spans catalog

All spans are created on the `S3Server` activity source. When Watson's tracing is on, the request span is a child of Watson's `Server` span, so one trace runs from the inbound `traceparent` through Watson, S3Server, your callback, and any spans your callback creates (your callback runs with `Activity.Current` set to the `callback *` span).

| Span name | Kind | Parent | Attributes | Status |
| --- | --- | --- | --- | --- |
| `S3 {operation}`, for example `S3 ObjectRead` | `Internal` under Watson, `Server` when no parent exists | Watson's server span | `s3.operation`, `s3.request_style`, `aws.request_id` (the `x-amz-request-id` returned to the client), `aws.s3.bucket`, `aws.s3.key` (opt-in), `aws.s3.upload_id`, `aws.s3.part_number`, `s3server.signature.version`, `s3server.signature.outcome`, `http.response.status_code`, `s3server.handler`, `s3server.outcome`, `error.type` and `s3server.stage` on failure | `Error` on 5xx, with an `exception` event when an exception caused it. 4xx responses are not span errors (OpenTelemetry server convention) but carry `error.type`. |
| `stage:{stage}`, for example `stage:signature_validation` | `Internal` | request span | `s3server.stage`, `error.type` on failure | `Error` with an `exception` event when the stage threw anything other than a 4xx `S3Exception`. |
| `callback {callback}`, for example `callback Object.Read` | `Internal` | request span | `s3server.callback`, `s3server.stage=callback`, `s3.operation`, `error.type` on failure | `Error` with an `exception` event for unexpected exceptions and 5xx `S3Exception`s. |

To find a request a client reported, search Tempo by its request ID: `{ span.aws.request_id = "req_..." }`. Slow object reads: `{ name = "S3 ObjectRead" && duration > 1s }`.

## Request pipeline and stages

Stages appear in this order (optional stages only when configured or applicable):

```
Watson server span (HTTP)
  S3 {operation}
    stage:parse                       S3Context and S3Request construction, FindMatchingBaseDomain, parameter validation
    stage:signature_validation        when ValidateSignaturesBeforePreRequestHandler is true
    stage:pre_request_handler
    stage:signature_validation        default position
    stage:authenticated_request_handler
    stage:deserialize                 XML request bodies (ACL, tagging, CompleteMultipartUpload, DeleteObjects, ...)
    callback {Service|Bucket|Object}.{Member}
    stage:serialize                   XML response bodies
    stage:send                        writing the response
    stage:default_request_handler     when no callback matched and DefaultRequestHandler is set
    stage:post_request_handler
```

Notes for reading the numbers:

- `stage:send` for `ObjectRead` and `ObjectReadRange` includes reading the stream your callback returned, because the body is copied from that stream to the socket. Slow storage reads therefore show up in `send`, not only in `callback`.
- Stages can nest: an error body is serialized inside `send`, and a callback that calls `ctx.Response.Send` itself records a `send` inside `callback`. Summing stage time can exceed request time.
- Chunked responses (`SendChunk`) are not timed per chunk.

## Histogram buckets

On .NET 9 and later (the `net10.0` build), S3Server advises explicit bucket boundaries through `InstrumentAdvice`, which the OpenTelemetry SDK honors:

- Durations (seconds): 0.0005, 0.001, 0.0025, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10, 30, 60, 120
- Object sizes (bytes): 0, 1 KiB, 16 KiB, 64 KiB, 256 KiB, 1 MiB, 4 MiB, 16 MiB, 64 MiB, 256 MiB, 1 GiB, 5 GiB

On `net8.0` and `netstandard2.1` the advice API does not exist, and the OpenTelemetry SDK's default boundaries (designed for milliseconds) would put nearly every duration in the first bucket. Configure a view in the host:

```csharp
double[] seconds = new double[] { 0.0005, 0.001, 0.0025, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10, 30, 60, 120 };

Sdk.CreateMeterProviderBuilder()
    .AddMeter("S3Server")
    .AddView(instrument => instrument.Meter.Name == "S3Server" && instrument.Unit == "s"
        ? new ExplicitBucketHistogramConfiguration { Boundaries = seconds }
        : null)
    // ...
```

Quantiles are never computed in-process; derive p50, p95, and p99 in Grafana with `histogram_quantile`.

## Logs and correlation

S3Server is a request/response library with no background work, so it ships no log pipeline and needs no Loki. Diagnostic text still goes through `S3ServerSettings.Logger`. That callback is invoked inside the request flow, where `Activity.Current` is the S3Server request or stage span, so a host logger can stamp `Activity.Current?.TraceId` and `SpanId` on each line to correlate logs with traces.

## Grafana dashboard

`assets/grafana/s3server.json` is a ready-to-import Grafana dashboard (uid `s3server-overview`) built on the metric names above. It has a Prometheus datasource variable and `job`, `instance`, and `operation` filters. Provision it into your product's Grafana folder next to a Watson HTTP dashboard.

| Row | Panels | Answers |
| --- | --- | --- |
| Overview | Listening, request rate, 5xx and 4xx ratios, p95, in flight, version, signature and size configuration | Is it up and healthy right now? |
| Operations | Rate by operation, outcome, and handler; p95 by operation; p50/p95/p99; top operations by p95 | Which S3 operation is slow or failing? |
| Pipeline stages | p95 by stage, time share by stage, stage failures | Where inside S3Server did the time go? |
| Callbacks (application storage) | Rate by callback and outcome, p95 by callback, exceptions by type, slowest callbacks | Is the storage backend the cause? |
| Signatures and authentication | Validations by version and outcome, p95 validation | Are clients failing auth, and why? |
| Errors | Errors by type, by stage, top errors | What failed and where? |
| Objects | Object size p50/p95, throughput, lifecycle events | What is the data volume? |

## Recommended alerts (PromQL)

```promql
# Server-error ratio above 5% for 5 minutes
sum(rate(s3server_requests_total{s3server_outcome="server_error"}[5m]))
  / clamp_min(sum(rate(s3server_requests_total[5m])), 1e-9) > 0.05

# p95 latency above 2 seconds for an operation (tune per operation)
histogram_quantile(0.95, sum by (le, s3_operation) (rate(s3server_request_duration_seconds_bucket[5m]))) > 2

# Storage callbacks throwing unexpected exceptions
sum by (s3server_callback, error_type) (rate(s3server_callback_invocations_total{s3server_outcome="exception"}[5m])) > 0

# Signature mismatches or unknown keys spiking (clock skew, rotated keys, or probing)
sum(rate(s3server_signature_validations_total{s3server_outcome=~"mismatch|unknown_key|expired"}[5m])) > 1

# Signature validation misconfigured (GetSecretKey not set): every signed request is rejected
sum(rate(s3server_signature_validations_total{s3server_outcome="misconfigured"}[5m])) > 0

# Operations reaching an unimplemented callback
sum by (s3_operation) (rate(s3server_requests_total{s3server_handler="unhandled", error_type="NotImplemented"}[15m])) > 0

# Server not listening
max(s3server_listening) == 0

# Requests piling up (in flight not draining)
sum(s3server_requests_active) > 500
```

## Cardinality and privacy

- Metric attributes are limited to the bounded sets above. Bucket names, object keys, request IDs, upload IDs, access keys, user agents, and client addresses never appear on metrics; the test suite asserts this.
- Worst-case series count is dominated by `s3server.requests` (operations x outcomes x handlers x error types seen) and the duration histograms (about 50 operations x 3 outcomes x 18 buckets). This is bounded regardless of traffic.
- Spans carry `aws.s3.bucket` (configurable) and `aws.request_id`. Object keys are off by default (`IncludeObjectKeys`). Secrets, signatures, credentials, and payloads are never recorded on spans or metrics. Exception events carry the exception type, message, and stack trace, as OpenTelemetry specifies; avoid putting secrets in exception messages your callbacks throw.
- Keep the Prometheus scrape endpoint and the trace backend on an internal network; they have no authentication of their own.

## Cost and failure behavior

- When no listener is attached to the `S3Server` meter or activity source, S3Server creates no per-request telemetry state, timers, or spans; each hook is a null check. Callback dispatch allocates one small delegate per request either way.
- Instrumentation is best-effort: every recording path catches its own exceptions, and a telemetry failure never changes a response.
- The meter and activity source are disposed with the `S3Server`.
