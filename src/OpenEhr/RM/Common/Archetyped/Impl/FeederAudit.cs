using System;
using System.Xml;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.DataTypes.Encapsulated;
using OpenEhr.RM.DataTypes.Basic;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Common.Archetyped.Impl
{
    /// <summary>
    /// Audit and other meta-data for systems in the feeder chain
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "COMMON", "FEEDER_AUDIT")]
    public class FeederAudit : OpenEhr.RM.Common.Archetyped.FeederAudit, System.Xml.Serialization.IXmlSerializable
    {
        public FeederAudit() 
        { }

        public FeederAudit(FeederAuditDetails originatingSystemAudit)
            : this()
        {
            Check.Require(originatingSystemAudit != null);

            this.originatingSystemAudit = originatingSystemAudit;
        }

        public FeederAudit(FeederAuditDetails originatingSystemAudit,
            AssumedTypes.List<DataTypes.Basic.DvIdentifier> originatingSystemItemIds,            
            DataTypes.Encapsulated.DvEncapsulated originalContent)
            : this()
        {
            Check.Require(originatingSystemAudit != null);
            this.originatingSystemAudit = originatingSystemAudit;
            this.originatingSystemItemIds = originatingSystemItemIds;
            this.originalContent = originalContent;
        }


        public FeederAudit(FeederAuditDetails originatingSystemAudit,
            AssumedTypes.List<DataTypes.Basic.DvIdentifier> originatingSystemItemIds,
            FeederAuditDetails feederSystemAudit,
            AssumedTypes.List<DataTypes.Basic.DvIdentifier> feederSystemItemIds,
            DataTypes.Encapsulated.DvEncapsulated originalContent)
            : this()
        {
            Check.Require(originatingSystemAudit != null);
            
            this.originatingSystemAudit = originatingSystemAudit;
            this.originatingSystemItemIds = originatingSystemItemIds;
            this.feederSystemAudit = feederSystemAudit;
            this.feederSystemItemIds = feederSystemItemIds;
            this.originalContent = originalContent;
        }

        private FeederAuditDetails originatingSystemAudit;

        public override FeederAuditDetails OriginatingSystemAudit
        {
            get { return this.originatingSystemAudit; }
        }

        private FeederAuditDetails feederSystemAudit;

        public override FeederAuditDetails FeederSystemAudit
        {
            get { return this.feederSystemAudit; }
        }

        private AssumedTypes.List<DataTypes.Basic.DvIdentifier> originatingSystemItemIds;

        public override AssumedTypes.List<DataTypes.Basic.DvIdentifier> OriginatingSystemItemIds
        {
            get { return this.originatingSystemItemIds; }        
        }

        private AssumedTypes.List<DataTypes.Basic.DvIdentifier> feederSystemItemIds;

        public override AssumedTypes.List<DataTypes.Basic.DvIdentifier> FeederSystemItemIds
        {
            get { return this.feederSystemItemIds; }
        }

        private DataTypes.Encapsulated.DvEncapsulated originalContent;

        public override DataTypes.Encapsulated.DvEncapsulated OriginalContent
        {
            get { return originalContent; }
            set
            {
                if (value != null)
                    this.originalContent = value;
                else
                    originalContent = null;
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

        public static XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new XmlQualifiedName("FEEDER_AUDIT", RmXmlSerializer.OpenEhrNamespace);
        }

        internal void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName == "originating_system_item_ids")
            {
                this.originatingSystemItemIds = new OpenEhr.AssumedTypes.List<OpenEhr.RM.DataTypes.Basic.DvIdentifier>();
                do
                {
                    OpenEhr.RM.DataTypes.Basic.DvIdentifier id = new OpenEhr.RM.DataTypes.Basic.DvIdentifier();
                    id.ReadXml(reader);
                    this.originatingSystemItemIds.Add(id);
                } while (reader.LocalName == "originating_system_item_ids" && reader.NodeType == System.Xml.XmlNodeType.Element);

            }

            if (reader.LocalName == "feeder_system_item_ids")
            {
                this.feederSystemItemIds = new OpenEhr.AssumedTypes.List<OpenEhr.RM.DataTypes.Basic.DvIdentifier>();
                do
                {
                    OpenEhr.RM.DataTypes.Basic.DvIdentifier id = new OpenEhr.RM.DataTypes.Basic.DvIdentifier();
                    id.ReadXml(reader);
                    this.feederSystemItemIds.Add(id);
                } while (reader.LocalName == "feeder_system_item_ids" && reader.NodeType == System.Xml.XmlNodeType.Element);

            }

            if (reader.LocalName == "original_content")
            {
                string originalContentType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                Check.Assert(!string.IsNullOrEmpty(originalContentType),
                    "the type of original_content must not be null or empty.");

                int i = originalContentType.IndexOf(":");
                if (i >= 0)
                    originalContentType = originalContentType.Substring(i + 1);

                if (originalContentType == "DV_MULTIMEDIA")
                    this.originalContent = new DvMultimedia();
                else if (originalContentType == "DV_PARSABLE")
                    this.originalContent = new DvParsable();
                else
                    throw new InvalidOperationException("originalContent type must be either DV_MULTIMEDIA or " +
                        "DV_PARSABLE (type: " + originalContentType + ")");

                this.originalContent.ReadXml(reader);
            }

            Check.Assert(reader.LocalName == "originating_system_audit",
                "Expected LocalName is 'originating_system_audit', but it is " + reader.LocalName);
            this.originatingSystemAudit = new FeederAuditDetails();
            this.originatingSystemAudit.ReadXml(reader);

            if (reader.LocalName == "feeder_system_audit")
            {
                this.feederSystemAudit = new FeederAuditDetails();
                this.feederSystemAudit.ReadXml(reader);
            }

            if (!reader.IsStartElement())
            {
                reader.ReadEndElement();
            }

            reader.MoveToContent();
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            if (this.OriginatingSystemItemIds != null && OriginatingSystemItemIds.Count > 0)
            {
                foreach (DvIdentifier id in this.OriginatingSystemItemIds)
                {
                    writer.WriteStartElement(openEhrPrefix, "originating_system_item_ids", RmXmlSerializer.OpenEhrNamespace);
                    id.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }

            if (this.FeederSystemItemIds != null && OriginatingSystemItemIds.Count > 0)
            {
                foreach (DvIdentifier id in this.FeederSystemItemIds)
                {
                    writer.WriteStartElement(openEhrPrefix, "feeder_system_item_ids", RmXmlSerializer.OpenEhrNamespace);
                    id.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }

            if (this.OriginalContent != null)
            {
                writer.WriteStartElement(openEhrPrefix, "original_content", RmXmlSerializer.OpenEhrNamespace);
                string type = ((IRmType)(this.originalContent)).GetRmTypeName();
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    type = openEhrPrefix + ":" + type;

                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, type);
               
                this.OriginalContent.WriteXml(writer);
                writer.WriteEndElement();
            }

            Check.Assert(this.OriginatingSystemAudit!=null, "originatingSystemAudit must not be null.");
            writer.WriteStartElement(openEhrPrefix, "originating_system_audit", RmXmlSerializer.OpenEhrNamespace);
            this.OriginatingSystemAudit.WriteXml(writer);
            writer.WriteEndElement();

            if (this.FeederSystemAudit != null)
            {
                writer.WriteStartElement(openEhrPrefix, "feeder_system_audit", RmXmlSerializer.OpenEhrNamespace);
                this.FeederSystemAudit.WriteXml(writer);
                writer.WriteEndElement();
            }
        }
    }
}
