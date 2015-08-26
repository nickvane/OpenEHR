using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Factories;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Quantity
{
    /// <summary>
    /// Defines a named range to be associated with any ORDERED datum. Each such
    /// range is particular to the patient and context, e.g. sex, age, and any other
    /// factor which affects ranges.
    /// May be used to represent normal, therapeutic, dangerous, critical etc ranges.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_REFERENCE_RANGE")]
    public class ReferenceRange<T>: System.Xml.Serialization.IXmlSerializable where T : DvOrdered<T>
    {
        public ReferenceRange()
        { }

        public ReferenceRange(Text.DvText meaning, DvInterval<T> range):this()
        {
            this.meaning = meaning;
            this.range = range;

            this.CheckInvariants();
        }

        private Text.DvText meaning;

        public Text.DvText Meaning
        {
            get
            {
                return this.meaning;
            }
           
        }

        private DvInterval<T> range;

        public DvInterval<T> Range
        {
            get
            {
                return this.range;
            }
            
        }

        /// <summary>
        /// Indicates if the value 'val' is inside the range
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool IsInRange(T val)
        {
            DesignByContract.Check.Require(val!=null);

            return this.Range.Has(val);
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
            return new System.Xml.XmlQualifiedName("REFERENCE_RANGE", RmXmlSerializer.OpenEhrNamespace);
        }

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "meaning",
                "Expected LocalName is meaning rather than " + reader.LocalName);
            if (this.meaning == null)
            {
                string meaningType = RmXmlSerializer.ReadXsiType(reader);
                if (meaningType == null)
                    this.meaning = new DvText();
                else
                    this.meaning = RmFactory.DataValue(meaningType) as DvText;
            }
            this.meaning.ReadXml(reader);

            DesignByContract.Check.Assert(reader.LocalName == "range",
                "Expected LocalName is range rather than " + reader.LocalName);            
            if (this.range == null)
                this.range = new DvInterval<T>();                   
            this.range.ReadXml(reader);              

            if (!reader.IsStartElement())
                reader.ReadEndElement();
            reader.MoveToContent();

            this.CheckInvariants();
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            string openEhrNamespace = RmXmlSerializer.OpenEhrNamespace;
            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            Check.Assert(this.Meaning != null, "meaning must not be null.");
            writer.WriteStartElement("meaning", openEhrNamespace);
            if (this.Meaning.GetType() == typeof(OpenEhr.RM.DataTypes.Text.DvCodedText))
            {
                string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, "DV_CODED_TEXT");
            }
            this.Meaning.WriteXml(writer);
            writer.WriteEndElement();

            Check.Assert(this.Range != null, "range must not be null.");
            writer.WriteStartElement("range", openEhrNamespace);           
            this.Range.WriteXml(writer);
            writer.WriteEndElement();
        }

        private void CheckInvariants()
        {
            Check.Invariant(this.Meaning != null, "Meaning must not be null.");
            Check.Invariant(this.Range != null, "Range must not be null.");
        }
    }
}
