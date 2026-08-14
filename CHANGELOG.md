# Change Log

## Current Version

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

