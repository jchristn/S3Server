using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
	/// <summary>
	/// Tag set.
	/// </summary>
	[XmlRoot(ElementName = "TagSet")]
	public class TagSet
	{
		// Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

		#region Public-Members

		/// <summary>
		/// Tag.
		/// </summary>
		[XmlElement(ElementName = "Tag", IsNullable = true)]
		public List<Tag> Tags
        {
			get
            {
				return _TagList;
            }
			set
            {
				if (value == null) _TagList = new List<Tag>();
				else _TagList = value;
            }
        }

		#endregion

		#region Private-Members

		private List<Tag> _TagList = new List<Tag>();

		#endregion

		#region Constructors-and-Factories

		/// <summary>
		/// Instantiate.
		/// </summary>
		public TagSet()
        {

        }

		/// <summary>
		/// Instantiate.
		/// </summary>
		/// <param name="tags">Tags.</param>
		public TagSet(List<Tag> tags)
        {
			Tags = tags;
        }

		#endregion

		#region Public-Methods

		#endregion

		#region Private-Methods

		#endregion
	}
}
