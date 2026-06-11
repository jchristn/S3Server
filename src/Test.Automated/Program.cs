namespace Test.Automated
{
    using System.Threading.Tasks;
    using Test.Shared;
    using Touchstone.Cli;

    /// <summary>
    /// Automated test runner for S3Server.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Exit code (0 for success, 1 for failures).</returns>
        public static async Task<int> Main(string[] args)
        {
            string resultsPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--results" && i + 1 < args.Length)
                {
                    resultsPath = args[i + 1];
                    break;
                }
            }

            return await ConsoleRunner.RunAsync(S3ServerSuites.All, resultsPath: resultsPath).ConfigureAwait(false);
        }
    }
}
