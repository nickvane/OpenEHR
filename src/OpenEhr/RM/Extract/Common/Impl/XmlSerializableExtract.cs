using System;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Extract.Common.Impl
{
    [System.Xml.Serialization.XmlRoot("extract", Namespace = "http://schemas.openehr.org/v1")]
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    public sealed class XmlSerializableExtract : Extract, System.Xml.Serialization.IXmlSerializable
    {
        public XmlSerializableExtract()
        { }

        public XmlSerializableExtract(string archetypeNodeId, DvText name, HierObjectId systemId)
            : base(archetypeNodeId, name, systemId)
        { }

        public XmlSerializableExtract(string archetypeNodeId, DvText name, HierObjectId systemId, ExtractChapter[] chapters)
            : base(archetypeNodeId, name, systemId)
        {
            foreach (ExtractChapter chapter in chapters)
                this.Chapters.Add(chapter);
        }

        #region IXmlSerializable Members

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            ExtractXmlSerializer serializer = new ExtractXmlSerializer();
            serializer.ReadXml(reader, this);
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            ExtractXmlSerializer serializer = new ExtractXmlSerializer();
            serializer.WriteXml(writer, this);
        }

        static public System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadExtractSchema(xs);
            return new System.Xml.XmlQualifiedName("EXTRACT", RmXmlSerializer.OpenEhrNamespace);
        }

        #endregion
    }
}
