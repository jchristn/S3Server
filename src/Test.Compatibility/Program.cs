namespace Test.Compatibility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Test.Shared;
    using Test.Shared.Compatibility;

    /// <summary>
    /// Runs the Amazon S3 compatibility scenarios against Amazon S3 or S3Server, or hosts the reference server for
    /// external clients such as the AWS CLI, boto3, and the MinIO client.
    ///
    /// Usage:
    ///   Test.Compatibility --target local
    ///   Test.Compatibility --target s3 --bucket my-bucket [--region us-west-1] [--endpoint https://s3.us-west-1.amazonaws.com]
    ///   Test.Compatibility --serve 8000 [--bucket compat-bucket] [--no-signatures]
    ///
    /// For --target s3, credentials are read from S3COMPAT_ACCESS_KEY and S3COMPAT_SECRET_KEY (never from arguments).
    /// Scenarios only touch objects under a unique s3server-compat-* prefix and delete only that prefix afterward;
    /// bucket settings and existing objects are never modified.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>Exit code: 0 when every scenario passed.</returns>
        public static async Task<int> Main(string[] args)
        {
            Dictionary<string, string> options = ParseArgs(args);

            if (options.TryGetValue("serve", out string servePort))
                return Serve(Int32.Parse(servePort), Get(options, "bucket", "compat-bucket"), !options.ContainsKey("no-signatures"));

            string targetName = Get(options, "target", "local");
            if (targetName == "s3") return await RunAmazon(options).ConfigureAwait(false);
            if (targetName == "local") return await RunLocal().ConfigureAwait(false);

            Console.WriteLine("Unknown target '" + targetName + "'.  Use --target local or --target s3.");
            return 2;
        }

        private static async Task<int> RunLocal()
        {
            using (ReferenceS3Server server = new ReferenceS3Server())
            using (CompatTarget target = new CompatTarget(server.Endpoint, server.Bucket, "us-west-1", server.AccessKey, server.SecretKey, false))
            {
                Console.WriteLine("Target: S3Server reference backend at " + server.Endpoint);
                return await Run(target).ConfigureAwait(false);
            }
        }

        private static async Task<int> RunAmazon(Dictionary<string, string> options)
        {
            string accessKey = Environment.GetEnvironmentVariable("S3COMPAT_ACCESS_KEY");
            string secretKey = Environment.GetEnvironmentVariable("S3COMPAT_SECRET_KEY");
            if (String.IsNullOrEmpty(accessKey) || String.IsNullOrEmpty(secretKey))
            {
                Console.WriteLine("Set S3COMPAT_ACCESS_KEY and S3COMPAT_SECRET_KEY to run against Amazon S3.");
                return 2;
            }

            if (!options.TryGetValue("bucket", out string bucket))
            {
                Console.WriteLine("--bucket is required for --target s3.");
                return 2;
            }

            string region = Get(options, "region", "us-west-1");
            string endpoint = Get(options, "endpoint", "https://s3." + region + ".amazonaws.com");

            using (CompatTarget target = new CompatTarget(endpoint, bucket, region, accessKey, secretKey, true))
            {
                Console.WriteLine("Target: Amazon S3 " + endpoint + "/" + bucket + " (prefix " + target.Prefix + ")");
                return await Run(target).ConfigureAwait(false);
            }
        }

        private static async Task<int> Run(CompatTarget target)
        {
            TestRunner runner = new TestRunner();
            runner.TestTimeoutMs = 60000;

            await CompatibilityScenarios.RunAllAsync(runner, target, CancellationToken.None).ConfigureAwait(false);

            foreach (TestResult result in runner.Results)
            {
                Console.WriteLine((result.Passed ? "PASS  " : "FAIL  ") + result.TestName);
                if (!result.Passed) Console.WriteLine("      " + result.ErrorMessage);
            }

            int failed = runner.Results.Count(r => !r.Passed);
            Console.WriteLine();
            Console.WriteLine("Total: " + runner.Results.Count + "  Passed: " + (runner.Results.Count - failed) + "  Failed: " + failed);
            return failed == 0 ? 0 : 1;
        }

        private static int Serve(int port, string bucket, bool signatures)
        {
            using (ReferenceS3Server server = new ReferenceS3Server(port, bucket, signatures))
            {
                Console.WriteLine("Reference S3Server listening on " + server.Endpoint);
                Console.WriteLine("Bucket: " + server.Bucket);
                if (signatures) Console.WriteLine("Access key: " + server.AccessKey + "  Secret key: " + server.SecretKey);
                else Console.WriteLine("Signature validation disabled; any credentials are accepted.");
                Console.WriteLine("Press ENTER to stop.");
                Console.ReadLine();
            }

            return 0;
        }

        private static Dictionary<string, string> ParseArgs(string[] args)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith("--", StringComparison.Ordinal)) continue;
                string name = args[i].Substring(2);
                string value = (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal)) ? args[++i] : "";
                ret[name] = value;
            }

            return ret;
        }

        private static string Get(Dictionary<string, string> options, string name, string defaultValue)
        {
            return options.TryGetValue(name, out string value) && !String.IsNullOrEmpty(value) ? value : defaultValue;
        }
    }
}
