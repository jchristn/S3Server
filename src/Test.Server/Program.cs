namespace Test.Server
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using GetSomeInput;
    using S3ServerLibrary;
    using S3ServerLibrary.S3Objects;
    using WatsonWebserver;
    using WatsonWebserver.Core;

    /*
     * Note: This must be run as administrator if the S3Server constructor uses '*', '+', or '0.0.0.0' as the listener hostname.
     *       Administrator not required if using 'localhost'.
     *       S3 clients will report failed operation if interacting with this node; it returns a simple 200 to each request.
     */

    public static class Program
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        static S3ServerSettings _Settings = new S3ServerSettings();

        static S3Server _Server = null;
        static bool _RunForever = true;
        static bool _ForcePathStyle = true;
        static bool _ValidateSignatures = false;
        static bool _DebugSignatures = true;

        static string _Location = "us-west-1";
        static ObjectMetadata _ObjectMetadata = new ObjectMetadata("hello.txt", DateTime.Now, "etag", 13, new Owner("admin", "Administrator"));
        static Owner _Owner = new Owner("admin", "Administrator");
        static Grantee _Grantee = new Grantee("admin", "Administrator", null, "CanonicalUser", "admin@admin.com");
        static Tag _Tag = new Tag("key", "value");

        static string _SecretKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY";

        static bool _RandomizeHeadResponses = false;
        static Random _Random = new Random(Int32.MaxValue);

        static void Main(string[] args)
        {
            /*
            Console.Write("Base domain (.localhost): ");
            string baseDomain = Console.ReadLine();
            */

            Console.WriteLine("");
            Console.WriteLine("This program must be run as administrator");
            Console.WriteLine("");

            _Settings.Webserver.Hostname = "localhost";
            _Settings.Webserver.Port = 8000;
            _Settings.Webserver.Ssl.Enable = false;

            _Settings.Logging.Exceptions = true;
            _Settings.Logging.HttpRequests = false;
            _Settings.Logging.S3Requests = false;
            _Settings.Logger = Logger;

            _Settings.DefaultRequestHandler = DefaultRequestHandler;
            _Settings.PreRequestHandler = PreRequestHandler;
            _Settings.PostRequestHandler = PostRequestHandler;

            _Server = new S3Server(_Settings);

            if (!_ForcePathStyle)
            {
                _Server.Service.FindMatchingBaseDomain = FindMatchingBaseDomain;
                Console.WriteLine("Server configured to use virtual hosting URLs; ensure client is configured accordingly");
            }
            else
            {
                Console.WriteLine("Server configured to use path-style URLs; ensure client is configured accordingly");
            }

            if (_ValidateSignatures)
            {
                _Server.Service.GetSecretKey = GetSecretKey;
                _Server.Settings.EnableSignatures = true;

                if (_DebugSignatures)
                    _Server.Settings.Logging.SignatureV4Validation = true;
            }

            _Server.Service.ListBuckets = ListBuckets;
            _Server.Service.ServiceExists = ServiceExists;
            
            _Server.Bucket.Delete = BucketDelete;
            _Server.Bucket.DeleteTagging = BucketDeleteTags;
            _Server.Bucket.DeleteWebsite = BucketDeleteWebsite;
            _Server.Bucket.Exists = BucketExists;
            _Server.Bucket.ReadVersioning = BucketReadVersioning;
            _Server.Bucket.Read = BucketRead;
            _Server.Bucket.ReadAcl = BucketReadAcl;
            _Server.Bucket.ReadLocation = BucketReadLocation;
            _Server.Bucket.ReadLogging = BucketReadLogging;
            _Server.Bucket.ReadTagging = BucketReadTags;
            _Server.Bucket.ReadVersioning = BucketReadVersioning;
            _Server.Bucket.ReadVersions = BucketReadVersions;
            _Server.Bucket.ReadWebsite = BucketReadWebsite;
            _Server.Bucket.WriteVersioning = BucketWriteVersioning;
            _Server.Bucket.Write = BucketWrite;
            _Server.Bucket.WriteAcl = BucketWriteAcl;
            _Server.Bucket.WriteLogging = BucketWriteLogging;
            _Server.Bucket.WriteTagging = BucketWriteTags;
            _Server.Bucket.WriteVersioning = BucketWriteVersioning;
            _Server.Bucket.WriteWebsite = BucketWriteWebsite;

            _Server.Object.Delete = ObjectDelete;
            _Server.Object.DeleteMultiple = ObjectDeleteMultiple;
            _Server.Object.DeleteTagging = ObjectDeleteTags;
            _Server.Object.Exists = ObjectExists;
            _Server.Object.Read = ObjectRead;
            _Server.Object.ReadAcl = ObjectReadAcl;
            _Server.Object.ReadLegalHold = ObjectReadLegalHold;
            _Server.Object.ReadRetention = ObjectReadRetention;
            _Server.Object.ReadRange = ObjectReadRange;
            _Server.Object.ReadTagging = ObjectReadTags;
            _Server.Object.Write = ObjectWrite;
            _Server.Object.WriteAcl = ObjectWriteAcl;
            _Server.Object.WriteLegalHold = ObjectWriteLegalHold;
            _Server.Object.WriteRetention = ObjectWriteRetention;
            _Server.Object.WriteTagging = ObjectWriteTags;

            _Server.Start();
            Console.WriteLine("Listening on http://" + _Settings.Webserver.Hostname + ":" + _Settings.Webserver.Port);

            while (_RunForever)
            {
                string userInput = Inputty.GetString("Command [? for help]:", null, false);
                switch (userInput)
                {
                    case "?":
                        Menu();
                        break;

                    case "cls":
                        Console.Clear();
                        break;

                    case "q":
                        _RunForever = false;
                        break;
                }
            }
        }

        private static string GetSecretKey(S3Context context)
        {
            return _SecretKey;
        }

        static void Menu()
        {
            Console.WriteLine("--- Available Commands ---");
            Console.WriteLine("  ?         Help, this menu");
            Console.WriteLine("  q         Quit the program");
            Console.WriteLine("  cls       Clear the screen");
        }

        #region S3-API-Handlers

        #region Pre-Post-Default

        static async Task<bool> PreRequestHandler(S3Context ctx)
        {
            // Console.WriteLine(SerializationHelper.SerializeJson(ctx, true));
            return false;
        }

        static async Task DefaultRequestHandler(S3Context ctx)
        {
            Console.WriteLine("DefaultRequestHandler " + ctx.Http.Request.Method.ToString() + " " + ctx.Http.Request.Url.RawWithoutQuery);
            await ctx.Response.Send(ErrorCode.InvalidRequest);
        }

        static async Task PostRequestHandler(S3Context ctx)
        {
            Logger("Request complete: " + ctx.Http.Request.Method.ToString() + " " + ctx.Http.Request.Url.RawWithQuery + ": " + ctx.Response.StatusCode);
        }

        #endregion

        #region Service-APIs

        private static async Task<ListAllMyBucketsResult> ListBuckets(S3Context ctx)
        {
            Console.WriteLine("ListBuckets");

            ListAllMyBucketsResult result = new ListAllMyBucketsResult();
            result.Owner = new Owner("admin", "Administrator");

            List<Bucket> buckets = new List<Bucket>()
            {
                new Bucket("default", DateTime.Now)
            };

            result.Buckets = new Buckets(buckets);
            return result;
        }

        private static async Task<string> ServiceExists(S3Context context)
        {
            return "us-west-1";
        }

        private static string FindMatchingBaseDomain(string hostname)
        {
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));

            List<string> matches = new List<string>();

            if (hostname.Equals("s3.local.gd"))
            {
                return "s3.local.gd";
            }
            else
            {
                if (hostname.EndsWith(".s3.local.gd")) matches.Add("s3.local.gd");
            }

            if (matches.Count > 0)
            {
                return matches.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);
            }

            throw new KeyNotFoundException("A base domain could not be found for hostname '" + hostname + "'.");
        }

        #endregion

        #region Bucket-APIs

        static async Task BucketDelete(S3Context ctx)
        {
            Console.WriteLine("BucketDelete: " + ctx.Request.Bucket);
        }

        static async Task BucketDeleteTags(S3Context ctx)
        {
            Console.WriteLine("BucketDeleteTags: " + ctx.Request.Bucket);
        }

        static async Task BucketDeleteWebsite(S3Context ctx)
        {
            Console.WriteLine("BucketDeleteWebsite: " + ctx.Request.Bucket);
        }

        static async Task<bool> BucketExists(S3Context ctx)
        {
            Console.WriteLine("BucketExists: " + ctx.Request.Bucket);

            if (!_RandomizeHeadResponses) return true;

            int val = _Random.Next(100);
            if (val % 2 == 0) return true;
            else return false;
        }

        static async Task<ListBucketResult> BucketRead(S3Context ctx)
        {
            Console.WriteLine("BucketRead: " + ctx.Request.Bucket);

            List<ObjectMetadata> contents = new List<ObjectMetadata>()
            {
                _ObjectMetadata
            };

            ListBucketResult result = new ListBucketResult(
                "default",
                contents,
                1,
                ctx.Request.MaxKeys,
                ctx.Request.Prefix,
                ctx.Request.Marker,
                ctx.Request.Delimiter,
                false,
                null,
                null,
                _Location);

            return result;
        }

        static async Task<AccessControlPolicy> BucketReadAcl(S3Context ctx)
        {
            Console.WriteLine("BucketReadAcl: " + ctx.Request.Bucket);

            AccessControlList acl = new AccessControlList(
                new List<Grant>()
                {
                    new Grant(_Grantee, PermissionEnum.FullControl)
                });

            AccessControlPolicy policy = new AccessControlPolicy(
                _Owner,
                acl);

            return policy;
        }

        static async Task<LocationConstraint> BucketReadLocation(S3Context ctx)
        {
            Console.WriteLine("BucketReadLocation: " + ctx.Request.Bucket);

            return new LocationConstraint(_Location);
        }

        static async Task<BucketLoggingStatus> BucketReadLogging(S3Context ctx)
        {
            Console.WriteLine("BucketReadLogging: " + ctx.Request.Bucket);

            BucketLoggingStatus status = new BucketLoggingStatus(new LoggingEnabled("default", "prefix", new TargetGrants()));
            return status;
        }

        static async Task<Tagging> BucketReadTags(S3Context ctx)
        {
            Console.WriteLine("BucketReadTags: " + ctx.Request.Bucket);

            Tagging tagging = new Tagging(new TagSet(new List<Tag> { _Tag }));

            return tagging;
        }

        static async Task<VersioningConfiguration> BucketReadVersioning(S3Context ctx)
        {
            Console.WriteLine("BucketReadVersioning: " + ctx.Request.Bucket);

            VersioningConfiguration vc = new VersioningConfiguration(VersioningStatusEnum.Enabled, MfaDeleteStatusEnum.Disabled);
            return vc;
        }

        static async Task<ListVersionsResult> BucketReadVersions(S3Context ctx)
        {
            Console.WriteLine("BucketReadVersions: " + ctx.Request.Bucket);

            List<ObjectVersion> versions = new List<ObjectVersion>()
            {
                new ObjectVersion("version1.key", "1", true, DateTime.UtcNow, "etag", 500, _Owner)
            };

            List<DeleteMarker> deleteMarkers = new List<DeleteMarker>()
            {
                new DeleteMarker("deleted1.key", "2", true, DateTime.UtcNow, _Owner)
            };

            List<VersionedEntity> entities = new List<VersionedEntity>();
            entities.AddRange(deleteMarkers);
            entities.AddRange(versions);

            ListVersionsResult lvr = new ListVersionsResult(
                "default", 
                versions, 
                deleteMarkers, 
                ctx.Request.MaxKeys,
                ctx.Request.Prefix,
                ctx.Request.Marker,
                null,
                false,
                "us-west-1");

            return lvr;
        }

        static async Task<WebsiteConfiguration> BucketReadWebsite(S3Context ctx)
        {
            Console.WriteLine("BucketReadWebsite: " + ctx.Request.Bucket);

            WebsiteConfiguration website = new WebsiteConfiguration();
            website.ErrorDocument = new ErrorDocument("error.html");
            website.IndexDocument = new IndexDocument("index.html");
            website.RedirectAllRequestsTo = new RedirectAllRequestsTo("localhost", ProtocolEnum.Http);
            website.RoutingRules = new RoutingRules(
                new List<RoutingRule> {
                    new RoutingRule(new Condition("400", "prefix"),
                        new Redirect("localhost", 302, ProtocolEnum.Http, null, null))
                }
            );
            return website;
        }

        static async Task BucketWriteVersioning(S3Context ctx, VersioningConfiguration ver)
        {
            Console.WriteLine("BucketWriteVersioning: " + ctx.Request.Bucket);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

        static async Task BucketWrite(S3Context ctx)
        {
            Console.WriteLine("BucketWrite: " + ctx.Request.Bucket);
        }

        static async Task BucketWriteAcl(S3Context ctx, AccessControlPolicy acp)
        {
            Console.WriteLine("BucketWriteAcl: " + ctx.Request.Bucket);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

        static async Task BucketWriteLogging(S3Context ctx, BucketLoggingStatus logging)
        {
            Console.WriteLine("BucketWriteLogging: " + ctx.Request.Bucket);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

        static async Task BucketWriteWebsite(S3Context ctx, WebsiteConfiguration website)
        {
            Console.WriteLine("BucketWriteWebsite: " + ctx.Request.Bucket);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

        static async Task BucketWriteTags(S3Context ctx, Tagging tags)
        {
            Console.WriteLine("BucketWriteTags: " + ctx.Request.Bucket);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

        #endregion

        #region Object-APIs

        static async Task ObjectDelete(S3Context ctx)
        {
            Console.WriteLine("ObjectDelete: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
        }

        static async Task<DeleteResult> ObjectDeleteMultiple(S3Context ctx, DeleteMultiple del)
        {
            Console.WriteLine("ObjectDelete: " + ctx.Request.Bucket);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);

            DeleteResult result = new DeleteResult(
                new List<Deleted>()
                {
                    new Deleted("hello.txt", "1", false)
                },
                null);

            return result;
        }

        static async Task ObjectDeleteTags(S3Context ctx)
        {
            Console.WriteLine("ObjectDeleteTags: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
        }

        static async Task<ObjectMetadata> ObjectExists(S3Context ctx)
        {
            Console.WriteLine("ObjectExists: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

            if (!_RandomizeHeadResponses) return _ObjectMetadata;

            int val = _Random.Next(100);
            if (val % 2 == 0) return _ObjectMetadata;
            else return null;
        }

        static async Task<S3Object> ObjectRead(S3Context ctx)
        {
            Console.WriteLine("ObjectRead: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

            return new S3Object("hello.txt", "1", true, DateTime.Now, "etag", 13, new Owner("admin", "Administrator"), "Hello, world!", "text/plain");
        }

        static async Task<AccessControlPolicy> ObjectReadAcl(S3Context ctx)
        {
            Console.WriteLine("ObjectReadAcl: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

            AccessControlList acl = new AccessControlList(
                new List<Grant>()
                {
                    new Grant(_Grantee, PermissionEnum.FullControl)
                });

            AccessControlPolicy policy = new AccessControlPolicy(
                _Owner,
                acl);

            return policy;
        }

        static async Task<LegalHold> ObjectReadLegalHold(S3Context ctx)
        {
            Console.WriteLine("ObjectReadLegalHold: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

            LegalHold legalHold = new LegalHold("OFF");

            return legalHold;
        }

        static async Task<S3Object> ObjectReadRange(S3Context ctx)
        {
            Console.WriteLine("ObjectReadRange: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

            S3Object s3obj = new S3Object("hello.txt", "1", true, DateTime.Now, "etag", 13, new Owner("admin", "Administrator"), "Hello, world!", "text/plain");

            string data = s3obj.DataString;
            data = data.Substring((int)ctx.Request.RangeStart, (int)((int)ctx.Request.RangeEnd - (int)ctx.Request.RangeStart));
            int len = data.Length;
            s3obj.DataString = data;
            s3obj.Size = len;
            return s3obj;
        }

        static async Task<Retention> ObjectReadRetention(S3Context ctx)
        {
            Console.WriteLine("ObjectReadRetention: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

            Retention ret = new Retention(RetentionModeEnum.Governance, DateTime.Now.AddDays(100));
            
            return ret;
        }

        static async Task<Tagging> ObjectReadTags(S3Context ctx)
        {
            Console.WriteLine("ObjectReadTags: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

            Tagging tagging = new Tagging(new TagSet(new List<Tag> { _Tag }));

            return tagging;
        }

        static async Task ObjectWrite(S3Context ctx)
        {
            Console.WriteLine("ObjectWrite      : " + ctx.Request.Bucket + "/" + ctx.Request.Key);
            Console.WriteLine("Content type     : " + ctx.Request.ContentType);
            Console.WriteLine("Chunked transfer : " + ctx.Request.Chunked);

            if (ctx.Request.Chunked)
            {
                while (true)
                {
                    Chunk chunk = ctx.Request.ReadChunk().Result;
                    Console.WriteLine(SerializationHelper.SerializeJson(chunk, true));

                    Console.Write(chunk.Length + ": ");

                    if (chunk.Length > 0)
                    {
                        Console.WriteLine(chunk.Length + "/" + chunk.IsFinal + ": " + Encoding.UTF8.GetString(chunk.Data));
                    }
                    if (chunk.IsFinal)
                    {
                        Console.WriteLine("Final chunk encountered");
                        break;
                    }                    
                }
            }
            else
            {
                Console.WriteLine(ctx.Request.ContentLength + ": " + ctx.Request.DataAsString);
            }
        }

        static async Task ObjectWriteAcl(S3Context ctx, AccessControlPolicy acp)
        {
            Console.WriteLine("ObjectWriteAcl: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

        static async Task ObjectWriteLegalHold(S3Context ctx, LegalHold legalHold)
        {
            Console.WriteLine("ObjectWriteLegalHold: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

        static async Task ObjectWriteRetention(S3Context ctx, Retention retention)
        {
            Console.WriteLine("ObjectWriteRetention: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

        static async Task ObjectWriteTags(S3Context ctx, Tagging tags)
        {
            Console.WriteLine("ObjectWriteTags: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

        #endregion

        #endregion

        #region Misc

        private static void Logger(string msg)
        {
            Console.WriteLine(msg);
        }

        #endregion

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
