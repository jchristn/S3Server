namespace S3ServerLibrary
{
    using System;

    /// <summary>
    /// Per-thread request details consulted by the S3 models while S3Server serializes a response, so the XML can take the
    /// shape Amazon S3 uses for that request (ListObjects v1 versus v2, owner omission, DeleteObjects quiet mode) without
    /// modifying the objects returned by callbacks.  XmlSerializer runs synchronously on the calling thread, so a
    /// thread-static scope is safe for concurrent requests.  Outside a scope, every flag has its default value and the
    /// models serialize exactly as they did before.
    /// </summary>
    internal static class ResponseSerializationContext
    {
        #region Internal-Members

        /// <summary>
        /// ListObjects version for the response being serialized: 1, 2, or 0 when not serializing a listing.
        /// </summary>
        internal static int ListType
        {
            get { return _ListType; }
        }

        /// <summary>
        /// True to omit Owner from listed objects (ListObjectsV2 without fetch-owner=true).
        /// </summary>
        internal static bool SuppressListOwner
        {
            get { return _SuppressListOwner; }
        }

        /// <summary>
        /// True to omit Deleted entries from a DeleteResult (DeleteObjects with Quiet set to true).
        /// </summary>
        internal static bool DeleteQuiet
        {
            get { return _DeleteQuiet; }
        }

        #endregion

        #region Private-Members

        [ThreadStatic] private static int _ListType;
        [ThreadStatic] private static bool _SuppressListOwner;
        [ThreadStatic] private static bool _DeleteQuiet;

        #endregion

        #region Internal-Methods

        /// <summary>
        /// Run a serialization with the supplied flags set, restoring the previous values afterward.
        /// </summary>
        /// <param name="listType">ListObjects version, or 0.</param>
        /// <param name="suppressListOwner">Omit Owner from listed objects.</param>
        /// <param name="deleteQuiet">Omit Deleted entries.</param>
        /// <param name="serialize">Serialization to run.  Cannot be null.</param>
        /// <returns>Serialized XML.</returns>
        internal static string Run(int listType, bool suppressListOwner, bool deleteQuiet, Func<string> serialize)
        {
            if (serialize == null) throw new ArgumentNullException(nameof(serialize));

            int priorListType = _ListType;
            bool priorSuppress = _SuppressListOwner;
            bool priorQuiet = _DeleteQuiet;

            _ListType = listType;
            _SuppressListOwner = suppressListOwner;
            _DeleteQuiet = deleteQuiet;

            try
            {
                return serialize();
            }
            finally
            {
                _ListType = priorListType;
                _SuppressListOwner = priorSuppress;
                _DeleteQuiet = priorQuiet;
            }
        }

        #endregion
    }
}
