namespace Test.Automated
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Test.Shared;
    using Test.Shared.Tests;

    /// <summary>
    /// Automated test runner for S3Server.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Exit code (0 for success, 1 for failures).</returns>
        public static async Task<int> Main(string[] args)
        {
            Console.WriteLine(new string('=', 80));
            Console.WriteLine("  S3Server Automated Test Suite");
            Console.WriteLine(new string('=', 80));

            TestRunner runner = new TestRunner();
            Stopwatch totalStopwatch = Stopwatch.StartNew();
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

            try
            {
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("  Server Tests");
                Console.WriteLine("========================================");

                using (S3TestServer server = new S3TestServer(enableSignatures: false))
                {
                    await ServiceTests.RunAllAsync(runner, server, cts.Token).ConfigureAwait(false);
                    await BucketTests.RunAllAsync(runner, server, cts.Token).ConfigureAwait(false);
                    await ObjectTests.RunAllAsync(runner, server, cts.Token).ConfigureAwait(false);
                    await MultipartUploadTests.RunAllAsync(runner, server, cts.Token).ConfigureAwait(false);
                    await ErrorHandlingTests.RunAllAsync(runner, server, cts.Token).ConfigureAwait(false);
                    await S3ComplianceTests.RunAllAsync(runner, server, cts.Token).ConfigureAwait(false);
                }

                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("  Signature Validation Tests");
                Console.WriteLine("========================================");

                using (S3TestServer sigServer = new S3TestServer(enableSignatures: true))
                {
                    await SignatureValidationTests.RunAllAsync(runner, sigServer, cts.Token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FATAL ERROR:");
                Console.ResetColor();
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                cts.Dispose();
            }

            totalStopwatch.Stop();
            runner.PrintSummary(totalStopwatch.Elapsed.TotalMilliseconds);

            foreach (TestResult r in runner.Results)
            {
                if (!r.Passed) return 1;
            }

            return 0;
        }
    }
}
