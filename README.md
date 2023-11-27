![alt tag](https://github.com/jchristn/s3server/blob/master/assets/logo.ico)

# S3Server

Simple S3 server-side interface, produced using Amazon's public documentation.  Want a simple S3 storage server built using S3Server?  Check out [Less3](https://github.com/jchristn/less3).

[![NuGet Version](https://img.shields.io/nuget/v/S3Server.svg?style=flat)](https://www.nuget.org/packages/S3Server/) [![NuGet](https://img.shields.io/nuget/dt/S3Server.svg)](https://www.nuget.org/packages/S3Server) 

## Feedback and Enhancements

Is there an API you'd like exposed that isn't currently?  Did you identify an issue or have other feedback?  Please file an issue here!

## New in v6.0.x

- Breaking changes with dependency updates
- Moved usings inside of namespaces to reduce collisions
- Moved from ```new byte[0]``` to ```Array.Empty<byte>()```
- Size limits for ```ObjectWrite``` (e.g. ```PutObject```), returns ```EntityTooLarge``` if exceeded
- Boolean for enabling or disabling signature validation
- Added bucket and object callbacks in support of multipart uploads
- Added object callback for S3 Select API

## Examples

Refer to ```Test.Client``` and ```Test.Server``` projects for full examples.

## Important

The following notes should be read prior to using S3Server:

- **Path-style URLs are default**, i.e. ```http://hostname.com/bucket/key```

- **Virtual hosting URLs** can be used by setting ```S3Server.ServiceCallbacks.FindMatchingBaseDomain``` 

  - You must use a wildcard listener (e.g. ```*``` or ```+```) and *run under administrative privileges*
  - This callback takes one input parameter, the hostname from the HTTP request
  - Your implementation should compare the hostname with your list of base domains
  - The return value from this method should be the base domain to which the hostname matches
  - If no match is found, throw a ```KeyNotFoundException```
  - You may have to set DNS or your ```hosts``` file to resolve these names accordingly
  - Alternatively, use a hostname that resolves to localhost (e.g. ```[host].local.gd``` or ```[host].fbi.com```)
  - Any request where the base domain is NOT found in incoming hostname will be treated as a **path-style URL request** 

## Server

Refer to the ```Test.Server``` project for a more complete example.

```csharp
using S3ServerLibrary;
using S3ServerLibrary.S3Objects;

// Initialize the server
S3Server server = new S3Server("[hostname]", 8000, false, DefaultRequestHandler); // host, port, SSL

// Set callbacks
server.Service.ListBuckets = ListBuckets;
server.Bucket.Exists = BucketExists;
// etc

// Start the server
server.Start();

// Example callback definition
static async Task DefaultRequestHandler(S3Context ctx)
{
  ctx.Response.StatusCode = 400;
  ctx.Response.ContentType = Constants.ContentTypeText;
  ctx.Response.Send("Bad request");
}

// Callback expecting a response object
static async Task<ListAllMyBucketsResult> ListBuckets(S3Context ctx)
{
  return new ListAllMyBucketsResult();
}

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

The following callbacks are supported:

### Service Callbacks

| Callback Name            | Description                         | Method | URL                  |
|--------------------------|-------------------------------------|--------|----------------------|
| Service.ListBuckets      | List buckets                        | GET    | /                    |
| Service.ServiceExists    | Retrieve region for the service     | HEAD   | /                    |

### Bucket Callbacks

| Callback Name            | Description                         | Method | URL                  |
|--------------------------|-------------------------------------|--------|----------------------|
| Bucket.Delete            | Delete a bucket                     | DELETE | /[bucket]            |
| Bucket.DeleteAcl         | Delete ACLs from a bucket           | DELETE | /[bucket]?acl        |
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
