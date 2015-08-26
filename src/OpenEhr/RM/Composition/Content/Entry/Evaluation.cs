using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.Common.Generic;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Composition.Content.Entry
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "EHR", "EVALUATION")]
    public class Evaluation : CareEntry, System.Xml.Serialization.IXmlSerializable
    {
        public Evaluation() 
        { }

        public Evaluation(DvText name, string archetypeNodeId, UidBasedId uid,
         Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit,
         CodePhrase language, CodePhrase encoding, PartyProxy subject, PartyProxy proider,
         Participation[] otherParticipations, ObjectRef workflowId, ItemStructure protocol,
         ObjectRef guidelineId, ItemStructure data)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit, language,
          encoding, subject, proider, otherParticipations, workflowId, protocol, guidelineId)
        {
            Check.Require(data != null, "data must not be null");

            this.data = data;
            if (this.data != null)
                this.data.Parent = this;

            SetAttributeDictionary();
            CheckInvariants();
        }
        
        private DataStructures.ItemStructure.ItemStructure data;
        [RmAttribute("data", 1)]
        public DataStructures.ItemStructure.ItemStructure Data
        {
            get
            {
                if(this.data == null)
                    this.data = base.attributesDictionary["data"] as DataStructures.ItemStructure.ItemStructure;
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
            return new System.Xml.XmlQualifiedName("EVALUATION", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            DesignByContract.Check.Assert(reader.LocalName == "data",
                "Expected LocalName is 'data', but it is " + reader.LocalName);
            string dataType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
            this.data = OpenEhr.RM.Common.Archetyped.Impl.Locatable.GetLocatableObjectByType(dataType)
                as ItemStructure;
            if (this.data == null)
                throw new InvalidOperationException("data type must be type of ItemStructure: " + dataType);
            this.data.ReadXml(reader);
            this.data.Parent = this;
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "data", RmXmlSerializer.OpenEhrNamespace);
            string dataType = ((IRmType)this.Data).GetRmTypeName();
            if (!string.IsNullOrEmpty(openEhrPrefix))
                dataType = openEhrPrefix + ":" + dataType;
            writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, dataType);
            this.Data.WriteXml(writer);
            writer.WriteEndElement();

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

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();
            base.attributesDictionary["data"] = this.data;
        }
    }
}
