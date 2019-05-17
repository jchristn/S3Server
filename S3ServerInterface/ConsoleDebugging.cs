using System;
using System.Collections.Generic;
using System.Text;

namespace S3ServerInterface
{
    /// <summary>
    /// Enable or disable console debugging for various items.
    /// </summary>
    public class ConsoleDebugging
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable console debugging for HTTP requests.
        /// </summary>
        public bool HttpRequests = false;

        /// <summary>
        /// Enable or disable console debugging for S3 request construction.
        /// </summary>
        public bool S3Requests = false;

        /// <summary>
        /// Enable or disable console debugging for exceptions.
        /// </summary>
        public bool Exceptions = false;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        public ConsoleDebugging()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
