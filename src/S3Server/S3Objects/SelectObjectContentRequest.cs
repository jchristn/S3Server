namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Select object content request.
    /// </summary>
    [XmlRoot(ElementName = "SelectObjectContentRequest")]
    public class SelectObjectContentRequest
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Expression.
        /// </summary>
        [XmlElement(ElementName = "Expression", IsNullable = false)]
        public string Expression { get; set; } = null;

        /// <summary>
        /// Expression type.
        /// </summary>
        [XmlElement(ElementName = "ExpressionType", IsNullable = false)]
        public ExpressionTypeEnum ExpressionType { get; set; } = ExpressionTypeEnum.SQL;

        /// <summary>
        /// Request progress.
        /// </summary>
        [XmlElement(ElementName = "RequestProgress", IsNullable = true)]
        public RequestProgress RequestProgress
        {
            get
            {
                return _RequestProgress;
            }
            set
            {
                if (value == null) _RequestProgress = new RequestProgress();
                else _RequestProgress = value;
            }
        }

        /// <summary>
        /// Input serialization.
        /// </summary>
        [XmlElement(ElementName = "InputSerialization", IsNullable = false)]
        public InputSerialization InputSerialization
        {
            get
            {
                return _InputSerialization;
            }
            set
            {
                if (value == null) _InputSerialization = new InputSerialization();
                else _InputSerialization = value;
            }
        }

        /// <summary>
        /// Output serialization.
        /// </summary>
        [XmlElement(ElementName = "OutputSerialization", IsNullable = false)]
        public OutputSerialization OutputSerialization
        {
            get
            {
                return _OutputSerialization;
            }
            set
            {
                if (value == null) _OutputSerialization = new OutputSerialization();
                else _OutputSerialization = value;
            }
        }

        /// <summary>
        /// Scan range.
        /// </summary>
        [XmlElement(ElementName = "ScanRange", IsNullable = true)]
        public ScanRange ScanRange { get; set; } = new ScanRange();

        #endregion

        #region Private-Members

        private RequestProgress _RequestProgress = new RequestProgress();
        private InputSerialization _InputSerialization = new InputSerialization();
        private OutputSerialization _OutputSerialization = new OutputSerialization();
        private ScanRange _ScanRange = new ScanRange();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public SelectObjectContentRequest()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
