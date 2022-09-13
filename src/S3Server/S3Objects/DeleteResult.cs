using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Result from a delete operation.
    /// </summary>
    [XmlRoot(ElementName = "DeleteResult", IsNullable = true)]
    public class DeleteResult
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// List of deleted resources.
        /// </summary>
        [XmlElement(ElementName = "Deleted", IsNullable = true)]
        public List<Deleted> DeletedObjects { get; set; } = new List<Deleted>();

        /// <summary>
        /// List of errors encountered during the operation.
        /// </summary>
        [XmlElement(ElementName = "Error", IsNullable = true)]
        public List<Error> Errors { get; set; } = new List<Error>();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public DeleteResult()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="deleted">Delete.</param>
        /// <param name="error">Error.</param>
        public DeleteResult(List<Deleted> deleted, List<Error> error)
        {
            DeletedObjects = deleted;
            Errors = error;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
