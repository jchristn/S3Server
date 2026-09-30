# S3Server Bug Fix Plan

Status: implemented in S3Server `7.4.0` (see CHANGELOG.md). BF-08 was dropped after verification: `Error.HttpStatusCode` is get-only, so XmlSerializer never emitted it; a regression test guards this. BF-15 needed documentation and a test only: `RetrieveQueryValue` already returned decoded values. BF-11 is implemented through a `ListVersionsEntryCollection` serialization surface, because XmlSerializer rejects two members mapped to the same `Version` element name. BF-14 keeps `Accept-Charset` (the MinIO client check was not run). Original baseline: S3Server `7.3.2` at commit `63402dc`.

Less3 found most of these problems while its S3 compatibility suite (146 cases, run against real AWS SDK, AWS CLI and MinIO clients) was being built. Less3 works around every one of them today. It answers several operations from `DefaultRequestHandler`, writes some responses by hand, and returns zero-length results so S3Server's follow-up send does nothing. Those workarounds are fragile, and every other S3Server consumer has to rediscover them. The fixes belong here.

Every item was re-checked against 7.3.2 before it went into this plan. Several problems recorded against 6.0 are already fixed and are listed at the end so nobody redoes them. Items marked "verified on the wire" were reproduced with raw HTTP against a running server.

## Ground Rules for the Implementer

The rules in `c:\code\agents\requirements` apply to every change here. The ones that bite most often are listed so they are not missed. The full text in `CODE_STYLE.md`, `BACKEND_TEST_ARCHITECTURE.md`, `VERSIONING.md` and `WRITING_DOCUMENTS.md` still governs.

- Code style: usings inside the namespace, no `var`, no tuples, one class or enum per file, `_PascalCase` private fields, XML documentation on every public member (with defaults, ranges and `<exception>` tags), none on private members, and no `Console.WriteLine` in library code. Build with zero warnings.
- No em-dashes anywhere: code, XML docs, comments, CHANGELOG, README, commit messages.
- Tests: every item ships with positive and negative cases in `src/Test.Shared/Tests`, wired through `S3ServerSuites` so `Test.Automated`, `Test.Xunit` and `Test.Nunit` all run them. Test.Shared code writes nothing to the console. Clients target `127.0.0.1`, never `localhost`.
- Versions: do not change `<Version>` in `S3Server.csproj` or the CHANGELOG heading. The grouping at the end of this plan is a proposal for the maintainer, who decides the version numbers.
- Documentation: each item adds a CHANGELOG entry under a new "Unreleased" heading, and updates README.md and CLAUDE.md wherever they describe the changed behavior.
- Compatibility: any change to existing behavior must be called out in the CHANGELOG entry, and where this plan offers an opt-out setting, the setting is part of the work.

## Priority 1: Security and Data Integrity

Each item in this group lets a request skip authentication, lets a request destroy data, or turns a client mistake into a server crash. Fix them first and release them together if possible. BF-01 and BF-02 are two halves of one problem.

### BF-01. PreRequestHandler runs before signature validation

`RequestHandler` in `S3Server.cs` calls `Settings.PreRequestHandler` at lines 237-245 and ends the request when it returns `true`. The `EnableSignatures` block does not start until line 247. Anything a consumer answers from `PreRequestHandler` is therefore never signature-checked. Less3 learned this the hard way. It answered canned-ACL writes there (see BF-02), and a request carrying nothing but a valid access key could change a bucket's ACL. The documentation presents `PreRequestHandler` as the place for authentication and logging, so consumers are steered straight into the hole.

Moving signature validation above `PreRequestHandler` is the obvious fix, and it would break consumers that populate `ctx.Metadata` in `PreRequestHandler` for `GetSecretKey` to read. The plan keeps both working:

1. Add `S3ServerSettings.AuthenticatedRequestHandler` (`Func<S3Context, Task<bool>>`, default `null`). `RequestHandler` invokes it after the signature block and before the routing `switch`. The contract is the same as `PreRequestHandler`: return `true` when the handler has sent the response.
2. Add `S3ServerSettings.ValidateSignaturesBeforePreRequestHandler` (`bool`, default `false`). When `true`, the signature block runs first. When `false`, the current order is kept. The XML doc must say plainly that with the default, `PreRequestHandler` sees unauthenticated input and must not act on or disclose data.
3. Update the XML docs of `PreRequestHandler`, README.md and CLAUDE.md ("Request Handling Flow") to describe both hooks and the ordering setting.

