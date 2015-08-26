using System;
using System.Xml;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Basic
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_IDENTIFIER")]
    public sealed class DvIdentifier : DataValue, System.Xml.Serialization.IXmlSerializable
    {
        public DvIdentifier()
        { }

        public DvIdentifier(string issuer, string assigner, string id, string type):
            this()
        {
            this.issuer = issuer;
            this.assigner = assigner;
            this.id = id;
            this.type = type;

            this.CheckInvariants();
        }

        private string issuer;

        public string Issuer
        {
            get
            {
                return this.issuer;
            }
            set
            {
                this.issuer = value;
            }
        }

        private string assigner;

        public string Assigner
        {
            get
            {
                return this.assigner;
            }
            set
            {
                this.assigner = value;
            }
        }

        private string id;

        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        private string type;

        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        public override string ToString()
        {
            throw new Exception("The method or operation is not implemented.");
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

        protected override void ReadXmlBase(XmlReader reader)
        {
            Check.Assert(reader.LocalName == "issuer", "local name must be 'issuer'");
            this.issuer = reader.ReadElementString("issuer", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "assigner", "local name must be 'assigner'");
            this.assigner = reader.ReadElementString("assigner", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "id", "local name must be 'id'");
            this.id = reader.ReadElementString("id", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "type", "local name must be 'type'");
            this.type = reader.ReadElementString("type", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();
        }

        protected override void WriteXmlBase(XmlWriter writer)
        {
            writer.WriteElementString("issuer", RmXmlSerializer.OpenEhrNamespace, this.Issuer);
            writer.WriteElementString("assigner", RmXmlSerializer.OpenEhrNamespace, this.Assigner);
            writer.WriteElementString("id", RmXmlSerializer.OpenEhrNamespace, this.Id);
            writer.WriteElementString("type", RmXmlSerializer.OpenEhrNamespace, this.Type);
        }

        public static XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new XmlQualifiedName("DV_IDENTIFIER", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void CheckInvariants()
        {
            Check.Invariant(this.Issuer != null && this.Issuer.Length > 0, "Issuer must not be null or empty.");
            Check.Invariant(this.Assigner != null && this.Assigner.Length > 0, "Issuer must not be null or empty.");
            Check.Invariant(this.Id != null && this.Id.Length > 0, "Id must not be null or empty.");
            Check.Invariant(this.Type != null && this.Type.Length > 0, "Type must not be null or empty.");
        }
    }
}
