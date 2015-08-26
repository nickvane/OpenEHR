using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Quantity
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_COUNT")]
    public class DvCount : DvAmount<DvCount>, System.Xml.Serialization.IXmlSerializable
    {
        public DvCount()
        { }

        public DvCount(long magnitude, float accuracy, bool accuracyIsPercent, string magnitudeStatus,
            CodePhrase normalStatus, DvInterval<DvCount> normalRange, ReferenceRange<DvCount>[] otherReferenceRanges)
            : this()
        {
            this.magnitude = magnitude;
            this.magnitudeSet = true;           

            SetBaseData(accuracy, accuracyIsPercent, magnitudeStatus, normalStatus, normalRange, otherReferenceRanges);

            CheckInvariants();
        }

        public DvCount(long magnitude)
            : this(magnitude, -1, false, null, null, null, null)
        { }
       
        public DvCount(long magnitude, DvInterval<DvCount> normalRange)
            : this(magnitude, -1, false, null, null, normalRange, null)
        { }

        public DvCount(long magnitude, DvInterval<DvCount> normalRange,
            float accuracy, bool accuracyIsPercent, string magnitudeStatus)
            : this(magnitude, accuracy, accuracyIsPercent, magnitudeStatus, null, normalRange, null)
        { }

        public DvCount(long magnitude, DvInterval<DvCount> normalRange,
            ReferenceRange<DvCount>[] referenceRanges,
            float accuracy, bool accuracyIsPercent, string magnitudeStatus)
            : this(magnitude, accuracy, accuracyIsPercent, magnitudeStatus, null, normalRange, referenceRanges)
        { }

        private long magnitude;
        private bool magnitudeSet;

        public long Magnitude
        {
            get
            {
                return this.magnitude;
            }
        }

        protected override double GetMagnitude()
        {
            return this.magnitude;
        }

        public override string ToString()
        {
            return this.Magnitude.ToString();
        }

        public override bool IsStrictlyComparableTo(DvOrdered<DvCount> other)
        {
            DesignByContract.Check.Require(other!=null && other is DvCount);
            
            return true;
        }

        protected void CheckVariants()
        {
            base.CheckInvariants();
        }

        protected override DvAmount<DvCount> Plus(DvAmount<DvCount> b)
        {
            DesignByContract.Check.Require(this.IsStrictlyComparableTo(b));
            
            DvCount bObj = b as DvCount;            

            return new DvCount(this.Magnitude + bObj.Magnitude);
        }

        protected override DvAmount<DvCount> Subtract(DvAmount<DvCount> b)
        {
            DesignByContract.Check.Require(this.IsStrictlyComparableTo(b));

            DvCount bObj = b as DvCount;

            return new DvCount(this.Magnitude - bObj.Magnitude);
        }

        protected override DvAmount<DvCount> GetDvAmountWithZeroMagnitude()
        {
            return new DvCount(0); 
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
            return new System.Xml.XmlQualifiedName("DV_COUNT", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {            
            base.ReadXmlBase(reader);
            
            Check.Assert(reader.LocalName == "magnitude", "reader.LocalName must be 'magnitude'");
            this.magnitude = reader.ReadElementContentAsLong("magnitude", RmXmlSerializer.OpenEhrNamespace);
            this.magnitudeSet = true;

            reader.MoveToContent();
        }
        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            this.CheckInvariants();
            base.WriteXmlBase(writer);
            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer); 

            writer.WriteElementString(prefix, "magnitude", RmXmlSerializer.OpenEhrNamespace, this.Magnitude.ToString());

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
