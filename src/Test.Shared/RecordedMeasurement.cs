namespace Test.Shared
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// One measurement captured by TelemetryCollector.
    /// </summary>
    public class RecordedMeasurement
    {
        #region Public-Members

        /// <summary>
        /// Instrument name.
        /// </summary>
        public string Instrument { get; set; } = null;

        /// <summary>
        /// Instrument unit, may be null.
        /// </summary>
        public string Unit { get; set; } = null;

        /// <summary>
        /// Measured value as a double.
        /// </summary>
        public double Value { get; set; } = 0;

        /// <summary>
        /// Attributes recorded with the measurement.
        /// </summary>
        public Dictionary<string, object> Tags { get; set; } = new Dictionary<string, object>(StringComparer.Ordinal);

        #endregion

        #region Public-Methods

        /// <summary>
        /// Retrieve an attribute value as a string.
        /// </summary>
        /// <param name="key">Attribute key.</param>
        /// <returns>Value, or null if absent.</returns>
        public string Tag(string key)
        {
            if (Tags.TryGetValue(key, out object value) && value != null) return value.ToString();
            return null;
        }

        /// <summary>
        /// Determine whether every supplied attribute matches.
        /// </summary>
        /// <param name="expected">Expected attribute key and value pairs.</param>
        /// <returns>True if all match.</returns>
        public bool Matches(params string[] expected)
        {
            for (int i = 0; i + 1 < expected.Length; i += 2)
            {
                if (!String.Equals(Tag(expected[i]), expected[i + 1], StringComparison.Ordinal)) return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            List<string> parts = new List<string>();
            foreach (KeyValuePair<string, object> tag in Tags) parts.Add(tag.Key + "=" + tag.Value);
            return Instrument + "{" + String.Join(",", parts) + "} " + Value;
        }

        #endregion
    }
}
