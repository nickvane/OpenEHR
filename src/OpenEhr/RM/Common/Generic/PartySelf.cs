using System;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Common.Generic
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "COMMON", "PARTY_SELF")]
    public class PartySelf : PartyProxy, System.Xml.Serialization.IXmlSerializable
    {
        public PartySelf()
            : base()
        { }

        public PartySelf(OpenEhr.RM.Support.Identification.PartyRef externalRef)
            : this()
        {
            this.SetBaseData(externalRef);
        }

        public override string ToString()
        {
            return "Self";
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

        #endregion

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName(RmTypeName, RmXmlSerializer.OpenEhrNamespace);
        }

        public const string RmTypeName = "PARTY_SELF";
    }
}
