using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataStructures.ItemStructure.Representation;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.AssumedTypes.Impl;
using OpenEhr.Factories;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;
using OpenEhr.AM.Archetype.ConstraintModel;

namespace OpenEhr.RM.DataStructures.History
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_STRUCTURES", "HISTORY")]
    public class History<T> : DataStructure, System.Xml.Serialization.IXmlSerializable where T : DataStructures.ItemStructure.ItemStructure
    {
        public History() 
        { }

        public History(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
            Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit,
            DvDateTime origin, DvDuration period, DvDuration duration,
            Event<T>[] events, ItemStructure.ItemStructure summary)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit)
        {
            this.period = period;
            this.duration = duration;
            this.summary = summary;
            if (this.summary != null)
                this.summary.Parent = this;
            if (events != null)
            {
                this.events = RmFactory.LocatableList<Event<T>>(this, events);
            }
            this.origin = origin;

            SetAttributeDictionary();
            CheckInvariants();
        }

        private DataTypes.Quantity.DateTime.DvDateTime origin;

        [RmAttribute("origin", 1)]
        public DataTypes.Quantity.DateTime.DvDateTime Origin
        {
            get
            {
                if (this.origin == null)
                    this.origin = base.attributesDictionary["origin"] as DataTypes.Quantity.DateTime.DvDateTime;

                return this.origin;
            }
            set
            {
                Check.Require(value != null, "value must not be null");
                this.origin = value;
                base.attributesDictionary["origin"] = value;
            }
        }

        private DataTypes.Quantity.DateTime.DvDuration period;

        [RmAttribute("period")]
        public DataTypes.Quantity.DateTime.DvDuration Period
        {
            get
            {
                if (this.period == null)
                    this.period = base.attributesDictionary["period"] as DataTypes.Quantity.DateTime.DvDuration;
                return this.period;
            }
            set
            {
                this.period = value;
                base.attributesDictionary["period"] = this.period;
            }
        }

        private DataTypes.Quantity.DateTime.DvDuration duration;

        [RmAttribute("duration")]
        public DataTypes.Quantity.DateTime.DvDuration Duration
        {
            get
            {
                if (this.duration == null)
                    this.duration = base.attributesDictionary["duration"] as DataTypes.Quantity.DateTime.DvDuration;
                return this.duration;
            }
            set
            {
                this.duration = value;
                base.attributesDictionary["duration"] = this.duration;
            }
        }

        private ItemStructure.ItemStructure summary;

        [RmAttribute("summary")]
        public ItemStructure.ItemStructure Summary
        {
            get
            {
                if (this.summary == null)
                    this.summary = base.attributesDictionary["summary"] as ItemStructure.ItemStructure;
                return this.summary;
            }
            set
            {
                if (this.summary != null)
                    this.summary.Parent = null;
                this.summary = value;
                if (this.summary != null)
                    this.summary.Parent = this;
                base.attributesDictionary["summary"] = this.summary;
            }
        }

        private AssumedTypes.List<Event<T>> events;

        [RmAttribute("events")]
        public AssumedTypes.List<Event<T>> Events
        {
            get
            {
                if (this.events == null)
                    this.events = base.attributesDictionary["events"] as AssumedTypes.List<Event<T>>;
                return this.events;
            }
        }

        public bool IsPeriodic()
        {
            throw new System.NotImplementedException();
        }

        // CM: 16/04/2008
        public override Item AsHierarchy()
        {
            throw new NotImplementedException("as_hierarchy function has not been implemented in HISTORY");
        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadCompositionSchema(xs);
            return new System.Xml.XmlQualifiedName("HISTORY", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            // TODO: CM: 09/07/09 need to disable this assertion check when EhrTypes is not used
            // in deserialization since TDS origin is optional. When the origin is not existing in
            // openEHR instance, need to get the origin by CalculatOrigin function. At the moment,
            // TddTransformer still uses EhrTypes for the deserialization. 
            DesignByContract.Check.Assert(reader.LocalName == "origin",
                "Expected LocalName is 'origin', but it is " + reader.LocalName);

            // HKF: 7 Aug 2009 - need to allow origin to be nil for TDD and EhrGateDataObjects transformation prior to OperationalTemplate augmentation
            if (reader.HasAttributes && reader.GetAttribute("nil", RmXmlSerializer.XsiNamespace) == "true")
            {
                // leave origin as null and skip
                reader.Skip();
            }
            else
            {
                this.origin = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime();
                this.origin.ReadXml(reader);
            }

            if (reader.LocalName == "period")
            {
                this.period = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration();
                this.period.ReadXml(reader);
            }

            if (reader.LocalName == "duration")
            {
                this.duration = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration();
                this.duration.ReadXml(reader);
            }

            if (reader.LocalName == "events")
            {
                LocatableList<Event<ItemStructure.ItemStructure>> events =
                    new LocatableList<Event<OpenEhr.RM.DataStructures.ItemStructure.ItemStructure>>();
                do
                {
                    string eventType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);

                    Event<ItemStructure.ItemStructure> anEvent =
                        OpenEhr.RM.Common.Archetyped.Impl.Locatable.GetLocatableObjectByType(eventType)
                        as Event<ItemStructure.ItemStructure>;
                    if (anEvent == null)
                        throw new InvalidOperationException("anEvent must not be null.");
                    anEvent.ReadXml(reader);

                    anEvent.Parent = this;
                    events.Add(anEvent);

                } while (reader.LocalName == "events");

                this.events = events as AssumedTypes.List<Event<T>>;
            }

            if (reader.LocalName == "summary")
            {
                string summaryType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                this.summary = Locatable.GetLocatableObjectByType(summaryType) as ItemStructure.ItemStructure;
                if (this.summary == null)
                    throw new InvalidOperationException("History summary type must be type of ItemStructure: " + summaryType);
                this.summary.ReadXml(reader);
                this.summary.Parent = this;
            }
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            base.WriteXmlBase(writer);

            writer.WriteStartElement(openEhrPrefix, "origin", RmXmlSerializer.OpenEhrNamespace);
            this.Origin.WriteXml(writer);
            writer.WriteEndElement();

            if (this.Period != null)
            {
                writer.WriteStartElement(openEhrPrefix, "period", RmXmlSerializer.OpenEhrNamespace);
                this.Period.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.Duration != null)
            {
                writer.WriteStartElement(openEhrPrefix, "duration", RmXmlSerializer.OpenEhrNamespace);
                this.Duration.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.Events != null && this.Events.Count > 0)
            {
                foreach (Event<T> anEvent in this.Events)
                {
                    writer.WriteStartElement(openEhrPrefix, "events", RmXmlSerializer.OpenEhrNamespace);
                    
                    string eventType = ((IRmType)anEvent).GetRmTypeName();
                    if (!string.IsNullOrEmpty(openEhrPrefix))
                        eventType = openEhrPrefix + ":" + eventType;
                    
                    writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, eventType);
                    
                    anEvent.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }

            if (this.Summary != null)
            {
                writer.WriteStartElement(openEhrPrefix, "summary", RmXmlSerializer.OpenEhrNamespace);
                string summaryType = ((IRmType)this.Summary).GetRmTypeName();
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    summaryType = openEhrPrefix + ":" + summaryType;

                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, summaryType);

                this.Summary.WriteXml(writer);
                writer.WriteEndElement();    
            }
        }

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();
            base.attributesDictionary["origin"]= this.origin;
            base.attributesDictionary["period"]= this.period;
            base.attributesDictionary["duration"]= this.duration;
            base.attributesDictionary["events"]= this.events;
            base.attributesDictionary["summary"]= this.summary;
        }

        protected override void CheckInvariants()
        {
            base.CheckInvariants();
            // %HYYKA%
            // HKF: 7 Aug 2009 - need to allow origin to be null for TDD and EhrGateDataObjects transformation prior to OperationalTemplate augmentation
            //DesignByContract.Check.Invariant(this.Origin != null, "origin must not be null.");
            //DesignByContract.Check.Invariant(this.Events == null || this.Events.Count > 0,
            //    "events /= Void and then not events.is_empty");
        }

        protected void CheckInvariantsDefault()
        {
            base.CheckInvariantsDefault();

            // %HYYKA%
            //DesignByContract.Check.Invariant(this.Origin != null, "origin must not be null.");
        }

        protected override string RmTypeName
        {
            get { return ((IRmType)this).GetXmlRmTypeName(); }
        }

        /// <summary>
        /// Get the origin value based on the history.Events time or IntervalStartTime
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="history"></param>
        /// <returns></returns>
        static public DvDateTime CalculateOrigin(History<T> history)
        {
            Check.Require(history != null, "history must not be null");

            DvDateTime originValue = null;
            if (history.Events != null && history.Events.Count > 0)
            {
                foreach (Event<T> anEvent in history.Events)
                {
                    DvDateTime eventTime;
                    IntervalEvent<T> intervalEvent = anEvent as IntervalEvent<T>;
                    if (intervalEvent != null)
                        eventTime = intervalEvent.IntervalStartTime();
                    else
                        eventTime = anEvent.Time;

                    CComplexObject constraint = ((IRmType)anEvent).Constraint as CComplexObject;
                    if (constraint != null)
                    {
                        CAttribute offsetConstraint = constraint.GetAttribute("offset");
                        if (offsetConstraint != null && offsetConstraint.Children != null && offsetConstraint.Children.Count == 1)
                        {
                            CComplexObject offsetDuration = offsetConstraint.Children[0] as CComplexObject;
                            eventTime = eventTime.Subtract((DvDuration)offsetDuration.DefaultValue);
                        }
                    }

                    if (originValue == null || originValue > eventTime)
                        originValue = new DvDateTime(eventTime.Value);
                }
            }
            Check.Ensure(originValue != null, "originValue must not be null");
            return originValue;
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
    }
}
