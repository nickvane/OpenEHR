using System;
using OpenEhr.RM.DataTypes.Basic;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Support.Identification;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.AssumedTypes;

namespace OpenEhr.RM.Common.Generic
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "COMMON", "PARTY_RELATED")]
    public class PartyRelated : PartyIdentified, System.Xml.Serialization.IXmlSerializable
    {
        public PartyRelated()
        { }

        public PartyRelated(string name, List<DvIdentifier> identifiers, DvCodedText relationship,
            PartyRef externalRef)
        {
            Check.Require(relationship != null, "relationship must not be null");

            this.relationship = relationship;

            SetBaseData(name, identifiers, externalRef);
        }

        DataTypes.Text.DvCodedText relationship;

        [RmAttribute("relationship", 1)]
        [RmTerminology("subject relationship")]
        public DataTypes.Text.DvCodedText Relationship
        {
            get { 
                return this.relationship;
            }
        }

        protected override void CheckInvariants()
        {
            base.CheckInvariants();
            DesignByContract.Check.Invariant(this.Relationship != null, "relationship must not be null.");
        }

        #region serialization
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

            DesignByContract.Check.Assert(reader.LocalName == "relationship",
                "Expected localName is relationship not " + reader.LocalName);
            this.relationship = new DvCodedText();
            this.relationship.ReadXml(reader);
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            writer.WriteStartElement(prefix, "relationship", RmXmlSerializer.OpenEhrNamespace);
            this.Relationship.WriteXml(writer);
            writer.WriteEndElement();
        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName(RmTypeName, RmXmlSerializer.OpenEhrNamespace);

        }
        #endregion
    }
}
