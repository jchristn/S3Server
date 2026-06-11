namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Orchestrates shared test execution and records results.
    /// </summary>
    public class TestRunner
    {
        private readonly List<TestResult> _Results = new List<TestResult>();
        private readonly List<string> _DiscoveredTests = new List<string>();

        /// <summary>
        /// List of test results.
        /// </summary>
        public IReadOnlyList<TestResult> Results => _Results;

        /// <summary>
        /// List of tests discovered while walking legacy runner-style suites.
        /// </summary>
        public IReadOnlyList<string> DiscoveredTests => _DiscoveredTests;

        /// <summary>
        /// True to collect test names without executing the test bodies.
        /// </summary>
        public bool DiscoveryOnly { get; set; } = false;

        /// <summary>
        /// Optional target test name.  When set, only this test body is executed.
        /// </summary>
        public string TargetTestName { get; set; } = null;

        /// <summary>
        /// True when the target test name was encountered.
        /// </summary>
        public bool TargetWasSeen { get; private set; } = false;

        /// <summary>
        /// True when the target test body was executed.
        /// </summary>
        public bool TargetWasExecuted { get; private set; } = false;

        /// <summary>
        /// Per-test timeout in milliseconds.
        /// Default is 10000 (10 seconds).
        /// </summary>
        public int TestTimeoutMs { get; set; } = 5000;

        /// <summary>
        /// Create a discovery-only runner.
        /// </summary>
        /// <returns>Discovery runner.</returns>
        public static TestRunner CreateDiscoveryRunner()
        {
            return new TestRunner
            {
                DiscoveryOnly = true
            };
        }

        /// <summary>
        /// Create a runner that executes only one named test.
        /// </summary>
        /// <param name="testName">Test name to execute.</param>
        /// <returns>Targeted runner.</returns>
        public static TestRunner CreateTargetedRunner(string testName)
        {
            if (String.IsNullOrEmpty(testName)) throw new ArgumentNullException(nameof(testName));

            return new TestRunner
            {
                TargetTestName = testName
            };
        }

        /// <summary>
        /// Run a single test and record the result.
        /// </summary>
        /// <param name="testName">Name of the test.</param>
        /// <param name="testAction">Async action to execute. Throwing any exception signals failure.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>TestResult.</returns>
        public async Task<TestResult> RunTestAsync(string testName, Func<CancellationToken, Task> testAction, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(testName)) throw new ArgumentNullException(nameof(testName));
            if (testAction == null) throw new ArgumentNullException(nameof(testAction));

            _DiscoveredTests.Add(testName);

            if (DiscoveryOnly)
                return new TestResult(testName, true, 0);

            if (!String.IsNullOrEmpty(TargetTestName)
                && !String.Equals(TargetTestName, testName, StringComparison.Ordinal))
            {
                return new TestResult(testName, true, 0);
            }

            if (!String.IsNullOrEmpty(TargetTestName))
                TargetWasSeen = true;

            DateTime start = DateTime.UtcNow;
            TestResult result;

            try
            {
                using (CancellationTokenSource testCts = CancellationTokenSource.CreateLinkedTokenSource(token))
                {
                    testCts.CancelAfter(TestTimeoutMs);
                    await testAction(testCts.Token).ConfigureAwait(false);
                }
                result = new TestResult(testName, true, (DateTime.UtcNow - start).TotalMilliseconds);
                TargetWasExecuted = true;
            }
            catch (Exception ex)
            {
                result = new TestResult(testName, false, (DateTime.UtcNow - start).TotalMilliseconds, ex.Message);
                TargetWasExecuted = true;
            }

            _Results.Add(result);
            return result;
        }
    }
}
