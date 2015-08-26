using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Common.Generic;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.Support.Identification;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.Factories;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Composition.Content.Entry
{
    [Serializable]
    [RmType("openEHR", "EHR", "ENTRY")]
    public abstract class Entry : ContentItem
    {
        protected Entry()
        { }

        protected Entry(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
           Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit,
            CodePhrase language, CodePhrase encoding, PartyProxy subject, PartyProxy proider,
            Participation[] otherParticipations, ObjectRef workflowId)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit)
        {
            Check.Require(language != null, "language must not be null");
            Check.Require(encoding != null, "encoding must not be null");
            Check.Require(subject != null, "subject must not be null");

            this.language = language;
            this.encoding = encoding;
            this.subject = subject;
            this.provider = proider;
            if (otherParticipations != null)
                this.otherParticipations = new OpenEhr.AssumedTypes.List<Participation>(otherParticipations);
            this.workflowId = workflowId;            
        }

        private DataTypes.Text.CodePhrase language;

        [RmAttribute("language", 1)]
        [RmCodeset("languages", "ISO_639-1")]
        public DataTypes.Text.CodePhrase Language
        {
            get
            {
                if (this.language == null)
                    this.language = base.attributesDictionary["language"] as DataTypes.Text.CodePhrase;
                return this.language;
            }
            set
            {
                Check.Require(value != null, "language must not be null.");
                this.language = value;
                base.attributesDictionary["language"] = this.language;
            }
        }

        private DataTypes.Text.CodePhrase encoding;

        [RmAttribute("encoding", 1)]
        [RmCodeset("character sets", "IANA_character-sets")]
        public DataTypes.Text.CodePhrase Encoding
        {
            get
            {
                if(this.encoding == null)
                    this.encoding = base.attributesDictionary["encoding"] as DataTypes.Text.CodePhrase;
                return this.encoding;
            }
            set
            {
                Check.Require(value != null, "encoding must not be null.");
                this.encoding = value;
                base.attributesDictionary["encoding"] = this.encoding;
            }
        }

        private Common.Generic.PartyProxy subject;

        [RmAttribute("subject", 1)]
        public Common.Generic.PartyProxy Subject
        {
            get
            {
                if(this.subject == null)
                    this.subject = base.attributesDictionary["subject"] as Common.Generic.PartyProxy;
                return this.subject;
            }
            set
            {
                Check.Require(value != null, "subject must not be null.");
                this.subject = value;
                base.attributesDictionary["subject"] = this.subject;
            }
        }

        private Common.Generic.PartyProxy provider;

        [RmAttribute("provider")]
        public Common.Generic.PartyProxy Provider
        {
            get
            {
                if(this.provider == null)
                    this.provider = base.attributesDictionary["provider"] as Common.Generic.PartyProxy;
                return this.provider;
            }
            set
            {
                this.provider = value;
                base.attributesDictionary["provider"] = this.provider;
            }
        }

        private AssumedTypes.List<Common.Generic.Participation> otherParticipations;

        public AssumedTypes.List<Common.Generic.Participation> OtherParticipations
        {
            get
            {
                if(this.otherParticipations == null)
                    this.otherParticipations = base.attributesDictionary["other_participations"] as AssumedTypes.List<Common.Generic.Participation>;
                return this.otherParticipations;
            }
        }

        private Support.Identification.ObjectRef workflowId;

        public Support.Identification.ObjectRef WorkflowId
        {
            get
            {
                if(this.workflowId ==null)
                    this.workflowId = base.attributesDictionary["workflow_id"] as Support.Identification.ObjectRef;
                return this.workflowId;
            }
            set
            {
                this.workflowId = value;
                base.attributesDictionary["workflow_id"] = this.workflowId;
            }
        }

        public bool SubjectIsSelf()
        {
            throw new System.NotImplementedException();
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            Check.Assert(reader.LocalName == "language", "Expected LocalName is 'language' rather than " +
                reader.LocalName);
            this.language = new OpenEhr.RM.DataTypes.Text.CodePhrase();
            this.language.ReadXml(reader);

            Check.Assert(reader.LocalName == "encoding", "Expected LocalName is 'encoding' rather than " +
                reader.LocalName);
            this.encoding = new OpenEhr.RM.DataTypes.Text.CodePhrase();
            this.encoding.ReadXml(reader);

            Check.Assert(reader.LocalName == "subject", "Expected LocalName is 'subject' rather than " +
                reader.LocalName);
            string subjectType = RmXmlSerializer.ReadXsiType(reader);
            this.subject = RmFactory.PartyProxy(subjectType); 

            this.subject.ReadXml(reader);
            

            if (reader.LocalName == "provider")
            {
                string providerType = RmXmlSerializer.ReadXsiType(reader);
                this.provider = RmFactory.PartyProxy(providerType); 

                this.provider.ReadXml(reader);               
            }

            if (reader.LocalName == "other_participations")
            {
                this.otherParticipations = new OpenEhr.AssumedTypes.List<Participation>();
                do
                {
                    Participation p = new Participation();
                    p.ReadXml(reader);

                    this.otherParticipations.Add(p);

                } while (reader.LocalName == "other_participations");
            }

            if (reader.LocalName == "work_flow_id")
            {
                string workFlowIdType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                
                // CM: 06/09/10 when workFlowIdType is null or empty, it's type of OBJECT_REF
                if (string.IsNullOrEmpty(workFlowIdType))
                    this.workflowId = new ObjectRef();
                else
                    this.workflowId = ObjectRef.GetObjectRefByType(workFlowIdType);

                this.workflowId.ReadXml(reader);
                
            }
           
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {           

            base.WriteXmlBase(writer);

            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "language", RmXmlSerializer.OpenEhrNamespace);
            this.Language.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement(openEhrPrefix, "encoding", RmXmlSerializer.OpenEhrNamespace);
            this.Encoding.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement(openEhrPrefix, "subject", RmXmlSerializer.OpenEhrNamespace);
            string subjectType = ((IRmType)this.Subject).GetRmTypeName();
            if (!string.IsNullOrEmpty(openEhrPrefix))
                subjectType = openEhrPrefix + ":" + subjectType;
            writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, subjectType);
            this.Subject.WriteXml(writer);
            writer.WriteEndElement();

            if (this.Provider != null)
            {
                writer.WriteStartElement(openEhrPrefix, "provider", RmXmlSerializer.OpenEhrNamespace);
                string providerType = ((IRmType)this.Provider).GetRmTypeName();
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    providerType = openEhrPrefix + ":" + providerType;
                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, providerType);
                this.Provider.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.OtherParticipations != null)
            {
                foreach (Participation p in this.OtherParticipations)
                {
                    writer.WriteStartElement(openEhrPrefix, "other_participations", RmXmlSerializer.OpenEhrNamespace);
                    p.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }

            if (this.WorkflowId != null)
            {
                writer.WriteStartElement(openEhrPrefix, "work_flow_id", RmXmlSerializer.OpenEhrNamespace);
                if (this.WorkflowId.GetType() != typeof(OpenEhr.RM.Support.Identification.ObjectRef))
                {
                    string workFlowIdType = ((IRmType)this.WorkflowId).GetRmTypeName();

                    if (!string.IsNullOrEmpty(openEhrPrefix))
                        workFlowIdType = openEhrPrefix + ":" + workFlowIdType;
                    writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, workFlowIdType);
                }
                this.WorkflowId.WriteXml(writer);
                writer.WriteEndElement();
            }

        }

        protected override void CheckInvariants()
        {
            base.CheckInvariants();
            // %HYYKA%
            //Check.Invariant(this.Language != null, "Language must not be null");
            //// TODO: Language_valid: language /= Void and then code_set(Code_set_id_languages).has_code(language)
            //Check.Invariant(this.Encoding != null, "Encoding must not be null");
            //// TODO: Encoding_valid: encoding /= Void and then code_set(Code_set_id_character
            //// sets).has_code(encoding)
            //Check.Invariant(this.Subject != null, "Subject must not be null");
            //// TODO: Subject_validity: subject_is_self implies subject.generating_type =
            ////“PARTY_SELF”

            //Check.Invariant(this.OtherParticipations == null || this.OtherParticipations.Count > 0,
            //    "Other_participations_valid: other_participations /= Void implies not other_participations.is_empty");

            Check.Invariant(this.IsArchetypeRoot, "is_archetype_root");

        }

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();

            base.attributesDictionary["language"] = this.language;
            base.attributesDictionary["encoding"] = this.encoding;
            base.attributesDictionary["subject"] = this.subject;
            base.attributesDictionary["provider"] = this.provider;
            base.attributesDictionary["other_participations"] = this.otherParticipations;
            base.attributesDictionary["workflow_id"] = this.workflowId;
        }

        protected void CheckInvariantsDefault()
        {
            base.CheckInvariantsDefault();
            Check.Invariant(this.Language != null, "Language must not be null");
            // TODO: Language_valid: language /= Void and then code_set(Code_set_id_languages).has_code(language)
            Check.Invariant(this.Encoding != null, "Encoding must not be null");
            // TODO: Encoding_valid: encoding /= Void and then code_set(Code_set_id_character
            // sets).has_code(encoding)
            Check.Invariant(this.Subject != null, "Subject must not be null");
            // TODO: Subject_validity: subject_is_self implies subject.generating_type =
            //“PARTY_SELF”           
        }
    }
}
