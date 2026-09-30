namespace S3ServerLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Parses HTTP conditional request header values (If-Match, If-None-Match, If-Modified-Since,
    /// If-Unmodified-Since, and the x-amz-copy-source-if-* equivalents) per RFC 9110.
    /// Thread-safe; all members are stateless.
    /// </summary>
    internal static class ConditionalHeaderParser
    {
        #region Private-Members

        private static readonly string[] _HttpDateFormats = new string[]
        {
            "r",                                // IMF-fixdate (RFC 1123): Sun, 06 Nov 1994 08:49:37 GMT
            "dddd, dd-MMM-yy HH:mm:ss 'GMT'",   // obsolete RFC 850: Sunday, 06-Nov-94 08:49:37 GMT
            "ddd MMM d HH:mm:ss yyyy",          // obsolete asctime: Sun Nov  6 08:49:37 1994
            "ddd MMM dd HH:mm:ss yyyy"
        };

        #endregion

        #region Internal-Methods

        /// <summary>
        /// Parse an entity tag list such as <c>"abc", W/"def"</c> or <c>*</c>.
        /// Entity tags are returned exactly as sent (including quotes and any W/ prefix), trimmed of surrounding whitespace.
        /// Commas inside quoted entity tags do not split the list.
        /// </summary>
        /// <param name="value">Header value.  Null or whitespace returns null.</param>
        /// <returns>List of entity tags, or null when the header is absent or empty.</returns>
        internal static List<string> ParseEntityTagList(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return null;

            List<string> ret = new List<string>();
            StringBuilder current = new StringBuilder();
            bool inQuotes = false;

            foreach (char c in value)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    current.Append(c);
                }
                else if (c == ',' && !inQuotes)
                {
                    AddTag(ret, current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            AddTag(ret, current.ToString());

            if (ret.Count < 1) return null;
            return ret;
        }

        /// <summary>
        /// Parse an HTTP date (IMF-fixdate, RFC 850, or asctime).
        /// Per RFC 9110, an unparseable date is ignored, which is represented by a null return value.
        /// </summary>
        /// <param name="value">Header value.</param>
        /// <returns>UTC timestamp, or null if absent or unparseable.</returns>
        internal static DateTime? ParseHttpDate(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return null;

            string trimmed = value.Trim();

            if (DateTime.TryParseExact(
                trimmed,
                _HttpDateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowInnerWhite,
                out DateTime parsed))
            {
                return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            }

            return null;
        }

        #endregion

        #region Private-Methods

        private static void AddTag(List<string> list, string tag)
        {
            if (String.IsNullOrWhiteSpace(tag)) return;
            list.Add(tag.Trim());
        }

        #endregion
    }
}
