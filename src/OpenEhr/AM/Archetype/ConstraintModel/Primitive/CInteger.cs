using System;
using OpenEhr.AssumedTypes;
using OpenEhr.RM.DataTypes.Quantity;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.Archetype.ConstraintModel.Primitive
{
    /// <summary>
    /// Constraint on instances of Integer.
    /// </summary>
    [Serializable]
    [AmType("C_INTEGER")]
    public class CInteger: CPrimitive
    {
        #region Constructors
        public CInteger() { }

        public CInteger(Set<int> list, Interval<int> range)
        {
            DesignByContract.Check.Require((list == null && range == null) || (list !=null ^ range!=null),
                CommonStrings.CIntegerListAndRangeNotNull);

            this.List = list;
            this.Range = range;
        }
        #endregion

        #region Class properties

        private Set<int> list;

        /// <summary>
        /// Set of Integers specifying constraint
        /// </summary>
        public Set<int> List
        {
            get { return list; }
            set
            {
                DesignByContract.Check.Require(value == null || (value != null ^ this.Range != null),
                   CommonStrings.CIntegerListAndRangeNotNull);
                list = value;
            }
        }


        private Interval<int> range;
        /// <summary>
        /// Range of Integers specifying constraint
        /// </summary>
        public Interval<int> Range
        {
            get { return range; }
            set
            {
                DesignByContract.Check.Require(value == null || (value != null ^ this.List != null),
                    CommonStrings.CIntegerListAndRangeNotNull);
                range = value;
            }
        }

        private int defaultValue = 0;

        public override object DefaultValue
        {
            get { return defaultValue; }

        }      

        internal bool assumedValueSet;
        private int assumedValue;
        public override object AssumedValue
        {
            get
            {
                return this.assumedValue;
            }
            set
            {
                this.assumedValue = (int)value;
                this.assumedValueSet = true;
            }
        }
        #endregion

        #region functions

        public override bool HasAssumedValue()
        {
            return this.assumedValueSet;
        }

        internal override string ValidValue(object aValue)
        {
            DesignByContract.Check.Require(aValue != null, string.Format(CommonStrings.XMustNotBeNull, "aValue"));

            int intValue = int.MinValue;

            // CM: 20/04/09 when aValue is typeof ProportionKind, need to cast it to int before calling toString
            string aValueStr = aValue.ToString();
            if (aValue.GetType() == typeof(ProportionKind))
                aValueStr = ((int)aValue).ToString();

            if (int.TryParse(aValueStr, out intValue))
            {
                if (this.List != null && this.List.Count > 0)
                {
                    if (!this.list.Has(intValue))
                    {
                        return string.Format(AmValidationStrings.XNotInCIntegerList, intValue);
                    }
                }

                if (this.Range != null)
                {
                    if (!this.Range.Has(intValue))
                    {
                        return string.Format(AmValidationStrings.IntegerXOutOfRange, intValue);
                    }
                }
                return string.Empty;
            }
            return string.Format(AmValidationStrings.InvalidIntegerX, aValue);
        }


        internal override bool IsSubsetOf(CPrimitive other)
        {
            throw new NotImplementedException(
                string.Format(AmValidationStrings.IsSubsetNotImplementedInX, "CInteger"));
        }
        #endregion
    }
}
