using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Support.Identification
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "SUPPORT", "TEMPLATE_ID")]
    public class TemplateId : ObjectId, System.Xml.Serialization.IXmlSerializable
    {
        public TemplateId()
        { }
        
        public TemplateId(string value)
            : this()
        {
            SetBaseData(value);
        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName(RmTypeName, RmXmlSerializer.OpenEhrNamespace);

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

        const string RmTypeName = "TEMPLATE_ID";

        protected override bool IsValidValue(string value)
        {
            Check.Require(value != null, "value must not be null");

            return value != string.Empty;
        }
    }
}
