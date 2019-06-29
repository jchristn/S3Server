using System;
using System.Collections.Generic;
using System.Text;

using S3ServerInterface;

namespace S3ServerInterface
{
    public class ServiceCallbacks
    {
        #region Public-Members

        /// <summary>
        /// List all buckets.
        /// Success: return an S3Response object with status 200.
        /// </summary>
        public Func<S3Request, S3Response> ListBuckets = null;
         
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
