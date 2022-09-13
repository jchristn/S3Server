using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Result from a ListAllMyBuckets request.
    /// </summary>
    [XmlRoot(ElementName = "ListAllMyBucketsResult", IsNullable = true)]
    public class ListAllMyBucketsResult
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Bucket owner.
        /// </summary>
        [XmlElement(ElementName = "Owner", IsNullable = true)]
        public Owner Owner { get; set; } = new Owner();

        /// <summary>
        /// Buckets owned by the user.
        /// </summary>
        [XmlElement(ElementName = "Buckets", IsNullable = true)]
        public Buckets Buckets { get; set; } = new Buckets();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public ListAllMyBucketsResult()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="owner">Owmer/</param>
        /// <param name="buckets">Buckets.</param>
        public ListAllMyBucketsResult(Owner owner, Buckets buckets)
        {
            Owner = owner;
            Buckets = buckets;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
