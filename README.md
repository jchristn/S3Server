![alt tag](https://github.com/jchristn/s3serverinterface/blob/master/assets/logo.ico)

# S3ServerInterface

Simple S3 server-side interface, produced using Amazon's public documentation.  

[![nuget](https://badge.fury.io/nu/Object.svg)](https://www.nuget.org/packages/S3ServerInterface/) 

## Feedback and Enhancements

Is there an API you'd like exposed that isn't currently?  Did you identify an issue or have other feedback?  Please file an issue here!

## New in v2.0.1.3

- S3RequestStyle and S3RequestType enumerations

## Examples

Refer to ```S3ClientTest``` and ```S3ServerTest``` projects for full examples.

## Notes

The following notes should be read prior to using S3ServerInterface:

- IP addresses are not supported for the hostname.  You *must* use ```localhost``` or a DNS FQDN.
- If you use ```*``` or ```+``` for the hostname, you *must* run under administrative privileges. 
- Bucket names *must* appear in the URL and not in the hostname, i.e. ```ForcePathStyle```.

## Server

```
using S3ServerInterface;

// Initialize the server
_Server = new S3Server("+", 8000, false, DefaultRequestHandler); // host, port, SSL

// Set callbacks
_Server.Bucket.Exists = BucketExists;
_Server.Bucket.Read = BucketRead;
_Server.Bucket.Delete = BucketDelete;
_Server.Object.Exists = ObjectExists;
_Server.Object.Read = ObjectRead;
_Server.Object.Write = ObjectWrite;
_Server.Object.Delete = ObjectDelete;
// etc

// Example callback definition
static async Task DefaultRequestHandler(S3Request req, S3Response resp)
{
	resp.StatusCode = 400;
	resp.ContentType = "text/plain";
	resp.Send("Bad request");
}

static S3Response BucketExists(S3Request req)
{
   // implement your logic here
   resp.StatusCode = 200;
   resp.Send();
}
```

## Client

Use the following example with the AWSSDK.S3 NuGet package to point your S3 client toward S3ServerInterface.
``` 
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

BasicAWSCredentials cred = new Amazon.Runtime.BasicAWSCredentials("access key", "secret key");            
AmazonS3Config config = new AmazonS3Config
{
  RegionEndpoint = RegionEndpoint.USWest1, 
  ServiceURL = "http://localhost:8000/",  
  ForcePathStyle = true,
  UseHttp = true
};
IAmazonS3 client = new AmazonS3Client(cred, config);
```

## Request and Responses

Both an ```S3Request``` and ```S3Response``` object are passed to the callback.  

S3ServerInterface expects the code you implement in your callbacks to directly set values in the supplied ```S3Response``` and trigger the end of the connection either:

- When chunk transfer-encoding is not indicated in the request, through the use of one of the ```Send()``` APIs
- When chunk transfer-encoding is indicated in the request, through the use of ```SendChunk()``` and ```SendFinalChunk()``` APIs
- Whether chunk transfer-encoding is enabled or disabled, through the use of ```Send(ErrorCode)``` API

S3ServerInterface provides a series of classes that you can use to deserialize request bodies or to serialize your responses.  These are in the namespace ```S3ServerInterface.S3Objects```.

### S3Request
```
public DateTime TimestampUtc { get; set; }
public HttpContext Http { get; set; } 
public string SourceIp { get; set; }
public int SourcePort { get; set; }
public HttpMethod Method { get; set; }
public string FullUrl { get; set; }
public string RawUrl { get; set; }
public List<string> RawUrlEntries { get; set; }
public long ContentLength { get; set; }
public string ContentType { get; set; }
public bool Chunked { get; set; }
public Dictionary<string, string> Querystring { get; set; }
public Dictionary<string, string> Headers { get; set; }
public string Region { get; set; }
public string Hostname { get; set; }
public RequestStyle Style { get; set; }
public string Bucket { get; set; }
public string Key { get; set; }
public string VersionId { get; set; }
public string Authorization { get; set; }
public string Signature { get; set; }
public string AccessKey { get; set; }
public Stream Data { get; set; }
```

### S3Response
```
public int StatusCode { get; set; }
public Dictionary<string, string> { get; set; }
public string ContentType { get; set; }
public long ContentLength { get; set; }
public Stream Data { get; set; }
public bool Chunked { get; set; }
```

### Reading Request Body Data

All request body metadata is included in the ```S3Request``` object and all payload/request body data is included in ```S3Request.Data```.

If ```S3Request.Chunked``` is **TRUE**, then request body data should be read from the ```S3Request``` object using ```S3Request.ReadChunk()```.  This method returns a ```Chunk``` object which is of the form:

```        
public int Length;
public byte[] Data;
public string Metadata;
public bool IsFinalChunk;
```

If ```S3Request.Chunked``` is **FALSE**, then the request body can be read directly from ```S3Request.Data``` using the content length specified in ```S3Request.ContentLength```.

```
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

As of v2.0.x, the following callbacks are supported:

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

These items may be addressed in a future release.

## Version History

Refer to CHANGELOG.md for details.
