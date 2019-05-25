# S3ServerInterface

Simple S3 server-side interface, produced using Amazon's public documentation.  

## Feedback and Enhancements

Is there an API you'd like exposed that isn't currently?  Did you identify an issue or have other feedback?  Please file an issue here!

## New in v1.2.x

- Default request handler (when no appropriate callback can be found) caused breaking change to constructor
- Pre-request handler (to allow you to implement your own APIs prior to attempting to match an S3 API)
- Additional constructors
- Various console debugging settings can be found in ```S3Server.ConsoleDebug.*``` 

## Examples

Refer to ```S3ClientTest``` and ```S3ServerTest``` projects for full examples.

## Notes

The following notes should be read prior to using S3ServerInterface:
- IP addresses are not supported for the hostname.  You must use ```localhost``` or a DNS FQDN.
- Chunk encoding is not supported.  It must be disabled in the client SDK.
- Bucket names must appear in the URL and not in the hostname.

## Server
```
using S3ServerInterface;

// Initialize the server
_Server = new S3Server("+", 8000, false, DefaultRequestHandler// host, port, SSL

// Set callbacks
_Server.Bucket.Exists = BucketExists;
_Server.Bucket.Read = BucketRead;
_Server.Bucket.Delete = BucketDelete;
_Server.Object.Exists = ObjectExists;
_Server.Object.Read = ObjectRead;
_Server.Object.Write = ObjectWrite;
_Server.Object.Delete = ObjectDelete;

// Example callback definition
static S3Response DefaultRequestHandler(S3Request req)
{
   // unknown API
   return new S3Response(req, 400, "text/plain", null, Encoding.UTF8.GetBytes("Unknown API"));
}

static S3Response BucketExists(S3Request req)
{
   // implement your logic here
   return new S3Response(req, 200, "text/plain", null, Encoding.UTF8.GetBytes("Hello, world!"));
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

An ```S3Request``` object is passed to the callback, and it expects an ```S3Response``` in return.  S3ServerInterface does NOT parse the request body for you, nor does it create a well-formed response.  That will be the responsibility of your code.

### S3Request
```
public HttpRequest Http;
public string SourceIp;
public int SourcePort;
public HttpMethod Method;
public string FullUrl;
public string RawUrl;
public long ContentLength;
public string ContentType;
public Dictionary<string, string> Querystring;
public Dictionary<string, string> Headers;
public string Region;
public string Hostname;
public RequestStyle Style;
public string Bucket;
public string Key;
public string Authorization;
public string Signature;
public string AccessKey;
public byte[] Data;
```

### S3Response
```
public S3Request Request;
public int StatusCode;
public Dictionary<string, string> Headers;
public string ContentType;
public long ContentLength;
public byte[] Data;
```

## Operation

S3ServerInterface parses incoming HTTP requests, extracting key pieces of information to determine the type of request sent by the caller.  The logic to handle these requests is NOT provided by S3ServerInterface; you have to create that logic yourself.  Callbacks are called when a request of that type has been received, otherwise, a 500 error is returned to the client.

Refer to https://docs.aws.amazon.com/AmazonS3/latest/API/Welcome.html for the S3 API documentation used to create this project.

As of v1.1.0, the following callbacks are supported:

### Bucket Callbacks

| Callback Name            | Description                       |
|--------------------------|-----------------------------------|
| Bucket.Exists            | Check if a bucket exists          |
| Bucket.Write             | Create a bucket                   |
| Bucket.WriteTags         | Write tags to a bucket            |
| Bucket.Read              | Enumerate a bucket                |
| Bucket.ReadTags          | Read tags on a bucket             |
| Bucket.DeleteTags        | Delete tags from a bucket         |
| Bucket.SetVersioning     | Set bucket versioning             |
| Bucket.GetVersioning     | Get bucket versioning             |

### Object Callbacks

| Callback Name            | Description                                    |
|--------------------------|------------------------------------------------|
| Object.Exists            | Check if an object exists                      |
| Object.Write             | Write an object                                |
| Object.WriteAcl          | Write an object access control list            |
| Object.WriteTags         | Write tags to an object                        |
| Object.Read              | Read an object                                 |
| Object.ReadAcl           | Read an object's access control list           |
| Object.ReadRange         | Read a range of bytes from an object           |
| Object.ReadTags          | Read an object's tags                          |
| Object.Delete            | Delete an object                               |
| Object.DeleteTags        | Delete an object's tags                        |
| Object.DeleteMultiple    | Delete multiple objects                        |

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

- Legal hold
- Multipart upload
- Parts
- Restore
- Retention
- Torrent

## Future Enhancements

The roadmap for this project includes:

- Adding more callbacks for the ones listed above as not supported / not yet available
- Adding stream support (instead of byte arrays for data in the request and response)
- Adding request parsers and response builders

## Version History

v1.1.x

- Separate callbacks for each of the various operations (breaking change)

v1.0.x

- Initial release, one request handler method
