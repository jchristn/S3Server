![alt tag](https://github.com/jchristn/s3server/blob/main/assets/logo.ico)

# S3Server

**S3Server** is a lightweight, high-performance .NET library that provides a server-side interface for building Amazon S3-compatible storage services. It parses incoming S3 HTTP requests and routes them to your callback implementations, allowing you to focus on storage logic rather than protocol details.

[![NuGet Version](https://img.shields.io/nuget/v/S3Server.svg?style=flat)](https://www.nuget.org/packages/S3Server/) [![NuGet](https://img.shields.io/nuget/dt/S3Server.svg)](https://www.nuget.org/packages/S3Server)

## What is S3Server?

S3Server is a **protocol adapter** that handles the complexity of the Amazon S3 REST API, allowing you to build S3-compatible storage servers without dealing with HTTP parsing, XML serialization, signature validation, or AWS-specific request routing.

**What S3Server does:**
- Parses incoming S3 HTTP requests
- Determines request type (service, bucket, or object operations)
- Validates AWS Signature V4 and optional legacy Signature V2
- Supports opt-in anonymous public access decisions
- Deserializes XML request bodies
- Routes requests to your callback methods
- Serializes response objects to XML
- Emits archive restore headers from object restore state
- Handles error responses with proper S3 error codes

**What S3Server does NOT do:**
- Store objects or buckets (you implement storage in your callbacks)
- Store users, credentials, ACLs, or policies (you control access data in your application)
- Manage metadata persistence (you handle metadata storage)

Want a complete S3-compatible storage server built using S3Server? Check out **[Less3](https://github.com/jchristn/less3)**.

## Why Use S3Server?

- **S3 API Compatibility**: Build services that work with existing S3 clients (AWS SDK, CLI, MinIO client, etc.)
- **Focus on Storage Logic**: Spend your time implementing storage, not parsing HTTP requests
- **Flexible Architecture**: Complete control over where and how you store data
- **Multi-Framework Support**: Targets .NET 8.0 and .NET 10.0
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
- Object operations (CRUD, ACLs, tags, legal hold, retention, range reads, archive restore)
- Multipart upload support (initiate, upload parts, complete, abort, list parts)
- S3 Select API support

✅ **URL Style Support**
- Path-style URLs: `http://host:port/bucket/key` (default)
- Virtual-hosted-style URLs: `http://bucket.domain/key` (configurable)

✅ **Security & Validation**
- AWS Signature V4 validation, plus opt-in legacy Signature V2 validation
- Opt-in anonymous authorization hook for public-read/public-write compatibility
- Pre-request hooks for custom validation
- Post-request hooks for logging and metrics

✅ **Observability**
- OpenTelemetry-shaped metrics and traces on a `Meter` and `ActivitySource` named `S3Server`, with no exporter dependency
- Per-operation request rate, latency, and errors; per-stage timing (signature validation, your storage callback, XML serialization, send)
- Callback, signature-validation, and error-type metrics, plus one span per request with stage and callback child spans nested under Watson's HTTP span
- Ready-to-import Grafana dashboard in `assets/grafana/s3server.json`; see [TELEMETRY.md](TELEMETRY.md)

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

    // Optional: Enable legacy AWS Signature V2 validation when signatures are enabled
    EnableSignatureV2 = false,

    // Optional: Validate signatures before PreRequestHandler runs (default false)
    ValidateSignaturesBeforePreRequestHandler = false,

    // Optional: Route suffix ranges (bytes=-N) to Object.ReadRange (default true)
    RouteSuffixRangesToReadRange = true,

    // Optional: Keep the webserver's default Host, Accept, Accept-Language, Accept-Charset,
    // Cache-Control, and CORS response headers on every response (default false, which removes them)
    PreserveWebserverDefaultHeaders = false,

    // Optional: Send the webserver's CORS (Access-Control-*) headers on responses to requests
    // that carry an Origin header, for browser-based clients (default false, as Amazon S3 does
    // for a bucket without a CORS configuration)
    EmitCorsHeaders = false,

    // Note: UseTcpServer is deprecated in v7.0; Watson now uses TCP natively
    UseTcpServer = false
};

// Optional: Telemetry (on by default; nothing is exported until your host subscribes)
settings.Telemetry.Enable = true;
settings.Telemetry.IncludeObjectKeys = false; // object keys on spans are opt-in
```

### Telemetry

S3Server emits metrics and traces through a `System.Diagnostics.Metrics.Meter` and an `ActivitySource`, both named `S3Server`. Watson, the underlying webserver, emits the HTTP layer under the name `Watson`. Subscribe your collector to both. With [Radiant](https://www.nuget.org/packages/Radiant):

```csharp
RadiantSettings telemetry = new RadiantSettings("my-s3-service");
telemetry.Sources.AddMeter("Watson");
telemetry.Sources.AddActivitySource("Watson");
telemetry.Sources.AddMeter("S3Server");
telemetry.Sources.AddActivitySource("S3Server");

using (RadiantHost host = RadiantHost.Start(telemetry))
using (S3Server server = new S3Server(settings))
{
    server.Start();
    // ...
}
```

Or with the OpenTelemetry SDK: `.AddMeter("Watson", "S3Server")` and `.AddSource("Watson", "S3Server")`. Key metrics include `s3server.requests`, `s3server.request.duration`, `s3server.stage.duration`, `s3server.callback.duration`, `s3server.signature.validations`, and `s3server.errors`. Every request gets an `S3 {operation}` span with `stage:*` and `callback *` children, so a slow request resolves to the stage or storage callback that caused it. See [TELEMETRY.md](TELEMETRY.md) for the full metrics and spans catalog, configuration, histogram buckets, recommended alerts, and the Grafana dashboard in `assets/grafana/s3server.json`.

### Request Handlers

S3Server provides hooks to intercept requests at different stages. The order is:

```
PreRequestHandler -> signature validation -> AuthenticatedRequestHandler -> operation callback -> PostRequestHandler
```

With `ValidateSignaturesBeforePreRequestHandler = true`, signature validation moves ahead of `PreRequestHandler`.

> **Security:** with the default order, `PreRequestHandler` runs **before** signature validation and sees unauthenticated input. Use it for logging, metrics, or populating `ctx.Metadata` for `Service.GetSecretKey`. Do not answer, modify, or disclose data from it. Anything that answers requests belongs in `AuthenticatedRequestHandler`, which only runs for requests that passed signature validation (or were allowed by `Service.IsAnonymousRequestAllowed`).

```csharp
// Pre-request handler (logging, metadata for GetSecretKey)
// Sees unauthenticated input under the default order.
// Return true to terminate request, false to continue routing
settings.PreRequestHandler = async (ctx) =>
{
    // Add custom metadata for downstream callbacks
    ctx.Metadata = new { TenantId = LookupTenant(ctx.Request.AccessKey) };

    return false; // Continue to signature validation and routing
};

// Authenticated request handler (authorization, request interception)
// Only invoked after signature validation succeeds.
// Return true to terminate request, false to continue routing
settings.AuthenticatedRequestHandler = async (ctx) =>
{
    if (!IsAuthorized(ctx))
    {
        await ctx.Response.Send(ErrorCode.AccessDenied);
        return true; // Terminate
    }

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

Enable AWS signature validation for authenticated requests. Signature V4 is enabled by `EnableSignatures`; legacy Signature V2 remains disabled unless `EnableSignatureV2` is also set.

```csharp
settings.EnableSignatures = true;
settings.EnableSignatureV2 = false; // Set true only for legacy S3-compatible clients
settings.Logging.SignatureV4Validation = true; // Optional debug logging

// Implement callback to retrieve secret key for access key
server.Service.GetSecretKey = (ctx) =>
{
    string accessKey = ctx.Request.AccessKey;

    // Look up and return the secret key for this access key
    return "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY";

    // Or throw exception if access key is invalid
    // throw new S3Exception(new Error(ErrorCode.InvalidAccessKeyId));
};
```

When `EnableSignatureV2` is `true`, S3Server validates legacy V2 header signatures such as `Authorization: AWS AWSAccessKeyId:Signature` and V2 signed URLs containing `AWSAccessKeyId`, `Expires`, and `Signature`. Expired V2 signed URLs are rejected. V2 support is intended for compatibility with older S3 clients and S3-compatible storage ecosystems; AWS has deprecated Signature V2, so leave it disabled unless you need that legacy behavior.

### Anonymous Public Access

When signature validation is enabled, unsigned requests are rejected by default. To support S3-compatible public-read or public-write behavior, set `Service.IsAnonymousRequestAllowed`. S3Server calls this callback only for requests that do not include an `Authorization` header, V2 signed URL parameters, or recognized V4 presigned URL material. Requests that include signature material still validate normally and fail closed if the signature is invalid.

```csharp
settings.EnableSignatures = true;

server.Service.GetSecretKey = (ctx) =>
{
    return LookupSecretKey(ctx.Request.AccessKey);
};

server.Service.IsAnonymousRequestAllowed = async (ctx) =>
{
    // Example: consult your bucket/object ACL store for the AllUsers group.
    switch (ctx.Request.PermissionsRequired)
    {
        case S3PermissionType.ObjectRead:
        case S3PermissionType.BucketRead:
            return await GrantsPublicRead(ctx.Request.Bucket, ctx.Request.Key);

        case S3PermissionType.BucketWrite:
            return await GrantsPublicWrite(ctx.Request.Bucket);

        default:
            return false;
    }
};
```

Keep the callback narrow. For example, a public-read object should allow `ObjectRead`, not ACL writes or administrative operations. A request carrying a bad signature should remain rejected rather than falling back to anonymous access.

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
| `Service.IsAnonymousRequestAllowed` | Allow selected unsigned anonymous requests | N/A | N/A | `bool` |

### Bucket Callbacks

| Callback | Description | Method | URL | Response Type |
|----------|-------------|--------|-----|---------------|
| `Bucket.Write` | Create a bucket | PUT | `/[bucket]` | `void` |
| `Bucket.Read` | List objects in bucket | GET | `/[bucket]` | `ListBucketResult` |
| `Bucket.Exists` | Check if bucket exists | HEAD | `/[bucket]` | `bool` |
| `Bucket.Delete` | Delete a bucket | DELETE | `/[bucket]` | `void` |
| `Bucket.ReadAcl` | Read bucket ACL | GET | `/[bucket]?acl` | `AccessControlPolicy` |
| `Bucket.WriteAcl` | Write bucket ACL (policy is `null` for canned ACLs) | PUT | `/[bucket]?acl` | `void` |
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
| `Object.Copy` | Copy object | PUT | `/[bucket]/[key]` with `x-amz-copy-source` | `CopyObjectResult` |
| `Object.Read` | Download object | GET | `/[bucket]/[key]` | `S3Object` |
| `Object.Exists` | Check if object exists | HEAD | `/[bucket]/[key]` | `ObjectMetadata` |
| `Object.Delete` | Delete object | DELETE | `/[bucket]/[key]` | `void` |
| `Object.ReadRange` | Download byte range | GET | `/[bucket]/[key]`* | `S3Object` |
| `Object.ReadAcl` | Get object ACL | GET | `/[bucket]/[key]?acl` | `AccessControlPolicy` |
| `Object.WriteAcl` | Set object ACL (policy is `null` for canned ACLs) | PUT | `/[bucket]/[key]?acl` | `void` |
| `Object.DeleteAcl` | Delete object ACL | DELETE | `/[bucket]/[key]?acl` | `void` |
| `Object.ReadTagging` | Get object tags | GET | `/[bucket]/[key]?tagging` | `Tagging` |
| `Object.WriteTagging` | Set object tags | PUT | `/[bucket]/[key]?tagging` | `void` |
| `Object.DeleteTagging` | Delete object tags | DELETE | `/[bucket]/[key]?tagging` | `void` |
| `Object.ReadLegalHold` | Get legal hold status | GET | `/[bucket]/[key]?legal-hold` | `LegalHold` |
| `Object.WriteLegalHold` | Set legal hold status | PUT | `/[bucket]/[key]?legal-hold` | `void` |
| `Object.ReadRetention` | Get retention status | GET | `/[bucket]/[key]?retention` | `Retention` |
| `Object.WriteRetention` | Set retention status | PUT | `/[bucket]/[key]?retention` | `void` |
| `Object.DeleteMultiple` | Delete multiple objects | POST | `/[bucket]?delete` | `DeleteResult` |
| `Object.Restore` | Restore archived object | POST | `/[bucket]/[key]?restore` | `RestoreObjectResult` |
| `Object.SelectContent` | S3 Select query | POST | `/[bucket]/[key]?select&select-type=2` | `void` |

\* `ReadRange` is triggered when a valid single-range `Range` header is present. A `Range` header that cannot be parsed (multiple ranges, a unit other than `bytes`, non-numeric bounds, or `last < first`) is ignored, as Amazon S3 does, and the request is served by `Object.Read` with `200`.

In a `ReadRange` callback, set `S3Object.Size` to the number of bytes in the returned range and `S3Object.TotalSize` to the full object size. S3Server then emits `Content-Range: bytes start-end/total` on the `206 Partial Content` response; ranged/multipart download clients (for example the AWS CLI) require this numeric total. If `TotalSize` is left null, the total is emitted as `*` (unknown).

For a suffix range (`Range: bytes=-N`, the last N bytes), `RangeStart` and `RangeEnd` are null and `ctx.Request.RangeSuffixLength` is set. Return the last N bytes (or the whole object if it is shorter) and set `TotalSize`, which is required: S3Server computes `Content-Range` from it, and fails the request with `InternalError` if it is missing. Set `RouteSuffixRangesToReadRange = false` to send suffix ranges to `Object.Read` as versions before 7.4.0 did.

When `TotalSize` is set, S3Server applies Amazon S3's range rules itself, so a `ReadRange` callback can simply clamp and return what it has:

- A range that starts at or past the end of the object (`bytes=10-` on a 10-byte object) and a zero-length suffix (`bytes=-0`) return `416 InvalidRange` with `RangeRequested` and `ActualObjectSize` in the error body.
- A suffix range on an empty object returns `200` with an empty body.
- `Content-Range` describes the bytes actually returned, so `bytes=0-100` on a 10-byte object is answered with `bytes 0-9/10`.

`HEAD` honors `Range` the same way: `206` with `Content-Range` and the length of the range, or `416`.

`Object.WriteAcl` and `Bucket.WriteAcl` receive a `null` policy when the request has no body, which is how canned ACLs (`x-amz-acl`) and grant headers (`x-amz-grant-*`) arrive. Read those headers with `ctx.Request.RetrieveHeaderValue`. A body that is present but not valid XML is rejected with `MalformedACLError`, as Amazon S3 does.

`Object.Copy` receives a PUT carrying `x-amz-copy-source`. The source is parsed into `ctx.Request.CopySourceBucket`, `CopySourceKey`, and `CopySourceVersionId`. If `Object.Copy` is not set, the request goes to `DefaultRequestHandler` or returns `NotImplemented`. It is never delivered to `Object.Write`.

For archived objects, set `ObjectMetadata.RestoreStatus` and `S3Object.RestoreStatus` to have S3Server emit the `x-amz-restore` response header on `HEAD` and `GET`.

### Multipart Upload Callbacks

| Callback | Description | Method | URL | Response Type |
|----------|-------------|--------|-----|---------------|
| `Object.CreateMultipartUpload` | Initiate multipart upload | POST | `/[bucket]/[key]?uploads` | `InitiateMultipartUploadResult` |
| `Object.UploadPart` | Upload a part | PUT | `/[bucket]/[key]?partNumber=N&uploadId=ID` | `void` |
| `Object.UploadPartCopy` | Upload a part by copying | PUT | `/[bucket]/[key]?partNumber=N&uploadId=ID` with `x-amz-copy-source` | `CopyPartResult` |
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
long? RangeSuffixLength       // N for a suffix range (bytes=-N); RangeStart and RangeEnd are then null

// Copy requests (ObjectCopy and ObjectUploadPartCopy)
string CopySourceBucket       // From x-amz-copy-source, URL-decoded
string CopySourceKey          // From x-amz-copy-source, URL-decoded
string CopySourceVersionId    // From the versionId parameter of x-amz-copy-source
long? CopySourceRangeStart    // From x-amz-copy-source-range
long? CopySourceRangeEnd      // From x-amz-copy-source-range

// Conditional requests (evaluated by your callback; see the property docs for RFC 9110 order)
List<string> IfMatch          // Entity tags as sent, or "*"
List<string> IfNoneMatch      // Entity tags as sent, or "*"
DateTime? IfModifiedSince     // Null when absent or not a valid HTTP date
DateTime? IfUnmodifiedSince   // Null when absent or not a valid HTTP date
// Also CopySourceIfMatch, CopySourceIfNoneMatch, CopySourceIfModifiedSince, CopySourceIfUnmodifiedSince

// Listing parameters
int MaxKeys                   // Maximum keys to return (default 1000)
string Prefix                 // Object key prefix filter
string Delimiter              // Delimiter for grouping
string Marker                 // Pagination marker
string ContinuationToken      // Continuation token for v2 listing
string StartAfter             // start-after for v2 listing
bool FetchOwner               // fetch-owner=true for v2 listing

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
string RetrieveQueryValue(string key)   // URL-decoded value, or null if absent
Task<Chunk> ReadChunk()       // Read chunk for chunked transfer encoding
```

Use `RetrieveQueryValue` for querystring parameters S3Server does not parse. It returns decoded values (`a%2Fb%20c` becomes `a/b c`). `ctx.Http.Request.Query.Elements` holds the raw, percent-encoded values.

Querystring parameters are validated only by the operations that use them, with the error Amazon S3 returns (`400 InvalidArgument` with `ArgumentName` and `ArgumentValue`), before any handler runs:

| Parameter | Operations | Rejected values |
|-----------|------------|-----------------|
| `max-keys` | ListObjects, ListObjectsV2, ListObjectVersions | negative, non-integer, out of range |
| `max-uploads` | ListMultipartUploads | negative, non-integer, out of range |
| `max-parts`, `part-number-marker` | ListParts | negative, non-integer, out of range |
| `partNumber` | GetObject, HeadObject, UploadPart, UploadPartCopy | anything other than 1 to 10000 |
| `encoding-type` | the listing operations | anything other than `url` |

Other operations ignore these parameters, as Amazon S3 does (for example, `GET /bucket/key?max-keys=abc` returns the object). The error is also available to handlers as `ctx.Request.ValidationError`.

`ctx.Request` also parses `KeyMarker`, `VersionIdMarker`, `UploadIdMarker`, `MaxUploads`, and `EncodingType`. `PartNumberMarker` defaults to `0`, as Amazon S3's does.

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

### Conditional Requests (304 Not Modified)

S3Server parses `If-Match`, `If-None-Match`, `If-Modified-Since`, and `If-Unmodified-Since` into `ctx.Request`. Your callback decides whether each condition holds, because only it knows the object's ETag and modification time. To answer `304 Not Modified`, add the `ETag` (and optionally `Last-Modified`) headers and throw `NotModified`. The response is sent with headers only, with no body and no `Content-Type`, as Amazon S3 does:

```csharp
server.Object.Read = async (ctx) =>
{
    ObjectMetadata md = GetMetadata(ctx.Request.Key);
    ctx.Response.Headers.Add("ETag", md.ETag);

    if (ctx.Request.IfNoneMatch != null && ctx.Request.IfNoneMatch.Contains(md.ETag))
        throw new S3Exception(new Error(ErrorCode.NotModified));

    // ... return object
};
```

A failed `If-Match` or `If-Unmodified-Since` should throw `PreconditionFailed` (412).

### Response Format

S3Server shapes response bodies the way Amazon S3 does, so callbacks can return plain models:

- **Namespace.** Response bodies are serialized in the Amazon S3 namespace (`http://s3.amazonaws.com/doc/2006-03-01/`). A top-level `Error` has no namespace, and `BucketLoggingStatus` uses `http://doc.s3.amazonaws.com/2006-03-01`. Request bodies are accepted with or without a namespace.
- **Timestamps.** `LastModified`, `Initiated`, `CreationDate`, and `RetainUntilDate` are written in UTC with exactly three fractional digits (`2026-01-02T03:04:05.000Z`).
- **ListObjects v1 and v2.** A v1 response always has `Marker` (empty when not set) and never `KeyCount`, `ContinuationToken`, `StartAfter`, or `NextContinuationToken`. A v2 response never has `Marker` or `NextMarker`, and omits `Owner` from each object unless the request set `fetch-owner=true`.
- **`encoding-type=url`.** When a listing request asks for it (the AWS CLI and boto3 always do), S3Server URL-encodes `Key`, `Prefix`, `Delimiter`, `Marker`, `NextMarker`, `StartAfter`, `KeyMarker`, and `NextKeyMarker` the way Amazon S3 does, and sets `EncodingType` to `url`. If a callback already set `EncodingType` on its result, S3Server assumes the callback encoded the values itself and leaves them alone.
- **DeleteObjects quiet mode.** When the request sets `<Quiet>true</Quiet>`, `Deleted` entries are omitted and only errors are returned.
- **Error details.** `Error` carries `ArgumentName`, `ArgumentValue`, `Condition`, `RangeRequested`, `ActualObjectSize`, `UploadId`, and `BucketName`, each written only when set. S3Server fills `Key` for `NoSuchKey`, `BucketName` for `NoSuchBucket`, `UploadId` for `NoSuchUpload`, and `RangeRequested` for `InvalidRange` from the request. Set `Condition` (for example `If-Match`) when throwing `PreconditionFailed`.
- **HEAD errors.** An error response to `HEAD` has a status and headers but no body.
- **Headers.** Responses carry no request-only or CORS headers by default; see `PreserveWebserverDefaultHeaders` and `EmitCorsHeaders`. `Connection: close` is sent only when the webserver's keep-alive is disabled (`WebserverSettings.IO.EnableKeepAlive`, which is off by default).

Serializing a model directly with `SerializationHelper.SerializeXml` (outside a request) keeps the model's own shape: nothing is omitted or encoded based on a request.

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

### Archived Object Restore

```csharp
server.Object.Restore = async (ctx, request) =>
{
    // Start restore work in your backend, or extend an existing restore window.
    bool alreadyRestored = TryExtendRestoreWindow(
        ctx.Request.Bucket,
        ctx.Request.Key,
        request.Days.Value,
        request.EffectiveTier);

    return new RestoreObjectResult
    {
        AlreadyRestored = alreadyRestored
    };
};

server.Object.Exists = async (ctx) =>
{
    return new ObjectMetadata(ctx.Request.Key, DateTime.UtcNow, "etag", 123, new Owner("admin", "Administrator"), StorageClassEnum.GLACIER)
    {
        ContentType = "application/octet-stream",
        RestoreStatus = new RestoreStatus
        {
            OngoingRequest = false,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        }
    };
};

server.Object.Read = async (ctx) =>
{
    if (!IsRestored(ctx.Request.Bucket, ctx.Request.Key))
        throw new S3Exception(new Error(ErrorCode.InvalidObjectState));

    return new S3Object(ctx.Request.Key, "1", true, DateTime.UtcNow, "etag", 5, new Owner("admin", "Administrator"), "hello", "text/plain", StorageClassEnum.GLACIER)
    {
        RestoreStatus = new RestoreStatus
        {
            OngoingRequest = false,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        }
    };
};
```

S3Server routes `POST ?restore`, returns `202 Accepted` for a newly initiated restore, `200 OK` when your callback reports an already-restored object, and surfaces restore state through the `x-amz-restore` header on `HEAD` and `GET`.

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
- **Behavior that depends on stored data is left to callbacks**: for example, `GET /bucket/key?partNumber=N` on a multipart object (Amazon S3 returns that part with `206` and `x-amz-mp-parts-count`), versioning semantics, and bucket CORS configurations

The following S3 operations are not exposed through callbacks (may be added in future releases):

**Bucket operations:**
- Accelerate, Analytics, CORS, Encryption, Inventory, Lifecycle, Notification, Object lock configuration, Policy status, Public access block, Metrics, Payment, Policy, Replication

**Object operations:**
- Torrent

## Examples and Testing

Comprehensive examples are available in the repository:

- **`Test.Server`**: Complete server implementation with all callbacks
- **`Test.Client`**: S3 client examples using AWS SDK
- **`Test.RequestStyle`**: Manual path-style vs virtual-hosted-style URL example harness
- **`Test.SignatureValidation`**: Manual AWS Signature V4 and V2 validation example harness
- **`Test.Automated`**: Touchstone console runner over the shared suites
- **`Test.Shared`**: Touchstone descriptor source of truth for automated, xUnit, and NUnit tests, including parser/routing, compatibility, adversarial, fuzz, lifecycle, and signature coverage
- **`Test.Xunit`**: xUnit test project using `Touchstone.XunitAdapter`
- **`Test.Nunit`**: NUnit test project using `Touchstone.NunitAdapter`
- **`Test.Compatibility`**: Runs the Amazon S3 compatibility scenarios against S3Server or Amazon S3, or hosts the reference server for external clients

Run the test server (requires admin on Windows for wildcard listeners):

```bash
dotnet run --project src/Test.Server/Test.Server.csproj
```

Run the shared automated suite through each supported runner:

```bash
dotnet run --project src/Test.Automated/Test.Automated.csproj --framework net8.0
dotnet run --project src/Test.Automated/Test.Automated.csproj --framework net10.0
dotnet test src/Test.Xunit/Test.Xunit.csproj
dotnet test src/Test.Nunit/Test.Nunit.csproj
```

The xUnit and NUnit adapter projects execute these socket-bound integration descriptors serially to avoid local listener port races.

### Amazon S3 Compatibility Tests

The `Compatibility` suite runs wire-level scenarios (ranges, `HEAD` ranges, parameter validation, listing shapes and pagination, `encoding-type=url`, conditional requests, response headers, CopyObject, multipart uploads including UploadPartCopy, DeleteObjects, ACLs, and error bodies). They are sent as raw SigV4-signed requests to S3Server running `Test.Shared.Compatibility.ReferenceS3Backend`, an in-memory backend, with signature validation enabled. Every expectation was recorded from Amazon S3, and `Test.Compatibility` can send the same scenarios to Amazon S3 to confirm they still hold:

```bash
# Against S3Server with the reference backend
dotnet run --project src/Test.Compatibility/Test.Compatibility.csproj -- --target local

# Against Amazon S3 (credentials are read from the environment, never from arguments)
export S3COMPAT_ACCESS_KEY=...
export S3COMPAT_SECRET_KEY=...
dotnet run --project src/Test.Compatibility/Test.Compatibility.csproj -- --target s3 --bucket my-bucket --region us-west-1

# Host the reference server for external clients (AWS CLI, boto3, MinIO client)
dotnet run --project src/Test.Compatibility/Test.Compatibility.csproj -- --serve 8000 --bucket compat-bucket
```

Against Amazon S3, the scenarios create objects only under a unique `s3server-compat-*` prefix and delete only that prefix afterward. They never change bucket settings or touch other objects.

Collect coverage for the library with the shared runsettings file:

```bash
dotnet test src/Test.Xunit/Test.Xunit.csproj --settings coverage.runsettings --collect:"XPlat Code Coverage"
dotnet test src/Test.Nunit/Test.Nunit.csproj --settings coverage.runsettings --collect:"XPlat Code Coverage"
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

- **Watson** (7.1.0): HTTP server framework (supports HTTP/1.1, HTTP/2, and HTTP/3)
- **AWSSignatureGenerator** (1.1.0): AWS Signature V4 validation, streaming signature support, and legacy S3 Signature V2 helpers
- **PrettyId** (2.0.1): Request ID generation
- **System.Diagnostics.DiagnosticSource** (8.0.1, `netstandard2.1` only): `Meter` and `ActivitySource` for telemetry (inbox on `net8.0` and `net10.0`)
- **Touchstone** (0.1.12): Shared test descriptors and console/xUnit/NUnit test runners

## Resources

- **Documentation**: [Amazon S3 REST API Reference](https://docs.aws.amazon.com/AmazonS3/latest/API/Welcome.html)
- **Complete Example**: [Less3 - Production S3 Server](https://github.com/jchristn/less3)
- **NuGet Package**: [S3Server on NuGet](https://www.nuget.org/packages/S3Server/)

## Feedback and Enhancements

Have a feature request or found an issue? Please [file an issue on GitHub](https://github.com/jchristn/s3server/issues)!

## Version History

Refer to [CHANGELOG.md](CHANGELOG.md) for version history and release notes.

## New in v8.1.0

- Built-in observability: metrics and traces on a `Meter` and `ActivitySource` named `S3Server`, configured with `S3ServerSettings.Telemetry`. Covers S3 operations, pipeline stages, application callbacks, signature validation outcomes, errors by type and stage, object sizes, lifecycle, and build and config gauges
- Spans nest under Watson's per-request server span and join an inbound W3C `traceparent`
- Grafana dashboard in `assets/grafana/s3server.json`, and [TELEMETRY.md](TELEMETRY.md) documenting every telemetry point
- No public API is removed or changed; no exporter dependency is added

## New in v8.0.1

- ListObjectVersions delete markers contain exactly `Key`, `VersionId`, `IsLatest`, `LastModified`, and `Owner`, with no `xsi:nil` `ETag` and no `StorageClass`
- DeleteObjects per-key errors are written as `Key`, `VersionId`, `Code`, `Message`, while standalone error bodies still start with `Code`
- No public API is removed or changed. See [CHANGELOG.md](CHANGELOG.md) for details

## New in v8.0.0

This major release makes S3Server's responses match Amazon S3 in the details that 7.4.0 left out. Each behavior was checked against Amazon S3, and the new `Compatibility` suite enforces it. See [CHANGELOG.md](CHANGELOG.md) for breaking changes and migration notes.

- Querystring parameters are validated only by the operations that use them, with Amazon S3's exact errors (`ArgumentName`, `ArgumentValue`, and messages). `partNumber` must be 1 to 10000, and `encoding-type` must be `url`
- `encoding-type=url` listings are URL-encoded automatically, and ListObjects v1 and v2 responses take their exact Amazon S3 shapes
- `HEAD` honors `Range`, unsatisfiable ranges return `416` with `RangeRequested` and `ActualObjectSize`, and a suffix range on an empty object returns `200`
- Error bodies carry Amazon S3's detail elements, and errors on `HEAD` have no body
- Timestamps are written with millisecond precision, as Amazon S3 does
- DeleteObjects honors `Quiet`, and malformed ACL bodies return `MalformedACLError`
- CORS and `Accept-Charset` headers are no longer sent by default (`EmitCorsHeaders` restores CORS for browser clients), and `Connection: close` follows the keep-alive setting
- New `S3Request` properties: `KeyMarker`, `VersionIdMarker`, `UploadIdMarker`, `MaxUploads`, `EncodingType`, and `ValidationError`
- New compatibility test tooling: the `Compatibility` suite and the `Test.Compatibility` runner

## New in v7.4.0

This release fixes security, data integrity, and protocol accuracy problems found while building Less3's S3 compatibility suite. Some existing behavior changes; see [CHANGELOG.md](CHANGELOG.md) for details and opt-out settings.

- `AuthenticatedRequestHandler` runs after signature validation. `ValidateSignaturesBeforePreRequestHandler` moves validation ahead of `PreRequestHandler`, which by default sees unauthenticated input
- Canned ACL writes (`x-amz-acl`, `x-amz-grant-*`, empty body) reach `WriteAcl` with a `null` policy instead of failing
- Signature enforcement fails closed if `Service.GetSecretKey` is cleared after `Start()`
- Unparseable requests (for example `max-keys=-1`) return an S3 XML error instead of the webserver's HTML error page. Invalid numeric parameters return `400 InvalidArgument`
- Malformed `Range` headers are ignored and the full object is served, as Amazon S3 does. Suffix ranges (`bytes=-N`) route to `Object.ReadRange`
- CopyObject and UploadPartCopy have their own request types and callbacks (`Object.Copy`, `Object.UploadPartCopy`) and are never delivered to `Object.Write` or `Object.UploadPart` as empty uploads
- Response bodies carry the S3 XML namespace, `DeleteResult` omits nil and false values, `ListBucketResult` adds `NextMarker`, `StartAfter`, and `ContinuationToken`, and `ListVersionsResult.Entries` interleaves versions and delete markers
- `ErrorCode.NotModified` sends a bodyless `304`, and conditional request headers are parsed into typed properties
- Request-only headers (`Host`, `Accept`, `Accept-Language`, `Cache-Control`) are no longer echoed on every response

## New in v7.3.2

- Upgrades to Watson `7.1.0` and refreshes the remaining dependencies to their latest stable versions
- Fixes S3 Select request parsing: invalid `[XmlElement(IsNullable = true)]` annotations on non-nullable value-type properties of the S3 Select models caused `DeserializeXml<SelectObjectContentRequest>` to throw for every `SelectObjectContent` request; the annotations were removed
- Fixes the `S3Object.DataString` setter, which truncated multibyte UTF-8 payloads by copying the character count instead of the byte count
- Expands automated test coverage with new positive/negative unit suites for the S3 data models and `SerializationHelper` (line coverage ~86% to ~94%, branch coverage ~63% to ~79%)
- No public API surface changes

## New in v7.3.1

- Range (`206 Partial Content`) responses emit a real `Content-Range` total (`bytes start-end/total`) when the `Object.ReadRange` callback sets the new `S3Object.TotalSize` to the full object size
- Fixes ranged/multipart download clients such as the AWS CLI (`aws s3 cp` of large objects), which cannot parse the previous `bytes start-end/*` total
- Backward compatible: when `TotalSize` is not set, the `Content-Range` total remains `*` as before

## New in v7.3.0

- Adds `Service.IsAnonymousRequestAllowed` for opt-in unsigned anonymous access when `EnableSignatures` is true
- Supports public-read/public-write-style compatibility while keeping unsigned requests rejected by default
- Keeps invalid signed requests fail-closed; requests with authorization or signed URL material do not fall back to anonymous access
- Adds signature validation coverage for allowed anonymous reads/writes, denied anonymous requests, invalid signed requests, and V4 presigned URL material

## New in v7.2.0

- Adds opt-in legacy AWS Signature V2 validation behind `S3ServerSettings.EnableSignatureV2`
- Validates V2 `Authorization: AWS ...` headers and V2 signed URLs with `AWSAccessKeyId`, `Expires`, and `Signature`
- Keeps V2 disabled by default while preserving existing V4 validation behavior
- Adds canonical AWS Signature V2 fixture tests, V2 positive/negative request tests, and virtual-hosted-style request coverage
- Uses fixed-time comparison for V2 signature checks
- Serializes xUnit and NUnit adapter execution for socket-bound shared descriptors

## New in v7.1.2

- Exposes each shared Touchstone scenario as a first-class descriptor across console, xUnit, and NUnit runners
- Adds parser/routing, protocol compatibility, adversarial, deterministic fuzz, lifecycle/concurrency, and deeper signature coverage
- Hardens S3 query parsing for common camel-case aliases used by V2 signed URLs and multipart requests
- Includes S3 request identifiers in XML error responses sent through `S3Response.Send(Error)`
- Adds `coverage.runsettings` for XPlat Code Coverage collection

## New in v7.1.1

- Updated `AWSSignatureGenerator` to 1.1.0
- Migrated shared tests to Touchstone descriptors consumed by console, xUnit, and NUnit runners
- Added `Test.Nunit`
- Added stricter negative signature and restore-body coverage
- Added [archive/V2_SIGNATURES.md](archive/V2_SIGNATURES.md) with the end-to-end plan for V2 header and signed URL validation

## New in v7.1.0

- Added `Object.Restore` callback support for `POST ?restore`
- Added typed restore request/result/status models
- `S3Object` and `ObjectMetadata` now emit `x-amz-restore` when `RestoreStatus` is supplied
- Added restore coverage in automated, compliance, xUnit, and signature validation tests

## New in v7.0.5

- Watson 7.0.9 dependency update
- Fix UriFormatException when server is bound to wildcard hostname (`*`, `+`, `0.0.0.0`)
- Signature validation fail-closed, range 206, unwired ops return 501
- Updated to Watson 7.0 (Watson.Lite removed; Watson now handles all transport modes natively)
- Target frameworks updated to .NET 8.0 and .NET 10.0 (dropped netstandard2.0, netstandard2.1, net6.0)
- Updated to AWSSignatureGenerator 1.0.12 with streaming signature validation (AWSSDK 4.x compatible)
- Updated to PrettyId 2.0.1
- AWS Signature V4 validation now supports streaming signatures (`STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER`)
- `MaxPutObjectSize` check uses `X-Amz-Decoded-Content-Length` when aws-chunked encoding is in use
- Removed `UseTcpServer` setting (Watson 7.0 uses TCP natively)
- Added comprehensive test infrastructure (Test.Shared, Test.Xunit)
- Added signature validation test coverage using real AWSSDK 4.x client calls
- Added S3 compliance test suite

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
