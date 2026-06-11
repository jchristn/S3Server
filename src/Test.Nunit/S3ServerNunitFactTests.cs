namespace Test.Nunit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Test.Shared;
    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    /// <summary>
    /// NUnit fact-style host for shared S3Server tests.
    /// </summary>
    [TestFixture]
    [NonParallelizable]
    public sealed class S3ServerNunitFactTests : TouchstoneNunitBase
    {
        /// <inheritdoc />
        protected override IReadOnlyList<TestSuiteDescriptor> Suites
        {
            get { return S3ServerSuites.All; }
        }

        /// <summary>
        /// Run all shared descriptors as one NUnit test.
        /// </summary>
        [Test]
        public async Task RunAll()
        {
            await RunAllAsync().ConfigureAwait(false);
        }
    }
}
