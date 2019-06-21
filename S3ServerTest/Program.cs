using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using S3ServerInterface;
using S3ServerInterface.S3Objects;

namespace Test
{
    class Program
    {
        static S3Server _Server;
        static bool _RunForever = true;

        static void Main(string[] args)
        {
            _Server = new S3Server("+", 8000, false, DefaultRequestHandler);
            _Server.ConsoleDebug.Exceptions = false;
            _Server.ConsoleDebug.HttpRequests = false;
            _Server.ConsoleDebug.S3Requests = false;

            _Server.PreRequestHandler = PreRequestHandler;
            _Server.PostRequestHandler = PostRequestHandler;
            _Server.DefaultRequestHandler = DefaultRequestHandler;
            
            _Server.Bucket.Delete = BucketDelete;
            _Server.Bucket.DeleteTags = BucketDeleteTags;
            _Server.Bucket.Exists = BucketExists;
            _Server.Bucket.ReadVersioning = BucketReadVersioning;
            _Server.Bucket.Read = BucketRead;
            _Server.Bucket.ReadTags = BucketReadTags;
            _Server.Bucket.WriteVersioning = BucketWriteVersioning;
            _Server.Bucket.Write = BucketWrite;
            _Server.Bucket.WriteTags = BucketWriteTags;

            _Server.Object.Delete = ObjectDelete;
            _Server.Object.DeleteMultiple = ObjectDeleteMultiple;
            _Server.Object.DeleteTags = ObjectDeleteTags;
            _Server.Object.Exists = ObjectExists;
            _Server.Object.Read = ObjectRead;
            _Server.Object.ReadAcl = ObjectReadAcl;
            _Server.Object.ReadRange = ObjectReadRange;
            _Server.Object.ReadTags = ObjectReadTags;
            _Server.Object.Write = ObjectWrite;
            _Server.Object.WriteAcl = ObjectWriteAcl;
            _Server.Object.WriteTags = ObjectWriteTags; 

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

        static S3Response PreRequestHandler(S3Request req)
        {
            Console.WriteLine("");
            Console.WriteLine("S3 Request Received");
            Console.WriteLine(req.ToString());

            S3Response resp = new S3Response(req, 200, "text/plain", null, null);
            return resp;
        }

        static bool PostRequestHandler(S3Request req, S3Response resp)
        {
            Console.WriteLine("");
            Console.WriteLine("S3 Response Sending");
            Console.WriteLine(resp.ToString());
            return true;
        }

        static S3Response DefaultRequestHandler(S3Request req)
        {
            Console.WriteLine("");
            Console.WriteLine("S3 Request Received");
            Console.WriteLine(req.ToString());

            S3Response resp = new S3Response(req, 200, "text/plain", null, null);
            return resp;
        }

        #region Bucket-APIs

        static S3Response BucketDelete(S3Request req)
        {
            Console.WriteLine("BucketDelete");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);            
        }

        static S3Response BucketExists(S3Request req)
        {
            Console.WriteLine("BucketExists");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null); 

        }

        static S3Response BucketDeleteTags(S3Request req)
        {
            Console.WriteLine("BucketDeleteTags");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response BucketReadVersioning(S3Request req)
        {
            Console.WriteLine("BucketReadVersioning");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response BucketRead(S3Request req)
        {
            Console.WriteLine("BucketRead");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response BucketReadTags(S3Request req)
        {
            Console.WriteLine("BucketReadTags");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response BucketWriteVersioning(S3Request req, VersioningConfiguration version)
        {
            Console.WriteLine("BucketWriteVersioning");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response BucketWrite(S3Request req, CreateBucketConfiguration config)
        {
            Console.WriteLine("BucketWrite");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response BucketWriteTags(S3Request req, Tagging tagging)
        {
            Console.WriteLine("BucketWriteTags");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        #endregion

        #region Object-APIs

        static S3Response ObjectDelete(S3Request req)
        {
            Console.WriteLine("ObjectDelete");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response ObjectDeleteMultiple(S3Request req, Delete delete)
        {
            Console.WriteLine("ObjectDeleteMultiple");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response ObjectDeleteTags(S3Request req)
        {
            Console.WriteLine("ObjectDeleteTags");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response ObjectExists(S3Request req)
        {
            Console.WriteLine("ObjectExists");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response ObjectRead(S3Request req)
        {
            Console.WriteLine("ObjectRead");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response ObjectReadAcl(S3Request req)
        {
            Console.WriteLine("ObjectReadAcl");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response ObjectReadRange(S3Request req)
        {
            Console.WriteLine("ObjectReadRange");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response ObjectReadTags(S3Request req)
        {
            Console.WriteLine("ObjectReadTags");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response ObjectWrite(S3Request req)
        {
            Console.WriteLine("ObjectWrite");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response ObjectWriteAcl(S3Request req, AccessControlPolicy acp)
        {
            Console.WriteLine("ObjectWriteAcl");
            Console.WriteLine(req.ToString());
            return new S3Response(req, 200, "text/plain", null, null);
        }

        static S3Response ObjectWriteTags(S3Request req, Tagging tagging)
        {
            try
            {
                Console.WriteLine("ObjectWriteTags");
                Console.WriteLine(req.ToString());
                if (tagging == null) Console.WriteLine("NULL");
                Console.WriteLine(SerializeJson(tagging, true));
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }

            return new S3Response(req, 200, "text/plain", null, null);
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
    }
}
