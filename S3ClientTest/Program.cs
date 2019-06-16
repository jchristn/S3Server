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

                    case "write":
                        WriteObject();
                        break;

                    case "write tags":
                        WriteObjectTags();
                        break;

                    case "read":
                        ReadObject();
                        break;

                    case "delete":
                        DeleteObject();
                        break;

                    case "exists":
                        ObjectExists();
                        break;
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
            Console.WriteLine("--- Available Commands ---");
            Console.WriteLine("  ?           Help, this menu");
            Console.WriteLine("  q           Quit the program");
            Console.WriteLine("  cls         Clear the screen");
            Console.WriteLine("  endpoint    Set endpoint (currently " + _Endpoint + ")");
            Console.WriteLine("  access      Set access key (currently " + _AccessKey + ")");
            Console.WriteLine("  secret      Set secret key (currently " + _SecretKey + ")");
            Console.WriteLine("  region      Set AWS region (currently " + _S3Region.ToString() + ")");
            Console.WriteLine("  bucket      Set S3 bucket (currently " + _Bucket + ")");
            Console.WriteLine("  init        Initialize client (needed after changing keys or region)");
            Console.WriteLine("  write       Write an object");
            Console.WriteLine("  write tags  Write object tags");
            Console.WriteLine("  read        Read an object");
            Console.WriteLine("  delete      Delete an object");
            Console.WriteLine("  exists      Check if object exists");
        }

        #endregion

        #region S3

        static void SetBucket()
        {
            _Bucket = Common.InputString("Bucket:", "bucket", false);
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
            _AccessKey = Common.InputString("Access key:", "access", false); 
        }

        static void SetSecretKey()
        {
            _SecretKey = Common.InputString("Secret key:", "secret", false); 
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

        #endregion 
    }
}
