using System;
using System.Xml;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;
using OpenEhr.Factories;

namespace OpenEhr.RM.Common.Generic
{
    [Serializable]
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [RmType("openEHR", "COMMON", "PARTICIPATION")]
    public class Participation : RmType, System.Xml.Serialization.IXmlSerializable
    {
        public Participation()
        { }

        public Participation(DataTypes.Text.DvText function,
            DataTypes.Quantity.DvInterval<DataTypes.Quantity.DateTime.DvDateTime> time,
            DataTypes.Text.DvCodedText mode, PartyProxy performer)
            : this()
        {
            Check.Require(function != null, "function must not be null.");
            Check.Require(mode != null, "mode must not be null");
            Check.Require(performer != null, "performer must not be null");

            this.function = function;
            this.time = time;
            this.mode = mode;
            this.performer = performer;

            this.CheckStrictInvariants();
        }

        private DataTypes.Text.DvText function;

        [RmAttribute("function", 1)]
        [RmTerminology("participation function")]
        public DataTypes.Text.DvText Function
        {
            get
            {
                return function;
            }
            set
            {
                this.function = value;
            }
        }

        private DataTypes.Quantity.DvInterval<DataTypes.Quantity.DateTime.DvDateTime> time;

        public DataTypes.Quantity.DvInterval<DataTypes.Quantity.DateTime.DvDateTime> Time
        {
            get
            {
                return time;
            }
        }

        private DataTypes.Text.DvCodedText mode;

        [RmAttribute("mode", 1)]
        [RmTerminology("participation mode")]
        public DataTypes.Text.DvCodedText Mode
        {
            get
            {
                return mode;
            }
            set
            {
                this.mode = value;
            }
        }

        private PartyProxy performer;

        public PartyProxy Performer
        {
            get
            {
                return this.performer;
            }           
            set
            {
                this.performer = value;
            }
        }

        private void CheckStrictInvariants()
        {
            Check.Require(this.Function != null, "function must not be null");
            Check.Require(this.Mode != null, "mode must not be null");
            Check.Require(this.Performer != null, "performer must not be null");
        }

        #region IXmlSerializable Members

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(XmlReader reader)
        {
            this.ReadXml(reader);
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(XmlWriter writer)
        {
            this.WriteXml(writer);
        }

        #endregion
        internal void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "function",
                "Expected LocalName is 'function' not " + reader.LocalName);
            string functionType = RmXmlSerializer.ReadXsiType(reader);
            if (!string.IsNullOrEmpty(functionType))
            {
                this.function = RmFactory.DataValue(functionType) as DataTypes.Text.DvText;
            }
            else
                this.function = new OpenEhr.RM.DataTypes.Text.DvText();
            this.function.ReadXml(reader);
            reader.MoveToContent();                

            Check.Assert(reader.LocalName == "performer",
                "Expected LocalName is 'performer' not " + reader.LocalName);
            string performerType = RmXmlSerializer.ReadXsiType(reader);
            this.performer = RmFactory.PartyProxy(performerType);

            this.performer.ReadXml(reader);

            if (reader.LocalName == "time")
            {
                this.time = new OpenEhr.RM.DataTypes.Quantity.DvInterval<OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime>();
                this.time.ReadXml(reader);
            }          

            Check.Assert(reader.LocalName == "mode", "Expected LocalName is 'mode' not " + reader.LocalName);
            this.mode = new OpenEhr.RM.DataTypes.Text.DvCodedText();
            this.mode.ReadXml(reader);         

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
                "Expected endElement of PartyIdentified.");
            reader.ReadEndElement();
            reader.MoveToContent();
        }

        internal void WriteXml(XmlWriter writer)
        {
            this.CheckStrictInvariants();

            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "function", RmXmlSerializer.OpenEhrNamespace);
            if (this.Function.GetType() == typeof(DataTypes.Text.DvCodedText))
            {
                string functionType = ((IRmType)this.Function).GetRmTypeName();
                if (!string.IsNullOrEmpty(functionType))
                {
                    if (!string.IsNullOrEmpty(openEhrPrefix))
                        functionType = openEhrPrefix + ":" + functionType;
                }
                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, functionType);
            }
            this.Function.WriteXml(writer);
            writer.WriteEndElement();
           
            writer.WriteStartElement(openEhrPrefix, "performer", RmXmlSerializer.OpenEhrNamespace);
            string performerType = ((IRmType)this.Performer).GetRmTypeName();
            if (!string.IsNullOrEmpty(openEhrPrefix))
                performerType = openEhrPrefix + ":" + performerType;
            writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, performerType);
            this.Performer.WriteXml(writer);
            writer.WriteEndElement();

            if (this.Time != null)
            {
                writer.WriteStartElement(openEhrPrefix, "time", RmXmlSerializer.OpenEhrNamespace);               
                this.Time.WriteXml(writer);
                writer.WriteEndElement();
            }

            writer.WriteStartElement(openEhrPrefix, "mode", RmXmlSerializer.OpenEhrNamespace);
            this.Mode.WriteXml(writer);
            writer.WriteEndElement();
           
        }

        public static XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new XmlQualifiedName("PARTICIPATION", RmXmlSerializer.OpenEhrNamespace);

        }
    }
}
