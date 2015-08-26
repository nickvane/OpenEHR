using System;
using OpenEhr.DesignByContract;
using System.Text.RegularExpressions;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Quantity
{
    /// <summary>
    /// Abstract class defining the concept of true quantified values, 
    /// i.e. values which are not only ordered, but which have a magnitude, 
    /// and for which the addition and difference operations can be defined.
    /// </summary>
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_QUANTIFIED")]
    public abstract class DvQuantified<T> : DvOrdered<T>
        where T: DvQuantified<T>
    {
        private string magnitudeStatus;
        private const string magnitudeStatusPattern = "^(=|>|<|>=|<=|~)$";

        public string MagnitudeStatus
        {
            get { return this.magnitudeStatus; }
        }

        public abstract bool AccuracyUnknown();

        public object Accuracy()
        {
            return GetAccuracy();
        }

        protected abstract object GetAccuracy(); 

        public static bool ValidMagnitudeStatus(string s)
        {
            Regex rg = new Regex(magnitudeStatusPattern);
            return rg.Match(s).Success;
        }

        // TODO: check with Heath, not need this
        public double Magnitude()
        {
            return this.GetMagnitude();
        }

        protected abstract double GetMagnitude();
       
        protected override int CompareTo(object obj)
        {
            DvQuantified<T> quantifiedObj = obj as DvQuantified<T>;
            Check.Require(quantifiedObj!=null && this.IsStrictlyComparableTo(quantifiedObj));


            if (this.GetMagnitude() > quantifiedObj.GetMagnitude())
                return 1;
            else if (this.GetMagnitude() < quantifiedObj.GetMagnitude())
                return -1;
            else
                return 0;
        }

        protected void SetBaseData(string magnitudeStatus, CodePhrase normalStatus, DvInterval<T> normalRange,
           ReferenceRange<T>[] otherReferenceRanges)
        {
            DesignByContract.Check.Require(magnitudeStatus == null || ValidMagnitudeStatus(magnitudeStatus),
                "if magnitudeStatus is not null or empty, it must be a valid value.");           

            base.SetBaseData(normalStatus, normalRange, otherReferenceRanges);
            this.magnitudeStatus = magnitudeStatus;
        }

        protected void CheckInvariants()
        {
            base.CheckInvariants();
            DesignByContract.Check.Invariant(this.MagnitudeStatus == null ||
                    ValidMagnitudeStatus(this.MagnitudeStatus));
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            if (reader.LocalName == "magnitude_status")
            {
                this.magnitudeStatus = reader.ReadElementString("magnitude_status", RmXmlSerializer.OpenEhrNamespace);               
            }
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer); 

            if (this.MagnitudeStatus != null)
            {
                writer.WriteElementString(prefix, "magnitude_status", RmXmlSerializer.OpenEhrNamespace,
                    this.MagnitudeStatus);
            }
        }
    }
}
