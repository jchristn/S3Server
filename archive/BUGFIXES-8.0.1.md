# S3Server Bug Fix Plan (after 8.0.0)

Status: implemented in S3Server `8.0.1` (see CHANGELOG.md). BF-16 uses a virtual `ShouldSerializeStorageClass` overridden in `DeleteMarker`. BF-17 uses a `DeleteError` serialization surface behind `DeleteResult.XmlErrors`, with the Key, VersionId, Code, Message order taken from the Amazon S3 documentation. Neither order was recorded against Amazon S3, and no Compatibility scenario was added: a delete marker needs a versioned bucket, and the scenarios never change bucket settings. Wire tests through S3Server cover both instead. Baseline: S3Server `8.0.0` at commit `3603ef7`. Line numbers refer to that commit.

Less3 moved to 8.0.0 and now uses the native callbacks for ListObjects, DeleteObjects, the ACL writes, CopyObject and UploadPartCopy. Its 157-case compatibility suite passes, and so do the AWS CLI script (77 checks) and the MinIO client script (48 checks), which were run with signature validation enabled. That run found one real defect and one cosmetic difference. The defect still forces Less3 to write one response itself, so it should go first.

## Ground Rules for the Implementer

The rules in `c:\code\agents\requirements` apply, as they did for BF-01 to BF-15: the code style in `CODE_STYLE.md`, Touchstone tests in `src/Test.Shared` wired through `S3ServerSuites`, a CHANGELOG entry under a new "Unreleased" heading, no em-dashes anywhere, and no change to `<Version>` without the maintainer's decision (`VERSIONING.md`, rule 4).

## BF-16. Delete markers in ListObjectVersions carry a nil ETag and a StorageClass

A version listing that contains a delete marker is serialized like this:

```xml
<DeleteMarker><Key>k</Key><VersionId>2</VersionId><IsLatest>true</IsLatest>
  <LastModified>2026-09-30T02:53:30.650Z</LastModified>
  <ETag p3:nil="true" xmlns:p3="http://www.w3.org/2001/XMLSchema-instance"></ETag>
  <StorageClass>STANDARD</StorageClass>
  <Owner><ID>...</ID><DisplayName>...</DisplayName></Owner></DeleteMarker>
```

Amazon S3 sends exactly `Key`, `VersionId`, `IsLatest`, `LastModified` and `Owner` for a delete marker. The extra elements come from the shared base class. `S3Objects/VersionedEntity.cs` declares `ETag` with `[XmlElement(ElementName = "ETag", IsNullable = true)]`, so a null value becomes an `xsi:nil` element, and it writes `StorageClass` unconditionally. `DeleteMarker` only sets `ETag` and `Size` to null. `Size` is already handled by `ShouldSerializeSize()`, which is why it does not appear. The 63 Compatibility scenarios added in 8.0.0 do not list a delete marker, which is how the defect got past them.

The fix belongs on the base class, so both entry types get it and the `Entries`, `Versions` and `DeleteMarkers` paths all behave the same:

1. Change `ETag` to `IsNullable = false` and add `ShouldSerializeETag()` returning `!String.IsNullOrEmpty(ETag)`.
2. Add `ShouldSerializeStorageClass()` to `VersionedEntity` returning `!(this is DeleteMarker)`. A virtual method overridden in `DeleteMarker` works too and reads better. Pick one and document it.
3. Check `IsLatest` and `Owner`, which are also `IsNullable = true`. Amazon S3 always sends both, so leave them, but make sure a null `Owner` produces no element rather than `xsi:nil`, with a matching `ShouldSerializeOwner()`.

Tests (`ResponseSerializationFixTests.cs` and the Compatibility suite):

- A `DeleteMarker` serializes to exactly `Key`, `VersionId`, `IsLatest`, `LastModified` and `Owner`, in that order, and the output contains no `nil`.
- An `ObjectVersion` still serializes `ETag`, `Size` and `StorageClass`.
- A `ListVersionsResult` whose `Entries` interleave versions and delete markers keeps its order, and each entry has its own correct shape.
- A new Compatibility scenario deletes an object in a versioned bucket, lists its versions, and asserts the delete-marker shape. Record the expectation against Amazon S3 with `Test.Compatibility --target s3` before relying on it.

Done when those pass, and when Less3 can register `Bucket.ReadVersions` and delete `BucketHandler.ListObjectVersionsXml` with its compatibility suite still green.

## BF-17. Per-key errors in DeleteResult list Code before Key

Cosmetic only. The `<Error>` entries inside a `DeleteResult` come out as `Code`, `Message`, `Key`, `VersionId`, because `S3Objects/Error.cs` declares `Key` and `VersionId` after `Code` and `Message`. The DeleteObjects examples in the Amazon S3 documentation show `Key` first (`<Error><Key>sample2.txt</Key><Code>AccessDenied</Code><Message>Access Denied</Message></Error>`). None of the clients exercised in this round care, since the AWS SDK for .NET, the AWS CLI and the MinIO client all read the elements by name. Less3's own tests had matched on a substring and had to be rewritten to parse the XML, though, and anything else that compares output text will trip over it the same way.

Reordering members in `Error` would also change the top-level error body, where Amazon S3 puts `Code` first. So set explicit `Order` values instead, or give `DeleteResult` its own error element type that writes `Key`, `VersionId`, `Code` and `Message` in that order. Confirm the order against Amazon S3 with `Test.Compatibility --target s3` first. If S3 turns out to send `Code` first after all, close this item with a note and no code change.

Tests: a `DeleteResult` holding an error serializes `Key` before `Code`, a standalone `Error` still starts with `Code`, and both round-trip through `DeserializeXml`.

## Proposed Release Grouping

BF-16 changes wire output that was simply wrong and adds no API, so it fits a patch release. BF-17 changes element order only, and can ride along or wait. The maintainer decides the version number.

## Less3 Follow-Up After Release

Once BF-16 ships, Less3 registers `_S3Server.Bucket.ReadVersions`, deletes `BucketHandler.ListObjectVersionsXml`, `ApiHandler.HandleDirectResponseRequest` and `Helpers/S3UrlEncoder`, and drops the direct-response call from `DefaultRequestHandler`. The acceptance check is the same as before: `S3CompatProtocol_VersionListing_DeleteMarkerShape`, the AwsCliTest "Delete marker shape" check, and the full compatibility suite on all four databases.
