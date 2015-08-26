using System;
using OpenEhr.AssumedTypes;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.Archetype.ConstraintModel.Primitive
{
    /// <summary>
    /// Constraint on instances of Real.
    /// </summary>
    [Serializable]
    [AmType("C_REAL")]
    public class CReal: CPrimitive
    {
        #region Constructors
        public CReal(Set<float> list, Interval<float> range)
        {
            DesignByContract.Check.Require((list == null && range == null) || (list !=null ^ range!=null),
                CommonStrings.CRealListAndRangeNotNull);

            this.List = list;
            this.Range = range;
        }

        public CReal() { }
        #endregion

        #region Class properties
        private Set<float> list;

        /// <summary>
        /// Set of Reals specifying constraint
        /// </summary>
        public Set<float> List
        {
            get { return list; }
            set
            {
                DesignByContract.Check.Require(value == null || (value != null ^ this.Range != null),
                   CommonStrings.CRealListAndRangeNotNull);
                list = value;
            }
        }


        private Interval<float> range;
        /// <summary>
        /// Range of real specifying constraint
        /// </summary>
        public Interval<float> Range
        {
            get { return range; }
            set
            {
                DesignByContract.Check.Require(value == null || (value != null ^ this.List != null),
                    CommonStrings.CRealListAndRangeNotNull);
                range = value;
            }
        }

        private float defaultValue = 0F;

        public override object DefaultValue
        {
            get { return this.defaultValue ; }


        }       

        private float assumedValue;
        internal bool assumedValueSet;
        public override object AssumedValue
        {
            get
            {
                DesignByContract.Check.Require(this.assumedValueSet, AmValidationStrings.CRealAssumedValueSetFalse);

                return this.assumedValue;
            }
            set
            {
                this.assumedValue = (float)value;
                this.assumedValueSet = true;
            }
        }
        #endregion

        #region Functions

        public override bool HasAssumedValue()
        {
            return assumedValueSet;
        }

        internal override string ValidValue(object aValue)
        {
            DesignByContract.Check.Require(aValue != null, string.Format(CommonStrings.XMustNotBeNull, "aValue"));
            DesignByContract.Check.Require(this.List == null ^ this.Range == null, CommonStrings.CRealListAndRangeNotNull);
            
            float floatValue = 0.0F;

            if (float.TryParse(aValue.ToString(), out floatValue))
            {
                if (this.List != null && this.List.Count > 0)
                {
                    if (!this.List.Has(floatValue))
                    {
                        return string.Format(AmValidationStrings.XNotInCRealList, floatValue);
                    }
                }

                if (this.Range != null)
                {
                    if (!this.Range.Has(floatValue))
                    {
                        return string.Format(AmValidationStrings.RealXOutOfRange, floatValue);
                    }
                }

                return string.Empty;
            }

            return string.Format(AmValidationStrings.InvalidRealX, aValue);
        }

        internal override bool IsSubsetOf(CPrimitive other)
        {
            throw new NotImplementedException(
                string.Format(AmValidationStrings.IsSubsetNotImplementedInX, "CReal"));
        }

        #endregion
    }
}