namespace Test.Automated
{
    using Xunit;

    /// <summary>
    /// Automated tests for S3Server using WatsonWebserver.Lite (TCP-based HTTP server).
    /// UseTcpServer is true.
    /// </summary>
    [Collection("S3Server")]
    public class S3ServerTcpTests : S3ServerTestsBase
    {
        /// <summary>
        /// Instantiate.
        /// </summary>
        public S3ServerTcpTests() : base(true)
        {
        }
    }
}
