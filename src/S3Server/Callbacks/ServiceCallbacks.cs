using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using S3ServerLibrary.S3Objects;
 
namespace S3ServerLibrary
{
    /// <summary>
    /// Callback methods for service operations.
    /// </summary>
    public class ServiceCallbacks
    {
        #region Public-Members

        /// <summary>
        /// List all buckets.
        /// </summary>
        public Func<S3Context, Task<ListAllMyBucketsResult>> ListBuckets = null;

        /// <summary>
        /// Service exists.
        /// </summary>
        public Func<S3Context, Task<string>> ServiceExists = null;

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
