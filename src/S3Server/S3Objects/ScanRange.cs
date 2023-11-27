namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Scan range.
    /// </summary>
    [XmlRoot(ElementName = "ScanRange", IsNullable = true)]
    public class ScanRange
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// End.
        /// </summary>
        [XmlElement(ElementName = "End", IsNullable = true)]
        public long End
        {
            get
            {
                return _End;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(End));
                _End = value;
            }
        }

        /// <summary>
        /// Start.
        /// </summary>
        [XmlElement(ElementName = "Start", IsNullable = true)]
        public long Start
        {
            get
            {
                return _Start;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Start));
                _Start = value;
            }
        }

        #endregion

        #region Private-Members

        private long _End = 0;
        private long _Start = 0;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public ScanRange()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
