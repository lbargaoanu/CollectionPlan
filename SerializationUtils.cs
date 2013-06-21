using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IntelliTrace.Config
{
    public static class SerializationUtils
    {
        public static string ToXml(this object value)
        {
            return ToXml(value, string.Empty);
        }

        public static string ToXml(this object value, string defaultNamespace)
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            var xmlWriter = new XmlTextWriter(writer) { Formatting = Formatting.Indented};
            ToXml(value, defaultNamespace, xmlWriter);
            return writer.ToString();
        }

        public static void ToXml(this object value, Stream stream)
        {
            var xmlWriter = new XmlTextWriter(stream, Encoding.UTF8) { Formatting = Formatting.Indented };
            ToXml(value, string.Empty, xmlWriter);
        }

        public static XElement ToXElement(this object value)
        {
            return ToXElement(value, string.Empty);
        }

        public static XElement ToXElement(this object value, string defaultNamespace)
        {
            return XElement.Parse(value.ToXml(defaultNamespace));
        }

        public static void ToXml(this object value, string defaultNamespace, XmlTextWriter xmlTextWriter)
        {
            using(xmlTextWriter)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(value.GetType(), defaultNamespace);
                xmlSerializer.Serialize(xmlTextWriter, value);
            }
        }

        public static T FromXElement<T>(XElement xElement)
        {
            return FromXElement<T>(xElement, string.Empty);
        }

        public static T FromXElement<T>(XElement xElement, string defaultNamespace)
        {
            return FromXml<T>(xElement.CreateReader(), defaultNamespace);
        }

        public static T FromXml<T>(Stream xmlStream)
        {
            return FromXml<T>(xmlStream, string.Empty);
        }

        public static T FromXml<T>(Stream xmlStream, string defaultNamespace)
        {
            return FromXml<T>(new XmlTextReader(xmlStream), defaultNamespace);
        }

        public static T FromXml<T>(string xml)
        {
            return FromXml<T>(xml, string.Empty);
        }

        public static T FromXml<T>(string xml, string defaultNamespace)
        {
            return FromXml<T>(new XmlTextReader(xml, XmlNodeType.Document, null), defaultNamespace);
        }

        public static T FromXml<T>(XmlReader reader)
        {
            return FromXml<T>(reader, string.Empty);
        }

        public static T FromXml<T>(XmlReader reader, string defaultNamespace)
        {
            using(reader)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), defaultNamespace);
                return (T) xmlSerializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// XML serializes an Object instance.
        /// </summary>
        /// <param name="obj">object to serialize.</param>
        /// <param name="stream">stream to write xml to</param>
        public static void XmlSerialize<T>(this T obj, Stream stream)
        {
            var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings() { NewLineOnAttributes = true, Indent = true, IndentChars =" " });
            XmlSerialize<T>(xmlWriter, obj);
        }

        /// <summary>
        /// XML serializes an Object instance.
        /// </summary>
        /// <param name="obj">object to serialize.</param>
        /// <param name="xmlWriter">xmlWriter to write xml to</param>
        public static void XmlSerialize<T>(this T obj, XmlWriter xmlWriter)
        {
            XmlSerialize<T>(xmlWriter, obj);
        }

        /// <summary>
        /// XML serializes an Object instance.
        /// </summary>
        /// <param name="obj">object to serialize.</param>
        /// <param name="stream">xmlWriter to write xml to</param>
        public static void XmlSerialize<T>(this XmlWriter xmlWriter, T obj)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            xs.Serialize(xmlWriter, obj);
        }

        /// <summary>
        /// XML deserializes
        /// </summary>
        /// <param name="xmlReader">xmlReader to read xml from</param>
        /// <returns>Deserialized object</returns>
        public static T XmlDeserialize<T>(this XmlReader xmlReader)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            return (T)xs.Deserialize(xmlReader);
        }
    }


    public static class Xsi
    {
        public static XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        public static XName schemaLocation = xsi + "schemaLocation";
        public static XName noNamespaceSchemaLocation = xsi + "noNamespaceSchemaLocation";

        public static XDocument Normalize(XDocument source, XmlSchemaSet schema)
        {
            bool havePSVI = false;
            // validate, throw errors, add PSVI information
            if(schema != null)
            {
                source.Validate(schema, null, true);
                havePSVI = true;
            }
            return new XDocument(
                source.Declaration,
                source.Nodes().Select(n =>
                {
                    // Remove comments, processing instructions, and text nodes that are
                    // children of XDocument.  Only white space text nodes are allowed as
                    // children of a document, so we can remove all text nodes.
                    if(n is XComment || n is XProcessingInstruction || n is XText)
                        return null;
                    XElement e = n as XElement;
                    if(e != null)
                        return NormalizeElement(e, havePSVI);
                    return n;
                }
                )
            );
        }

        public static bool DeepEqualsWithNormalization(XElement element1, XElement element2, XmlSchemaSet schemaSet = null)
        {
            var doc1 = new XDocument(element1);
            var doc2 = new XDocument(element2);
            XDocument d1 = Normalize(doc1, schemaSet);
            XDocument d2 = Normalize(doc2, schemaSet);
            return XNode.DeepEquals(d1, d2);
        }

        public static bool DeepEqualsWithNormalization(XDocument doc1, XDocument doc2, XmlSchemaSet schemaSet)
        {
            XDocument d1 = Normalize(doc1, schemaSet);
            XDocument d2 = Normalize(doc2, schemaSet);
            return XNode.DeepEquals(d1, d2);
        }

        private static IEnumerable<XAttribute> NormalizeAttributes(XElement element, bool havePSVI)
        {
            return element.Attributes()
                    .Where(a => !a.IsNamespaceDeclaration &&
                        a.Name != Xsi.schemaLocation &&
                        a.Name != Xsi.noNamespaceSchemaLocation)
                    .OrderBy(a => a.Name.NamespaceName)
                    .ThenBy(a => a.Name.LocalName)
                    .Select(
                        a =>
                        {
                            if(havePSVI)
                            {
                                var dt = a.GetSchemaInfo().SchemaType.TypeCode;
                                switch(dt)
                                {
                                    case XmlTypeCode.Boolean:
                                        return new XAttribute(a.Name, (bool)a);
                                    case XmlTypeCode.DateTime:
                                        return new XAttribute(a.Name, (DateTime)a);
                                    case XmlTypeCode.Decimal:
                                        return new XAttribute(a.Name, (decimal)a);
                                    case XmlTypeCode.Double:
                                        return new XAttribute(a.Name, (double)a);
                                    case XmlTypeCode.Float:
                                        return new XAttribute(a.Name, (float)a);
                                    case XmlTypeCode.HexBinary:
                                    case XmlTypeCode.Language:
                                        return new XAttribute(a.Name,
                                            ((string)a).ToLower());
                                }
                            }
                            return a;
                        }
                    );
        }

        private static XNode NormalizeNode(XNode node, bool havePSVI)
        {
            // trim comments and processing instructions from normalized tree
            if(node is XComment || node is XProcessingInstruction)
                return null;
            XElement e = node as XElement;
            if(e != null)
                return NormalizeElement(e, havePSVI);
            // Only thing left is XCData and XText, so clone them
            return node;
        }

        private static XElement NormalizeElement(XElement element, bool havePSVI)
        {
            if(havePSVI)
            {
                var dt = element.GetSchemaInfo();
                switch(dt.SchemaType.TypeCode)
                {
                    case XmlTypeCode.Boolean:
                        return new XElement(element.Name,
                            NormalizeAttributes(element, havePSVI),
                            (bool)element);
                    case XmlTypeCode.DateTime:
                        return new XElement(element.Name,
                            NormalizeAttributes(element, havePSVI),
                            (DateTime)element);
                    case XmlTypeCode.Decimal:
                        return new XElement(element.Name,
                            NormalizeAttributes(element, havePSVI),
                            (decimal)element);
                    case XmlTypeCode.Double:
                        return new XElement(element.Name,
                            NormalizeAttributes(element, havePSVI),
                            (double)element);
                    case XmlTypeCode.Float:
                        return new XElement(element.Name,
                            NormalizeAttributes(element, havePSVI),
                            (float)element);
                    case XmlTypeCode.HexBinary:
                    case XmlTypeCode.Language:
                        return new XElement(element.Name,
                            NormalizeAttributes(element, havePSVI),
                            ((string)element).ToLower());
                    default:
                        return new XElement(element.Name,
                            NormalizeAttributes(element, havePSVI),
                            element.Nodes().Select(n => NormalizeNode(n, havePSVI))
                        );
                }
            }
            else
            {
                return new XElement(element.Name,
                    NormalizeAttributes(element, havePSVI),
                    element.Nodes().Select(n => NormalizeNode(n, havePSVI))
                );
            }
        }
    }
}