Tests (`SignatureValidationTests.cs`):

- A `PreRequestHandler` that returns `true` still answers a request with a forged signature under the default order. This pins down the documented behavior.
- The same request is rejected with `SignatureDoesNotMatch` when `ValidateSignaturesBeforePreRequestHandler` is `true`.
- `AuthenticatedRequestHandler` is never invoked for a forged, unsigned or unknown-key request, and is invoked exactly once for a valid one.
- `AuthenticatedRequestHandler` returning `true` ends the request, and its response is the one the client receives.
- An `IsAnonymousRequestAllowed` anonymous request reaches `AuthenticatedRequestHandler` (it passed authorization by policy).

Done when all runners pass and a consumer can move logic from `PreRequestHandler` to `AuthenticatedRequestHandler` without changing anything else.

### BF-02. Canned ACL writes are rejected as MalformedXML

`PUT /bucket?acl` and `PUT /bucket/key?acl` with an empty body and an `x-amz-acl` header (or `x-amz-grant-*` headers) are valid S3 requests. The AWS CLI sends them for `put-bucket-acl --acl public-read`. `BucketWriteAcl` (`S3Server.cs` 551-573) and `ObjectWriteAcl` (1032-1054) always deserialize the body into an `AccessControlPolicy`. An empty body throws, and the client gets `MalformedXML`. The only way a consumer can support canned ACLs is to answer them before routing, which is how Less3 ended up inside BF-01.

Change both cases: when `s3ctx.Request.DataAsString` is null or whitespace, skip deserialization and invoke the callback with a `null` policy. A non-empty body that fails to parse still returns `MalformedXML`. Update the XML docs on `BucketCallbacks.WriteAcl` and `ObjectCallbacks.WriteAcl` to say the policy is `null` when the request carries no body and the grant is in the headers. Consumers that dereference the policy without a null check will now see the request instead of S3Server rejecting it, so the CHANGELOG entry must say so.

Tests (`BucketTests.cs`, `ObjectTests.cs`):

- An empty-body `PUT ?acl` with `x-amz-acl: public-read` reaches the callback with a `null` policy, and the header is readable from the request.
- A body of `<not-xml` still returns `MalformedXML` without invoking the callback.
- A well-formed `AccessControlPolicy` body still arrives populated.

### BF-03. Signature enforcement still fails open if GetSecretKey is cleared after Start

`Start()` now refuses to start with `EnableSignatures` set and no `GetSecretKey` (lines 147-151). The per-request check at line 249 is still `if (Service.GetSecretKey != null)`, though, and it has no `else`. A consumer that swaps callbacks at runtime and passes through `null` gets unsigned access with no error. Add the `else` branch: log `"signature validation enabled but Service.GetSecretKey is not set; request rejected"` and throw `S3Exception(new Error(ErrorCode.AccessDenied))`.

Tests: start with `GetSecretKey` set, set it to `null`, and send a correctly signed request. It must be rejected with `AccessDenied`, and no operation callback may run.

### BF-04. Request parsing failures produce Watson's HTML 500 page

`S3Context` is constructed at line 225 inside the `try`. When construction throws, `s3ctx` is still `null`. Both `catch` blocks (1149-1174) only respond when `s3ctx != null`, so nothing is sent, and Watson answers with its own HTML page, "It's me, not you", and status 500. Verified on the wire with `GET /bucket?max-keys=-1`: the `S3Request.MaxKeys` setter (`S3Request.cs` 125-136) throws `ArgumentOutOfRangeException` while the query string is applied (line 697). A non-numeric `max-keys=abc` goes the other way: `TryRetrieveQueryInt` (638-647) quietly ignores it and lists 1,000 keys.

Amazon S3 answers both with `400 InvalidArgument`. The fix has two parts:

1. Parsing errors caused by the client become `S3Exception(new Error(ErrorCode.InvalidArgument))` with a message naming the parameter. That covers negative or non-numeric `max-keys`, `max-parts`, `part-number-marker` and `partNumber`. Throw from `S3Request` where the value is parsed, not from the property setter, so programmatic use of the setters keeps its current `ArgumentOutOfRangeException` contract.
2. In `RequestHandler`, when `s3ctx` is `null` in either `catch`, write the S3 XML error directly on the Watson context (`ctx.Response`): status from the `S3Exception` (or 500 for anything else), content type `application/xml`, body `SerializationHelper.SerializeXml(error)`. No request may ever reach Watson's default error page.

