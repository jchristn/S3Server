using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
 
using WatsonWebserver;

using S3ServerInterface.Callbacks;

namespace S3ServerInterface
{
    /// <summary>
    /// S3 server.  
    /// Bucket names must not be in the hostname; they must be in the URL path. 
    /// </summary>
    public class S3Server : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable console debugging for various items.
        /// </summary>
        public ConsoleDebugging ConsoleDebug = new ConsoleDebugging();

        /// <summary>
        /// Callback methods for requests received for service operations.
        /// </summary>
        public ServiceCallbacks Service = new ServiceCallbacks();

        /// <summary>
        /// Callback methods for requests received for bucket operations.
        /// </summary>
        public BucketCallbacks Bucket = new BucketCallbacks();

        /// <summary>
        /// Callback methods for requests received for object operations.
        /// </summary>
        public ObjectCallbacks Object = new ObjectCallbacks();

        /// <summary>
        /// Callback method to use prior to examining requests for AWS S3 APIs.
        /// Return true if you wish to terminate the request, otherwise, return false, which will further route the request.
        /// </summary>
        public Func<S3Request, S3Response, Task<bool>> PreRequestHandler = null;
         
        /// <summary>
        /// Callback method to call when no matching AWS S3 API callback could be found.
        /// This callback should return an S3Response at all times.
        /// </summary>
        public Func<S3Request, S3Response, Task> DefaultRequestHandler = null;

        /// <summary>
        /// Specify whether or not exceptions should be included in status 500 Internal Server Error messages.
        /// </summary>
        public bool SendExceptionsInResponses = true;

        #endregion

        #region Private-Members

        private bool _Disposed = false;

        private string _Hostname;
        private int _Port;
        private bool _Ssl;
        private Server _Server; 

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// Using this constructor results in no pre-request handler (your own API handler), and no custom default request handler (when an S3 API cannot be matched).
        /// </summary>
        /// <param name="hostname">The hostname on which to listen.</param>
        /// <param name="port">The TCP port number.</param>
        /// <param name="ssl">Enable or disable SSL.</param> 
        public S3Server(
            string hostname,
            int port,
            bool ssl)
        {
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0 || port > 65535) throw new ArgumentException("Port must be between 0 and 65535.");

            _Hostname = hostname;
            _Port = port;
            _Ssl = ssl;

            PreRequestHandler = null;
            DefaultRequestHandler = null;

