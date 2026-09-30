namespace Test.Shared.Compatibility
{
    using System;

    /// <summary>
    /// Object or part stored by the reference backend.
    /// </summary>
    public class ReferenceObject
    {
        /// <summary>
        /// Data.  Never null.
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Quoted ETag.
        /// </summary>
        public string ETag { get; set; } = null;

        /// <summary>
        /// Last modified time (UTC).
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Content type.
        /// </summary>
        public string ContentType { get; set; } = "binary/octet-stream";
    }
}
