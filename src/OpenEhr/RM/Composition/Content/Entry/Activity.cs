using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.DataTypes.Encapsulated;
using OpenEhr.RM.Support.Identification;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Composition.Content.Entry
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "EHR", "ACTIVITY")]
    public class Activity : Locatable, System.Xml.Serialization.IXmlSerializable
    {
        public Activity() 
        { }

        public Activity(DvText name, string archetypeNodeId, UidBasedId uid,
          Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit,
            ItemStructure description, DvParsable timing, string actionArchetypeId)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit)
        {
            Check.Require(description != null, "description must not be null");
            Check.Require(timing != null, "timing must not be null");
            Check.Require(!string.IsNullOrEmpty(actionArchetypeId), "action_archetype_id must not be null or empty");

            this.description = description;
            this.description.Parent = this;
            this.timing = timing;
            this.actionArchetypeId = actionArchetypeId;

            SetAttributeDictionary();
            CheckInvariants();
        }

        private DataStructures.ItemStructure.ItemStructure description;

        [RmAttribute("description", 1)]
        public DataStructures.ItemStructure.ItemStructure Description
        {
            get
            {
                if(this.description == null)
                    this.description = base.attributesDictionary["description"] as DataStructures.ItemStructure.ItemStructure;
                return this.description;
            }
            set
            {
                Check.Require(value != null, "value must not be null.");
                this.description = value;
                base.attributesDictionary["description"] = this.description;
            }
        }

        private DataTypes.Encapsulated.DvParsable timing;

        [RmAttribute("timing", 1)]
        public DataTypes.Encapsulated.DvParsable Timing
        {
            get
            {
                if(this.timing == null)
                    this.timing = base.attributesDictionary["timing"] as OpenEhr.RM.DataTypes.Encapsulated.DvParsable;
                return this.timing;
            }
            set
            {
                Check.Require(value != null, "value must not be null.");
                this.timing = value;
                base.attributesDictionary["timing"] = this.timing;
            }
        }

        private string actionArchetypeId;

        [RmAttribute("action_archetype_id", 1)]
        public string ActionArchetypeId
        {
            get
            {
                if(this.actionArchetypeId == null)
                    this.actionArchetypeId = base.attributesDictionary["action_archetype_id"] as string;
                return this.actionArchetypeId;
            }
            set
            {
                Check.Require(!string.IsNullOrEmpty(value), "value must not be null or empty.");
                this.actionArchetypeId = value;
                base.attributesDictionary["action_archetype_id"] = this.actionArchetypeId;
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
            return new System.Xml.XmlQualifiedName("ACTIVITY", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            DesignByContract.Check.Assert(reader.LocalName == "description",
              "Expected LocalName is 'description', but it is " + reader.LocalName);
            string descriptionType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
            this.description = Locatable.GetLocatableObjectByType(descriptionType) as ItemStructure;
            if (this.description == null)
                throw new InvalidOperationException("descriptionType must be type of ItemStructure: " + descriptionType);
            this.description.ReadXml(reader);
            this.description.Parent = this;

            DesignByContract.Check.Assert(reader.LocalName == "timing",
              "Expected LocalName is 'timing', but it is " + reader.LocalName);
            this.timing = new OpenEhr.RM.DataTypes.Encapsulated.DvParsable();
            this.timing.ReadXml(reader);

            DesignByContract.Check.Assert(reader.LocalName == "action_archetype_id",
               "Expected LocalName is 'action_archetype_id', but it is " + reader.LocalName);
            this.actionArchetypeId = reader.ReadElementString("action_archetype_id", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();

        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "description", RmXmlSerializer.OpenEhrNamespace);
            string descriptionType = ((IRmType)this.Description).GetRmTypeName();
            if (!string.IsNullOrEmpty(openEhrPrefix))
                descriptionType = openEhrPrefix + ":" + descriptionType;
            writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, descriptionType);
            this.Description.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement(openEhrPrefix, "timing", RmXmlSerializer.OpenEhrNamespace);           
            this.Timing.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteElementString(openEhrPrefix, "action_archetype_id", RmXmlSerializer.OpenEhrNamespace,
                this.ActionArchetypeId);

        }

        protected override void CheckInvariants()
        {
            base.CheckInvariants();

            DesignByContract.Check.Invariant(this.Description != null, "Description must not be null.");
            DesignByContract.Check.Invariant(this.Timing != null, "Timing must not be null.");
            DesignByContract.Check.Invariant(this.ActionArchetypeId != null && this.ActionArchetypeId.Length>0, 
                "ActionArchetypeId must not be null or empty.");
        }

        protected void CheckInvariantsDefault()
        {
            base.CheckInvariantsDefault();

            // %HYYKA%
            //DesignByContract.Check.Invariant(this.Timing != null, "Timing must not be null.");
        }

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();

            base.attributesDictionary["description"] = this.description;
            base.attributesDictionary["timing"]= this.timing;
            base.attributesDictionary["action_archetype_id"] = this.actionArchetypeId;
        }
    }

}
