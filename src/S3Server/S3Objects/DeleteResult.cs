namespace S3ServerLibrary.S3Objects
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Result from a delete operation.
    /// </summary>
    [XmlRoot(ElementName = "DeleteResult", IsNullable = true)]
    public class DeleteResult
    {
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

        /// <summary>
        /// Helper method for XML serialization.  Deleted entries are omitted when the DeleteObjects request set Quiet
        /// to true, so only errors are returned, as Amazon S3 does.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeDeletedObjects()
        {
            return !ResponseSerializationContext.DeleteQuiet && DeletedObjects != null && DeletedObjects.Count > 0;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
