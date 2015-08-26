using System;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Quantity
{
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_AMOUNT")]
    public abstract class DvAmount<T> : DvQuantified<T>, IFormattable
        where T: DvAmount<T>
    {
        const float unknownAccuracyValue = -1.0F;
        
        // CM: 13/01/10 EHR-739 openEHR v1.0.2 accuracy is -1 when it is not set
        private float accuracy = -1.0F;

        public float Accuracy
        {
            get
            {
                if (this.accuracy != unknownAccuracyValue)
                    return this.accuracy;
                return this.accuracy;
            }

        }

        private bool accuracyIsPercent;
        private bool accuracyIsPercentSet;

        public bool AccuracyIsPercent
        {
            get
            {
                return this.accuracyIsPercent;
            }
           
        }

        public static DvAmount<T> operator -(DvAmount<T> a, DvAmount<T> b)
        {
           return a.Subtract(b);
        }

        protected abstract DvAmount<T> Subtract(DvAmount<T> b);
        protected abstract DvAmount<T> Plus(DvAmount<T> b);

        public static DvAmount<T> operator +(DvAmount<T> a, DvAmount<T> b)
        {
            return a.Plus(b);
        }

        // CM: 21/05/07
        // prefix "-": like Current
        public static DvAmount<T> operator -(DvAmount<T> b)
        {
            return b.GetDvAmountWithZeroMagnitude() - b;           
        }

        public override bool AccuracyUnknown()
        {
            return this.Accuracy == unknownAccuracyValue;
        }

        protected override object GetAccuracy()
        {
            return this.Accuracy;
        }

        public bool ValidPercentage()
        {
            return this.Accuracy >= 0.0 && this.Accuracy <= 100.0;
        }

        protected abstract DvAmount<T> GetDvAmountWithZeroMagnitude();

        protected void SetBaseData(float accuracy, bool accuracyIsPercent, string magnitudeStatus, CodePhrase normalStatus,
            DvInterval<T> normalRange, ReferenceRange<T>[] otherReferenceRanges)
        {
            DesignByContract.Check.Require(accuracy >= -1, "accuracy must be greater or equal to -1");           

            base.SetBaseData(magnitudeStatus, normalStatus, normalRange, otherReferenceRanges);

            // means accuracy should be set
            if (accuracy != unknownAccuracyValue) 
            {
                this.accuracy = accuracy;
                this.accuracyIsPercent = accuracyIsPercent;
                this.accuracyIsPercentSet = true;
            }
        }

        protected void SetBaseData(string magnitudeStatus, CodePhrase normalStatus, DvInterval<T> normalRange,
          ReferenceRange<T>[] otherReferenceRanges)
        {
            base.SetBaseData(magnitudeStatus, normalStatus, normalRange, otherReferenceRanges);
           
        }

        protected virtual void CheckInvariants()
        {
            DesignByContract.Check.Invariant(this.Accuracy != 0 || !AccuracyIsPercent);
            DesignByContract.Check.Invariant(!AccuracyIsPercent || ValidPercentage());
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            if (reader.LocalName == "accuracy")
            {
                // only set accuracy when the value is greater than -1
                float accuracyValue = reader.ReadElementContentAsFloat("accuracy", RmXmlSerializer.OpenEhrNamespace);
                if (accuracyValue < unknownAccuracyValue)
                    throw new ApplicationException("accuracy must be greater or equal to -1");
               
                this.accuracy = accuracyValue;
            }
            if (reader.LocalName == "accuracy_is_percent")
            {
                this.accuracyIsPercent = reader.ReadElementContentAsBoolean("accuracy_is_percent",
                    RmXmlSerializer.OpenEhrNamespace);
                this.accuracyIsPercentSet = true;
            }

            if (!reader.IsStartElement())
            {
                reader.ReadEndElement();
                reader.MoveToContent();
            }
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);
            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer); 

            if(!this.AccuracyUnknown())
            {
                writer.WriteElementString(prefix, "accuracy", RmXmlSerializer.OpenEhrNamespace, this.Accuracy.ToString());
                writer.WriteElementString(prefix, "accuracy_is_percent", RmXmlSerializer.OpenEhrNamespace,
                        this.AccuracyIsPercent.ToString().ToLower());
            }
           
        }

        #region IFormattable Members

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return this.ToString();
        }

        #endregion
    }
}
