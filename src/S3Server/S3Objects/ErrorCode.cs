namespace S3ServerLibrary.S3Objects
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using System.Xml.Serialization;

    // see https://docs.aws.amazon.com/AmazonS3/latest/API/ErrorResponses.html#ErrorCodeList

    /// <summary>
    /// Amazon S3 error code.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ErrorCode
    {
        /// <summary>
        /// Access denied.
        /// </summary>
        [EnumMember(Value = "AccessDenied")]
        [XmlEnum(Name = "AccessDenied")]
        AccessDenied,

        /// <summary>
        /// There is a problem with your AWS account that prevents the operation from completing successfully.
        /// </summary>
        [EnumMember(Value = "AccountProblem")]
        [XmlEnum(Name = "AccountProblem")]
        AccountProblem,

        /// <summary>
        /// All access to this Amazon S3 resource has been disabled.
        /// </summary>
        [EnumMember(Value = "AllAccessDisabled")]
        [XmlEnum(Name = "AllAccessDisabled")]
        AllAccessDisabled,

        /// <summary>
        /// The email address you provided is associated with more than one account.
        /// </summary>
        [EnumMember(Value = "AmbiguousGrantByEmailAddress")]
        [XmlEnum(Name = "AmbiguousGrantByEmailAddress")]
        AmbiguousGrantByEmailAddress,

        /// <summary>
        /// The authorization header you provided is invalid.
        /// </summary>
        [EnumMember(Value = "AuthorizationHeaderMalformed")]
        [XmlEnum(Name = "AuthorizationHeaderMalformed")]
        AuthorizationHeaderMalformed,

        /// <summary>
        /// The Content-MD5 you specified did not match what we received.
        /// </summary>
        [EnumMember(Value = "BadDigest")]
        [XmlEnum(Name = "BadDigest")]
        BadDigest,

        /// <summary>
        /// The requested bucket name is not available. The bucket namespace is shared by all users of the system. Please select a different name and try again.
        /// </summary>
        [EnumMember(Value = "BucketAlreadyExists")]
        [XmlEnum(Name = "BucketAlreadyExists")]
        BucketAlreadyExists,

        /// <summary>
        /// The bucket you tried to create already exists, and you own it.
        /// </summary>
        [EnumMember(Value = "BucketAlreadyOwnedByYou")]
        [XmlEnum(Name = "BucketAlreadyOwnedByYou")]
        BucketAlreadyOwnedByYou,

        /// <summary>
        /// The bucket you tried to delete is not empty.
        /// </summary>
        [EnumMember(Value = "BucketNotEmpty")]
        [XmlEnum(Name = "BucketNotEmpty")]
        BucketNotEmpty,

        /// <summary>
        /// This request does not support credentials.
        /// </summary>
        [EnumMember(Value = "CredentialsNotSupported")]
        [XmlEnum(Name = "CredentialsNotSupported")]
        CredentialsNotSupported,

        /// <summary>
        /// Cross-location logging not allowed. Buckets in one geographic location cannot log information to a bucket in another location.
        /// </summary>
        [EnumMember(Value = "CrossLocationLoggingProhibited")]
        [XmlEnum(Name = "CrossLocationLoggingProhibited")]
        CrossLocationLoggingProhibited,

        /// <summary>
        /// Your proposed upload is smaller than the minimum allowed object size.
        /// </summary>
        [EnumMember(Value = "EntityTooSmall")]
        [XmlEnum(Name = "EntityTooSmall")]
        EntityTooSmall,

        /// <summary>
        /// Your proposed upload exceeds the maximum allowed object size.
        /// </summary>
        [EnumMember(Value = "EntityTooLarge")]
        [XmlEnum(Name = "EntityTooLarge")]
        EntityTooLarge,

        /// <summary>
        /// The provided token has expired.
        /// </summary>
        [EnumMember(Value = "ExpiredToken")]
        [XmlEnum(Name = "ExpiredToken")]
        ExpiredToken,

        /// <summary>
        /// Indicates that the versioning configuration specified in the request is invalid.
        /// </summary>
        [EnumMember(Value = "IllegalVersioningConfigurationException")]
        [XmlEnum(Name = "IllegalVersioningConfigurationException")]
        IllegalVersioningConfigurationException,

        /// <summary>
        /// You did not provide the number of bytes specified by the Content-Length HTTP header.
        /// </summary>
        [EnumMember(Value = "IncompleteBody")]
        [XmlEnum(Name = "IncompleteBody")]
        IncompleteBody,

        /// <summary>
        /// POST requires exactly one file upload per request.
        /// </summary>
        [EnumMember(Value = "IncorrectNumberOfFilesInPostRequest")]
        [XmlEnum(Name = "IncorrectNumberOfFilesInPostRequest")]
        IncorrectNumberOfFilesInPostRequest,

        /// <summary>
        /// Inline data exceeds the maximum allowed size.
        /// </summary>
        [EnumMember(Value = "InlineDataTooLarge")]
        [XmlEnum(Name = "InlineDataTooLarge")]
        InlineDataTooLarge,

        /// <summary>
        /// We encountered an internal error. Please try again.
        /// </summary>
        [EnumMember(Value = "InternalError")]
        [XmlEnum(Name = "InternalError")]
        InternalError,

        /// <summary>
        /// The AWS access key ID you provided does not exist in our records.
        /// </summary>
        [EnumMember(Value = "InvalidAccessKeyId")]
        [XmlEnum(Name = "InvalidAccessKeyId")]
        InvalidAccessKeyId,

        /// <summary>
        /// You must specify the Anonymous role.
        /// </summary>
        [EnumMember(Value = "InvalidAddressingHeader")]
        [XmlEnum(Name = "InvalidAddressingHeader")]
        InvalidAddressingHeader,

        /// <summary>
        /// Invalid Argument.
        /// </summary>
        [EnumMember(Value = "InvalidArgument")]
        [XmlEnum(Name = "InvalidArgument")]
        InvalidArgument,

        /// <summary>
        /// The specified bucket is not valid.
        /// </summary>
        [EnumMember(Value = "InvalidBucketName")]
        [XmlEnum(Name = "InvalidBucketName")]
        InvalidBucketName,

        /// <summary>
        /// The request is not valid with the current state of the bucket.
        /// </summary>
        [EnumMember(Value = "InvalidBucketState")]
        [XmlEnum(Name = "InvalidBucketState")]
        InvalidBucketState,

        /// <summary>
        /// The Content-MD5 you specified is not valid.
        /// </summary>
        [EnumMember(Value = "InvalidDigest")]
        [XmlEnum(Name = "InvalidDigest")]
        InvalidDigest,

        /// <summary>
        /// The encryption request you specified is not valid. The valid value is AES256.
        /// </summary>
        [EnumMember(Value = "InvalidEncryptionAlgorithmError")]
        [XmlEnum(Name = "InvalidEncryptionAlgorithmError")]
        InvalidEncryptionAlgorithmError,

        /// <summary>
        /// The specified location constraint is not valid.
        /// </summary>
        [EnumMember(Value = "InvalidLocationConstraint")]
        [XmlEnum(Name = "InvalidLocationConstraint")]
        InvalidLocationConstraint,

        /// <summary>
        /// The operation is not valid for the current state of the object.
        /// </summary>
        [EnumMember(Value = "InvalidObjectState")]
        [XmlEnum(Name = "InvalidObjectState")]
        InvalidObjectState,

        /// <summary>
        /// One or more of the specified parts could not be found. The part might not have been uploaded, or the specified entity tag might not have matched the part's entity tag.
        /// </summary>
        [EnumMember(Value = "InvalidPart")]
        [XmlEnum(Name = "InvalidPart")]
        InvalidPart,

        /// <summary>
        /// The list of parts was not in ascending order. Parts list must be specified in order by part number.
        /// </summary>
        [EnumMember(Value = "InvalidPartOrder")]
        [XmlEnum(Name = "InvalidPartOrder")]
        InvalidPartOrder,

        /// <summary>
        /// All access to this object has been disabled.
        /// </summary>
        [EnumMember(Value = "InvalidPayer")]
        [XmlEnum(Name = "InvalidPayer")]
        InvalidPayer,

        /// <summary>
        /// The content of the form does not meet the conditions specified in the policy document.
        /// </summary>
        [EnumMember(Value = "InvalidPolicyDocument")]
        [XmlEnum(Name = "InvalidPolicyDocument")]
        InvalidPolicyDocument,

        /// <summary>
        /// The requested range cannot be satisfied.
        /// </summary>
        [EnumMember(Value = "InvalidRange")]
        [XmlEnum(Name = "InvalidRange")]
        InvalidRange,

        /// <summary>
        /// Your request is invalid.
        /// </summary>
        [EnumMember(Value = "InvalidRequest")]
        [XmlEnum(Name = "InvalidRequest")]
        InvalidRequest,

        /// <summary>
        /// The provided security credentials are not valid.
        /// </summary>
        [EnumMember(Value = "InvalidSecurity")]
        [XmlEnum(Name = "InvalidSecurity")]
        InvalidSecurity,

        /// <summary>
        /// The SOAP request body is invalid.
        /// </summary>
        [EnumMember(Value = "InvalidSOAPRequest")]
        [XmlEnum(Name = "InvalidSOAPRequest")]
        InvalidSOAPRequest,

        /// <summary>
        /// The storage class you specified is not valid.
        /// </summary>
        [EnumMember(Value = "InvalidStorageClass")]
        [XmlEnum(Name = "InvalidStorageClass")]
        InvalidStorageClass,

        /// <summary>
        /// The target bucket for logging does not exist, is not owned by you, or does not have the appropriate grants for the log-delivery group.
        /// </summary>
        [EnumMember(Value = "InvalidTargetBucketForLogging")]
        [XmlEnum(Name = "InvalidTargetBucketForLogging")]
        InvalidTargetBucketForLogging,

        /// <summary>
        /// The provided token is malformed or otherwise invalid.
        /// </summary>
        [EnumMember(Value = "InvalidToken")]
        [XmlEnum(Name = "InvalidToken")]
        InvalidToken,

        /// <summary>
        /// Couldn't parse the specified URI.
        /// </summary>
        [EnumMember(Value = "InvalidURI")]
        [XmlEnum(Name = "InvalidURI")]
        InvalidURI,

        /// <summary>
        /// Your key is too long.
        /// </summary>
        [EnumMember(Value = "KeyTooLongError")]
        [XmlEnum(Name = "KeyTooLongError")]
        KeyTooLongError,

        /// <summary>
        /// The XML you provided was not well-formed or did not validate against our published schema.
        /// </summary>
        [EnumMember(Value = "MalformedACLError")]
        [XmlEnum(Name = "MalformedACLError")]
        MalformedACLError,

        /// <summary>
        /// The body of your POST request is not well-formed multipart/form-data.
        /// </summary>
        [EnumMember(Value = "MalformedPOSTRequest")]
        [XmlEnum(Name = "MalformedPOSTRequest")]
        MalformedPOSTRequest,

        /// <summary>
        /// The XML you provided was not well-formed or did not validate against our published schema.
        /// </summary>
        [EnumMember(Value = "MalformedXML")]
        [XmlEnum(Name = "MalformedXML")]
        MalformedXML,

        /// <summary>
        /// Your request was too big.
        /// </summary>
        [EnumMember(Value = "MaxMessageLengthExceeded")]
        [XmlEnum(Name = "MaxMessageLengthExceeded")]
        MaxMessageLengthExceeded,

        /// <summary>
        /// Your POST request fields preceding the upload file were too large.
        /// </summary>
        [EnumMember(Value = "MaxPostPreDataLengthExceededError")]
        [XmlEnum(Name = "MaxPostPreDataLengthExceededError")]
        MaxPostPreDataLengthExceededError,

        /// <summary>
        /// Your metadata headers exceed the maximum allowed metadata size.
        /// </summary>
        [EnumMember(Value = "MetadataTooLarge")]
        [XmlEnum(Name = "MetadataTooLarge")]
        MetadataTooLarge,

        /// <summary>
        /// The specified method is not allowed against this resource.
        /// </summary>
        [EnumMember(Value = "MethodNotAllowed")]
        [XmlEnum(Name = "MethodNotAllowed")]
        MethodNotAllowed,

        /// <summary>
        /// A SOAP attachment was expected, but none were found.
        /// </summary>
        [EnumMember(Value = "MissingAttachment")]
        [XmlEnum(Name = "MissingAttachment")]
        MissingAttachment,

        /// <summary>
        /// You must provide the Content-Length HTTP header.
        /// </summary>
        [EnumMember(Value = "MissingContentLength")]
        [XmlEnum(Name = "MissingContentLength")]
        MissingContentLength,

        /// <summary>
        /// Request body is empty.
        /// </summary>
        [EnumMember(Value = "MissingRequestBodyError")]
        [XmlEnum(Name = "MissingRequestBodyError")]
        MissingRequestBodyError,

        /// <summary>
        /// The SOAP 1.1 request is missing a security element.
        /// </summary>
        [EnumMember(Value = "MissingSecurityElement")]
        [XmlEnum(Name = "MissingSecurityElement")]
        MissingSecurityElement,

        /// <summary>
        /// Your request is missing a required header.
        /// </summary>
        [EnumMember(Value = "MissingSecurityHeader")]
        [XmlEnum(Name = "MissingSecurityHeader")]
        MissingSecurityHeader,

        /// <summary>
        /// There is no such thing as a logging status subresource for a key.
        /// </summary>
        [EnumMember(Value = "NoLoggingStatusForKey")]
        [XmlEnum(Name = "NoLoggingStatusForKey")]
        NoLoggingStatusForKey,

        /// <summary>
        /// The specified bucket does not exist.
        /// </summary>
        [EnumMember(Value = "NoSuchBucket")]
        [XmlEnum(Name = "NoSuchBucket")]
        NoSuchBucket,

        /// <summary>
        /// The specified bucket does not have a bucket policy.
        /// </summary>
        [EnumMember(Value = "NoSuchBucketPolicy")]
        [XmlEnum(Name = "NoSuchBucketPolicy")]
        NoSuchBucketPolicy,

        /// <summary>
        /// The specified key does not exist.
        /// </summary>
        [EnumMember(Value = "NoSuchKey")]
        [XmlEnum(Name = "NoSuchKey")]
        NoSuchKey,

        /// <summary>
        /// The lifecycle configuration does not exist.
        /// </summary>
        [EnumMember(Value = "NoSuchLifecycleConfiguration")]
        [XmlEnum(Name = "NoSuchLifecycleConfiguration")]
        NoSuchLifecycleConfiguration,

        /// <summary>
        /// The specified multipart upload does not exist. The upload ID might be invalid, or the multipart upload might have been aborted or completed.
        /// </summary>
        [EnumMember(Value = "NoSuchUpload")]
        [XmlEnum(Name = "NoSuchUpload")]
        NoSuchUpload,

        /// <summary>
        /// The version ID specified in the request does not match an existing version.
        /// </summary>
        [EnumMember(Value = "NoSuchVersion")]
        [XmlEnum(Name = "NoSuchVersion")]
        NoSuchVersion,

        /// <summary>
        /// A header you provided implies functionality that is not implemented.
        /// </summary>
        [EnumMember(Value = "NotImplemented")]
        [XmlEnum(Name = "NotImplemented")]
        NotImplemented,

        /// <summary>
        /// Your account is not signed up for the Amazon S3 service.
        /// </summary>
        [EnumMember(Value = "NotSignedUp")]
        [XmlEnum(Name = "NotSignedUp")]
        NotSignedUp,

        /// <summary>
        /// A conflicting conditional operation is currently in progress against this resource. Try again.
        /// </summary>
        [EnumMember(Value = "OperationAborted")]
        [XmlEnum(Name = "OperationAborted")]
        OperationAborted,

        /// <summary>
        /// The bucket you are attempting to access must be addressed using the specified endpoint. Send all future requests to this endpoint.
        /// </summary>
        [EnumMember(Value = "PermanentRedirect")]
        [XmlEnum(Name = "PermanentRedirect")]
        PermanentRedirect,

        /// <summary>
        /// At least one of the preconditions you specified did not hold.
        /// </summary>
        [EnumMember(Value = "PreconditionFailed")]
        [XmlEnum(Name = "PreconditionFailed")]
        PreconditionFailed,

        /// <summary>
        /// Temporary redirect.
        /// </summary>
        [EnumMember(Value = "Redirect")]
        [XmlEnum(Name = "Redirect")]
        Redirect,

        /// <summary>
        /// Object restore is already in progress.
        /// </summary>
        [EnumMember(Value = "RestoreAlreadyInProgress")]
        [XmlEnum(Name = "RestoreAlreadyInProgress")]
        RestoreAlreadyInProgress,

        /// <summary>
        /// Bucket POST must be of the enclosure-type multipart/form-data.
        /// </summary>
        [EnumMember(Value = "RequestIsNotMultiPartContent")]
        [XmlEnum(Name = "RequestIsNotMultiPartContent")]
        RequestIsNotMultiPartContent,

        /// <summary>
        /// Your socket connection to the server was not read from or written to within the timeout period.
        /// </summary>
        [EnumMember(Value = "RequestTimeout")]
        [XmlEnum(Name = "RequestTimeout")]
        RequestTimeout,

        /// <summary>
        /// The difference between the request time and the server's time is too large.
        /// </summary>
        [EnumMember(Value = "RequestTimeTooSkewed")]
        [XmlEnum(Name = "RequestTimeTooSkewed")]
        RequestTimeTooSkewed,

        /// <summary>
        /// Requesting the torrent file of a bucket is not permitted.
        /// </summary>
        [EnumMember(Value = "RequestTorrentOfBucketError")]
        [XmlEnum(Name = "RequestTorrentOfBucketError")]
        RequestTorrentOfBucketError,

        /// <summary>
        /// The request signature we calculated does not match the signature you provided.
        /// </summary>
        [EnumMember(Value = "SignatureDoesNotMatch")]
        [XmlEnum(Name = "SignatureDoesNotMatch")]
        SignatureDoesNotMatch,

        /// <summary>
        /// Service unavailable.
        /// </summary>
        [EnumMember(Value = "ServiceUnavailable")]
        [XmlEnum(Name = "ServiceUnavailable")]
        ServiceUnavailable,

        /// <summary>
        /// Reduce your request rate.
        /// </summary>
        [EnumMember(Value = "SlowDown")]
        [XmlEnum(Name = "SlowDown")]
        SlowDown,

        /// <summary>
        /// You are being redirected to the bucket while DNS updates.
        /// </summary>
        [EnumMember(Value = "TemporaryRedirect")]
        [XmlEnum(Name = "TemporaryRedirect")]
        TemporaryRedirect,

        /// <summary>
        /// The provided token must be refreshed.
        /// </summary>
        [EnumMember(Value = "TokenRefreshRequired")]
        [XmlEnum(Name = "TokenRefreshRequired")]
        TokenRefreshRequired,

        /// <summary>
        /// You have attempted to create more buckets than allowed.
        /// </summary>
        [EnumMember(Value = "TooManyBuckets")]
        [XmlEnum(Name = "TooManyBuckets")]
        TooManyBuckets,

        /// <summary>
        /// This request does not support content.
        /// </summary>
        [EnumMember(Value = "UnexpectedContent")]
        [XmlEnum(Name = "UnexpectedContent")]
        UnexpectedContent,

        /// <summary>
        /// The email address you provided does not match any account on record.
        /// </summary>
        [EnumMember(Value = "UnresolvableGrantByEmailAddress")]
        [XmlEnum(Name = "UnresolvableGrantByEmailAddress")]
        UnresolvableGrantByEmailAddress,

        /// <summary>
        /// The bucket POST must contain the specified field name. If it is specified, check the order of the fields.
        /// </summary>
        [EnumMember(Value = "UserKeyMustBeSpecified")]
        [XmlEnum(Name = "UserKeyMustBeSpecified")]
        UserKeyMustBeSpecified,

        /// <summary>
        /// The server side encryption configuration was not found.
        /// </summary>
        [EnumMember(Value = "ServerSideEncryptionConfigurationNotFoundError")]
        [XmlEnum(Name = "ServerSideEncryptionConfigurationNotFoundError")]
        ServerSideEncryptionConfigurationNotFoundError
    }
}
