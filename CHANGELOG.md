# Change Log

## Current Version

v8.0.0

S3Server's responses now match Amazon S3 in the details 7.4.0 left out. Every behavior below was recorded from Amazon S3 (us-west-1), and a new compatibility suite enforces it. This is a major version because several changes alter existing behavior; see "Breaking changes" and "Migrating from 7.x".

Request validation

- Querystring parameters are validated only by the operations that use them, as Amazon S3 does. 7.4.0 rejected an invalid `max-keys` or `partNumber` on any request; now, for example, `GET /bucket/key?max-keys=abc` returns the object. The operations and parameters are: `max-keys` (ListObjects, ListObjectsV2, ListObjectVersions), `max-uploads` (ListMultipartUploads), `max-parts` and `part-number-marker` (ListParts), `partNumber` (GetObject, HeadObject, UploadPart, UploadPartCopy), and `encoding-type` (the listing operations)
- Errors match Amazon S3 exactly, including its per-operation wording (for example `Argument maxKeys must be an integer between 0 and 2147483647` for ListObjects, but `max-keys cannot be negative` for ListObjectVersions), and carry `ArgumentName` and `ArgumentValue`
- `partNumber` must be an integer from 1 to 10000 (0 was previously accepted), and `encoding-type` must be `url`
- Copy-source errors use Amazon S3's messages (`Invalid copy source object key`, `Version id cannot be the empty string`, and the `x-amz-copy-source-range` form message) with `ArgumentName` and `ArgumentValue`
- Validation errors are now returned after the request context is created, so they carry `x-amz-request-id`, `RequestId`, and `HostId`, and `PostRequestHandler` runs for them. The error is exposed as `S3Request.ValidationError`. Errors written when a request cannot be parsed at all also carry generated request identifiers
- Added `S3Request.KeyMarker`, `VersionIdMarker`, `UploadIdMarker` (from `key-marker`, `version-id-marker`, `upload-id-marker`), `MaxUploads` (from `max-uploads`, default 1000), and `EncodingType` (from `encoding-type`, normalized to `url`)
- `S3Request.PartNumberMarker` now defaults to 0, Amazon S3's default, instead of 1

Ranges

- `HEAD` now honors `Range`: `206` with `Content-Range` and the length of the range, or `416`. Previously the header was ignored
- When `S3Object.TotalSize` is set, S3Server applies Amazon S3's range rules after `Object.ReadRange` returns: a range starting at or past the end of the object and a zero-length suffix return `416 InvalidRange`, and a suffix range on an empty object returns `200` with an empty body (7.4.0 returned 416)
- `Content-Range` now describes the bytes actually returned, so `bytes=0-100` on a 10-byte object is answered with `bytes 0-9/10` (previously `bytes 0-100/10`)
- `InvalidRange` errors carry `RangeRequested` and `ActualObjectSize`

Response bodies

- `encoding-type=url` is now applied: `Key`, `Prefix`, `Delimiter`, `Marker`, `NextMarker`, `StartAfter`, `KeyMarker`, and `NextKeyMarker` are URL-encoded with Amazon S3's rules (a space becomes `+`, and `/`, letters, digits, `-`, `_`, `.`, and `*` are kept) in ListObjects, ListObjectsV2, ListObjectVersions, and ListMultipartUploads responses, and `EncodingType` is set to `url`. The AWS CLI and boto3 always request this. A callback that sets `EncodingType` itself is assumed to have encoded its values and is left alone
- ListObjects v1 responses always include `Marker` (empty when not set) and never `KeyCount`, `ContinuationToken`, `StartAfter`, or `NextContinuationToken`. ListObjectsV2 responses never include `Marker` or `NextMarker`, and omit `Owner` from each object unless the request set `fetch-owner=true`
- DeleteObjects honors `<Quiet>true</Quiet>`: `Deleted` entries are omitted and only errors are returned
- `LastModified`, `Initiated`, `CreationDate`, and `RetainUntilDate` are written in UTC with exactly three fractional digits (`2026-01-02T03:04:05.000Z`) instead of up to seven
- `Error` adds `BucketName`, `UploadId`, `ArgumentName`, `ArgumentValue`, `Condition`, `RangeRequested`, and `ActualObjectSize`, each written only when set. S3Server fills `Key` for `NoSuchKey`, `BucketName` for `NoSuchBucket`, `UploadId` for `NoSuchUpload`, and `RangeRequested` for `InvalidRange` from the request when the callback did not
- The `InvalidRange`, `PreconditionFailed`, and `MalformedACLError` messages now use Amazon S3's wording
- A malformed ACL body returns `MalformedACLError` instead of `MalformedXML`
- Error responses to `HEAD` have no body
- `CopyObjectResult` and `CopyPartResult` add optional `ChecksumCRC32`, `ChecksumCRC32C`, `ChecksumCRC64NVME`, `ChecksumSHA1`, `ChecksumSHA256`, and `ChecksumType`
- `ListMultipartUploadsResult.MaxUploads` accepts 0, as Amazon S3 does
- A `null` result from `Bucket.Read`, `Bucket.ReadVersions`, or `Bucket.ReadMultipartUploads` returns `NoSuchBucket` instead of `InternalError`, and a `null` result from `Object.DeleteMultiple` returns an empty `DeleteResult`
- Serializing a model directly with `SerializationHelper.SerializeXml` outside a request keeps the model's own shape; the request-dependent behavior above applies only to responses S3Server sends

