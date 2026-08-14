namespace Test.Shared.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using S3ServerLibrary;
    using S3ServerLibrary.S3Objects;

    /// <summary>
    /// Serialization tests that exercise the S3Objects data models and the SerializationHelper
    /// through XML/JSON round-trips.  These tests do not require a running server.
    /// </summary>
    public static class SerializationTests
    {
        /// <summary>
        /// Run all serialization tests.
        /// </summary>
        /// <param name="runner">Test runner.</param>
        /// <param name="token">Cancellation token.</param>
        public static async Task RunAllAsync(TestRunner runner, CancellationToken token = default)
        {
            #region Select-Object-Content

            await runner.RunTestAsync("SelectObjectContentRequest CSV round-trips through XML", (ct) =>
            {
                SelectObjectContentRequest request = new SelectObjectContentRequest();
                request.Expression = "SELECT * FROM S3Object";
                request.ExpressionType = ExpressionTypeEnum.SQL;
                request.RequestProgress = new RequestProgress { Enabled = true };
                request.InputSerialization = new InputSerialization
                {
                    CompressionType = CompressionTypeEnum.GZIP,
                    CSV = new CsvInputSerialization
                    {
                        AllowQuotedRecordDelimiter = true,
                        Comments = "#",
                        FieldDelimiter = ",",
                        FileHeaderInfo = FileHeaderInfoEnum.USE,
                        QuoteCharacter = "\"",
                        QuoteEscapeCharacter = "\\",
                        RecordDelimiter = "\n"
                    }
                };
                request.OutputSerialization = new OutputSerialization
                {
                    CSV = new CsvOutputSerialization
                    {
                        FieldDelimiter = ",",
                        QuoteCharacter = "\"",
                        QuoteEscapeCharacter = "\\",
                        QuoteFields = QuoteFieldsEnum.ASNEEDED,
                        RecordDelimiter = "\n"
                    }
                };
                request.ScanRange = new ScanRange { Start = 10, End = 200 };

                SelectObjectContentRequest copy = RoundTripXml(request);
                AssertHelper.AreEqual("SELECT * FROM S3Object", copy.Expression, "expression");
                AssertHelper.AreEqual(ExpressionTypeEnum.SQL, copy.ExpressionType, "expression type");
                AssertHelper.IsTrue(copy.RequestProgress.Enabled, "request progress enabled");
                AssertHelper.AreEqual(CompressionTypeEnum.GZIP, copy.InputSerialization.CompressionType, "compression");
                AssertHelper.IsNotNull(copy.InputSerialization.CSV, "input CSV");
                AssertHelper.AreEqual(FileHeaderInfoEnum.USE, copy.InputSerialization.CSV.FileHeaderInfo, "file header info");
                AssertHelper.IsNotNull(copy.OutputSerialization.CSV, "output CSV");
                AssertHelper.AreEqual(QuoteFieldsEnum.ASNEEDED, copy.OutputSerialization.CSV.QuoteFields, "quote fields");
                AssertHelper.AreEqual(10L, copy.ScanRange.Start, "scan range start");
                AssertHelper.AreEqual(200L, copy.ScanRange.End, "scan range end");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("SelectObjectContentRequest JSON and Parquet round-trip through XML", (ct) =>
            {
                SelectObjectContentRequest request = new SelectObjectContentRequest();
                request.Expression = "SELECT s.id FROM S3Object s";
                request.InputSerialization = new InputSerialization
                {
                    JSON = new JsonInputSerialization { Type = JsonTypeEnum.LINES },
                    Parquet = new ParquetInputSerialization()
                };
                request.OutputSerialization = new OutputSerialization
                {
                    JSON = new JsonOutputSerialization { RecordDelimiter = ";" }
                };

                SelectObjectContentRequest copy = RoundTripXml(request);
                AssertHelper.IsNotNull(copy.InputSerialization.JSON, "input JSON");
                AssertHelper.AreEqual(JsonTypeEnum.LINES, copy.InputSerialization.JSON.Type, "json type");
                AssertHelper.IsNotNull(copy.InputSerialization.Parquet, "input Parquet");
                AssertHelper.IsNotNull(copy.OutputSerialization.JSON, "output JSON");
                AssertHelper.AreEqual(";", copy.OutputSerialization.JSON.RecordDelimiter, "json record delimiter");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("SelectObjectContentRequest deserializes from a client request body", (ct) =>
            {
                // Mirrors the exact server code path (S3Server.cs DeserializeXml<SelectObjectContentRequest>).
                string xml =
                    "<SelectObjectContentRequest xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">"
                    + "<Expression>SELECT * FROM S3Object</Expression>"
                    + "<ExpressionType>SQL</ExpressionType>"
                    + "<RequestProgress><Enabled>true</Enabled></RequestProgress>"
                    + "<InputSerialization><CompressionType>GZIP</CompressionType>"
                    + "<CSV><FileHeaderInfo>USE</FileHeaderInfo><FieldDelimiter>,</FieldDelimiter></CSV>"
                    + "</InputSerialization>"
                    + "<OutputSerialization><JSON><RecordDelimiter>\n</RecordDelimiter></JSON></OutputSerialization>"
                    + "<ScanRange><Start>5</Start><End>50</End></ScanRange>"
                    + "</SelectObjectContentRequest>";

                SelectObjectContentRequest request = SerializationHelper.DeserializeXml<SelectObjectContentRequest>(xml);
                AssertHelper.AreEqual("SELECT * FROM S3Object", request.Expression, "expression");
                AssertHelper.AreEqual(ExpressionTypeEnum.SQL, request.ExpressionType, "expression type");
                AssertHelper.IsTrue(request.RequestProgress.Enabled, "request progress");
                AssertHelper.AreEqual(CompressionTypeEnum.GZIP, request.InputSerialization.CompressionType, "compression");
                AssertHelper.AreEqual(FileHeaderInfoEnum.USE, request.InputSerialization.CSV.FileHeaderInfo, "file header info");
                AssertHelper.AreEqual("\n", request.OutputSerialization.JSON.RecordDelimiter, "json record delimiter");
                AssertHelper.AreEqual(5L, request.ScanRange.Start, "scan range start");
                AssertHelper.AreEqual(50L, request.ScanRange.End, "scan range end");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("SelectObjectContentRequest null sub-objects fall back to defaults", (ct) =>
            {
                SelectObjectContentRequest request = new SelectObjectContentRequest();
                request.RequestProgress = null;
                request.InputSerialization = null;
                request.OutputSerialization = null;

                AssertHelper.IsNotNull(request.RequestProgress, "request progress default");
                AssertHelper.IsNotNull(request.InputSerialization, "input serialization default");
                AssertHelper.IsNotNull(request.OutputSerialization, "output serialization default");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ScanRange rejects negative start and end", (ct) =>
            {
                AssertThrows<ArgumentOutOfRangeException>(() => new ScanRange { Start = -1 }, "negative start");
                AssertThrows<ArgumentOutOfRangeException>(() => new ScanRange { End = -5 }, "negative end");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region Website-Configuration

            await runner.RunTestAsync("WebsiteConfiguration with routing rules round-trips through XML", (ct) =>
            {
                WebsiteConfiguration config = new WebsiteConfiguration(
                    new RedirectAllRequestsTo("example.com", ProtocolEnum.Https),
                    new IndexDocument("index.html"),
                    new ErrorDocument("error.html"),
                    new RoutingRules(new List<RoutingRule>
                    {
                        new RoutingRule(
                            new Condition("404", "images/"),
                            new Redirect("cdn.example.com", 302, ProtocolEnum.Https, "new-images/", null))
                    }));

                WebsiteConfiguration copy = RoundTripXml(config);
                AssertHelper.IsNotNull(copy.RedirectAllRequestsTo, "redirect all");
                AssertHelper.AreEqual("example.com", copy.RedirectAllRequestsTo.HostName, "redirect host");
                AssertHelper.AreEqual(ProtocolEnum.Https, copy.RedirectAllRequestsTo.Protocol, "redirect protocol");
                AssertHelper.AreEqual("index.html", copy.IndexDocument.Suffix, "index suffix");
                AssertHelper.AreEqual("error.html", copy.ErrorDocument.Key, "error key");
                AssertHelper.HasCount(copy.RoutingRules.Rules, 1, "routing rules");
                RoutingRule rule = copy.RoutingRules.Rules[0];
                AssertHelper.AreEqual("404", rule.Condition.HttpErrorCodeReturnedEquals, "condition http error");
                AssertHelper.AreEqual("images/", rule.Condition.KeyPrefixEquals, "condition key prefix");
                AssertHelper.AreEqual("cdn.example.com", rule.Redirect.HostName, "redirect host name");
                AssertHelper.AreEqual(302, rule.Redirect.HttpRedirectCode, "redirect code");
                AssertHelper.AreEqual("new-images/", rule.Redirect.ReplaceKeyPrefixWith, "replace key prefix");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Redirect default construction uses documented defaults", (ct) =>
            {
                Redirect redirect = new Redirect();
                AssertHelper.AreEqual(301, redirect.HttpRedirectCode, "default redirect code");
                AssertHelper.AreEqual(ProtocolEnum.Http, redirect.Protocol, "default protocol");
                Redirect copy = RoundTripXml(redirect);
                AssertHelper.AreEqual(301, copy.HttpRedirectCode, "round-trip redirect code");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("ErrorDocument rejects null or empty key", (ct) =>
            {
                AssertThrows<ArgumentNullException>(() => new ErrorDocument(null), "null error document key");
                AssertThrows<ArgumentNullException>(() => new ErrorDocument(String.Empty), "empty error document key");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region Access-Control

            await runner.RunTestAsync("AccessControlPolicy with canonical and group grantees round-trips through XML", (ct) =>
            {
                AccessControlPolicy policy = new AccessControlPolicy(
                    new Owner("owner-id", "Owner Name"),
                    new AccessControlList(new List<Grant>
                    {
                        new Grant(new CanonicalUser { ID = "user-id", DisplayName = "User" }, PermissionEnum.FullControl),
                        new Grant(new Group { URI = "http://acs.amazonaws.com/groups/global/AllUsers" }, PermissionEnum.Read)
                    }));

                AccessControlPolicy copy = RoundTripXml(policy);
                AssertHelper.IsNotNull(copy.Owner, "owner");
                AssertHelper.AreEqual("owner-id", copy.Owner.ID, "owner id");
                AssertHelper.HasCount(copy.Acl.Grants, 2, "grants");
                AssertHelper.AreEqual(PermissionEnum.FullControl, copy.Acl.Grants[0].Permission, "grant 0 permission");
                AssertHelper.AreEqual("user-id", copy.Acl.Grants[0].Grantee.ID, "grant 0 grantee id");
                AssertHelper.AreEqual(PermissionEnum.Read, copy.Acl.Grants[1].Permission, "grant 1 permission");
                AssertHelper.AreEqual("http://acs.amazonaws.com/groups/global/AllUsers", copy.Acl.Grants[1].Grantee.URI, "grant 1 uri");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Grantee subtypes set their grantee type", (ct) =>
            {
                AssertHelper.AreEqual("CanonicalUser", new CanonicalUser().GranteeType, "canonical user type");
                AssertHelper.AreEqual("Group", new Group().GranteeType, "group type");
                AssertHelper.AreEqual("AmazonCustomerByEmail", new AmazonCustomerByEmail().GranteeType, "email type");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Grantee rejects unknown grantee type", (ct) =>
            {
                AssertThrows<ArgumentException>(() => new Grantee { GranteeType = "Martian" }, "unknown grantee type");
                AssertThrows<ArgumentNullException>(() => new Grantee { GranteeType = null }, "null grantee type");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region Restore

            await runner.RunTestAsync("RestoreRequest with glacier job parameters round-trips through XML", (ct) =>
            {
                RestoreRequest request = new RestoreRequest();
                request.Days = 7;
                request.Description = "restore for audit";
                request.GlacierJobParameters = new GlacierJobParameters { Tier = RestoreTierEnum.Expedited };

                RestoreRequest copy = RoundTripXml(request);
                AssertHelper.AreEqual(7, copy.Days.Value, "days");
                AssertHelper.AreEqual("restore for audit", copy.Description, "description");
                AssertHelper.IsNotNull(copy.GlacierJobParameters, "glacier job parameters");
                AssertHelper.AreEqual(RestoreTierEnum.Expedited, copy.GlacierJobParameters.Tier.Value, "glacier tier");
                AssertHelper.AreEqual(RestoreTierEnum.Expedited, copy.EffectiveTier.Value, "effective tier from glacier");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("RestoreRequest top-level tier drives effective tier", (ct) =>
            {
                RestoreRequest request = new RestoreRequest();
                request.Tier = RestoreTierEnum.Bulk;
                AssertHelper.AreEqual(RestoreTierEnum.Bulk, request.EffectiveTier.Value, "effective tier from top-level");
                AssertHelper.IsFalse(request.HasUnsupportedRestoreSelectFields, "no unsupported fields");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("RestoreRequest rejects days less than one", (ct) =>
            {
                AssertThrows<ArgumentOutOfRangeException>(() => new RestoreRequest { Days = 0 }, "zero days");
                AssertThrows<ArgumentOutOfRangeException>(() => new RestoreRequest { Days = -3 }, "negative days");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("RestoreRequest detects unsupported restore-select fields", (ct) =>
            {
                string xml =
                    "<RestoreRequest xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">"
                    + "<Days>1</Days>"
                    + "<SelectParameters><Expression>SELECT * FROM S3Object</Expression></SelectParameters>"
                    + "</RestoreRequest>";

                RestoreRequest request = SerializationHelper.DeserializeXml<RestoreRequest>(xml);
                AssertHelper.IsTrue(request.HasUnsupportedRestoreSelectFields, "unsupported restore-select fields detected");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region Delete-Multiple

            await runner.RunTestAsync("DeleteMultiple round-trips through XML", (ct) =>
            {
                DeleteMultiple delete = new DeleteMultiple(true, new List<S3ServerLibrary.S3Objects.Object>
                {
                    new S3ServerLibrary.S3Objects.Object("key1", "version-1"),
                    new S3ServerLibrary.S3Objects.Object("key2", null)
                });

                DeleteMultiple copy = RoundTripXml(delete);
                AssertHelper.IsTrue(copy.Quiet, "quiet");
                AssertHelper.HasCount(copy.Objects, 2, "objects");
                AssertHelper.AreEqual("key1", copy.Objects[0].Key, "object 0 key");
                AssertHelper.AreEqual("version-1", copy.Objects[0].VersionId, "object 0 version");
                AssertHelper.AreEqual("key2", copy.Objects[1].Key, "object 1 key");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Object rejects null or empty key", (ct) =>
            {
                AssertThrows<ArgumentNullException>(() => new S3ServerLibrary.S3Objects.Object(null, null), "null object key");
                AssertThrows<ArgumentNullException>(() => new S3ServerLibrary.S3Objects.Object(String.Empty, null), "empty object key");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region Tagging

            await runner.RunTestAsync("Tagging round-trips through XML", (ct) =>
            {
                Tagging tagging = new Tagging(new TagSet(new List<Tag>
                {
                    new Tag { Key = "env", Value = "prod" },
                    new Tag { Key = "team", Value = "storage" }
                }));

                Tagging copy = RoundTripXml(tagging);
                AssertHelper.IsNotNull(copy.Tags, "tag set");
                AssertHelper.HasCount(copy.Tags.Tags, 2, "tags");
                AssertHelper.AreEqual("env", copy.Tags.Tags[0].Key, "tag 0 key");
                AssertHelper.AreEqual("prod", copy.Tags.Tags[0].Value, "tag 0 value");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region Error

            await runner.RunTestAsync("Error exposes message and HTTP status for every error code", (ct) =>
            {
                foreach (ErrorCode code in Enum.GetValues(typeof(ErrorCode)))
                {
                    Error error = new Error(code);
                    AssertHelper.IsTrue(!String.IsNullOrEmpty(error.Message), "message for " + code);
                    AssertHelper.InRange(error.HttpStatusCode, 100, 599, "status code for " + code);
                }
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Error honors custom message override", (ct) =>
            {
                Error error = new Error(ErrorCode.NoSuchKey);
                AssertHelper.AreEqual("The specified key does not exist.", error.Message, "default message");
                error.Message = "custom message";
                AssertHelper.AreEqual("custom message", error.Message, "custom message");
                AssertHelper.AreEqual(404, error.HttpStatusCode, "no such key status");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("Error ShouldSerialize helpers reflect populated fields", (ct) =>
            {
                Error empty = new Error(ErrorCode.AccessDenied);
                AssertHelper.IsFalse(empty.ShouldSerializeKey(), "empty key");
                AssertHelper.IsFalse(empty.ShouldSerializeVersionId(), "empty version id");
                AssertHelper.IsFalse(empty.ShouldSerializeRequestId(), "empty request id");
                AssertHelper.IsFalse(empty.ShouldSerializeResource(), "empty resource");
                AssertHelper.IsFalse(empty.ShouldSerializeHostId(), "empty host id");

                Error populated = new Error(ErrorCode.NoSuchKey, "the-key", "the-version", "req-1", "/bucket/the-key");
                populated.HostId = "host-1";
                AssertHelper.IsTrue(populated.ShouldSerializeKey(), "populated key");
                AssertHelper.IsTrue(populated.ShouldSerializeVersionId(), "populated version id");
                AssertHelper.IsTrue(populated.ShouldSerializeRequestId(), "populated request id");
                AssertHelper.IsTrue(populated.ShouldSerializeResource(), "populated resource");
                AssertHelper.IsTrue(populated.ShouldSerializeHostId(), "populated host id");

                Error copy = RoundTripXml(populated);
                AssertHelper.AreEqual(ErrorCode.NoSuchKey, copy.Code, "round-trip code");
                AssertHelper.AreEqual("the-key", copy.Key, "round-trip key");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion

            #region Serialization-Helper

            await runner.RunTestAsync("SerializationHelper JSON round-trip and CopyObject", (ct) =>
            {
                Owner owner = new Owner("id-1", "Display Name");
                string json = SerializationHelper.SerializeJson(owner, true);
                AssertHelper.StringContains(json, "id-1", "json id");

                Owner deserialized = SerializationHelper.DeserializeJson<Owner>(json);
                AssertHelper.AreEqual("id-1", deserialized.ID, "json id round-trip");
                AssertHelper.AreEqual("Display Name", deserialized.DisplayName, "json display name round-trip");

                Owner copy = SerializationHelper.CopyObject(owner);
                AssertHelper.AreEqual("id-1", copy.ID, "copy id");
                AssertHelper.IsNull(SerializationHelper.SerializeJson(null), "null serializes to null");
                AssertHelper.IsNull(SerializationHelper.CopyObject<Owner>(null), "copy of null is null");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("SerializationHelper deserializes namespace-agnostic XML", (ct) =>
            {
                string xml = "<Owner><ID>abc</ID><DisplayName>No Namespace</DisplayName></Owner>";
                Owner owner = SerializationHelper.DeserializeXml<Owner>(xml);
                AssertHelper.AreEqual("abc", owner.ID, "namespace-agnostic id");
                AssertHelper.AreEqual("No Namespace", owner.DisplayName, "namespace-agnostic display name");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("SerializationHelper deserializes XML from byte array", (ct) =>
            {
                string xml = SerializationHelper.SerializeXml(new Owner("bytes-id", "Bytes"));
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(xml);
                Owner owner = SerializationHelper.DeserializeXml<Owner>(bytes);
                AssertHelper.AreEqual("bytes-id", owner.ID, "byte array id");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("SerializationHelper rejects null and empty inputs", (ct) =>
            {
                AssertThrows<ArgumentNullException>(() => SerializationHelper.SerializeXml(null), "serialize null");
                AssertThrows<ArgumentNullException>(() => SerializationHelper.DeserializeXml<Owner>((string)null), "deserialize null string");
                AssertThrows<ArgumentNullException>(() => SerializationHelper.DeserializeXml<Owner>((byte[])null), "deserialize null bytes");
                AssertThrows<ArgumentNullException>(() => SerializationHelper.DeserializeXml<Owner>(String.Empty), "deserialize empty string");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("SerializationHelper throws on malformed XML", (ct) =>
            {
                AssertThrowsAny(() => SerializationHelper.DeserializeXml<Owner>("<Owner><ID>unterminated"), "malformed xml");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("SerializationHelper serializes exceptions via the exception converter", (ct) =>
            {
                string json = SerializationHelper.SerializeJson(new InvalidOperationException("boom"), false);
                AssertHelper.StringContains(json, "boom", "exception message");
                AssertHelper.StringContains(json, "Message", "exception message property");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            await runner.RunTestAsync("SerializationHelper serializes NameValueCollection via its converter", (ct) =>
            {
                System.Collections.Specialized.NameValueCollection nvc = new System.Collections.Specialized.NameValueCollection();
                nvc.Add("alpha", "1");
                nvc.Add("beta", "2");
                string json = SerializationHelper.SerializeJson(nvc, false);
                AssertHelper.StringContains(json, "alpha", "nvc key alpha");
                AssertHelper.StringContains(json, "beta", "nvc key beta");
                return Task.CompletedTask;
            }, token).ConfigureAwait(false);

            #endregion
        }

        private static T RoundTripXml<T>(T obj) where T : class
        {
            string xml = SerializationHelper.SerializeXml(obj);
            AssertHelper.IsTrue(!String.IsNullOrEmpty(xml), "serialized xml for " + typeof(T).Name);
            return SerializationHelper.DeserializeXml<T>(xml);
        }

        private static void AssertThrows<TException>(Action action, string name) where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }
            catch (Exception ex)
            {
                throw new Exception($"Expected {typeof(TException).Name} for {name}, but got {ex.GetType().Name}.");
            }

            throw new Exception($"Expected {typeof(TException).Name} for {name}, but no exception was thrown.");
        }

        private static void AssertThrowsAny(Action action, string name)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                return;
            }

            throw new Exception($"Expected an exception for {name}, but none was thrown.");
        }
    }
}
