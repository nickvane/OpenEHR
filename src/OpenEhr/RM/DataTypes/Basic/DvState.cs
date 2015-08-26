using System;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Basic
{
    /// <summary>
    /// For representing state values which obey a defined state machine, such as a variable
    /// representing the states of an instruction or care process.
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_STATE")]
    public class DvState: DataValue, System.Xml.Serialization.IXmlSerializable
    {
        #region Constructors
         public DvState() 
         { }

        // CM: 15/02/08
        public DvState(DvCodedText value, bool isTerminal):
            this()
        {
            this.Value = value;
            this.IsTerminal = isTerminal;

            this.CheckInvariants();
        }

        #endregion

        #region Class properties
        private DvCodedText value;

        /// <summary>
        /// The state name. State names are determined by a state/event table defined in archetypes,
        /// and coded using openEHR Terminology or local archetype terms, as specified by the archetype.
        /// </summary>
        public DvCodedText Value
        {
            get
            {
                return value;
            }
            set
            {
                DesignByContract.Check.Require(value != null, "value must not be null.");
                this.value = value;
            }
        }

        private bool isTerminalSet;
        private bool isTerminal;
        /// <summary>
        /// Indicates whether this state is a terminal state, such as “aborted”, “completed” etc
        /// from which no further transitions are possible.
        /// </summary>
        public bool IsTerminal
        {
            get
            {
                return isTerminal;
            }
            set
            {
                this.isTerminal = value;
                this.isTerminalSet = true;
            }
        }

        #endregion

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            if (reader.LocalName != "value")
                throw new InvalidXmlException("value", reader.LocalName);
            this.Value = new DvCodedText();
            this.Value.ReadXml(reader);

            if (reader.LocalName != "is_terminal")
                throw new InvalidXmlException("is_terminal", reader.LocalName);
            this.IsTerminal = reader.ReadElementContentAsBoolean("is_terminal", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            DesignByContract.Check.Require(this.Value != null, "Value must not be null.");
            
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "value", RmXmlSerializer.OpenEhrNamespace);
            this.Value.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteElementString("is_terminal", RmXmlSerializer.OpenEhrNamespace, this.IsTerminal.ToString().ToLower());
        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("DV_STATE", RmXmlSerializer.OpenEhrNamespace);
        }


        protected override void CheckInvariants()
        {
            DesignByContract.Check.Invariant(this.Value != null, "DvState.Value must not be null.");
        }

        #region IXmlSerializable Members

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            this.ReadXmlBase(reader);
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            this.WriteXmlBase(writer);
        }

        #endregion
    }
}
