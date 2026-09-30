namespace Test.Shared.Tests
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.S3.Model;
    using S3ServerLibrary;
    using S3ServerLibrary.S3Objects;

    /// <summary>
    /// Request pipeline ordering tests: PreRequestHandler versus signature validation, AuthenticatedRequestHandler,
    /// and fail-closed behavior when Service.GetSecretKey is cleared at runtime.
    /// These tests run against a server instance with EnableSignatures = true.
    /// </summary>
    public static class RequestPipelineTests
    {
        /// <summary>
        /// Run all request pipeline tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">S3 test server with signatures enabled.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            await runner.RunTestAsync("Default order lets PreRequestHandler answer a forged request (documented behavior)", async (ct) =>
            {
                Func<S3Context, Task<bool>> original = server.Server.Settings.PreRequestHandler;
                server.Server.Settings.PreRequestHandler = async (ctx) =>
                {
                    await original(ctx).ConfigureAwait(false);
                    if (ctx.Request.Key != "pre-answered.txt") return false;

                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentType = "text/plain";
                    await ctx.Response.Send("pre-handler").ConfigureAwait(false);
                    return true;
                };

                HttpResponseMessage response = await server.HttpClient.SendAsync(ForgedRequest(server, "pre-answered.txt"), ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, response, "pre-handler answered forged request");
                AssertHelper.AreEqual("pre-handler", await response.Content.ReadAsStringAsync().ConfigureAwait(false), "pre-handler body");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ValidateSignaturesBeforePreRequestHandler rejects a forged request before PreRequestHandler", async (ct) =>
            {
                int preHandlerCalls = 0;
                server.Server.Settings.ValidateSignaturesBeforePreRequestHandler = true;
                server.Server.Settings.PreRequestHandler = async (ctx) =>
                {
                    Interlocked.Increment(ref preHandlerCalls);
                    ctx.Response.StatusCode = 200;
                    await ctx.Response.Send("pre-handler").ConfigureAwait(false);
                    return true;
                };

                HttpResponseMessage response = await server.HttpClient.SendAsync(ForgedRequest(server, "pre-answered.txt"), ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, response, "forged request rejected");
                AssertHelper.StringContains(await response.Content.ReadAsStringAsync().ConfigureAwait(false), "SignatureDoesNotMatch", "error code");
                AssertHelper.AreEqual(0, preHandlerCalls, "PreRequestHandler not invoked");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ValidateSignaturesBeforePreRequestHandler still runs PreRequestHandler for valid requests", async (ct) =>
            {
                int preHandlerCalls = 0;
                server.Server.Settings.ValidateSignaturesBeforePreRequestHandler = true;
                server.Server.Settings.PreRequestHandler = async (ctx) =>
                {
                    Interlocked.Increment(ref preHandlerCalls);
                    return false;
                };

                GetObjectResponse response = await server.S3Client.GetObjectAsync(server.Bucket, "test-object.txt", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "valid GetObject");
                AssertHelper.AreEqual(1, preHandlerCalls, "PreRequestHandler invoked once");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("AuthenticatedRequestHandler is never invoked for forged unsigned or unknown-key requests", async (ct) =>
            {
                int calls = 0;
                server.Server.Settings.AuthenticatedRequestHandler = async (ctx) =>
                {
                    Interlocked.Increment(ref calls);
                    return false;
                };

                HttpResponseMessage forged = await server.HttpClient.SendAsync(ForgedRequest(server, "test-object.txt"), ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, forged, "forged");

                HttpResponseMessage unsigned = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/test-object.txt", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, unsigned, "unsigned");

                using (IAmazonS3 unknown = CreateClient(server, "AKIAUNKNOWNKEYEXAMPLE", "SomeRandomSecretKeyForTestPurposes123"))
                {
                    AmazonS3Exception s3e = await CaptureAmazonS3Exception(async () =>
                    {
                        await unknown.ListBucketsAsync(new ListBucketsRequest(), ct).ConfigureAwait(false);
                    }).ConfigureAwait(false);
                    AssertHelper.AreEqual(HttpStatusCode.Forbidden, s3e.StatusCode, "unknown key");
                }

                AssertHelper.AreEqual(0, calls, "AuthenticatedRequestHandler not invoked for rejected requests");

                GetObjectResponse valid = await server.S3Client.GetObjectAsync(server.Bucket, "test-object.txt", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)valid.HttpStatusCode, "valid request");
                AssertHelper.AreEqual(1, calls, "AuthenticatedRequestHandler invoked exactly once for a valid request");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("AuthenticatedRequestHandler returning true ends the request with its response", async (ct) =>
            {
                int readsBefore = server.ObjectReadCount;
                server.Server.Settings.AuthenticatedRequestHandler = async (ctx) =>
                {
                    if (ctx.Request.Key != "auth-handled.txt") return false;

                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentType = "text/plain";
                    await ctx.Response.Send("authenticated-handler").ConfigureAwait(false);
                    return true;
                };

                GetObjectResponse response = await server.S3Client.GetObjectAsync(server.Bucket, "auth-handled.txt", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)response.HttpStatusCode, "handled request");

                using (StreamReader reader = new StreamReader(response.ResponseStream))
                {
                    AssertHelper.AreEqual("authenticated-handler", await reader.ReadToEndAsync().ConfigureAwait(false), "handler body");
                }

                AssertHelper.AreEqual(readsBefore, server.ObjectReadCount, "Object.Read not invoked");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Anonymous request permitted by policy reaches AuthenticatedRequestHandler", async (ct) =>
            {
                int calls = 0;
                server.Server.Service.IsAnonymousRequestAllowed = async (ctx) => ctx.Request.Key == "public.txt";
                server.Server.Settings.AuthenticatedRequestHandler = async (ctx) =>
                {
                    Interlocked.Increment(ref calls);
                    return false;
                };

                HttpResponseMessage allowed = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/public.txt", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.OK, allowed, "anonymous allowed");
                AssertHelper.AreEqual(1, calls, "handler invoked for permitted anonymous request");

                HttpResponseMessage denied = await server.HttpClient.GetAsync(server.BaseUrl + "/" + server.Bucket + "/private.txt", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(HttpStatusCode.Forbidden, denied, "anonymous denied");
                AssertHelper.AreEqual(1, calls, "handler not invoked for denied anonymous request");
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Signature enforcement fails closed when GetSecretKey is cleared after Start", async (ct) =>
            {
                int readsBefore = server.ObjectReadCount;
                server.Server.Service.GetSecretKey = null;

                AmazonS3Exception s3e = await CaptureAmazonS3Exception(async () =>
                {
                    await server.S3Client.GetObjectAsync(server.Bucket, "test-object.txt", ct).ConfigureAwait(false);
                }).ConfigureAwait(false);

                AssertHelper.AreEqual(HttpStatusCode.Forbidden, s3e.StatusCode, "status");
                AssertHelper.AreEqual("AccessDenied", s3e.ErrorCode, "error code");
                AssertHelper.AreEqual(readsBefore, server.ObjectReadCount, "Object.Read not invoked");

                server.Server.Service.GetSecretKey = (ctx) => ctx.Request.AccessKey == server.AccessKey ? server.SecretKey : null;
                GetObjectResponse restored = await server.S3Client.GetObjectAsync(server.Bucket, "test-object.txt", ct).ConfigureAwait(false);
                AssertHelper.StatusCodeEquals(200, (int)restored.HttpStatusCode, "restored GetSecretKey allows request");
            }, token).ConfigureAwait(false);
        }

        private static HttpRequestMessage ForgedRequest(S3TestServer server, string key)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "/" + server.Bucket + "/" + key);
            request.Headers.TryAddWithoutValidation("Authorization", "AWS4-HMAC-SHA256 Credential=" + server.AccessKey + "/20260101/us-west-1/s3/aws4_request, SignedHeaders=host;x-amz-date, Signature=forged-signature");
            request.Headers.TryAddWithoutValidation("x-amz-date", "20260101T000000Z");
            return request;
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

        private static async Task<AmazonS3Exception> CaptureAmazonS3Exception(Func<Task> action)
        {
            try
            {
                await action().ConfigureAwait(false);
            }
            catch (AmazonS3Exception s3e)
            {
                return s3e;
            }

            throw new InvalidOperationException("Expected AmazonS3Exception.");
        }
    }
}
