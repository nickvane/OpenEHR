using System;
using System.ComponentModel;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Support.Identification
{
    [TypeConverter(typeof(TerminologyIdTypeConverter))]
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "SUPPORT", "GENERIC_ID")]
    public class GenericId : ObjectId, System.Xml.Serialization.IXmlSerializable
    {
        public GenericId()
        { }

        public GenericId(string value, string scheme)
            : this() 
        {
            this.scheme = scheme;
            SetBaseData(value);
            this.CheckInvariants();
        }

        private string scheme;

        public string Scheme
        {
            get
            {
                return this.scheme;
            }
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

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);
            DesignByContract.Check.Assert(reader.LocalName == "scheme", 
                "Expected LocalName is 'scheme' not "+reader.LocalName);
            this.scheme = reader.ReadElementString("scheme", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string openEhrNamespace = RmXmlSerializer.OpenEhrNamespace;
            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            writer.WriteElementString(prefix, "scheme", openEhrNamespace, this.Scheme);
        }

        protected override void CheckInvariants()
        {
            base.CheckInvariants();
            DesignByContract.Check.Invariant(this.Scheme != null, "scheme must not be null");
        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName(RmTypeName, RmXmlSerializer.OpenEhrNamespace);
        }

        const string RmTypeName = "GENERIC_ID";

        protected override bool IsValidValue(string value)
        {
            Check.Require(value != null, "value must not be null");

            return value != string.Empty;
        }
    }
}