Tests (`ParserRoutingTests.cs`, `ErrorHandlingTests.cs`):

- `max-keys=-1`, `max-keys=abc`, `max-parts=-5` and `partNumber=x` each return 400, an XML body, and `<Code>InvalidArgument</Code>`.
- `max-keys=0` and `max-keys=1000` still route normally.
- A fault injected into `S3Context` construction (use a request type the parser cannot handle, or a test-only hook) returns an XML `InternalError`, never `text/html`.

### BF-05. Malformed Range headers crash the request

`ParseRangeHeader` (`S3Request.cs` 1043-1057) splits on `-` and calls `Convert.ToInt64`. Anything it does not expect throws during context construction and lands in BF-04's HTML 500. Verified on the wire: `bytes=abc`, `bytes=0-1,4-5` and `items=0-1` all returned the HTML page.

RFC 9110 section 14.2 allows a server to ignore a Range header it cannot or will not satisfy. Amazon S3 does exactly that: it serves the full object with 200 for multiple ranges, unknown units and garbage. Replace `ParseRangeHeader` with a `TryParseRangeHeader` that returns `false` instead of throwing. When it fails, `RangeStart`, `RangeEnd` and the new `RangeSuffixLength` (BF-12) stay `null`, and the request routes as a normal `ObjectRead`. Log the ignored header at the S3 request logging level.

Treat these as unparseable: a unit other than `bytes`, more than one range, non-numeric bounds, both bounds empty, and `last < first`. The last case (for example `bytes=5-2`) is invalid under RFC 9110. Before closing this item, confirm with the AWS CLI against real S3 that S3 returns 200 with the full object for it, and record the result in the test's comment.

Tests (`ParserRoutingTests.cs`, `ObjectTests.cs`): each malformed form above returns 200 with the full body through `Object.Read`, and `Object.ReadRange` is not invoked. `bytes=0-4` and `bytes=5-` still route to `ReadRange` with 206.

### BF-06. CopyObject is routed as an empty PutObject

A `PUT /bucket/key` carrying `x-amz-copy-source` is classified as `ObjectWrite`, because nothing in `S3Request` reads that header. A consumer that does not special-case it stores a 0-byte object where the copy should be. `aws s3 mv` between two keys then deletes the source. Less3 lost data this way before it started intercepting the header. `UploadPart` with `x-amz-copy-source` (UploadPartCopy) has the same problem, and neither callback can return the XML body S3 requires (`CopyObjectResult`, `CopyPartResult`).

1. `S3Request`: parse `x-amz-copy-source` into `CopySourceBucket`, `CopySourceKey` (URL-decoded, leading `/` optional) and `CopySourceVersionId` (from `?versionId=`), and `x-amz-copy-source-range` into `CopySourceRangeStart`/`CopySourceRangeEnd`. Parse the four `x-amz-copy-source-if-*` headers into typed properties, sharing the parser from BF-13. A malformed copy source is `400 InvalidArgument`.
2. `S3RequestType`: add `ObjectCopy` (PUT, key present, copy-source header present, no `uploadId`) and `ObjectUploadPartCopy` (the same with `uploadId` and `partNumber`). Classify them before `ObjectWrite` and `ObjectUploadPart`.
3. Add `S3Objects/CopyObjectResult.cs` (`ETag`, `LastModified`) and `S3Objects/CopyPartResult.cs` (same shape), plus callbacks `ObjectCallbacks.Copy` (`Func<S3Context, Task<CopyObjectResult>>`) and `ObjectCallbacks.UploadPartCopy` (`Func<S3Context, Task<CopyPartResult>>`). `RequestHandler` serializes the result with status 200.
4. When the matching callback is not set, the request falls through to `DefaultRequestHandler` or `NotImplemented` like any other unwired operation. It must never reach `ObjectWrite`. That is a behavior change, and the CHANGELOG must say that copies used to arrive at `Object.Write` with an empty body.

Tests (`ObjectTests.cs`, `MultipartUploadTests.cs`):

- An AWS SDK `CopyObjectAsync` reaches `Object.Copy` with the parsed source (including a key containing spaces and `%2F`), and the SDK reads the returned ETag.
- A copy source with `?versionId=3` populates `CopySourceVersionId`.
- With `Object.Copy` unset, the response is `NotImplemented` and `Object.Write` is never invoked.
- `CopyPartAsync` with a range reaches `UploadPartCopy` with the range parsed.
- A plain PUT without the header still reaches `Object.Write`.

