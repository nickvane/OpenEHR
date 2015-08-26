using System;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Xml.Schema;
using System.IO;

using OpenEhr.DesignByContract;
using OpenEhr.RM.Common.ChangeControl;
using OpenEhr.RM.Impl;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Support.Identification;
using OpenEhr.Utilities;

namespace OpenEhr.Serialisation
{

    public class RmXmlSerializer
    {
        public const string OpenEhrNamespace = "http://schemas.openehr.org/v1";
        public const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        public const string XsdNamespace = "http://www.w3.org/2001/XMLSchema";


        static Lazy<System.Xml.Serialization.XmlSerializer> xmlSchemaSerializer 
            = new Lazy<System.Xml.Serialization.XmlSerializer>(() =>
                new System.Xml.Serialization.XmlSerializer(typeof(XmlSchema)));

        static System.Xml.Serialization.XmlSerializer XmlSchemaSerializer
        {
            get { return xmlSchemaSerializer.Value; }
        }

        private static System.Xml.Schema.XmlSchema baseTypesSchema = null;
        private static object baseTypesSchemaLock = new object();

        internal static XmlSchema GetOpenEhrSchema(string schemaId)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "OpenEhr.Schema." + schemaId + ".xsd";
            XmlSchema schema = null;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new ArgumentException("schemaId", "Schema resource " + resourceName + " not found in manifest");

                schema = (XmlSchema)XmlSchemaSerializer.Deserialize(new XmlTextReader(stream));
            }

