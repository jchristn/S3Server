namespace Test.Automated
{
    using Xunit;

    /// <summary>
    /// Automated tests for S3Server using WatsonWebserver (http.sys-based HTTP server).
    /// UseTcpServer is false.
    /// </summary>
    [Collection("S3Server")]
    public class S3ServerHttpTests : S3ServerTestsBase
    {
        /// <summary>
        /// Instantiate.
        /// </summary>
        public S3ServerHttpTests() : base(false)
        {
        }
    }
}
