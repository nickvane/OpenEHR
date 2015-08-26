using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Quantity
{
    /// <summary>
    /// Class of enumeration constants defining types of proportion for the DvProportion 
    /// class.
    /// </summary>
    public enum ProportionKind
    {
        pkRatio = 0,
        pkUnitary = 1,
        pkPercent = 2,
        pkFraction = 3,
        pkIntegerFraction = 4
    };

    /// <summary>
    /// Models a ratio of values, i.e. where the numerator 
    /// and denominator are both pure numbers. Used for recording titers (e.g. 1:128),
    /// concentration ratios, e.g. Na:K (unitary denominator), albumin:creatinine
    /// ratio, and percentages, e.g. red cell distirbution width (RDW).
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_PROPORTION")]
    public class DvProportion : DvAmount<DvProportion>, System.Xml.Serialization.IXmlSerializable
    {
        #region constructors

        public DvProportion()
        { }

        public DvProportion(float numerator, float denominator, ProportionKind type)
            : this()
        {
            DesignByContract.Check.Require(type != ProportionKind.pkUnitary ||denominator == 1);
            DesignByContract.Check.Require(type != ProportionKind.pkPercent || denominator == 100);

            this.type = type;

            if (type == ProportionKind.pkFraction || type == ProportionKind.pkIntegerFraction)
            {
                this.numerator = (float)Math.Truncate(numerator);
                this.denominator = (float)Math.Truncate(denominator);
                this.precision = 0;
                this.precisionSet = true;
            }

            else
            {
                this.numerator = numerator;
                this.denominator = denominator;
            }

            this.numeratorSet = true;
            this.denominatorSet = true;
               
            CheckInvariants();
        }

        public DvProportion(float numerator, float denominator, ProportionKind type,
           DvInterval<DvProportion> normalRange, ReferenceRange<DvProportion>[] otherReferenceRanges)
            : this()
        {
            DesignByContract.Check.Require(otherReferenceRanges == null || otherReferenceRanges.Length > 0);
            DesignByContract.Check.Require(type != ProportionKind.pkUnitary || denominator == 1);
            DesignByContract.Check.Require(type != ProportionKind.pkPercent || denominator == 100);

            this.type = type;

            if (type == ProportionKind.pkFraction || type == ProportionKind.pkIntegerFraction)
            {
                this.numerator = (float)Math.Truncate(numerator);
                this.denominator = (float)Math.Truncate(denominator);
                this.precision = 0;
                this.precisionSet = true;
            }
            else
            {
                this.numerator = numerator;
                this.denominator = denominator;
            }

            this.numeratorSet = true;
            this.denominatorSet = true;            

            SetBaseData(null, normalRange, otherReferenceRanges);

            this.CheckInvariants();
        }

        public DvProportion(float numerator, float denominator, ProportionKind type, bool isIntegral, int precision,
         DvInterval<DvProportion> normalRange, ReferenceRange<DvProportion>[] otherReferenceRanges)
            : this()
        {
            DesignByContract.Check.Require(isIntegral || (type != ProportionKind.pkFraction &&
                   type != ProportionKind.pkIntegerFraction));
            DesignByContract.Check.Require(otherReferenceRanges == null || otherReferenceRanges.Length > 0);
            DesignByContract.Check.Require(type != ProportionKind.pkUnitary || denominator == 1);
            DesignByContract.Check.Require(type != ProportionKind.pkPercent || denominator == 100);

            this.type = type;

            if (type == ProportionKind.pkFraction || type == ProportionKind.pkIntegerFraction)
            {
                this.numerator = (float)Math.Truncate(numerator);
                this.denominator = (float)Math.Truncate(denominator);
                this.precision = 0;
                this.precisionSet = true;
            }
            else
            {
                this.numerator = numerator;
                this.denominator = denominator;
            }

            this.numeratorSet = true;
            this.denominatorSet = true;

            this.precision = precision;
            this.precisionSet = true;

            SetBaseData(null, normalRange, otherReferenceRanges);

            this.CheckInvariants();
        }

        // CM: 14/02/08
        public DvProportion(float numerator, float denominator, ProportionKind type, int precision,
            float accuracy, bool accuracyIsPercent, string magnitudeStatus, CodePhrase normalStatus,
            DvInterval<DvProportion> normalRange, ReferenceRange<DvProportion>[] otherReferenceRanges)
            : this()
        {
            DesignByContract.Check.Require(otherReferenceRanges == null || otherReferenceRanges.Length > 0);
            DesignByContract.Check.Require(type != ProportionKind.pkUnitary || denominator == 1);
            DesignByContract.Check.Require(type != ProportionKind.pkPercent || denominator == 100);

            this.type = type;

            if (type == ProportionKind.pkFraction || type == ProportionKind.pkIntegerFraction)
            {
                this.numerator = (float)Math.Truncate(numerator);
                this.denominator = (float)Math.Truncate(denominator);
                this.precision = 0;
                this.precisionSet = true;
            }
            else
            {
                this.numerator = numerator;
                this.denominator = denominator;
            }

            this.numeratorSet = true;
            this.denominatorSet = true;

            this.precision = precision;
            this.precisionSet = true;

            SetBaseData(accuracy, accuracyIsPercent, magnitudeStatus, normalStatus, normalRange, otherReferenceRanges);

            this.CheckInvariants();
        }


        public DvProportion(float numerator, float denominator, ProportionKind type,
            bool isIntegral, int precision)
            : this()
        {
            DesignByContract.Check.Require(isIntegral || (type != ProportionKind.pkFraction &&
                    type != ProportionKind.pkIntegerFraction));
            DesignByContract.Check.Require(type != ProportionKind.pkUnitary || denominator == 1);
            DesignByContract.Check.Require(type != ProportionKind.pkPercent || denominator == 100);
            DesignByContract.Check.Require(isIntegral || precision != 0);

            this.type = type;

            if (type == ProportionKind.pkFraction || type == ProportionKind.pkIntegerFraction)
            {
                this.numerator = (float)Math.Truncate(numerator);
                this.denominator = (float)Math.Truncate(denominator);                
            }
            else
            {
                this.numerator = numerator;
                this.denominator = denominator;
            }

            if (precision != -1)
            {
                this.precision = precision;
                this.precisionSet = true;
            }

            this.CheckInvariants();
        }

        #endregion

        #region class properties

        private float numerator;
        private bool numeratorSet;

        public float Numerator
        {
            get
            {
                return this.numerator;
            }
        }

        private float denominator;
        private bool denominatorSet;

        public float Denominator
        {
            get
            {
                return this.denominator;
            }
        }

        private ProportionKind type;
        private bool typeSet;

        public ProportionKind Type
        {
            get
            {
                return this.type;
            }
        }

        private int precision =-1;
        private bool precisionSet;

        public int Precision
        {
            get
            {
                return this.precision;
            }
        }
        #endregion

        #region class functions

        ///<summary>
        /// True if n is one of the defined types
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool ValidProportionKind(int n)
        {
            return Enum.IsDefined(typeof(ProportionKind), n);
        }

        public bool IsIntegral()
        {
            if (this.Precision == 0)
                return true;
            return false;
        }

        public float Magnitude()
        {
            return (float)(this.GetMagnitude());
        }

        #endregion

        protected override double GetMagnitude()
        {
            return this.Numerator / this.Denominator;
        }

        protected override DvAmount<DvProportion> Subtract(DvAmount<DvProportion> b)
        {
            DesignByContract.Check.Require(this.IsStrictlyComparableTo(b));            
            DvProportion bObj = b as DvProportion;

            double subtractResult = this.Magnitude() - bObj.Magnitude();
            return this.GetDvProportionByMagnitude(subtractResult);
            
        }

        protected override DvAmount<DvProportion> Plus(DvAmount<DvProportion> b)
        {
            DesignByContract.Check.Require(this.IsStrictlyComparableTo(b));
            DvProportion bObj = b as DvProportion;

            double result = this.Magnitude() + bObj.Magnitude();
            return this.GetDvProportionByMagnitude(result);
        }

        protected override DvAmount<DvProportion> GetDvAmountWithZeroMagnitude()
        {
            return new DvProportion(0, this.Denominator, this.Type);
        }

        public override bool IsStrictlyComparableTo(DvOrdered<DvProportion> other)
        {            
            DvProportion otherObj = other as DvProportion;
            DesignByContract.Check.Require(other != null && otherObj !=null);

            return otherObj.Type == this.Type;

        }

        protected override void CheckInvariants()
        {
            base.CheckInvariants();

            // Type_validity
            DesignByContract.Check.Invariant(ValidProportionKind((int)this.Type));

            // Is_integral_validity
            DesignByContract.Check.Invariant(!this.IsIntegral() || ((Math.Floor(this.Numerator) == this.Numerator)
                 && (Math.Floor(this.Denominator) == this.Denominator)));

            // Fraction_validity
            DesignByContract.Check.Invariant((this.Type != ProportionKind.pkFraction &&
            this.Type != ProportionKind.pkIntegerFraction) || this.IsIntegral());

            // Unitary_validity
            DesignByContract.Check.Invariant(this.Type != ProportionKind.pkUnitary ||
                this.Denominator == 1);

            // Percent_validity
            DesignByContract.Check.Invariant(this.Type != ProportionKind.pkPercent ||
                this.Denominator == 100);
        }

        public override string ToString()
        {
            if (this.Type == ProportionKind.pkRatio)
            {
                return this.Numerator.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + ":" + this.Denominator.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (this.Type == ProportionKind.pkUnitary)
            {
                return this.Magnitude().ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (this.Type == ProportionKind.pkPercent)
            {
                return this.Numerator.ToString(System.Globalization.CultureInfo.InvariantCulture) + "%";
            }
            else if (this.Type == ProportionKind.pkFraction)
            {
                // TODO: may check .NET framework whether there is any class
                // supporting this presentation.
                return this.Numerator.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/" + this.Denominator.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (this.Type == ProportionKind.pkIntegerFraction)
            {
                string result = "";

                int integerValue = 0;

                if (this.Numerator > this.Denominator)
                    integerValue = (int)(Math.Truncate(this.Numerator / this.Denominator));

                if (integerValue > 0)
                {
                    result += integerValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    // CM: 26/02/09 we don't want to change denominator, so for the situation 4/2, we need to
                    // represent it as 2 0/2 so that the original value can always be returned.
                    if (this.Numerator - integerValue * this.Denominator >= 0)
                    {
                        result += " ";

                        result += (this.Numerator - integerValue * this.Denominator).ToString(System.Globalization.CultureInfo.InvariantCulture);
                        result += "/" + this.Denominator.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                        throw new ApplicationException("Should not be less than zero.");
                }
                else
                {
                    result += this.Numerator.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    result += "/" + this.Denominator.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }

                return result;

            }
            throw new NotSupportedException();
        }

        internal DvProportion GetDvProportionByMagnitude(double magnitude)
        {
            float numerator = (float) magnitude;
            float denominator = 1;
            if (this.Type == ProportionKind.pkFraction || 
                this.Type == ProportionKind.pkIntegerFraction ||
                this.Type == ProportionKind.pkRatio)
            {
                while (numerator - Math.Truncate(numerator) > 0)
                {
                    denominator = denominator * 10;
                    numerator = numerator * 10;
                }
                return SimplifyFractions(new DvProportion(numerator, denominator, this.Type));
            }
            else if(this.Type == ProportionKind.pkPercent)
            {
                denominator = this.Denominator;
                numerator =(float)( magnitude * this.Denominator);
            }           

            return new DvProportion(numerator, denominator, this.Type);
        }

        private DvProportion SimplifyFractions(DvProportion proportion)
        { 
            DesignByContract.Check.Require(proportion!=null);
            DesignByContract.Check.Require(proportion.Type == ProportionKind.pkFraction || 
                proportion.Type == ProportionKind.pkIntegerFraction || 
                proportion.Type == ProportionKind.pkRatio);
            
            float numerator = proportion.Numerator;
            float denominator = proportion.Denominator;

            if (numerator != 0 && denominator != 0)
            {
                if (numerator == denominator)
                {
                    numerator = 1F;
                    denominator = 1F;
                }
                else
                {
                    float max = Math.Max(numerator, denominator);
                    float min = Math.Min(numerator, denominator);

                    float minSimplify = 0;
                    float maxSimplify = 0;

                    if (max % min == 0)
                    {
                        minSimplify = 1;
                        maxSimplify = max / min;
                    }
                    else
                    {
                        float remainder = max % min;
                        minSimplify = min / remainder;
                        maxSimplify = max / remainder;
                    }

                    if (denominator == max)
                    {
                        denominator = maxSimplify;
                        numerator = minSimplify;
                    }
                    else
                    {
                        denominator = minSimplify;
                        numerator = maxSimplify;
                    }
                }
            }

            return new DvProportion(numerator, denominator, proportion.Type);
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

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            Check.Assert(reader.LocalName == "numerator", "Expected LocalName should be numerator rather than "
                + reader.LocalName);
            this.numerator = reader.ReadElementContentAsFloat("numerator", RmXmlSerializer.OpenEhrNamespace);
            this.numeratorSet = true;

            reader.MoveToContent();
            Check.Assert(reader.LocalName == "denominator", "Expected LocalName should be denominator rather than "
                + reader.LocalName);
            this.denominator = reader.ReadElementContentAsFloat("denominator", RmXmlSerializer.OpenEhrNamespace);
            this.denominatorSet = true;

            reader.MoveToContent();
            Check.Assert(reader.LocalName == "type", "Expected LocalName should be type rather than "
                + reader.LocalName);
            int typeValue = reader.ReadElementContentAsInt("type", RmXmlSerializer.OpenEhrNamespace);
            if (typeValue == 0)
                this.type = ProportionKind.pkRatio;
            else if (typeValue == 1)
                this.type = ProportionKind.pkUnitary;
            else if (typeValue == 2)
                this.type = ProportionKind.pkPercent;
            else if (typeValue == 3)
                this.type = ProportionKind.pkFraction;
            else if (typeValue == 4)
                this.type = ProportionKind.pkIntegerFraction;
            else
                throw new InvalidOperationException("invalid type value for Proportion.type: " + typeValue);
            this.typeSet = true;

            reader.MoveToContent();
            if (reader.LocalName == "precision")
            {
                this.precision = reader.ReadElementContentAsInt("precision", RmXmlSerializer.OpenEhrNamespace);
                this.precisionSet = true;
            }
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            writer.WriteElementString("numerator", RmXmlSerializer.OpenEhrNamespace,
                this.Numerator.ToString(System.Globalization.CultureInfo.InvariantCulture));

            writer.WriteElementString("denominator", RmXmlSerializer.OpenEhrNamespace,
                this.Denominator.ToString(System.Globalization.CultureInfo.InvariantCulture));

            writer.WriteElementString("type", RmXmlSerializer.OpenEhrNamespace,
                ((int)(this.Type)).ToString(System.Globalization.CultureInfo.InvariantCulture));

            if (this.Precision != -1)
                writer.WriteElementString("precision", RmXmlSerializer.OpenEhrNamespace,
                    this.Precision.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("DV_PROPORTION", RmXmlSerializer.OpenEhrNamespace);

        }
    }
}
