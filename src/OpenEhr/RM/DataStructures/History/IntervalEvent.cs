using System;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataStructures.History
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_STRUCTURES","INTERVAL_EVENT")]
    public class IntervalEvent<T> : Event<T>, System.Xml.Serialization.IXmlSerializable where T : DataStructures.ItemStructure.ItemStructure
    {
        public IntervalEvent()
        { }

        public IntervalEvent(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
            Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit,
            DvDateTime time, T data,
            ItemStructure.ItemStructure state, DvDuration width, 
            int? sampleCount, DvCodedText mathFunction)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit, time, data, state)
        {
            Check.Require(width != null, "width must not be null");
            Check.Require(mathFunction != null, "math_function must not be null");

            this.width = width;
           
            if (sampleCount != null)
            {
                this.sampleCount = (int)sampleCount;
                this.sampleCountSet = true;
            }

            this.mathFunction = mathFunction;

            SetAttributeDictionary();
            CheckInvariants();
        }

        #region class properties
        private DvDuration width;

        [RmAttribute("width", 1)]
        public DvDuration Width
        {
            get
            {
                if (width == null)
                    width = base.attributesDictionary["width"] as DvDuration;
                return width;
            }
            set
            {
                Check.Require(value != null, "value must not be null.");
                this.width = value;
                base.attributesDictionary["width"] = this.width;
            }
        }

        private int sampleCount = 0;
        private bool sampleCountSet;

        [RmAttribute("sample_count")]
        public int SampleCount
        {
            get
            {
                return sampleCount;
            }
            set
            {
                this.sampleCount = value;
                this.sampleCountSet = true;
                base.attributesDictionary["sample_count"] = value;
            }
        }

        private DvCodedText mathFunction;

        [RmAttribute("math_function", 1)]
        [RmTerminology("event math function")]
        public DvCodedText MathFunction
        {
            get
            {
                if (mathFunction == null)
                    mathFunction = base.attributesDictionary["math_function"] as DvCodedText;
                return this.mathFunction;
            }
            set
            {
                Check.Require(value != null, "value must not be null.");
                this.mathFunction = value;
                base.attributesDictionary["math_function"] = this.mathFunction;
            }
        }

        #endregion

        public override DvDuration Offset()
        {
            History<T> parent = this.Parent as History<T>;

            if (parent == null)
                throw new ApplicationException("parent must not be null.");

            DataTypes.Quantity.DateTime.DvDuration offset;
            if (parent.Origin != null)
            {
                DvDateTime intervalStartTime = this.IntervalStartTime();
                Check.Assert(intervalStartTime != null, "intervalStartTime must not be null.");
                offset = intervalStartTime.Diff(parent.Origin);
                Check.Ensure(offset != null, "offset must not be null");

                return offset;
            }
            else
                return base.Offset();
        }

        public DvDateTime IntervalStartTime()
        {
            DesignByContract.Check.Require(this.Width != null, "Width must not be null.");
            DesignByContract.Check.Require(this.Time != null, "Time must not be null.");

            return this.Time.Subtract(this.width) as DvDateTime;
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
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("INTERVAL_EVENT", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            DesignByContract.Check.Assert(reader.LocalName == "width",
                "Expected local name is width not " + reader.LocalName);
            this.width = new DvDuration();
            this.width.ReadXml(reader);

            if (reader.LocalName == "sample_count")
            {
                this.SampleCount = reader.ReadElementContentAsInt("sample_count", RmXmlSerializer.OpenEhrNamespace);
                reader.MoveToContent();
            }

            DesignByContract.Check.Assert(reader.LocalName == "math_function",
               "Expected local name is math_function not " + reader.LocalName);
            this.mathFunction = new DvCodedText();
            this.mathFunction.ReadXml(reader);
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            DesignByContract.Check.Assert(this.Width != null, "width is mandatory.");
            writer.WriteStartElement("width", RmXmlSerializer.OpenEhrNamespace);
            this.Width.WriteXml(writer);
            writer.WriteEndElement();

            if (this.sampleCountSet)
                writer.WriteElementString("sample_count", RmXmlSerializer.OpenEhrNamespace, this.SampleCount.ToString());

            DesignByContract.Check.Assert(this.MathFunction != null, "math function must not be null.");
            writer.WriteStartElement("math_function", RmXmlSerializer.OpenEhrNamespace);
            this.MathFunction.WriteXml(writer);
            writer.WriteEndElement();
        }

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();

            base.attributesDictionary["width"] = this.width;
            if (this.sampleCountSet)
                base.attributesDictionary["sample_count"] = this.sampleCount;
            base.attributesDictionary["math_function"] = this.mathFunction;
        }
    }
}
