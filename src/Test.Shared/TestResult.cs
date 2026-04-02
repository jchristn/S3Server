namespace Test.Shared
{
    using System;

    /// <summary>
    /// Represents the result of a single test execution.
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Name of the test.
        /// </summary>
        public string TestName { get; set; }

        /// <summary>
        /// Whether the test passed.
        /// </summary>
        public bool Passed { get; set; }

        /// <summary>
        /// Execution time in milliseconds.
        /// </summary>
        public double RuntimeMs { get; set; }

        /// <summary>
        /// Error message if the test failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="testName">Name of the test.</param>
        /// <param name="passed">Whether the test passed.</param>
        /// <param name="runtimeMs">Execution time in milliseconds.</param>
        /// <param name="errorMessage">Error message if the test failed.</param>
        public TestResult(string testName, bool passed, double runtimeMs, string errorMessage = null)
        {
            TestName = testName;
            Passed = passed;
            RuntimeMs = runtimeMs;
            ErrorMessage = errorMessage;
        }
    }
}
