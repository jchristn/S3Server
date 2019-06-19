using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WatsonWebserver;

using S3ServerInterface.Callbacks;
using S3ServerInterface.S3Objects;

namespace S3ServerInterface
{
    /// <summary>
    /// S3 server.  
    /// Bucket names must not be in the hostname; they must be in the URL path.
    /// Chunk-style encoding must be disabled.
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
        /// Return null if you wish to allow the request to proceed, otherwise, return an S3Response, which will be sent to the requestor with no further processing.
        /// </summary>
        public Func<S3Request, S3Response> PreRequestHandler = null;

        /// <summary>
        /// Callback method to call when no matching AWS S3 API callback could be found.
        /// This callback should return an S3Response at all times.
        /// </summary>
        public Func<S3Request, S3Response> DefaultRequestHandler = null;

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
            Func<S3Request, S3Response> defaultRequestHandler)
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
            Func<S3Request, S3Response> preRequestHandler,
            Func<S3Request, S3Response> defaultRequestHandler)
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

        private HttpResponse RequestHandler(HttpRequest req)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));
            DateTime startTime = DateTime.Now.ToUniversalTime();
            HttpResponse resp = new HttpResponse(req, 200, null, "text/plain", null);

            try
            {
                S3Request s3req = new S3Request(req, ConsoleDebug.S3Requests);
                S3Response s3resp = new S3Response(s3req, 400, "text/plain", null, Encoding.UTF8.GetBytes("Unknown endpoint."));

                if (ConsoleDebug.HttpRequests)
                {
                    Console.WriteLine(Common.SerializeJson(req, true));
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine(Common.SerializeJson(s3req, true));
                }

                if (PreRequestHandler != null)
                {
                    s3resp = PreRequestHandler(s3req);
                    if (s3resp == null)
                    {
                        s3resp = new S3Response(s3req, 500, "text/plain", null, Encoding.UTF8.GetBytes("Unknown endpoint."));
                    }
                    else
                    {
                        resp = s3resp.ToHttpResponse();
                        return resp;
                    }
                }

                switch (s3req.Method)
                {
                    case HttpMethod.HEAD:
                        if (req.RawUrlEntries.Count == 1)
                        { 
                            if (Bucket.Exists != null)
                            {
                                s3resp = Bucket.Exists(s3req);
                                resp = s3resp.ToHttpResponse();
                                return resp;
                            }
                            else
                            {
                                resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Bucket exists not implemented.");
                                return resp;
                            } 
                        }
                        else if (req.RawUrlEntries.Count == 2)
                        { 
                            if (Object.Exists != null)
                            {
                                s3resp = Object.Exists(s3req);
                                resp = s3resp.ToHttpResponse();
                                return resp;
                            }
                            else
                            {
                                resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Object exists not implemented.");
                                return resp;
                            } 
                        }
                        else
                        {
                            resp = new HttpResponse(req, 400, null, "text/plain", Encoding.UTF8.GetBytes("Bad request."));
                            return resp;
                        }

                    case HttpMethod.GET:
                        if (req.RawUrlEntries.Count == 0)
                        {
                            if (Service.ListBuckets != null)
                            {
                                s3resp = Service.ListBuckets(s3req);
                                resp = s3resp.ToHttpResponse();
                                return resp;
                            }
                            else
                            {
                                resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Service read not implemented.");
                                return resp;

                            }
                        }
                        else if (req.RawUrlEntries.Count == 1)
                        {
                            if (req.QuerystringEntries.ContainsKey("tagging"))
                            {
                                if (Bucket.ReadTags != null)
                                {
                                    s3resp = Bucket.ReadTags(s3req);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Bucket read tags not implemented.");
                                    return resp;
                                }
                            }
                            else if (req.QuerystringEntries.ContainsKey("versioning"))
                            {
                                if (Bucket.ReadVersioning != null)
                                {
                                    s3resp = Bucket.ReadVersioning(s3req);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Bucket get versioning not implemented.");
                                    return resp;
                                }
                            }
                            else
                            {
                                if (Bucket.Read != null)
                                {
                                    s3resp = Bucket.Read(s3req);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Bucket read not implemented.");
                                    return resp;
                                }
                            }
                        }
                        else if (req.RawUrlEntries.Count == 2)
                        {
                            if (req.Headers.ContainsKey("Range"))
                            {
                                if (Object.ReadRange != null)
                                {
                                    s3resp = Object.ReadRange(s3req);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Object read range not implemented.");
                                    return resp;
                                }
                            }
                            else if (req.QuerystringEntries.ContainsKey("tagging"))
                            {
                                if (Object.ReadTags != null)
                                {
                                    s3resp = Object.ReadTags(s3req);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Object read tags not implemented.");
                                    return resp;
                                }
                            }
                            else if (req.QuerystringEntries.ContainsKey("acl"))
                            {
                                if (Object.ReadAcl != null)
                                {
                                    s3resp = Object.ReadAcl(s3req);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Object read ACL not implemented.");
                                    return resp;
                                }
                            }
                            else if (req.QuerystringEntries.ContainsKey("legal-hold"))
                            {
                                if (Object.ReadLegalHold != null)
                                {
                                    s3resp = Object.ReadLegalHold(s3req);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Object read legal hold not implemented.");
                                    return resp;
                                }
                            }
                            else if (req.QuerystringEntries.ContainsKey("retention"))
                            {
                                if (Object.ReadRetention != null)
                                {
                                    s3resp = Object.ReadRetention(s3req);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Object read retention not implemented.");
                                    return resp;
                                }
                            }
                            else
                            {
                                if (Object.Read != null)
                                {
                                    s3resp = Object.Read(s3req);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Object read not implemented.");
                                    return resp;
                                }
                            }
                        }
                        else
                        {
                            resp = new HttpResponse(req, 400, null, "text/plain", Encoding.UTF8.GetBytes("Bad request."));
                            return resp;
                        }

                    case HttpMethod.PUT:
                        if (req.RawUrlEntries.Count == 1)
                        {
                            if (req.QuerystringEntries.ContainsKey("tagging"))
                            {
                                if (Bucket.WriteTags != null)
                                {
                                    Tagging tagging = null;
                                    if (req.Data != null && req.ContentLength > 0)
                                        tagging = Common.DeserializeXml<Tagging>(Encoding.UTF8.GetString(req.Data));

                                    s3resp = Bucket.WriteTags(s3req, tagging);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Bucket write tags not implemented.");
                                    return resp;
                                }
                            }
                            else if (req.QuerystringEntries.ContainsKey("versioning"))
                            {
                                if (Bucket.WriteVersioning != null)
                                {
                                    VersioningConfiguration versioning = null;
                                    if (req.Data != null && req.ContentLength > 0)
                                        versioning = Common.DeserializeXml<VersioningConfiguration>(Encoding.UTF8.GetString(req.Data));

                                    s3resp = Bucket.WriteVersioning(s3req, versioning);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Bucket set versioning not implemented.");
                                    return resp;
                                }
                            }
                            else
                            {
                                if (Bucket.Write != null)
                                {
                                    CreateBucketConfiguration bucket = null;
                                    if (req.Data != null && req.ContentLength > 0)
                                        bucket = Common.DeserializeXml<CreateBucketConfiguration>(Encoding.UTF8.GetString(req.Data));

                                    s3resp = Bucket.Write(s3req, bucket);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Bucket write not implemented.");
                                    return resp;
                                }
                            }
                        }
                        else if (req.RawUrlEntries.Count == 2)
                        {
                            if (req.QuerystringEntries.ContainsKey("tagging"))
                            {
                                if (Object.WriteTags != null)
                                {
                                    Tagging tagging = null;
                                    if (req.Data != null && req.ContentLength > 0)
                                        tagging = Common.DeserializeXml<Tagging>(Encoding.UTF8.GetString(req.Data));

                                    s3resp = Object.WriteTags(s3req, tagging);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Object write tags not implemented.");
                                    return resp;
                                }
                            }
                            else if (req.QuerystringEntries.ContainsKey("acl"))
                            {
                                if (Object.WriteAcl != null)
                                {
                                    AccessControlPolicy acl = null;
                                    if (req.Data != null && req.ContentLength > 0)
                                        acl = Common.DeserializeXml<AccessControlPolicy>(Encoding.UTF8.GetString(req.Data));

                                    s3resp = Object.WriteAcl(s3req, acl);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Object write ACL not implemented.");
                                    return resp;
                                }
                            }
                            else if (req.QuerystringEntries.ContainsKey("legal-hold"))
                            {
                                if (Object.WriteLegalHold != null)
                                {
                                    LegalHold legalHold = null;
                                    if (req.Data != null && req.ContentLength > 0)
                                        legalHold = Common.DeserializeXml<LegalHold>(Encoding.UTF8.GetString(req.Data));

                                    s3resp = Object.WriteLegalHold(s3req, legalHold);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Object write legal hold not implemented.");
                                    return resp;
                                }
                            }
                            else if (req.QuerystringEntries.ContainsKey("retention"))
                            {
                                if (Object.WriteRetention != null)
                                {
                                    Retention retention = null;
                                    if (req.Data != null && req.ContentLength > 0)
                                        retention = Common.DeserializeXml<Retention>(Encoding.UTF8.GetString(req.Data));

                                    s3resp = Object.WriteRetention(s3req, retention);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Object write retention not implemented.");
                                    return resp;
                                }
                            }
                            else
                            {
                                if (Object.Write != null)
                                {
                                    s3resp = Object.Write(s3req);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Object write not implemented.");
                                    return resp;
                                }
                            }
                        }
                        else
                        {
                            resp = new HttpResponse(req, 400, null, "text/plain", Encoding.UTF8.GetBytes("Bad request."));
                            return resp;
                        }

                    case HttpMethod.POST:
                        if (req.RawUrlEntries.Count == 1)
                        {
                            if (req.QuerystringEntries.ContainsKey("delete"))
                            {
                                if (Object.DeleteMultiple != null)
                                {
                                    Delete delete = null;
                                    if (req.Data != null && req.ContentLength > 0)
                                        delete = Common.DeserializeXml<Delete>(Encoding.UTF8.GetString(req.Data));

                                    s3resp = Object.DeleteMultiple(s3req, delete);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Object delete multiple not implemented.");
                                    return resp;
                                }
                            } 
                            else
                            {
                                resp = new HttpResponse(req, 400, null, "text/plain", Encoding.UTF8.GetBytes("Bad request."));
                                return resp;
                            }
                        } 
                        else
                        {
                            resp = new HttpResponse(req, 400, null, "text/plain", Encoding.UTF8.GetBytes("Bad request."));
                            return resp;
                        }

                    case HttpMethod.DELETE:
                        if (req.RawUrlEntries.Count == 1)
                        {
                            if (req.QuerystringEntries.ContainsKey("tagging"))
                            {
                                if (Bucket.DeleteTags != null)
                                {
                                    s3resp = Bucket.DeleteTags(s3req);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Bucket delete tags not implemented.");
                                    return resp;
                                }
                            }
                            else
                            {
                                if (Bucket.Delete != null)
                                {
                                    s3resp = Bucket.Delete(s3req);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Bucket delete not implemented.");
                                    return resp;
                                }
                            }
                        }
                        else if (req.RawUrlEntries.Count == 2)
                        {
                            if (req.QuerystringEntries.ContainsKey("tagging"))
                            {
                                if (Object.DeleteTags != null)
                                {
                                    s3resp = Object.DeleteTags(s3req);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Object delete tags not implemented.");
                                    return resp;
                                }
                            }
                            else
                            {
                                if (Object.Delete != null)
                                {
                                    s3resp = Object.Delete(s3req);
                                    resp = s3resp.ToHttpResponse();
                                    return resp;
                                }
                                else
                                {
                                    resp.Data = Encoding.UTF8.GetBytes("Unknown endpoint.  Object delete not implemented.");
                                    return resp;
                                }
                            }
                        }
                        else
                        {
                            resp = new HttpResponse(req, 400, null, "text/plain", Encoding.UTF8.GetBytes("Bad request."));
                            return resp;
                        } 

                    default:
                        if (DefaultRequestHandler != null)
                        {
                            S3Response defaultResp = DefaultRequestHandler(s3req);
                            resp = defaultResp.ToHttpResponse();
                        }
                        else
                        {
                            s3resp = new S3Response(s3req, 400, "text/plain", null, Encoding.UTF8.GetBytes("Unknown endpoint."));
                            resp = s3resp.ToHttpResponse();
                        }
                        return resp;
                }
            }
            catch (Exception e)
            {
                if (ConsoleDebug.Exceptions)
                {
                    Console.WriteLine(Common.SerializeJson(e, true));
                }

                resp = new HttpResponse(req, 500, null, "text/plain", null);
                return resp;
            }
            finally
            {
                if (ConsoleDebug.HttpRequests)
                {
                    Console.WriteLine(
                        req.SourceIp + ":" + 
                        req.SourcePort + " " + 
                        req.Method.ToString() + " " + 
                        req.RawUrlWithoutQuery + " " + 
                        resp.StatusCode + 
                        " [" + Common.TotalMsFrom(startTime) + "]");
                }
            }
        }

        #endregion
    }
}
