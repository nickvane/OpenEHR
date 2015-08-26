using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.AssumedTypes;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Quantity
{    
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_ORDERED")]
    public abstract class DvOrdered<T>
        : Basic.DataValue, IComparable where T : DvOrdered<T>
    {
        #region class attributes
        DvInterval<T> normalRange;

        public DvInterval<T> NormalRange
        {
            get { return this.normalRange; }
        }

        private List<ReferenceRange<T>> otherReferenceRanges;

        public List<ReferenceRange<T>> OtherReferenceRanges
        {
            get { return this.otherReferenceRanges; }
        }

        private Text.CodePhrase normalStatus;

        [RmAttribute("normal_status")]
        [RmCodeset("normal statuses", "openehr_normal_statuses")]
        public Text.CodePhrase NormalStatus
        {
            get { return normalStatus; }
        }
        #endregion

        #region operators implementation
        public static bool operator <(DvOrdered<T> a, DvOrdered<T> b)
        {
            Check.Require(a.IsStrictlyComparableTo(b));

            return a.CompareTo(b) < 0;
        }

        public static bool operator >(DvOrdered<T> a, DvOrdered<T> b)
        {
            Check.Require(a.IsStrictlyComparableTo(b));

            return a.CompareTo(b) > 0;
        }

        public static bool operator ==(DvOrdered<T> a, DvOrdered<T> b)
        {
            if ((object)a != null)
                return a.Equals(b);

            // a is null, check if b is null
            if ((object)b != null)
                return false;
            else
                return true;
        }

        public static bool operator !=(DvOrdered<T> a, DvOrdered<T> b)
        {
            if ((object)a == null && (object)b == null)
                return false;
            else if ((object)a != null && (object)b != null)
            {
                Check.Require(a.IsStrictlyComparableTo(b));

                return a.CompareTo(b) != 0;
            }
            else
                return true;
        }

        public static bool operator <=(DvOrdered<T> a, DvOrdered<T> b)
        {
            Check.Require(a.IsStrictlyComparableTo(b));

            return a.CompareTo(b) <= 0;
        }

        public static bool operator >=(DvOrdered<T> a, DvOrdered<T> b)
        {
            Check.Require(a.IsStrictlyComparableTo(b));

            return a.CompareTo(b) >= 0;
        }

        #endregion

        #region class functions
        /// <summary>
        /// Value is in the normal range
        /// </summary>
        public bool IsNormal()
        {
            DesignByContract.Check.Require(this.NormalRange != null);

            return this.NormalRange.Has(this as T);
        }

        /// <summary>
        /// True if this quantity has no reference ranges or accuracy
        /// </summary>
        /// <returns></returns>
        public bool IsSimple()
        {
            return this.OtherReferenceRanges == null && this.NormalRange == null;
        }

        public abstract bool IsStrictlyComparableTo(DvOrdered<T> other);

        #endregion

        #region IComparable Members

        // TODO: change it to protect
        protected abstract int CompareTo(object obj);

        int IComparable.CompareTo(object obj)
        {
            return this.CompareTo(obj);
        }

        #endregion

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
                return false;

            // If parameter cannot be cast to Point return false.
            DvOrdered<T> orderedValue = obj as DvOrdered<T>;
            if ((object)orderedValue == null)
                return false;

            // Return true if the ordered value match:
            if (this.IsStrictlyComparableTo(orderedValue))
                return this.CompareTo(orderedValue) == 0;
            else
                return false;
        }

        public bool Equals(DvOrdered<T> orderedValue)
        {
            // If parameter is null return false:
            if ((object)orderedValue == null)
                return false;

            // Return true if the ordered value match:
            if (this.IsStrictlyComparableTo(orderedValue))
                return this.CompareTo(orderedValue) == 0;
            else
                return false;
        }

        protected void SetBaseData(CodePhrase normalStatus, DvInterval<T> normalRange, ReferenceRange<T>[] otherReferenceRanges)
        {
            DesignByContract.Check.Require(otherReferenceRanges == null || otherReferenceRanges.Length > 0,
                    "if otherReferenceRanges is not null, it must not be empty.");
            
            this.normalStatus = normalStatus;
            this.normalRange = normalRange;

            if (otherReferenceRanges != null)
                this.otherReferenceRanges = new List<ReferenceRange<T>>(otherReferenceRanges);
        }

        protected override void CheckInvariants()
        {
            Check.Invariant(this.OtherReferenceRanges == null ||
                     this.OtherReferenceRanges.Count > 0);

            Check.Invariant(!IsSimple() ||
             (this.NormalRange == null && this.OtherReferenceRanges == null));
        }


        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            if (reader.LocalName == "normal_range")
            {
                if (this.normalRange == null)
                    this.normalRange = new DvInterval<T>();
                this.normalRange.ReadXml(reader);
            }

            if (reader.LocalName == "other_reference_ranges")
            {
                List<ReferenceRange<T>> otherRefRanges =
                       new List<ReferenceRange<T>>();
                do
                {
                    ReferenceRange<T> refRange = new ReferenceRange<T>();
                    refRange.ReadXml(reader);
                    otherRefRanges.Add(refRange);

                } while (reader.LocalName == "other_reference_ranges");

                this.otherReferenceRanges = otherRefRanges;
            }

            if (reader.LocalName == "normal_status")
            {
                if (this.normalStatus == null)
                    this.normalStatus = new CodePhrase();

                // CM: 06/03/08
                this.normalStatus.ReadXml(reader);
            }
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            DvInterval<T> normalRange = NormalRange;
            if (this.NormalRange != null)
            {
                string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
                writer.WriteStartElement(prefix, "normal_range", RmXmlSerializer.OpenEhrNamespace);
                NormalRange.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.OtherReferenceRanges != null)
            {
                foreach (ReferenceRange<T> refRange in this.OtherReferenceRanges)
                {
                    writer.WriteStartElement("other_reference_ranges", RmXmlSerializer.OpenEhrNamespace);
                    refRange.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }

            // CM: 06/03/08
            if (this.NormalStatus != null)
            {
                writer.WriteStartElement("normal_status", RmXmlSerializer.OpenEhrNamespace);
                this.NormalStatus.WriteXml(writer);
                writer.WriteEndElement();
            }
        }
    }
}
