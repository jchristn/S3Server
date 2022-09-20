using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using GetSomeInput;
using Newtonsoft.Json;
using S3ServerLibrary;
using S3ServerLibrary.S3Objects;
using WatsonWebserver;

namespace Test.Server
{
    /*
     * 
     * 
     * Note: This must be run as administrator if the S3Server constructor uses '*', '+', or '0.0.0.0' as the listener hostname.
     *       Administrator not required if using 'localhost'.
     *       S3 clients will report failed operation if interacting with this node; it returns a simple 200 to each request.
     * 
     * 
     * 
     */

    class Program
    {
        static string _Hostname = "localhost";
        static int _Port = 8000;

        static S3Server _Server = null;
        static bool _RunForever = true;
        static bool _ForcePathStyle = true;

        static string _Location = "us-west-1";
        static ObjectMetadata _ObjectMetadata = new ObjectMetadata("hello.txt", DateTime.Now, "etag", 13, new Owner("admin", "Administrator"));
        static S3Object _S3Object = new S3Object("hello.txt", "1", true, DateTime.Now, "etag", 13, new Owner("admin", "Administrator"), "Hello, world!", "text/plain");
        static Owner _Owner = new Owner("admin", "Administrator");
        static Grantee _Grantee = new Grantee("admin", "Administrator", null, "CanonicalUser", "admin@admin.com");
        static Tag _Tag = new Tag("key", "value");

        static bool _RandomizeHeadResponses = false;
        static Random _Random = new Random(Int32.MaxValue);

