using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
 
namespace S3ServerInterface
{
    /// <summary>
    /// Callback methods for service operations.
    /// </summary>
    public class ServiceCallbacks
    {
        #region Public-Members

        /// <summary>
        /// List all buckets.
        /// Success: send an S3Response object with status 200 and ListAllMyBucketsResult in the body.
        /// </summary>
        public Func<S3Context, Task> ListBuckets = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
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
