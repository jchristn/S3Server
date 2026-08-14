namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using S3ServerLibrary;
    using Test.Shared.Tests;
    using Touchstone.Core;

    /// <summary>
    /// Shared Touchstone suite descriptors for S3Server.
    /// </summary>
    public static class S3ServerSuites
    {
        /// <summary>
        /// All shared test suites.
        /// </summary>
        public static IReadOnlyList<TestSuiteDescriptor> All
        {
            get
            {
                return new List<TestSuiteDescriptor>
                {
                    ServerApiSuite(),
                    SignatureValidationSuite(),
                    SerializationSuite(),
                    UtilitySuite()
                };
            }
        }

        /// <summary>
        /// Server API test suite.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor ServerApiSuite()
        {
            const string suiteId = "ServerApi";

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "S3 Server API",
                cases: BuildServerCases(
                    suiteId,
                    new RunnerSuiteSource("Service", "Service", ServiceTests.RunAllAsync),
                    new RunnerSuiteSource("Bucket", "Bucket", BucketTests.RunAllAsync),
                    new RunnerSuiteSource("Object", "Object", ObjectTests.RunAllAsync),
                    new RunnerSuiteSource("MultipartUpload", "Multipart upload", MultipartUploadTests.RunAllAsync),
                    new RunnerSuiteSource("ErrorHandling", "Error handling", ErrorHandlingTests.RunAllAsync),
                    new RunnerSuiteSource("S3Compliance", "S3 compliance", S3ComplianceTests.RunAllAsync),
                    new RunnerSuiteSource("ParserRouting", "Parser and routing", ParserRoutingTests.RunAllAsync),
                    new RunnerSuiteSource("ProtocolCompatibility", "Protocol compatibility", ProtocolCompatibilityTests.RunAllAsync),
                    new RunnerSuiteSource("RequestStyle", "Request style compatibility", RequestStyleCompatibilityTests.RunAllAsync),
                    new RunnerSuiteSource("AdversarialHttp", "Adversarial HTTP", AdversarialHttpTests.RunAllAsync),
                    new RunnerSuiteSource("Fuzz", "Fuzz and generated inputs", FuzzTests.RunAllAsync),
                    new RunnerSuiteSource("Lifecycle", "Lifecycle and concurrency", LifecycleTests.RunAllAsync)));
        }

        /// <summary>
        /// Signature validation test suite.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor SignatureValidationSuite()
        {
            const string suiteId = "SignatureValidation";

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Signature Validation",
                cases: BuildServerCases(
                    suiteId,
                    new RunnerSuiteSource("CanonicalSignatureFixtures", "Canonical signature fixtures", CanonicalSignatureFixtureTests.RunAllAsync),
                    new RunnerSuiteSource("SignatureV4", "Signature V4", SignatureValidationTests.RunAllAsync, true),
                    new RunnerSuiteSource("SignatureV2", "Signature V2", SignatureV2ValidationTests.RunAllAsync, true, true)));
        }

        /// <summary>
        /// Serialization test suite.  These cases exercise the S3Objects data models and the
        /// SerializationHelper without requiring a running server.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor SerializationSuite()
        {
            const string suiteId = "Serialization";

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Serialization",
                cases: BuildRunnerCases(
                    suiteId,
                    new RunnerOnlySource("Serialization", "Serialization", SerializationTests.RunAllAsync),
                    new RunnerOnlySource("DataModel", "Data model", DataModelTests.RunAllAsync)));
        }

        /// <summary>
        /// Utility and parsing test suite.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor UtilitySuite()
        {
            const string suiteId = "Utility";

            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Utility Behavior",
                cases: new List<TestCaseDescriptor>
                {
                    UtilityCase(suiteId, "ReplaceWildcardAsterisk", "Asterisk wildcard hostname is replaced with Host", ct =>
                    {
                        AssertHelper.AreEqual(
                            "http://localhost:8000/bucket/key",
                            S3Request.ReplaceWildcardHostname("http://*:8000/bucket/key", "localhost"),
                            "wildcard replacement");
                        return Task.CompletedTask;
                    }),
                    UtilityCase(suiteId, "ReplaceWildcardPlus", "Plus wildcard hostname is replaced with Host", ct =>
                    {
                        AssertHelper.AreEqual(
                            "http://myhost.local:8000/bucket/key",
                            S3Request.ReplaceWildcardHostname("http://+:8000/bucket/key", "myhost.local"),
                            "wildcard replacement");
                        return Task.CompletedTask;
                    }),
                    UtilityCase(suiteId, "ReplaceWildcardQuadZero", "0.0.0.0 wildcard hostname is replaced with Host", ct =>
                    {
                        AssertHelper.AreEqual(
                            "http://192.168.1.100:8000/bucket/key",
                            S3Request.ReplaceWildcardHostname("http://0.0.0.0:8000/bucket/key", "192.168.1.100"),
                            "wildcard replacement");
                        return Task.CompletedTask;
                    }),
                    UtilityCase(suiteId, "ReplaceWildcardNormalHost", "Non-wildcard hostname is unchanged", ct =>
                    {
                        AssertHelper.AreEqual(
                            "http://localhost:8000/bucket/key",
                            S3Request.ReplaceWildcardHostname("http://localhost:8000/bucket/key", "otherhost"),
                            "wildcard replacement");
                        return Task.CompletedTask;
                    }),
                    UtilityCase(suiteId, "ReplaceWildcardHttps", "HTTPS wildcard hostname is replaced", ct =>
                    {
                        AssertHelper.AreEqual(
                            "https://secure.example.com:443/bucket/key",
                            S3Request.ReplaceWildcardHostname("https://*:443/bucket/key", "secure.example.com"),
                            "wildcard replacement");
                        return Task.CompletedTask;
                    }),
                    UtilityCase(suiteId, "ReplaceWildcardQuery", "Wildcard replacement preserves path and query", ct =>
                    {
                        AssertHelper.AreEqual(
                            "http://localhost:8000/bucket/key?versionId=1",
                            S3Request.ReplaceWildcardHostname("http://*:8000/bucket/key?versionId=1", "localhost"),
                            "wildcard replacement");
                        return Task.CompletedTask;
                    }),
                    UtilityCase(suiteId, "ReplaceWildcardRoot", "Wildcard replacement handles root URLs", ct =>
                    {
                        AssertHelper.AreEqual(
                            "http://localhost:8000/",
                            S3Request.ReplaceWildcardHostname("http://*:8000/", "localhost"),
                            "wildcard replacement");
                        return Task.CompletedTask;
                    }),
                    UtilityCase(suiteId, "ReplaceWildcardNoPort", "Wildcard replacement handles URLs without ports", ct =>
                    {
                        AssertHelper.AreEqual(
                            "http://localhost/bucket/key",
                            S3Request.ReplaceWildcardHostname("http://*/bucket/key", "localhost"),
                            "wildcard replacement");
                        return Task.CompletedTask;
                    })
                });
        }

        private static IReadOnlyList<TestCaseDescriptor> BuildServerCases(
            string suiteId,
            params RunnerSuiteSource[] sources)
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            foreach (RunnerSuiteSource source in sources)
            {
                foreach (string testName in DiscoverTestNames(source.ExecuteAsync, source.EnableSignatures, source.EnableSignatureV2))
                {
                    string caseId = source.CategoryId + "_" + NormalizeCaseId(testName);
                    string displayName = source.DisplayName + ": " + testName;

                    cases.Add(ServerCase(
                        suiteId,
                        caseId,
                        displayName,
                        source.ExecuteAsync,
                        source.EnableSignatures,
                        source.EnableSignatureV2,
                        testName));
                }
            }

            return cases;
        }

        private static IReadOnlyList<TestCaseDescriptor> BuildRunnerCases(
            string suiteId,
            params RunnerOnlySource[] sources)
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>();

            foreach (RunnerOnlySource source in sources)
            {
                foreach (string testName in DiscoverRunnerTestNames(source.ExecuteAsync))
                {
                    string caseId = source.CategoryId + "_" + NormalizeCaseId(testName);
                    string displayName = source.DisplayName + ": " + testName;

                    cases.Add(RunnerCase(
                        suiteId,
                        caseId,
                        displayName,
                        source.ExecuteAsync,
                        testName));
                }
            }

            return cases;
        }

        private static IReadOnlyList<string> DiscoverRunnerTestNames(
            Func<TestRunner, CancellationToken, Task> executeAsync)
        {
            TestRunner runner = TestRunner.CreateDiscoveryRunner();
            executeAsync(runner, CancellationToken.None).GetAwaiter().GetResult();

            List<string> names = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (string name in runner.DiscoveredTests)
            {
                if (seen.Add(name))
                    names.Add(name);
            }

            return names;
        }

        private static TestCaseDescriptor RunnerCase(
            string suiteId,
            string caseId,
            string displayName,
            Func<TestRunner, CancellationToken, Task> executeAsync,
            string targetTestName)
        {
            return new TestCaseDescriptor(
                suiteId: suiteId,
                caseId: caseId,
                displayName: displayName,
                executeAsync: async ct =>
                {
                    TestRunner runner = TestRunner.CreateTargetedRunner(targetTestName);
                    await executeAsync(runner, ct).ConfigureAwait(false);

                    if (!runner.TargetWasExecuted)
                        throw new InvalidOperationException("Target test was not executed: " + targetTestName);

                    AssertAllPassed(runner);
                });
        }

        private static IReadOnlyList<string> DiscoverTestNames(
            Func<TestRunner, S3TestServer, CancellationToken, Task> executeAsync,
            bool enableSignatures,
            bool enableSignatureV2)
        {
            using (S3TestServer server = new S3TestServer(enableSignatures: enableSignatures, startServer: false, enableSignatureV2: enableSignatureV2))
            {
                TestRunner runner = TestRunner.CreateDiscoveryRunner();
                executeAsync(runner, server, CancellationToken.None).GetAwaiter().GetResult();

                List<string> names = new List<string>();
                HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

                foreach (string name in runner.DiscoveredTests)
                {
                    if (seen.Add(name))
                        names.Add(name);
                }

                return names;
            }
        }

        private static TestCaseDescriptor ServerCase(
            string suiteId,
            string caseId,
            string displayName,
            Func<TestRunner, S3TestServer, CancellationToken, Task> executeAsync,
            bool enableSignatures,
            bool enableSignatureV2,
            string targetTestName)
        {
            return new TestCaseDescriptor(
                suiteId: suiteId,
                caseId: caseId,
                displayName: displayName,
                executeAsync: async ct =>
                {
                    using (S3TestServer server = new S3TestServer(enableSignatures: enableSignatures, enableSignatureV2: enableSignatureV2))
                    {
                        TestRunner runner = TestRunner.CreateTargetedRunner(targetTestName);
                        await executeAsync(runner, server, ct).ConfigureAwait(false);

                        if (!runner.TargetWasExecuted)
                            throw new InvalidOperationException("Target test was not executed: " + targetTestName);

                        AssertAllPassed(runner);
                    }
                });
        }

        private static TestCaseDescriptor UtilityCase(
            string suiteId,
            string caseId,
            string displayName,
            Func<CancellationToken, Task> executeAsync)
        {
            return new TestCaseDescriptor(
                suiteId: suiteId,
                caseId: caseId,
                displayName: displayName,
                executeAsync: executeAsync);
        }

        private static void AssertAllPassed(TestRunner runner)
        {
            StringBuilder failures = new StringBuilder();

            foreach (TestResult result in runner.Results)
            {
                if (result.Passed) continue;
                failures.AppendLine(result.TestName + ": " + result.ErrorMessage);
            }

            if (failures.Length > 0)
                throw new InvalidOperationException(failures.ToString());
        }

        private static string NormalizeCaseId(string value)
        {
            if (String.IsNullOrEmpty(value)) return "Unnamed";

            StringBuilder builder = new StringBuilder();
            bool capitalizeNext = true;

            foreach (char c in value)
            {
                if (Char.IsLetterOrDigit(c))
                {
                    if (builder.Length == 0 && Char.IsDigit(c))
                        builder.Append("Case");

                    builder.Append(capitalizeNext ? Char.ToUpperInvariant(c) : c);
                    capitalizeNext = false;
                }
                else
                {
                    capitalizeNext = true;
                }
            }

            if (builder.Length == 0)
                return "Unnamed";

            return builder.ToString();
        }

        private sealed class RunnerSuiteSource
        {
            public string CategoryId { get; private set; }
            public string DisplayName { get; private set; }
            public Func<TestRunner, S3TestServer, CancellationToken, Task> ExecuteAsync { get; private set; }
            public bool EnableSignatures { get; private set; }
            public bool EnableSignatureV2 { get; private set; }

            public RunnerSuiteSource(
                string categoryId,
                string displayName,
                Func<TestRunner, S3TestServer, CancellationToken, Task> executeAsync,
                bool enableSignatures = false,
                bool enableSignatureV2 = false)
            {
                if (String.IsNullOrEmpty(categoryId)) throw new ArgumentNullException(nameof(categoryId));
                if (String.IsNullOrEmpty(displayName)) throw new ArgumentNullException(nameof(displayName));
                if (executeAsync == null) throw new ArgumentNullException(nameof(executeAsync));

                CategoryId = categoryId;
                DisplayName = displayName;
                ExecuteAsync = executeAsync;
                EnableSignatures = enableSignatures;
                EnableSignatureV2 = enableSignatureV2;
            }
        }

        private sealed class RunnerOnlySource
        {
            public string CategoryId { get; private set; }
            public string DisplayName { get; private set; }
            public Func<TestRunner, CancellationToken, Task> ExecuteAsync { get; private set; }

            public RunnerOnlySource(
                string categoryId,
                string displayName,
                Func<TestRunner, CancellationToken, Task> executeAsync)
            {
                if (String.IsNullOrEmpty(categoryId)) throw new ArgumentNullException(nameof(categoryId));
                if (String.IsNullOrEmpty(displayName)) throw new ArgumentNullException(nameof(displayName));
                if (executeAsync == null) throw new ArgumentNullException(nameof(executeAsync));

                CategoryId = categoryId;
                DisplayName = displayName;
                ExecuteAsync = executeAsync;
            }
        }
    }
}
