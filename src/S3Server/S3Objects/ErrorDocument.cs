using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Object that should serve as the document to return should an error be encountered.
    /// </summary>
    [XmlRoot(ElementName = "ErrorDocument", IsNullable = true)]
    public class ErrorDocument
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// The key of the object that should serve as the error document.
        /// </summary>
        [XmlElement(ElementName = "Key", IsNullable = true)]
        public string Key { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public ErrorDocument()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="key">Key.</param>
        public ErrorDocument(string key)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            Key = key;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
