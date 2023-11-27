namespace S3ServerLibrary.S3Objects
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// A collection of buckets.
    /// </summary>
    [XmlRoot(ElementName = "Buckets", IsNullable = true)]
    public class Buckets
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// A list of individual buckets.
        /// </summary>
        [XmlElement(ElementName = "Bucket", IsNullable = true)]
        public List<Bucket> BucketList { get; set; } = new List<Bucket>();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public Buckets()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="buckets">Buckets.</param>
        public Buckets(List<Bucket> buckets)
        {
            if (buckets != null) BucketList = buckets;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
