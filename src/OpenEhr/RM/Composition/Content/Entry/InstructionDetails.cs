using System;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.DataStructures.ItemStructure;

namespace OpenEhr.RM.Composition.Content.Entry
{
    /// <summary>
    /// Model of a transition in the Instruction State machine, caused by a 
    /// careflow step. The attributes document the careflow step as well as the ISM transition.
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "EHR", "INSTRUCTION_DETAILS")]
    public class InstructionDetails : AttributeDictionaryPathable, System.Xml.Serialization.IXmlSerializable
    {
        public InstructionDetails(LocatableRef instructionId,
            string activityId, ItemStructure wfDetails)
            : this()
        {
            Check.Require(instructionId != null, "instruction_id must not be null");
            Check.Require(!string.IsNullOrEmpty(activityId), "activity_id must not be null or empty");

            this.instructionId = instructionId;
            this.activityId = activityId;
            this.wfDetails = wfDetails;
            if (this.wfDetails != null)
                this.wfDetails.Parent = this;

            SetAttributeDictionary();
        }

        public InstructionDetails()
        { }
        
        private string activityId;

        [RmAttribute("activity_id", 1)]
        public string ActivityId
        {
            get
            {
                if(this.activityId == null)
                    this.activityId = this.attributesDictionary["activity_id"] as string;
                return this.activityId;
            }
            set
            {
                Check.Require(value != null, "value must not be null.");
                this.activityId = value;
                base.attributesDictionary["activity_id"] = this.activityId;
            }
        }

        private DataStructures.ItemStructure.ItemStructure wfDetails;

        [RmAttribute("wf_details")]
        public DataStructures.ItemStructure.ItemStructure WfDetails
        {
            get
            {
                if(this.wfDetails == null)
                    this.wfDetails = this.attributesDictionary["wf_details"] as DataStructures.ItemStructure.ItemStructure;
                return this.wfDetails;
            }
            set
            {
                if (this.wfDetails != null)
                    this.wfDetails.Parent = null;
                this.wfDetails = value;
                if (this.wfDetails != null)
                    this.wfDetails.Parent = this;
                base.attributesDictionary["wf_details"] = this.wfDetails;
            }
        }  
    
        private OpenEhr.RM.Support.Identification.LocatableRef instructionId;

        [RmAttribute("instruction_id, 1")]
        public OpenEhr.RM.Support.Identification.LocatableRef InstructionId
        {
            get
            {
                if(this.instructionId == null)
                    this.instructionId = this.attributesDictionary["instruction_id"] as OpenEhr.RM.Support.Identification.LocatableRef;
                return this.instructionId;
            }
            set
            {
                Check.Require(value != null, "value must not be null.");
                this.instructionId = value;
                base.attributesDictionary["instruction_id"] = this.instructionId;
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
            return new System.Xml.XmlQualifiedName("INSTRUCTION_DETAILS", RmXmlSerializer.OpenEhrNamespace);
        }

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            DesignByContract.Check.Assert(reader.LocalName == "instruction_id",
                "Expected LocalName is 'instruction_id', but it is " + reader.LocalName);
            this.instructionId = new OpenEhr.RM.Support.Identification.LocatableRef();
            this.instructionId.ReadXml(reader);

            DesignByContract.Check.Assert(reader.LocalName == "activity_id",
                "Expected LocalName is 'activity_id', but it is " + reader.LocalName);
            this.activityId = reader.ReadElementString("activity_id", RmXmlSerializer.OpenEhrNamespace);

            if (reader.LocalName == "wf_details")
            {
                string wfDetailsType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                this.wfDetails = OpenEhr.RM.Common.Archetyped.Impl.Locatable.GetLocatableObjectByType(wfDetailsType)
                    as OpenEhr.RM.DataStructures.ItemStructure.ItemStructure;
                if (this.wfDetails == null)
                    throw new InvalidOperationException("wfDetailsType must by type of ItemStructure: " + wfDetails);
                this.wfDetails.ReadXml(reader);
                this.wfDetails.Parent = this;
            }

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement, "Expected endElement of InstructionDetails.");
            reader.ReadEndElement();
            reader.MoveToContent();

            // CM: 26/11/09 need to set attributDictionary since InstructionDetails is not type of locatable
            this.SetAttributeDictionary();
            this.CheckInvariants();
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            this.CheckInvariants();

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "instruction_id", RmXmlSerializer.OpenEhrNamespace);
            this.InstructionId.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteElementString(openEhrPrefix, "activity_id", RmXmlSerializer.OpenEhrNamespace, this.ActivityId);

            if (this.WfDetails != null)
            {
                writer.WriteStartElement(openEhrPrefix, "wf_details", RmXmlSerializer.OpenEhrNamespace);
                string wfDetailstype = ((IRmType)this.WfDetails).GetRmTypeName();
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    wfDetailstype = openEhrPrefix + ":" + wfDetailstype;
                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, wfDetailstype);
                this.WfDetails.WriteXml(writer);
                writer.WriteEndElement();
            }

        }

        private void CheckInvariants()
        {
            // %HYYKA%
            //DesignByContract.Check.Invariant(this.InstructionId != null, "InstructionId must not be null.");
            this.CheckInvariantsDefault();
            DesignByContract.Check.Invariant(this.ActivityId != null && this.ActivityId.Length>0, 
                "ActivityId must not be null or empty.");
        }

        private void CheckInvariantsDefault()
        {
            DesignByContract.Check.Invariant(this.InstructionId != null, "InstructionId must not be null.");
            
        }

        protected override void SetAttributeDictionary()
        {
            Check.Require(this.attributesDictionary != null, "attributeDictionary must not be null");

            this.attributesDictionary["instruction_id"] = this.InstructionId;
            this.attributesDictionary["activity_id"] = this.ActivityId;
            this.attributesDictionary["wf_details"] = this.WfDetails;
        }
    }
}
