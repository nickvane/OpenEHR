using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Common.Archetyped.Impl
{
    /// <summary>
    /// Archetypes act as the configuration basis for the particular structures of instances
    /// defined by the reference model. To enable archetypes to be used to create valid
    /// data, key classes in the reference model act as “root” points for archetyping;
    /// accordingly, these classes have the archetype_details attribute set. An instance of
    /// the class ARCHETYPED contains the relevant archetype identification information,
    /// allowing generating archetypes to be matched up with data instances
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "COMMON", "ARCHETYPED")]
    public class Archetyped : OpenEhr.RM.Common.Archetyped.Archetyped, System.Xml.Serialization.IXmlSerializable
    {
        public Archetyped()
        { }

        public Archetyped(Support.Identification.ArchetypeId archetypeId, string rmVersion, 
            Support.Identification.TemplateId templateId)
        {
            Check.Require(archetypeId != null, "archetype_id must not be null");
            Check.Require(!string.IsNullOrEmpty(rmVersion), "rm_version must not be null or empty");
            this.archetypeId = archetypeId;
            this.rmVersion = rmVersion;
            this.templateId = templateId;

            CheckInvariants();
        }

        private Support.Identification.ArchetypeId archetypeId;

        public override Support.Identification.ArchetypeId ArchetypeId
        {
            get
            {
                return this.archetypeId;
            }
        }

        private string rmVersion;
        
        public override string RmVersion
        {
            get
            {
                return this.rmVersion;
            }
        }

        private Support.Identification.TemplateId templateId;
        public override Support.Identification.TemplateId TemplateId
        {
            get
            {
                return this.templateId;
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
            return new System.Xml.XmlQualifiedName("ARCHETYPED", RmXmlSerializer.OpenEhrNamespace);
        }

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "archetype_id", "Expected local name is 'archetype_id', not " + reader.LocalName);
            this.archetypeId = new OpenEhr.RM.Support.Identification.ArchetypeId();
            this.archetypeId.ReadXml(reader);

            if (reader.LocalName == "template_id")
            {
                this.templateId = new OpenEhr.RM.Support.Identification.TemplateId();
                this.templateId.ReadXml(reader);
            }

            Check.Assert(reader.LocalName == "rm_version", "Expected local name is 'rm_version', not " + reader.LocalName);
            this.rmVersion = reader.ReadElementString("rm_version", RmXmlSerializer.OpenEhrNamespace);            

            reader.MoveToContent();

            if (!reader.IsStartElement())
            {
                reader.ReadEndElement();
                reader.MoveToContent();
            }
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            this.CheckInvariants();

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "archetype_id", RmXmlSerializer.OpenEhrNamespace);
            this.ArchetypeId.WriteXml(writer);
            writer.WriteEndElement();

            if (this.TemplateId != null)
            {
                writer.WriteStartElement(openEhrPrefix, "template_id", RmXmlSerializer.OpenEhrNamespace);
                this.TemplateId.WriteXml(writer);
                writer.WriteEndElement();
            }

            writer.WriteElementString(openEhrPrefix, "rm_version", RmXmlSerializer.OpenEhrNamespace, this.RmVersion);

        }
    }
}
