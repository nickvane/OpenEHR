using System;
using OpenEhr.RM.DataTypes.Basic;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.DataStructures.ItemStructure.Representation
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_STRUCTURES", "ELEMENT")]
    public class Element : Item, System.Xml.Serialization.IXmlSerializable
    {
        public Element()
        { }

        public Element(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
            Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit,
            DataValue value, DvCodedText nullFlavour)
            :base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit)
        {
            this.value = value;
            this.nullFlavour = nullFlavour;

            SetAttributeDictionary();
            CheckInvariants();
        }

        private DataTypes.Text.DvCodedText nullFlavour;

        [RmAttribute("null_flavour")]
        [RmTerminology("null flavours")]
        public DataTypes.Text.DvCodedText NullFlavour
        {
            get
            {
                if(this.nullFlavour == null)
                    this.nullFlavour = base.attributesDictionary["null_flavour"] as DataTypes.Text.DvCodedText;
                return this.nullFlavour;
            }
            set
            {
                this.nullFlavour = value;
                base.attributesDictionary["key"] = this.nullFlavour;
            }
        }

        private DataTypes.Basic.DataValue value;

        [RmAttribute("value")]
        public DataTypes.Basic.DataValue Value
        {
            get
            {
                if(this.value == null)
                    this.value = base.attributesDictionary["value"] as DataTypes.Basic.DataValue;
                return this.value;
            }
            set
            {
                this.value = value;
                base.attributesDictionary["value"] = this.value;
            }
        }
    
        public bool IsNull()
        {
            throw new System.NotImplementedException();
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
            RmXmlSerializer.LoadCompositionSchema(xs);
            return new System.Xml.XmlQualifiedName("ELEMENT", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            if (reader.LocalName == "value")
            {
                this.value = DataTypes.Basic.DataValue.ReadDataValue(reader);
            }

            if (reader.LocalName == "null_flavour")
            {
                this.nullFlavour = new OpenEhr.RM.DataTypes.Text.DvCodedText();
                this.nullFlavour.ReadXml(reader);
            }

        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            if (this.Value != null)
            {
                writer.WriteStartElement(openEhrPrefix, "value", RmXmlSerializer.OpenEhrNamespace);

                string valueType = ((IRmType)this.Value).GetXmlRmTypeName();
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    valueType = openEhrPrefix + ":" + valueType;
                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, valueType);
                this.Value.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.NullFlavour != null)
            {
                writer.WriteStartElement(openEhrPrefix, "null_flavour", RmXmlSerializer.OpenEhrNamespace);
                this.NullFlavour.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();

            base.attributesDictionary["value"] = this.value;
            base.attributesDictionary["null_flavour"] = this.nullFlavour;
        }
    }
}