Response headers

- `Accept-Charset` and the CORS `Access-Control-*` headers are no longer sent by default. Amazon S3 sends no CORS headers for a bucket without a CORS configuration. Added `S3ServerSettings.EmitCorsHeaders` (default `false`), which sends the webserver's CORS headers on responses to requests that carry an `Origin` header, for browser-based clients. `PreserveWebserverDefaultHeaders = true` still restores every previous default header
- `Connection: close` is added only when the webserver's keep-alive is disabled (`WebserverSettings.IO.EnableKeepAlive`, which is off by default), so enabling keep-alive is no longer defeated

Tests

- Added a `Compatibility` suite (63 scenarios) that sends raw SigV4-signed requests to S3Server running `ReferenceS3Backend`, an in-memory backend, with signature validation enabled. It covers ranges, `HEAD` ranges, parameter validation, listing shapes and pagination, `encoding-type=url`, conditional requests, response headers, CopyObject, multipart uploads including UploadPartCopy, DeleteObjects, ACLs, and error bodies. Every expectation was recorded from Amazon S3, and all 63 scenarios pass against both Amazon S3 and S3Server
- Added `Test.Compatibility`, which runs the scenarios against Amazon S3 (`--target s3`, with credentials from `S3COMPAT_ACCESS_KEY` and `S3COMPAT_SECRET_KEY`), against S3Server (`--target local`), or hosts the reference server for external clients (`--serve`). Against Amazon S3 it only touches objects under a unique `s3server-compat-*` prefix
- Checked with the AWS CLI 2.33 (including multipart upload, ranged download, server-side multipart copy, and `s3 sync`), boto3 1.42, and the MinIO client against the reference server
- Added unit coverage for URL encoding, timestamp formatting, error detail elements, and copy result checksums, and updated tests whose expectations changed

Breaking changes

- `partNumber=0` is rejected
- Numeric parameters are no longer rejected on operations that do not use them
- ACL writes with a malformed body return `MalformedACLError`
- `S3Request.PartNumberMarker` defaults to 0
- Listing responses are URL-encoded when the request asks for `encoding-type=url`, and their elements now follow the ListObjects version
- Timestamps have millisecond precision
- CORS and `Accept-Charset` headers are not sent by default
- `HEAD` with `Range` returns `206` or `416`
- A suffix range on an empty object returns `200`
- `Content-Range` reflects the bytes returned

Migrating from 7.x

- If a `ReadParts` callback treats `PartNumberMarker` as inclusive, or relies on the old default of 1, review its comparison: Amazon S3 lists parts after the marker, starting from 0
- If a listing callback URL-encodes keys itself when `encoding-type=url` is requested, set `EncodingType = "url"` on the result so S3Server does not encode them again
- Browser-based clients need `EmitCorsHeaders = true`
- A `ReadRange` callback no longer needs to detect unsatisfiable ranges when it sets `TotalSize`

v7.4.0

