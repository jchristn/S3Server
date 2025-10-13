# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

S3Server is a .NET library that provides a server-side interface for emulating AWS S3 operations. It's a NuGet package that allows developers to build S3-compatible storage servers (see [Less3](https://github.com/jchristn/less3) for a full implementation).

The library parses incoming S3 HTTP requests and routes them to callback methods that developers must implement. It does NOT provide the storage logic itself - that's the responsibility of the consuming application.

## Build and Test Commands

### Build the solution
```bash
dotnet build src/S3Server.sln
```

### Build specific configuration
```bash
dotnet build src/S3Server.sln -c Release
dotnet build src/S3Server.sln -c Debug
```

### Build the main library project
```bash
dotnet build src/S3Server/S3Server.csproj
```

### Run test server (must run as administrator on Windows for wildcard listeners)
```bash
dotnet run --project src/Test.Server/Test.Server.csproj
```

### Run test client
```bash
dotnet run --project src/Test.Client/Test.Client.csproj
```

### Pack NuGet package
```bash
dotnet pack src/S3Server/S3Server.csproj -c Release
```

## Architecture

### Core Request Flow

1. **S3Server** (S3Server.cs:19) - Main entry point that wraps WatsonWebserver
   - Listens for HTTP requests via Watson webserver
   - Routes all requests to `RequestHandler` method (line 181)
   - Manages lifecycle (Start/Stop/Dispose)

2. **S3Context** (S3Context.cs:11) - Request context passed to all callbacks
   - Contains `S3Request`: Parsed S3 request details
   - Contains `S3Response`: Response object for sending data back
   - Contains `Http`: Underlying HTTP context from WatsonWebserver
   - Contains `Metadata`: User-defined metadata (useful for PreRequestHandler auth)

3. **S3Request** (S3Request.cs:16) - Parsed S3 request information
   - Determines `RequestType` (bucket vs object operation)
   - Determines `RequestStyle` (path-style vs virtual-hosted)
   - Extracts bucket name, object key, query parameters
   - Parses AWS signature information

4. **Callback System** - Three categories of callbacks:
   - `ServiceCallbacks` (ServiceCallbacks.cs:10): Service-level operations (list buckets, service exists)
   - `BucketCallbacks`: Bucket operations (create, delete, read, ACLs, tags, etc.)
   - `ObjectCallbacks`: Object operations (read, write, delete, multipart, ACLs, etc.)

### Request Handling Flow

```
HTTP Request → S3Server.RequestHandler()
  → S3Context created (wraps HTTP context)
  → S3Request parses request (determines type, style, bucket, key)
  → PreRequestHandler (optional - for auth/logging)
  → Signature validation (if enabled)
  → Switch on RequestType → Invoke appropriate callback
  → Callback returns data or throws S3Exception
  → S3Response sends HTTP response
  → PostRequestHandler (optional - for logging/metrics)
```

### Request Types and URL Patterns

**Path-style URLs** (default): `http://host:port/bucket/key`
**Virtual-hosted URLs**: `http://bucket.basedomain/key` (requires `FindMatchingBaseDomain` callback)

The library automatically detects which style is being used based on the hostname and configured base domains.

## Key Implementation Details

### S3ServerSettings Configuration

Settings must be configured BEFORE starting the server:

- `Webserver`: WebserverSettings from Watson (hostname, port, SSL)
- `Logger`: Action<string> for logging
- `Logging`: Toggle logging for HTTP requests, S3 requests, signatures
- `EnableSignatures`: Enable AWS signature V4 validation
- `OperationLimits`: Size limits (e.g., MaxPutObjectSize)
- `PreRequestHandler`: Called before routing (return true to terminate)
- `DefaultRequestHandler`: Called when no callback matches
- `PostRequestHandler`: Called after response sent

### Callback Patterns

Callbacks follow three patterns:

1. **Return typed result**: `Task<ListAllMyBucketsResult> ListBuckets(S3Context ctx)`
2. **Return void**: `Task BucketDelete(S3Context ctx)` - just return after completion
3. **Throw S3Exception**: `throw new S3Exception(new Error(ErrorCode.NoSuchBucket))`

The library automatically serializes return values to XML and sets appropriate status codes.

### Error Handling

Use `S3Exception` with `Error` objects for S3-compliant errors:
- `throw new S3Exception(new Error(ErrorCode.NoSuchBucket))`
- `throw new S3Exception(new Error(ErrorCode.AccessDenied))`
- See ErrorCode enum for all available error codes

### Multipart Upload Support

The library supports multipart upload operations through callbacks:
- `CreateMultipartUpload`: Initiate upload
- `UploadPart`: Upload individual parts
- `CompleteMultipartUpload`: Finalize upload
- `AbortMultipartUpload`: Cancel upload
- `ReadParts`: List parts of an upload

### Target Frameworks

The library targets multiple frameworks (S3Server.csproj:4):
- netstandard2.0
- netstandard2.1
- net6.0
- net8.0

Use appropriate .NET SDK versions when building.

## Virtual Hosting Configuration

