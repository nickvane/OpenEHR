using System;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Support.Identification
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "SUPPORT", "PARTY_REF")]
    public sealed class PartyRef : ObjectRef, System.Xml.Serialization.IXmlSerializable
    {
        public PartyRef() 
        { }

        public PartyRef(ObjectId objectId, string namespaceValue, string typeValue)
            : this()
        {
            SetBaseData(objectId, namespaceValue, typeValue);
        }

        #region IXmlSerializable Members

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            this.ReadXml(reader);
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            this.WriteXml(writer);
        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName(RmTypeName, RmXmlSerializer.OpenEhrNamespace);

        }

        #endregion

        const string RmTypeName = "PARTY_REF";
    }
}
