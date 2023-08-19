using System;
using System.Collections.Generic;
using System.Text;

namespace S3ServerLibrary
{
    internal class Constants
    {
        internal static string AmazonTimestampFormatVerbose = "ddd, dd MMM yyy HH:mm:ss GMT";
        internal static string AmazonTimestampFormatCompact = "yyyyMMddTHHmmssZ";
        internal static string AmazonDatestampFormat = "yyyyMMdd";

        internal static string HeaderStorageClass = "x-amz-storage-class";
        internal static string HeaderLastModified = "Last-Modified";
        internal static string HeaderRequestId = "x-amz-request-id";
        internal static string HeaderTraceId = "x-amz-id-2";
        internal static string HeaderBucketRegion = "x-amz-bucket-region";
        internal static string HeaderETag = "ETag";
        internal static string HeaderConnection = "Connection";
        internal static string HeaderAcceptRanges = "Accept-Ranges";

        internal static string ContentTypeXml = "application/xml";
        internal static string ContentTypeText = "text/plain";
        internal static string ContentTypeOctetStream = "application/octet-stream";
    }
}
