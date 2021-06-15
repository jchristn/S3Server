![alt tag](https://github.com/jchristn/s3serverinterface/blob/master/assets/logo.ico)

# S3ServerInterface

Simple S3 server-side interface, produced using Amazon's public documentation.  Want a simple S3 storage server built using S3ServerInterface?  Check out [Less3](https://github.com/jchristn/less3).

[![NuGet Version](https://img.shields.io/nuget/v/S3Server.svg?style=flat)](https://www.nuget.org/packages/S3Server/) [![NuGet](https://img.shields.io/nuget/dt/S3ServerInterface.svg)](https://www.nuget.org/packages/S3Server) 

## Feedback and Enhancements

Is there an API you'd like exposed that isn't currently?  Did you identify an issue or have other feedback?  Please file an issue here!

## New in v3.2.1

- Breaking change, removal of handling for validating S3 signatures (too error-prone)
- Internal refactor

## Examples

Refer to ```Test.Client``` and ```Test.Server``` projects for full examples.

## Important

The following notes should be read prior to using S3ServerInterface:

- If you use ```*```, ```+```, ```0.0.0.0```, or any other wildcard for the hostname, you *must* run under administrative privileges. 

- By default, S3Server expects bucket names to appear in the URL, i.e. ```http://hostname.com/bucket/key```
- If you wish to change this so S3Server expects bucket names to appear in the hostname, i.e. ```http://bucket.hostname.com/key```:
  - Set ```S3Server.BaseDomain``` to the base domain, i.e. ```.hostname.com```
  - The ```S3Server.BaseDomain``` must start with a ```.``` (period)
  - Any request where the base domain is NOT found in incoming hostname will be treated as if the bucket name is in the URL and not the hostname

## Server

```csharp
using S3ServerInterface;

// Initialize the server
S3Server server = new S3Server("+", 8000, false, DefaultRequestHandler); // host, port, SSL

// Set callbacks
server.Bucket.Exists = BucketExists;
server.Bucket.Read = BucketRead;
server.Bucket.Delete = BucketDelete;
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

static S3Response BucketExists(S3Context ctx)
{
   // implement your logic here
   ctx.Response.StatusCode = 200;
   ctx.Response.Send();
}
```

## Client

Use the following example with the AWSSDK.S3 NuGet package to point your S3 client toward S3ServerInterface.
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

S3ServerInterface expects the code you implement in your callbacks to directly set values in the supplied ```S3Context.S3Response``` and trigger the end of the connection either:

- When chunk transfer-encoding is not indicated in the request, through the use of one of the ```Send()``` APIs
- When chunk transfer-encoding is indicated in the request, through the use of ```SendChunk()``` and ```SendFinalChunk()``` APIs
- Whether chunk transfer-encoding is enabled or disabled, through the use of ```Send(ErrorCode)``` API

S3ServerInterface provides a series of classes that you can use to deserialize request bodies or to serialize your responses.  These are in the namespace ```S3ServerInterface.S3Objects```.

### Reading Request Body Data

All request body metadata is included in the ```S3Request``` object and all payload/request body data is included in ```S3Request.Data```.

If ```S3Request.Chunked``` is **TRUE**, then request body data should be read from the ```S3Request``` object using ```S3Request.ReadChunk()```.  This method returns a ```Chunk``` object which is of the form:

```csharp
public int Length;
public byte[] Data;
public string Metadata;
public bool IsFinalChunk;
```

If ```S3Request.Chunked``` is **FALSE**, then the request body can be read directly from ```S3Request.Data``` using the content length specified in ```S3Request.ContentLength```.

```csharp
byte[] buffer = new byte[65536];
long bytesRemaining = req.ContentLength;
while (bytesRemaining > 0)
{
	int bytesRead = req.Data.ReadAsync(buffer, 0, buffer.Length);
	if (bytesRead > 0)
	{
		// do something with data in buffer from 0 to bytesRead
	}
}
```

## Operation

S3ServerInterface parses incoming HTTP requests, extracting key pieces of information to determine the type of request sent by the caller.  The logic to handle these requests is NOT provided by S3ServerInterface; you have to create that logic yourself in your callbacks.  Callbacks are called when a request of that type has been received, otherwise, a generic 400 error is returned to the client.

Refer to https://docs.aws.amazon.com/AmazonS3/latest/API/Welcome.html for the S3 API documentation used to create this project.

As of v3.0.0, the following callbacks are supported:

### Bucket Callbacks

| Callback Name            | Description                       | Method | URL                  |
|--------------------------|-----------------------------------|--------|----------------------|
| Bucket.Exists            | Check if a bucket exists          | HEAD   | /[bucket]            |
| Bucket.Write             | Create a bucket                   | PUT    | /[bucket]            |
| Bucket.WriteAcl          | Write an ACL to a bucket          | PUT    | /[bucket]?acl        |
| Bucket.WriteTags         | Write tags to a bucket            | PUT    | /[bucket]?tags       |
| Bucket.Read              | Enumerate a bucket                | GET    | /[bucket]            |
| Bucket.ReadLocation      | Retrieve a bucket's region        | GET    | /[bucket]?location   |
| Bucket.ReadAcl           | Read ACL on a bucket              | GET    | /[bucket]?acl        |
| Bucket.ReadTags          | Read tags on a bucket             | GET    | /[bucket]?tags       |
| Bucket.ReadVersions      | Read object versions in a bucket  | GET    | /[bucket]?versions   |
| Bucket.DeleteTags        | Delete tags from a bucket         | GET    | /[bucket]?tags       |
| Bucket.SetVersioning     | Set bucket versioning             | PUT    | /[bucket]?versioning |
| Bucket.GetVersioning     | Get bucket versioning             | PUT    | /[bucket]?versioning |


### Object Callbacks

| Callback Name            | Description                             | Method | URL                         |
|--------------------------|-----------------------------------------|--------|-----------------------------|
| Object.Exists            | Check if an object exists               | HEAD   | /[bucket]/[key]             |
| Object.Write             | Write an object                         | PUT    | /[bucket]/[key]             |
| Object.WriteAcl          | Write an object access control list     | PUT    | /[bucket]/[key]?acl         |
| Object.WriteTags         | Write tags to an object                 | PUT    | /[bucket]/[key]?tags        |
| Object.WriteLegalHold    | Write a legal hold status to an object  | PUT    | /[bucket]/[key]?legal-hold  |
| Object.WriteRetention    | Write a retention status to an object   | PUT    | /[bucket]/[key]?retention   |
| Object.Read              | Read an object                          | GET    | /[bucket]/[key]             |
| Object.ReadAcl           | Read an object's access control list    | GET    | /[bucket]/[key]?acl         |
| Object.ReadRange         | Read a range of bytes from an object    | GET    | /[bucket]/[key]^1           |
| Object.ReadTags          | Read an object's tags                   | GET    | /[bucket]/[key]?tags        |
| Object.ReadLegalHold     | Read an object's legal hold status      | GET    | /[bucket]/[key]?legal-hold  |
| Object.ReadRetention     | Read an object's retention status       | GET    | /[bucket]/[key]?retention   |
| Object.Delete            | Delete an object                        | DELETE | /[bucket]/[key]             |
| Object.DeleteTags        | Delete an object's tags                 | DELETE | /[bucket]/[key]?tags        |
| Object.DeleteMultiple    | Delete multiple objects                 | POST   | /[bucket]?delete^2          |

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
- Logging
- Notification
- Object lock configuration
- Object versions
- Policy status
- Public access block
- Metrics
- Payment
- Policy
- Replication
- Website

The following object operations are not exposed through callbacks:
 
- Multipart upload
- Parts
- Restore 
- Torrent

The following general capabilities are not yet supported by S3ServerInterface

- Validation of chunk signatures

These items may be addressed in a future release.

## Version History

Refer to CHANGELOG.md for details.
