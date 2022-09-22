using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace S3ServerLibrary.S3Objects
{
    /// <summary>
    /// Amazon S3 error.
    /// </summary>
    [XmlRoot(ElementName = "Error", IsNullable = true)]
    public class Error
    {
        // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

        #region Public-Members

        /// <summary>
        /// Object key.
        /// </summary>
        [XmlElement(ElementName = "Key", IsNullable = true)]
        public string Key { get; set; } = null;

        /// <summary>
        /// The version identifier for the resource.
        /// </summary>
        [XmlElement(ElementName = "VersionId", IsNullable = true)]
        public string VersionId { get; set; } = null;

        /// <summary>
        /// Unique identifier for the request.
        /// </summary>
        [XmlElement(ElementName = "RequestId", IsNullable = true)]
        public string RequestId { get; set; } = null;

        /// <summary>
        /// The resource incident to the request.
        /// </summary>
        [XmlElement(ElementName = "Resource", IsNullable = true)]
        public string Resource { get; set; } = null;

        /// <summary>
        /// Error code.
        /// </summary>
        [XmlElement(ElementName = "Code")]
        public ErrorCode Code { get; set; } = ErrorCode.InternalError;

        /// <summary>
        /// Message associated with the error.
        /// </summary>
        [XmlElement(ElementName = "Message", IsNullable = true)]
        public string Message
        {
            get
            {
                switch (Code)
                {
                    case ErrorCode.AccessDenied:
                        return "Access denied.";
                    case ErrorCode.AccountProblem:
                        return "There is a problem with your AWS account that prevents the operation from completing successfully.";
                    case ErrorCode.AllAccessDisabled:
                        return "All access to this Amazon S3 resource has been disabled.";
                    case ErrorCode.AmbiguousGrantByEmailAddress:
                        return "The email address you provided is associated with more than one account.";
                    case ErrorCode.AuthorizationHeaderMalformed:
                        return "The authorization header you provided is invalid.";
                    case ErrorCode.BadDigest:
                        return "The Content-MD5 you specified did not match what we received.";
                    case ErrorCode.BucketAlreadyExists:
                        return "The requested bucket name is not available. The bucket namespace is shared by all users of the system. Please select a different name and try again.";
                    case ErrorCode.BucketAlreadyOwnedByYou:
                        return "The bucket you tried to create already exists, and you own it.";
                    case ErrorCode.BucketNotEmpty:
                        return "The bucket you tried to delete is not empty.";
                    case ErrorCode.CredentialsNotSupported:
                        return "This request does not support credentials.";
                    case ErrorCode.CrossLocationLoggingProhibited:
                        return "Cross-location logging not allowed. Buckets in one geographic location cannot log information to a bucket in another location.";
                    case ErrorCode.EntityTooSmall:
                        return "Your proposed upload is smaller than the minimum allowed object size.";
                    case ErrorCode.EntityTooLarge:
                        return "Your proposed upload exceeds the maximum allowed object size.";
                    case ErrorCode.ExpiredToken:
                        return "The provided token has expired.";
                    case ErrorCode.IllegalVersioningConfigurationException:
                        return "Indicates that the versioning configuration specified in the request is invalid.";
                    case ErrorCode.IncompleteBody:
                        return "You did not provide the number of bytes specified by the Content-Length HTTP header.";
                    case ErrorCode.IncorrectNumberOfFilesInPostRequest:
                        return "POST requires exactly one file upload per request.";
                    case ErrorCode.InlineDataTooLarge:
                        return "Inline data exceeds the maximum allowed size.";
                    case ErrorCode.InternalError:
                        return "We encountered an internal error. Please try again.";
                    case ErrorCode.InvalidAccessKeyId:
                        return "The AWS access key ID you provided does not exist in our records.";
                    case ErrorCode.InvalidAddressingHeader:
                        return "You must specify the Anonymous role.";
                    case ErrorCode.InvalidArgument:
                        return "Invalid Argument.";
                    case ErrorCode.InvalidBucketName:
                        return "The specified bucket is not valid.";
                    case ErrorCode.InvalidBucketState:
                        return "The request is not valid with the current state of the bucket.";
                    case ErrorCode.InvalidDigest:
                        return "The Content-MD5 you specified is not valid.";
                    case ErrorCode.InvalidEncryptionAlgorithmError:
                        return "The encryption request you specified is not valid. The valid value is AES256.";
                    case ErrorCode.InvalidLocationConstraint:
                        return "The specified location constraint is not valid.";
                    case ErrorCode.InvalidObjectState:
                        return "The operation is not valid for the current state of the object.";
                    case ErrorCode.InvalidPart:
                        return "One or more of the specified parts could not be found. The part might not have been uploaded, or the specified entity tag might not have matched the part's entity tag.";
                    case ErrorCode.InvalidPartOrder:
                        return "The list of parts was not in ascending order. Parts list must be specified in order by part number.";
                    case ErrorCode.InvalidPayer:
                        return "All access to this object has been disabled.";
                    case ErrorCode.InvalidPolicyDocument:
                        return "The content of the form does not meet the conditions specified in the policy document.";
                    case ErrorCode.InvalidRange:
                        return "The requested range cannot be satisfied.";
                    case ErrorCode.InvalidRequest:
                        return "Your request is invalid.";
                    case ErrorCode.InvalidSecurity:
                        return "The provided security credentials are not valid.";
                    case ErrorCode.InvalidSOAPRequest:
                        return "The SOAP request body is invalid.";
                    case ErrorCode.InvalidStorageClass:
                        return "The storage class you specified is not valid.";
                    case ErrorCode.InvalidTargetBucketForLogging:
                        return "The target bucket for logging does not exist, is not owned by you, or does not have the appropriate grants for the log-delivery group.";
                    case ErrorCode.InvalidToken:
                        return "The provided token is malformed or otherwise invalid.";
                    case ErrorCode.InvalidURI:
                        return "Couldn't parse the specified URI.";
                    case ErrorCode.KeyTooLongError:
                        return "Your key is too long.";
                    case ErrorCode.MalformedACLError:
                        return "The XML you provided was not well-formed or did not validate against our published schema.";
                    case ErrorCode.MalformedPOSTRequest:
                        return "The body of your POST request is not well-formed multipart/form-data.";
                    case ErrorCode.MalformedXML:
                        return "The XML you provided was not well-formed or did not validate against our published schema.";
                    case ErrorCode.MaxMessageLengthExceeded:
                        return "Your request was too big.";
                    case ErrorCode.MaxPostPreDataLengthExceededError:
                        return "Your POST request fields preceding the upload file were too large.";
                    case ErrorCode.MetadataTooLarge:
                        return "Your metadata headers exceed the maximum allowed metadata size.";
                    case ErrorCode.MethodNotAllowed:
                        return "The specified method is not allowed against this resource.";
                    case ErrorCode.MissingAttachment:
                        return "A SOAP attachment was expected, but none were found.";
                    case ErrorCode.MissingContentLength:
                        return "You must provide the Content-Length HTTP header.";
                    case ErrorCode.MissingRequestBodyError:
                        return "Request body is empty.";
                    case ErrorCode.MissingSecurityElement:
                        return "The SOAP 1.1 request is missing a security element.";
                    case ErrorCode.MissingSecurityHeader:
                        return "Your request is missing a required header.";
                    case ErrorCode.NoLoggingStatusForKey:
                        return "There is no such thing as a logging status subresource for a key.";
                    case ErrorCode.NoSuchBucket:
                        return "The specified bucket does not exist.";
                    case ErrorCode.NoSuchBucketPolicy:
                        return "The specified bucket does not have a bucket policy.";
                    case ErrorCode.NoSuchKey:
                        return "The specified key does not exist.";
                    case ErrorCode.NoSuchLifecycleConfiguration:
                        return "The lifecycle configuration does not exist.";
                    case ErrorCode.NoSuchUpload:
                        return "The specified multipart upload does not exist. The upload ID might be invalid, or the multipart upload might have been aborted or completed.";
                    case ErrorCode.NoSuchVersion:
                        return "The version ID specified in the request does not match an existing version.";
                    case ErrorCode.NotImplemented:
                        return "A header you provided implies functionality that is not implemented.";
                    case ErrorCode.NotSignedUp:
                        return "Your account is not signed up for the Amazon S3 service.";
                    case ErrorCode.OperationAborted:
                        return "A conflicting conditional operation is currently in progress against this resource. Try again.";
                    case ErrorCode.PermanentRedirect:
                        return "The bucket you are attempting to access must be addressed using the specified endpoint. Send all future requests to this endpoint.";
                    case ErrorCode.PreconditionFailed:
                        return "At least one of the preconditions you specified did not hold.";
                    case ErrorCode.Redirect:
                        return "Temporary redirect.";
                    case ErrorCode.RestoreAlreadyInProgress:
                        return "Object restore is already in progress.";
                    case ErrorCode.RequestIsNotMultiPartContent:
                        return "Bucket POST must be of the enclosure-type multipart/form-data.";
                    case ErrorCode.RequestTimeout:
                        return "Your socket connection to the server was not read from or written to within the timeout period.";
                    case ErrorCode.RequestTimeTooSkewed:
                        return "The difference between the request time and the server's time is too large.";
                    case ErrorCode.RequestTorrentOfBucketError:
                        return "Requesting the torrent file of a bucket is not permitted.";
                    case ErrorCode.SignatureDoesNotMatch:
                        return "The request signature we calculated does not match the signature you provided.";
                    case ErrorCode.ServiceUnavailable:
                        return "Reduce your request rate.";
                    case ErrorCode.SlowDown:
                        return "Reduce your request rate.";
                    case ErrorCode.TemporaryRedirect:
                        return "You are being redirected to the bucket while DNS updates.";
                    case ErrorCode.TokenRefreshRequired:
                        return "The provided token must be refreshed.";
                    case ErrorCode.TooManyBuckets:
                        return "You have attempted to create more buckets than allowed.";
                    case ErrorCode.UnexpectedContent:
                        return "This request does not support content.";
                    case ErrorCode.UnresolvableGrantByEmailAddress:
                        return "The email address you provided does not match any account on record.";
                    case ErrorCode.UserKeyMustBeSpecified:
                        return "The bucket POST must contain the specified field name. If it is specified, check the order of the fields.";
                    case ErrorCode.ServerSideEncryptionConfigurationNotFoundError:
                        return "The server side encryption configuration was not found.";
                    default:
                        return "An error of an unknown type was encountered.";
                }
            }
        }

        /// <summary>
        /// HTTP status code.
        /// </summary>
        [XmlElement(ElementName = "HttpStatusCode")]
        public int HttpStatusCode
        {
            get
            {
                switch (Code)
                {
                    case ErrorCode.AccessDenied:
                        return 403;
                    case ErrorCode.AccountProblem:
                        return 403;
                    case ErrorCode.AllAccessDisabled:
                        return 403;
                    case ErrorCode.AmbiguousGrantByEmailAddress:
                        return 400;
                    case ErrorCode.AuthorizationHeaderMalformed:
                        return 400;
                    case ErrorCode.BadDigest:
                        return 400;
                    case ErrorCode.BucketAlreadyExists:
                        return 409;
                    case ErrorCode.BucketAlreadyOwnedByYou:
                        return 409;
                    case ErrorCode.BucketNotEmpty:
                        return 409;
                    case ErrorCode.CredentialsNotSupported:
                        return 400;
                    case ErrorCode.CrossLocationLoggingProhibited:
                        return 403;
                    case ErrorCode.EntityTooSmall:
                        return 400;
                    case ErrorCode.EntityTooLarge:
                        return 400;
                    case ErrorCode.ExpiredToken:
                        return 400;
                    case ErrorCode.IllegalVersioningConfigurationException:
                        return 400;
                    case ErrorCode.IncompleteBody:
                        return 400;
                    case ErrorCode.IncorrectNumberOfFilesInPostRequest:
                        return 400;
                    case ErrorCode.InlineDataTooLarge:
                        return 400;
                    case ErrorCode.InternalError:
                        return 500;
                    case ErrorCode.InvalidAccessKeyId:
                        return 403;
                    case ErrorCode.InvalidAddressingHeader:
                        return 400;
                    case ErrorCode.InvalidArgument:
                        return 400;
                    case ErrorCode.InvalidBucketName:
                        return 400;
                    case ErrorCode.InvalidBucketState:
                        return 409;
                    case ErrorCode.InvalidDigest:
                        return 400;
                    case ErrorCode.InvalidEncryptionAlgorithmError:
                        return 400;
                    case ErrorCode.InvalidLocationConstraint:
                        return 400;
                    case ErrorCode.InvalidObjectState:
                        return 403;
                    case ErrorCode.InvalidPart:
                        return 400;
                    case ErrorCode.InvalidPartOrder:
                        return 400;
                    case ErrorCode.InvalidPayer:
                        return 403;
                    case ErrorCode.InvalidPolicyDocument:
                        return 400;
                    case ErrorCode.InvalidRange:
                        return 416;
                    case ErrorCode.InvalidRequest:
                        return 400;
                    case ErrorCode.InvalidSecurity:
                        return 403;
                    case ErrorCode.InvalidSOAPRequest:
                        return 400;
                    case ErrorCode.InvalidStorageClass:
                        return 400;
                    case ErrorCode.InvalidTargetBucketForLogging:
                        return 400;
                    case ErrorCode.InvalidToken:
                        return 400;
                    case ErrorCode.InvalidURI:
                        return 400;
                    case ErrorCode.KeyTooLongError:
                        return 400;
                    case ErrorCode.MalformedACLError:
                        return 400;
                    case ErrorCode.MalformedPOSTRequest:
                        return 400;
                    case ErrorCode.MalformedXML:
                        return 400;
                    case ErrorCode.MaxMessageLengthExceeded:
                        return 400;
                    case ErrorCode.MaxPostPreDataLengthExceededError:
                        return 400;
                    case ErrorCode.MetadataTooLarge:
                        return 400;
                    case ErrorCode.MethodNotAllowed:
                        return 405;
                    case ErrorCode.MissingAttachment:
                        return 400;
                    case ErrorCode.MissingContentLength:
                        return 411;
                    case ErrorCode.MissingRequestBodyError:
                        return 400;
                    case ErrorCode.MissingSecurityElement:
                        return 400;
                    case ErrorCode.MissingSecurityHeader:
                        return 400;
                    case ErrorCode.NoLoggingStatusForKey:
                        return 400;
                    case ErrorCode.NoSuchBucket:
                        return 404;
                    case ErrorCode.NoSuchBucketPolicy:
                        return 404;
                    case ErrorCode.NoSuchKey:
                        return 404;
                    case ErrorCode.NoSuchLifecycleConfiguration:
                        return 404;
                    case ErrorCode.NoSuchUpload:
                        return 404;
                    case ErrorCode.NoSuchVersion:
                        return 404;
                    case ErrorCode.NotImplemented:
                        return 501;
                    case ErrorCode.NotSignedUp:
                        return 403;
                    case ErrorCode.OperationAborted:
                        return 409;
                    case ErrorCode.PermanentRedirect:
                        return 301;
                    case ErrorCode.PreconditionFailed:
                        return 412;
                    case ErrorCode.Redirect:
                        return 307;
                    case ErrorCode.RestoreAlreadyInProgress:
                        return 409;
                    case ErrorCode.RequestIsNotMultiPartContent:
                        return 400;
                    case ErrorCode.RequestTimeout:
                        return 400;
                    case ErrorCode.RequestTimeTooSkewed:
                        return 403;
                    case ErrorCode.RequestTorrentOfBucketError:
                        return 400;
                    case ErrorCode.SignatureDoesNotMatch:
                        return 403;
                    case ErrorCode.ServiceUnavailable:
                        return 503;
                    case ErrorCode.SlowDown:
                        return 503;
                    case ErrorCode.TemporaryRedirect:
                        return 307;
                    case ErrorCode.TokenRefreshRequired:
                        return 400;
                    case ErrorCode.TooManyBuckets:
                        return 400;
                    case ErrorCode.UnexpectedContent:
                        return 400;
                    case ErrorCode.UnresolvableGrantByEmailAddress:
                        return 400;
                    case ErrorCode.UserKeyMustBeSpecified:
                        return 400;
                    case ErrorCode.ServerSideEncryptionConfigurationNotFoundError:
                        return 400;
                    default:
                        return 500;
                }
            }
        }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public Error()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="error">ErrorCode.</param>
        /// <param name="key">Key.</param>
        /// <param name="versionId">Version ID.</param>
        /// <param name="requestId">Request ID.</param>
        /// <param name="resource">Resource.</param>
        public Error(ErrorCode error, string key = null, string versionId = null, string requestId = null, string resource = null)
        {
            Code = error;
            Key = key;
            VersionId = versionId;
            RequestId = requestId;
            Resource = resource;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