            return schema;
        }

        public static void LoadBaseTypesSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            if (!xs.Contains(OpenEhrNamespace))
            {
                if (baseTypesSchema == null)
                {
                    lock (baseTypesSchemaLock)
                    {
                        if (baseTypesSchema == null)
                            baseTypesSchema = GetOpenEhrSchema("BaseTypes");
                    }
                }
                xs.Add(baseTypesSchema);

            }
        }

        private static System.Xml.Schema.XmlSchema structureSchema = null;
        private static object structureSchemaLock = new object();

        public static void LoadStructureSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            if (!xs.Contains(OpenEhrNamespace))
            {
                if (structureSchema == null)
                {
                    lock (structureSchemaLock)
                    {
                        if (structureSchema == null)
                        {
                            System.Xml.Schema.XmlSchema tempSchema = GetOpenEhrSchema("Structure");
                            tempSchema.Includes.RemoveAt(0);

                            System.Xml.Schema.XmlSchema includeSchema = GetOpenEhrSchema("BaseTypes");

                            foreach (System.Xml.Schema.XmlSchemaObject item in includeSchema.Items)
                                tempSchema.Items.Add(item);

                            structureSchema = tempSchema;
                        }
                    }
                }
                xs.Add(structureSchema);
            }
        }

        public static void LoadCompositionSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            if (!xs.Contains(OpenEhrNamespace))
            {
                System.Xml.Schema.XmlSchema compositionSchema = GetOpenEhrSchema("Composition");

                System.Xml.Schema.XmlSchema contentSchema = GetOpenEhrSchema("Content");
                compositionSchema.Includes.RemoveAt(0);

                foreach (System.Xml.Schema.XmlSchemaObject item in contentSchema.Items)
                    compositionSchema.Items.Add(item);

                System.Xml.Schema.XmlSchema structureSchema = GetOpenEhrSchema("Structure");
 
                foreach (System.Xml.Schema.XmlSchemaObject item in structureSchema.Items)
                    compositionSchema.Items.Add(item);

                System.Xml.Schema.XmlSchema baseTypesSchema = GetOpenEhrSchema("BaseTypes");

                foreach (System.Xml.Schema.XmlSchemaObject item in baseTypesSchema.Items)
                    compositionSchema.Items.Add(item);

                xs.Add(compositionSchema);

                xs.Compile();
            }
        }

        public static void LoadEhrStatusSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
           if (!xs.Contains(OpenEhrNamespace))
            {
                System.Xml.Schema.XmlSchema ehrStatusSchema = GetOpenEhrSchema("EhrStatus");
                ehrStatusSchema.Includes.RemoveAt(0);
                
                System.Xml.Schema.XmlSchema structureSchema = GetOpenEhrSchema("Structure");
              
                foreach (System.Xml.Schema.XmlSchemaObject item in structureSchema.Items)
                    ehrStatusSchema.Items.Add(item);

                System.Xml.Schema.XmlSchema baseTypesSchema = GetOpenEhrSchema("BaseTypes");
              
                foreach (System.Xml.Schema.XmlSchemaObject item in baseTypesSchema.Items)
                    ehrStatusSchema.Items.Add(item);

                xs.Add(ehrStatusSchema);

                xs.Compile();
            }
        }

        public static void LoadVersionSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            if (!xs.Contains(OpenEhrNamespace))
            {
                XmlSchema versionSchema = GetOpenEhrSchema("Version");
                versionSchema.Includes.RemoveAt(0);

                XmlSchema schema = GetOpenEhrSchema("Composition");

                foreach (XmlSchemaObject item in schema.Items)
                    versionSchema.Items.Add(item);

                XmlSchema contentSchema = GetOpenEhrSchema("Content");

                foreach (XmlSchemaObject item in contentSchema.Items)
                    versionSchema.Items.Add(item);

                XmlSchema structureSchema = GetOpenEhrSchema("Structure");

                foreach (XmlSchemaObject item in structureSchema.Items)
                    versionSchema.Items.Add(item);

                XmlSchema baseTypesSchema = GetOpenEhrSchema("BaseTypes");

                foreach (XmlSchemaObject item in baseTypesSchema.Items)
                    versionSchema.Items.Add(item);
                
                xs.Add(versionSchema);
            }
        }

        public static void LoadExtractSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            if (!xs.Contains(OpenEhrNamespace))
            {
                System.Xml.Schema.XmlSchema extractSchema = GetOpenEhrSchema("Extract");
                extractSchema.Includes.RemoveAt(0);

                System.Xml.Schema.XmlSchema schema = GetOpenEhrSchema("Version");
                foreach (System.Xml.Schema.XmlSchemaObject item in schema.Items)
                    extractSchema.Items.Add(item);

                System.Xml.Schema.XmlSchema compositionSchema = GetOpenEhrSchema("Composition");
                foreach (System.Xml.Schema.XmlSchemaObject item in compositionSchema.Items)
                    extractSchema.Items.Add(item);

                System.Xml.Schema.XmlSchema contentSchema = GetOpenEhrSchema("Content");
                foreach (System.Xml.Schema.XmlSchemaObject item in contentSchema.Items)
                    extractSchema.Items.Add(item);

                System.Xml.Schema.XmlSchema structureSchema = GetOpenEhrSchema("Structure");
                foreach (System.Xml.Schema.XmlSchemaObject item in structureSchema.Items)
                    extractSchema.Items.Add(item);

                System.Xml.Schema.XmlSchema baseTypesSchema = GetOpenEhrSchema("BaseTypes");
                foreach (System.Xml.Schema.XmlSchemaObject item in baseTypesSchema.Items)
                    extractSchema.Items.Add(item);

                xs.Add(extractSchema);
            }
        }

        public static string UseOpenEhrPrefix(System.Xml.XmlWriter writer)
        {
            string oePrefix = writer.LookupPrefix(OpenEhrNamespace);
            if (oePrefix == null)
            {
                oePrefix = "oe";
                writer.WriteAttributeString("xmlns", oePrefix, null, OpenEhrNamespace);
            }
            return oePrefix;
        }

        public static string UseXsiPrefix(System.Xml.XmlWriter writer)
        {
            string xsiPrefix = writer.LookupPrefix(XsiNamespace);
            if (xsiPrefix == null)
            {
                xsiPrefix = "xsi";
                writer.WriteAttributeString("xmlns", xsiPrefix, null, XsiNamespace);
            }

            return xsiPrefix;
        }

        public static string UseXsdPrefix(System.Xml.XmlWriter writer)
        {
            string xsdPrefix = writer.LookupPrefix(XsdNamespace);
            if (xsdPrefix == null)
            {
                xsdPrefix = "xsd";
                writer.WriteAttributeString("xmlns", xsdPrefix, null, XsdNamespace);
            }
            return xsdPrefix;
        }

        internal static void WriteXml(XmlWriter writer, ObjectId objectId)
        {
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string oePrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            string typeName = ((IRmType)objectId).GetRmTypeName();
            if (!string.IsNullOrEmpty(oePrefix))
                typeName = oePrefix + ":" + typeName;

            writer.WriteAttributeString(xsiPrefix, "type", XsiNamespace, typeName);

            ((System.Xml.Serialization.IXmlSerializable)objectId).WriteXml(writer);
        }

        internal static void WriteXml(XmlWriter writer, ObjectRef objectRef)
        {
            if (objectRef.GetType() != typeof(ObjectRef))
            {
                string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
                string oePrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

                string typeName = ((IRmType)objectRef).GetRmTypeName();
                if (!string.IsNullOrEmpty(oePrefix))
                    typeName = oePrefix + ":" + typeName;

                writer.WriteAttributeString(xsiPrefix, "type", XsiNamespace, typeName);
            }

            ((System.Xml.Serialization.IXmlSerializable)objectRef).WriteXml(writer);
        }

        internal static void WriteXml(XmlWriter writer, DvText text)
        {
            if (text.GetType() != typeof(DvText))
            {
                string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
                string oePrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

                string typeName = ((IRmType)text).GetRmTypeName();
                if (!string.IsNullOrEmpty(oePrefix))
                    typeName = oePrefix + ":" + typeName;

                writer.WriteAttributeString(xsiPrefix, "type", XsiNamespace, typeName);
            }

            ((System.Xml.Serialization.IXmlSerializable)text).WriteXml(writer);
        }

        public static string ReadXsiType(XmlReader reader)
        {
            Check.Require(reader != null, "reader must not be null");

            string xsiType = reader.GetAttribute("type", XsiNamespace);

            if (xsiType != null)
            {
                int index = xsiType.IndexOf(':');
                if (index >= 0)
                    xsiType = xsiType.Substring(index + 1);
            }
            return xsiType;
        }

        /// <summary>
        /// XML serialisation to TextWriter
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="version"></param>
        static public void Serialize<T>(XmlWriter writer, Version<T> version)
            where T : class
        {
            Check.Require(writer != null, "writer type must not be null");
            Check.Require(version != null, "version type must not be null");

            writer.WriteStartElement("version", OpenEhrNamespace);
            version.WriteXml(writer);
            writer.WriteEndElement();
            writer.Flush();
        }

        static public Version<T> Deserialize<T>(XmlReader reader)
            where T : class
        {
            OriginalVersion<T> version = new OriginalVersion<T>();
            version.ReadXml(reader);
            return version;
        }
    }
}
