# V2 Signatures Product Plan

This document is the end-to-end plan for adding legacy Amazon S3 Signature Version 2 validation to S3Server. It is intentionally actionable: each section uses checkboxes that can be annotated during implementation, review, and release.

S3 Signature V2 is deprecated for AWS S3, but it still appears in legacy clients and S3-compatible storage ecosystems. S3Server should support it only as an explicit compatibility feature, with fail-closed validation, clear documentation, and test coverage that proves both header signatures and query-string signed URLs behave correctly.

## Current Status

Implementation update: V2 support shipped as an opt-in compatibility feature in `7.2.0`. Remaining unchecked items in this archived plan are follow-up hardening or broader compatibility work, not blockers for the current release.

- [x] `AWSSignatureGenerator` has been updated to `1.1.0`.
- [x] `AWSSignatureGenerator` 1.1.0 exposes `V2SignatureResult` for header signatures.
- [x] `AWSSignatureGenerator` 1.1.0 exposes `V2SignedUrlResult` for legacy `AWSAccessKeyId`, `Expires`, and `Signature` query-string URLs.
- [x] `Test.Shared` is now the source of truth consumed by Touchstone descriptors.
- [x] `Test.Automated` now runs shared suites through `Touchstone.Cli`.
- [x] `Test.Xunit` now runs shared suites through `Touchstone.XunitAdapter`.
- [x] `Test.Nunit` has been added and runs shared suites through `Touchstone.NunitAdapter`.
- [x] Touchstone integration uses NuGet packages only, not local project references.
- [x] Shared test code no longer writes to the console.
- [x] Existing V4, unsigned-request, unsupported-V2-query, and wildcard-host behavior has regression coverage.
- [x] S3Server validates V2 header signatures when `EnableSignatures` and `EnableSignatureV2` are both true.
- [x] S3Server validates V2 signed URLs when `EnableSignatures` and `EnableSignatureV2` are both true.

## Requirements Alignment

This work must align with `C:\code\agents\requirements`:

- [x] `BACKEND_TEST_ARCHITECTURE.md`: shared Touchstone descriptors live in `Test.Shared`; console, xUnit, and NUnit are thin hosts.
- [x] `BACKEND_TEST_ARCHITECTURE.md`: `Test.Shared` does not own console output.
- [x] `BACKEND_TEST_ARCHITECTURE.md`: `Test.Automated` uses the console runner and returns process status codes.
- [x] `BACKEND_TEST_ARCHITECTURE.md`: `Test.Xunit` and `Test.Nunit` run the same shared descriptors.
- [x] `CODE_STYLE.md`: add XML documentation for any new public settings, public enum values, and public request properties.
- [x] `CODE_STYLE.md`: keep private implementation helpers undocumented unless a local comment materially clarifies complex canonicalization logic.
- [x] `AUTHENTICATION.md`: signature validation remains fail-closed and every authenticated request is validated before route callbacks execute.
- [x] `AUTHENTICATION.md`: credential lookup remains centralized through `Service.GetSecretKey`.
- [x] `WRITING_DOCUMENTS.md`: README and CHANGELOG updates must explain the feature concretely without replacing prose with only lists.
- [x] `REPOSITORY_REQUIREMENTS.md`: repository-level README and CHANGELOG stay current for the release.

## Product Decision

The recommended product shape is opt-in legacy compatibility:

- [x] Add V2 validation without changing the callback model.
- [x] Continue using `Service.GetSecretKey(S3Context ctx)` as the only secret-key lookup hook.
- [x] Keep `S3ServerSettings.EnableSignatures` as the top-level gate for signature validation.
- [x] Add a small explicit compatibility switch, `S3ServerSettings.EnableSignatureV2`, default `false`.
- [x] When `EnableSignatures == true` and `EnableSignatureV2 == false`, keep the current V2 rejection behavior.
- [x] When `EnableSignatures == true` and `EnableSignatureV2 == true`, validate V2 header signatures and V2 signed URLs.
- [x] Do not add a new principal, token, policy, session, or authorization subsystem.
- [x] Do not add request callbacks specific to V2.
- [x] Do not treat V2 signed URLs as anonymous access. They are authenticated requests using the supplied access key.

This keeps the feature minor in product shape: the server already parses V2-looking fields and already exposes `S3SignatureVersion.Version2`. The implementation changes validation behavior behind an explicit setting rather than changing the library's overall geometry.

## Library Implementation

