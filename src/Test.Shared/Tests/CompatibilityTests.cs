namespace Test.Shared.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Test.Shared.Compatibility;

    /// <summary>
    /// Runs the Amazon S3 compatibility scenarios against S3Server with the reference backend and signature validation enabled.
    /// The expectations were recorded from Amazon S3; Test.Compatibility --target s3 re-checks them against Amazon S3.
    /// </summary>
    public static class CompatibilityTests
    {
        /// <summary>
        /// Run all compatibility scenarios.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public static async Task RunAllAsync(TestRunner runner, CancellationToken token = default)
        {
            if (runner.DiscoveryOnly)
            {
                using (CompatTarget discovery = new CompatTarget("http://127.0.0.1:1", "compat-bucket", "us-west-1", "a", "b", false, "s3server-compat/"))
                {
                    await CompatibilityScenarios.RunAllAsync(runner, discovery, token).ConfigureAwait(false);
                }

                return;
            }

            using (ReferenceS3Server server = new ReferenceS3Server())
            using (CompatTarget target = new CompatTarget(server.Endpoint, server.Bucket, "us-west-1", server.AccessKey, server.SecretKey, false, "s3server-compat/"))
            {
                await CompatibilityScenarios.RunAllAsync(runner, target, token).ConfigureAwait(false);
            }
        }
    }
}
