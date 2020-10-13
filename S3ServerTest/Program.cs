using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using S3ServerInterface;

namespace Test
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
        static S3Server _Server = null;
        static bool _RunForever = true;

        static void Main(string[] args)
        {
            Console.Write("Base domain (.localhost): ");
            string baseDomain = Console.ReadLine();

            Console.WriteLine("Listening on http://*:8000/");

            _Server = new S3Server("*", 8000, false, DefaultRequestHandler);
            _Server.Logging.Exceptions = true;
            _Server.Logging.HttpRequests = true;
            _Server.Logging.S3Requests = true;
            _Server.Logger = Logger;
            _Server.BaseDomain = baseDomain;

            _Server.AuthenticateSignatures = true;
            _Server.GetSecretKey = GetSecretKey;
            _Server.PreRequestHandler = PreRequestHandler;
            _Server.DefaultRequestHandler = DefaultRequestHandler;
            _Server.PostRequestHandler = PostRequestHandler;

            _Server.Service.ListBuckets = ListBuckets;

            _Server.Bucket.Delete = BucketDelete;
            _Server.Bucket.DeleteTags = BucketDeleteTags;
            _Server.Bucket.DeleteWebsite = BucketDeleteWebsite;
            _Server.Bucket.Exists = BucketExists;
            _Server.Bucket.ReadVersioning = BucketReadVersioning;
            _Server.Bucket.Read = BucketRead;
            _Server.Bucket.ReadAcl = BucketReadAcl;
            _Server.Bucket.ReadLocation = BucketReadLocation;
            _Server.Bucket.ReadLogging = BucketReadLogging;
            _Server.Bucket.ReadTags = BucketReadTags;
            _Server.Bucket.ReadVersioning = BucketReadVersioning;
            _Server.Bucket.ReadVersions = BucketReadVersions;
            _Server.Bucket.ReadWebsite = BucketReadWebsite;
            _Server.Bucket.WriteVersioning = BucketWriteVersioning;
            _Server.Bucket.Write = BucketWrite;
            _Server.Bucket.WriteAcl = BucketWriteAcl;
            _Server.Bucket.WriteLogging = BucketWriteLogging;
            _Server.Bucket.WriteTags = BucketWriteTags;
            _Server.Bucket.WriteVersioning = BucketWriteVersioning;
            _Server.Bucket.WriteWebsite = BucketWriteWebsite;

            _Server.Object.Delete = ObjectDelete;
            _Server.Object.DeleteMultiple = ObjectDeleteMultiple;
            _Server.Object.DeleteTags = ObjectDeleteTags;
            _Server.Object.Exists = ObjectExists;
            _Server.Object.Read = ObjectRead;
            _Server.Object.ReadAcl = ObjectReadAcl;
            _Server.Object.ReadLegalHold = ObjectReadLegalHold;
            _Server.Object.ReadRetention = ObjectReadRetention;
            _Server.Object.ReadRange = ObjectReadRange;
            _Server.Object.ReadTags = ObjectReadTags;
            _Server.Object.Write = ObjectWrite;
            _Server.Object.WriteAcl = ObjectWriteAcl;
            _Server.Object.WriteAcl = ObjectWriteLegalHold;
            _Server.Object.WriteAcl = ObjectWriteRetention;
            _Server.Object.WriteTags = ObjectWriteTags;

            _Server.Start();

            while (_RunForever)
            {
                string userInput = InputString("Command [? for help]:", null, false);
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

        static async Task SendResponse(S3Request req, S3Response resp, string text)
        {
            Console.WriteLine("[" + req.SourceIp + ":" + req.SourcePort + "] " + text);

            byte[] data = Encoding.UTF8.GetBytes(text);

            resp.StatusCode = 200;
            resp.ContentType = "text/plain";
            resp.ContentLength = data.Length;
            resp.Data = new MemoryStream();
            resp.Data.Write(data, 0, data.Length);
            resp.Data.Seek(0, SeekOrigin.Begin);

            await resp.Send();
            return;
        }

        static async Task SendChunkResponse(S3Request req, S3Response resp, string text)
        {
            Console.WriteLine("[" + req.SourceIp + ":" + req.SourcePort + "] " + text);

            byte[] data = Encoding.UTF8.GetBytes(text);

            resp.StatusCode = 200;
            resp.ContentType = "text/plain";
            resp.ContentLength = data.Length;

            await resp.SendFinalChunk(data);
            return;
        }

        static byte[] GetSecretKey(string accessKey)
        {
            return Encoding.UTF8.GetBytes("default");
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<bool> PreRequestHandler(S3Request req, S3Response resp)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return false;
        }

        static async Task DefaultRequestHandler(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Default request handler");
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task PostRequestHandler(S3Request req, S3Response resp)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Logger("Request complete: " + req.Method.ToString() + " " + req.RawUrl + ": " + resp.StatusCode);
        }

        #region Service-APIs

        static async Task ListBuckets(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "List buckets");
        }

        #endregion

        #region Bucket-APIs

        static async Task BucketDelete(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket delete");
        }

        static async Task BucketDeleteTags(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket delete tags");
        }

        static async Task BucketDeleteWebsite(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket delete website");
        }

        static async Task BucketExists(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket exists");
        }

        static async Task BucketRead(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket read");
        }

        static async Task BucketReadAcl(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket read ACL");
        }

        static async Task BucketReadLocation(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket read location");
        }

        static async Task BucketReadLogging(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket read logging");
        }

        static async Task BucketReadTags(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket read tags");
        }

        static async Task BucketReadVersioning(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket read versioning");
        }

        static async Task BucketReadVersions(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket read versions");
        }

        static async Task BucketReadWebsite(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket read website");
        }

        static async Task BucketWriteVersioning(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket write versioning");
        }

        static async Task BucketWrite(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket write");
        }

        static async Task BucketWriteAcl(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket write ACL");
        }

        static async Task BucketWriteLogging(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket write logging");
        }

        static async Task BucketWriteWebsite(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket write website");
        }

        static async Task BucketWriteTags(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Bucket write tags");
        }

        #endregion

        #region Object-APIs

        static async Task ObjectDelete(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Object delete");
        }

        static async Task ObjectDeleteMultiple(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Object delete multiple");
        }

        static async Task ObjectDeleteTags(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Object delete tags");
        }

        static async Task ObjectExists(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Object exists");
        }

        static async Task ObjectRead(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Object read");
        }

        static async Task ObjectReadAcl(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Object read acl");
        }

        static async Task ObjectReadLegalHold(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Object read legal hold");
        }

        static async Task ObjectReadRange(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Object read range");
        }

        static async Task ObjectReadRetention(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Object read retention");
        }

        static async Task ObjectReadTags(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Object read tags");
        }

        static async Task ObjectWrite(S3Request req, S3Response resp)
        {
            if (req.Chunked)
            {
                await SendChunkResponse(req, resp, "Object write chunked");
            }
            else
            {
                await SendResponse(req, resp, "Object write");
            }
        }

        static async Task ObjectWriteAcl(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Object write acl");
        }

        static async Task ObjectWriteLegalHold(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Object write legal hold");
        }

        static async Task ObjectWriteRetention(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Object write retention");
        }

        static async Task ObjectWriteTags(S3Request req, S3Response resp)
        {
            await SendResponse(req, resp, "Object write tags");
        }

        #endregion

        #endregion

        #region Console-Input

        static bool InputBoolean(string question, bool yesDefault)
        {
            Console.Write(question);

            if (yesDefault) Console.Write(" [Y/n]? ");
            else Console.Write(" [y/N]? ");

            string userInput = Console.ReadLine();

            if (String.IsNullOrEmpty(userInput))
            {
                if (yesDefault) return true;
                return false;
            }

            userInput = userInput.ToLower();

            if (yesDefault)
            {
                if (
                    (String.Compare(userInput, "n") == 0)
                    || (String.Compare(userInput, "no") == 0)
                   )
                {
                    return false;
                }

                return true;
            }
            else
            {
                if (
                    (String.Compare(userInput, "y") == 0)
                    || (String.Compare(userInput, "yes") == 0)
                   )
                {
                    return true;
                }

                return false;
            }
        }

        static string InputString(string question, string defaultAnswer, bool allowNull)
        {
            while (true)
            {
                Console.Write(question);

                if (!String.IsNullOrEmpty(defaultAnswer))
                {
                    Console.Write(" [" + defaultAnswer + "]");
                }

                Console.Write(" ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    if (!String.IsNullOrEmpty(defaultAnswer)) return defaultAnswer;
                    if (allowNull) return null;
                    else continue;
                }

                return userInput;
            }
        }

        static List<string> InputStringList(string question, bool allowEmpty)
        {
            List<string> ret = new List<string>();

            while (true)
            {
                Console.Write(question);

                Console.Write(" ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    if (ret.Count < 1 && !allowEmpty) continue;
                    return ret;
                }

                ret.Add(userInput);
            }
        }

        static int InputInteger(string question, int defaultAnswer, bool positiveOnly, bool allowZero)
        {
            while (true)
            {
                Console.Write(question);
                Console.Write(" [" + defaultAnswer + "] ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    return defaultAnswer;
                }

                int ret = 0;
                if (!Int32.TryParse(userInput, out ret))
                {
                    Console.WriteLine("Please enter a valid integer.");
                    continue;
                }

                if (ret == 0)
                {
                    if (allowZero)
                    {
                        return 0;
                    }
                }

                if (ret < 0)
                {
                    if (positiveOnly)
                    {
                        Console.WriteLine("Please enter a value greater than zero.");
                        continue;
                    }
                }

                return ret;
            }
        }

        #endregion

        #region Serialization

        static string SerializeJson(object obj, bool pretty)
        {
            if (obj == null) return null;
            string json;

            if (pretty)
            {
                json = JsonConvert.SerializeObject(
                  obj,
                  Newtonsoft.Json.Formatting.Indented,
                  new JsonSerializerSettings
                  {
                      NullValueHandling = NullValueHandling.Ignore,
                      DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                  });
            }
            else
            {
                json = JsonConvert.SerializeObject(obj,
                  new JsonSerializerSettings
                  {
                      NullValueHandling = NullValueHandling.Ignore,
                      DateTimeZoneHandling = DateTimeZoneHandling.Utc
                  });
            }

            return json;
        }

        #endregion

        #region Misc

        private static void Logger(string msg)
        {
            Console.WriteLine(msg);
        }

        #endregion
    }
}