## Priority 2: Protocol Accuracy

Nothing in this group is a security problem. Each one is a place where a strict client, proxy or test harness sees something Amazon S3 would never send, and where a consumer cannot fix the response from inside a callback.

### BF-07. Responses carry no S3 XML namespace

Verified on the wire: `ListAllMyBucketsResult`, `InitiateMultipartUploadResult`, `ListMultipartUploadsResult` and `VersioningConfiguration` are all serialized as bare root elements. Amazon S3 puts every response body except `Error` in `http://s3.amazonaws.com/doc/2006-03-01/`. `SerializationHelper.SerializeXml` (lines 175-205) adds that URI as the default prefix mapping, but no type is declared in it, so the mapping never appears. The response classes carry a commented-out `// Namespace = ...` line, which suggests this was tried once and backed out. The likely reason is that putting a namespace on the root alone leaves nested types in the empty namespace, and they then serialize with `xmlns=""`.

Construct the serializer with a default namespace instead: `new XmlSerializer(t, "http://s3.amazonaws.com/doc/2006-03-01/")`. Every type and member that does not name its own namespace then lands in the S3 namespace, nested types included. Keep `Error` (and only `Error`, when it is the root) in the empty namespace, as S3 does. The serializer cache key must include that distinction. Deserialization is already namespace-agnostic (`GetNamespaceAgnosticSerializer`) and needs no change. Delete the commented-out namespace lines once the tests pass.

Tests (`SerializationTests.cs`): for every response type S3Server serializes, parse the output and assert that the root `NamespaceURI` is the S3 namespace and that no descendant has an empty namespace. A standalone `Error` has an empty namespace. An `Error` inside a `DeleteResult` is in the S3 namespace. Round-trip each type through `DeserializeXml`.

### BF-08. Error bodies include a non-standard HttpStatusCode element

`Error.HttpStatusCode` (`S3Objects/Error.cs` line 244) is serialized into every error body, including the per-key `<Error>` entries of a `DeleteResult`. S3 never sends it. Keep the property, which S3Server uses internally for the status code, but mark it `[XmlIgnore]` and `[JsonIgnore]` if it is not needed in JSON logs. Tests: a serialized `Error` contains no `HttpStatusCode` element, and `S3Exception.HttpStatusCode` still yields the right status for `NoSuchKey`, `AccessDenied` and `InternalError`.

### BF-09. DeleteResult entries serialize nil and false values S3 never sends

`Deleted` (`S3Objects/Deleted.cs`) declares `VersionId` and `DeleteMarkerVersionId` with `IsNullable = true`, so a null value is written as `<VersionId xsi:nil="true"/>`. `DeleteMarker` defaults to `false` and is always written. For a plain DeleteObjects on an unversioned bucket, S3 returns only `<Deleted><Key>k</Key></Deleted>`. Set `IsNullable = false` on all four elements and change `DeleteMarker` to default `null`. Add `ShouldSerializeVersionId()`, `ShouldSerializeDeleteMarkerVersionId()` (non-empty) and `ShouldSerializeDeleteMarker()` (value is `true`). The constructor keeps its signature.

Tests: a `Deleted` with only a key serializes to exactly `<Key>` and nothing else. One with `DeleteMarker = true` and a marker version emits both. No `xsi:nil` appears anywhere in a serialized `DeleteResult`.

### BF-10. ListBucketResult cannot express v1 and v2 pagination

`ListBucketResult` has `Marker` and `NextContinuationToken` but not `NextMarker` (v1 with a delimiter), `StartAfter` (v2 echo) or `ContinuationToken` (v2 echo of the request token). Clients paging a v1 listing with a delimiter need `NextMarker`, because the last `Contents` key is not the right resume point when the page ended on a common prefix. Add all three as optional strings with `ShouldSerialize*` returning `true` when non-empty. Also add `S3Request.StartAfter` (parsed from `start-after`) and `S3Request.FetchOwner` (`fetch-owner=true`). `ContinuationToken` is already parsed.

Tests: each new element appears only when set, and an AWS SDK `ListObjectsV2` paginator driven by a test callback that returns tokens walks every page. A v1 `ListObjects` with a delimiter resumes from `NextMarker`.

### BF-11. ListVersionsResult cannot interleave versions and delete markers

