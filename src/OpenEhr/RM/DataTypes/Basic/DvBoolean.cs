using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Basic
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_BOOLEAN")]
    public class DvBoolean : DataValue, System.Xml.Serialization.IXmlSerializable
    {
        public DvBoolean()
        { }

        public DvBoolean(bool value)
            : this()
        {
            this.value = value;
        }

        private Boolean value;
        private Boolean valueSet;

        public Boolean Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                this.valueSet = true;
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
            Check.Assert(reader.LocalName == "value", "local name must be 'value rather than '" + reader.LocalName);
            this.value = reader.ReadElementContentAsBoolean("value", RmXmlSerializer.OpenEhrNamespace);
            this.valueSet = true;
            reader.MoveToContent();

        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            writer.WriteElementString(openEhrPrefix, "value", RmXmlSerializer.OpenEhrNamespace, this.Value.ToString().ToLower());

        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("DV_BOOLEAN", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void CheckInvariants()
        {
            Check.Invariant(this.valueSet, "value must have been set.");
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
