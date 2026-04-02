namespace Test.Xunit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Test.Shared;
    using Test.Shared.Tests;

    /// <summary>
    /// xUnit tests for S3Server.
    /// Each test method delegates to the shared test suites in Test.Shared.
    /// </summary>
    [Collection("S3Server")]
    public class S3ServerTests : IDisposable
    {
        private readonly S3TestServer _Server;
        private readonly S3TestServer _SigServer;

        /// <summary>
        /// Instantiate.
        /// </summary>
        public S3ServerTests()
        {
            _Server = new S3TestServer(port: 8011, enableSignatures: false);
            _SigServer = new S3TestServer(port: 8012, enableSignatures: true);
        }

        /// <summary>
        /// Dispose of resources.
        /// </summary>
        public void Dispose()
        {
            _Server?.Dispose();
            _SigServer?.Dispose();
        }

        /// <summary>
        /// Run service API tests.
        /// </summary>
        [Fact]
        public async Task ServiceApiTests()
        {
            TestRunner runner = new TestRunner();
            await ServiceTests.RunAllAsync(runner, _Server);
            AssertAllPassed(runner);
        }

        /// <summary>
        /// Run bucket API tests.
        /// </summary>
        [Fact]
        public async Task BucketApiTests()
        {
            TestRunner runner = new TestRunner();
            await BucketTests.RunAllAsync(runner, _Server);
            AssertAllPassed(runner);
        }

        /// <summary>
        /// Run object API tests.
        /// </summary>
        [Fact]
        public async Task ObjectApiTests()
        {
            TestRunner runner = new TestRunner();
            await ObjectTests.RunAllAsync(runner, _Server);
            AssertAllPassed(runner);
        }

        /// <summary>
        /// Run multipart upload tests.
        /// </summary>
        [Fact]
        public async Task MultipartUploadApiTests()
        {
            TestRunner runner = new TestRunner();
            await MultipartUploadTests.RunAllAsync(runner, _Server);
            AssertAllPassed(runner);
        }

        /// <summary>
        /// Run error handling tests.
        /// </summary>
        [Fact]
        public async Task ErrorHandlingApiTests()
        {
            TestRunner runner = new TestRunner();
            await ErrorHandlingTests.RunAllAsync(runner, _Server);
            AssertAllPassed(runner);
        }

        /// <summary>
        /// Run S3 compliance tests.
        /// </summary>
        [Fact]
        public async Task S3ComplianceApiTests()
        {
            TestRunner runner = new TestRunner();
            await S3ComplianceTests.RunAllAsync(runner, _Server);
            AssertAllPassed(runner);
        }

        /// <summary>
        /// Run signature validation tests.
        /// </summary>
        [Fact]
        public async Task SignatureValidationApiTests()
        {
            TestRunner runner = new TestRunner();
            await SignatureValidationTests.RunAllAsync(runner, _SigServer);
            AssertAllPassed(runner);
        }

        private void AssertAllPassed(TestRunner runner)
        {
            foreach (TestResult r in runner.Results)
            {
                Assert.True(r.Passed, $"{r.TestName}: {r.ErrorMessage}");
            }
        }
    }
}
