namespace Test.Xunit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Test.Shared;
    using Touchstone.Core;

    /// <summary>
    /// xUnit theory-style host for shared S3Server tests.
    /// </summary>
    public sealed class S3ServerTheoryTests
    {
        /// <summary>
        /// Provides non-skipped test cases as theory data.
        /// </summary>
        /// <returns>Theory data rows.</returns>
        public static TheoryData<TestCaseDescriptor> TestCases()
        {
            TheoryData<TestCaseDescriptor> data = new TheoryData<TestCaseDescriptor>();

            foreach (TestSuiteDescriptor suite in S3ServerSuites.All)
            {
                foreach (TestCaseDescriptor testCase in suite.Cases)
                {
                    if (!testCase.Skip)
                        data.Add(testCase);
                }
            }

            return data;
        }

        /// <summary>
        /// Execute a single shared test case descriptor.
        /// </summary>
        /// <param name="testCase">Test case to run.</param>
        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task RunTest(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
