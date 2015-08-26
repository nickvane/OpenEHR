using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.RM.DataStructures.History;
using OpenEhr.RM.Common.Generic;
using OpenEhr.RM.Support.Identification;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Composition.Content.Entry
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "EHR", "OBSERVATION")]
    public class Observation : CareEntry, System.Xml.Serialization.IXmlSerializable
    {
        public Observation() 
        { }

        public Observation(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
           Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit,
           CodePhrase language, CodePhrase encoding, PartyProxy subject, PartyProxy proider,
           Participation[] otherParticipations, ObjectRef workflowId, ItemStructure protocol,
           ObjectRef guidelineId, History<ItemStructure> data, History<ItemStructure> state)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit, language,
            encoding, subject, proider, otherParticipations, workflowId, protocol, guidelineId)
        {
            Check.Require(data != null, "data must not be null");

            this.data = data;
            if (this.data != null)
                this.data.Parent = this;
            this.state = state;
            if (this.state != null)
                this.state.Parent = this;

            SetAttributeDictionary();
            CheckInvariants();
        }

        private DataStructures.History.History<DataStructures.ItemStructure.ItemStructure> data;

        [RmAttribute("data", 1)]
        public DataStructures.History.History<DataStructures.ItemStructure.ItemStructure> Data
        {
            get
            {
                if(this.data == null)
                    this.data = base.attributesDictionary["data"] as DataStructures.History.History<DataStructures.ItemStructure.ItemStructure> ;
                return this.data;
            }
            set
            {
                Check.Require(value != null, "value must not be null.");
                if (this.data != null)
                    this.data.Parent = null;
                this.data = value;
                this.data.Parent = this;
                base.attributesDictionary["data"] = this.data;
            }
        }

        private DataStructures.History.History<DataStructures.ItemStructure.ItemStructure> state;
        [RmAttribute("state")]
        public DataStructures.History.History<DataStructures.ItemStructure.ItemStructure> State
        {
            get
            {
                if(this.state == null)
                    this.state = base.attributesDictionary["state"] as DataStructures.History.History<DataStructures.ItemStructure.ItemStructure>;
                return this.state;
            }
            set
            {
                if (this.state != null)
                    this.state.Parent = null;
                this.state = value;
                if (this.state != null)
                    this.state.Parent = this;
                base.attributesDictionary["state"] = this.state;
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
            return new System.Xml.XmlQualifiedName("OBSERVATION", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            DesignByContract.Check.Assert(reader.LocalName == "data",
                "Expected LocalName is 'data', but it is " + reader.LocalName);
            this.data = new OpenEhr.RM.DataStructures.History.History<OpenEhr.RM.DataStructures.ItemStructure.ItemStructure>();
            this.data.ReadXml(reader);
            this.data.Parent = this;

            if (reader.LocalName == "state")
            {
                this.state = new OpenEhr.RM.DataStructures.History.History<OpenEhr.RM.DataStructures.ItemStructure.ItemStructure>();
                this.state.ReadXml(reader);
                this.state.Parent = this;
            }

        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "data", RmXmlSerializer.OpenEhrNamespace);
            this.Data.WriteXml(writer);
            writer.WriteEndElement();

            if (this.State != null)
            {
                writer.WriteStartElement(openEhrPrefix, "state", RmXmlSerializer.OpenEhrNamespace);
                this.State.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();
            base.attributesDictionary["data"]= this.data;
            base.attributesDictionary["state"]= this.state;
        }

        protected override void CheckInvariants()
        {
            base.CheckInvariants();

            // %HYYKA%
            //DesignByContract.Check.Invariant(this.Data != null, "data must not be null.");
        }

        protected void CheckInvariantsDefault()
        {
            base.CheckInvariantsDefault();           
        }
    }
}