To support virtual-hosted style URLs (`bucket.domain.com/key`):

1. Use wildcard listener hostname (`*` or `+`) in WebserverSettings
2. Must run as administrator (Windows) or with appropriate permissions
3. Implement `Service.FindMatchingBaseDomain` callback:
   - Input: hostname from HOST header
   - Output: matching base domain (or throw KeyNotFoundException)
   - Example: input="mybucket.s3.local.gd" → output="s3.local.gd"
4. Configure DNS or hosts file to resolve bucket subdomains
5. Useful test domains: `*.local.gd` or `*.fbi.com` (resolve to localhost)

## Signature Validation

AWS Signature V4 validation can be enabled:

1. Set `Settings.EnableSignatures = true`
2. Implement `Service.GetSecretKey` callback that returns base64 secret key for access key
3. Library validates signature using AWSSignatureGenerator package
4. Only V4 signatures are supported (V2 returns error)
5. Chunk signature validation is NOT yet supported

## Notable Dependencies

- **Watson** (6.3.13): HTTP server framework
- **AWSSignatureGenerator** (1.0.9): AWS signature validation
- **PrettyId** (1.0.5): Request ID generation

## Project Structure

```
src/
  S3Server/              - Main library code
    S3Server.cs          - Entry point class
    S3Context.cs         - Request context
    S3Request.cs         - Request parser
    S3Response.cs        - Response builder
    S3ServerSettings.cs  - Configuration
    Callbacks/           - Callback interfaces
      ServiceCallbacks.cs
      BucketCallbacks.cs
      ObjectCallbacks.cs
    S3Objects/           - S3 data models (serialized to/from XML)
  Test.Server/           - Example server implementation
  Test.Client/           - Example S3 client
  Test.RequestStyle/     - Request style testing
  Test.SignatureValidation/ - Signature validation testing
```

## Important Notes

- This library provides the HTTP interface and request parsing ONLY
- Storage logic (persisting buckets/objects) must be implemented in callbacks
- The library returns generic 400 errors if no callback is set for a request type
- All XML serialization/deserialization is handled automatically
- The library uses WatsonWebserver's HttpContextBase internally
- S3Request extracts bucket/key from either path-style or virtual-hosted URLs
- The main request handler (S3Server.cs:181) is a 800+ line switch statement routing requests

## XML Handling

Request/response bodies use XML serialization via SerializationHelper:
- Incoming XML is deserialized to S3Objects (Tagging, AccessControlPolicy, etc.)
- Callbacks return S3Objects, which are serialized to XML automatically
- Invalid XML throws InvalidOperationException, caught and returns MalformedXML error
- All S3Objects models are in `S3Objects/` directory

## Size Limits

OperationLimitsSettings.MaxPutObjectSize controls maximum object size for PutObject. Exceeding this returns EntityTooLarge error. Multipart uploads are not subject to this limit on a per-part basis.

## Coding Standards and Style Rules

**THESE RULES MUST BE FOLLOWED STRICTLY FOR ALL CODE IN THIS REPOSITORY.**

### File Structure and Organization

**Namespace and Using Statements:**
- Namespace declaration must always be at the top of the file
- Using statements must be contained INSIDE the namespace block
- Microsoft and standard system library usings come first, in alphabetical order
- Other using statements follow, in alphabetical order

**Example:**
```csharp
namespace S3ServerLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using PrettyId;
    using WatsonWebserver.Core;

    public class MyClass { }
}
```

**File Organization:**
- Limit each file to containing exactly ONE class or exactly ONE enum
- Do NOT nest multiple classes or multiple enums in a single file
- Regions for Public-Members, Private-Members, Constructors-and-Factories, Public-Methods, and Private-Methods are NOT required for small files under 500 lines

### Documentation Requirements

**XML Documentation:**
- All public members, constructors, and public methods MUST have XML code documentation
- NO code documentation should be applied to private members or private methods
- Document default values, minimum values, and maximum values where appropriate
- Specify what different values mean or what effect they may have
- Document which exceptions public methods can throw using `/// <exception>` tags
- Document nullability expectations in XML comments
- Document thread safety guarantees in XML comments

### Naming Conventions

**Private Members:**
- Private class member variable names MUST start with an underscore and then be Pascal cased
- Correct: `_FooBar`
- Incorrect: `_fooBar`

**Variable Declarations:**
- Do NOT use `var` when defining a variable
- Always use the actual type name

**Example:**
```csharp
// Correct
string myValue = "test";
List<string> items = new List<string>();

// Incorrect
var myValue = "test";
var items = new List<string>();
```

### Property Implementation

**Public Members:**
- All public members should have explicit getters and setters
- Use backing private variables when value requires range or null validation

**Example:**
```csharp
private int _MaxConnections = 100;

/// <summary>
/// Maximum number of connections.
/// Default is 100, minimum is 1, maximum is 10000.
/// </summary>
public int MaxConnections
{
    get { return _MaxConnections; }
    set
    {
        if (value < 1) throw new ArgumentOutOfRangeException(nameof(MaxConnections));
        _MaxConnections = value;
    }
}
```

