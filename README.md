![alt tag](https://github.com/jchristn/s3server/blob/master/assets/logo.ico)

# S3Server

Simple S3 server-side interface, produced using Amazon's public documentation.  Want a simple S3 storage server built using S3Server?  Check out [Less3](https://github.com/jchristn/less3).

[![NuGet Version](https://img.shields.io/nuget/v/S3Server.svg?style=flat)](https://www.nuget.org/packages/S3Server/) [![NuGet](https://img.shields.io/nuget/dt/S3Server.svg)](https://www.nuget.org/packages/S3Server) 

## Feedback and Enhancements

Is there an API you'd like exposed that isn't currently?  Did you identify an issue or have other feedback?  Please file an issue here!

## New in v5.0.x

- Minor breaking change
- Rename ```S3RequestStyle``` values to ```PathStyle``` and ```VirtualHostedStyle```
- Remove Newtonsoft.Json dependency
- Changes to (hopefully) improve compatibility with S3 ListObjects APIs

## Examples

Refer to ```Test.Client``` and ```Test.Server``` projects for full examples.

## Important

The following notes should be read prior to using S3Server:

- If you use ```*```, ```+```, ```0.0.0.0```, or any other wildcard for the hostname, you *must* run under administrative privileges. 

- **Path-style URLs are default**, i.e. ```http://hostname.com/bucket/key```
- **Virtual hosting URLs** can be used by setting ```S3Server.BaseDomain``` so that bucket names to appear in the hostname, i.e. ```http://bucket.hostname.com/key```:
  - Set ```S3Server.BaseDomain``` to the base domain, i.e. ```.hostname.com```
  - The ```S3Server.BaseDomain``` must start with a ```.``` (period)
  - You may have to set DNS or your ```hosts``` file to resolve these names accordingly
- Any request where the base domain is NOT found in incoming hostname will be treated as a **path-style URL request** 

## Server

```csharp
using S3ServerLibrary;
using S3ServerLibrary.S3Objects;

// Initialize the server
S3Server server = new S3Server("+", 8000, false, DefaultRequestHandler); // host, port, SSL

// Set callbacks
server.Bucket.Exists = BucketExists;
server.Bucket.Read = BucketRead;
server.Bucket.Delete = BucketDelete;
server.Bucket.WriteTagging = BucketWriteTagging;
server.Object.Exists = ObjectExists;
server.Object.Read = ObjectRead;
server.Object.Write = ObjectWrite;
server.Object.Delete = ObjectDelete;
// etc

// Start the server
server.Start();

// Example callback definition
static async Task DefaultRequestHandler(S3Context ctx)
{
  ctx.Response.StatusCode = 400;
  ctx.Response.ContentType = "text/plain";
  ctx.Response.Send("Bad request");
}

// Callback expecting a response object
static async Task<bool> BucketExists(S3Context ctx)
{
  return true;
  // throw new S3Exception(new Error(ErrorCode.NoSuchBucket));
}

// Callback passing object to you
static async Task BucketWriteTags(S3Context ctx, Tagging tags)
{
  return;
}
```

## Client

Use the following example with the AWSSDK.S3 NuGet package to point your S3 client toward S3Server.
``` csharp
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

BasicAWSCredentials cred = new Amazon.Runtime.BasicAWSCredentials("access key", "secret key");            
AmazonS3Config config = new AmazonS3Config
{
  RegionEndpoint = RegionEndpoint.USWest1, 
  ServiceURL = "http://localhost:8000/",  
  ForcePathStyle = true, // required if not using virtual hosting style
  UseHttp = true
};
IAmazonS3 client = new AmazonS3Client(cred, config);
```

## Request and Responses

An ```S3Context``` object is passed to your callbacks which includes three properties:

- ```S3Request``` - the parsed S3 request
- ```S3Response``` - the response object with which you will interact to send a response
- ```Metadata``` - an object that you can choose to set with your own value
  - This is often helpful if you use the ```PreRequestHandler``` for authentication or other purposes

S3Server expects the code you implement in your callbacks to either:

- Respond with a variable (see ```S3Server.Bucket.Exists```, expects a ```Boolean```)
- Simply return when no response object is required (see ```S3Server.Object.DeleteTagging```)
- Throw an ```S3Exception``` with the appropriate ```Error``` object if something goes wrong

## Operation

S3Server parses incoming HTTP requests, extracting key pieces of information to determine the type of request sent by the caller.  The logic to handle these requests is NOT provided by S3Server; you have to create that logic yourself in your callbacks.  Callbacks are called when a request of that type has been received, otherwise, a generic 400 error is returned to the client.

Refer to https://docs.aws.amazon.com/AmazonS3/latest/API/Welcome.html for the S3 API documentation used to create this project.

As of v3.0.0, the following callbacks are supported:

### Bucket Callbacks

| Callback Name            | Description                         | Method | URL                  |
|--------------------------|-------------------------------------|--------|----------------------|
| Bucket.Delete            | Delete a bucket                     | DELETE | /[bucket]            |
| Bucket.DeleteTagging     | Delete tags from a bucket           | DELETE | /[bucket]?tagging    |
| Bucket.DeleteWebsite     | Delete bucket website configuration | DELETE | /[bucket]?website    |
| Bucket.Exists            | Check if a bucket exists            | HEAD   | /[bucket]            |
| Bucket.Read              | Enumerate a bucket                  | GET    | /[bucket]            |
| Bucket.ReadAcl           | Read ACL on a bucket                | GET    | /[bucket]?acl        |
| Bucket.ReadLocation      | Read a bucket's region              | GET    | /[bucket]?location   |
| Bucket.ReadLogging       | Read bucket's logging configuration | GET    | /[bucket]?logging    |
| Bucket.ReadTagging       | Read tags on a bucket               | GET    | /[bucket]?tagging    |
| Bucket.ReadVersioning    | Read bucket versioning              | GET    | /[bucket]?versioning |
| Bucket.ReadVersions      | Read object versions in a bucket    | GET    | /[bucket]?versions   |
| Bucket.ReadWebsite       | Read bucket website configuration   | GET    | /[bucket]?website    |
| Bucket.Write             | Create a bucket                     | PUT    | /[bucket]            |
| Bucket.WriteAcl          | Write an ACL to a bucket            | PUT    | /[bucket]?acl        |
| Bucket.WriteLogging      | Write bucket logging configuration  | PUT    | /[bucket]?logging    |
| Bucket.WriteTagging      | Write tags to a bucket              | PUT    | /[bucket]?tagging    |
| Bucket.WriteVersioning   | Write bucket versioning             | PUT    | /[bucket]?versioning |
| Bucket.WriteWebsite      | Write bucket website configuration  | PUT    | /[bucket]?website    |

### Object Callbacks

| Callback Name            | Description                             | Method | URL                         |
|--------------------------|-----------------------------------------|--------|-----------------------------|
| Object.Delete            | Delete an object                        | DELETE | /[bucket]/[key]             |
| Object.DeleteTagging     | Delete an object's tags                 | DELETE | /[bucket]/[key]?tagging     |
| Object.DeleteMultiple    | Delete multiple objects                 | POST   | /[bucket]?delete^2          |
| Object.Exists            | Check if an object exists               | HEAD   | /[bucket]/[key]             |
| Object.Write             | Write an object                         | PUT    | /[bucket]/[key]             |
| Object.WriteAcl          | Write an object access control list     | PUT    | /[bucket]/[key]?acl         |
| Object.WriteTagging      | Write tags to an object                 | PUT    | /[bucket]/[key]?tagging     |
| Object.WriteLegalHold    | Write a legal hold status to an object  | PUT    | /[bucket]/[key]?legal-hold  |
| Object.WriteRetention    | Write a retention status to an object   | PUT    | /[bucket]/[key]?retention   |
| Object.Read              | Read an object                          | GET    | /[bucket]/[key]             |
| Object.ReadAcl           | Read an object's access control list    | GET    | /[bucket]/[key]?acl         |
| Object.ReadLegalHold     | Read an object's legal hold status      | GET    | /[bucket]/[key]?legal-hold  |
| Object.ReadRange         | Read a range of bytes from an object    | GET    | /[bucket]/[key]^1           |
| Object.ReadRetention     | Read an object's retention status       | GET    | /[bucket]/[key]?retention   |
| Object.ReadTagging       | Read an object's tags                   | GET    | /[bucket]/[key]?tagging     |

^1 Refer to the Bytes header in ```S3Request.Headers``` for the range that should be retrieved.
^2 A delete multiple request body must be supplied.

### Unsupported / Not Yet Available

Operations against the service or AWS accounts is not exposed through callbacks.  

The following bucket operations are not exposed through callbacks:

- Accelerate
- Analytics
- CORS
- Encryption
- Inventory
- Lifecycle
- Notification
- Object lock configuration
- Object versions
- Policy status
- Public access block
- Metrics
- Payment
- Policy
- Replication

The following object operations are not exposed through callbacks:
 
- Multipart upload
- Parts
- Restore 
- Torrent

The following general capabilities are not yet supported by S3Server

- Validation of chunk signatures

These items may be addressed in a future release.

## Version History

Refer to CHANGELOG.md for details.
