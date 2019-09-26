# Change Log

## Current Version

v2.0.1.3

- S3RequestStyle and S3RequestType enumerations

## Previous Versions

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

