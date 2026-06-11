namespace Test.Xunit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Test.Shared;
    using Touchstone.Core;
    using Touchstone.XunitAdapter;

    /// <summary>
    /// xUnit fact-style host for shared S3Server tests.
    /// </summary>
    public sealed class S3ServerFactTests : TouchstoneFactBase
    {
        /// <inheritdoc />
        protected override IReadOnlyList<TestSuiteDescriptor> Suites
        {
            get { return S3ServerSuites.All; }
        }

        /// <summary>
        /// Run all shared descriptors as one xUnit fact.
        /// </summary>
        [Fact]
        public async Task RunAll()
        {
            await RunAllAsync();
        }
    }
}
