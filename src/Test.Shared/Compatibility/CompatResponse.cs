namespace Test.Shared.Compatibility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Captured response from a compatibility target, with helpers for asserting S3 response details.
    /// </summary>
    public class CompatResponse
    {
        #region Public-Members

        /// <summary>
        /// HTTP status code.
        /// </summary>
        public int StatusCode { get; private set; }

        /// <summary>
        /// Response and content headers, case-insensitive.  Multiple values are joined with ", ".
        /// </summary>
        public Dictionary<string, string> Headers { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Response body.  Never null.
        /// </summary>
        public byte[] Body { get; private set; } = Array.Empty<byte>();

        /// <summary>
        /// Response body as UTF-8 text.  Never null.
        /// </summary>
        public string Text { get; private set; } = "";

        /// <summary>
        /// Parsed XML body, or null when the body is empty or not XML.
        /// </summary>
        public XDocument Xml
        {
            get
            {
                if (_XmlParsed) return _Xml;
                _XmlParsed = true;
                if (String.IsNullOrWhiteSpace(Text)) return null;
                try { _Xml = XDocument.Parse(Text, LoadOptions.PreserveWhitespace); } catch (XmlException) { _Xml = null; }
                return _Xml;
            }
        }

        /// <summary>
        /// Error code from an XML error body, or null.
        /// </summary>
        public string ErrorCode
        {
            get
            {
                if (Xml == null || Xml.Root == null || Xml.Root.Name.LocalName != "Error") return null;
                return Element("Code");
            }
        }

        #endregion

        #region Private-Members

        private XDocument _Xml = null;
        private bool _XmlParsed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="statusCode">Status code.</param>
        /// <param name="headers">Headers.</param>
        /// <param name="body">Body.</param>
        public CompatResponse(int statusCode, Dictionary<string, string> headers, byte[] body)
        {
            StatusCode = statusCode;
            if (headers != null) Headers = headers;
            Body = body ?? Array.Empty<byte>();
            Text = Encoding.UTF8.GetString(Body);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Header value, or null when absent.
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <returns>Value or null.</returns>
        public string Header(string name)
        {
            return Headers.TryGetValue(name, out string value) ? value : null;
        }

        /// <summary>
        /// Value of the first element with the supplied local name anywhere in the body, or null.
        /// </summary>
        /// <param name="localName">Element local name.</param>
        /// <returns>Value or null.</returns>
        public string Element(string localName)
        {
            if (Xml == null) return null;
            XElement e = Xml.Descendants().FirstOrDefault(x => x.Name.LocalName == localName);
            return e != null ? e.Value : null;
        }

        /// <summary>
        /// Values of every element with the supplied local name, optionally only those under a parent with the supplied local name.
        /// </summary>
        /// <param name="localName">Element local name.</param>
        /// <param name="parentLocalName">Parent local name, or null for any.</param>
        /// <returns>Values in document order.</returns>
        public List<string> Elements(string localName, string parentLocalName = null)
        {
            if (Xml == null) return new List<string>();
            return Xml.Descendants()
                .Where(x => x.Name.LocalName == localName && (parentLocalName == null || (x.Parent != null && x.Parent.Name.LocalName == parentLocalName)))
                .Select(x => x.Value)
                .ToList();
        }

        /// <summary>
        /// True if an element with the supplied local name exists directly under the root.
        /// </summary>
        /// <param name="localName">Element local name.</param>
        /// <returns>True if present.</returns>
        public bool HasRootChild(string localName)
        {
            return Xml != null && Xml.Root != null && Xml.Root.Elements().Any(x => x.Name.LocalName == localName);
        }

        /// <summary>
        /// Summary for diagnostics.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            return StatusCode + " " + (Text.Length > 400 ? Text.Substring(0, 400) : Text);
        }

        #endregion
    }
}
