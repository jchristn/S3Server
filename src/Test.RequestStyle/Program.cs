namespace Test.RequestStyle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection.Metadata;
    using System.Threading.Tasks;
    using S3ServerLibrary;
    using RestWrapper;

    static class Program
    {
        static S3ServerSettings _Settings = new S3ServerSettings();
        static S3Server _Server;

        static List<string> _Hostnames = new List<string>
        {
            "s3.local.gd",
            "s3.fbi.com"
        };

        static List<string> _Buckets = new List<string>
        {
            "bucket1",
            "bucket2",
        };

        static List<string> _Keys = new List<string>
        {
            "key1",
            "key2",
        };

        static void Main(string[] args)
        {
            Console.WriteLine("");
            Console.WriteLine("This test must be run as administrator");
            Console.WriteLine("");

            _Settings.Webserver.Hostname = "*";
            _Settings.Webserver.Port = 8000;
            _Settings.Webserver.Ssl.Enable = false;

            _Settings.Logging.HttpRequests = false;
            _Settings.Logging.S3Requests = false;
            _Settings.Logger = Logger;
            _Settings.DefaultRequestHandler = DefaultRequestHandler;

            _Server = new S3Server(_Settings);

            _Server.Start();

            string url = null;

            // With base domain finder
            _Server.Service.FindMatchingBaseDomain = BaseDomainFinder;
            
            Console.WriteLine("Testing with base domain finder");
            foreach (string hostname in _Hostnames)
            {
                foreach (string bucket in _Buckets)
                {
                    url = "http://" + bucket + "." + hostname + ":" + _Settings.Webserver.Port + "/";
                    Console.WriteLine("");
                    Console.WriteLine("  Using URL " + url);
                    SendRequest(url);

                    foreach (string key in _Keys)
                    {
                        url = "http://" + bucket + "." + hostname + ":" + _Settings.Webserver.Port + "/" + key;
                        Console.WriteLine("");
                        Console.WriteLine("  Using URL " + url);
                        SendRequest(url);
                    }
                }
            }

            // Without base domain finder
            _Server.Service.FindMatchingBaseDomain = null;

            Console.WriteLine("");
            Console.WriteLine("Testing without base domain finder");
            foreach (string hostname in _Hostnames)
            {
                foreach (string bucket in _Buckets)
                {
                    url = "http://" + hostname + ":" + _Settings.Webserver.Port + "/" + bucket;
                    Console.WriteLine("");
                    Console.WriteLine("  Using URL " + url);
                    SendRequest(url);

                    foreach (string key in _Keys)
                    {
                        url = "http://" + hostname + ":" + _Settings.Webserver.Port + "/" + bucket + "/" + key;
                        Console.WriteLine("");
                        Console.WriteLine("  Using URL " + url);
                        SendRequest(url);
                    }
                }
            }
        }

        static void SendRequest(string url)
        {
            RestRequest req = new RestRequest(url, HttpMethod.Get);
            req.Send(Array.Empty<byte>());
        }

        static async Task DefaultRequestHandler(S3Context ctx)
        {
            
            Console.WriteLine("    Request Style  : " + ctx.Request.RequestStyle.ToString());
            Console.WriteLine("    Request Type   : " + ctx.Request.RequestType.ToString());
            Console.WriteLine("    Full URL       : " + ctx.Http.Request.Url.Full);
            Console.WriteLine("    Host Header    : " + ctx.Http.Request.Headers.Get("Host"));
            Console.WriteLine("    Base Domain    : " + ctx.Request.BaseDomain);
            Console.WriteLine("    Bucket         : " + ctx.Request.Bucket);
            Console.WriteLine("    Key            : " + ctx.Request.Key);

            await ctx.Response.Send();
        }

        static void Logger(string msg)
        {
            if (!String.IsNullOrEmpty(msg)) Console.WriteLine(msg);
        }

        static string BaseDomainFinder(string hostname)
        {
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));

            List<string> baseDomains = new List<string>
            {
                "s3.local.gd",
                "s3.fbi.com"
            };

            List<string> matches = new List<string>();

            foreach (string domain in baseDomains)
            {
                if (hostname.Equals(domain)) return domain;

                string testDomain = "." + domain;
                if (hostname.EndsWith(testDomain)) matches.Add(domain);
            }

            if (matches.Count > 0)
            {
                return matches.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);
            }

            throw new KeyNotFoundException("A base domain could not be found for hostname '" + hostname + "'.");
        }
    }
}