### Async and Threading

**Async Method Requirements:**
- Every async method should accept a CancellationToken as an input parameter, unless the class has a CancellationToken or CancellationTokenSource as a class member
- Async calls should use `.ConfigureAwait(false)` where appropriate
- Async calls should check whether cancellation has been requested at appropriate places
- When implementing a method that returns an IEnumerable, also create an async variant of that same method that includes a CancellationToken

**Thread Safety:**
- Use Interlocked operations for simple atomic operations
- Prefer ReaderWriterLockSlim over lock for read-heavy scenarios
- Document thread safety guarantees in XML comments

### Exception Handling

**Exception Best Practices:**
- Use specific exception types rather than generic Exception
- Always include meaningful error messages with context
- Consider using custom exception types for domain-specific errors
- Document which exceptions public methods can throw using `/// <exception>` tags
- Use exception filters when appropriate: `catch (SqlException ex) when (ex.Number == 2601)`
- Proactively identify and eliminate any situations in code where null might cause exceptions to be thrown

### Resource Management

**IDisposable Pattern:**
- Implement IDisposable/IAsyncDisposable when holding unmanaged resources or disposable objects
- Use 'using' statements or 'using' declarations for IDisposable objects
- Follow the full Dispose pattern with `protected virtual void Dispose(bool disposing)`
- Always call `base.Dispose()` in derived classes

### Null Handling

**Nullable Reference Types:**
- Use nullable reference types (enable `<Nullable>enable</Nullable>` in project files)
- Validate input parameters with guard clauses at method start
- Use `ArgumentNullException.ThrowIfNull()` for .NET 6+ or manual null checks
- Consider using the Result pattern or Option/Maybe types for methods that can fail
- Document nullability in XML comments

### LINQ and Collections

**LINQ Best Practices:**
- Prefer LINQ methods over manual loops when readability is not compromised
- Use `.Any()` instead of `.Count() > 0` for existence checks
- Be aware of multiple enumeration issues - consider `.ToList()` when needed
- Use `.FirstOrDefault()` with null checks rather than `.First()` when element might not exist

### Design Principles

**Configuration Over Constants:**
- Avoid using constant values for things that a developer may later want to configure or otherwise change
- Instead use a public member with a backing private member set to a reasonable default
- Document the default value in XML comments

**Tuples:**
- Do NOT use tuples unless absolutely, absolutely necessary
- Prefer creating a proper class or struct to represent multiple return values

**Library Code:**
- Ensure NO Console.WriteLine statements are added to library code
- Use the Logger callback pattern for diagnostic output

### Code Assumptions

**Opaque Classes:**
- Do not make any assumptions about what class members or class methods exist on a class that is opaque to you
- When in doubt, ask for the implementation of an opaque class

**SQL Statements:**
- If code uses manually prepared strings for SQL statements, assume there is a good reason
- Do not automatically suggest converting to ORMs or query builders

### Code Quality

**Compilation:**
- When possible, compile the code and ensure it is free of errors and warnings
- Test changes to verify they work correctly

**README Accuracy:**
- If a README exists, analyze it and ensure it is accurate
- Update documentation when making changes that affect usage

### Example: Complete Class Following All Rules

```csharp
namespace S3ServerLibrary
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Manages S3 object operations.
    /// </summary>
    public class ObjectManager : IDisposable
    {
        #region Public-Members

        private int _MaxRetries = 3;

        /// <summary>
        /// Maximum number of retry attempts.
        /// Default is 3, minimum is 0, maximum is 10.
        /// Higher values increase reliability but may impact performance.
        /// </summary>
        public int MaxRetries
        {
            get { return _MaxRetries; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(MaxRetries), "MaxRetries must be non-negative.");
                if (value > 10) throw new ArgumentOutOfRangeException(nameof(MaxRetries), "MaxRetries cannot exceed 10.");
                _MaxRetries = value;
            }
        }

        #endregion

        #region Private-Members

        private bool _Disposed = false;
        private CancellationTokenSource _TokenSource = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public ObjectManager()
        {
            _TokenSource = new CancellationTokenSource();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Process an object asynchronously.
        /// Thread-safe method that can be called concurrently.
        /// </summary>
        /// <param name="objectKey">Object key to process. Cannot be null or empty.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentNullException">Thrown if objectKey is null.</exception>
        /// <exception cref="ArgumentException">Thrown if objectKey is empty.</exception>
        /// <exception cref="OperationCanceledException">Thrown if operation is cancelled.</exception>
        public async Task ProcessObjectAsync(string objectKey, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(objectKey)) throw new ArgumentNullException(nameof(objectKey));

            token.ThrowIfCancellationRequested();

            // Implementation here
            await Task.Delay(100, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private-Methods

        /// <summary>
        /// Dispose of resources.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed) return;

            if (disposing)
            {
                if (_TokenSource != null)
                {
                    _TokenSource.Cancel();
                    _TokenSource.Dispose();
                }
            }

            _Disposed = true;
        }

        #endregion
    }
}
```
