using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Quantity
{
    /// <summary>
    /// Models rankings and scores, e.g. pain, Apgar values, etc, where there is 
    /// a) implied ordering, b) no implication that the distance between each value is constant,
    /// and c) the total number of values is finite. 
    /// Use: used for recording any clinical datum which is customarily recorded using
    /// symbolic values. Example: the results on a urinalysis strip, e.g. 
    /// {neg, trace, +, ++, +++} are used for leucocytes, protein, nitrites etc; 
    /// for non-haemolysed blood {neg, trace, moderate}; for haemolysed blood 
    /// {neg, trace, small, moderate, large}.
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_ORDINAL")]
    public class DvOrdinal : DvOrdered<DvOrdinal>, IFormattable, System.Xml.Serialization.IXmlSerializable
    {
        #region constructors

        public DvOrdinal()
        { }

        public DvOrdinal(int value, Text.DvCodedText symbol, CodePhrase normalStatus,
            DvInterval<DvOrdinal> normalRange, ReferenceRange<DvOrdinal>[] otherReferenceRanges)
            : this()
        {
            DesignByContract.Check.Require(symbol != null, "symbol must not be null.");
       
            this.value = value;
            this.valueSet = true;
       
            this.symbol = symbol;

            SetBaseData(normalStatus, normalRange, otherReferenceRanges);

            this.CheckInvariants();
        }

        public DvOrdinal(int value, Text.DvCodedText symbol,
            DvInterval<DvOrdinal> normalRange,
            ReferenceRange<DvOrdinal>[] referenceRanges)
            : this(value, symbol, null, normalRange, referenceRanges)
        { }

        #endregion

        private int value;
        private bool valueSet;

        public int Value
        {
            get {
                return this.value;
            }
        }

        private Text.DvCodedText symbol;

        public Text.DvCodedText Symbol
        {
            get
            {
                return this.symbol;
            }
        }

        // TODO: Limits function
        public ReferenceRange<DvOrdinal> Limits
        {
            get { throw new NotImplementedException(); }
            
        }

        public override bool IsStrictlyComparableTo(DvOrdered<DvOrdinal> other)
        {
            DesignByContract.Check.Require(other != null && other is DvOrdinal);

            return true;
        }

        protected override int CompareTo(object obj)
        {
            DesignByContract.Check.Require(IsStrictlyComparableTo(obj as DvOrdinal));

            DvOrdinal objOrdinal = obj as DvOrdinal;
            if (this.Value == objOrdinal.Value)
                return 0;
            else if (this.Value > objOrdinal.Value)
                return 1;
            else
                return -1;
        }

        public override string ToString()
        {
            return this.Value + "|" + this.Symbol.ToString();
        }


        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {            
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("DV_ORDINAL", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            Check.Assert(reader.LocalName == "value", "reader.LocalName must be 'value'");
            this.value = reader.ReadElementContentAsInt("value", RmXmlSerializer.OpenEhrNamespace);
            this.valueSet = true;

            if (!reader.IsStartElement())
                reader.ReadEndElement();

            reader.MoveToContent();

            Check.Assert(reader.LocalName == "symbol", "reader.LocalName must be 'symbol'");
            if (this.symbol == null)
                this.symbol = new OpenEhr.RM.DataTypes.Text.DvCodedText();
            this.symbol.ReadXml(reader);

        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            writer.WriteElementString("value", RmXmlSerializer.OpenEhrNamespace, this.Value.ToString());
            writer.WriteStartElement("symbol", RmXmlSerializer.OpenEhrNamespace);
            this.Symbol.WriteXml(writer);
            writer.WriteEndElement();
        }

        protected override void CheckInvariants()
        {
            base.CheckInvariants();
            Check.Invariant(this.valueSet, "Value must be set");
            Check.Invariant(this.Symbol != null, "Symbol must not be null");
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

        #region IFormattable Members

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return this.ToString();
        }

        #endregion
    }
}
