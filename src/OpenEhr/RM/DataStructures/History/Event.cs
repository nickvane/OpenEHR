using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.AM.Archetype.ConstraintModel;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.DataStructures.History
{
    [Serializable]
    [RmType("openEHR", "DATA_STRUCTURES", "EVENT")]
    public abstract class Event<T> 
        : Locatable 
        where T : ItemStructure.ItemStructure
    {
        protected Event()
        { }
        
        protected Event(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
            Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit,
            DvDateTime time, T data,
            ItemStructure.ItemStructure state)
            : base(name, archetypeNodeId,
            uid, links, archetypeDetails, feederAudit)
        {
            Check.Require(time != null, "time must not be null");

            this.time = time;

            this.data = data;
            if (this.data != null)
                this.data.Parent = this;
            this.state = state;
            if (this.state != null)
                this.state.Parent = this;
        }

        private DataTypes.Quantity.DateTime.DvDateTime time;

        [RmAttribute("time", 1)]
        public DataTypes.Quantity.DateTime.DvDateTime Time
        {
            get
            {
                if(this.time == null)
                    this.time = base.attributesDictionary["time"] as DataTypes.Quantity.DateTime.DvDateTime;
                return this.time;
            }
            set
            {
                Check.Require(value != null, "value must not be null.");
                this.time = value;
                base.attributesDictionary["time"] = this.time;
            }
        }

        private DataStructures.ItemStructure.ItemStructure state;

        [RmAttribute("state")]
        public DataStructures.ItemStructure.ItemStructure State
        {
            get
            {
                if(this.state == null)
                    this.state = base.attributesDictionary["state"] as ItemStructure.ItemStructure;
                return this.state;
            }
            set
            {
                this.state = value;
                base.attributesDictionary["state"] = this.state;
            }
        }

        private T data;

        [RmAttribute("data", 1)]
        public T Data  
        {
            get
            {
                if(this.data == null)
                    this.data = base.attributesDictionary["data"] as T;
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

        public virtual DataTypes.Quantity.DateTime.DvDuration Offset()
        {
            History<T> parent = this.Parent as History<T>;

            if (parent == null)
                throw new ApplicationException("parent must not be null.");

            DataTypes.Quantity.DateTime.DvDuration offset;
            if (parent.Origin != null)
                offset = this.Time.Diff(parent.Origin);
            else
            {
                if (this.HasConstraint)
                {
                    CAttribute attribute = this.Constraint.GetAttribute("offset");
                    Check.Assert(attribute != null, "attribute must not be null");

                    CComplexObject constraint = attribute.Children[0] as CComplexObject;
                    Check.Assert(constraint != null, "constraint must not be null");
                    offset = constraint.DefaultValue as DataTypes.Quantity.DateTime.DvDuration;
                }
                else
                    throw new ApplicationException("origin is null with no offset constraint");
            }
            Check.Ensure(offset != null, "offset must not be null");
            return offset;
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            Check.Assert(reader.LocalName == "time", "Expected LocalName is 'time', but it's " + reader.LocalName);
            this.time = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime();
            this.time.ReadXml(reader);

            Check.Assert(reader.LocalName == "data", "Expected LocalName is 'data', but it's " + reader.LocalName);
            string dataType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
            this.data = OpenEhr.RM.Common.Archetyped.Impl.Locatable.GetLocatableObjectByType(dataType)
                as T;
            this.data.ReadXml(reader);
            this.data.Parent = this;

            if (reader.LocalName == "state")
            {
                string stateType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                // CM: 01/04/08 fixed a bug
                this.state = OpenEhr.RM.Common.Archetyped.Impl.Locatable.GetLocatableObjectByType(stateType)
                    as ItemStructure.ItemStructure;
                if (this.state == null)
                    throw new InvalidOperationException("stateType must be type of ItemStructure: " + stateType);
                this.state.ReadXml(reader);

                this.state.Parent = this;
            }

        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {           
            base.WriteXmlBase(writer);

            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "time", RmXmlSerializer.OpenEhrNamespace);
            this.Time.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement(openEhrPrefix, "data", RmXmlSerializer.OpenEhrNamespace);
            string dataType = ((IRmType)this.Data).GetRmTypeName();
            if (!string.IsNullOrEmpty(openEhrPrefix))
                dataType = openEhrPrefix + ":" + dataType;
            writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, dataType);
            ((ItemStructure.ItemStructure)(this.data)).WriteXml(writer);
            writer.WriteEndElement();

            if (this.State != null)
            {
                writer.WriteStartElement(openEhrPrefix, "state", RmXmlSerializer.OpenEhrNamespace);
                string stateType = ((IRmType)this.State).GetRmTypeName();
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    stateType = openEhrPrefix + ":" + stateType;
                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, stateType);
                this.State.WriteXml(writer);
                writer.WriteEndElement();
            }
        }     

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();

            base.attributesDictionary["time"] = this.time;
            base.attributesDictionary["data"] = this.data;
            base.attributesDictionary["state"]= this.state;
        }

        protected override void CheckInvariants()
        {
            base.CheckInvariants();

            // %HYYKA%
            //Check.Invariant(this.Time != null, "Time must not be null.");
            //Check.Invariant(this.Data != null, "Data must not be null.");
        }

        protected void CheckInvariantsDefault()
        {
            base.CheckInvariantsDefault();

            // %HYYKA%
            //Check.Invariant(this.Time != null, "Time must not be null.");
            //Check.Invariant(this.Data != null, "Data must not be null.");
        }

        protected override string RmTypeName
        {
            get { return ((IRmType)this).GetXmlRmTypeName(); }
        }
    }
}