Security, data integrity, and protocol accuracy fixes found while building Less3's S3 compatibility suite. The minor version is bumped because the release adds public API and changes some existing behavior. Every behavior change is listed under "Behavior changes" below, with its opt-out setting where one exists.

Security and data integrity

- Added `S3ServerSettings.AuthenticatedRequestHandler` (`Func<S3Context, Task<bool>>`, default `null`). It runs after signature validation and before routing, and has the same contract as `PreRequestHandler` (return `true` when the handler has sent the response). Forged, unsigned, and unknown-key requests never reach it; unsigned requests permitted by `Service.IsAnonymousRequestAllowed` do
- Added `S3ServerSettings.ValidateSignaturesBeforePreRequestHandler` (`bool`, default `false`). When `true`, signature validation runs before `PreRequestHandler`. The default keeps the existing order so consumers that populate `ctx.Metadata` in `PreRequestHandler` for `Service.GetSecretKey` keep working. The XML documentation now states that `PreRequestHandler` sees unauthenticated input under the default order and must not act on or disclose data
- Canned ACL writes (`PUT ?acl` with `x-amz-acl` or `x-amz-grant-*` headers and no body) now reach `Bucket.WriteAcl` and `Object.WriteAcl` with a `null` policy. Previously they failed with `500 InternalError`, which pushed consumers into answering them from `PreRequestHandler` without signature validation. A body that is present but not valid XML still returns `MalformedXML`
- Signature validation now fails closed with `AccessDenied` when `EnableSignatures` is `true` and `Service.GetSecretKey` has been set to `null` after `Start()`. Previously such requests were processed without signature validation
- CopyObject (`PUT` with `x-amz-copy-source`) is now `S3RequestType.ObjectCopy` and routes to the new `Object.Copy` callback, which returns the new `CopyObjectResult`. UploadPartCopy is now `S3RequestType.ObjectUploadPartCopy` and routes to the new `Object.UploadPartCopy` callback, which returns the new `CopyPartResult`. Both results are serialized as the response body with status 200. Previously both arrived at `Object.Write` or `Object.UploadPart` with an empty body, which stored a 0-byte object and could lose data (for example `aws s3 mv` deleted the source afterward)
- `S3Request` parses `x-amz-copy-source` into `CopySourceBucket`, `CopySourceKey`, and `CopySourceVersionId` (URL-decoded; a percent-encoded separator slash, as the AWS SDK for .NET sends, is accepted), `x-amz-copy-source-range` into `CopySourceRangeStart` and `CopySourceRangeEnd`, and the `x-amz-copy-source-if-*` headers into `CopySourceIfMatch`, `CopySourceIfNoneMatch`, `CopySourceIfModifiedSince`, and `CopySourceIfUnmodifiedSince`. A malformed copy source or copy range returns `400 InvalidArgument`
- Requests that fail while being parsed now receive an S3 XML error body instead of the webserver's HTML "It's me, not you" 500 page
- Negative or non-numeric `max-keys`, `max-parts`, `part-number-marker`, and `partNumber` values now return `400 InvalidArgument` with a message naming the parameter, as Amazon S3 does. The `S3Request` property setters keep their `ArgumentOutOfRangeException` contract for programmatic use
- Malformed `Range` headers (a unit other than `bytes`, multiple ranges, non-numeric bounds, both bounds empty, or `last < first`) are now ignored per RFC 9110 section 14.2 and the request routes to `Object.Read` with `200` and the full object, as Amazon S3 does. Previously they produced the HTML 500 page

Protocol accuracy

