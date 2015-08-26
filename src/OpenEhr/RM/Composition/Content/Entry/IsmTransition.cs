using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.Common.Archetyped.Impl;

namespace OpenEhr.RM.Composition.Content.Entry
{
    /// <summary>
    /// Model of a transition in the Instruction State machine, caused by a 
    /// careflow step. The attributes document the careflow step as well as the ISM transition.
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "EHR", "ISM_TRANSITION")]
    public class IsmTransition : AttributeDictionaryPathable, System.Xml.Serialization.IXmlSerializable
    {

        public IsmTransition() 
        { }

        public IsmTransition(DataTypes.Text.DvCodedText currentState, DataTypes.Text.DvCodedText transition,
            DataTypes.Text.DvCodedText careflowStep)
            : this()
        {
            Check.Require(currentState != null, "current_state must not be null");

            this.currentState = currentState;
            this.transition = transition;
            this.careflowStep = careflowStep;

            SetAttributeDictionary();
            this.SetAttributeDictionary();
        }
        
        private DataTypes.Text.DvCodedText currentState;

        [RmAttribute("current_state", 1)]
        [RmTerminology("instruction states")]
        public DataTypes.Text.DvCodedText CurrentState
        {
            get
            {
                if(this.currentState == null)
                    this.currentState = this.attributesDictionary["current_state"] as DataTypes.Text.DvCodedText;
                return this.currentState;
            }
            set
            {
                Check.Require(value != null, "value must not be null.");
                this.currentState = value;
                base.attributesDictionary["current_state"] = this.currentState;
            }
        }

        private DataTypes.Text.DvCodedText transition;

        [RmAttribute("transition")]
        [RmTerminology("instruction transitions")]
        public DataTypes.Text.DvCodedText Transition
        {
            get
            {
                if(this.transition == null)
                    this.transition = this.attributesDictionary["transition"] as DataTypes.Text.DvCodedText;
                return this.transition;
            }
            set
            {
                this.transition = value;
                base.attributesDictionary["transition"] = this.transition;
            }
        }

        private DataTypes.Text.DvCodedText careflowStep;
        [RmAttribute("careflow_step")]
        public DataTypes.Text.DvCodedText CareflowStep
        {
            get
            {
                if(this.careflowStep == null)
                    this.careflowStep = this.attributesDictionary["careflow_step"] as DataTypes.Text.DvCodedText;
                return this.careflowStep;
            }
            set
            {
                this.careflowStep = value;
                base.attributesDictionary["careflow_step"] = this.careflowStep;
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
            return new System.Xml.XmlQualifiedName("ISM_TRANSITION", RmXmlSerializer.OpenEhrNamespace);
        }

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            DesignByContract.Check.Assert(reader.LocalName == "current_state",
                "Expected LocalName is 'current_state', but it is " + reader.LocalName);
            this.currentState = new OpenEhr.RM.DataTypes.Text.DvCodedText();
            this.currentState.ReadXml(reader);

            if (reader.LocalName == "transition") 
            {
                this.transition = new OpenEhr.RM.DataTypes.Text.DvCodedText();
                this.transition.ReadXml(reader);               
            }

            if (reader.LocalName == "careflow_step")
            {
                this.careflowStep = new OpenEhr.RM.DataTypes.Text.DvCodedText();
                this.careflowStep.ReadXml(reader);                
            }

            Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement, "Expected endElement of IsmTransition.");
            reader.ReadEndElement();
            reader.MoveToContent();

            this.SetAttributeDictionary();
            this.CheckInvariants();
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            this.CheckInvariants();
            
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "current_state", RmXmlSerializer.OpenEhrNamespace);
            this.CurrentState.WriteXml(writer);
            writer.WriteEndElement();

            if (this.Transition != null)
            {
                writer.WriteStartElement(openEhrPrefix, "transition", RmXmlSerializer.OpenEhrNamespace);
                this.Transition.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.CareflowStep != null)
            {
                writer.WriteStartElement(openEhrPrefix, "careflow_step", RmXmlSerializer.OpenEhrNamespace);
                this.CareflowStep.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        private void CheckInvariants()
        {
            Check.Invariant(this.CurrentState!= null, "CurrentState must not be null.");
        }

        protected override void SetAttributeDictionary()
        {
            Check.Require(this.attributesDictionary != null, "attributeDictionary must not be null");

            this.attributesDictionary["current_state"] = this.currentState;
            this.attributesDictionary["transition"] = this.transition;
            this.attributesDictionary["careflow_step"] = this.careflowStep;
        } 
    }
}
