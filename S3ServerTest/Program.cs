using System;
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
            _Server = new S3Server("+", 8000, false);
            _Server.DebugHttpRequests = false;
            _Server.DebugS3RequestConstruction = false;
            _Server.RequestReceived = RequestReceived;

            while (_RunForever)
            {
                string userInput = Common.InputString("Command [? for help]:", null, false);
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

            S3Response resp = new S3Response(req, true, 200, "text/plain", null);
            return resp;
        }
    }
}
