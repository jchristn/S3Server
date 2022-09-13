using System;
using System.Collections.Generic;
using System.Text;

namespace S3ServerLibrary
{
    /// <summary>
    /// Object extensions.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Return a JSON string of this object.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="pretty">Enable or disable pretty print.</param>
        /// <returns>JSON string.</returns>
        public static string ToJson(this object obj, bool pretty)
        {
            return Common.SerializeJson(obj, pretty);
        }
    }
}