### Request Parsing

- [x] Add explicit query-auth detection in `S3Request.ParseHttpContext`.
- [x] Detect V2 signed URLs when query parameters include `AWSAccessKeyId`, `Expires`, and `Signature`.
- [x] Accept canonical AWS query parameter casing.
- [x] Accept lower-case variants already used internally by current parsing if Watson normalizes query keys.
- [x] Do not globally make all S3 subresource query keys case-insensitive; limit compatibility handling to auth parameter names and known S3 aliases.
- [x] Set `SignatureVersion = S3SignatureVersion.Version2` for V2 signed URLs.
- [x] Populate `AccessKey` from `AWSAccessKeyId`.
- [x] Populate `Expires` from `Expires`.
- [x] Populate `Signature` from `Signature`, after normal URL decoding.
- [x] Preserve existing V2 header parsing for `Authorization: AWS accessKey:signature`.
- [ ] Reject requests that include both `Authorization` and V2 query auth parameters unless AWS-compatible behavior is verified and deliberately documented.
- [ ] Add internal tests for query auth parsing with mixed-case, lower-case, URL-encoded signature values, empty values, and duplicate auth parameters.

### Signature Validation Flow

- [ ] Refactor the signature validation block in `S3Server.RequestHandler` into small private helpers:
  - [ ] `ValidateSignatureAsync` or `ValidateSignature`.
  - [ ] `ValidateV4Signature`.
  - [ ] `ValidateV2HeaderSignature`.
  - [ ] `ValidateV2SignedUrl`.
  - [ ] `ResolveSignatureFullUrl`.
  - [ ] `StripV2QueryAuthentication`.
- [x] Keep validation before all operation callback routing.
- [x] Continue throwing `AccessDenied` when the access key is unknown or `Service.GetSecretKey` returns null.
- [x] Continue throwing `SignatureDoesNotMatch` when a supplied signature does not match.
- [x] Throw `ExpiredToken` or the closest existing S3-compatible error for expired V2 signed URLs.
- [x] Throw `AuthorizationHeaderMalformed` or `AccessDenied` for malformed V2 auth fields; choose one behavior and document it.
- [x] Compare signatures using a constant-time helper where practical.
- [x] Do not log secret keys.
- [ ] Do not log full signed URLs at high verbosity unless the signature value is redacted.

### V2 Header Signatures

- [x] For `Authorization: AWS accessKey:signature`, construct `V2SignatureResult`.
- [x] Use the HTTP method in upper case.
- [x] Use the request URL after wildcard-host replacement, matching existing V4 behavior.
- [x] Pass the request headers into `V2SignatureResult`.
- [x] Pass the explicit bucket name when needed for virtual-hosted-style or CNAME/custom endpoint canonicalization.
- [x] Verify `Date` or `x-amz-date` handling against AWS examples.
- [x] Verify `Content-MD5` and `Content-Type` are included exactly as received.
- [ ] Verify canonicalized `x-amz-*` headers include repeated headers, whitespace normalization, and lower-case sorting.
- [ ] Verify canonicalized subresources include S3-supported subresources such as `acl`, `location`, `logging`, `uploads`, `partNumber`, `uploadId`, `versionId`, `versioning`, and `website`.
- [ ] Compare `V2SignatureResult.Signature` with `S3Request.Signature`.

### V2 Signed URLs

- [x] Parse `Expires` as Unix epoch seconds using invariant culture.
- [x] Reject missing, empty, non-numeric, zero, or negative `Expires`.
- [x] Reject signed URLs whose `Expires` is earlier than the current UTC time.
- [x] Reconstruct the unsigned URL by removing only V2 auth parameters:
  - [x] `AWSAccessKeyId`
  - [x] `Expires`
  - [x] `Signature`
- [x] Preserve all non-auth query parameters in their effective canonical form.
- [x] Preserve S3 subresource query parameters that are part of V2 canonicalized resources.
- [ ] Preserve response header override query parameters when `AWSSignatureGenerator` includes them in canonicalization.
- [x] Construct `V2SignedUrlResult` with the unsigned URL, access key, secret key, and parsed expiration.
- [x] Pass any signed `x-amz-*` headers the client must also send.
- [x] Compare `V2SignedUrlResult.Signature` with the decoded query-string `Signature`.
- [ ] Confirm that `+`, `/`, `=`, `%2B`, `%2F`, and `%3D` signature encodings all validate or fail according to AWS-compatible decoding rules.
- [x] Confirm V2 signed URLs do not require a `Date` header.