        static void Main(string[] args)
        {
            /*
            Console.Write("Base domain (.localhost): ");
            string baseDomain = Console.ReadLine();
            */

            _Server = new S3Server(_Hostname, _Port, false, DefaultRequestHandler);
            _Server.Logging.Exceptions = true;
            _Server.Logging.HttpRequests = false;
            _Server.Logging.S3Requests = false;
            _Server.Logger = Logger;

            if (!_ForcePathStyle)
            {
                _Server.BaseDomains.Add(".localhost");
                _Server.BaseDomains.Add(".localhost.com");
                _Server.BaseDomains.Add(".localhost1.com");
                _Server.BaseDomains.Add(".localhost2.com");
                Console.WriteLine("Server configured to use virtual hosting URLs; ensure client is configured accordingly");
            }
            else
            {
                Console.WriteLine("Server configured to use path-style URLs; ensure client is configured accordingly");
            }

            _Server.PreRequestHandler = PreRequestHandler;
            _Server.DefaultRequestHandler = DefaultRequestHandler;
            _Server.PostRequestHandler = PostRequestHandler;

            _Server.Service.ListBuckets = ListBuckets;

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
            Console.WriteLine("Listening on http://" + _Hostname + ":" + _Port);

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

        static void Menu()
        {
            Console.WriteLine("--- Available Commands ---");
            Console.WriteLine("  ?         Help, this menu");
            Console.WriteLine("  q         Quit the program");
            Console.WriteLine("  cls       Clear the screen");
        }

        #region S3-API-Handlers

        #region Pre-Post-Default

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<bool> PreRequestHandler(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Console.WriteLine(SerializationHelper.SerializeJson(ctx, true));
            return false;
        }

        static async Task DefaultRequestHandler(S3Context ctx)
        {
            Console.WriteLine("DefaultRequestHandler " + ctx.Http.Request.Method.ToString() + " " + ctx.Http.Request.Url.RawWithoutQuery);
            await ctx.Response.Send(ErrorCode.InvalidRequest);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task PostRequestHandler(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Logger("Request complete: " + ctx.Http.Request.Method.ToString() + " " + ctx.Http.Request.Url.RawWithQuery + ": " + ctx.Response.StatusCode);
        }

        #endregion

        #region Service-APIs

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<ListAllMyBucketsResult> ListBuckets(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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

        #endregion

        #region Bucket-APIs

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task BucketDelete(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketDelete: " + ctx.Request.Bucket);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task BucketDeleteTags(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketDeleteTags: " + ctx.Request.Bucket);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task BucketDeleteWebsite(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketDeleteWebsite: " + ctx.Request.Bucket);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<bool> BucketExists(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketExists: " + ctx.Request.Bucket);

            if (!_RandomizeHeadResponses) return true;

            int val = _Random.Next(100);
            if (val % 2 == 0) return true;
            else return false;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<ListBucketResult> BucketRead(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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
                "us-west-");

            return result;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<AccessControlPolicy> BucketReadAcl(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketReadAcl: " + ctx.Request.Bucket);

            AccessControlList acl = new AccessControlList(
                new List<Grant>()
                {
                    new Grant(_Grantee, "FULL_CONTROL")
                });

            AccessControlPolicy policy = new AccessControlPolicy(
                _Owner,
                acl);

            return policy;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<LocationConstraint> BucketReadLocation(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketReadLocation: " + ctx.Request.Bucket);

            return new LocationConstraint(_Location);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<BucketLoggingStatus> BucketReadLogging(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketReadLogging: " + ctx.Request.Bucket);

            BucketLoggingStatus status = new BucketLoggingStatus(new LoggingEnabled("default", "prefix", new TargetGrants()));
            return status;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<Tagging> BucketReadTags(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketReadTags: " + ctx.Request.Bucket);

            Tagging tagging = new Tagging(new TagSet(new List<Tag> { _Tag }));

            return tagging;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<VersioningConfiguration> BucketReadVersioning(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketReadVersioning: " + ctx.Request.Bucket);

            VersioningConfiguration vc = new VersioningConfiguration("Enabled", "Enabled");
            return vc;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<ListVersionsResult> BucketReadVersions(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketReadVersions: " + ctx.Request.Bucket);

            List<ObjectVersion> versions = new List<ObjectVersion>()
            {
                new ObjectVersion("version1.key", "1", true, DateTime.Now.ToUniversalTime(), "etag", 500, _Owner)
            };

            List<DeleteMarker> deleteMarkers = new List<DeleteMarker>()
            {
                new DeleteMarker("deleted1.key", "2", true, DateTime.Now.ToUniversalTime(), _Owner)
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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<WebsiteConfiguration> BucketReadWebsite(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketReadWebsite: " + ctx.Request.Bucket);

            WebsiteConfiguration website = new WebsiteConfiguration();
            website.ErrorDocument = new ErrorDocument("error.html");
            website.IndexDocument = new IndexDocument("index.html");
            website.RedirectAllRequestsTo = new RedirectAllRequestsTo("localhost", "http");
            website.RoutingRules = new RoutingRules(
                new List<RoutingRule> {
                    new RoutingRule(new Condition("400", "prefix"),
                        new Redirect("localhost", "302", "http", null, null))
                }
            );
            return website;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task BucketWriteVersioning(S3Context ctx, VersioningConfiguration ver)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketWriteVersioning: " + ctx.Request.Bucket);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task BucketWrite(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketWrite: " + ctx.Request.Bucket);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task BucketWriteAcl(S3Context ctx, AccessControlPolicy acp)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketWriteAcl: " + ctx.Request.Bucket);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task BucketWriteLogging(S3Context ctx, BucketLoggingStatus logging)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketWriteLogging: " + ctx.Request.Bucket);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task BucketWriteWebsite(S3Context ctx, WebsiteConfiguration website)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketWriteWebsite: " + ctx.Request.Bucket);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task BucketWriteTags(S3Context ctx, Tagging tags)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("BucketWriteTags: " + ctx.Request.Bucket);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

        #endregion

        #region Object-APIs

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task ObjectDelete(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("ObjectDelete: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<DeleteResult> ObjectDeleteMultiple(S3Context ctx, DeleteMultiple del)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task ObjectDeleteTags(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("ObjectDeleteTags: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<ObjectMetadata> ObjectExists(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("ObjectExists: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

            if (!_RandomizeHeadResponses) return _ObjectMetadata;

            int val = _Random.Next(100);
            if (val % 2 == 0) return _ObjectMetadata;
            else return null;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<S3Object> ObjectRead(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("ObjectRead: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

            return _S3Object;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<AccessControlPolicy> ObjectReadAcl(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("ObjectReadAcl: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

            AccessControlList acl = new AccessControlList(
                new List<Grant>()
                {
                    new Grant(_Grantee, "FULL_CONTROL")
                });

            AccessControlPolicy policy = new AccessControlPolicy(
                _Owner,
                acl);

            return policy;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<LegalHold> ObjectReadLegalHold(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("ObjectReadLegalHold: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

            LegalHold legalHold = new LegalHold("OFF");

            return legalHold;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<S3Object> ObjectReadRange(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("ObjectReadRange: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

            string data = _S3Object.DataString;
            data = data.Substring((int)ctx.Request.RangeStart, (int)((int)ctx.Request.RangeEnd - (int)ctx.Request.RangeStart));
            int len = data.Length;
            _S3Object.DataString = data;
            _S3Object.Size = len;
            return _S3Object;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<Retention> ObjectReadRetention(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("ObjectReadRetention: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

            Retention ret = new Retention("GOVERNANCE", DateTime.Now.AddDays(100));
            
            return ret;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<Tagging> ObjectReadTags(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("ObjectReadTags: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

            Tagging tagging = new Tagging(new TagSet(new List<Tag> { _Tag }));

            return tagging;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task ObjectWrite(S3Context ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("ObjectWrite      : " + ctx.Request.Bucket + "/" + ctx.Request.Key);
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
                        Console.WriteLine(chunk.Length + "/" + chunk.IsFinalChunk + ": " + Encoding.UTF8.GetString(chunk.Data));
                    }
                    if (chunk.IsFinalChunk)
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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task ObjectWriteAcl(S3Context ctx, AccessControlPolicy acp)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("ObjectWriteAcl: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task ObjectWriteLegalHold(S3Context ctx, LegalHold legalHold)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("ObjectWriteLegalHold: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task ObjectWriteRetention(S3Context ctx, Retention retention)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("ObjectWriteRetention: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
            Console.WriteLine(ctx.Request.DataAsString + Environment.NewLine);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task ObjectWriteTags(S3Context ctx, Tagging tags)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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
    }
}
