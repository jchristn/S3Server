namespace S3ServerLibrary
{
    using S3ServerLibrary.S3Objects;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Callback methods for service operations.
    /// </summary>
    public class ServiceCallbacks
    {
        #region Public-Members

        /// <summary>
        /// List all buckets.
        /// </summary>
        public Func<S3Context, Task<ListAllMyBucketsResult>> ListBuckets { get; set; } = null;

        /// <summary>
        /// Service exists.
        /// </summary>
        public Func<S3Context, Task<string>> ServiceExists { get; set; } = null;

        /// <summary>
        /// Find matching base domain.  
        /// The input string will be the hostname from the HOST header.  
        /// </summary>
        public Func<string, string> FindMatchingBaseDomain { get; set; } = null;

        /// <summary>
        /// Method to invoke to retrieve the base64-encoded secret key for a given requestor.
        /// </summary>
        public Func<S3Context, string> GetSecretKey { get; set; } = null;

        /// <summary>
        /// Method to invoke to determine whether an unsigned anonymous request should bypass signature validation.
        /// This callback is only consulted when signature validation is enabled and the request contains no
        /// authorization header, signed URL parameters, or other recognized signature material.
        /// </summary>
        public Func<S3Context, Task<bool>> IsAnonymousRequestAllowed { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public ServiceCallbacks()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