- Response bodies are now serialized in the Amazon S3 namespace (`http://s3.amazonaws.com/doc/2006-03-01/`), including nested elements. A top-level `Error` keeps no namespace and `BucketLoggingStatus` uses `http://doc.s3.amazonaws.com/2006-03-01`, both matching Amazon S3. Added `SerializationHelper.S3XmlNamespace` and `SerializationHelper.S3LoggingXmlNamespace`. `DeserializeXml` accepts bare, S3-namespaced, and other-namespaced XML, and now honors `xsi:nil` in namespaced XML
- `Deleted` (inside `DeleteResult`) no longer serializes `xsi:nil` elements or `<DeleteMarker>false</DeleteMarker>`. `VersionId` and `DeleteMarkerVersionId` are omitted when empty, `DeleteMarker` is omitted unless `true`, and `DeleteMarker` now defaults to `null` instead of `false`
- `ListBucketResult` adds `NextMarker` (ListObjects v1 resume point when a delimiter is used), `ContinuationToken`, and `StartAfter` (ListObjectsV2 echoes), each omitted when empty. `S3Request` adds `StartAfter` (from `start-after`) and `FetchOwner` (from `fetch-owner=true`)
- `ListVersionsResult` adds `Entries`, a single list of `ObjectVersion` and `DeleteMarker` items serialized in list order so versions and delete markers can be interleaved by key, newest first, as Amazon S3 returns them. When `Entries` is empty, `Versions` and `DeleteMarkers` serialize exactly as before. Deserialization fills all three lists. Serialization goes through a new `ListVersionsEntryCollection` surface property, `XmlEntries`
- Suffix ranges (`Range: bytes=-N`) now set the new `S3Request.RangeSuffixLength` (with `RangeStart` and `RangeEnd` null), route to `Object.ReadRange`, and return `206` with `Content-Range: bytes start-end/total` computed from `S3Object.TotalSize`, which is required for suffix ranges. A missing `TotalSize` fails the request with `InternalError`, and a zero-length suffix or a suffix of an empty object returns `416 InvalidRange`
- Added `ErrorCode.NotModified` (HTTP 304). `S3Response.Send(Error)` and `S3Response.Send(ErrorCode)` send it with headers only, with no body and no `Content-Type`, and preserve headers such as `ETag` and `Last-Modified` already added by the callback
- `S3Request` parses `If-Match` and `If-None-Match` (entity tags as sent, including `*` and weak `W/` tags) and `If-Modified-Since` and `If-Unmodified-Since` (IMF-fixdate, RFC 850, and asctime; unparseable dates are ignored per RFC 9110) into `IfMatch`, `IfNoneMatch`, `IfModifiedSince`, and `IfUnmodifiedSince`. Evaluation stays in the callback; the property documentation describes the RFC 9110 section 13.2.2 evaluation order
- Request-only headers `Host`, `Accept`, and `Accept-Language`, and the blanket `Cache-Control: no-cache`, are no longer added to every response from the webserver's default headers. `Cache-Control` set by a callback now appears once. The CORS `Access-Control-*` defaults and `Accept-Charset` are kept. Added `S3ServerSettings.PreserveWebserverDefaultHeaders` (`bool`, default `false`) to restore the previous headers

Clarity

- `S3Request.RetrieveQueryValue` is documented as returning URL-decoded values (it already did), and the README notes that `ctx.Http.Request.Query.Elements` holds raw, percent-encoded values
- `VersionedEntity.cs` was split so `ObjectVersion` and `DeleteMarker` each have their own file, and the commented-out `// Namespace = ...` lines were removed from the S3 models
- `S3Response.Send(Error)` now throws `ArgumentNullException` for a `null` error up front (it previously failed inside serialization)

Behavior changes

- CopyObject and UploadPartCopy requests no longer reach `Object.Write` or `Object.UploadPart`. Without `Object.Copy` or `Object.UploadPartCopy` they go to `DefaultRequestHandler` or return `NotImplemented`
- `Bucket.WriteAcl` and `Object.WriteAcl` can now receive a `null` policy. Callbacks that dereference the policy without a null check must add one
- Suffix ranges now reach `Object.ReadRange` instead of `Object.Read`, and `RangeEnd` no longer carries the suffix length. Set `RouteSuffixRangesToReadRange = false` to keep the previous routing
- Malformed `Range` headers now return `200` with the full object instead of an error
- Invalid numeric querystring values now return `400 InvalidArgument`; non-numeric values were previously ignored and negative values produced an HTML 500 page
- Every response body except a top-level `Error` now declares an XML namespace, and responses carry fewer headers. Standard S3 clients are unaffected, but anything that string-matches S3Server output may need updating. Set `PreserveWebserverDefaultHeaders = true` to restore the previous headers
- `Deleted.DeleteMarker` defaults to `null` instead of `false`

Tests

- Added `RequestParsingFixTests`, `ResponseFixTests`, `RequestPipelineTests`, and `ResponseSerializationFixTests` with positive and negative cases for every item above, wired through `S3ServerSuites` so `Test.Automated`, `Test.Xunit`, and `Test.Nunit` all run them
- Updated the adversarial HTTP tests for the new `InvalidArgument` and ignored-range behavior