`ListVersionsResult` holds `Versions` and `DeleteMarkers` in two lists, so all `<Version>` elements are written before all `<DeleteMarker>` elements. S3 returns them in one sequence ordered by key, then newest first, and clients that rebuild a key's history from that order get it wrong. Add `List<VersionedEntity> Entries` annotated with `[XmlElement("Version", typeof(ObjectVersion))]` and `[XmlElement("DeleteMarker", typeof(DeleteMarker))]`, which serializes one mixed list in order. When `Entries` is non-empty, `ShouldSerializeVersions()` and `ShouldSerializeDeleteMarkers()` return `false`. Existing consumers that fill the two separate lists keep today's output.

Tests: a result with `Entries` = version, marker, version serializes in that order with the right element names. A result using the old lists is unchanged. Both round-trip through `DeserializeXml`.

### BF-12. Suffix ranges are served as full objects

For `Range: bytes=-N`, `ParseRangeHeader` sets `RangeEnd = N` and leaves `RangeStart` null. Classification (`S3Request.cs` line 1104) requires `_RangeStart != null`, so the request becomes `ObjectRead` and is answered 200 with the whole object. (Less3 returns 206 today only because it writes the response itself.) Encoding a suffix length in `RangeEnd` also makes the property mean two different things.

1. Add `S3Request.RangeSuffixLength` (`long?`). For `bytes=-N`, set it to `N` and leave `RangeStart` and `RangeEnd` null.
2. Classify as `ObjectReadRange` when either `RangeStart` or `RangeSuffixLength` is set.
3. In the `ObjectReadRange` response path (lines 881-888), for a suffix range compute `start = TotalSize - Size` and `end = TotalSize - 1`, and emit `Content-Range: bytes start-end/TotalSize`. `S3Object.TotalSize` is required for a suffix range. If the callback leaves it unset, log the omission and fail the request with `InternalError`, because S3Server cannot compute the header.
4. Add `S3ServerSettings.RouteSuffixRangesToReadRange` (`bool`, default `true`). When `false`, suffix ranges keep today's routing to `ObjectRead`, which lets consumers whose `ReadRange` assumes `RangeStart` is set upgrade safely. The CHANGELOG must call out the routing change and the setting.

Tests (`ObjectTests.cs`): `bytes=-3` on a 10-byte object returns 206, the body `789`, and `Content-Range: bytes 7-9/10`. A suffix range longer than the object is clamped by the callback and still produces a consistent header. A `ReadRange` callback that omits `TotalSize` for a suffix range yields `InternalError`, not a malformed header. With `RouteSuffixRangesToReadRange = false`, the request reaches `Object.Read`.

### BF-13. Conditional requests cannot return 304 Not Modified

`ErrorCode` has `PreconditionFailed` (412) but no `NotModified`, and `S3Response.Send(Error)` always writes a body. A callback evaluating `If-None-Match` or `If-Modified-Since` therefore cannot answer the way S3 does: status 304, no body, and the `ETag` and `Last-Modified` headers.

1. Add `ErrorCode.NotModified` mapped to HTTP 304. In the `S3Exception` path, when the status is 304, send headers only: no body and no `Content-Type`. Any `ETag` or `Last-Modified` headers the callback already added to `s3ctx.Response.Headers` must survive.
2. Parse `If-Match`, `If-None-Match` (comma-separated lists, `*`, weak `W/` tags), `If-Modified-Since` and `If-Unmodified-Since` (RFC 1123 dates; an unparseable date is ignored, per RFC 9110) into `S3Request` properties. Share the parser with the `x-amz-copy-source-if-*` headers in BF-06.
3. Evaluation stays in the callback, since only the consumer knows the object's ETag and date. Document the precedence rules from RFC 9110 section 13.2.2 in the property docs so every consumer implements the same order.

Tests: a callback throwing `NotModified` produces 304 with an empty body and the ETag header intact, for both GET and HEAD. A callback throwing `PreconditionFailed` produces 412 with an XML body. The header parsing covers lists, `*`, weak tags and a garbage date.

### BF-14. Watson's default request headers are echoed on every response

Verified on the wire: a HEAD for an object comes back with `Host: 127.0.0.1:8960`, `Accept: */*`, `Accept-Language: en-US, en`, `Accept-Charset: utf8` and `Cache-Control: no-cache`. These come from Watson's `WebserverSettings.Headers.DefaultHeaders`, which the `S3Server` constructor (lines 103-122) copies through, forcing `Accept-Charset: utf8` along the way (the comment says "Minio support"). `Host` and the `Accept*` headers are request headers and do not belong on a response. `Cache-Control: no-cache` actively breaks S3 semantics, because S3 returns the `Cache-Control` value stored with the object, and a blanket `no-cache` either overrides or duplicates it.

