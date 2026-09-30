namespace Test.Shared.Compatibility
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Multipart upload tracked by the reference backend.
    /// </summary>
    public class ReferenceUpload
    {
        /// <summary>
        /// Bucket.
        /// </summary>
        public string Bucket { get; set; } = null;

        /// <summary>
        /// Key.
        /// </summary>
        public string Key { get; set; } = null;

        /// <summary>
        /// Upload ID.
        /// </summary>
        public string UploadId { get; set; } = null;

        /// <summary>
        /// Initiation time (UTC).
        /// </summary>
        public DateTime Initiated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Parts by part number.
        /// </summary>
        public ConcurrentDictionary<int, ReferenceObject> Parts { get; set; } = new ConcurrentDictionary<int, ReferenceObject>();
    }
}
