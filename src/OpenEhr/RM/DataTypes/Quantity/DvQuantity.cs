using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.AssumedTypes;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Quantity
{
    /// <summary>
    /// Quantifified type representing "scientific" quantities, i.e.
    /// quantities expressed as a magnitude and units.
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_QUANTITY")]
    public class DvQuantity : DvAmount<DvQuantity>, System.Xml.Serialization.IXmlSerializable
    {
        #region constructors
        public DvQuantity()
        { }

        public DvQuantity(double magnitude, string units, int precision, float accuracy,
            bool accuracyIsPercent, string magnitudeStatus, CodePhrase normalStatus,
            DvInterval<DvQuantity> normalRange, ReferenceRange<DvQuantity>[] otherReferenceRanges)
            : this(magnitude, units, accuracy, accuracyIsPercent, magnitudeStatus, normalStatus, normalRange,
            otherReferenceRanges)
        {
            DesignByContract.Check.Require(precision >= -1, "precision must be greater or equal to -1");

            this.precision = precision;
            this.precisionSet = true;
        }

        private DvQuantity(double magnitude, string units, float accuracy,
           bool accuracyIsPercent, string magnitudeStatus, CodePhrase normalStatus,
           DvInterval<DvQuantity> normalRange, ReferenceRange<DvQuantity>[] otherReferenceRanges)
            : this()
        {
            this.magnitude = magnitude;
            this.units = units;

            SetBaseData(accuracy, accuracyIsPercent, magnitudeStatus, normalStatus, normalRange, otherReferenceRanges);
        }

        public DvQuantity(Double magnitude, string units)
            : this(magnitude, units, -1, false, null, null, null, null)
        { }

        public DvQuantity(Double magnitude, string units, int precision)
            : this(magnitude, units, precision, -1, false, null, null, null, null)
        { }

        public DvQuantity(Double magnitude, string units, DvInterval<DvQuantity> normalRange)
            : this(magnitude, units, -1, false, null, null, normalRange, null)
        { }

        public DvQuantity(Double magnitude, string units, int precision, DvInterval<DvQuantity> normalRange)
            : this(magnitude, units, precision, -1, false, null, null, normalRange, null)
        { }

        public DvQuantity(Double magnitude, string units, int precision,
          DvInterval<DvQuantity> normalRange, string magnitudeStatus)
            : this(magnitude, units, precision, -1, false, magnitudeStatus, null, normalRange, null)
        { }

        public DvQuantity(Double magnitude, string units, DvInterval<DvQuantity> normalRange,
            ReferenceRange<DvQuantity>[] referenceRanges)
            : this(magnitude, units, -1, false, null, null, normalRange, referenceRanges)
        { }

        public DvQuantity(Double magnitude, string units, int precision,
            DvInterval<DvQuantity> normalRange, ReferenceRange<DvQuantity>[] referenceRanges)
            : this(magnitude, units, precision, -1, false, null, null, normalRange, referenceRanges)
        { }

        public DvQuantity(Double magnitude, string units, int precision,
         DvInterval<DvQuantity> normalRange, ReferenceRange<DvQuantity>[] referenceRanges,
            string magnitudeStatus)
            : this(magnitude, units, precision, -1, false, magnitudeStatus, null, normalRange, referenceRanges)
        { }

        public DvQuantity(Double magnitude, string units, int precision,
           DvInterval<DvQuantity> normalRange, ReferenceRange<DvQuantity>[] referenceRanges,
            float accuracy, bool accuracyIsPercent, string magnitudeStatus)
            : this(magnitude, units, precision, accuracy, accuracyIsPercent, magnitudeStatus, null, 
            normalRange, referenceRanges)
        { }

        #endregion

        double magnitude;

        public double Magnitude
        {
            get { return GetMagnitude(); }
            set { this.magnitude = value; }
        }

        protected override double GetMagnitude()
        {
            return this.magnitude;
        }

        string units;

        public string Units
        {
            get { return this.units; }
            set
            {
                Check.Invariant(value != null, "Units must not be null.");
                this.units = value;
            }
        }

        bool precisionSet = false;
        int precision = -1;

        public int Precision
        {
            get { return this.precision; }
        }

        public override string ToString()
        {
            string quantityString = Magnitude.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + " " + Units;
           
            if (!string.IsNullOrEmpty(this.MagnitudeStatus) && this.MagnitudeStatus!="=")
                return this.MagnitudeStatus + " "+quantityString;

            return quantityString;

        }
        
        private List<ReferenceRange<DvQuantity>> otherReferenceRanges;

        public override bool IsStrictlyComparableTo(DvOrdered<DvQuantity> other)
        {
            DvQuantity otherObj = other as DvQuantity;

            DesignByContract.Check.Require(otherObj!=null);
            
            if (!otherObj.Units.Equals(this.Units, StringComparison.InvariantCulture))
                return false;

            return true;
        }

        /// <summary>
        /// True if precision = 0; quantity represents an integral number
        /// </summary>
        /// <returns></returns>
        public bool IsIntegral()
        {
            return this.Precision == 0;
        }

        protected override DvAmount<DvQuantity> Subtract(DvAmount<DvQuantity> b)
        {
            DesignByContract.Check.Require(this.IsStrictlyComparableTo(b));

            DvQuantity bObj = b as DvQuantity;            

            return new DvQuantity(this.Magnitude - bObj.Magnitude, this.Units);
        }

        protected override DvAmount<DvQuantity> Plus(DvAmount<DvQuantity> b)
        {
            DesignByContract.Check.Require(this.IsStrictlyComparableTo(b));

            DvQuantity bObj = b as DvQuantity;            

            return new DvQuantity(this.Magnitude + bObj.Magnitude, this.Units);
        }

        protected override DvAmount<DvQuantity> GetDvAmountWithZeroMagnitude()
        {
            return new DvQuantity(0, this.Units);
        }

        protected void CheckInvariants()
        {
            base.CheckInvariants();
            DesignByContract.Check.Invariant(this.Precision>=-1);
            // %HYYKA%
            //DesignByContract.Check.Invariant(this.magnitudeSet, "magnitude must have been set.");

            //DesignByContract.Check.Invariant(!string.IsNullOrEmpty(this.Units), 
            //    "Units must not be null or empty.");
            DesignByContract.Check.Invariant(this.Units != null, "Units must not be null");
        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("DV_QUANTITY", RmXmlSerializer.OpenEhrNamespace);
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

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            Check.Assert(reader.LocalName == "magnitude", "reader localName must be 'magnitude'");
            this.magnitude
                = reader.ReadElementContentAsDouble("magnitude", RmXmlSerializer.OpenEhrNamespace);

            reader.MoveToContent();

            //innerQuantity.units 
            Check.Assert(reader.LocalName == "units", "reader localName must be 'units'");
            this.units
                = reader.ReadElementString("units", RmXmlSerializer.OpenEhrNamespace);
            Check.Assert(this.units != null, "units must not be null.");
            reader.MoveToContent();

            if (reader.LocalName == "precision")
            {
                this.precision = reader.ReadElementContentAsInt("precision", RmXmlSerializer.OpenEhrNamespace);
                this.precisionSet = true;
                reader.MoveToContent();
            }

        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            this.WriteXml(writer);
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer); 

            // CM: 29/09/09 EHR-1008
            writer.WriteElementString(prefix, "magnitude", RmXmlSerializer.OpenEhrNamespace, this.Magnitude.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            
            writer.WriteElementString(prefix, "units", RmXmlSerializer.OpenEhrNamespace, this.Units);

            // CM: 08/05/09 precision is optional, include this value only when it is recorded. 
            // In openehr schema, it's default value is -1. In fact, -1 is equivallent to not recorded. In the
            // specifications, -1 means no limits
            if (this.Precision > -1)
                writer.WriteElementString(prefix, "precision", RmXmlSerializer.OpenEhrNamespace, this.Precision.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        
        #endregion
    }
}
