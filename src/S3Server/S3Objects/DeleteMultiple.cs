using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Request to delete multiple resources.
    /// </summary>
    [XmlRoot(ElementName = "Delete", IsNullable = true)]
    public class DeleteMultiple
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Enable or disable quiet deletion.
        /// </summary>
        [XmlElement(ElementName = "Quiet")]
        public bool Quiet { get; set; } = false;

        /// <summary>
        /// List of objects to delete.
        /// </summary>
        [XmlElement(ElementName = "Object", IsNullable = true)]
        public List<Object> Objects { get; set; } = new List<Object>();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public DeleteMultiple()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="quiet">Quiet.</param>
        /// <param name="objects">Objects.</param>
        public DeleteMultiple(bool quiet, List<Object> objects)
        {
            if (objects == null) throw new ArgumentNullException(nameof(objects));
            Quiet = quiet;
            Objects = objects;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
