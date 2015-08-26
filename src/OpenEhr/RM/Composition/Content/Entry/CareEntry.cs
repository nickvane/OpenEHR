using System;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.Common.Generic;
using OpenEhr.RM.Support.Identification;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Composition.Content.Entry
{
    [Serializable]
    [RmType("openEHR", "EHR", "CARE_ENTRY")]
    public abstract class CareEntry : Entry
    {
        protected CareEntry()
        { }

        protected CareEntry(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
          Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit,
            CodePhrase language, CodePhrase encoding, PartyProxy subject, PartyProxy proider,
           Participation[] otherParticipations, ObjectRef workflowId, ItemStructure protocol,
            ObjectRef guidelineId)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit,
            language, encoding, subject, proider, otherParticipations, workflowId)
        {
            this.protocol = protocol;
            if(this.protocol!= null)
                this.protocol.Parent = this;

            this.guidelineId = guidelineId;
        }

        private DataStructures.ItemStructure.ItemStructure protocol;
        [RmAttribute("protocol")]
        public DataStructures.ItemStructure.ItemStructure Protocol
        {
            get
            {
                if(this.protocol == null)
                    this.protocol = base.attributesDictionary["protocol"] as DataStructures.ItemStructure.ItemStructure;
                return this.protocol;
            }
            set
            {
                if (this.protocol != null)
                    this.protocol.Parent = null;
                this.protocol = value;
                if (this.protocol != null)
                    this.protocol.Parent = this;
                base.attributesDictionary["protocol"] = this.protocol;
            }
        }

        private Support.Identification.ObjectRef guidelineId;

        [RmAttribute("guideline_id")]
        public Support.Identification.ObjectRef GuidelineId
        {
            get
            {
                if (this.guidelineId == null)
                    this.guidelineId = base.attributesDictionary["guideline_id"] as Support.Identification.ObjectRef;
                return this.guidelineId;
            }
            set
            {
                this.guidelineId = value;
                base.attributesDictionary["guideline_id"] = this.guidelineId;
            }
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            if (reader.LocalName == "protocol")
            {
                string protocolType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                this.protocol = OpenEhr.RM.Common.Archetyped.Impl.Locatable.GetLocatableObjectByType(protocolType)
                    as ItemStructure;
                if (this.protocol == null)
                    throw new InvalidOperationException("otherContextType must be subtype of ItemStructure " + protocolType);
                this.protocol.ReadXml(reader);
                this.protocol.Parent = this;
            }

            if (reader.LocalName == "guideline_id")
            {
                string guidelineIdType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                this.guidelineId = OpenEhr.RM.Support.Identification.ObjectRef.GetObjectRefByType(guidelineIdType);

                this.guidelineId.ReadXml(reader);
            }

        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            
            if (this.Protocol != null)
            {
                writer.WriteStartElement(openEhrPrefix, "protocol", RmXmlSerializer.OpenEhrNamespace);
                string protocolType = ((IRmType)this.Protocol).GetRmTypeName();
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    protocolType = openEhrPrefix + ":" + protocolType;
                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, protocolType);
                this.Protocol.WriteXml(writer);
                writer.WriteEndElement();
            }            

        }

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();
            base.attributesDictionary["protocol"] = this.protocol;
            base.attributesDictionary["guideline_id"] = this.guidelineId;
        }
    }
}
