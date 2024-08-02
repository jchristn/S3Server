namespace Test.Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using System.Text;

    using Amazon;
    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.S3.Model;
    using Amazon.S3.Util;

    using GetSomeInput;

    class Program
    {
        static bool _Ssl = false;
        static string _Endpoint = null;

        static string _AccessKey = "AKIAIOSFODNN7EXAMPLE";
        static string _SecretKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY";
        static string _SignatureVersion = "4";

        static bool _ForcePathStyle = true;
        static AmazonS3Config _S3Config = null;
        static IAmazonS3 _S3Client = null;
        static BasicAWSCredentials _S3Credentials = null;
        static RegionEndpoint _S3Region = RegionEndpoint.USWest1;
        static string _Bucket = null;

        static bool _RunForever = true;
        static bool _UseProxy = false;
        static string _ProxyHost = "127.0.0.1";
        static int _ProxyPort = 8888;

        static void Main(string[] args)
        {
            Initialize();

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

                    case "endpoint":
                        SetEndpoint();
                        break;

                    case "access":
                        SetAccessKey();
                        break;

                    case "secret":
                        SetSecretKey();
                        break;

                    case "region":
                        SetRegion();
                        break;

                    case "bucket":
                        SetBucket();
                        break;

                    case "init":
                        InitS3Client();
                        break;

                    #region Bucket-Commands

                    case "list buckets":
                        ListBuckets();
                        break;

                    case "list bucket":
                        ListBucket();
                        break;

                    case "write bucket":
                        WriteBucket();
                        break;

                    case "write bucket acl":
                        WriteBucketAcl();
                        break;

                    case "write bucket tags":
                        WriteBucketTags();
                        break;

                    case "read bucket acl":
                        ReadBucketAcl();
                        break;

                    case "read bucket tags":
                        ReadBucketTags();
                        break;

                    case "write bucket ver":
                        WriteBucketVersioning();
                        break;

                    case "read bucket ver":
                        ReadBucketVersioning();
                        break;

                    case "read bucket vers":
                        ReadBucketVersions();
                        break;

                    case "delete bucket":
                        DeleteBucket();
                        break;

                    case "delete bucket tags":
                        DeleteBucketTags();
                        break;

                    case "bucket exists":
                        BucketExists();
                        break;

                    #endregion

                    #region Object-Commands

                    case "write":
                        WriteObject();
                        break;

                    case "write acl":
                        WriteObjectAcl();
                        break;

                    case "write tags":
                        WriteObjectTags();
                        break;

                    case "write retention":
                        WriteObjectRetention();
                        break;

                    case "read":
                        ReadObject();
                        break;

                    case "read ver":
                        ReadObjectWithVersion();
                        break;

                    case "read acl":
                        ReadObjectAcl();
                        break;

                    case "read range":
                        ReadObjectRange();
                        break;

                    case "read tags":
                        ReadObjectTags();
                        break;

                    case "read retention":
                        ReadObjectRetention();
                        break;

                    case "delete":
                        DeleteObject();
                        break;

                    case "delete tags":
                        DeleteObjectTags();
                        break;

                    case "delete multiple":
                        DeleteMultiple();
                        break;

                    case "exists":
                        ObjectExists();
                        break;

                        #endregion
                }
            }
        }

        #region Support

        static void Initialize()
        {
            SetBucket();
            SetRegion();
            SetSsl();
            SetEndpoint();
            SetAccessKey();
            SetSecretKey();
            InitS3Client();
        }

        static void Menu()
        {
            Console.WriteLine("-- Available Commands --");
            Console.WriteLine("   ?                   Help, this menu");
            Console.WriteLine("   q                   Quit the program");
            Console.WriteLine("   cls                 Clear the screen");
            Console.WriteLine("   endpoint            Set endpoint (currently " + _Endpoint + ")");
            Console.WriteLine("   access              Set access key (currently " + _AccessKey + ")");
            Console.WriteLine("   secret              Set secret key (currently " + _SecretKey + ")");
            Console.WriteLine("   region              Set AWS region (currently " + _S3Region.ToString() + ")");
            Console.WriteLine("   bucket              Set S3 bucket (currently " + _Bucket + ")");
            Console.WriteLine("   init                Initialize client (needed after changing keys or region)");
            Console.WriteLine("");
            Console.WriteLine("-- Bucket Commands --");
            Console.WriteLine("   list buckets        List buckets");
            Console.WriteLine("   list bucket         List the contents of a bucket");
            Console.WriteLine("   write bucket        Create a bucket");
            Console.WriteLine("   write bucket acl    Write a bucket's ACL");
            Console.WriteLine("   write bucket tags   Write tags to a bucket");
            Console.WriteLine("   read bucket acl     Read a bucket's ACL");
            Console.WriteLine("   read bucket tags    Read tags from a bucket");
            Console.WriteLine("   write bucket ver    Write bucket versioning");
            Console.WriteLine("   read bucket ver     Read bucket versioning");
            Console.WriteLine("   read bucket vers    Read bucket versions");
            Console.WriteLine("   delete bucket       Delete a bucket");
            Console.WriteLine("   delete bucket tags  Delete a bucket's tags");
            Console.WriteLine("   bucket exists       Check if bucket exists");
            Console.WriteLine("");
            Console.WriteLine("-- Object Commands --");
            Console.WriteLine("   write               Write an object");
            Console.WriteLine("   write acl           Write an object's ACL");
            Console.WriteLine("   write tags          Write object tags");
            Console.WriteLine("   write retention     Write object retention");
            Console.WriteLine("   read                Read an object");
            Console.WriteLine("   read ver            Read a specific object version");
            Console.WriteLine("   read acl            Read an object's ACL");
            Console.WriteLine("   read range          Read a range of bytes from an object");
            Console.WriteLine("   read tags           Read an object's tags");
            Console.WriteLine("   read retention      Read an object's retention");
            Console.WriteLine("   delete              Delete an object");
            Console.WriteLine("   delete multiple     Delete multiple objects");
            Console.WriteLine("   delete tags         Delete tags from an object");
            Console.WriteLine("   exists              Check if object exists");
            Console.WriteLine("");
        }

        #endregion

        #region S3

        static void SetBucket()
        {
            _Bucket = Inputty.GetString("Bucket:", "default", false);
        }

        static void SetRegion()
        {
            string userInput = Inputty.GetString("Region:", "USWest1", false);

            switch (userInput)
            {
                case "APNortheast1":
                    _S3Region = RegionEndpoint.APNortheast1;
                    break;
                case "APSoutheast1":
                    _S3Region = RegionEndpoint.APSoutheast1;
                    break;
                case "APSoutheast2":
                    _S3Region = RegionEndpoint.APSoutheast2;
                    break;
                case "EUWest1":
                    _S3Region = RegionEndpoint.EUWest1;
                    break;
                case "SAEast1":
                    _S3Region = RegionEndpoint.SAEast1;
                    break;
                case "USEast1":
                    _S3Region = RegionEndpoint.USEast1;
                    break;
                case "USWest1":
                    _S3Region = RegionEndpoint.USWest1;
                    break;
                case "USWest2":
                    _S3Region = RegionEndpoint.USWest2;
                    break;
            }
        }

        static void SetSsl()
        {
            _Ssl = Inputty.GetBoolean("Use SSL:", false);
        }

        static void SetEndpoint()
        {
            _Endpoint = Inputty.GetString("Endpoint:", "http://s3.local.gd:8000/", false);
        }

        static void SetAccessKey()
        {
            _AccessKey = Inputty.GetString("Access key:", _AccessKey, false);
        }

        static void SetSecretKey()
        {
            _SecretKey = Inputty.GetString("Secret key:", _SecretKey, false);
        }

        static void InitS3Client()
        {
            _S3Credentials = new Amazon.Runtime.BasicAWSCredentials(_AccessKey, _SecretKey);

            _S3Config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USWest1,
                ServiceURL = _Endpoint,
                ForcePathStyle = _ForcePathStyle,
                UseHttp = _Ssl,
                SignatureVersion = _SignatureVersion,
                SignatureMethod = SigningAlgorithm.HmacSHA1
            };

            if (_UseProxy && !String.IsNullOrEmpty(_ProxyHost))
            {
                _S3Config.ProxyHost = _ProxyHost;
                _S3Config.ProxyPort = _ProxyPort;
            }

            _S3Client = new AmazonS3Client(_S3Credentials, _S3Config);

            if (_ForcePathStyle)
            {
                Console.WriteLine("Client configured to use path-style URLs; ensure server is configured accordingly");
            }
            else
            { 
                Console.WriteLine("Client configured to use virtual hosting URLs; ensure server is configured accordingly");
            }
        }

        #endregion

        #region Service-Primitives

        static void ListBuckets()
        {
            ListBucketsRequest request = new ListBucketsRequest();
            ListBucketsResponse response = _S3Client.ListBucketsAsync(request).Result;
            int statusCode = (int)response.HttpStatusCode;

            if (response != null)
            {
                Console.WriteLine("Success");
                Console.WriteLine("  Owner   : " + (response.Owner != null ? response.Owner.DisplayName : "null"));
                Console.WriteLine("  Buckets : " + response.Buckets.Count);
                foreach (S3Bucket bucket in response.Buckets)
                {
                    Console.WriteLine("    " + bucket.BucketName);
                }
            }
            else
            {
                Console.WriteLine("Failed");
            }
        }

        static void ListBucket()
        {
            string continuationToken = Inputty.GetString("Continuation token:", null, true);

            ListObjectsV2Request request = new ListObjectsV2Request();
            request.BucketName = _Bucket;
            request.ContinuationToken = continuationToken;

            ListObjectsV2Response response = _S3Client.ListObjectsV2Async(request).Result;
            if (response != null)
            {
                foreach (S3Object curr in response.S3Objects)
                {
                    Console.WriteLine(curr.Key + ": " + curr.Size + " bytes");
                }

                if (!String.IsNullOrEmpty(response.NextContinuationToken))
                {
                    Console.WriteLine("Continuation token: " + response.NextContinuationToken);
                }

                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failed");
            }
        }

        #endregion

        #region Bucket-Primitives

        static void WriteBucket()
        {
            string id = Inputty.GetString("Name:", null, false);

            PutBucketRequest request = new PutBucketRequest();
            request.BucketName = id;

            PutBucketResponse response = _S3Client.PutBucketAsync(request).Result;
            int statusCode = (int)response.HttpStatusCode;

            if (response != null)
            {
                Console.WriteLine("Success");
                return;
            }
            else
            {
                Console.WriteLine("Failed");
                return;
            }
        }

        static void WriteBucketAcl()
        {
            string id = Inputty.GetString("Bucket:", null, false);
            string owner = Inputty.GetString("Owner:", "default", false);

            PutACLRequest request = new PutACLRequest();
            request.BucketName = id;
            request.AccessControlList = new S3AccessControlList();
            request.AccessControlList.Owner = new Owner();
            request.AccessControlList.Owner.DisplayName = owner;

            request.AccessControlList.Grants = new List<S3Grant>();
            S3Grant grant = new S3Grant();
            S3Grantee grantee = new S3Grantee();
            grantee.CanonicalUser = owner;
            grantee.DisplayName = owner;
            grant.Grantee = grantee;

            request.AccessControlList.Grants.Add(grant);

            PutACLResponse response = _S3Client.PutACLAsync(request).Result;
            int statusCode = (int)response.HttpStatusCode;

            if (response != null)
            {
                Console.WriteLine("Success");
                return;
            }
            else
            {
                Console.WriteLine("Failed");
                return;
            }
        }

        static void WriteBucketTags()
        {
            string bucket = Inputty.GetString("Bucket:", null, false);
            string key = Inputty.GetString("Tag Key:", null, false);
            string val = Inputty.GetString("Tag Value:", null, false);

            PutBucketTaggingRequest request = new PutBucketTaggingRequest();
            request.BucketName = bucket;
            request.TagSet = new List<Tag>();
            Tag tag = new Tag();
            tag.Key = key;
            tag.Value = val;
            request.TagSet.Add(tag);

            PutBucketTaggingResponse response = _S3Client.PutBucketTaggingAsync(request).Result;
            if (response != null)
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failed");
            }
        }

        static void ReadBucketAcl()
        {
            string id = Inputty.GetString("Bucket:", null, false);

            GetACLRequest request = new GetACLRequest();
            request.BucketName = id;

            GetACLResponse response = _S3Client.GetACLAsync(request).Result;

            if (response != null)
            {
                if (response.AccessControlList != null)
                {
                    Console.WriteLine("Owner: " + response.AccessControlList.Owner.DisplayName + " ID " + response.AccessControlList.Owner.Id);
                    Console.WriteLine("Grants:");
                    foreach (S3Grant grant in response.AccessControlList.Grants)
                    {
                        Console.WriteLine("| Grantee    : " + grant.Grantee.DisplayName);
                        Console.WriteLine("| Permission : " + grant.Permission);
                    }
                }

                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failed");
            }
        }

        static void ReadBucketTags()
        {
            string bucket = Inputty.GetString("Bucket:", null, false);

            GetBucketTaggingRequest request = new GetBucketTaggingRequest();
            request.BucketName = bucket;

            GetBucketTaggingResponse response = _S3Client.GetBucketTaggingAsync(request).Result;
            if (response != null)
            {
                Console.WriteLine("Success");
                foreach (Tag curr in response.TagSet)
                {
                    Console.WriteLine("  " + curr.Key + ": " + curr.Value);
                }
            }
            else
            {
                Console.WriteLine("Failed");
            }
        }

        static void WriteBucketVersioning()
        {
            string bucket = Inputty.GetString("Bucket:", null, false);
            bool versioning = Inputty.GetBoolean("Enable versioning:", true);

            PutBucketVersioningRequest request = new PutBucketVersioningRequest();
            request.BucketName = bucket;
            request.VersioningConfig = new S3BucketVersioningConfig();
            request.VersioningConfig.EnableMfaDelete = false;

            if (versioning)
                request.VersioningConfig.Status = VersionStatus.Enabled;
            else
                request.VersioningConfig.Status = VersionStatus.Suspended;

            PutBucketVersioningResponse response = _S3Client.PutBucketVersioningAsync(request).Result;
            if (response != null)
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failed");
            }
        }

        static void ReadBucketVersioning()
        {
            string bucket = Inputty.GetString("Bucket:", null, false);

            GetBucketVersioningRequest request = new GetBucketVersioningRequest();
            request.BucketName = bucket;

            GetBucketVersioningResponse response = _S3Client.GetBucketVersioningAsync(request).Result;
            if (response != null)
            {
                Console.WriteLine("Success");
                Console.WriteLine("  MFA delete  : " + response.VersioningConfig.EnableMfaDelete.ToString());
                Console.WriteLine("  Versioning  : " + response.VersioningConfig.Status.Value);
            }
            else
            {
                Console.WriteLine("Failed");
            }
        }

        static void ReadBucketVersions()
        {
            string bucket = Inputty.GetString("Bucket:", null, false);

            ListVersionsRequest request = new ListVersionsRequest();
            request.BucketName = bucket;

            ListVersionsResponse response = _S3Client.ListVersionsAsync(request).Result;
            if (response != null)
            {
                Console.WriteLine("Success");
                Console.WriteLine("Bucket name     : " + response.Name);
                Console.WriteLine("Max keys        : " + response.MaxKeys);
                Console.WriteLine("Is truncated    : " + response.IsTruncated);

                Console.Write(    "Versions        : ");
                if (response.Versions != null && response.Versions.Count > 0)
                {
                    Console.WriteLine(response.Versions.Count);
                    foreach (S3ObjectVersion ver in response.Versions)
                    {
                        Console.Write("| " + ver.Key + "[" + ver.VersionId + "] ");
                        if (ver.IsDeleteMarker) Console.WriteLine("(delete marker)");
                        else Console.WriteLine(ver.Size + " bytes");
                    }
                }

                Console.Write(    "Common prefixes : ");
                if (response.CommonPrefixes != null && response.CommonPrefixes.Count > 0)
                {
                    Console.WriteLine(response.CommonPrefixes.Count);
                    foreach (string prefix in response.CommonPrefixes) Console.WriteLine("| " + prefix);
                }
                else
                {
                    Console.WriteLine("0");
                }
            }
            else
            {
                Console.WriteLine("Failed");
            }
        }

        static void DeleteBucket()
        {
            string id = Inputty.GetString("Bucket:", null, false);

            DeleteBucketRequest request = new DeleteBucketRequest();
            request.BucketName = id;

            DeleteBucketResponse response = _S3Client.DeleteBucketAsync(request).Result;
            if (response != null)
            {
                Console.WriteLine("Success");
                return;
            }
            else
            {
                Console.WriteLine("Failed");
                return;
            }
        }

        static void DeleteBucketTags()
        {
            string id = Inputty.GetString("Bucket:", null, false);

            DeleteBucketTaggingRequest request = new DeleteBucketTaggingRequest();
            request.BucketName = id;

            DeleteBucketTaggingResponse response = _S3Client.DeleteBucketTaggingAsync(request).Result;
            if (response != null)
            {
                Console.WriteLine("Success");
                return;
            }
            else
            {
                Console.WriteLine("Failed");
                return;
            }
        }

        static void BucketExists()
        {
            string id = Inputty.GetString("Bucket:", null, false);
            Console.WriteLine("Exists: " + AmazonS3Util.DoesS3BucketExistV2Async(_S3Client, id).Result);
        }

        #endregion

        #region S3-Object-Primitives

        static void WriteObject()
        {
            string id = Inputty.GetString("Key:", null, false);
            string data = Inputty.GetString("Data:", null, true);
            byte[] bytes = Array.Empty<byte>();
            if (!String.IsNullOrEmpty(data)) bytes = Encoding.UTF8.GetBytes(data);

            try
            {
                Stream s = new MemoryStream(bytes);

                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = _Bucket,
                    Key = id,
                    InputStream = s,
                    ContentType = "application/octet-stream"
                };

                PutObjectResponse response = _S3Client.PutObjectAsync(request).Result;
                int statusCode = (int)response.HttpStatusCode;

                if (response != null)
                {
                    Console.WriteLine("Success");
                    return;
                }
                else
                {
                    Console.WriteLine("Failed");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void WriteObjectAcl()
        {
            string id = Inputty.GetString("Key:", null, false);
            string owner = Inputty.GetString("Owner:", "default", false);

            try
            {
                PutACLRequest request = new PutACLRequest
                {
                    BucketName = _Bucket,
                    Key = id
                };

                request.Key = id;
                request.AccessControlList = new S3AccessControlList();

                request.AccessControlList.Owner = new Owner();
                request.AccessControlList.Owner.DisplayName = owner;

                request.AccessControlList.Grants = new List<S3Grant>();
                S3Grant grant = new S3Grant();
                S3Grantee grantee = new S3Grantee();
                grantee.CanonicalUser = owner;
                grantee.DisplayName = owner;
                grantee.EmailAddress = owner;
                grant.Grantee = grantee;

                request.AccessControlList.Grants.Add(grant);

                PutACLResponse response = _S3Client.PutACLAsync(request).Result;
                int statusCode = (int)response.HttpStatusCode;

                if (response != null)
                {
                    Console.WriteLine("Success");
                    return;
                }
                else
                {
                    Console.WriteLine("Failed");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void WriteObjectTags()
        {
            string id = Inputty.GetString("Key:", null, false);
            int ver = Inputty.GetInteger("Version:", 1, true, false);
            string key = Inputty.GetString("Tag Key:", null, false);
            string val = Inputty.GetString("Tag Value:", null, false);

            PutObjectTaggingRequest request = new PutObjectTaggingRequest();
            request.BucketName = _Bucket;
            request.Key = id;
            request.VersionId = ver.ToString();

            Tag tag = new Tag();
            tag.Key = key;
            tag.Value = val;

            request.Tagging = new Tagging();
            request.Tagging.TagSet = new List<Tag>();
            request.Tagging.TagSet.Add(tag);

            PutObjectTaggingResponse response = _S3Client.PutObjectTaggingAsync(request).Result;
            int statusCode = (int)response.HttpStatusCode;

            if (response != null)
            {
                Console.WriteLine("Success");
                return;
            }
            else
            {
                Console.WriteLine("Failed");
                return;
            }
        }

        static void WriteObjectRetention()
        {
            string id = Inputty.GetString("Key:", null, false);

            try
            {
                PutObjectRetentionRequest request = new PutObjectRetentionRequest
                {
                    BucketName = _Bucket,
                    Key = id
                };

                request.Retention = new ObjectLockRetention();
                request.Retention.Mode = "GOVERNANCE";
                request.Retention.RetainUntilDate = DateTime.Now.AddYears(5);

                PutObjectRetentionResponse response = _S3Client.PutObjectRetentionAsync(request).Result;
                int statusCode = (int)response.HttpStatusCode;

                if (response != null)
                {
                    Console.WriteLine("Success");
                    return;
                }
                else
                {
                    Console.WriteLine("Failed");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void ReadObject()
        {
            string id = Inputty.GetString("Key:", null, false);

            try
            {
                GetObjectRequest request = new GetObjectRequest();
                request.BucketName = _Bucket;
                request.Key = id;

                using (GetObjectResponse response = _S3Client.GetObjectAsync(request).Result)
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    if (response.ContentLength > 0)
                    {
                        // first copy the stream
                        byte[] data = new byte[response.ContentLength];

                        Stream bodyStream = response.ResponseStream;
                        data = ReadStreamFully(bodyStream);

                        int statusCode = (int)response.HttpStatusCode;

                        if (data != null && data.Length > 0)
                        {
                            Console.WriteLine(Encoding.UTF8.GetString(data));
                            Console.WriteLine(Environment.NewLine + "Success");
                        }
                        else
                        {
                            Console.WriteLine("No data");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed");
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to read object");
            }
        }

        static void ReadObjectWithVersion()
        {
            string id = Inputty.GetString("Key:", null, false);
            int ver = Inputty.GetInteger("Version:", 1, true, false);

            try
            {
                GetObjectRequest request = new GetObjectRequest();
                request.BucketName = _Bucket;
                request.Key = id;
                request.VersionId = ver.ToString();

                using (GetObjectResponse response = _S3Client.GetObjectAsync(request).Result)
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    if (response.ContentLength > 0)
                    {
                        // first copy the stream
                        byte[] data = new byte[response.ContentLength];

                        Stream bodyStream = response.ResponseStream;
                        data = ReadStreamFully(bodyStream);

                        int statusCode = (int)response.HttpStatusCode;

                        if (data != null && data.Length > 0)
                        {
                            Console.WriteLine(Encoding.UTF8.GetString(data));
                            Console.WriteLine(Environment.NewLine + "Success");
                        }
                        else
                        {
                            Console.WriteLine("No data");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed");
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to read object");
            }
        }

        static void ReadObjectAcl()
        {
            string id = Inputty.GetString("Key:", null, false);

            GetACLRequest request = new GetACLRequest();
            request.BucketName = _Bucket;
            request.Key = id;

            GetACLResponse response = _S3Client.GetACLAsync(request).Result;

            if (response != null)
            {
                if (response.AccessControlList != null)
                {
                    Console.WriteLine("Owner: " + response.AccessControlList.Owner.DisplayName + " ID " + response.AccessControlList.Owner.Id);
                    Console.WriteLine("Grants:");
                    foreach (S3Grant grant in response.AccessControlList.Grants)
                    {
                        Console.WriteLine("| Grantee    : " + grant.Grantee.DisplayName);
                        Console.WriteLine("| Permission : " + grant.Permission);
                    }
                }

                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failed");
            }
        }

        static void ReadObjectRange()
        {
            string id = Inputty.GetString("Key:", null, false);
            int ver = Inputty.GetInteger("Version:", 1, true, false);
            int startPos = Inputty.GetInteger("Start position:", 0, true, true);
            int endPos = Inputty.GetInteger("End position:", 0, true, true);

            try
            {
                GetObjectRequest request = new GetObjectRequest();
                request.BucketName = _Bucket;
                request.Key = id;
                request.ByteRange = new ByteRange(startPos, endPos);
                request.VersionId = ver.ToString();

                using (GetObjectResponse response = _S3Client.GetObjectAsync(request).Result)
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    if (response.ContentLength > 0)
                    {
                        // first copy the stream
                        byte[] data = new byte[response.ContentLength];

                        Stream bodyStream = response.ResponseStream;
                        data = ReadStreamFully(bodyStream);

                        int statusCode = (int)response.HttpStatusCode;

                        if (data != null && data.Length > 0)
                        {
                            Console.WriteLine(Encoding.UTF8.GetString(data));
                            Console.WriteLine(Environment.NewLine + "Success");
                        }
                        else
                        {
                            Console.WriteLine("No data");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed");
                    }
                }
            }
            catch (Exception)
            {
                throw new IOException("Unable to read object.");
            }
        }

        static void ReadObjectTags()
        {
            string id = Inputty.GetString("Key:", null, false);
            int ver = Inputty.GetInteger("Version:", 1, true, false);

            try
            {
                GetObjectTaggingRequest request = new GetObjectTaggingRequest();
                request.BucketName = _Bucket;
                request.Key = id;
                request.VersionId = ver.ToString();

                GetObjectTaggingResponse response = _S3Client.GetObjectTaggingAsync(request).Result;

                if (response != null)
                {
                    Console.WriteLine("Success");
                    foreach (Tag curr in response.Tagging)
                    {
                        Console.WriteLine("  " + curr.Key + ": " + curr.Value);
                    }
                }
                else
                {
                    Console.WriteLine("Failed");
                }
            }
            catch (Exception)
            {
                throw new IOException("Unable to read object tags.");
            }
        }

        static void ReadObjectRetention()
        {
            string id = Inputty.GetString("Key:", null, false);

            GetObjectRetentionRequest request = new GetObjectRetentionRequest();
            request.BucketName = _Bucket;
            request.Key = id;

            GetObjectRetentionResponse response = _S3Client.GetObjectRetentionAsync(request).Result;

            if (response != null)
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failed");
            }
        }

        static void DeleteObject()
        {
            string id = Inputty.GetString("Key:", null, false);
            int ver = Inputty.GetInteger("Version:", 1, true, false);

            DeleteObjectRequest request = new DeleteObjectRequest();
            request.BucketName = _Bucket;
            request.Key = id;
            request.VersionId = ver.ToString();

            DeleteObjectResponse response = _S3Client.DeleteObjectAsync(request).Result;
            int statusCode = (int)response.HttpStatusCode;

            if (response != null)
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failed");
            }
        }

        static void DeleteObjectTags()
        {
            string id = Inputty.GetString("Key:", null, false);
            int ver = Inputty.GetInteger("Version:", 1, true, false);

            try
            {
                DeleteObjectTaggingRequest request = new DeleteObjectTaggingRequest();
                request.BucketName = _Bucket;
                request.Key = id;
                request.VersionId = ver.ToString();

                DeleteObjectTaggingResponse response = _S3Client.DeleteObjectTaggingAsync(request).Result;
                int statusCode = (int)response.HttpStatusCode;

                if (response != null)
                {
                    Console.WriteLine("Success");
                    return;
                }
                else
                {
                    Console.WriteLine("Failed");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void DeleteMultiple()
        {
            List<KeyVersion> versions = new List<KeyVersion>();
            while (true)
            {
                string key = Inputty.GetString("Key [null to end]:", null, true);
                if (String.IsNullOrEmpty(key)) break;
                int ver = Inputty.GetInteger("Version:", 1, true, false);
                KeyVersion version = new KeyVersion();
                version.Key = key;
                version.VersionId = ver.ToString();
                versions.Add(version);
            }

            DeleteObjectsRequest request = new DeleteObjectsRequest();
            request.BucketName = _Bucket;
            request.Objects = versions;

            DeleteObjectsResponse response = _S3Client.DeleteObjectsAsync(request).Result;
            int statusCode = (int)response.HttpStatusCode;

            if (response != null)
            {
                Console.WriteLine("Deleted objects:");
                if (response.DeletedObjects != null && response.DeletedObjects.Count > 0)
                {
                    foreach (DeletedObject curr in response.DeletedObjects)
                    {
                        Console.WriteLine("  " + curr.Key + " version " + curr.VersionId);
                    }
                }
                else
                {
                    Console.WriteLine("  (none)");
                }
                Console.WriteLine("Errors:");
                if (response.DeleteErrors != null && response.DeleteErrors.Count > 0)
                {
                    foreach (DeleteError curr in response.DeleteErrors)
                    {
                        Console.WriteLine("  " + curr.Key + " version " + curr.VersionId);
                    }
                }
                else
                {
                    Console.WriteLine("  (none)");
                }
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failed");
            }
        }

        static void ObjectExists()
        {
            string id = Inputty.GetString("Key:", null, false);
            int ver = Inputty.GetInteger("Version:", 1, true, false);

            try
            {
                GetObjectMetadataRequest request = new GetObjectMetadataRequest();
                request.BucketName = _Bucket;
                request.Key = id;
                request.VersionId = ver.ToString();

                GetObjectMetadataResponse response = _S3Client.GetObjectMetadataAsync(request).Result;
                Console.WriteLine("Exists");
            }
            catch (Exception)
            {
                Console.WriteLine("Does not exist");
            }
        }

        private static byte[] ReadStreamFully(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (!input.CanRead) throw new InvalidOperationException("Input stream is not readable");

            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;

                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }

        #endregion 
    }
}