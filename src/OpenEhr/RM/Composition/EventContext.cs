using System;
using System.Xml;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Generic;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Composition
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "EHR", "EVENT_CONTEXT")]
    public class EventContext 
        : AttributeDictionaryPathable, System.Xml.Serialization.IXmlSerializable, IVisitable
    {
        public EventContext()
        { }

        public EventContext(DvDateTime startTime, DvDateTime endTime,
            DvCodedText setting, string location, PartyIdentified healthCareFacility,
            ItemStructure otherContext, Participation[] participations):this()
        {
            Check.Require(startTime != null, "start_time must not be null");
            Check.Require(setting != null, "setting must not be null");

            this.startTime = startTime;
            this.endTime = endTime;
            this.setting = setting;
            this.location = location;
            this.healthCareFacility = healthCareFacility;
            this.otherContext = otherContext;
            if (this.otherContext != null)
                this.otherContext.Parent = this;
            if (participations != null)
                this.participations = new OpenEhr.AssumedTypes.List<Participation>(participations);

            SetAttributeDictionary();
            this.CheckInvariants();
        }

        
        private DataTypes.Quantity.DateTime.DvDateTime startTime;

        [RmAttribute("start_time", 1)]
        public DataTypes.Quantity.DateTime.DvDateTime StartTime
        {
            get
            {
                if(this.startTime == null)
                    this.startTime = this.attributesDictionary["start_time"] as DataTypes.Quantity.DateTime.DvDateTime;
                return this.startTime;
            }
            set
            {
                Check.Require(value != null, "value must not be null.");
                this.startTime = value;
                base.attributesDictionary["start_time"] = this.startTime;
            }
        }

        private DataTypes.Quantity.DateTime.DvDateTime endTime;

        [RmAttribute("end_time")]
        public DataTypes.Quantity.DateTime.DvDateTime EndTime
        {
            get
            {
                if(this.endTime == null)
                    this.endTime = this.attributesDictionary["end_time"] as DataTypes.Quantity.DateTime.DvDateTime;
                return this.endTime;
            }
            set
            {
                this.endTime = value;
                base.attributesDictionary["end_time"] = this.endTime;
            }
        }

        private string location;

        [RmAttribute("location")]
        public string Location
        {
            get
            {
                if(this.location == null)
                    this.location = this.attributesDictionary["location"] as string;
                return this.location;
            }
            set
            {
                this.location = value;
                base.attributesDictionary["location"] = this.location;
            }
        }

        private DataTypes.Text.DvCodedText setting;

        [RmAttribute("setting", 1)]
        [RmTerminology("setting")]
        public DataTypes.Text.DvCodedText Setting
        {
            get
            {
                if(this.setting == null)
                    this.setting = this.attributesDictionary["setting"] as DataTypes.Text.DvCodedText;
                return this.setting;
            }
            set
            {
                Check.Require(value != null, "value must not be null.");
                this.setting = value;
                base.attributesDictionary["setting"] = this.setting;
            }
        }

        private Common.Generic.PartyIdentified healthCareFacility;

        [RmAttribute("health_care_facility")]
        public Common.Generic.PartyIdentified HealthCareFacility
        {
            get
            {
                if(this.healthCareFacility == null)
                    this.healthCareFacility = this.attributesDictionary["health_care_facility"] as Common.Generic.PartyIdentified;
                return this.healthCareFacility;
            }
            set
            {
                this.healthCareFacility = value;
                base.attributesDictionary["health_care_facility"] = this.healthCareFacility;
            }
        }

        private AssumedTypes.List<Common.Generic.Participation> participations;

        [RmAttribute("participations")]
        public AssumedTypes.List<Common.Generic.Participation> Participations
        {
            get
            {
                if(this.participations == null)
                    this.participations = this.attributesDictionary["participations"] as AssumedTypes.List<Common.Generic.Participation>;
                return this.participations;
            }
        }

        private ItemStructure otherContext;

        [RmAttribute("other_context")]
        public ItemStructure OtherContext
        {
            get
            {
                if(this.otherContext == null)
                    this.otherContext = this.attributesDictionary["other_context"] as ItemStructure;
                return this.otherContext;
            }
            set
            {
                if (this.otherContext != null)
                    this.otherContext.Parent = null;
                this.otherContext = value;
                if (this.otherContext != null)
                    this.otherContext.Parent = this;
                base.attributesDictionary["other_context"] = this.otherContext;
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
            RmXmlSerializer.LoadCompositionSchema(xs);
            return new XmlQualifiedName("EVENT_CONTEXT", RmXmlSerializer.OpenEhrNamespace);
        }

        internal void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            DesignByContract.Check.Assert(reader.LocalName == "start_time",
                "Expected LocalName is 'start_time', but it is " + reader.LocalName);
            this.startTime = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime();
            this.startTime.ReadXml(reader);

            if (reader.LocalName == "end_time")
            {
                this.endTime = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime();
                this.endTime.ReadXml(reader);
            }

            if (reader.LocalName == "location")
            {
                this.location = reader.ReadElementString("location", RmXmlSerializer.OpenEhrNamespace);
            }

            DesignByContract.Check.Assert(reader.LocalName == "setting",
                "Expected LocalName is 'setting', but it is " + reader.LocalName);
            this.setting = new OpenEhr.RM.DataTypes.Text.DvCodedText();
            this.setting.ReadXml(reader);

            if (reader.LocalName == "other_context")
            {
                string otherContextType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                this.otherContext = OpenEhr.RM.Common.Archetyped.Impl.Locatable.GetLocatableObjectByType(otherContextType)
                    as ItemStructure;
                if (this.otherContext == null)
                    throw new InvalidOperationException("otherContextType must be subtype of ItemStructure " + otherContextType);
                this.otherContext.ReadXml(reader);
                this.otherContext.Parent = this;
            }

            if (reader.LocalName == "health_care_facility")
            {
                this.healthCareFacility = new OpenEhr.RM.Common.Generic.PartyIdentified();
                this.healthCareFacility.ReadXml(reader);

            }

            if (reader.LocalName == "participations")
            {
                this.participations = new OpenEhr.AssumedTypes.List<OpenEhr.RM.Common.Generic.Participation>();
                do
                {
                    OpenEhr.RM.Common.Generic.Participation p = new OpenEhr.RM.Common.Generic.Participation();
                    p.ReadXml(reader);
                    this.participations.Add(p);

                } while (reader.LocalName == "participations");
            }


            Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement, "Expected endElement of EventContext.");
            reader.ReadEndElement();
            reader.MoveToContent();

            this.SetAttributeDictionary();
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
             Check.Require(this.StartTime != null, "startTime must not be null.");
            Check.Require(this.Setting!= null, "setting must not be null.");

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "start_time", RmXmlSerializer.OpenEhrNamespace);
            this.StartTime.WriteXml(writer);
            writer.WriteEndElement();

            if (this.EndTime != null)
            {
                writer.WriteStartElement(openEhrPrefix, "end_time", RmXmlSerializer.OpenEhrNamespace);
                this.EndTime.WriteXml(writer);
                writer.WriteEndElement();
            }
            if (this.Location != null)
            {
                writer.WriteElementString(openEhrPrefix, "location", RmXmlSerializer.OpenEhrNamespace, this.Location);                
            }

            writer.WriteStartElement(openEhrPrefix, "setting", RmXmlSerializer.OpenEhrNamespace);
            this.Setting.WriteXml(writer);
            writer.WriteEndElement();

            if (this.OtherContext != null)
            {
                writer.WriteStartElement(openEhrPrefix, "other_context", RmXmlSerializer.OpenEhrNamespace);
                string otherContextType = ((IRmType)this.OtherContext).GetRmTypeName();
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    otherContextType = openEhrPrefix + ":" + otherContextType;
                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, otherContextType);
                this.OtherContext.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.HealthCareFacility != null)
            {
                writer.WriteStartElement(openEhrPrefix, "health_care_facility", RmXmlSerializer.OpenEhrNamespace);
                this.HealthCareFacility.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.Participations != null)
            {
                foreach (OpenEhr.RM.Common.Generic.Participation p in this.Participations)
                {
                    writer.WriteStartElement(openEhrPrefix, "participations", RmXmlSerializer.OpenEhrNamespace);
                    p.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }

        protected override void SetAttributeDictionary()
        {
            Check.Require(this.attributesDictionary != null, "attributeDictionary must not be null");

            this.attributesDictionary["start_time"] = this.startTime;
            this.attributesDictionary["end_time"] = this.endTime;
            this.attributesDictionary["location"] = this.location;
            this.attributesDictionary["setting"] = this.setting;
            this.attributesDictionary["other_context"] = this.otherContext;
            this.attributesDictionary["health_care_facility"] = this.healthCareFacility;
            this.attributesDictionary["participations"] = this.participations;
        }

        private void CheckInvariants()
        {
            // %HYYKA%
            //this.CheckInvariantsDefault();
            //Check.Invariant(this.StartTime != null, "start_time must not be null.");
            //Check.Invariant(this.Participations == null || this.Participations.Count>0, 
            //    "participations is not null implies not empty.");
            //Check.Invariant(this.Location== null || this.Location.Length>0, "Location is not null implies not empty.");

            //Check.Invariant(this.Setting != null, "setting must not be null");

            // TODO: terminology(terminology_id_openehr).has_code_for_group_id
        }

        private void CheckInvariantsDefault()
        {
            Check.Invariant(this.StartTime != null, "start_time must not be null.");
            Check.Invariant(this.Setting != null, "setting must not be null");
        }


        #region IRmType Members

        const string RM_TYPE_NAME = "EVENT_CONTEXT";

        #endregion

        #region IVisitable Members

        void IVisitable.Accept(IVisitor visitor)
        {
            visitor.VisitEventContext(this);
        }

        #endregion
    }
}
