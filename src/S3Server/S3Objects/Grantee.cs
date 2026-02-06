namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// A permission recipient.
    /// </summary>
    [XmlInclude(typeof(CanonicalUser))]
    [XmlInclude(typeof(Group))]
    [XmlRoot(ElementName = "Grantee")]
    public class Grantee
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// ID of the grantee.
        /// </summary>
        [XmlElement(ElementName = "ID")]
        public string ID { get; set; } = null;

        /// <summary>
        /// Display name.
        /// </summary>
        [XmlElement(ElementName = "DisplayName")]
        public string DisplayName { get; set; } = null;

        /// <summary>
        /// For a group, the URI of the group.
        /// </summary>
        [XmlElement(ElementName = "URI")]
        public string URI { get; set; } = null;

        /// <summary>
        /// Type of grantee.
        /// Valid values are CanonicalUser, AmazonCustomerByEmail, Group.
        /// </summary>
        [XmlIgnore]
        public string GranteeType
        {
            get
            {
                return _Type;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(GranteeType));
                if (!_TypeValidValues.Contains(value)) throw new ArgumentException("Unknown Type '" + value + "'.");
                _Type = value;
            }
        }

        /// <summary>
        /// Email address of the grantee.
        /// </summary>
        [XmlElement(ElementName = "EmailAddress")]
        public string EmailAddress { get; set; } = null;

        #endregion

        #region Private-Members

        private string _Type = "CanonicalUser";
        private List<string> _TypeValidValues = new List<string>
        {
            "CanonicalUser",
            "AmazonCustomerByEmail",
            "Group"
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public Grantee()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="id">ID.</param>
        /// <param name="displayName">Display name.</param>
        /// <param name="uri">URI.</param>
        /// <param name="granteeType">Grantee type.  Valid values are CanonicalUser, AmazonCustomerByEmail, Group.</param>
        /// <param name="email">Email.</param>
        public Grantee(string id, string displayName, string uri, string granteeType, string email)
        {
            ID = id;
            DisplayName = displayName;
            URI = uri;
            GranteeType = granteeType;
            EmailAddress = email;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeID()
        {
            return !String.IsNullOrEmpty(ID);
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeDisplayName()
        {
            return !String.IsNullOrEmpty(DisplayName);
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeURI()
        {
            return !String.IsNullOrEmpty(URI);
        }

        /// <summary>
        /// Helper method for XML serialization.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool ShouldSerializeEmailAddress()
        {
            return !String.IsNullOrEmpty(EmailAddress);
        }

        #endregion

        #region Private-Methods

        #endregion
    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    [XmlType(TypeName = "CanonicalUser")]
    public class CanonicalUser : Grantee
    {
        /// <summary>
        /// Instantiate.
        /// </summary>
        public CanonicalUser()
        {
            base.GranteeType = "CanonicalUser";
        }
    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    [XmlType(TypeName = "Group")]
    public class Group : Grantee
    {
        /// <summary>
        /// Instantiate.
        /// </summary>
        public Group()
        {
            base.GranteeType = "Group";
        }
    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    [XmlType(TypeName = "Group")]
    public class AmazonCustomerByEmail : Grantee
    {
        /// <summary>
        /// Instantiate.
        /// </summary>
        public AmazonCustomerByEmail()
        {
            base.GranteeType = "AmazonCustomerByEmail";
        }
    }
}
