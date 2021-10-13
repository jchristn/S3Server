using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using S3ServerLibrary.S3Objects;

namespace S3ServerLibrary
{
    /// <summary>
    /// S3 object.
    /// </summary>
    public class S3Object
    {
        #region Public-Members

        /// <summary>
        /// Object key.
        /// </summary>
        public string Key { get; set; } = null;

        /// <summary>
        /// Version ID.
        /// </summary>
        public string VersionId { get; set; } = null;

        /// <summary>
        /// Indicates if this version is the latest version for the object.
        /// </summary>
        public bool IsLatest { get; set; } = true;

        /// <summary>
        /// Timestamp from the last modification of the resource.
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// ETag of the resource.
        /// </summary>
        public string ETag { get; set; } = null;

        /// <summary>
        /// Content type.
        /// </summary>
        public string ContentType { get; set; } = "application/octet-stream";

        /// <summary>
        /// The size in bytes of the resource.
        /// </summary>
        public long Size
        {
            get
            {
                return _Size;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Size));
                _Size = value;
            }
        }

        /// <summary>
        /// The class of storage where the resource resides.
        /// Valid values are STANDARD, REDUCED_REDUNDANCY, GLACIER, STANDARD_IA, ONEZONE_IA, INTELLIGENT_TIERING, DEEP_ARCHIVE, OUTPOSTS.
        /// </summary>
        public string StorageClass
        {
            get
            {
                return _StorageClass;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(StorageClass));
                if (!_StorageClassValidValues.Contains(value)) throw new ArgumentException("Unknown StorageClass '" + value + "'.");
                _StorageClass = value;
            }
        }

        /// <summary>
        /// Object owner.
        /// </summary>
        public Owner Owner { get; set; } = new Owner();

        /// <summary>
        /// Stream containing data.
        /// </summary>
        public Stream Data
        {
            get
            {
                if (_DataStream != null) return _DataStream;
                if (_DataBytes != null)
                {
                    _DataStream = new MemoryStream();
                    _DataStream.Write(_DataBytes, 0, _DataBytes.Length);
                    _DataStream.Seek(0, SeekOrigin.Begin);
                    return _DataStream;
                }
                return null;
            }
            set
            {
                _DataStream = null;
                _DataBytes = null;

                if (value != null) _DataStream = value;
            }
        }

        /// <summary>
        /// Data in byte array form.  If the data was supplied as a stream, the stream will be fully read.  If the stream supports seeking, it will automatically seek to the beginning.
        /// </summary>
        public byte[] DataBytes
        {
            get
            {
                if (_DataBytes != null) return _DataBytes;
                if (_DataStream != null)
                {
                    _DataBytes = Common.ReadFromStream(_DataStream, Size, 65536);
                    if (_DataStream.CanSeek) _DataStream.Seek(0, SeekOrigin.Begin);
                    return _DataBytes;
                }
                return null;
            }
            set
            {
                _DataStream = null;
                _DataBytes = null;

                if (value != null)
                {
                    _DataBytes = new byte[value.Length];
                    Buffer.BlockCopy(value, 0, _DataBytes, 0, value.Length);

                    _DataStream = new MemoryStream();
                    _DataStream.Write(_DataBytes, 0, _DataBytes.Length);
                    _DataStream.Seek(0, SeekOrigin.Begin);
                }
            }
        }

        /// <summary>
        /// Data in string form.  If the data was supplied as a stream, the stream will be fully read.  If the stream supports seeking, it will automatically seek to the beginning.
        /// </summary>
        public string DataString
        {
            get
            {
                if (DataBytes != null) return Encoding.UTF8.GetString(DataBytes);
                return null;
            }
            set
            {
                _DataStream = null;
                _DataBytes = null;

                if (!String.IsNullOrEmpty(value))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(value);
                    _DataBytes = new byte[bytes.Length];
                    Buffer.BlockCopy(bytes, 0, _DataBytes, 0, value.Length);

                    _DataStream = new MemoryStream();
                    _DataStream.Write(_DataBytes, 0, _DataBytes.Length);
                    _DataStream.Seek(0, SeekOrigin.Begin);
                }
            }
        }

        #endregion

        #region Private-Members

        private Stream _DataStream = null;
        private byte[] _DataBytes = null;
        private long _Size = 0;
        private string _StorageClass = "STANDARD";
        private List<string> _StorageClassValidValues = new List<string>
        {
            "STANDARD",
            "REDUCED_REDUNDANCY",
            "GLACIER",
            "STANDARD_IA",
            "ONEZONE_IA",
            "INTELLIGENT_TIERING",
            "DEEP_ARCHIVE",
            "OUTPOSTS"
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public S3Object()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="versionId">Version ID.</param>
        /// <param name="isLatest">Is latest.</param>
        /// <param name="lastModified">Last modified.</param>
        /// <param name="etag">ETag.</param>
        /// <param name="size">Size.</param>
        /// <param name="owner">Owner.</param>
        /// <param name="data">Stream containing data.</param>
        /// <param name="contentType">Content type.</param>
        /// <param name="storageClass">Storage class.</param>
        public S3Object(string key, string versionId, bool isLatest, DateTime lastModified, string etag, long size, Owner owner, Stream data, string contentType = "application/octet-stream", string storageClass = "STANDARD")
        {
            Key = key;
            VersionId = versionId;
            IsLatest = isLatest;
            LastModified = lastModified;
            ETag = etag;
            ContentType = contentType;
            Size = size;
            StorageClass = storageClass;
            Owner = owner;
            Data = data;
        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="versionId">Version ID.</param>
        /// <param name="isLatest">Is latest.</param>
        /// <param name="lastModified">Last modified.</param>
        /// <param name="etag">ETag.</param>
        /// <param name="size">Size.</param>
        /// <param name="owner">Owner.</param>
        /// <param name="data">Stream containing data.</param>
        /// <param name="contentType">Content type.</param>
        /// <param name="storageClass">Storage class.</param>
        public S3Object(string key, string versionId, bool isLatest, DateTime lastModified, string etag, long size, Owner owner, byte[] data, string contentType = "application/octet-stream", string storageClass = "STANDARD")
        {
            Key = key;
            VersionId = versionId;
            IsLatest = isLatest;
            LastModified = lastModified;
            ETag = etag;
            ContentType = contentType;
            Size = size;
            StorageClass = storageClass;
            Owner = owner;
            DataBytes = data;
        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="versionId">Version ID.</param>
        /// <param name="isLatest">Is latest.</param>
        /// <param name="lastModified">Last modified.</param>
        /// <param name="etag">ETag.</param>
        /// <param name="size">Size.</param>
        /// <param name="owner">Owner.</param>
        /// <param name="data">Stream containing data.</param>
        /// <param name="contentType">Content type.</param>
        /// <param name="storageClass">Storage class.</param>
        public S3Object(string key, string versionId, bool isLatest, DateTime lastModified, string etag, long size, Owner owner, string data, string contentType = "application/octet-stream", string storageClass = "STANDARD")
        {
            Key = key;
            VersionId = versionId;
            IsLatest = isLatest;
            LastModified = lastModified;
            ETag = etag;
            ContentType = contentType;
            Size = size;
            StorageClass = storageClass;
            Owner = owner;
            DataString = data;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
