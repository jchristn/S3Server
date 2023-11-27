namespace S3ServerLibrary
{
    using System;

    /// <summary>
    /// Boundary conditions for certain operations, e.g. PutObject, GetObject, etc.
    /// </summary>
    public class OperationLimitsSettings
    {
        #region Public-Members

        /// <summary>
        /// Maximum content-length for object write (PutObject) before use of multi-part upload is required.
        /// </summary>
        public long MaxPutObjectSize
        {
            get
            {
                return _MaxPutObjectSize;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(MaxPutObjectSize));
                else _MaxPutObjectSize = value;
            }
        }

        #endregion

        #region Private-Members

        private long _MaxPutObjectSize = (1024 * 1024 * 128); // 128MB

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public OperationLimitsSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