### Settings And Public Surface

- [x] Add `S3ServerSettings.EnableSignatureV2` with XML documentation.
- [x] Default `EnableSignatureV2` to `false`.
- [x] Document that `EnableSignatureV2` has no effect unless `EnableSignatures` is also `true`.
- [ ] Consider adding `LoggingSettings.SignatureV2Validation` for diagnostic parity with V4.
- [ ] If diagnostic logging is added, redact signatures and never include the secret key.
- [ ] Do not rename `EnableSignatures`; that would be a breaking change.
- [ ] Do not remove `S3SignatureVersion.Version2`; it is already part of the public shape.

## Test Architecture Plan

`Test.Shared` remains the source of truth. New V2 test coverage should be added as shared Touchstone descriptors and should run unchanged through `Test.Automated`, `Test.Xunit`, and `Test.Nunit`.

- [x] `Test.Shared` references `Touchstone.Core`.
- [x] `Test.Automated` references `Touchstone.Cli`.
- [x] `Test.Xunit` references `Touchstone.XunitAdapter`.
- [x] `Test.Nunit` references `Touchstone.NunitAdapter`.
- [x] `Test.Xunit` and `Test.Nunit` reference `Test.Shared`, not S3Server directly.
- [x] Existing shared tests run through console, xUnit, and NUnit.
- [x] Add a new shared suite named `SignatureV2Validation`.
- [x] Split current V2 parser and unsupported-behavior coverage into clear descriptor names so failures identify the specific rule.
- [x] Keep server lifecycle local to descriptors or suite hooks so discovery does not leave servers running.
- [x] Keep tests deterministic; avoid wall-clock fragility by using short-lived future expirations and explicit expired timestamps.
- [x] Serialize xUnit and NUnit adapter execution so socket-bound descriptors do not race for local listener ports.
- [ ] Add generated JSON result output to CI or local release instructions using `dotnet run --project src/Test.Automated -- --results results.json`.

## Test Coverage Additions

### Positive V2 Header Cases

- [x] Header V2 `GET /` list buckets succeeds.
- [ ] Header V2 `HEAD /bucket/key` succeeds.
- [x] Header V2 `GET /bucket/key` succeeds.
- [ ] Header V2 `PUT /bucket/key` with empty body succeeds.
- [x] Header V2 `PUT /bucket/key` with body, `Content-Type`, and `Content-MD5` succeeds.
- [ ] Header V2 `DELETE /bucket/key` succeeds.
- [ ] Header V2 bucket subresource request `GET /bucket?acl` succeeds.
- [ ] Header V2 multipart subresource request with `uploadId` and `partNumber` succeeds.
- [x] Header V2 request using `x-amz-date` and no `Date` header succeeds.
- [ ] Header V2 request with repeated `x-amz-meta-*` headers succeeds when canonicalized correctly.
- [ ] Header V2 virtual-hosted-style request succeeds.
- [ ] Header V2 path-style request succeeds.
- [ ] Header V2 wildcard-bound server request succeeds after Host header replacement.

### Positive V2 Signed URL Cases

- [x] V2 signed URL `GET /bucket/key` succeeds.
- [ ] V2 signed URL `HEAD /bucket/key` succeeds.
- [ ] V2 signed URL `PUT /bucket/key` succeeds when signed with required headers.
- [x] V2 signed URL with `acl` subresource succeeds.
- [ ] V2 signed URL with `versionId` succeeds.
- [ ] V2 signed URL with response header override query parameters succeeds if supported by the generator.
- [ ] V2 signed URL with URL-encoded object key succeeds.
- [ ] V2 signed URL with spaces and reserved characters in the key succeeds.
- [x] V2 signed URL with canonical AWS query parameter casing succeeds.
- [ ] V2 signed URL with lower-case auth parameter names succeeds only if that compatibility behavior is deliberately accepted.

### Negative And Adversarial V2 Cases