In the constructor, remove `Host`, `Accept`, `Accept-Language` and `Cache-Control` from the default headers. Keep the CORS `Access-Control-*` defaults and `Accept-Charset` for now. Add `S3ServerSettings.PreserveWebserverDefaultHeaders` (`bool`, default `false`) for consumers that rely on the old output. Before dropping `Accept-Charset` as well, run `MinioClientTest.bat` from Less3 against a build without it. If `mc` still passes, remove it in the same change and delete the "Minio support" branch.

Tests (`ProtocolCompatibilityTests.cs`): GET and HEAD responses contain none of the removed headers. A callback that sets `Cache-Control: max-age=60` produces exactly one `Cache-Control` header with that value. `PreserveWebserverDefaultHeaders = true` restores the old set.

## Priority 3: Clarity

### BF-15. Raw and decoded query values are easy to mix up

`ctx.Http.Request.Query.Elements` holds percent-encoded values (`prefix=alpha%2F` stays `alpha%2F`), while `S3Request.Prefix`, `Marker` and the other parsed properties are decoded. Less3 read `Query.Elements` directly for parameters S3Server does not parse, got encoded strings, and had to write its own decoder. `S3Request.RetrieveQueryValue` (line 598) is public, but its XML doc does not say whether the value is decoded.

Add a test that sends `?x-test=a%2Fb%20c` and pins down what `RetrieveQueryValue("x-test")` returns. If it is not `a/b c`, decode it there with `WebUtility.UrlDecode`. Then document the method as the decoded accessor, and document `Query.Elements` in README.md as raw. Tests: the value above, a value with `%2B`, and an absent key returning `null`.

## Already Fixed in 7.x

Three problems from the original Less3 report no longer reproduce and should not be reopened. `Start()` refuses `EnableSignatures` without `GetSecretKey`; BF-03 covers the one request-time path that is left. `ObjectReadRange` now returns 206 with `Content-Range` and a real total when `TotalSize` is set (7.3.1). A recognized request type with no callback now returns `NotImplemented` instead of `InvalidRequest` (lines 1137-1145).

## Proposed Release Grouping

The maintainer sets version numbers (VERSIONING.md, rule 4). The grouping below only suggests how the work splits by compatibility impact.

| Group | Items | Compatibility | Suggested bump |
|---|---|---|---|
| A. Fail-closed and crash fixes | BF-03, BF-04, BF-05, BF-08, BF-09, BF-15 | Bug fixes. Output changes only where it was wrong. | Patch |
| B. New hooks and surface | BF-01, BF-02, BF-06, BF-10, BF-11, BF-12, BF-13 | Additive API, with documented routing changes and opt-out settings | Minor |
| C. Wire-format corrections | BF-07, BF-14 | Every response changes (namespace, fewer headers). Clients should not notice, but anything that string-matches S3Server output will. | Minor, called out prominently |

Group A can ship on its own. BF-01 and BF-02 should ship together, because Less3 can only drop its pre-handler workaround once both are available.

## Less3 Follow-Up After Release

Once a release containing these fixes is published, Less3 can delete its workarounds. The list lives here so the Less3 change can be planned from this document:

- BF-01 and BF-02: register `Bucket.WriteAcl` and `Object.WriteAcl` again and stop routing ACL writes through `DefaultRequestHandler`.
- BF-06: register `Object.Copy` and `Object.UploadPartCopy` and remove the copy interception in `ObjectHandler.Write` and `ObjectHandler.UploadPart`.
- BF-09, BF-10 and BF-11: register `Bucket.Read`, `Bucket.ReadVersions` and `Object.DeleteMultiple` again and retire `S3XmlBuilder` and `ApiHandler.HandleDirectResponseRequest`.
- BF-12 and BF-13: remove the zero-length-result trick for suffix ranges and 304, and read `RangeSuffixLength` instead of interpreting `RangeEnd`.
- BF-04 and BF-15: widen the `max-keys=-1` compatibility test to expect 400, and replace `ApiHelper.QueryValue` with `S3Request.RetrieveQueryValue`.
- BF-07: drop the local-name matching in the ListMultipartUploads compatibility test.

Each removal must keep all 146 Less3 compatibility cases green on SQLite, PostgreSQL, MySQL and SQL Server. That suite is the real acceptance test for this plan, because it was written against Amazon S3's behavior, not against S3Server's.