            _Server = new Server(_Hostname, _Port, _Ssl, RequestHandler); 
        }

        /// <summary>
        /// Instantiate the object.
        /// Using this constructor results in no pre-request handler (your own API handler), but (if not null) allows a custom default request handler (when an S3 API cannot be matched).
        /// </summary>
        /// <param name="hostname">The hostname on which to listen.</param>
        /// <param name="port">The TCP port number.</param>
        /// <param name="ssl">Enable or disable SSL.</param> 
        /// <param name="defaultRequestHandler">Default request handler used when no other callbacks can be found.</param>
        public S3Server(
            string hostname,
            int port,
            bool ssl,
            Func<S3Request, S3Response, Task> defaultRequestHandler)
        {
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0 || port > 65535) throw new ArgumentException("Port must be between 0 and 65535.");

            _Hostname = hostname;
            _Port = port;
            _Ssl = ssl;
            PreRequestHandler = null;
            DefaultRequestHandler = defaultRequestHandler;

            _Server = new Server(_Hostname, _Port, _Ssl, RequestHandler); 
        }

        /// <summary>
        /// Instantiate the object.
        /// Using this constructor results in (if not null) a pre-request handler (your own API handler), and (if not null) a custom default request handler (when an S3 API cannot be matched).
        /// </summary>
        /// <param name="hostname">The hostname on which to listen.</param>
        /// <param name="port">The TCP port number.</param>
        /// <param name="ssl">Enable or disable SSL.</param>
        /// <param name="preRequestHandler">Request handler to call prior to evaluating for S3 requests, can be null.</param>
        /// <param name="defaultRequestHandler">Default request handler used when no other callbacks can be found.</param>
        public S3Server(
            string hostname,
            int port,
            bool ssl,
            Func<S3Request, S3Response, Task<bool>> preRequestHandler,
            Func<S3Request, S3Response, Task> defaultRequestHandler)
        {
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0 || port > 65535) throw new ArgumentException("Port must be between 0 and 65535."); 

            _Hostname = hostname;
            _Port = port;
            _Ssl = ssl;
            PreRequestHandler = preRequestHandler;
            DefaultRequestHandler = defaultRequestHandler;

            _Server = new Server(_Hostname, _Port, _Ssl, RequestHandler); 
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Tear down the client and dispose of background workers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private-Methods

        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_Server != null) _Server.Dispose();
            }

            _Disposed = true;
        }

        private async Task RequestHandler(HttpContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            DateTime startTime = DateTime.Now.ToUniversalTime();

            S3Request s3req = new S3Request(ctx, ConsoleDebug.S3Requests);
            S3Response s3resp = new S3Response(s3req);
            bool success = false;

            try
            {

                if (ConsoleDebug.HttpRequests)
                {
                    Console.WriteLine(Common.SerializeJson(s3req, true));
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine(Common.SerializeJson(s3req, true));
                }

                if (PreRequestHandler != null)
                {
                    success = await PreRequestHandler(s3req, s3resp);
                    if (success)
                    {
                        await s3resp.Send();
                        return;
                    } 
                }

                switch (s3req.Method)
                {
                    case HttpMethod.HEAD:
                        #region HEAD

                        if (ctx.Request.RawUrlEntries.Count == 1)
                        { 
                            if (Bucket.Exists != null)
                            {
                                await Bucket.Exists(s3req, s3resp);
                                return;
                            } 
                        }
                        else if (ctx.Request.RawUrlEntries.Count == 2)
                        { 
                            if (Object.Exists != null)
                            {
                                await Object.Exists(s3req, s3resp);
                                return;
                            } 
                        }

                        break;

                    #endregion

                    case HttpMethod.GET:
                        #region GET

                        if (ctx.Request.RawUrlEntries.Count == 0)
                        {
                            if (Service.ListBuckets != null)
                            {
                                await Service.ListBuckets(s3req, s3resp);
                                return;
                            } 
                        }
                        else if (ctx.Request.RawUrlEntries.Count == 1)
                        {
                            if (ctx.Request.QuerystringEntries.ContainsKey("acl"))
                            {
                                if (Bucket.ReadAcl != null)
                                {
                                    await Bucket.ReadAcl(s3req, s3resp);
                                    return;
                                } 
                            }
                            else if (ctx.Request.QuerystringEntries.ContainsKey("location"))
                            {
                                if (Bucket.ReadLocation != null)
                                {
                                    await Bucket.ReadLocation(s3req, s3resp);
                                    return;
                                } 
                            }
                            else if (ctx.Request.QuerystringEntries.ContainsKey("tagging"))
                            {
                                if (Bucket.ReadTags != null)
                                {
                                    await Bucket.ReadTags(s3req, s3resp);
                                    return;
                                } 
                            }
                            else if (ctx.Request.QuerystringEntries.ContainsKey("versions"))
                            {
                                if (Bucket.ReadVersions != null)
                                {
                                    await Bucket.ReadVersions(s3req, s3resp);
                                    return;
                                } 
                            }
                            else if (ctx.Request.QuerystringEntries.ContainsKey("versioning"))
                            {
                                if (Bucket.ReadVersioning != null)
                                {
                                    await Bucket.ReadVersioning(s3req, s3resp);
                                    return;
                                } 
                            }
                            else
                            {
                                if (Bucket.Read != null)
                                {
                                    await Bucket.Read(s3req, s3resp);
                                    return;
                                } 
                            }
                        }
                        else if (ctx.Request.RawUrlEntries.Count >= 2)
                        {
                            if (ctx.Request.Headers.ContainsKey("Range"))
                            {
                                if (Object.ReadRange != null)
                                {
                                    await Object.ReadRange(s3req, s3resp);
                                    return;
                                } 
                            }
                            else if (ctx.Request.QuerystringEntries.ContainsKey("acl"))
                            {
                                if (Object.ReadAcl != null)
                                {
                                    await Object.ReadAcl(s3req, s3resp);
                                    return;
                                } 
                            }
                            else if (ctx.Request.QuerystringEntries.ContainsKey("tagging"))
                            {
                                if (Object.ReadTags != null)
                                {
                                    await Object.ReadTags(s3req, s3resp);
                                    return;
                                } 
                            }
                            else if (ctx.Request.QuerystringEntries.ContainsKey("acl"))
                            {
                                if (Object.ReadAcl != null)
                                {
                                    await Object.ReadAcl(s3req, s3resp);
                                    return;
                                } 
                            }
                            else if (ctx.Request.QuerystringEntries.ContainsKey("legal-hold"))
                            {
                                if (Object.ReadLegalHold != null)
                                {
                                    await Object.ReadLegalHold(s3req, s3resp);
                                    return;
                                } 
                            }
                            else if (ctx.Request.QuerystringEntries.ContainsKey("retention"))
                            {
                                if (Object.ReadRetention != null)
                                {
                                    await Object.ReadRetention(s3req, s3resp);
                                    return;
                                } 
                            }
                            else
                            {
                                if (Object.Read != null)
                                {
                                    await Object.Read(s3req, s3resp);
                                    return;
                                } 
                            }
                        }

                        break;

                    #endregion

                    case HttpMethod.PUT:
                        #region PUT

                        if (ctx.Request.RawUrlEntries.Count == 1)
                        {
                            if (ctx.Request.QuerystringEntries.ContainsKey("acl"))
                            {
                                if (Bucket.WriteAcl != null)
                                {
                                    await Bucket.WriteAcl(s3req, s3resp);
                                    return;
                                } 
                            }
                            else if (ctx.Request.QuerystringEntries.ContainsKey("tagging"))
                            {
                                if (Bucket.WriteTags != null)
                                {
                                    await Bucket.WriteTags(s3req, s3resp);
                                    return;
                                } 
                            }
                            else if (ctx.Request.QuerystringEntries.ContainsKey("versioning"))
                            {
                                if (Bucket.WriteVersioning != null)
                                {
                                    await Bucket.WriteVersioning(s3req, s3resp);
                                    return;
                                } 
                            }
                            else
                            {
                                if (Bucket.Write != null)
                                { 
                                    await Bucket.Write(s3req, s3resp);
                                    return;
                                } 
                            }
                        }
                        else if (ctx.Request.RawUrlEntries.Count >= 2)
                        {
                            if (ctx.Request.QuerystringEntries.ContainsKey("tagging"))
                            {
                                if (Object.WriteTags != null)
                                {
                                    await Object.WriteTags(s3req, s3resp);
                                    return;
                                } 
                            }
                            else if (ctx.Request.QuerystringEntries.ContainsKey("acl"))
                            {
                                if (Object.WriteAcl != null)
                                { 
                                    await Object.WriteAcl(s3req, s3resp);
                                    return;
                                } 
                            }
                            else if (ctx.Request.QuerystringEntries.ContainsKey("legal-hold"))
                            {
                                if (Object.WriteLegalHold != null)
                                { 
                                    await Object.WriteLegalHold(s3req, s3resp);
                                    return;
                                } 
                            }
                            else if (ctx.Request.QuerystringEntries.ContainsKey("retention"))
                            {
                                if (Object.WriteRetention != null)
                                { 
                                    await Object.WriteRetention(s3req, s3resp);
                                    return;
                                } 
                            }
                            else
                            {
                                if (Object.Write != null)
                                {
                                    await Object.Write(s3req, s3resp);
                                    return;
                                } 
                            }
                        }

                        break;

                    #endregion

                    case HttpMethod.POST:
                        #region POST

                        if (ctx.Request.RawUrlEntries.Count >= 1)
                        {
                            if (ctx.Request.QuerystringEntries.ContainsKey("delete"))
                            {
                                if (Object.DeleteMultiple != null)
                                { 
                                    await Object.DeleteMultiple(s3req, s3resp);
                                    return;
                                } 
                            }  
                        }

                        break;

                    #endregion

                    case HttpMethod.DELETE:
                        #region DELETE

                        if (ctx.Request.RawUrlEntries.Count == 1)
                        {
                            if (ctx.Request.QuerystringEntries.ContainsKey("tagging"))
                            {
                                if (Bucket.DeleteTags != null)
                                {
                                    await Bucket.DeleteTags(s3req, s3resp);
                                    return;
                                } 
                            }
                            else
                            {
                                if (Bucket.Delete != null)
                                {
                                    await Bucket.Delete(s3req, s3resp);
                                    return;
                                } 
                            }
                        }
                        else if (ctx.Request.RawUrlEntries.Count >= 2)
                        {
                            if (ctx.Request.QuerystringEntries.ContainsKey("tagging"))
                            {
                                if (Object.DeleteTags != null)
                                {
                                    await Object.DeleteTags(s3req, s3resp);
                                    return;
                                } 
                            }
                            else
                            {
                                if (Object.Delete != null)
                                {
                                    await Object.Delete(s3req, s3resp);
                                    return;
                                } 
                            }
                        } 

                        break;

                    #endregion

                    default:
                        if (DefaultRequestHandler != null)
                        {
                            await DefaultRequestHandler(s3req, s3resp);
                            return;
                        }
                        break;
                }

                await Send400Response(s3resp);
                return;
            }
            catch (Exception e)
            {
                if (ConsoleDebug.Exceptions)
                {
                    Console.WriteLine(Common.SerializeJson(e, true));
                }

                await Send500Response(s3resp, e);
                return;
            }
            finally
            { 
                if (ConsoleDebug.HttpRequests)
                {
                    Console.WriteLine(
                        ctx.Request.SourceIp + ":" +
                        ctx.Request.SourcePort + " " +
                        ctx.Request.Method.ToString() + " " +
                        ctx.Request.RawUrlWithoutQuery + " " +
                        s3resp.StatusCode + 
                        " [" + Common.TotalMsFrom(startTime) + "ms]");
                }
            }
        }

        private async Task SendUnknownEndpointResponse(S3Response resp, string endpoint)
        {
            resp.StatusCode = 400;
            resp.ContentType = "text/plain";
            await resp.Send("Unknown endpoint: " + endpoint);
        }

        private async Task Send400Response(S3Response resp)
        {
            resp.StatusCode = 400;
            resp.ContentType = "text/plain";
            await resp.Send("Bad request");
        }

        private async Task Send500Response(S3Response resp, Exception e)
        {
            resp.StatusCode = 500;
            resp.ContentLength = 0;
            resp.Data = new MemoryStream();
            resp.ContentType = "text/plain";

            string msg = null;

            if (SendExceptionsInResponses)
            {
                resp.ContentType = "application/json";
                msg = Common.SerializeJson(e, true);
            }
            else
            {
                msg = "Internal server error";
            }
             
            await resp.Send(msg);
            return;
        }

        #endregion
    }
}