- [x] V2 header signature with wrong secret returns `SignatureDoesNotMatch`.
- [x] V2 header signature with unknown access key returns `AccessDenied` or `InvalidAccessKeyId`, matching the chosen existing convention.
- [x] V2 header signature with missing `Date` and missing `x-amz-date` fails closed.
- [ ] V2 header signature with tampered method fails.
- [ ] V2 header signature with tampered path fails.
- [ ] V2 header signature with tampered bucket fails.
- [ ] V2 header signature with tampered query subresource fails.
- [ ] V2 header signature with tampered `Content-Type` fails.
- [ ] V2 header signature with tampered `Content-MD5` fails.
- [ ] V2 header signature with tampered `x-amz-*` header fails.
- [x] V2 signed URL with expired `Expires` fails.
- [ ] V2 signed URL with non-numeric `Expires` fails.
- [ ] V2 signed URL with missing `AWSAccessKeyId` fails.
- [ ] V2 signed URL with missing `Signature` fails.
- [ ] V2 signed URL with missing `Expires` fails.
- [x] V2 signed URL with tampered object key fails.
- [ ] V2 signed URL with tampered non-auth query parameter fails when that parameter participates in canonicalization.
- [ ] V2 signed URL with duplicate `Signature` parameters fails closed.
- [ ] V2 signed URL with both `Authorization` header and query signature fails closed unless AWS-compatible precedence is deliberately implemented.
- [ ] V2 signed URL with unsigned required `x-amz-*` header fails.
- [ ] V2 signed URL replay before expiry still succeeds; document that expiry is the replay bound.
- [x] V2 request is still rejected when `EnableSignatureV2 == false`.
- [x] V2 request is still rejected when `EnableSignatures == false` does not run validation, matching current unauthenticated mode semantics.

### Regression Coverage

- [x] V4 signed SDK requests still pass after the package upgrade.
- [x] Wrong V4 secret is rejected.
- [x] Unknown V4 access key is rejected.
- [x] Unsigned requests are rejected when `EnableSignatures == true`.
- [x] Unsupported V2 signed URLs are rejected before V2 support is implemented.
- [x] Current V2 header and signed URL parser state is covered before V2 validation is implemented.
- [x] Existing V4 streaming upload coverage remains green after V2 support is added.
- [x] Existing range, restore, multipart, and error handling suites remain green after V2 support is added.
- [ ] `dotnet test src/Test.Xunit --framework net10.0` passes.
- [ ] `dotnet test src/Test.Nunit --framework net10.0` passes.
- [ ] `dotnet run --project src/Test.Automated --framework net10.0` passes.
- [ ] Release build passes for all target frameworks.

## Test Engineering Assessment

The current suite has useful broad API coverage, but V2 support needs deeper canonicalization and adversarial coverage than ordinary route testing. Signature bugs usually hide in encoding, header normalization, duplicated parameters, case handling, and path reconstruction. Positive tests alone are inadequate because a bad implementation can accept the happy path while allowing tampered requests.

- [x] Baseline test runner architecture has been improved by migrating to Touchstone.
- [x] Shared tests now execute through console, xUnit, and NUnit without duplicating test logic.
- [x] Existing negative signature tests have been tightened to assert status and error behavior instead of accepting any exception.
- [x] Missing restore body coverage has been added.
- [x] Parser/routing coverage now asserts current V2 header and V2 signed URL query extraction.
- [x] Adversarial coverage now includes duplicate query parameters, malformed numeric values, malformed ranges, and oversized decoded content length.
- [x] Lifecycle/concurrency coverage now runs through the shared Touchstone descriptors.
- [x] Add fixture-based V2 tests using canonical AWS examples where available.
- [x] Add generator-based V2 tests using `AWSSignatureGenerator` 1.1.0 for broad request combinations.
- [ ] Add hand-tampered variants for every generated positive V2 case.
- [x] Add parser-only unit tests for current query auth extraction behavior.
- [ ] Add parser-only unit tests for URL stripping and auth precedence.
- [x] Add tests for virtual-hosted-style and wildcard hostname behavior under V2, not only V4.
- [x] Add tests that assert no callback executes when signature validation fails.
- [x] Add tests that assert `Service.GetSecretKey` receives the parsed access key for both V2 headers and V2 signed URLs.

## Documentation Plan

### README

- [x] Update the feature list from "Validates AWS Signature V4" to explain V4 plus optional legacy V2.
- [x] Update the "AWS Signature Validation" section with both V4 and V2 examples.
- [x] Document `EnableSignatures`.
- [x] Document `EnableSignatureV2`.
- [x] Document that V2 is deprecated and disabled by default.
- [x] Document V2 header format: `Authorization: AWS AWSAccessKeyId:Signature`.
- [x] Document V2 signed URL format: `AWSAccessKeyId`, `Expires`, and `Signature`.
- [x] Document that expired V2 signed URLs are rejected.
- [x] Remove or revise the known limitation that says V2 is unsupported only when V2 support ships.
- [x] Update the dependency list to `AWSSignatureGenerator` 1.1.0.
- [x] Update the test project list to include `Test.Nunit` and Touchstone.

