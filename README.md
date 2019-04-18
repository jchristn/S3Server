# S3ServerInterface

Simple S3 server-side interface, produced using Amazon's public documentation.  

## Examples

Refer to ```S3ClientTest``` and ```S3ServerTest``` projects for full examples.

## Notes

IP addresses are not supported for the hostname.  You must use ```localhost``` or a DNS FQDN.

## Server
```
using S3ServerInterface;

_Server = new S3Server("+", 8000, false); // host, port, SSL
_Server.RequestReceived = RequestReceived;

static S3Response RequestReceived(S3Request req)
{
   // original request, success, status code, content-type, headers, data
   return new S3Response(req, true, 200, "text/plain", null, "Hello, world!");
}
```