v7.3.2

- Upgraded to Watson `7.1.0` and refreshed the remaining dependencies to their latest stable versions (AWSSDK.S3, RestWrapper, Microsoft.NET.Test.Sdk, NUnit, NUnit3TestAdapter, coverlet.collector, and related test packages)
- Fixed S3 Select request parsing: several value-type properties on the S3 Select models (`RequestProgress.Enabled`, `ScanRange.Start`/`End`, `CsvInputSerialization.AllowQuotedRecordDelimiter`/`FileHeaderInfo`, `CsvOutputSerialization.QuoteFields`, and `JsonInputSerialization.Type`) were annotated with `[XmlElement(IsNullable = true)]`, which is illegal on non-nullable value types and caused `DeserializeXml<SelectObjectContentRequest>` to throw for every `SelectObjectContent` request; the invalid annotations were removed
- Fixed `S3Object.DataString` setter, which copied `value.Length` (character count) instead of the UTF-8 byte length, corrupting multibyte string payloads
- Expanded automated test coverage toward full coverage with new positive and negative unit suites for the S3 data models and `SerializationHelper`, raising line coverage from ~86% to ~94% and branch coverage from ~63% to ~79%
- No public API surface changes

v7.3.1

- Range (`206 Partial Content`) responses now emit a real `Content-Range` total (`bytes start-end/total`) when the `Object.ReadRange` callback sets the new `S3Object.TotalSize` property to the full object size
- Previously the total was always `*` (unknown), which prevented ranged/multipart download clients such as the AWS CLI (`aws s3 cp` of large objects) from parsing the object size
- Backward compatible: when `TotalSize` is not set, the `Content-Range` total remains `*` exactly as before, so existing implementations are unaffected
- Added a range test asserting the `Content-Range` total reflects `TotalSize`

v7.3.0

- Added `Service.IsAnonymousRequestAllowed` for opt-in unsigned anonymous request authorization when `EnableSignatures` is true
- Unsigned requests remain rejected by default unless the host application explicitly allows the parsed request
- Requests that include an authorization header, V2 signed URL parameters, or recognized V4 presigned URL material continue through signature validation and do not fall back to anonymous access
- Added shared signature validation coverage for anonymous public-read/public-write-style requests, denied anonymous requests, invalid signed requests, and V4 presigned URL material
- Package version is now `7.3.0` because the release adds a public compatibility callback

v7.2.0

- Added opt-in legacy AWS Signature V2 validation through `S3ServerSettings.EnableSignatureV2`
- V2 `Authorization: AWS ...` header signatures now validate when `EnableSignatures` and `EnableSignatureV2` are both true
- V2 signed URLs with `AWSAccessKeyId`, `Expires`, and `Signature` now validate when V2 support is explicitly enabled
- Expired, tampered, unknown-access-key, and malformed V2 requests fail closed before operation callbacks execute
- Added canonical AWS Signature V2 fixture coverage, generator-backed V2 request coverage, and virtual-hosted-style request-style tests
- V2 signature comparison now uses fixed-time byte comparison
- xUnit and NUnit adapter execution is serialized to avoid socket-bound integration test port races
- Package version is now `7.2.0` because the release adds a public compatibility setting

v7.1.2

- Flattened shared Touchstone coverage so each named scenario is exposed as a first-class descriptor in the console, xUnit, and NUnit runners
- Added parser/routing tests that assert parsed bucket, key, request type, permissions, range, multipart, V2 header, and V2 signed URL state through real HTTP requests
- Added protocol compatibility coverage for S3 XML response shape, metadata headers, request identifiers, and 204 empty-body behavior
- Added adversarial HTTP coverage for malformed numeric query values, malformed ranges, duplicate query parameters, oversized decoded content length, and post-failure server health
- Added deterministic fuzz-style coverage for object keys, prefixes, and `max-keys` boundaries
- Added lifecycle and concurrency coverage for parallel service/object requests, idempotent disposal, and stopped-listener behavior
- Hardened query parsing for common S3 camel-case aliases such as `AWSAccessKeyId`, `Expires`, `Signature`, `uploadId`, `partNumber`, and `versionId`
- S3 XML error responses sent through `S3Response.Send(Error)` now include request identifiers when available
- Added `coverage.runsettings` for XPlat Code Coverage collection

