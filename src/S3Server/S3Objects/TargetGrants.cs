namespace S3ServerLibrary.S3Objects
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Recipients of the grants to access the resource.
    /// </summary>
    [XmlRoot(ElementName = "TargetGrants")]
    public class TargetGrants
    {
        // Namespace = "http://doc.s3.amazonaws.com/2006-03-01"

        #region Public-Members

        /// <summary>
        /// Grant specifying to whom rights are provided to the resource.
        /// </summary>
        [XmlElement(ElementName = "Grant", IsNullable = true)]
        public List<Grant> Grants { get; set; } = new List<Grant>();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public TargetGrants()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="grants">Grants.</param>
        public TargetGrants(List<Grant> grants)
        {
            if (grants != null) Grants = grants;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
