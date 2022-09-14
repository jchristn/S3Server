# Change Log

## Current Version

v5.0.x

- Minor breaking change
- Rename ```S3RequestStyle``` values to ```PathStyle``` and ```VirtualHostedStyle```
- Remove Newtonsoft.Json dependency
- Changes to (hopefully) improve compatibility with S3 ListObjects APIs

## Previous Versions

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

