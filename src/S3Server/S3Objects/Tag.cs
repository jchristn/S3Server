using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
	/// <summary>
	/// Tag.
	/// </summary>
	[XmlRoot(ElementName = "Tag")]
	public class Tag
	{
		// Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

		#region Public-Members

		/// <summary>
		/// Key.
		/// </summary>
		[XmlElement(ElementName = "Key", IsNullable = true)]
		public string Key
        {
			get
            {
				return _Key;
            }
			set
            {
				if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Key));
				_Key = value;
            }
        }

		/// <summary>
		/// Value.
		/// </summary>
		[XmlElement(ElementName = "Value", IsNullable = true)]
		public string Value { get; set; } = null;

		#endregion

		#region Private-Members

		private string _Key = null;

		#endregion

		#region Constructors-and-Factories

		/// <summary>
		/// Instantiate.
		/// </summary>
		public Tag()
        {

        }

		/// <summary>
		/// Instantiate.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="val">Value.</param>
		public Tag(string key, string val)
        {
			Key = key;
			Value = val;
        }

		#endregion

		#region Public-Methods

		#endregion

		#region Private-Methods

		#endregion
	}
}