v7.1.1

- Updated AWSSignatureGenerator to 1.1.0
- Migrated the shared automated tests to Touchstone descriptors
- Test.Automated now uses Touchstone.Cli
- Test.Xunit now uses Touchstone.XunitAdapter
- Added Test.Nunit using Touchstone.NunitAdapter
- Added stricter negative signature validation coverage
- Added missing restore request body coverage
- Added `archive/V2_SIGNATURES.md` with the implementation plan for V2 signatures and V2 signed URLs

v7.1.0

- Added `Object.Restore` callback for `POST /[bucket]/[key]?restore`
- Added `RestoreRequest`, `RestoreObjectResult`, `RestoreStatus`, `GlacierJobParameters`, and `RestoreTierEnum`
- `S3Object` and `ObjectMetadata` can now carry restore state, emitted as the `x-amz-restore` header on GET/HEAD
- Added `x-amz-restore-output-path` support on restore responses
- Added `ObjectAlreadyInActiveTierError` and `GlacierExpeditedRetrievalNotAvailable` error codes
- Added restore coverage to automated, xUnit, compliance, and signature validation tests

## Previous Versions

v7.0.x

- Watson 7.0.9 dependency update
- Updated to Watson 7.0 (Watson.Lite removed; Watson now handles all transport modes natively)
- Target frameworks updated to .NET 8.0 and .NET 10.0
- Updated to AWSSignatureGenerator 1.0.12 with streaming signature validation
- Updated to PrettyId 2.0.1
- AWS Signature V4 validation now supports streaming signatures
- Removed ```UseTcpServer``` setting (Watson 7.0 uses TCP natively)
- Added comprehensive test infrastructure (Test.Shared, Test.Xunit)
- Fix UriFormatException when server is bound to wildcard hostname (*, +, 0.0.0.0)
- Signature validation fail-closed, range 206, unwired ops return 501

v6.0.x

- Breaking changes with dependency updates
- Moved usings inside of namespaces to reduce collisions
- Moved from ```new byte[0]``` to ```Array.Empty<byte>()```
- Size limits for ```ObjectWrite``` (e.g. ```PutObject```), returns ```EntityTooLarge``` if exceeded
- Boolean for enabling or disabling signature validation
- Added bucket and object callbacks in support of multipart uploads
- Added object callback for S3 Select API

v5.3.x

- Dependency updates and bugfixes
- Removal of base domains as a property
- Added callback ```ServiceCallbacks.FindMatchingBaseDomain```
- Added test project ```Test.RequestStyle```

v5.2.x

- Minor breaking changes
- Dependency updates and bugfixes
- Strong naming
- Add HEAD service API (```Service.ServiceExists```)
- ```StorageClassEnum``` replaces the previous string value
- Remove unnecessary static methods
- Disable connection keepalive (via dependency updates)
- Bugfixes in test app
- Fix timestamp formats (impacting ```ObjectExists``` and ```ObjectRead```)
- No longer using GUID strings for request ID and trace ID
- ETag now encapsulated in quotes

v5.1.x

- Dependency updates and bugfixes
- Added ```BucketDeleteAcl``` API

v5.0.x

- Minor breaking change
- Rename ```S3RequestStyle``` values to ```PathStyle``` and ```VirtualHostedStyle```
- Remove Newtonsoft.Json dependency
- Changes to (hopefully) improve compatibility with S3 ListObjects APIs
- HEAD bucket and object APIs now return 404 with ```NoSuchBucket``` and ```NoSuchKey``` errors

v4.0.1

- Breaking changes, massive refactor
- Namespace change
- Request body now deserialized from XML and passed to callbacks
- Callbacks now expect either:
  - Appropriate response object, or
  - That your code will throw an ```S3Exception``` with the appropriate ```Error```
- Variable name consistency within objects
- S3Objects now have:
  - Additional constructors for ease of use
  - Input validation where appropriate (for instance, ```Retention.Mode```)
  - Valid values are present in the documentation
