using System;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Extract.Common.Impl
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    public sealed class XmlSerializableXVersionedObject<T> : XVersionedObject<T>, System.Xml.Serialization.IXmlSerializable
        where T : class
    {
        public XmlSerializableXVersionedObject()
        { }

        public XmlSerializableXVersionedObject(XVersionedObject<T> versionedComposition)
        {
            this.Uid = versionedComposition.Uid;
            this.OwnerId = versionedComposition.OwnerId;
            this.TimeCreated = versionedComposition.TimeCreated;
            this.TotalVersionCount = versionedComposition.TotalVersionCount;
            this.Versions = versionedComposition.Versions;
        }

        #region IXmlSerializable Members

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            ExtractXmlSerializer serializer = new ExtractXmlSerializer();
            serializer.ReadXml<T>(reader, this);
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            ExtractXmlSerializer serializer = new ExtractXmlSerializer();
            serializer.WriteXml<T>(writer, this);
        }

        static public System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadExtractSchema(xs);
            return new System.Xml.XmlQualifiedName("X_VERSIONED_OBJECT", RmXmlSerializer.OpenEhrNamespace);
        }

        #endregion
    }
}
