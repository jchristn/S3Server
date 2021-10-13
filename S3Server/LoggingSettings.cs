using System;
using System.Collections.Generic;
using System.Text;

namespace S3ServerLibrary
{
    /// <summary>
    /// Enable or disable logging for various items.
    /// </summary>
    public class LoggingSettings
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable debugging for HTTP requests.
        /// </summary>
        public bool HttpRequests = false;

        /// <summary>
        /// Enable or disable debugging for S3 request construction.
        /// </summary>
        public bool S3Requests = false;
         
        /// <summary>
        /// Enable or disable debugging for exceptions.
        /// </summary>
        public bool Exceptions = false;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public LoggingSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
