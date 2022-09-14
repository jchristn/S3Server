using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace S3ServerLibrary
{
    /// <summary>
    /// Common static methods.
    /// </summary>
    internal static class Common
    { 
        #region Input

        public static bool InputBoolean(string question, bool yesDefault)
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

        public static string InputString(string question, string defaultAnswer, bool allowNull)
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

        public static List<string> InputStringList(string question, bool allowEmpty)
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

        public static int InputInteger(string question, int defaultAnswer, bool positiveOnly, bool allowZero)
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
        
        #region IsTrue

        public static bool IsTrue(bool val)
        {
            return val;
        }

        public static bool IsTrue(bool? val)
        {
            if (val == null) return false;
            return Convert.ToBoolean(val);
        }

        public static bool IsTrue(string val)
        {
            if (String.IsNullOrEmpty(val)) return false;
            val = val.ToLower().Trim();
            int valInt = 0;
            if (Int32.TryParse(val, out valInt)) if (valInt == 1) return true;
            if (String.Compare(val, "true") == 0) return true;
            return false;
        }

        #endregion
        
        #region Misc

        public static string StringRemove(string original, string remove)
        {
            if (String.IsNullOrEmpty(original)) return null;
            if (String.IsNullOrEmpty(remove)) return original;

            int index = original.IndexOf(remove);
            string ret = (index < 0)
                ? original
                : original.Remove(index, remove.Length);

            return ret;
        }

        public static string Line(int count, string fill)
        {
            if (count < 1) return "";

            string ret = "";
            for (int i = 0; i < count; i++)
            {
                ret += fill;
            }

            return ret;
        }

        public static string RandomString(int numChar)
        {
            string ret = "";
            if (numChar < 1) return null;
            int valid = 0;
            Random random = new Random((int)DateTime.Now.Ticks);
            int num = 0;

            for (int i = 0; i < numChar; i++)
            {
                num = 0;
                valid = 0;
                while (valid == 0)
                {
                    num = random.Next(126);
                    if (((num > 47) && (num < 58)) ||
                        ((num > 64) && (num < 91)) ||
                        ((num > 96) && (num < 123)))
                    {
                        valid = 1;
                    }
                }
                ret += (char)num;
            }

            return ret;
        }

        public static double TotalMsFrom(DateTime startTime)
        {
            try
            {
                DateTime endTime = DateTime.Now;
                TimeSpan totalTime = (endTime - startTime);
                return totalTime.TotalMilliseconds;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static bool IsLaterThanNow(DateTime? dt)
        {
            try
            {
                DateTime curr = Convert.ToDateTime(dt);
                return Common.IsLaterThanNow(curr);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsLaterThanNow(DateTime dt)
        {
            if (DateTime.Compare(dt, DateTime.Now) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsIpv4Address(string val)
        {
            if (String.IsNullOrEmpty(val)) throw new ArgumentNullException(nameof(val));

            string ipv4Pattern = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
            Match match = Regex.Match(val, ipv4Pattern);
            if (match.Success) return true; // IPv4 regular expression match

            IPAddress ip = null;
            return IPAddress.TryParse(val, out ip);
        }

        public static bool IsIpv6Address(string val)
        {
            if (String.IsNullOrEmpty(val)) throw new ArgumentNullException(nameof(val));

            string ipv6Pattern = @"(?:^|(?<=\s))(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))(?=\s|$)";
            Match match = Regex.Match(val, ipv6Pattern);
            if (match.Success) return true; // IPv6 regular expression match

            IPAddress ip = null;
            return IPAddress.TryParse(val, out ip);
        }

        public static bool IsIpAddress(string val)
        {
            return IsIpv4Address(val) || IsIpv6Address(val);
        }

        public static bool ContainsUnsafeCharacters(string data)
        {
            /*
             * 
             * Returns true if unsafe characters exist
             * 
             * 
             */

            // see https://kb.acronis.com/content/39790

            if (String.IsNullOrEmpty(data)) return false;
            if (data.Equals(".")) return true;
            if (data.Equals("..")) return true;

            if (
                (String.Compare(data.ToLower(), "com1") == 0) ||
                (String.Compare(data.ToLower(), "com2") == 0) ||
                (String.Compare(data.ToLower(), "com3") == 0) ||
                (String.Compare(data.ToLower(), "com4") == 0) ||
                (String.Compare(data.ToLower(), "com5") == 0) ||
                (String.Compare(data.ToLower(), "com6") == 0) ||
                (String.Compare(data.ToLower(), "com7") == 0) ||
                (String.Compare(data.ToLower(), "com8") == 0) ||
                (String.Compare(data.ToLower(), "com9") == 0) ||
                (String.Compare(data.ToLower(), "lpt1") == 0) ||
                (String.Compare(data.ToLower(), "lpt2") == 0) ||
                (String.Compare(data.ToLower(), "lpt3") == 0) ||
                (String.Compare(data.ToLower(), "lpt4") == 0) ||
                (String.Compare(data.ToLower(), "lpt5") == 0) ||
                (String.Compare(data.ToLower(), "lpt6") == 0) ||
                (String.Compare(data.ToLower(), "lpt7") == 0) ||
                (String.Compare(data.ToLower(), "lpt8") == 0) ||
                (String.Compare(data.ToLower(), "lpt9") == 0) ||
                (String.Compare(data.ToLower(), "con") == 0) ||
                (String.Compare(data.ToLower(), "nul") == 0) ||
                (String.Compare(data.ToLower(), "prn") == 0) ||
                (String.Compare(data.ToLower(), "con") == 0)
                )
            {
                return true;
            }

            for (int i = 0; i < data.Length; i++)
            {
                if (
                    ((int)(data[i]) < 32) ||    // below range
                    ((int)(data[i]) > 126) ||   // above range
                    ((int)(data[i]) == 47) ||   // slash /
                    ((int)(data[i]) == 92) ||   // backslash \
                    ((int)(data[i]) == 63) ||   // question mark ?
                    ((int)(data[i]) == 60) ||   // less than < 
                    ((int)(data[i]) == 62) ||   // greater than >
                    ((int)(data[i]) == 58) ||   // colon :
                    ((int)(data[i]) == 42) ||   // asterisk *
                    ((int)(data[i]) == 124) ||  // pipe |
                    ((int)(data[i]) == 34) ||   // double quote "
                    ((int)(data[i]) == 39) ||   // single quote '
                    ((int)(data[i]) == 94)      // caret ^
                    )
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsUnsafeCharacters(List<string> data)
        {
            if (data == null || data.Count < 1) return true;
            foreach (string curr in data)
            {
                if (ContainsUnsafeCharacters(curr)) return true;
            }
            return false;
        }

        public static int GuidToInt(string guid)
        {
            if (String.IsNullOrEmpty(guid)) return 0;
            byte[] bytes = Encoding.UTF8.GetBytes(guid);
            int ret = 0;

            foreach (byte currByte in bytes)
            {
                ret += (int)currByte;
            }

            return ret;
        }

        public static string ReplaceLastOccurrence(string src, string find, string replace)
        {
            int place = src.LastIndexOf(find);

            if (place == -1)
                return src;

            string result = src.Remove(place, find.Length).Insert(place, replace);
            return result;
        }

        public static byte[] ReadFromStream(Stream stream, long count, int bufferLen)
        {
            if (count <= 0) return null;
            if (bufferLen <= 0) throw new ArgumentException("Buffer must be greater than zero bytes.");
            byte[] buffer = new byte[bufferLen];

            int read = 0;
            long bytesRemaining = count;
            MemoryStream ms = new MemoryStream();

            while (bytesRemaining > 0)
            {
                if (bufferLen > bytesRemaining) buffer = new byte[bytesRemaining];

                read = stream.Read(buffer, 0, buffer.Length);
                if (read > 0)
                {
                    ms.Write(buffer, 0, read);
                    bytesRemaining -= read;
                }
                else
                {
                    throw new IOException("Could not read from supplied stream.");
                }
            }

            byte[] data = ms.ToArray();
            return data;
        }

        #endregion

        #region Crypto

        public static byte[] Md5(byte[] data)
        {
            if (data == null) return null;
            return MD5.Create().ComputeHash(data);
        }

        public static byte[] Md5(string data)
        {
            if (String.IsNullOrEmpty(data)) return null;
            return Md5(Encoding.UTF8.GetBytes(data));
        }

        public static byte[] HmacSha1(byte[] input, byte[] key)
        {
            if (input == null) return null;
            if (key == null || key.Length < 1) return null;
            return new HMACSHA1(key).ComputeHash(input);
        }

        public static byte[] HmacSha256(byte[] input, byte[] key)
        {
            if (input == null) return null;
            if (key == null || key.Length < 1) return null;
            return new HMACSHA256(key).ComputeHash(input);
        }

        public static byte[] Sha1(byte[] input)
        {
            if (input == null) return null;
            using (SHA1 sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(input);
            }
        }

        public static byte[] Sha256(byte[] data)
        {
            if (data == null) return null;
            using (SHA256 sha256 = SHA256.Create())
            { 
                return sha256.ComputeHash(data);
            }
        }
         
        #endregion

        #region Encoding

        public static bool IsBase64String(string s)
        {
            if (String.IsNullOrEmpty(s)) throw new ArgumentNullException(nameof(s));
            s = s.Trim();
            return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None); 
        }

        public static byte[] Base64ToBytes(string data)
        {
            return Convert.FromBase64String(data);
        }

        public static string Base64ToString(string data)
        {
            if (String.IsNullOrEmpty(data)) return null;
            byte[] bytes = System.Convert.FromBase64String(data);
            return System.Text.UTF8Encoding.UTF8.GetString(bytes);
        }

        public static string BytesToBase64(byte[] data)
        {
            if (data == null) return null;
            if (data.Length < 1) return null;
            return System.Convert.ToBase64String(data);
        }

        public static string BytesToHex(byte[] data)
        {
            if (data == null || data.Length < 1) return null;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++) sb.Append(data[i].ToString("X2"));
            return sb.ToString();
        }

        public static string StringToBase64(string data)
        {
            if (String.IsNullOrEmpty(data)) return null;
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(data);
            return System.Convert.ToBase64String(bytes);
        }

        public static List<string> CsvToStringList(string csv)
        {
            if (String.IsNullOrEmpty(csv)) return null;

            List<string> ret = new List<string>();

            string[] array = csv.Split(',');

            if (array != null)
            {
                if (array.Length > 0)
                {
                    foreach (string curr in array)
                    {
                        if (String.IsNullOrEmpty(curr)) continue;
                        ret.Add(curr.Trim());
                    }

                    return ret;
                }
            }

            return null;
        }

        public static string StringListToCsv(List<string> strings)
        {
            if (strings == null || strings.Count < 1) return null;

            int added = 0;
            string ret = "";

            foreach (string curr in strings)
            {
                if (added == 0)
                {
                    ret += curr;
                }
                else
                {
                    ret += "," + curr;
                }

                added++;
            }

            return ret;
        }

        public static byte[] HexStringToBytes(string hexStr)
        {
            if (String.IsNullOrEmpty(hexStr)) return null;

            var ret = new byte[hexStr.Length / 2];
            for (var i = 0; i < ret.Length; i++)
            {
                ret[i] = Convert.ToByte(hexStr.Substring(i * 2, 2), 16);
            }

            return ret;
        }

        public static string HexStringToBase64(string hexStr)
        {
            if (String.IsNullOrEmpty(hexStr)) return null;

            return Convert.ToBase64String(HexStringToBytes(hexStr));
        }

        #endregion
    }
}