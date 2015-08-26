using System;
using System.ComponentModel;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.DataTypes.Encapsulated;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.Common.Generic;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.Factories;
using OpenEhr.Serialisation;
using OpenEhr.AssumedTypes.Impl;

namespace OpenEhr.RM.Composition.Content.Entry
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "EHR", "INSTRUCTION")]
    public class Instruction : CareEntry, System.Xml.Serialization.IXmlSerializable
    {
        public Instruction() 
        { }

        public Instruction(DvText name, string archetypeNodeId, UidBasedId uid,
         Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit,
         CodePhrase language, CodePhrase encoding, PartyProxy subject, PartyProxy proider,
         Participation[] otherParticipations, ObjectRef workflowId, ItemStructure protocol,
         ObjectRef guidelineId, DvText narrative, DvDateTime expiryTime,
         Activity[] activities, DvParsable wfDefinition)
          : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit, language,
          encoding, subject, proider, otherParticipations, workflowId, protocol, guidelineId)
        {
            Check.Require(narrative != null, "narrative must not be null");

            this.narrative = narrative;
            this.expiryTime = expiryTime;
            if (activities != null)
            {
                this.activities = RmFactory.LocatableList<Activity>(this, activities);
            }
            this.wfDefinition = wfDefinition;

            SetAttributeDictionary();
            CheckInvariants();
        }

        private DataTypes.Text.DvText narrative;

        [RmAttribute("narrative", 1)]
        public DataTypes.Text.DvText Narrative
        {
            get
            {
                if(this.narrative == null)
                    this.narrative = base.attributesDictionary["narrative"] as DataTypes.Text.DvText;
                return this.narrative;
            }
            set
            {
                Check.Require(value != null, "value must not be null.");
                this.narrative = value;
                base.attributesDictionary["narrative"] = this.narrative;
            }
        }

        private DataTypes.Quantity.DateTime.DvDateTime expiryTime;

        [RmAttribute("expiry_time")]
        public DataTypes.Quantity.DateTime.DvDateTime ExpiryTime
        {
            get
            {
                if(this.expiryTime == null)
                    this.expiryTime = base.attributesDictionary["expiry_time"] as DataTypes.Quantity.DateTime.DvDateTime;
                return this.expiryTime;
            }
            set
            {
                this.expiryTime = value;
                base.attributesDictionary["expiry_time"] = this.expiryTime;
            }
        }

        private DataTypes.Encapsulated.DvParsable wfDefinition;

        [RmAttribute("wf_definition")]
        public DataTypes.Encapsulated.DvParsable WfDefinition
        {
            get
            {
                if (this.wfDefinition == null)
                    this.wfDefinition = base.attributesDictionary["wf_definition"] as DataTypes.Encapsulated.DvParsable;
                return this.wfDefinition;
            }
            set
            {
                this.wfDefinition = value;
                base.attributesDictionary["wf_definition"] = this.wfDefinition;
            }
        }

        private LocatableList<Activity> activities;
        
        [RmAttribute("activities")]
        public AssumedTypes.List<Activity> Activities
        {
            get
            {
                if(this.activities == null)
                    this.activities = base.attributesDictionary["activities"] as LocatableList<Activity>;
                return this.activities;
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
            RmXmlSerializer.LoadCompositionSchema(xs);
            return new System.Xml.XmlQualifiedName("INSTRUCTION", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            DesignByContract.Check.Assert(reader.LocalName == "narrative",
                "Expected LocalName is 'narrative', but it is " + reader.LocalName);
            string narrativeType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
            if (narrativeType != null)
                this.narrative = new OpenEhr.RM.DataTypes.Text.DvCodedText();
            else
                this.narrative = new OpenEhr.RM.DataTypes.Text.DvText();
            this.narrative.ReadXml(reader);

            if (reader.LocalName == "expiry_time")
            {
                this.expiryTime = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime();
                this.expiryTime.ReadXml(reader);               
            }

            if (reader.LocalName == "wf_definition")
            {
                this.wfDefinition = new OpenEhr.RM.DataTypes.Encapsulated.DvParsable();
                this.wfDefinition.ReadXml(reader);      
            }

            if (reader.LocalName == "activities")
            {
                AssumedTypes.Impl.LocatableList<Activity> activities = new OpenEhr.AssumedTypes.Impl.LocatableList<Activity>();
                do
                {
                    Activity activity = new Activity();
                    activity.ReadXml(reader);

                    activity.Parent = this;
                    activities.Add(activity);


                } while (reader.LocalName == "activities" && reader.NodeType == System.Xml.XmlNodeType.Element);

                this.activities = activities;
            }

        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "narrative", RmXmlSerializer.OpenEhrNamespace);
            if (this.Narrative.GetType() == typeof(OpenEhr.RM.DataTypes.Text.DvCodedText))
            {
                string narrativeType = "DV_CODED_TEXT";
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    narrativeType = openEhrPrefix + ":" + narrativeType;
                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, narrativeType);
            }
            this.Narrative.WriteXml(writer);
            writer.WriteEndElement();

            if (this.ExpiryTime != null)
            {
                writer.WriteStartElement(openEhrPrefix, "expiry_time", RmXmlSerializer.OpenEhrNamespace);
                this.ExpiryTime.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.WfDefinition != null)
            {
                writer.WriteStartElement(openEhrPrefix, "wf_definition", RmXmlSerializer.OpenEhrNamespace);
                this.WfDefinition.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.Activities != null && this.Activities.Count > 0)
            {
                foreach (Activity activity in this.Activities)
                {
                    writer.WriteStartElement(openEhrPrefix, "activities", RmXmlSerializer.OpenEhrNamespace);                    
                    activity.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }

        protected override void CheckInvariants()
        {
            base.CheckInvariants();

            // %HYYKA%
            //DesignByContract.Check.Invariant(this.Narrative != null, "data must not be null.");
            //DesignByContract.Check.Invariant(this.Activities == null || this.Activities.Count>0,
            //    "Activities_valid: activities /= Void implies not activities.is_empty");
        }

        protected void CheckInvariantsDefault()
        {
            base.CheckInvariantsDefault();

            // %HYYKA%
            //DesignByContract.Check.Invariant(this.Narrative != null, "data must not be null.");            
        }

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();
            base.attributesDictionary["narrative"]= this.narrative;
            base.attributesDictionary["expiry_time"]= this.expiryTime;
            base.attributesDictionary["activities"]= this.activities;
            base.attributesDictionary["wf_definition"] = this.wfDefinition;

        }
    }
}