- Cleaned up XML annotations and moved to deserialization that ignores namespaces for better compatibility
- Inclusion of x-amz-request-id and x-amz-id-2 headers

v3.2.1

- Breaking change, removal of handling for validating S3 signatures (too error-prone)
- Internal refactor

v3.0.0

- Breaking change, now passing ```S3Context``` instead of discrete ```S3Request``` and ```S3Response``` objects to callbacks
- Breaking change, metadata now an ```object``` and moved to ```S3Context```

v2.2.0

- Breaking change to GetSecretKey (now passing the entire S3Request instead of just the access key)
- Dependency update

v2.1.3

- .NET 5 support

v2.1.1

- Breaking changes
- ```Start()``` and ```Stop()``` API; ```Start()``` API must be called to start the server
- ```PostRequestHandler``` callback
- ```IsListening``` property

v2.1.0

- Breaking changes
- Support for authenticating request signatures (not chunk signatures)
- Centralized logger support
- Minor refactor

v2.0.1.19

- New S3Request property ```ContinuationToken```

v2.0.1.18

- New S3Request property ```UserMetadata (Dictionary<object, object>)``` 

v2.0.1.17

- New S3Request property ```PermissionsRequired``` and new enum ```S3PermissionType```
- StringEnumConverter on all enums
- Dependency update

v2.0.1.16
 
- Added new properties to S3Request (IsServiceRequest, IsBucketRequest, IsObjectRequest)

v2.0.1.15

- Dependency update

v2.0.1.14
 
- Support for using IP addresses or hostnames in incoming requests
- Support for *either* having the bucket name in the hostname or in the URL (see ```S3Server.BaseDomain```)

- By default, S3Server expects bucket names to appear in the URL, i.e. ```http://hostname.com/bucket/key```
- If you wish to change this so S3Server expects bucket names to appear in the hostname, i.e. ```http://bucket.hostname.com/key```:
  - Set ```S3Server.BaseDomain``` to the base domain, i.e. ```.hostname.com```
  - The ```S3Server.BaseDomain``` must start with a ```.``` (period)
  - Any request where the base domain is NOT found in incoming hostname will be treated as if the bucket name is in the URL and not the hostname

v2.0.1.13

- Bugfixes

v2.0.1.12

- Bucket website callbacks and objects

v2.0.1.9

- Added callbacks and classes for bucket read logging and write logging

v2.0.1.8

- Moved population of RequestType into the S3Request constructor to fix issues with use of PreRequestHandler
- Added RangeStart and RangeEnd parameters to S3Request, automatically populated if Range header is set

v2.0.1.7

- Dependency update

v2.0.1.6

- Retention fix (nullable RetainUntil timestamp)

v2.0.1.5

- Added Retention object

v2.0.1.4

- Added LegalHold object
- Added more XML documentation

v2.0.1.3

- S3RequestStyle and S3RequestType enumerations

v2.0.0.0

- Breaking changes
- Async task-based callbacks
- Changes to callback signatures (response object is now also included) and to how responses are sent
- Stream support to better support large objects, memory efficiency, and throughput
- Better support for chunked transfer-encoding both on request as well as sending the response
- Added ```Prefix``` and ```MaxKeys``` to ```S3Request```
- Reliability and performance fixes
- Dependency updates

v1.5.x

- Added support for object keys that include '/'
- Added support for GET bucket location API and LocationConstraint object
- Automatically add ```X-Amz-Date```, ```Host```, and ```Server``` headers to S3Response if not supplied
- Stream support (more efficient memory use, support for large objects)
- Classes for commonly-used S3 server requests and responses
- Added VersionId to S3Request
 
v1.4.x

- Added Service callbacks including ListBuckets
- TimestampUtc in both S3Response/S3Request
- Owner, Error, and ErrorCode objects
- Now supports authorization v2 and v4 headers

v1.3.x

- Legal hold and retention callbacks

v1.2.x

- Default request handler (when no appropriate callback can be found) caused breaking change to constructor
- Pre-request handler (to allow you to implement your own APIs prior to attempting to match an S3 API)
- Additional constructors
- Various console debugging settings can be found in ```S3Server.ConsoleDebug.*``` 

v1.1.x

- Separate callbacks for each of the various operations (breaking change)

v1.0.x

- Initial release, one request handler method

