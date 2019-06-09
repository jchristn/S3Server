using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using S3ServerInterface;

namespace Test
{
    class Program
    {
        static S3Server _Server;
        static bool _RunForever = true;

        static void Main(string[] args)
        {
            _Server = new S3Server("+", 8000, false, RequestReceived);
            _Server.ConsoleDebug.Exceptions = false;
            _Server.ConsoleDebug.HttpRequests = false;
            _Server.ConsoleDebug.S3Requests = false;

            _Server.Bucket.Delete = RequestReceived;
            _Server.Bucket.DeleteTags = RequestReceived;
            _Server.Bucket.Exists = RequestReceived;
            _Server.Bucket.GetVersioning = RequestReceived;
            _Server.Bucket.Read = RequestReceived;
            _Server.Bucket.ReadTags = RequestReceived;
            _Server.Bucket.SetVersioning = RequestReceived;
            _Server.Bucket.Write = RequestReceived;
            _Server.Bucket.WriteTags = RequestReceived;

            _Server.Object.Delete = RequestReceived;
            _Server.Object.DeleteMultiple = RequestReceived;
            _Server.Object.DeleteTags = RequestReceived;
            _Server.Object.Exists = RequestReceived;
            _Server.Object.Read = RequestReceived;
            _Server.Object.ReadAcl = RequestReceived;
            _Server.Object.ReadRange = RequestReceived;
            _Server.Object.ReadTags = RequestReceived;
            _Server.Object.Write = RequestReceived;
            _Server.Object.WriteAcl = RequestReceived;
            _Server.Object.WriteTags = RequestReceived;

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

        static S3Response RequestReceived(S3Request req)
        {
            Console.WriteLine("");
            Console.WriteLine("S3 Request Received");
            Console.WriteLine(req.ToString());

            S3Response resp = new S3Response(req, 200, "text/plain", null, null);
            return resp;
        }

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

    }
}
