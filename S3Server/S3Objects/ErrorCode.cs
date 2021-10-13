using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    // see https://docs.aws.amazon.com/AmazonS3/latest/API/ErrorResponses.html#ErrorCodeList

    /// <summary>
    /// Amazon S3 error code.
    /// </summary>
    public enum ErrorCode
    { 
        /// <summary>
        /// Access denied.
        /// </summary>
        [XmlEnum(Name = "AccessDenied")]
        AccessDenied,
        /// <summary>
        /// There is a problem with your AWS account that prevents the operation from completing successfully.
        /// </summary>
        [XmlEnum(Name = "AccountProblem")]
        AccountProblem,
        /// <summary>
        /// All access to this Amazon S3 resource has been disabled.
        /// </summary>
        [XmlEnum(Name = "AllAccessDisabled")]
        AllAccessDisabled,
        /// <summary>
        /// The email address you provided is associated with more than one account.
        /// </summary>
        [XmlEnum(Name = "AmbiguousGrantByEmailAddress")]
        AmbiguousGrantByEmailAddress,
        /// <summary>
        /// The authorization header you provided is invalid.
        /// </summary>
        [XmlEnum(Name = "AuthorizationHeaderMalformed")]
        AuthorizationHeaderMalformed,
        /// <summary>
        /// The Content-MD5 you specified did not match what we received.
        /// </summary>
        [XmlEnum(Name = "BadDigest")]
        BadDigest,
        /// <summary>
        /// The requested bucket name is not available. The bucket namespace is shared by all users of the system. Please select a different name and try again.
        /// </summary>
        [XmlEnum(Name = "BucketAlreadyExists")]
        BucketAlreadyExists,
        /// <summary>
        /// The bucket you tried to create already exists, and you own it.
        /// </summary>
        [XmlEnum(Name = "BucketAlreadyOwnedByYou")]
        BucketAlreadyOwnedByYou,
        /// <summary>
        /// The bucket you tried to delete is not empty.
        /// </summary>
        [XmlEnum(Name = "BucketNotEmpty")]
        BucketNotEmpty,
        /// <summary>
        /// This request does not support credentials.
        /// </summary>
        [XmlEnum(Name = "CredentialsNotSupported")]
        CredentialsNotSupported,
        /// <summary>
        /// Cross-location logging not allowed. Buckets in one geographic location cannot log information to a bucket in another location.
        /// </summary>
        [XmlEnum(Name = "CrossLocationLoggingProhibited")]
        CrossLocationLoggingProhibited,
        /// <summary>
        /// Your proposed upload is smaller than the minimum allowed object size.
        /// </summary>
        [XmlEnum(Name = "EntityTooSmall")]
        EntityTooSmall,
        /// <summary>
        /// Your proposed upload exceeds the maximum allowed object size.
        /// </summary>
        [XmlEnum(Name = "EntityTooLarge")]
        EntityTooLarge,
        /// <summary>
        /// The provided token has expired.
        /// </summary>
        [XmlEnum(Name = "ExpiredToken")]
        ExpiredToken,
        /// <summary>
        /// Indicates that the versioning configuration specified in the request is invalid.
        /// </summary>
        [XmlEnum(Name = "IllegalVersioningConfigurationException")]
        IllegalVersioningConfigurationException,
        /// <summary>
        /// You did not provide the number of bytes specified by the Content-Length HTTP header.
        /// </summary>
        [XmlEnum(Name = "IncompleteBody")]
        IncompleteBody,
        /// <summary>
        /// POST requires exactly one file upload per request.
        /// </summary>
        [XmlEnum(Name = "IncorrectNumberOfFilesInPostRequest")]
        IncorrectNumberOfFilesInPostRequest,
        /// <summary>
        /// Inline data exceeds the maximum allowed size.
        /// </summary>
        [XmlEnum(Name = "InlineDataTooLarge")]
        InlineDataTooLarge,
        /// <summary>
        /// We encountered an internal error. Please try again.
        /// </summary>
        [XmlEnum(Name = "InternalError")]
        InternalError,
        /// <summary>
        /// The AWS access key ID you provided does not exist in our records.
        /// </summary>
        [XmlEnum(Name = "InvalidAccessKeyId")]
        InvalidAccessKeyId,
        /// <summary>
        /// You must specify the Anonymous role.
        /// </summary>
        [XmlEnum(Name = "InvalidAddressingHeader")]
        InvalidAddressingHeader,
        /// <summary>
        /// Invalid Argument.
        /// </summary>
        [XmlEnum(Name = "InvalidArgument")]
        InvalidArgument,
        /// <summary>
        /// The specified bucket is not valid.
        /// </summary>
        [XmlEnum(Name = "InvalidBucketName")]
        InvalidBucketName,
        /// <summary>
        /// The request is not valid with the current state of the bucket.
        /// </summary>
        [XmlEnum(Name = "InvalidBucketState")]
        InvalidBucketState,
        /// <summary>
        /// The Content-MD5 you specified is not valid.
        /// </summary>
        [XmlEnum(Name = "InvalidDigest")]
        InvalidDigest,
        /// <summary>
        /// The encryption request you specified is not valid. The valid value is AES256.
        /// </summary>
        [XmlEnum(Name = "InvalidEncryptionAlgorithmError")]
        InvalidEncryptionAlgorithmError,
        /// <summary>
        /// The specified location constraint is not valid.
        /// </summary>
        [XmlEnum(Name = "InvalidLocationConstraint")]
        InvalidLocationConstraint,
        /// <summary>
        /// The operation is not valid for the current state of the object.
        /// </summary>
        [XmlEnum(Name = "InvalidObjectState")]
        InvalidObjectState,
        /// <summary>
        /// One or more of the specified parts could not be found. The part might not have been uploaded, or the specified entity tag might not have matched the part's entity tag.
        /// </summary>
        [XmlEnum(Name = "InvalidPart")]
        InvalidPart,
        /// <summary>
        /// The list of parts was not in ascending order. Parts list must be specified in order by part number.
        /// </summary>
        [XmlEnum(Name = "InvalidPartOrder")]
        InvalidPartOrder,
        /// <summary>
        /// All access to this object has been disabled.
        /// </summary>
        [XmlEnum(Name = "InvalidPayer")]
        InvalidPayer,
        /// <summary>
        /// The content of the form does not meet the conditions specified in the policy document.
        /// </summary>
        [XmlEnum(Name = "InvalidPolicyDocument")]
        InvalidPolicyDocument,
        /// <summary>
        /// The requested range cannot be satisfied.
        /// </summary>
        [XmlEnum(Name = "InvalidRange")]
        InvalidRange,
        /// <summary>
        /// Your request is invalid.
        /// </summary>
        [XmlEnum(Name = "InvalidRequest")]
        InvalidRequest,
        /// <summary>
        /// The provided security credentials are not valid.
        /// </summary>
        [XmlEnum(Name = "InvalidSecurity")]
        InvalidSecurity,
        /// <summary>
        /// The SOAP request body is invalid.
        /// </summary>
        [XmlEnum(Name = "InvalidSOAPRequest")]
        InvalidSOAPRequest,
        /// <summary>
        /// The storage class you specified is not valid.
        /// </summary>
        [XmlEnum(Name = "InvalidStorageClass")]
        InvalidStorageClass,
        /// <summary>
        /// The target bucket for logging does not exist, is not owned by you, or does not have the appropriate grants for the log-delivery group.
        /// </summary>
        [XmlEnum(Name = "InvalidTargetBucketForLogging")]
        InvalidTargetBucketForLogging,
        /// <summary>
        /// The provided token is malformed or otherwise invalid.
        /// </summary>
        [XmlEnum(Name = "InvalidToken")]
        InvalidToken,
        /// <summary>
        /// Couldn't parse the specified URI.
        /// </summary>
        [XmlEnum(Name = "InvalidURI")]
        InvalidURI,
        /// <summary>
        /// Your key is too long.
        /// </summary>
        [XmlEnum(Name = "KeyTooLongError")]
        KeyTooLongError,
        /// <summary>
        /// The XML you provided was not well-formed or did not validate against our published schema.
        /// </summary>
        [XmlEnum(Name = "MalformedACLError")]
        MalformedACLError,
        /// <summary>
        /// The body of your POST request is not well-formed multipart/form-data.
        /// </summary>
        [XmlEnum(Name = "MalformedPOSTRequest")]
        MalformedPOSTRequest,
        /// <summary>
        /// The XML you provided was not well-formed or did not validate against our published schema.
        /// </summary>
        [XmlEnum(Name = "MalformedXML")]
        MalformedXML,
        /// <summary>
        /// Your request was too big.
        /// </summary>
        [XmlEnum(Name = "MaxMessageLengthExceeded")]
        MaxMessageLengthExceeded,
        /// <summary>
        /// Your POST request fields preceding the upload file were too large.
        /// </summary>
        [XmlEnum(Name = "MaxPostPreDataLengthExceededError")]
        MaxPostPreDataLengthExceededError,
        /// <summary>
        /// Your metadata headers exceed the maximum allowed metadata size.
        /// </summary>
        [XmlEnum(Name = "MetadataTooLarge")]
        MetadataTooLarge,
        /// <summary>
        /// The specified method is not allowed against this resource.
        /// </summary>
        [XmlEnum(Name = "MethodNotAllowed")]
        MethodNotAllowed,
        /// <summary>
        /// A SOAP attachment was expected, but none were found.
        /// </summary>
        [XmlEnum(Name = "MissingAttachment")]
        MissingAttachment,
        /// <summary>
        /// You must provide the Content-Length HTTP header.
        /// </summary>
        [XmlEnum(Name = "MissingContentLength")]
        MissingContentLength,
        /// <summary>
        /// Request body is empty.
        /// </summary>
        [XmlEnum(Name = "MissingRequestBodyError")]
        MissingRequestBodyError,
        /// <summary>
        /// The SOAP 1.1 request is missing a security element.
        /// </summary>
        [XmlEnum(Name = "MissingSecurityElement")]
        MissingSecurityElement,
        /// <summary>
        /// Your request is missing a required header.
        /// </summary>
        [XmlEnum(Name = "MissingSecurityHeader")]
        MissingSecurityHeader,
        /// <summary>
        /// There is no such thing as a logging status subresource for a key.
        /// </summary>
        [XmlEnum(Name = "NoLoggingStatusForKey")]
        NoLoggingStatusForKey,
        /// <summary>
        /// The specified bucket does not exist.
        /// </summary>
        [XmlEnum(Name = "NoSuchBucket")]
        NoSuchBucket,
        /// <summary>
        /// The specified bucket does not have a bucket policy.
        /// </summary>
        [XmlEnum(Name = "NoSuchBucketPolicy")]
        NoSuchBucketPolicy,
        /// <summary>
        /// The specified key does not exist.
        /// </summary>
        [XmlEnum(Name = "NoSuchKey")]
        NoSuchKey,
        /// <summary>
        /// The lifecycle configuration does not exist.
        /// </summary>
        [XmlEnum(Name = "NoSuchLifecycleConfiguration")]
        NoSuchLifecycleConfiguration,
        /// <summary>
        /// The specified multipart upload does not exist. The upload ID might be invalid, or the multipart upload might have been aborted or completed.
        /// </summary>
        [XmlEnum(Name = "NoSuchUpload")]
        NoSuchUpload,
        /// <summary>
        /// The version ID specified in the request does not match an existing version.
        /// </summary>
        [XmlEnum(Name = "NoSuchVersion")]
        NoSuchVersion,
        /// <summary>
        /// A header you provided implies functionality that is not implemented.
        /// </summary>
        [XmlEnum(Name = "NotImplemented")]
        NotImplemented,
        /// <summary>
        /// Your account is not signed up for the Amazon S3 service.
        /// </summary>
        [XmlEnum(Name = "NotSignedUp")]
        NotSignedUp,
        /// <summary>
        /// A conflicting conditional operation is currently in progress against this resource. Try again.
        /// </summary>
        [XmlEnum(Name = "OperationAborted")]
        OperationAborted,
        /// <summary>
        /// The bucket you are attempting to access must be addressed using the specified endpoint. Send all future requests to this endpoint.
        /// </summary>
        [XmlEnum(Name = "PermanentRedirect")]
        PermanentRedirect,
        /// <summary>
        /// At least one of the preconditions you specified did not hold.
        /// </summary>
        [XmlEnum(Name = "PreconditionFailed")]
        PreconditionFailed,
        /// <summary>
        /// Temporary redirect.
        /// </summary>
        [XmlEnum(Name = "Redirect")]
        Redirect,
        /// <summary>
        /// Object restore is already in progress.
        /// </summary>
        [XmlEnum(Name = "RestoreAlreadyInProgress")]
        RestoreAlreadyInProgress,
        /// <summary>
        /// Bucket POST must be of the enclosure-type multipart/form-data.
        /// </summary>
        [XmlEnum(Name = "RequestIsNotMultiPartContent")]
        RequestIsNotMultiPartContent,
        /// <summary>
        /// Your socket connection to the server was not read from or written to within the timeout period.
        /// </summary>
        [XmlEnum(Name = "RequestTimeout")]
        RequestTimeout,
        /// <summary>
        /// The difference between the request time and the server's time is too large.
        /// </summary>
        [XmlEnum(Name = "RequestTimeTooSkewed")]
        RequestTimeTooSkewed,
        /// <summary>
        /// Requesting the torrent file of a bucket is not permitted.
        /// </summary>
        [XmlEnum(Name = "RequestTorrentOfBucketError")]
        RequestTorrentOfBucketError,
        /// <summary>
        /// The request signature we calculated does not match the signature you provided.
        /// </summary>
        [XmlEnum(Name = "SignatureDoesNotMatch")]
        SignatureDoesNotMatch,
        /// <summary>
        /// Service unavailable.
        /// </summary>
        [XmlEnum(Name = "ServiceUnavailable")]
        ServiceUnavailable,
        /// <summary>
        /// Reduce your request rate.
        /// </summary>
        [XmlEnum(Name = "SlowDown")]
        SlowDown,
        /// <summary>
        /// You are being redirected to the bucket while DNS updates.
        /// </summary>
        [XmlEnum(Name = "TemporaryRedirect")]
        TemporaryRedirect,
        /// <summary>
        /// The provided token must be refreshed.
        /// </summary>
        [XmlEnum(Name = "TokenRefreshRequired")]
        TokenRefreshRequired,
        /// <summary>
        /// You have attempted to create more buckets than allowed.
        /// </summary>
        [XmlEnum(Name = "TooManyBuckets")]
        TooManyBuckets,
        /// <summary>
        /// This request does not support content.
        /// </summary>
        [XmlEnum(Name = "UnexpectedContent")]
        UnexpectedContent,
        /// <summary>
        /// The email address you provided does not match any account on record.
        /// </summary>
        [XmlEnum(Name = "UnresolvableGrantByEmailAddress")]
        UnresolvableGrantByEmailAddress,
        /// <summary>
        /// The bucket POST must contain the specified field name. If it is specified, check the order of the fields.
        /// </summary>
        [XmlEnum(Name = "UserKeyMustBeSpecified")]
        UserKeyMustBeSpecified,
        /// <summary>
        /// The server side encryption configuration was not found.
        /// </summary>
        [XmlEnum(Name = "ServerSideEncryptionConfigurationNotFoundError")]
        ServerSideEncryptionConfigurationNotFoundError
    }
}