### CHANGELOG

- [x] Add an entry for the package update and test harness migration in the current patch.
- [x] Add a future entry for V2 validation support when implemented.
- [x] Call out that V2 is opt-in and disabled by default.
- [x] Call out any behavior changes for previously rejected V2 requests.

### XML Documentation

- [x] Add XML docs for `S3ServerSettings.EnableSignatureV2`.
- [ ] Add XML docs for any V2 diagnostic logging setting.
- [ ] Add XML docs for any new public request property, if one is added.
- [x] Regenerate or update packaged XML documentation if this repo maintains checked-in XML docs manually.

### V2_SIGNATURES.md

- [x] Create this plan.
- [x] Keep this plan updated as implementation progresses.
- [x] Mark completed items before release.
- [ ] Move remaining non-release work into GitHub issues if V2 ships in multiple patches.

## Versioning And Release Plan

The current package update and test-harness migration are patch-level changes because they do not add public runtime behavior. V2 validation support itself should be a minor release if a public setting is added, or a patch release only if the team decides to use existing settings with no public API additions.

- [x] Current patch should increment `7.1.0` to `7.1.1`.
- [x] Future V2 support with a new public setting should increment to `7.2.0`.
- [ ] Future V2 support without a new public setting may increment within `7.1.x`, but only if the behavior change is accepted as patch-safe.
- [x] Update `<PackageReleaseNotes>` in `S3Server.csproj` for the 7.1.1 patch.
- [x] Update README version history for the 7.1.1 patch.
- [x] Update CHANGELOG for the 7.1.1 patch.
- [x] Run Debug build before release build for the 7.1.1 patch.
- [x] Run console, xUnit, and NUnit tests before release build for the 7.1.1 patch.
- [x] Run `dotnet build src/S3Server.sln -c Release` for the 7.1.1 patch.
- [x] Confirm `S3Server.7.1.1.nupkg` and `.snupkg` are produced.
- [x] Patch-level parser, error-response, and test-coverage hardening incremented `7.1.1` to `7.1.2`.
- [x] Update `<PackageReleaseNotes>` in `S3Server.csproj` for the 7.1.2 patch.
- [x] Update README version history for the 7.1.2 patch.
- [x] Update CHANGELOG for the 7.1.2 patch.
- [x] Run console, xUnit, and NUnit tests before release build for the 7.1.2 patch.
- [x] Run XPlat Code Coverage collection through xUnit and NUnit for the 7.1.2 patch.
- [x] Run `dotnet build src/S3Server.sln -c Release` for the 7.1.2 patch.
- [x] Confirm `S3Server.7.1.2.nupkg` and `.snupkg` are produced.
- [x] Update `<PackageReleaseNotes>` in `S3Server.csproj` for the 7.2.0 minor release.
- [x] Update README version history for the 7.2.0 minor release.
- [x] Update CHANGELOG for the 7.2.0 minor release.
- [x] Run console, xUnit, and NUnit tests before release build for the 7.2.0 minor release.
- [x] Run XPlat Code Coverage collection through xUnit and NUnit for the 7.2.0 minor release.
- [x] Run `dotnet build src/S3Server.sln -c Release` for the 7.2.0 minor release.
- [x] Confirm `S3Server.7.2.0.nupkg` and `.snupkg` are produced.

## Implementation Acceptance Criteria

V2 support is ready only when all of these are true:

- [x] V2 header signatures validate when `EnableSignatures == true` and `EnableSignatureV2 == true`.
- [x] V2 signed URLs validate when `EnableSignatures == true` and `EnableSignatureV2 == true`.
- [x] V2 requests fail when `EnableSignatureV2 == false`.
- [x] Expired V2 signed URLs fail.
- [x] Tampered V2 requests fail.
- [x] Unknown V2 access keys fail.
- [x] V4 behavior is unchanged.
- [x] Unsigned requests still fail when signature validation is enabled.
- [x] All shared tests pass through `Test.Automated`, `Test.Xunit`, and `Test.Nunit`.
- [x] README, CHANGELOG, XML docs, package release notes, and this plan are updated.
- [x] Release build succeeds.
