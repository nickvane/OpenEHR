using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Common.Archetyped.Impl
{
    /// <summary>
    /// The LINK type defines a logical relationship between two items, such as two
    /// ENTRYs or an ENTRY and a COMPOSITION. Links can be used across compositions,
    /// and across EHRs. Links can potentially be used between interior 
    /// (i.e. nonarchetype root) nodes, although this probably should be prevented in archetypes.
    /// Multiple LINKs can be attached to the root object of any archetyped structure 
    /// to give the effect of a 1->N link
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "COMMON", "LINK")]
    public class Link: OpenEhr.RM.Common.Archetyped.Link, System.Xml.Serialization.IXmlSerializable
    {
        #region constructors

        public Link(DataTypes.Text.DvText meaning, DataTypes.Text.DvText type,
            DataTypes.Uri.DvEhrUri target)
            : this()
        {
            Check.Require(meaning != null && type != null && target != null);
            this.meaning = meaning;
            this.type = type;
            this.target = target;

            CheckInvariants();
        }

        public Link()
        { }

        #endregion

        private DataTypes.Text.DvText meaning;
        
        /// <summary>
        /// Used to describe the relationship, usually in
        /// clinical terms, such as “in response to” (the
        /// relationship between test results and an order),
        /// “follow-up to” and so on. Such relationships
        /// can represent any clinically meaningful connection
        /// between pieces of information.
        /// </summary>        
        public override DataTypes.Text.DvText Meaning
        {
            get
            {
                return this.meaning;
            }
        }

        private DataTypes.Text.DvText type;

        /// <summary>
        /// The type attribute is used to indicate a clinical
        /// or domain-level meaning for the kind of link,
        /// for example “problem” or “issue”. If type values
        /// are designed appropriately, they can be used
        /// by the requestor of EHR extracts to categorise
        /// links which must be followed and which can be
        /// broken when the extract is created.
        /// </summary>
        public override DataTypes.Text.DvText Type
        {
            get
            {
                return this.type;
            }
        }

        private DataTypes.Uri.DvEhrUri target;

        /// <summary>
        /// The logical “to” object in the link relation, as
        /// per the linguistic sense of the meaning attribute.
        /// </summary>
        public override DataTypes.Uri.DvEhrUri Target
        {
            get
            {
                return this.target;
            }
            set
            {
                Check.Require(value != null, "Target must not be null");
                this.target = value;
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

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("LINK", RmXmlSerializer.OpenEhrNamespace);
        }

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "meaning", "Expected local name is 'meaning', not "+reader.LocalName);
            string meaningType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
            if (meaningType != null && meaningType.IndexOf("DV_CODED_TEXT") >= 0)
                this.meaning = new OpenEhr.RM.DataTypes.Text.DvCodedText();
            else
                this.meaning = new OpenEhr.RM.DataTypes.Text.DvText();
            this.meaning.ReadXml(reader);

            Check.Assert(reader.LocalName == "type", "Expected local name is 'type', not " + reader.LocalName);
            string typeType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
            if (typeType != null && typeType.IndexOf("DV_CODED_TEXT") >= 0)
                this.type = new OpenEhr.RM.DataTypes.Text.DvCodedText();
            else
                this.type = new OpenEhr.RM.DataTypes.Text.DvText();
            this.type.ReadXml(reader);

            Check.Assert(reader.LocalName == "target", "Expected local name is 'target', not " + reader.LocalName);
            this.target = new OpenEhr.RM.DataTypes.Uri.DvEhrUri();
            this.target.ReadXml(reader);

            reader.MoveToContent();

            if (!reader.IsStartElement())
            {
                reader.ReadEndElement();
                reader.MoveToContent();
            }

            CheckInvariants();
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            CheckInvariants();

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
           
            writer.WriteStartElement(openEhrPrefix, "meaning", RmXmlSerializer.OpenEhrNamespace);
            if (this.Meaning.GetType() == typeof(OpenEhr.RM.DataTypes.Text.DvCodedText))
            {
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, openEhrPrefix + ":DV_CODED_TEXT");
                else
                    writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, "DV_CODED_TEXT");
            }
            this.Meaning.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement(openEhrPrefix, "type", RmXmlSerializer.OpenEhrNamespace);
            if (this.Type.GetType() == typeof(OpenEhr.RM.DataTypes.Text.DvCodedText))
            {
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, openEhrPrefix + ":DV_CODED_TEXT");
                else
                    writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, "DV_CODED_TEXT");
            }
            this.Type.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement(openEhrPrefix, "target", RmXmlSerializer.OpenEhrNamespace);
            this.Target.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
