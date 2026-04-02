namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Orchestrates test execution with colored console output and summary reporting.
    /// </summary>
    public class TestRunner
    {
        private readonly List<TestResult> _Results = new List<TestResult>();

        /// <summary>
        /// List of test results.
        /// </summary>
        public IReadOnlyList<TestResult> Results => _Results;

        /// <summary>
        /// Per-test timeout in milliseconds.
        /// Default is 10000 (10 seconds).
        /// </summary>
        public int TestTimeoutMs { get; set; } = 5000;

        /// <summary>
        /// Run a single test and record the result.
        /// </summary>
        /// <param name="testName">Name of the test.</param>
        /// <param name="testAction">Async action to execute. Throwing any exception signals failure.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>TestResult.</returns>
        public async Task<TestResult> RunTestAsync(string testName, Func<CancellationToken, Task> testAction, CancellationToken token = default)
        {
            Stopwatch sw = Stopwatch.StartNew();
            TestResult result;

            try
            {
                using (CancellationTokenSource testCts = CancellationTokenSource.CreateLinkedTokenSource(token))
                {
                    testCts.CancelAfter(TestTimeoutMs);
                    await testAction(testCts.Token).ConfigureAwait(false);
                }
                sw.Stop();
                result = new TestResult(testName, true, sw.Elapsed.TotalMilliseconds);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("  PASS ");
            }
            catch (Exception ex)
            {
                sw.Stop();
                result = new TestResult(testName, false, sw.Elapsed.TotalMilliseconds, ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("  FAIL ");
            }

            Console.ResetColor();
            Console.WriteLine($" {testName} ({result.RuntimeMs:F1}ms)");

            if (!result.Passed)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"         {result.ErrorMessage}");
                Console.ResetColor();
            }

            _Results.Add(result);
            return result;
        }

        /// <summary>
        /// Print a summary of all test results to the console.
        /// </summary>
        /// <param name="totalRuntimeMs">Total elapsed time in milliseconds.</param>
        public void PrintSummary(double totalRuntimeMs)
        {
            int passed = 0;
            int failed = 0;
            List<TestResult> failures = new List<TestResult>();

            foreach (TestResult r in _Results)
            {
                if (r.Passed) passed++;
                else
                {
                    failed++;
                    failures.Add(r);
                }
            }

            Console.WriteLine();
            Console.WriteLine(new string('=', 80));
            Console.WriteLine("TEST SUMMARY");
            Console.WriteLine(new string('=', 80));
            Console.WriteLine($"  Total:   {_Results.Count}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  Passed:  {passed}");
            Console.ForegroundColor = failed > 0 ? ConsoleColor.Red : ConsoleColor.Green;
            Console.WriteLine($"  Failed:  {failed}");
            Console.ResetColor();
            Console.WriteLine($"  Runtime: {totalRuntimeMs:F1}ms");
            Console.WriteLine();

            if (failures.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FAILED TESTS:");
                Console.ResetColor();

                foreach (TestResult f in failures)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("  FAIL ");
                    Console.ResetColor();
                    Console.WriteLine($" {f.TestName}");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"         {f.ErrorMessage}");
                    Console.ResetColor();
                }

                Console.WriteLine();
            }

            if (failed > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("OVERALL: FAIL");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("OVERALL: PASS");
            }

            Console.ResetColor();
        }
    }
}
