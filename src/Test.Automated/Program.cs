namespace Test.Automated
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Runners;

    /// <summary>
    /// Test runner program for automated S3Server tests.
    /// </summary>
    public static class Program
    {
        private static int _Passed = 0;
        private static int _Failed = 0;
        private static int _Skipped = 0;
        private static string _CurrentTestClass = null;
        private static ManualResetEvent _Finished = new ManualResetEvent(false);
        private static List<string> _FailedTests = new List<string>();
        private static Stopwatch _Stopwatch = new Stopwatch();

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Exit code (0 for success, non-zero for failures).</returns>
        public static int Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("S3Server Automated Test Runner");
            Console.WriteLine("========================================");
            Console.WriteLine("");

            try
            {
                using (AssemblyRunner runner = AssemblyRunner.WithoutAppDomain(Assembly.GetExecutingAssembly().Location))
                {
                    runner.OnDiscoveryComplete = OnDiscoveryComplete;
                    runner.OnExecutionComplete = OnExecutionComplete;
                    runner.OnTestFailed = OnTestFailed;
                    runner.OnTestPassed = OnTestPassed;
                    runner.OnTestSkipped = OnTestSkipped;
                    runner.OnTestStarting = OnTestStarting;

                    Console.WriteLine("Discovering tests...");
                    _Stopwatch.Start();
                    runner.Start();

                    // Wait for completion
                    _Finished.WaitOne();
                    _Finished.Dispose();

                    _Stopwatch.Stop();

                    Console.WriteLine("");
                    Console.WriteLine("========================================");
                    Console.WriteLine("Test Results Summary");
                    Console.WriteLine("========================================");
                    Console.WriteLine($"Passed  : {_Passed}");
                    Console.WriteLine($"Failed  : {_Failed}");
                    Console.WriteLine($"Skipped : {_Skipped}");
                    Console.WriteLine($"Total   : {_Passed + _Failed + _Skipped}");
                    Console.WriteLine($"Runtime : {_Stopwatch.Elapsed.TotalSeconds:F3}s");
                    Console.WriteLine("========================================");

                    if (_Failed > 0)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("FAILED TESTS:");
                        foreach (string failedTest in _FailedTests)
                        {
                            Console.WriteLine($"  - {failedTest}");
                        }

                        Console.WriteLine("");
                        Console.WriteLine($"RESULT: FAIL ({_Failed} failed, {_Stopwatch.Elapsed.TotalSeconds:F3}s)");
                        return 1;
                    }
                    else
                    {
                        Console.WriteLine("");
                        Console.WriteLine($"RESULT: PASS ({_Passed} passed, {_Stopwatch.Elapsed.TotalSeconds:F3}s)");
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("FATAL ERROR:");
                Console.WriteLine(ex.ToString());
                return 1;
            }
        }

        private static void OnDiscoveryComplete(DiscoveryCompleteInfo info)
        {
            Console.WriteLine($"Found {info.TestCasesToRun} test(s) to run");
            Console.WriteLine("");
        }

        private static void OnExecutionComplete(ExecutionCompleteInfo info)
        {
            Console.WriteLine("");
            Console.WriteLine("Test execution complete");
            _Finished.Set();
        }

        private static void OnTestStarting(TestStartingInfo info)
        {
            string testClass = info.TestDisplayName;
            int lastDot = testClass.LastIndexOf('.');
            if (lastDot > 0) testClass = testClass.Substring(0, lastDot);

            if (_CurrentTestClass != null && _CurrentTestClass != testClass)
            {
                Console.WriteLine("");
            }

            _CurrentTestClass = testClass;
            Console.Write($"  {info.TestDisplayName}... ");
        }

        private static void OnTestPassed(TestPassedInfo info)
        {
            _Passed++;
            Console.WriteLine($"PASS ({info.ExecutionTime:F3}s)");
        }

        private static void OnTestFailed(TestFailedInfo info)
        {
            _Failed++;
            _FailedTests.Add(info.TestDisplayName);
            Console.WriteLine($"FAIL ({info.ExecutionTime:F3}s)");
            Console.WriteLine($"    Exception: {info.ExceptionType}");
            Console.WriteLine($"    Message  : {info.ExceptionMessage}");
            if (!String.IsNullOrEmpty(info.ExceptionStackTrace))
            {
                string[] lines = info.ExceptionStackTrace.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0) Console.WriteLine($"    Stack    : {lines[0].Trim()}");
            }
        }

        private static void OnTestSkipped(TestSkippedInfo info)
        {
            _Skipped++;
            Console.WriteLine($"SKIP ({info.SkipReason})");
        }
    }
}
