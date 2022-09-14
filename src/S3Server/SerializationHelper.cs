using System;
using System.Collections.Generic;
using System.IO;
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
    /// Serialization helper.
    /// </summary>
    public static class SerializationHelper
    {
        #region Public-Members

        #endregion

        #region Private-Members

        #endregion

        #region Public-Methods

        /// <summary>
        /// Copy an object.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="source">Source object.</param>
        /// <returns>Copy.</returns>
        public static T CopyObject<T>(T source)
        {
            if (source == null) return default(T);

            string json = SerializeJson(source, false);
            try
            {
                return DeserializeJson<T>(json);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Serialize JSON.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="pretty">Pretty print.</param>
        /// <returns>JSON string.</returns>
        public static string SerializeJson(object obj, bool pretty)
        {
            if (obj == null) return null;
            string json;

            if (pretty)
            {
                json = JsonSerializer.Serialize(obj,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    }
                );
            }
            else
            {
                json = JsonSerializer.Serialize(obj,
                    new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    }
                );
            }

            return json;
        }

        /// <summary>
        /// Deserialize JSON. 
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="json">JSON string.</param>
        /// <returns>Instance.</returns>
        public static T DeserializeJson<T>(string json)
        {
            if (String.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// Deserialize JSON. 
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="data">JSON data.</param>
        /// <returns>Instance.</returns>
        public static T DeserializeJson<T>(byte[] data)
        {
            if (data == null || data.Length < 1) throw new ArgumentNullException(nameof(data));
            return JsonSerializer.Deserialize<T>(data);
        }

        /// <summary>
        /// Deserialize XML.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="bytes">XML data.</param>
        /// <returns>Instance.</returns>
        public static T DeserializeXml<T>(byte[] bytes) where T : class
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            return DeserializeXml<T>(Encoding.UTF8.GetString(bytes));
        }

        /// <summary>
        /// Deserialize XML.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="xml">XML string.</param>
        /// <returns>Instance.</returns>
        public static T DeserializeXml<T>(string xml) where T : class
        {
            if (String.IsNullOrEmpty(xml)) throw new ArgumentNullException(nameof(xml));

            // remove preamble if exists
            string byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            while (xml.StartsWith(byteOrderMarkUtf8, StringComparison.Ordinal))
            {
                xml = xml.Remove(0, byteOrderMarkUtf8.Length);
            }

            /*
             * 
             * This code respects the supplied namespace and validates it vs the model in code
             * 
             * 
            XmlSerializer xmls = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                return (T)xmls.Deserialize(ms);
            }
            */

            // The code that follows ignores namespaces

            T obj = null;

            using (TextReader textReader = new StringReader(xml))
            {
                using (XmlTextReader reader = new XmlTextReader(textReader))
                {
                    reader.Namespaces = false;
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    obj = (T)serializer.Deserialize(reader);
                }
            }

            return obj;
        }

        /// <summary>
        /// Serialize XML.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns>XML string.</returns>
        public static string SerializeXml2(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            XmlSerializer xml = new XmlSerializer(obj.GetType());

            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    xml.Serialize(writer, obj);
                    byte[] bytes = stream.ToArray();
                    string ret = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                    // remove preamble if exists
                    string byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                    while (ret.StartsWith(byteOrderMarkUtf8, StringComparison.Ordinal))
                    {
                        ret = ret.Remove(0, byteOrderMarkUtf8.Length);
                    }

                    return ret;
                }
            }
        }

        /// <summary>
        /// Serialize XML.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="pretty">Pretty print.</param>
        /// <returns>XML string.</returns>
        public static string SerializeXml(object obj, bool pretty = false)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            XmlSerializer xml = new XmlSerializer(obj.GetType());

            using (MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.GetEncoding("ISO-8859-1");
                settings.NewLineChars = Environment.NewLine;
                settings.ConformanceLevel = ConformanceLevel.Document;
                if (pretty) settings.Indent = true;

                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    xml.Serialize(new XmlWriterExtended(writer), obj);
                    byte[] bytes = stream.ToArray();
                    string ret = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                    // remove preamble if exists
                    string byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                    while (ret.StartsWith(byteOrderMarkUtf8, StringComparison.Ordinal))
                    {
                        ret = ret.Remove(0, byteOrderMarkUtf8.Length);
                    }

                    return ret;
                }
            }
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
