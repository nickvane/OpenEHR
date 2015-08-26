using System;
using System.Xml;

using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.DataTypes.Basic;
using OpenEhr.AssumedTypes;

namespace OpenEhr.RM.Common.Generic
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "COMMON", "PARTY_IDENTIFIED")]
    public class PartyIdentified : PartyProxy, System.Xml.Serialization.IXmlSerializable
    {
        public PartyIdentified()
            : base()
        { }

        public PartyIdentified(string partyName) 
            : this()
        {
            this.name = partyName;
        }

        public PartyIdentified(string partyName, List<DvIdentifier> identifiers,
                Support.Identification.PartyRef externalRef)
            : this()
        {
            this.name = partyName;
            this.identifiers = identifiers;

            this.SetBaseData(externalRef);
        }

        public PartyIdentified(string partyName, Support.Identification.PartyRef externalRef)
            : this()
        {
            this.name = partyName;
            this.SetBaseData(externalRef);
        }

        private string name;

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        private List<DvIdentifier> identifiers;

        public List<DvIdentifier> Identifiers
        {
            get
            {
                return this.identifiers;
            }
        }

        public override string ToString()
        {
            return this.Name;
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
            base.ReadXmlBase(reader);

            if (reader.LocalName == "name")
            {
                this.name = reader.ReadElementString("name", RmXmlSerializer.OpenEhrNamespace);
            }

            reader.MoveToContent();

            if (reader.LocalName == "identifiers")
            {
                if (this.identifiers == null)
                    this.identifiers = new List<OpenEhr.RM.DataTypes.Basic.DvIdentifier>();
                do
                {
                    OpenEhr.RM.DataTypes.Basic.DvIdentifier id = new OpenEhr.RM.DataTypes.Basic.DvIdentifier();
                    id.ReadXml(reader);

                    this.identifiers.Add(id);

                } while (reader.LocalName == "identifiers");
            }
        }

        protected override void WriteXmlBase(XmlWriter writer)
        {            
            base.WriteXmlBase(writer);
            if (this.Name != null)
                writer.WriteElementString("name", RmXmlSerializer.OpenEhrNamespace, this.Name);
            if (this.Identifiers != null)
            {
                foreach (DvIdentifier id in this.Identifiers)
                {
                    writer.WriteStartElement("identifiers", RmXmlSerializer.OpenEhrNamespace);
                    id.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }
             

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName(RmTypeName, RmXmlSerializer.OpenEhrNamespace);

        }

        protected override void CheckInvariants()
        {
            /* %HYYKA% (need this in future?)
            DesignByContract.Check.Invariant(this.Name != null || this.Identifiers != null ||
                this.ExternalRef != null, "Basic_valid name /= Void or identifiers /= Void or external_ref /= Void");
            DesignByContract.Check.Invariant(this.Name == null || this.Name.Length > 0,
                "Name_valid: name /= Void implies not name.is_empty");
            DesignByContract.Check.Invariant(this.Identifiers == null || this.Identifiers.Count > 0,
                "Identifiers_valid: identifiers /= Void implies not identifiers.is_empty");
            */
        }

        protected void SetBaseData(string name, List<DvIdentifier> identifiers,
            OpenEhr.RM.Support.Identification.PartyRef externalRef)
        {
            base.SetBaseData(externalRef);
            this.name = name;
            this.identifiers = identifiers;
        }

        public const string RmTypeName = "PARTY_IDENTIFIED";
    }
}
