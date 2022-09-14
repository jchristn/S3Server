using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace S3ServerLibrary
{
    internal class XmlWriterExtended : XmlWriter
    {
        private XmlWriter baseWriter;

        public XmlWriterExtended(XmlWriter w)
        {
            baseWriter = w;
        }

        // Force WriteEndElement to use WriteFullEndElement
        public override void WriteEndElement() { baseWriter.WriteFullEndElement(); }

        public override void WriteFullEndElement()
        {
            baseWriter.WriteFullEndElement();
        }

        public override void Close()
        {
            baseWriter.Close();
        }

        public override void Flush()
        {
            baseWriter.Flush();
        }

        public override string LookupPrefix(string ns)
        {
            return (baseWriter.LookupPrefix(ns));
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            baseWriter.WriteBase64(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            baseWriter.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            baseWriter.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            baseWriter.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            baseWriter.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            baseWriter.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteEndAttribute()
        {
            baseWriter.WriteEndAttribute();
        }

        public override void WriteEndDocument()
        {
            baseWriter.WriteEndDocument();
        }

        public override void WriteEntityRef(string name)
        {
            baseWriter.WriteEntityRef(name);
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            baseWriter.WriteProcessingInstruction(name, text);
        }

        public override void WriteRaw(string data)
        {
            baseWriter.WriteRaw(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            baseWriter.WriteRaw(buffer, index, count);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            baseWriter.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartDocument(bool standalone)
        {
            baseWriter.WriteStartDocument(standalone);
        }

        public override void WriteStartDocument()
        {
            baseWriter.WriteStartDocument();
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            baseWriter.WriteStartElement(prefix, localName, ns);
        }

        public override WriteState WriteState
        {
            get { return baseWriter.WriteState; }
        }

        public override void WriteString(string text)
        {
            baseWriter.WriteString(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            baseWriter.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            baseWriter.WriteWhitespace(ws);
        }
    }
}
