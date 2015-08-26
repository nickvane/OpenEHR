using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.RM.Impl;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Text
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "TERM_MAPPING")]
    public class TermMapping : RmType, System.Xml.Serialization.IXmlSerializable
    {
        public TermMapping()
        { }

        public TermMapping(char match, DvCodedText purpose, CodePhrase target)
            : this()
        {
            this.match = match;
            this.purpose = purpose;
            this.target = target;

            this.CheckInvariants();
        }

        private char match = char.MinValue;

        public char Match
        {
            get { return this.match; }
        }

        private DvCodedText purpose;

        [RmAttribute("purpose")]
        [RmTerminology("term mapping purpose")]
        public DvCodedText Purpose
        {
            get { return this.purpose; }
        }

        private CodePhrase target;

        public CodePhrase Target
        {
            get { return this.target; }
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

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "match", "local name must be 'match' rather than "+reader.LocalName);
            this.match = reader.ReadElementString("match", RmXmlSerializer.OpenEhrNamespace).ToCharArray()[0];
            reader.MoveToContent();

            if (reader.LocalName == "purpose")
            {
                this.purpose = new DvCodedText();
                this.purpose.ReadXml(reader);               
            }

            Check.Assert(reader.LocalName == "target", "Expected localName is 'target' not "+reader.LocalName);
            this.target = new CodePhrase();
            this.target.ReadXml(reader);

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
                "Expected endElement of TermMapping");
            reader.ReadEndElement();
            reader.MoveToContent();

            this.CheckInvariants();
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            CheckInvariants();

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            writer.WriteElementString(openEhrPrefix, "match", RmXmlSerializer.OpenEhrNamespace, this.Match.ToString());
            if (this.Purpose != null)
            {
                writer.WriteStartElement(openEhrPrefix, "purpose", RmXmlSerializer.OpenEhrNamespace);
                this.Purpose.WriteXml(writer);
                writer.WriteEndElement();
            }

            writer.WriteStartElement(openEhrPrefix, "target", RmXmlSerializer.OpenEhrNamespace);
            this.Target.WriteXml(writer);
            writer.WriteEndElement();

        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("TERM_MAPPING", RmXmlSerializer.OpenEhrNamespace);
        }

        private void CheckInvariants()
        {
            Check.Invariant(this.Match != null, "Match must not be null");
            Check.Invariant(this.Target != null, "target must not be null");
        }
    }
}
