namespace S3ServerLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Xml;

    internal class XmlWriterExtended : XmlWriter
    {
        private XmlWriter baseWriter;
        private Stack<string> _Elements = new Stack<string>();
        private bool _UrlEncodeListingValues = false;

        // Timestamps Amazon S3 writes as ISO 8601 with exactly three fractional digits (2026-01-02T03:04:05.000Z).
        private static readonly HashSet<string> _TimestampElements = new HashSet<string>(StringComparer.Ordinal)
        {
            "LastModified",
            "Initiated",
            "CreationDate",
            "RetainUntilDate"
        };

        // Listing values Amazon S3 URL-encodes when the request specifies encoding-type=url.
        private static readonly HashSet<string> _UrlEncodedElements = new HashSet<string>(StringComparer.Ordinal)
        {
            "Key",
            "Prefix",
            "Delimiter",
            "Marker",
            "NextMarker",
            "StartAfter",
            "KeyMarker",
            "NextKeyMarker"
        };

        public XmlWriterExtended(XmlWriter w, bool urlEncodeListingValues = false)
        {
            baseWriter = w;
            _UrlEncodeListingValues = urlEncodeListingValues;
        }

        /// <summary>
        /// URL-encode a value the way Amazon S3 does for encoding-type=url: letters, digits, '-', '_', '.', '*', and '/'
        /// are left as is, a space becomes '+', and every other UTF-8 byte becomes %XX with uppercase hex digits.
        /// </summary>
        internal static string S3UrlEncode(string value)
        {
            if (String.IsNullOrEmpty(value)) return value;

            StringBuilder sb = new StringBuilder(value.Length * 2);
            foreach (byte b in Encoding.UTF8.GetBytes(value))
            {
                char c = (char)b;
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')
                    || c == '-' || c == '_' || c == '.' || c == '*' || c == '/')
                {
                    sb.Append(c);
                }
                else if (c == ' ')
                {
                    sb.Append('+');
                }
                else
                {
                    sb.Append('%').Append(b.ToString("X2", CultureInfo.InvariantCulture));
                }
            }

            return sb.ToString();
        }

        private string Transform(string text)
        {
            if (text == null || _Elements.Count < 1) return text;

            string element = _Elements.Peek();

            if (_TimestampElements.Contains(element)
                && DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsed))
            {
                DateTime utc = parsed.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(parsed, DateTimeKind.Utc) : parsed.ToUniversalTime();
                return utc.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
            }

            if (_UrlEncodeListingValues && _UrlEncodedElements.Contains(element))
                return S3UrlEncode(text);

            return text;
        }

        // Force WriteEndElement to use WriteFullEndElement
        public override void WriteEndElement()
        {
            if (_Elements.Count > 0) _Elements.Pop();
            baseWriter.WriteFullEndElement();
        }

        public override void WriteFullEndElement()
        {
            if (_Elements.Count > 0) _Elements.Pop();
            baseWriter.WriteFullEndElement();
        }

        public override void Close()
        {
            baseWriter.Close();
        }

        public override void Flush()
        {
            baseWriter.Flush();
        }

        public override string LookupPrefix(string ns)
        {
            return (baseWriter.LookupPrefix(ns));
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            baseWriter.WriteBase64(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            baseWriter.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            baseWriter.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            baseWriter.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            baseWriter.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            baseWriter.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteEndAttribute()
        {
            baseWriter.WriteEndAttribute();
        }

        public override void WriteEndDocument()
        {
            baseWriter.WriteEndDocument();
        }

        public override void WriteEntityRef(string name)
        {
            baseWriter.WriteEntityRef(name);
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            baseWriter.WriteProcessingInstruction(name, text);
        }

        public override void WriteRaw(string data)
        {
            baseWriter.WriteRaw(Transform(data));
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            baseWriter.WriteRaw(buffer, index, count);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            baseWriter.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartDocument(bool standalone)
        {
            baseWriter.WriteStartDocument(standalone);
        }

        public override void WriteStartDocument()
        {
            baseWriter.WriteStartDocument();
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            _Elements.Push(localName);
            baseWriter.WriteStartElement(prefix, localName, ns);
        }

        public override WriteState WriteState
        {
            get { return baseWriter.WriteState; }
        }

        public override void WriteString(string text)
        {
            baseWriter.WriteString(Transform(text));
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            baseWriter.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            baseWriter.WriteWhitespace(ws);
        }
    }
}
