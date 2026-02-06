![alt tag](https://github.com/jchristn/s3server/blob/master/assets/logo.ico)

# S3Server

**S3Server** is a lightweight, high-performance .NET library that provides a server-side interface for building Amazon S3-compatible storage services. It parses incoming S3 HTTP requests and routes them to your callback implementations, allowing you to focus on storage logic rather than protocol details.

[![NuGet Version](https://img.shields.io/nuget/v/S3Server.svg?style=flat)](https://www.nuget.org/packages/S3Server/) [![NuGet](https://img.shields.io/nuget/dt/S3Server.svg)](https://www.nuget.org/packages/S3Server)

## What is S3Server?

S3Server is a **protocol adapter** that handles the complexity of the Amazon S3 REST API, allowing you to build S3-compatible storage servers without dealing with HTTP parsing, XML serialization, signature validation, or AWS-specific request routing.

**What S3Server does:**
- Parses incoming S3 HTTP requests
- Determines request type (service, bucket, or object operations)
- Validates AWS Signature V4 (optional)
- Deserializes XML request bodies
- Routes requests to your callback methods
- Serializes response objects to XML
- Handles error responses with proper S3 error codes

**What S3Server does NOT do:**
- Store objects or buckets (you implement storage in your callbacks)
- Provide authentication or authorization logic (you control access in your callbacks)
- Manage metadata persistence (you handle metadata storage)

Want a complete S3-compatible storage server built using S3Server? Check out **[Less3](https://github.com/jchristn/less3)**.

## Why Use S3Server?

- **S3 API Compatibility**: Build services that work with existing S3 clients (AWS SDK, CLI, MinIO client, etc.)
- **Focus on Storage Logic**: Spend your time implementing storage, not parsing HTTP requests
- **Flexible Architecture**: Complete control over where and how you store data
- **Multi-Framework Support**: Targets .NET Standard 2.0, 2.1, .NET 6.0, and .NET 8.0
- **Production Ready**: Handles path-style and virtual-hosted-style URLs, signature validation, multipart uploads, and more

## Use Cases

- **Custom S3-compatible storage backends**: Build object storage on top of databases, file systems, cloud storage, or distributed systems
- **S3 gateway services**: Create proxies or gateways that translate S3 requests to other storage protocols
- **Testing and development**: Build mock S3 servers for testing applications without AWS dependencies
- **Compliance and data residency**: Keep complete control over data location and access patterns
- **Feature extension**: Add custom logic, caching, encryption, or auditing to S3 operations
- **Cost optimization**: Implement tiered storage or custom retention policies
- **Air-gapped environments**: Deploy S3-compatible storage in isolated networks

## Features

✅ **Complete S3 API Coverage**
- Service operations (list buckets, check service)
- Bucket operations (CRUD, ACLs, tags, versioning, website config, logging, location)
- Object operations (CRUD, ACLs, tags, legal hold, retention, range reads)
- Multipart upload support (initiate, upload parts, complete, abort, list parts)
- S3 Select API support

✅ **URL Style Support**
- Path-style URLs: `http://host:port/bucket/key` (default)
- Virtual-hosted-style URLs: `http://bucket.domain/key` (configurable)

✅ **Security & Validation**
- AWS Signature V4 validation (optional)
- Pre-request hooks for authentication
- Post-request hooks for logging and metrics

✅ **Developer Friendly**
- Strongly-typed request/response objects
- Comprehensive error handling with S3-compliant error codes
- Detailed logging support
- Configurable operation limits

## Quick Start

### Installation

```bash
dotnet add package S3Server
```

### Basic Example

```csharp
using S3ServerLibrary;
using S3ServerLibrary.S3Objects;

namespace S3ServerLibrary
{
    using System;
    using System.Threading.Tasks;
    using WatsonWebserver.Core;

    // Configure server settings
    S3ServerSettings settings = new S3ServerSettings();
    settings.Webserver = new WebserverSettings("localhost", 8000, false);
    settings.Logger = Console.WriteLine;

    // Create and configure server
    S3Server server = new S3Server(settings);

    // Wire up callbacks
    server.Service.ListBuckets = async (ctx) =>
    {
        ListAllMyBucketsResult result = new ListAllMyBucketsResult();
        result.Owner = new Owner("admin", "Administrator");
        result.Buckets = new Buckets(new List<Bucket>
        {
            new Bucket("my-bucket", DateTime.UtcNow)
        });
        return result;
    };

    server.Bucket.Exists = async (ctx) =>
    {
        // Check if bucket exists in your storage
        return true;
    };

    server.Object.Write = async (ctx) =>
    {
        // Save object data from ctx.Request.Data stream
        Console.WriteLine($"Writing object: {ctx.Request.Bucket}/{ctx.Request.Key}");
        Console.WriteLine($"Content length: {ctx.Request.ContentLength}");
        // Implement your storage logic here
    };

    server.Object.Read = async (ctx) =>
    {
        // Retrieve object from your storage
        byte[] data = System.Text.Encoding.UTF8.GetBytes("Hello, S3!");
        return new S3Object(
            ctx.Request.Key,
            "version-1",
            true,
            DateTime.UtcNow,
            "etag-123",
            data.Length,
            new Owner("admin", "Administrator"),
            data,
            "text/plain",
            StorageClassEnum.STANDARD
        );
    };

    // Start server
    server.Start();
    Console.WriteLine("S3 Server listening on http://localhost:8000");
}
```

## Configuration

### Server Settings

```csharp
S3ServerSettings settings = new S3ServerSettings
{
    // Required: Webserver configuration
    Webserver = new WebserverSettings("localhost", 8000, false),

    // Optional: Logger for diagnostic output
    Logger = (msg) => Console.WriteLine(msg),

    // Optional: Enable specific logging categories
    Logging = new LoggingSettings
    {
        HttpRequests = true,
        S3Requests = true,
        SignatureV4Validation = false
    },

    // Optional: Operation limits
    OperationLimits = new OperationLimitsSettings
    {
        MaxPutObjectSize = 5368709120 // 5GB default
    },

    // Optional: Enable AWS Signature V4 validation
    EnableSignatures = false,

    // Optional: Use TCP-based server (WatsonWebserver.Lite) instead of http.sys (WatsonWebserver)
    UseTcpServer = false
};
```

### Request Handlers

S3Server provides hooks to intercept requests at different stages:

```csharp
// Pre-request handler (auth, logging, validation)
// Return true to terminate request, false to continue routing
settings.PreRequestHandler = async (ctx) =>
{
    // Check authentication
    if (!IsAuthenticated(ctx))
    {
        ctx.Response.StatusCode = 403;
        await ctx.Response.Send(ErrorCode.AccessDenied);
        return true; // Terminate
    }

    // Add custom metadata for downstream callbacks
    ctx.Metadata = new { UserId = "user123" };

    return false; // Continue to callback routing
};

// Default request handler (called when no callback matches)
settings.DefaultRequestHandler = async (ctx) =>
{
    Console.WriteLine($"Unhandled request: {ctx.Request.RequestType}");
    await ctx.Response.Send(ErrorCode.InvalidRequest);
};

// Post-request handler (logging, metrics)
settings.PostRequestHandler = async (ctx) =>
{
    Console.WriteLine($"Completed: {ctx.Request.RequestType} - {ctx.Response.StatusCode}");
    // Log metrics, update statistics, etc.
};
```

### AWS Signature Validation

Enable AWS Signature V4 validation for authenticated requests:

```csharp
settings.EnableSignatures = true;
settings.Logging.SignatureV4Validation = true; // Optional debug logging

// Implement callback to retrieve secret key for access key
server.Service.GetSecretKey = (ctx) =>
{
    string accessKey = ctx.Request.AccessKey;

    // Look up secret key for this access key
    // Return base64-encoded secret key
    return "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY";

    // Or throw exception if access key is invalid
    // throw new S3Exception(new Error(ErrorCode.InvalidAccessKeyId));
};
```

**Note:** Only AWS Signature V4 is supported. V2 signatures will return an error. Chunk signature validation is not yet supported.

### Virtual-Hosted-Style URLs

Support bucket names in hostnames (`http://bucket.s3.local/key` instead of `http://s3.local/bucket/key`):

```csharp
// 1. Use wildcard listener (requires admin privileges on Windows)
settings.Webserver.Hostname = "*"; // or "+" or "0.0.0.0"

// 2. Implement base domain finder
server.Service.FindMatchingBaseDomain = (hostname) =>
{
    // Input: "mybucket.s3.local.gd"
    // Output: "s3.local.gd" (the base domain)

    if (hostname.EndsWith(".s3.local.gd"))
        return "s3.local.gd";

    if (hostname.EndsWith(".s3.example.com"))
        return "s3.example.com";

    // No match found - will be treated as path-style
    throw new KeyNotFoundException($"No base domain for {hostname}");
};
```

**DNS Configuration:**
- Configure DNS or `hosts` file to resolve bucket subdomains
- For local testing: `*.local.gd` and `*.fbi.com` resolve to localhost
- Example: `mybucket.s3.local.gd` → `127.0.0.1`

## Callback Implementation Patterns

### Pattern 1: Return Typed Result

```csharp
server.Bucket.ReadAcl = async (ctx) =>
{
    AccessControlList acl = new AccessControlList(
        new List<Grant>
        {
            new Grant(
                new Grantee("admin", "Administrator", null, "CanonicalUser", "admin@example.com"),
                PermissionEnum.FullControl
            )
        }
    );

    return new AccessControlPolicy(
        new Owner("admin", "Administrator"),
        acl
    );
};
```

### Pattern 2: Return Void (No Response Body)

```csharp
server.Bucket.Delete = async (ctx) =>
{
    string bucketName = ctx.Request.Bucket;

    // Delete bucket from your storage
    DeleteBucketFromStorage(bucketName);

    // Return normally - S3Server sends 204 No Content
    return;
};
```

### Pattern 3: Throw S3Exception on Error

```csharp
server.Bucket.Exists = async (ctx) =>
{
    if (!BucketExistsInStorage(ctx.Request.Bucket))
    {
        throw new S3Exception(new Error(ErrorCode.NoSuchBucket));
    }

    return true;
};
```

### Pattern 4: Using Metadata for Context

```csharp
// Set metadata in PreRequestHandler
settings.PreRequestHandler = async (ctx) =>
{
    ctx.Metadata = new { TenantId = GetTenantFromAuth(ctx) };
    return false;
};

// Access metadata in callbacks
server.Object.Write = async (ctx) =>
{
    dynamic metadata = ctx.Metadata;
    string tenantId = metadata.TenantId;

    // Use tenant context for multi-tenant storage
    SaveObject(tenantId, ctx.Request.Bucket, ctx.Request.Key, ctx.Request.Data);
};
```

## Complete API Reference

### Service Callbacks

| Callback | Description | Method | URL | Response Type |
|----------|-------------|--------|-----|---------------|
| `Service.ListBuckets` | List all buckets | GET | `/` | `ListAllMyBucketsResult` |
| `Service.ServiceExists` | Check service and return region | HEAD | `/` | `string` (region) |
| `Service.FindMatchingBaseDomain` | Find base domain for virtual hosting | N/A | N/A | `string` (base domain) |
| `Service.GetSecretKey` | Get secret key for access key (auth) | N/A | N/A | `string` (secret key) |

### Bucket Callbacks

| Callback | Description | Method | URL | Response Type |
|----------|-------------|--------|-----|---------------|
| `Bucket.Write` | Create a bucket | PUT | `/[bucket]` | `void` |
| `Bucket.Read` | List objects in bucket | GET | `/[bucket]` | `ListBucketResult` |
| `Bucket.Exists` | Check if bucket exists | HEAD | `/[bucket]` | `bool` |
| `Bucket.Delete` | Delete a bucket | DELETE | `/[bucket]` | `void` |
| `Bucket.ReadAcl` | Read bucket ACL | GET | `/[bucket]?acl` | `AccessControlPolicy` |
| `Bucket.WriteAcl` | Write bucket ACL | PUT | `/[bucket]?acl` | `void` |
| `Bucket.DeleteAcl` | Delete bucket ACL | DELETE | `/[bucket]?acl` | `void` |
| `Bucket.ReadLocation` | Get bucket region | GET | `/[bucket]?location` | `LocationConstraint` |
| `Bucket.ReadLogging` | Get logging config | GET | `/[bucket]?logging` | `BucketLoggingStatus` |
| `Bucket.WriteLogging` | Set logging config | PUT | `/[bucket]?logging` | `void` |
| `Bucket.ReadTagging` | Get bucket tags | GET | `/[bucket]?tagging` | `Tagging` |
| `Bucket.WriteTagging` | Set bucket tags | PUT | `/[bucket]?tagging` | `void` |
| `Bucket.DeleteTagging` | Delete bucket tags | DELETE | `/[bucket]?tagging` | `void` |
| `Bucket.ReadVersioning` | Get versioning config | GET | `/[bucket]?versioning` | `VersioningConfiguration` |
| `Bucket.WriteVersioning` | Set versioning config | PUT | `/[bucket]?versioning` | `void` |
| `Bucket.ReadVersions` | List object versions | GET | `/[bucket]?versions` | `ListVersionsResult` |
| `Bucket.ReadWebsite` | Get website config | GET | `/[bucket]?website` | `WebsiteConfiguration` |
| `Bucket.WriteWebsite` | Set website config | PUT | `/[bucket]?website` | `void` |
| `Bucket.DeleteWebsite` | Delete website config | DELETE | `/[bucket]?website` | `void` |
| `Bucket.ReadMultipartUploads` | List multipart uploads | GET | `/[bucket]?uploads` | `ListMultipartUploadsResult` |

### Object Callbacks

| Callback | Description | Method | URL | Response Type |
|----------|-------------|--------|-----|---------------|
| `Object.Write` | Upload object | PUT | `/[bucket]/[key]` | `void` |
| `Object.Read` | Download object | GET | `/[bucket]/[key]` | `S3Object` |
| `Object.Exists` | Check if object exists | HEAD | `/[bucket]/[key]` | `ObjectMetadata` |
| `Object.Delete` | Delete object | DELETE | `/[bucket]/[key]` | `void` |
| `Object.ReadRange` | Download byte range | GET | `/[bucket]/[key]`* | `S3Object` |
| `Object.ReadAcl` | Get object ACL | GET | `/[bucket]/[key]?acl` | `AccessControlPolicy` |
| `Object.WriteAcl` | Set object ACL | PUT | `/[bucket]/[key]?acl` | `void` |
| `Object.DeleteAcl` | Delete object ACL | DELETE | `/[bucket]/[key]?acl` | `void` |
| `Object.ReadTagging` | Get object tags | GET | `/[bucket]/[key]?tagging` | `Tagging` |
| `Object.WriteTagging` | Set object tags | PUT | `/[bucket]/[key]?tagging` | `void` |
| `Object.DeleteTagging` | Delete object tags | DELETE | `/[bucket]/[key]?tagging` | `void` |
| `Object.ReadLegalHold` | Get legal hold status | GET | `/[bucket]/[key]?legal-hold` | `LegalHold` |
| `Object.WriteLegalHold` | Set legal hold status | PUT | `/[bucket]/[key]?legal-hold` | `void` |
| `Object.ReadRetention` | Get retention status | GET | `/[bucket]/[key]?retention` | `Retention` |
| `Object.WriteRetention` | Set retention status | PUT | `/[bucket]/[key]?retention` | `void` |
| `Object.DeleteMultiple` | Delete multiple objects | POST | `/[bucket]?delete` | `DeleteResult` |
| `Object.SelectContent` | S3 Select query | POST | `/[bucket]/[key]?select&select-type=2` | `void` |

\* `ReadRange` is triggered when Range header is present

### Multipart Upload Callbacks

| Callback | Description | Method | URL | Response Type |
|----------|-------------|--------|-----|---------------|
| `Object.CreateMultipartUpload` | Initiate multipart upload | POST | `/[bucket]/[key]?uploads` | `InitiateMultipartUploadResult` |
| `Object.UploadPart` | Upload a part | PUT | `/[bucket]/[key]?partNumber=N&uploadId=ID` | `void` |
| `Object.ReadParts` | List uploaded parts | GET | `/[bucket]/[key]?uploadId=ID` | `ListPartsResult` |
| `Object.CompleteMultipartUpload` | Complete upload | POST | `/[bucket]/[key]?uploadId=ID` | `CompleteMultipartUploadResult` |
| `Object.AbortMultipartUpload` | Abort upload | DELETE | `/[bucket]/[key]?uploadId=ID` | `void` |

## Request and Response Objects

### S3Context

The `S3Context` object is passed to all callbacks:

```csharp
public class S3Context
{
    // Parsed S3 request details
    public S3Request Request { get; }

    // Response builder
    public S3Response Response { get; }

    // Underlying HTTP context (WatsonWebserver)
    public HttpContextBase Http { get; }

    // User-defined metadata (set in PreRequestHandler)
    public object Metadata { get; set; }

    // Timestamp information
    public Timestamp Timestamp { get; }
}
```

### S3Request

Key properties available in `S3Context.Request`:

```csharp
// Request identification
string RequestId              // Unique request ID
string TraceId                // Trace ID for debugging

// Request type and style
S3RequestType RequestType     // Enum: ServiceExists, BucketWrite, ObjectRead, etc.
S3RequestStyle RequestStyle   // PathStyle or VirtualHostedStyle

// S3 resource identifiers
string Bucket                 // Bucket name
string Key                    // Object key
string VersionId              // Version ID (if versioning enabled)

// Authentication
string AccessKey              // AWS access key
string Signature              // Request signature
S3SignatureVersion SignatureVersion // Version2, Version4, or Unknown

// Content details
long ContentLength            // Request body size
string ContentType            // Content type
Stream Data                   // Request body stream
string DataAsString           // Request body as string (fully reads stream)
byte[] DataAsBytes            // Request body as bytes (fully reads stream)

// Range requests
long? RangeStart              // Start byte for range request
long? RangeEnd                // End byte for range request

// Listing parameters
int MaxKeys                   // Maximum keys to return (default 1000)
string Prefix                 // Object key prefix filter
string Delimiter              // Delimiter for grouping
string Marker                 // Pagination marker
string ContinuationToken      // Continuation token for v2 listing

// Multipart upload
string UploadId               // Multipart upload ID
int PartNumber                // Part number for multipart upload
int MaxParts                  // Maximum parts to return

// Permissions
S3PermissionType PermissionsRequired // Permission needed for this operation

// Helper methods
bool HeaderExists(string key)
bool QuerystringExists(string key)
string RetrieveHeaderValue(string key)
string RetrieveQueryValue(string key)
Task<Chunk> ReadChunk()       // Read chunk for chunked transfer encoding
```

### S3Response

Methods for sending responses:

```csharp
// Send empty response
await ctx.Response.Send();

// Send string response
await ctx.Response.Send("response data");

// Send byte array response
await ctx.Response.Send(bytes);

// Send stream response
await ctx.Response.Send(contentLength, stream);

// Send error response
await ctx.Response.Send(ErrorCode.NoSuchBucket);
await ctx.Response.Send(new Error(ErrorCode.AccessDenied));

// Chunked transfer encoding
await ctx.Response.SendChunk(chunkData, isFinal);

// Set response properties before sending
ctx.Response.StatusCode = 200;
ctx.Response.ContentType = "application/json";
ctx.Response.ContentLength = data.Length;
ctx.Response.Headers.Add("X-Custom-Header", "value");
```

## Error Handling

S3Server provides comprehensive error handling with S3-compliant error codes:

```csharp
server.Object.Read = async (ctx) =>
{
    if (!ObjectExists(ctx.Request.Key))
    {
        throw new S3Exception(new Error(ErrorCode.NoSuchKey));
    }

    if (!HasPermission(ctx, ctx.Request.Key))
    {
        throw new S3Exception(new Error(ErrorCode.AccessDenied));
    }

    // ... return object
};
```

Common error codes:
- `ErrorCode.NoSuchBucket` - 404
- `ErrorCode.NoSuchKey` - 404
- `ErrorCode.AccessDenied` - 403
- `ErrorCode.BucketAlreadyExists` - 409
- `ErrorCode.BucketNotEmpty` - 409
- `ErrorCode.EntityTooLarge` - 400
- `ErrorCode.InvalidBucketName` - 400
- `ErrorCode.InternalError` - 500
- `ErrorCode.SignatureDoesNotMatch` - 403

See `S3Objects/ErrorCode.cs` for the complete list of 60+ error codes.

## Client Configuration

Use the AWS SDK to connect to your S3Server instance:

```csharp
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

BasicAWSCredentials cred = new BasicAWSCredentials("access-key", "secret-key");

AmazonS3Config config = new AmazonS3Config
{
    ServiceURL = "http://localhost:8000/",

    // Use path-style URLs (bucket in path, not hostname)
    ForcePathStyle = true,

    // Or use virtual-hosted-style URLs
    // ForcePathStyle = false,

    UseHttp = true,

    // Optional: Set region
    AuthenticationRegion = "us-west-1"
};

IAmazonS3 client = new AmazonS3Client(cred, config);

// Use client
await client.PutBucketAsync("my-bucket");
await client.PutObjectAsync(new PutObjectRequest
{
    BucketName = "my-bucket",
    Key = "test.txt",
    ContentBody = "Hello, S3!"
});
```

## Advanced Topics

### Chunked Transfer Encoding

Handle chunked uploads (e.g., AWS CLI streaming uploads):

```csharp
server.Object.Write = async (ctx) =>
{
    if (ctx.Request.Chunked)
    {
        List<byte[]> chunks = new List<byte[]>();

        while (true)
        {
            Chunk chunk = await ctx.Request.ReadChunk();

            if (chunk.Length > 0)
            {
                chunks.Add(chunk.Data);
            }

            if (chunk.IsFinal)
                break;
        }

        byte[] completeData = CombineChunks(chunks);
        SaveObject(ctx.Request.Bucket, ctx.Request.Key, completeData);
    }
    else
    {
        // Non-chunked upload
        SaveObject(ctx.Request.Bucket, ctx.Request.Key, ctx.Request.DataAsBytes);
    }
};
```

### Multipart Upload Implementation

```csharp
// 1. Initiate
server.Object.CreateMultipartUpload = async (ctx) =>
{
    string uploadId = Guid.NewGuid().ToString();

    // Store upload metadata
    StoreUploadMetadata(ctx.Request.Bucket, ctx.Request.Key, uploadId);

    return new InitiateMultipartUploadResult(
        ctx.Request.Bucket,
        ctx.Request.Key,
        uploadId
    );
};

// 2. Upload parts
server.Object.UploadPart = async (ctx) =>
{
    string uploadId = ctx.Request.UploadId;
    int partNumber = ctx.Request.PartNumber;

    // Store part data
    StorePart(uploadId, partNumber, ctx.Request.DataAsBytes);

    // Set ETag header for part
    string etag = CalculateETag(ctx.Request.DataAsBytes);
    ctx.Response.Headers.Add("ETag", etag);
};

// 3. Complete
server.Object.CompleteMultipartUpload = async (ctx, request) =>
{
    // Combine parts in order
    byte[] finalData = CombineParts(ctx.Request.UploadId, request.Parts);

    // Save final object
    SaveObject(ctx.Request.Bucket, ctx.Request.Key, finalData);

    // Clean up parts
    CleanupUpload(ctx.Request.UploadId);

    return new CompleteMultipartUploadResult
    {
        Location = $"http://localhost:8000/{ctx.Request.Bucket}/{ctx.Request.Key}",
        Bucket = ctx.Request.Bucket,
        Key = ctx.Request.Key,
        ETag = CalculateETag(finalData)
    };
};
```

### Operation Limits

Control maximum upload sizes:

```csharp
settings.OperationLimits = new OperationLimitsSettings
{
    // Maximum size for single PutObject (default 5GB)
    MaxPutObjectSize = 5368709120
};
```

When exceeded, S3Server automatically returns `EntityTooLarge` error.

**Note:** Multipart upload parts are not subject to this limit individually.

## Known Limitations

- **Chunk signature validation**: Not yet supported for chunked transfer encoding with AWS Signature V4
- **AWS Signature V2**: Not supported (returns error)

The following S3 operations are not exposed through callbacks (may be added in future releases):

**Bucket operations:**
- Accelerate, Analytics, CORS, Encryption, Inventory, Lifecycle, Notification, Object lock configuration, Policy status, Public access block, Metrics, Payment, Policy, Replication

**Object operations:**
- Torrent, Restore (Glacier)

## Examples and Testing

Comprehensive examples are available in the repository:

- **`Test.Server`**: Complete server implementation with all callbacks
- **`Test.Client`**: S3 client examples using AWS SDK
- **`Test.RequestStyle`**: Path-style vs virtual-hosted-style URL testing
- **`Test.SignatureValidation`**: AWS Signature V4 validation examples
- **`Test.Automated`**: Automated test suite (runs against both HTTP and TCP server modes)

Run the test server (requires admin on Windows for wildcard listeners):

```bash
dotnet run --project src/Test.Server/Test.Server.csproj
```

## Building from Source

```bash
# Build solution
dotnet build src/S3Server.sln

# Build specific configuration
dotnet build src/S3Server.sln -c Release

# Pack NuGet package
dotnet pack src/S3Server/S3Server.csproj -c Release
```

## Dependencies

- **WatsonWebserver** (6.3.13): HTTP server framework
- **AWSSignatureGenerator** (1.0.9): AWS Signature V4 validation
- **PrettyId** (1.0.5): Request ID generation
- **Timestamps** (1.0.1): Timestamp tracking

## Resources

- **Documentation**: [Amazon S3 REST API Reference](https://docs.aws.amazon.com/AmazonS3/latest/API/Welcome.html)
- **Complete Example**: [Less3 - Production S3 Server](https://github.com/jchristn/less3)
- **NuGet Package**: [S3Server on NuGet](https://www.nuget.org/packages/S3Server/)

## Feedback and Enhancements

Have a feature request or found an issue? Please [file an issue on GitHub](https://github.com/jchristn/s3server/issues)!

## Version History

Refer to [CHANGELOG.md](CHANGELOG.md) for version history and release notes.

## New in v6.0.x

- Breaking changes with dependency updates
- Moved usings inside namespaces to reduce collisions
- Moved from `new byte[0]` to `Array.Empty<byte>()`
- Size limits for `ObjectWrite` (e.g. `PutObject`), returns `EntityTooLarge` if exceeded
- Boolean for enabling or disabling signature validation
- **Added multipart upload support** (CreateMultipartUpload, UploadPart, CompleteMultipartUpload, AbortMultipartUpload, ReadParts)
- **Added S3 Select API support** (SelectContent)
- Added BucketReadMultipartUploads callback
- Added ObjectDeleteAcl callback
- Added `UseTcpServer` setting: when true, uses WatsonWebserver.Lite (TCP-based) instead of WatsonWebserver (http.sys-based)

## License

MIT License - see LICENSE file for details
