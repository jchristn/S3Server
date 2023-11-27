namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Complete multipart upload request.
    /// </summary>
    [XmlRoot(ElementName = "CompleteMultipartUpload")]
    public class CompleteMultipartUpload
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Parts.
        /// </summary>
        [XmlElement(ElementName = "Part", IsNullable = false)]
        public List<Part> Parts
        {
            get
            {
                return _Parts;
            }
            set
            {
                if (value == null) _Parts = new List<Part>();
                else _Parts = value;
            }
        }

        #endregion

        #region Private-Members

        private List<Part> _Parts = new List<Part>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public CompleteMultipartUpload()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
