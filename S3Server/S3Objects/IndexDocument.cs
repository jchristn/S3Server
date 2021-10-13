using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Parameters for the object that should serve as the index document for the bucket.
    /// </summary>
	[XmlRoot(ElementName = "IndexDocument", IsNullable = true)]
    public class IndexDocument
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// The suffix to use for the index document.
        /// </summary>
        [XmlElement(ElementName = "Suffix", IsNullable = true)]
        public string Suffix { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public IndexDocument()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="suffix">Suffix.</param>
        public IndexDocument(string suffix)
        {
            Suffix = suffix;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
