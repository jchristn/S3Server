namespace Test.Shared.Tests
{
    using System.Collections.Specialized;
    using System.Threading;
    using System.Threading.Tasks;
    using AWSSignatureGenerator;

    /// <summary>
    /// Canonical signature fixtures from the legacy Amazon S3 Signature V2 documentation.
    /// </summary>
    public static class CanonicalSignatureFixtureTests
    {
        /// <summary>
        /// Run all canonical signature fixture tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="server">Unused shared server instance.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, S3TestServer server, CancellationToken token = default)
        {
            await runner.RunTestAsync("AWS V2 fixture example 1 matches header signature", async (ct) =>
            {
                NameValueCollection headers = new NameValueCollection();
                headers.Add("Content-Md5", "c8fdb181845a4ca6b8fec737b3581d76");
                headers.Add("Content-Type", "text/html");
                headers.Add("Date", "Thu, 17 Nov 2005 18:49:58 GMT");
                headers.Add("X-Amz-Meta-Author", "foo@bar.com");
                headers.Add("X-Amz-Magic", "abracadabra");

                using (V2SignatureResult result = new V2SignatureResult(
                    "PUT",
                    "http://s3.amazonaws.com/quotes/nelson",
                    "44CF9590006BF252F707",
                    "OtxrzxIsfpFjA7SwPzILwy8Bw21TLhquhboDYROV",
                    headers,
                    null))
                {
                    AssertHelper.AreEqual("jZNOcbfWmD/A/f3hSvVzXZjM2HU=", result.Signature, "AWS fixture example 1 signature");
                    AssertHelper.StringContains(result.StringToSign, "/quotes/nelson", "AWS fixture example 1 string to sign");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("AWS V2 fixture example 2 matches x-amz-date signature", async (ct) =>
            {
                NameValueCollection headers = new NameValueCollection();
                headers.Add("Date", "XXXXXXXXX");
                headers.Add("X-Amz-Magic", "abracadabra");
                headers.Add("X-Amz-Date", "Thu, 17 Nov 2005 18:49:58 GMT");

                using (V2SignatureResult result = new V2SignatureResult(
                    "GET",
                    "http://s3.amazonaws.com/quotes/nelson",
                    "44CF9590006BF252F707",
                    "OtxrzxIsfpFjA7SwPzILwy8Bw21TLhquhboDYROV",
                    headers,
                    null))
                {
                    AssertHelper.AreEqual("5m+HAmc5JsrgyDelh9+a2dNrzN8=", result.Signature, "AWS fixture example 2 signature");
                    AssertHelper.AreEqual(string.Empty, result.DateElement, "AWS fixture example 2 date element");
                }
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("AWS V2 fixture example 3 matches signed URL signature", async (ct) =>
            {
                using (V2SignedUrlResult result = new V2SignedUrlResult(
                    "GET",
                    "http://s3.amazonaws.com/quotes/nelson",
                    "44CF9590006BF252F707",
                    "OtxrzxIsfpFjA7SwPzILwy8Bw21TLhquhboDYROV",
                    1141889120,
                    null,
                    null))
                {
                    AssertHelper.AreEqual("vjbyPxybdZaNmGa+yT272YEAiv4=", result.Signature, "AWS fixture example 3 signature");
                    AssertHelper.AreEqual("vjbyPxybdZaNmGa%2ByT272YEAiv4%3D", result.EncodedSignature, "AWS fixture example 3 encoded signature");
                    AssertHelper.StringContains(result.SignedUrl, "AWSAccessKeyId=44CF9590006BF252F707", "AWS fixture example 3 signed URL access key");
                }
            }, token).ConfigureAwait(false);
        }
    }
}
