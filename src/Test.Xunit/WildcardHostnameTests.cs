namespace Test.Xunit
{
    using S3ServerLibrary;

    /// <summary>
    /// Tests for wildcard hostname handling in S3Request.
    /// Validates that URLs containing wildcard hostnames (*, +, 0.0.0.0) are
    /// properly resolved using the Host header value.
    /// </summary>
    public class WildcardHostnameTests
    {
        /// <summary>
        /// Asterisk wildcard hostname is replaced with the Host header value.
        /// </summary>
        [Fact]
        public void ReplaceWildcard_Asterisk_ReplacesWithHost()
        {
            string result = S3Request.ReplaceWildcardHostname("http://*:8000/bucket/key", "localhost");
            Assert.Equal("http://localhost:8000/bucket/key", result);
        }

        /// <summary>
        /// Plus wildcard hostname is replaced with the Host header value.
        /// </summary>
        [Fact]
        public void ReplaceWildcard_Plus_ReplacesWithHost()
        {
            string result = S3Request.ReplaceWildcardHostname("http://+:8000/bucket/key", "myhost.local");
            Assert.Equal("http://myhost.local:8000/bucket/key", result);
        }

        /// <summary>
        /// Quad-zero wildcard hostname is replaced with the Host header value.
        /// </summary>
        [Fact]
        public void ReplaceWildcard_QuadZero_ReplacesWithHost()
        {
            string result = S3Request.ReplaceWildcardHostname("http://0.0.0.0:8000/bucket/key", "192.168.1.100");
            Assert.Equal("http://192.168.1.100:8000/bucket/key", result);
        }

        /// <summary>
        /// Non-wildcard hostname is returned unchanged.
        /// </summary>
        [Fact]
        public void ReplaceWildcard_NormalHostname_ReturnsUnchanged()
        {
            string result = S3Request.ReplaceWildcardHostname("http://localhost:8000/bucket/key", "otherhost");
            Assert.Equal("http://localhost:8000/bucket/key", result);
        }

        /// <summary>
        /// HTTPS URLs with wildcard hostname are handled correctly.
        /// </summary>
        [Fact]
        public void ReplaceWildcard_Https_ReplacesCorrectly()
        {
            string result = S3Request.ReplaceWildcardHostname("https://*:443/bucket/key", "secure.example.com");
            Assert.Equal("https://secure.example.com:443/bucket/key", result);
        }

        /// <summary>
        /// URL with path and query string preserves all components after hostname replacement.
        /// </summary>
        [Fact]
        public void ReplaceWildcard_WithPathAndQuery_PreservesComponents()
        {
            string result = S3Request.ReplaceWildcardHostname("http://*:8000/bucket/key?versionId=1", "localhost");
            Assert.Equal("http://localhost:8000/bucket/key?versionId=1", result);
        }

        /// <summary>
        /// Root URL with wildcard hostname is replaced correctly.
        /// </summary>
        [Fact]
        public void ReplaceWildcard_RootUrl_ReplacesCorrectly()
        {
            string result = S3Request.ReplaceWildcardHostname("http://*:8000/", "localhost");
            Assert.Equal("http://localhost:8000/", result);
        }

        /// <summary>
        /// Asterisk wildcard without port is replaced correctly.
        /// </summary>
        [Fact]
        public void ReplaceWildcard_NoPort_ReplacesCorrectly()
        {
            string result = S3Request.ReplaceWildcardHostname("http://*/bucket/key", "localhost");
            Assert.Equal("http://localhost/bucket/key", result);
        }
    }
}
