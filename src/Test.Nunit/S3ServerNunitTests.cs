namespace Test.Nunit
{
    using System.Collections;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Test.Shared;
    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    /// <summary>
    /// NUnit data-driven host for shared S3Server tests.
    /// </summary>
    [TestFixture]
    [NonParallelizable]
    public sealed class S3ServerNunitTests
    {
        private static IEnumerable TestCases()
        {
            return new TouchstoneTestCaseSource(S3ServerSuites.All);
        }

        /// <summary>
        /// Execute a single shared test case descriptor.
        /// </summary>
        /// <param name="testCase">Test case to run.</param>
        [Test]
        [TestCaseSource(nameof(TestCases))]
        public async Task RunTest(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }
}
