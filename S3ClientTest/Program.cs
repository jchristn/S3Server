using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3ClientTest
{
    class Program
    {
        static bool _Ssl = false;
        static string _Endpoint = null;
        static string _AccessKey = null;
        static string _SecretKey = null;
        static AmazonS3Config _S3Config = null;
        static IAmazonS3 _S3Client = null;
        static BasicAWSCredentials _S3Credentials = null;
        static RegionEndpoint _S3Region = RegionEndpoint.USWest1; 
        static string _Bucket = null;

        static bool _RunForever = true;

        static void Main(string[] args)
        {
            Initialize();

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

                    case "write bucket tags":
                        WriteBucketTags();
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

                    #endregion

                    #region Object-Commands

                    case "write":
                        WriteObject();
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
            Console.WriteLine("   write bucket tags   Write tags to a bucket");
            Console.WriteLine("   read bucket tags    Read tags from a bucket");
            Console.WriteLine("   write bucket ver    Write bucket versioning");
            Console.WriteLine("   read bucket ver     Read bucket versioning");
            Console.WriteLine("");
            Console.WriteLine("-- Object Commands --");
            Console.WriteLine("   write               Write an object");
            Console.WriteLine("   write tags          Write object tags");
            Console.WriteLine("   write retention     Write object retention");
            Console.WriteLine("   read                Read an object");
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
            _Bucket = Common.InputString("Bucket:", "default", false);
        }

        static void SetRegion()
        {
            string userInput = Common.InputString("Region:", "USWest1", false);

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
            _Ssl = Common.InputBoolean("Use SSL:", false);
        }

        static void SetEndpoint()
        {
            _Endpoint = Common.InputString("Endpoint:", "http://localhost:8000/", false);
        }

        static void SetAccessKey()
        {
            _AccessKey = Common.InputString("Access key:", "default", false); 
        }

        static void SetSecretKey()
        {
            _SecretKey = Common.InputString("Secret key:", "default", false); 
        }

        static void InitS3Client()
        {
            _S3Credentials = new Amazon.Runtime.BasicAWSCredentials(_AccessKey, _SecretKey);
             
            _S3Config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USWest1, 
                ServiceURL = _Endpoint,  
                ForcePathStyle = true,
                UseHttp = _Ssl
            };

            _S3Client = new AmazonS3Client(_S3Credentials, _S3Config);
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
                Console.WriteLine("  Owner   : " + response.Owner.DisplayName);
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
            string continuationToken = Common.InputString("Continuation token:", null, true);

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
            string id = Common.InputString("Name:", null, false);

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

        static void WriteBucketTags()
        {
            string bucket = Common.InputString("Bucket:", null, false);
            string key = Common.InputString("Key:", null, false);
            string val = Common.InputString("Value:", null, false);

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

        static void ReadBucketTags()
        {
            string bucket = Common.InputString("Bucket:", null, false);

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
            string bucket = Common.InputString("Bucket:", null, false);
            bool versioning = Common.InputBoolean("Enable versioning:", true);

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
            string bucket = Common.InputString("Bucket:", null, false);

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

        #endregion

        #region S3-Object-Primitives

        static void WriteObject()
        {
            string id = Common.InputString("Key:", null, false);
            byte[] data = Encoding.UTF8.GetBytes(Common.InputString("Data:", null, false));

            try
            {
                Stream s = new MemoryStream(data);

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
                Console.WriteLine(Common.SerializeJson(e, true));
            }
        }

        static void WriteObjectTags()
        {
            string id = Common.InputString("Key:", null, false);

            try
            {
                PutObjectTaggingRequest request = new PutObjectTaggingRequest
                {
                    BucketName = _Bucket,
                    Key = id
                };

                request.Tagging = new Tagging();
                request.Tagging.TagSet = new List<Tag>();

                Tag tag1 = new Tag();
                tag1.Key = "foo";
                tag1.Value = "bar";
                request.Tagging.TagSet.Add(tag1);

                Tag tag2 = new Tag();
                tag2.Key = "bar";
                tag2.Value = "baz";
                request.Tagging.TagSet.Add(tag2);

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
            catch (Exception e)
            {
                Console.WriteLine(Common.SerializeJson(e, true));
            }
        }

        static void WriteObjectRetention()
        {
            string id = Common.InputString("Key:", null, false);

            try
            {
                PutObjectRetentionRequest request = new PutObjectRetentionRequest
                {
                    BucketName = _Bucket,
                    Key = id
                };

                request.Retention = new ObjectLockRetention();
                request.Retention.Mode = "foo";
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
                Console.WriteLine(Common.SerializeJson(e, true));
            }
        }

        static void ReadObject()
        {
            string id = Common.InputString("Key:", null, false);

            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _Bucket,
                    Key = id
                };

                using (GetObjectResponse response = _S3Client.GetObjectAsync(request).Result)
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    if (response.ContentLength > 0)
                    {
                        // first copy the stream
                        byte[] data = new byte[response.ContentLength];

                        Stream bodyStream = response.ResponseStream;
                        data = Common.StreamToBytes(bodyStream);

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

        static void ReadObjectRange()
        {
            string id = Common.InputString("Key:", null, false);
            int startPos = Common.InputInteger("Start position:", 0, true, true);
            int endPos = Common.InputInteger("End position:", 0, true, true);

            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _Bucket,
                    Key = id,
                    ByteRange = new ByteRange(startPos, endPos)
                };

                using (GetObjectResponse response = _S3Client.GetObjectAsync(request).Result)
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    if (response.ContentLength > 0)
                    {
                        // first copy the stream
                        byte[] data = new byte[response.ContentLength];

                        Stream bodyStream = response.ResponseStream;
                        data = Common.StreamToBytes(bodyStream);

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
            string id = Common.InputString("Key:", null, false);

            try
            {
                GetObjectTaggingRequest request = new GetObjectTaggingRequest();
                request.BucketName = _Bucket;
                request.Key = id;

                GetObjectTaggingResponse response = _S3Client.GetObjectTaggingAsync(request).Result;

                if (response != null)
                {
                    Console.WriteLine(Common.SerializeXml(response.Tagging));
                    Console.WriteLine("Success");
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
            string id = Common.InputString("Key:", null, false);

            try
            {
                GetObjectRetentionRequest request = new GetObjectRetentionRequest();
                request.BucketName = _Bucket;
                request.Key = id;

                GetObjectRetentionResponse response = _S3Client.GetObjectRetentionAsync(request).Result;

                if (response != null)
                {
                    Console.WriteLine(Common.SerializeXml(response.Retention));
                    Console.WriteLine("Success");
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

        static void DeleteObject()
        {
            string id = Common.InputString("Key:", null, false);

            DeleteObjectRequest request = new DeleteObjectRequest
            {
                BucketName = _Bucket,
                Key = id
            };

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
            string id = Common.InputString("Key:", null, false);

            try
            {
                DeleteObjectTaggingRequest request = new DeleteObjectTaggingRequest
                {
                    BucketName = _Bucket,
                    Key = id
                };

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
                Console.WriteLine(Common.SerializeJson(e, true));
            }
        }

        static void DeleteMultiple()
        {
            List<string> keys = new List<string>();
            while (true)
            {
                string key = Common.InputString("Key [null to end]:", null, true);
                if (!String.IsNullOrEmpty(key)) keys.Add(key);
                else break;
            }

            DeleteObjectsRequest request = new DeleteObjectsRequest();
            request.BucketName = _Bucket;
            request.Objects = new List<KeyVersion>();
            
            foreach (string curr in keys)
            {
                KeyVersion key = new KeyVersion();
                key.Key = curr;
                request.Objects.Add(key);
            }

            DeleteObjectsResponse response = _S3Client.DeleteObjectsAsync(request).Result;
            int statusCode = (int)response.HttpStatusCode;

            if (response != null)
            {
                Console.WriteLine("Deleted objects:");
                Console.WriteLine(Common.SerializeXml(response.DeletedObjects));
                Console.WriteLine("Errors:");
                Console.WriteLine(Common.SerializeXml(response.DeleteErrors));
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Failed");
            }
        }

        static void ObjectExists()
        {
            string id = Common.InputString("Key:", null, false);

            try
            {
                GetObjectMetadataRequest request = new GetObjectMetadataRequest
                {
                    BucketName = _Bucket,
                    Key = id
                };

                GetObjectMetadataResponse response = _S3Client.GetObjectMetadataAsync(request).Result;
                Console.WriteLine("Exists");
            }
            catch (Exception)
            {
                Console.WriteLine("Does not exist");
            }
        }

        #endregion 
    }
}
