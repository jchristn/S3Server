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
		public Tag Tag { get; set; } = null;

		#endregion

		#region Private-Members

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
		/// <param name="tag">Tag.</param>
		public TagSet(Tag tag)
        {
			Tag = tag;
        }

		#endregion

		#region Public-Methods

		#endregion

		#region Private-Methods

		#endregion
	}
}
