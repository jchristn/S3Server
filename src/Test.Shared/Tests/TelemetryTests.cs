namespace Test.Shared.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.S3.Model;
    using S3ServerLibrary;

    /// <summary>
    /// Proves S3Server emits its metrics and spans: each request path, pipeline stage, callback outcome, signature outcome,
    /// failure path, lifecycle event, and configuration gauge, captured with an in-memory MeterListener and ActivityListener.
    /// Every server uses a unique meter and activity source name so concurrently running tests never see each other's data.
    /// </summary>
    public static class TelemetryTests
    {
        private const string N = S3ServerTelemetryNames.AttributeOperation;

        /// <summary>
        /// Run all telemetry tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, CancellationToken token = default)
        {
            await runner.RunTestAsync("No listener: requests succeed and nothing throws", async (ct) =>
            {
                using (S3TestServer server = new S3TestServer(configure: s => SetNames(s, UniqueName())))
                {
                    HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/hello.txt", ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "object read without listener");

                    response = await server.HttpClient.PutAsync(server.BaseUrl + "/exception-bucket", new ByteArrayContent(Array.Empty<byte>()), ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.InternalServerError, response, "failing callback without listener");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Telemetry disabled: nothing is emitted even with a listener", async (ct) =>
            {
                string name = UniqueName();
                using (TelemetryCollector collector = Collect(name))
                using (S3TestServer server = new S3TestServer(configure: s => { SetNames(s, name); s.Telemetry.Enable = false; }))
                {
                    HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/hello.txt", ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "object read");
                    await Task.Delay(200, ct).ConfigureAwait(false);
                    collector.CollectObservables();
                    AssertHelper.AreEqual(0, collector.Measurements.Count, "measurements with telemetry disabled");
                    AssertHelper.AreEqual(0, collector.Activities.Count, "activities with telemetry disabled");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Object read: request, stage, callback, and size metrics", async (ct) =>
            {
                string name = UniqueName();
                using (TelemetryCollector collector = Collect(name))
                using (S3TestServer server = new S3TestServer(configure: s => SetNames(s, name)))
                {
                    HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/hello.txt", ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "object read");

                    RecordedMeasurement req = await collector.WaitForAsync(S3ServerTelemetryNames.Requests,
                        N, "ObjectRead",
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeSuccess,
                        S3ServerTelemetryNames.AttributeHandler, S3ServerTelemetryNames.HandlerCallback).ConfigureAwait(false);
                    AssertHelper.IsNull(req.Tag(S3ServerTelemetryNames.AttributeErrorType), "error.type on success");

                    RecordedMeasurement duration = await collector.WaitForAsync(S3ServerTelemetryNames.RequestDuration, N, "ObjectRead").ConfigureAwait(false);
                    AssertHelper.AreEqual("s", duration.Unit, "duration unit");
                    AssertHelper.IsTrue(duration.Value >= 0, "duration non-negative");

                    foreach (string stage in new[] { S3ServerTelemetryNames.StageParse, S3ServerTelemetryNames.StagePreRequestHandler, S3ServerTelemetryNames.StageCallback, S3ServerTelemetryNames.StageSend, S3ServerTelemetryNames.StagePostRequestHandler })
                    {
                        await collector.WaitForAsync(S3ServerTelemetryNames.StageDuration,
                            S3ServerTelemetryNames.AttributeStage, stage,
                            S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeSuccess).ConfigureAwait(false);
                    }

                    await collector.WaitForAsync(S3ServerTelemetryNames.CallbackInvocations,
                        S3ServerTelemetryNames.AttributeCallback, "Object.Read",
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeSuccess).ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.CallbackDuration, S3ServerTelemetryNames.AttributeCallback, "Object.Read").ConfigureAwait(false);

                    RecordedMeasurement size = await collector.WaitForAsync(S3ServerTelemetryNames.ObjectSize, N, "ObjectRead").ConfigureAwait(false);
                    AssertHelper.AreEqual("By", size.Unit, "size unit");
                    AssertHelper.IsTrue(size.Value > 0, "object size recorded");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Object read: request span with stage and callback child spans", async (ct) =>
            {
                string name = UniqueName();
                using (TelemetryCollector collector = Collect(name))
                using (S3TestServer server = new S3TestServer(configure: s => SetNames(s, name)))
                {
                    HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/hello.txt", ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "object read");

                    Activity root = await collector.WaitForActivityAsync("S3 ObjectRead").ConfigureAwait(false);
                    AssertHelper.AreEqual("ObjectRead", root.GetTagItem(N) as string, "s3.operation");
                    AssertHelper.AreEqual(server.Bucket, root.GetTagItem(S3ServerTelemetryNames.AttributeBucket) as string, "aws.s3.bucket");
                    AssertHelper.IsNull(root.GetTagItem(S3ServerTelemetryNames.AttributeKey), "aws.s3.key omitted by default");
                    AssertHelper.AreEqual(200, Convert.ToInt32(root.GetTagItem(S3ServerTelemetryNames.AttributeHttpStatusCode)), "status code tag");
                    AssertHelper.IsNotNull(root.GetTagItem(S3ServerTelemetryNames.AttributeRequestId), "aws.request_id");

                    string requestIdHeader = response.Headers.GetValues("x-amz-request-id").First();
                    AssertHelper.AreEqual(requestIdHeader, root.GetTagItem(S3ServerTelemetryNames.AttributeRequestId) as string, "aws.request_id matches x-amz-request-id");
                    AssertHelper.AreNotEqual(ActivityStatusCode.Error, root.Status, "root status on success");

                    Activity callback = await collector.WaitForActivityAsync("callback Object.Read").ConfigureAwait(false);
                    AssertHelper.AreEqual(root.SpanId, callback.ParentSpanId, "callback span parent");
                    AssertHelper.AreEqual(root.TraceId, callback.TraceId, "callback span trace");

                    Activity parse = await collector.WaitForActivityAsync("stage:parse").ConfigureAwait(false);
                    AssertHelper.AreEqual(root.SpanId, parse.ParentSpanId, "parse span parent");

                    Activity send = await collector.WaitForActivityAsync("stage:send").ConfigureAwait(false);
                    AssertHelper.AreEqual(root.TraceId, send.TraceId, "send span trace");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Object write: object key on span when enabled and size from Content-Length", async (ct) =>
            {
                string name = UniqueName();
                using (TelemetryCollector collector = Collect(name))
                using (S3TestServer server = new S3TestServer(configure: s => { SetNames(s, name); s.Telemetry.IncludeObjectKeys = true; }))
                {
                    byte[] body = Encoding.UTF8.GetBytes("telemetry-body");
                    HttpResponseMessage response = await server.HttpClient.PutAsync(server.BaseUrl + "/" + server.Bucket + "/written.txt", new ByteArrayContent(body), ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "object write");

                    RecordedMeasurement size = await collector.WaitForAsync(S3ServerTelemetryNames.ObjectSize, N, "ObjectWrite").ConfigureAwait(false);
                    AssertHelper.AreEqual((double)body.Length, size.Value, "write size");

                    Activity root = await collector.WaitForActivityAsync("S3 ObjectWrite").ConfigureAwait(false);
                    AssertHelper.AreEqual("written.txt", root.GetTagItem(S3ServerTelemetryNames.AttributeKey) as string, "aws.s3.key when enabled");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Callback S3Exception: s3_error callback, client_error request, error by stage", async (ct) =>
            {
                string name = UniqueName();
                using (TelemetryCollector collector = Collect(name))
                using (S3TestServer server = new S3TestServer(configure: s => SetNames(s, name)))
                {
                    HttpResponseMessage response = await server.HttpClient.PutAsync(server.BaseUrl + "/access-denied-bucket", new ByteArrayContent(Array.Empty<byte>()), ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "access denied");

                    await collector.WaitForAsync(S3ServerTelemetryNames.CallbackInvocations,
                        S3ServerTelemetryNames.AttributeCallback, "Bucket.Write",
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeS3Error,
                        S3ServerTelemetryNames.AttributeErrorType, "AccessDenied").ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.Requests,
                        N, "BucketWrite",
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeClientError,
                        S3ServerTelemetryNames.AttributeErrorType, "AccessDenied").ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.Errors,
                        S3ServerTelemetryNames.AttributeErrorType, "AccessDenied",
                        S3ServerTelemetryNames.AttributeStage, S3ServerTelemetryNames.StageCallback).ConfigureAwait(false);

                    Activity root = await collector.WaitForActivityAsync("S3 BucketWrite").ConfigureAwait(false);
                    AssertHelper.AreNotEqual(ActivityStatusCode.Error, root.Status, "4xx is not a span error");
                    AssertHelper.AreEqual("AccessDenied", root.GetTagItem(S3ServerTelemetryNames.AttributeErrorType) as string, "error.type on span");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Callback exception: exception outcome, server_error, span error with exception event", async (ct) =>
            {
                string name = UniqueName();
                using (TelemetryCollector collector = Collect(name))
                using (S3TestServer server = new S3TestServer(configure: s => SetNames(s, name)))
                {
                    HttpResponseMessage response = await server.HttpClient.PutAsync(server.BaseUrl + "/exception-bucket", new ByteArrayContent(Array.Empty<byte>()), ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.InternalServerError, response, "callback exception");

                    await collector.WaitForAsync(S3ServerTelemetryNames.CallbackInvocations,
                        S3ServerTelemetryNames.AttributeCallback, "Bucket.Write",
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeException,
                        S3ServerTelemetryNames.AttributeErrorType, "System.InvalidOperationException").ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.Requests,
                        N, "BucketWrite",
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeServerError,
                        S3ServerTelemetryNames.AttributeErrorType, "System.InvalidOperationException").ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.Errors,
                        S3ServerTelemetryNames.AttributeErrorType, "System.InvalidOperationException",
                        S3ServerTelemetryNames.AttributeStage, S3ServerTelemetryNames.StageCallback).ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.StageDuration,
                        S3ServerTelemetryNames.AttributeStage, S3ServerTelemetryNames.StageCallback,
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeError).ConfigureAwait(false);

                    Activity callback = await collector.WaitForActivityAsync("callback Bucket.Write").ConfigureAwait(false);
                    AssertHelper.AreEqual(ActivityStatusCode.Error, callback.Status, "callback span status");
                    AssertHelper.IsTrue(callback.Events.Any(e => e.Name == "exception"), "callback span exception event");

                    Activity root = await collector.WaitForActivityAsync("S3 BucketWrite").ConfigureAwait(false);
                    AssertHelper.AreEqual(ActivityStatusCode.Error, root.Status, "request span status on 5xx");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Missing object: NoSuchKey answered without an exception is counted", async (ct) =>
            {
                string name = UniqueName();
                using (TelemetryCollector collector = Collect(name))
                using (S3TestServer server = new S3TestServer(configure: s => SetNames(s, name)))
                {
                    HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/nonexistent-object-xyz.bin", ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.NotFound, response, "missing object");

                    await collector.WaitForAsync(S3ServerTelemetryNames.Requests,
                        N, "ObjectRead",
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeClientError,
                        S3ServerTelemetryNames.AttributeErrorType, "NoSuchKey").ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.Errors,
                        S3ServerTelemetryNames.AttributeErrorType, "NoSuchKey",
                        S3ServerTelemetryNames.AttributeStage, S3ServerTelemetryNames.StageInternal).ConfigureAwait(false);
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Validation failure: rejected handler and parse-stage error", async (ct) =>
            {
                string name = UniqueName();
                using (TelemetryCollector collector = Collect(name))
                using (S3TestServer server = new S3TestServer(configure: s => SetNames(s, name)))
                {
                    HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "?max-keys=abc", ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.BadRequest, response, "invalid max-keys");

                    await collector.WaitForAsync(S3ServerTelemetryNames.Requests,
                        S3ServerTelemetryNames.AttributeHandler, S3ServerTelemetryNames.HandlerRejected,
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeClientError,
                        S3ServerTelemetryNames.AttributeErrorType, "InvalidArgument").ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.Errors,
                        S3ServerTelemetryNames.AttributeErrorType, "InvalidArgument",
                        S3ServerTelemetryNames.AttributeStage, S3ServerTelemetryNames.StageParse).ConfigureAwait(false);
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Malformed XML: deserialize stage error and MalformedXML", async (ct) =>
            {
                string name = UniqueName();
                using (TelemetryCollector collector = Collect(name))
                using (S3TestServer server = new S3TestServer(configure: s => SetNames(s, name)))
                {
                    HttpResponseMessage response = await server.HttpClient.PutAsync(server.BaseUrl + "/" + server.Bucket + "?tagging", new StringContent("<not-xml", Encoding.UTF8, "application/xml"), ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.BadRequest, response, "malformed tagging");

                    await collector.WaitForAsync(S3ServerTelemetryNames.StageDuration,
                        S3ServerTelemetryNames.AttributeStage, S3ServerTelemetryNames.StageDeserialize,
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeError).ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.Errors,
                        S3ServerTelemetryNames.AttributeErrorType, "MalformedXML",
                        S3ServerTelemetryNames.AttributeStage, S3ServerTelemetryNames.StageDeserialize).ConfigureAwait(false);
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Default handler and post-request handler failure are attributed", async (ct) =>
            {
                string name = UniqueName();
                using (TelemetryCollector collector = Collect(name))
                using (S3TestServer server = new S3TestServer(configure: s => SetNames(s, name)))
                {
                    HttpRequestMessage options = new HttpRequestMessage(HttpMethod.Options, server.BaseUrl + "/");
                    HttpResponseMessage response = await server.HttpClient.SendAsync(options, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "default handler");

                    await collector.WaitForAsync(S3ServerTelemetryNames.Requests,
                        S3ServerTelemetryNames.AttributeHandler, S3ServerTelemetryNames.HandlerDefault).ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.StageDuration,
                        S3ServerTelemetryNames.AttributeStage, S3ServerTelemetryNames.StageDefaultRequestHandler).ConfigureAwait(false);

                    response = await server.HttpClient.PutAsync(server.BaseUrl + "/post-handler-exception-bucket", new ByteArrayContent(Array.Empty<byte>()), ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "post handler exception does not change response");

                    await collector.WaitForAsync(S3ServerTelemetryNames.Errors,
                        S3ServerTelemetryNames.AttributeErrorType, "System.InvalidOperationException",
                        S3ServerTelemetryNames.AttributeStage, S3ServerTelemetryNames.StagePostRequestHandler).ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.StageDuration,
                        S3ServerTelemetryNames.AttributeStage, S3ServerTelemetryNames.StagePostRequestHandler,
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeError).ConfigureAwait(false);
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Unhandled and hook-terminated requests are attributed", async (ct) =>
            {
                string name = UniqueName();
                int port = S3TestServer.GetAvailablePort();
                S3ServerSettings settings = new S3ServerSettings();
                settings.Webserver.Hostname = "127.0.0.1";
                settings.Webserver.Port = port;
                SetNames(settings, name);
                settings.PreRequestHandler = async (ctx) =>
                {
                    if (ctx.Request.Bucket != "hooked") return false;
                    ctx.Response.StatusCode = 200;
                    return true;
                };

                using (TelemetryCollector collector = Collect(name))
                using (S3Server minimal = new S3Server(settings))
                using (HttpClient client = new HttpClient())
                {
                    minimal.Start();

                    HttpResponseMessage response = await client.GetAsync("http://127.0.0.1:" + port + "/", ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.NotImplemented, response, "no ListBuckets callback");

                    await collector.WaitForAsync(S3ServerTelemetryNames.Requests,
                        N, "ListBuckets",
                        S3ServerTelemetryNames.AttributeHandler, S3ServerTelemetryNames.HandlerUnhandled,
                        S3ServerTelemetryNames.AttributeErrorType, "NotImplemented").ConfigureAwait(false);

                    response = await client.GetAsync("http://127.0.0.1:" + port + "/hooked", ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "pre-request handler response");

                    await collector.WaitForAsync(S3ServerTelemetryNames.Requests,
                        S3ServerTelemetryNames.AttributeHandler, S3ServerTelemetryNames.HandlerPreRequest,
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeSuccess).ConfigureAwait(false);
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Signature validation outcomes: valid, mismatch, unknown_key, unsigned", async (ct) =>
            {
                string name = UniqueName();
                using (TelemetryCollector collector = Collect(name))
                using (S3TestServer server = new S3TestServer(enableSignatures: true, configure: s => SetNames(s, name)))
                {
                    await server.S3Client.ListBucketsAsync(ct).ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.SignatureValidations,
                        S3ServerTelemetryNames.AttributeSignatureVersion, S3ServerTelemetryNames.SignatureVersion4,
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeValid).ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.SignatureDuration,
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeValid).ConfigureAwait(false);

                    using (IAmazonS3 wrongSecret = CreateClient(server, server.AccessKey, "wrong-secret-key-wrong-secret-key-0000"))
                    {
                        await ExpectFailureAsync(() => wrongSecret.ListBucketsAsync(ct)).ConfigureAwait(false);
                    }

                    await collector.WaitForAsync(S3ServerTelemetryNames.SignatureValidations,
                        S3ServerTelemetryNames.AttributeSignatureVersion, S3ServerTelemetryNames.SignatureVersion4,
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeMismatch).ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.Errors,
                        S3ServerTelemetryNames.AttributeErrorType, "SignatureDoesNotMatch",
                        S3ServerTelemetryNames.AttributeStage, S3ServerTelemetryNames.StageSignatureValidation).ConfigureAwait(false);

                    using (IAmazonS3 unknownKey = CreateClient(server, "AKIAUNKNOWNKEY000000", server.SecretKey))
                    {
                        await ExpectFailureAsync(() => unknownKey.ListBucketsAsync(ct)).ConfigureAwait(false);
                    }

                    await collector.WaitForAsync(S3ServerTelemetryNames.SignatureValidations,
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeUnknownKey).ConfigureAwait(false);

                    HttpResponseMessage unsigned = await server.HttpClient.GetAsync(server.BaseUrl + "/", ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, unsigned, "unsigned request");

                    await collector.WaitForAsync(S3ServerTelemetryNames.SignatureValidations,
                        S3ServerTelemetryNames.AttributeSignatureVersion, S3ServerTelemetryNames.SignatureVersionNone,
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeUnsigned).ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.StageDuration,
                        S3ServerTelemetryNames.AttributeStage, S3ServerTelemetryNames.StageSignatureValidation,
                        S3ServerTelemetryNames.AttributeOutcome, S3ServerTelemetryNames.OutcomeError).ConfigureAwait(false);
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("W3C traceparent is adopted through the Watson server span", async (ct) =>
            {
                string name = UniqueName();
                using (TelemetryCollector collector = new TelemetryCollector(new[] { name }, new[] { name, "Watson" }))
                using (S3TestServer server = new S3TestServer(configure: s => SetNames(s, name)))
                {
                    ActivityTraceId traceId = ActivityTraceId.CreateRandom();
                    ActivitySpanId parentId = ActivitySpanId.CreateRandom();

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/hello.txt");
                    request.Headers.TryAddWithoutValidation("traceparent", "00-" + traceId.ToHexString() + "-" + parentId.ToHexString() + "-01");
                    HttpResponseMessage response = await server.HttpClient.SendAsync(request, ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "object read with traceparent");

                    Activity root = await collector.WaitForActivityAsync("S3 ObjectRead").ConfigureAwait(false);
                    AssertHelper.AreEqual(traceId, root.TraceId, "request span joins the inbound trace");
                    AssertHelper.AreEqual(ActivityKind.Internal, root.Kind, "request span kind under Watson");

                    Activity watson = collector.Activities.FirstOrDefault(a => a.Source.Name == "Watson" && a.TraceId == traceId);
                    AssertHelper.IsNotNull(watson, "Watson server span in the same trace");
                    AssertHelper.AreEqual(watson.SpanId, root.ParentSpanId, "request span is a child of the Watson span");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Concurrent requests: active request gauge returns to zero", async (ct) =>
            {
                string name = UniqueName();
                using (TelemetryCollector collector = Collect(name))
                using (S3TestServer server = new S3TestServer(configure: s => SetNames(s, name)))
                {
                    List<Task<HttpResponseMessage>> tasks = new List<Task<HttpResponseMessage>>();
                    for (int i = 0; i < 10; i++) tasks.Add(server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/obj-" + i + ".txt", ct));
                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    DateTime deadline = DateTime.UtcNow.AddSeconds(5);
                    while (collector.Find(S3ServerTelemetryNames.Requests, N, "ObjectRead").Count < 10 && DateTime.UtcNow < deadline)
                        await Task.Delay(20, ct).ConfigureAwait(false);

                    AssertHelper.AreEqual(10, collector.Find(S3ServerTelemetryNames.Requests, N, "ObjectRead").Count, "completed request count");

                    List<RecordedMeasurement> active = collector.Find(S3ServerTelemetryNames.RequestsActive);
                    AssertHelper.IsTrue(active.Any(m => m.Value > 0), "active increments recorded");
                    AssertHelper.AreEqual(0.0, active.Sum(m => m.Value), "active requests net to zero");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Lifecycle events and build and config gauges", async (ct) =>
            {
                string name = UniqueName();
                using (TelemetryCollector collector = Collect(name))
                {
                    using (S3TestServer server = new S3TestServer(enableSignatures: true, configure: s => SetNames(s, name)))
                    {
                        await collector.WaitForAsync(S3ServerTelemetryNames.LifecycleEvents,
                            S3ServerTelemetryNames.AttributeLifecycleEvent, S3ServerTelemetryNames.LifecycleStart).ConfigureAwait(false);

                        collector.CollectObservables();

                        RecordedMeasurement build = collector.Find(S3ServerTelemetryNames.BuildInfo).FirstOrDefault();
                        AssertHelper.IsNotNull(build, "build info gauge");
                        AssertHelper.AreEqual(1.0, build.Value, "build info value");
                        AssertHelper.IsFalse(String.IsNullOrEmpty(build.Tag(S3ServerTelemetryNames.AttributeVersion)), "build info version");

                        AssertHelper.AreEqual(1.0, collector.Find(S3ServerTelemetryNames.ConfigSignaturesEnabled).First().Value, "signatures enabled gauge");
                        AssertHelper.AreEqual(0.0, collector.Find(S3ServerTelemetryNames.ConfigSignatureV2Enabled).First().Value, "signature v2 gauge");
                        AssertHelper.AreEqual(1024.0, collector.Find(S3ServerTelemetryNames.ConfigMaxPutObjectSize).First().Value, "max put size gauge");
                        AssertHelper.AreEqual(1.0, collector.Find(S3ServerTelemetryNames.Listening).First().Value, "listening gauge");
                    }

                    await collector.WaitForAsync(S3ServerTelemetryNames.LifecycleEvents,
                        S3ServerTelemetryNames.AttributeLifecycleEvent, S3ServerTelemetryNames.LifecycleStop).ConfigureAwait(false);
                    await collector.WaitForAsync(S3ServerTelemetryNames.LifecycleEvents,
                        S3ServerTelemetryNames.AttributeLifecycleEvent, S3ServerTelemetryNames.LifecycleDispose).ConfigureAwait(false);
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Metric attributes are bounded: no bucket, key, or request id on metrics", async (ct) =>
            {
                string name = UniqueName();
                using (TelemetryCollector collector = Collect(name))
                using (S3TestServer server = new S3TestServer(configure: s => { SetNames(s, name); s.Telemetry.IncludeObjectKeys = true; }))
                {
                    HttpResponseMessage response = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/hello.txt", ct).ConfigureAwait(false);
                    AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "object read");
                    await collector.WaitForAsync(S3ServerTelemetryNames.Requests, N, "ObjectRead").ConfigureAwait(false);

                    HashSet<string> allowed = new HashSet<string>(StringComparer.Ordinal)
                    {
                        S3ServerTelemetryNames.AttributeOperation,
                        S3ServerTelemetryNames.AttributeOutcome,
                        S3ServerTelemetryNames.AttributeHandler,
                        S3ServerTelemetryNames.AttributeStage,
                        S3ServerTelemetryNames.AttributeCallback,
                        S3ServerTelemetryNames.AttributeSignatureVersion,
                        S3ServerTelemetryNames.AttributeLifecycleEvent,
                        S3ServerTelemetryNames.AttributeVersion,
                        S3ServerTelemetryNames.AttributeErrorType
                    };

                    foreach (RecordedMeasurement m in collector.Measurements)
                    {
                        foreach (string key in m.Tags.Keys)
                            AssertHelper.IsTrue(allowed.Contains(key), "unexpected metric attribute " + key + " on " + m.Instrument);
                    }
                }
            }, token).ConfigureAwait(false);
        }

        private static string UniqueName()
        {
            return "S3Server.Test." + Guid.NewGuid().ToString("N");
        }

        private static void SetNames(S3ServerSettings settings, string name)
        {
            settings.Telemetry.MeterName = name;
            settings.Telemetry.ActivitySourceName = name;
        }

        private static TelemetryCollector Collect(string name)
        {
            return new TelemetryCollector(new[] { name }, new[] { name });
        }

        private static IAmazonS3 CreateClient(S3TestServer server, string accessKey, string secretKey)
        {
            AmazonS3Config config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USWest1,
                ServiceURL = server.BaseUrl + "/",
                ForcePathStyle = true,
                UseHttp = true,
                MaxErrorRetry = 0,
                Timeout = TimeSpan.FromSeconds(5)
            };

            return new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), config);
        }

        private static async Task ExpectFailureAsync(Func<Task> action)
        {
            try
            {
                await action().ConfigureAwait(false);
            }
            catch (AmazonS3Exception)
            {
                return;
            }

            throw new InvalidOperationException("Expected the request to be rejected.");
        }
    }
}
