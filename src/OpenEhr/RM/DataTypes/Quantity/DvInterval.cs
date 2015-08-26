using System;

using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.Factories;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.DataTypes.Quantity
{
    /// <summary>
    /// Generic class defining an interval (i.e. range) of a comparable type.
    /// An interval is a contiguous subrange of a comparable base type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    //public class DvInterval<T> : Basic.DataValue, Support.Assumed.Interval<DvOrdered>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_INTERVAL")]
    public class DvInterval<T> 
        : Basic.DataValue, System.Xml.Serialization.IXmlSerializable, AssumedTypes.IInterval<T>
        where T : DvOrdered<T>
    {
        #region Constructors

        internal DvInterval()
        { }

        public DvInterval(T lower, bool lowerIncluded, bool lowerUnbounded,
            T upper, bool upperIncluded, bool upperUnbounded)
            : this()
        {
            // lower/upper boundary value included in range if not lower_unbounded/upper_unbounded
            DesignByContract.Check.Require(!lowerIncluded || !lowerUnbounded);
            DesignByContract.Check.Require(!lowerIncluded || lower != null);
            DesignByContract.Check.Require(!upperIncluded || !upperUnbounded);
            DesignByContract.Check.Require(!upperIncluded || upper != null);
            DesignByContract.Check.Require(lower == null || !lowerUnbounded);
            DesignByContract.Check.Require(upper == null || !upperUnbounded);
            DesignByContract.Check.Require(lower == null || upper == null || (lower < upper || lower == upper));
            DesignByContract.Check.Require(lower == null || upper == null || lower.IsStrictlyComparableTo(upper));

            SetLower(lower, lowerIncluded);
            this.SetUpper(upper, upperIncluded);

            this.SetLowerUnbounded(lowerUnbounded);

            this.SetUpperUnbounded(upperUnbounded);

            CheckInvariants();
        }

        public DvInterval(T lower, bool lowerIncluded,
            T upper, bool upperIncluded)
            : this()
        {
            if (lower != null)
            {
                SetLower(lower, lowerIncluded);
            }

            if (upper != null)
            {
                SetUpper(upper, upperIncluded);
            }
           
            CheckInvariants();
        }       

        public DvInterval(T value, bool included, bool lowerUnbounded, 
            bool upperUnbounded)
            : this()
        {
            DesignByContract.Check.Require(value!=null);
            DesignByContract.Check.Require(!lowerUnbounded || !upperUnbounded);

            // CM: 11/02/08
            if (!lowerUnbounded)
            {
                this.SetLower(value, included);
            }
            if (!upperUnbounded)
            {
                this.SetUpper(value, included);
            }

            this.lowerUnbounded = lowerUnbounded;
            this.lowerUnboundedSet = true;
            this.upperUnbounded = upperUnbounded;
            this.upperUnboundedSet = true;

            CheckInvariants();
        }

        #endregion

        #region Interval<DvOrdered> Members

        private T lower;

        public T Lower
        {
            get
            {
                return this.lower;
            }
          
        }

        private void SetLower(T value, bool included)
        {
            this.lower = value as T;

            if (lower != null)
            {
                this.lowerIncluded = included;
                this.lowerIncludedSet = true;
            }
        }

        private T upper;

        public T Upper
        {
            get
            {
                return this.upper;
            }
           
        }

        private void SetUpper(T value, bool included)
        {
            this.upper = value as T;

            if (upper != null)
            {
                this.upperIncluded = included;
                this.upperIncludedSet = true;
            }
        }

        private bool lowerUnbounded;
        internal bool lowerUnboundedSet;

        /// <summary>
        /// lower boundary open (i.e. = -infinity)
        /// </summary>
        public bool LowerUnbounded
        {
            get
            {
                return this.lowerUnbounded;
            }
        }

        private void SetLowerUnbounded(bool unbounded)
        {
            this.lowerUnbounded = unbounded;
            this.lowerUnboundedSet = true;
        }

        private bool upperUnbounded;
        internal bool upperUnboundedSet;

        public bool UpperUnbounded
        {
            get
            {
                return this.upperUnbounded;
            }
        }

        private void SetUpperUnbounded(bool unbounded)
        {
            this.upperUnbounded = unbounded;
            this.upperUnboundedSet = true;
        }

        private bool lowerIncluded;
        internal bool lowerIncludedSet;

        public bool LowerIncluded
        {
            get
            {
                return this.lowerIncluded;
            }
        }

        private bool upperIncluded;
        internal bool upperIncludedSet;

        public bool UpperIncluded
        {
            get
            {
                return this.upperIncluded;
            }
            
        }

        /// <summary>
        /// True if e is within the Lower and Upper range
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool Has(T e)
        {
            DesignByContract.Check.Require(e!=null);

            if (this.LowerUnbounded && this.UpperUnbounded)
                return true;

            if (!this.LowerUnbounded)
            {
                if (this.LowerIncluded)
                {
                    if (e < Lower)
                        return false;
                }
                else
                {
                    if (e < Lower || e == Lower)
                        return false;
                }
            }
            if (!this.UpperUnbounded)
            {
                if (this.UpperIncluded)
                {
                    if (e > this.Upper)
                        return false;
                }
                else
                {
                    if (e > this.Upper || e == this.Upper)
                        return false;
                }
            }

            return true;
        }

        protected override void CheckInvariants()
        {
            // %HYYKA%
            ////if (this.LowerUnbounded)
            //    DesignByContract.Check.Invariant(!this.LowerIncluded);
            DesignByContract.Check.Invariant(!this.LowerUnbounded || !this.LowerIncluded);

            //if (this.UpperUnbounded)
            //    DesignByContract.Check.Invariant(!this.UpperIncluded);
            DesignByContract.Check.Invariant(!this.UpperUnbounded || !this.UpperIncluded);

            //if (!this.LowerUnbounded && !this.UpperUnbounded)
            //{
            //    DesignByContract.Check.Invariant(this.Upper > this.Lower ||
            //        this.Upper == this.Lower);
            //    DesignByContract.Check.Invariant(this.Lower.IsStrictlyComparableTo(this.Upper));
            //}
            DesignByContract.Check.Invariant(this.LowerUnbounded || this.UpperUnbounded 
                ||  (this.Upper > this.Lower || this.Upper == this.Lower));
            DesignByContract.Check.Invariant(this.LowerUnbounded || this.UpperUnbounded 
                || this.Lower.IsStrictlyComparableTo(this.Upper));
        }
        #endregion

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            if (reader.LocalName == "lower")
            {
                string type = RmXmlSerializer.ReadXsiType(reader);
                this.lower = RmFactory.DataValue(type) as T;
                Check.Assert(this.lower != null, "lower must not be null");

                this.lower.ReadXml(reader);

                reader.MoveToContent();
            }

            if (reader.LocalName == "upper")
            {
                string type = RmXmlSerializer.ReadXsiType(reader);
                this.upper = RmFactory.DataValue(type) as T;
                Check.Assert(this.upper != null, "lower must not be null");

                this.upper.ReadXml(reader);

                reader.MoveToContent();
            }

            if (reader.LocalName == "lower_included")
            {
                this.lowerIncluded = reader.ReadElementContentAsBoolean();
                this.lowerIncludedSet = true;
                reader.MoveToContent();
            }

            if (reader.LocalName == "upper_included")
            {
                this.upperIncluded = reader.ReadElementContentAsBoolean();
                this.upperIncludedSet = true;
                reader.MoveToContent();
            }

            Check.Assert(reader.LocalName == "lower_unbounded", "localName must be 'lower_unbounded'");
            this.lowerUnbounded = reader.ReadElementContentAsBoolean();
            this.lowerUnboundedSet = true;
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "upper_unbounded", "localName must be 'upper_unbounded'");
            this.upperUnbounded = reader.ReadElementContentAsBoolean();
            this.upperUnboundedSet = true;
            reader.MoveToContent();

            CheckInvariants();
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            this.CheckInvariants();

            string openEhrNamespace = RmXmlSerializer.OpenEhrNamespace;
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiType = RmXmlSerializer.UseXsiPrefix(writer);

            if (this.Lower != null)
            {
                writer.WriteStartElement("lower", openEhrNamespace);

                string type = ((IRmType)this.Lower).GetRmTypeName();
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    type = openEhrPrefix + ":" + type;

                writer.WriteAttributeString(xsiType, "type", RmXmlSerializer.XsiNamespace, type);
                this.Lower.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.Upper != null)
            {
                writer.WriteStartElement("upper", openEhrNamespace);

                string type = ((IRmType)this.Upper).GetRmTypeName();
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    type = openEhrPrefix + ":" + type;
                writer.WriteAttributeString(xsiType, "type", RmXmlSerializer.XsiNamespace, type);

                this.Upper.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.Lower != null && this.lowerIncludedSet)
                writer.WriteElementString(openEhrPrefix, "lower_included", openEhrNamespace, 
                    this.LowerIncluded.ToString().ToLower());

            if (this.Upper != null && this.upperIncludedSet)
                writer.WriteElementString(openEhrPrefix, "upper_included", openEhrNamespace, 
                    this.UpperIncluded.ToString().ToLower());

            writer.WriteElementString(openEhrPrefix, "lower_unbounded", openEhrNamespace, 
                this.LowerUnbounded.ToString().ToLower());

            writer.WriteElementString(openEhrPrefix, "upper_unbounded", openEhrNamespace, 
                this.UpperUnbounded.ToString().ToLower());
        }      

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("DV_INTERVAL", RmXmlSerializer.OpenEhrNamespace);
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
