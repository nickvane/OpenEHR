using System;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.Archetype.ConstraintModel.Primitive
{
    /// <summary>
    /// Constraint on instances of Boolean.
    /// <use>Both attributes cannot be set to False, since this would mean that the Boolean value being constrained cannot be True or False.</use>
    /// </summary>
    [Serializable]
    [AmType("C_BOOLEAN")]
    public class CBoolean : CPrimitive
    {
        #region constructors
        public CBoolean(bool trueValid, bool falseValid)
        {
            DesignByContract.Check.Require(trueValid || falseValid, CommonStrings.EitherTrueOrFalseMustBeValid);
            this.TrueValid = trueValid;
            this.FalseValid = falseValid;
        }

        public CBoolean() { }
        #endregion

        #region class properties
        private bool trueValid;

        /// <summary>
        /// True if the value True is allowed
        /// </summary>
        public bool TrueValid
        {
            get { return trueValid; }
            set { trueValid = value; }
        }

        private bool falseValid;

        /// <summary>
        /// True if the value False is allowed
        /// </summary>
        public bool FalseValid
        {
            get { return falseValid; }
            set { falseValid = value; }
        }

        public override object DefaultValue
        {
            get
            {
                if (!FalseValid)
                    return true;

                return false;
            }

        }

        private bool assumedValue;
        internal bool assumedValueSet;
        public override object AssumedValue
        {
            get
            {
                return this.assumedValue;
            }
            set
            {
                this.assumedValue = (bool)value;
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

            bool booleanValue = false;

            if (bool.TryParse(aValue.ToString(), out booleanValue))
            {
                if ((TrueValid && booleanValue == true) || (falseValid && booleanValue == false))
                    return string.Empty;
            }
            return string.Format(AmValidationStrings.XMustBeValidY, aValue, "boolean");
        }

        internal override bool IsSubsetOf(CPrimitive other)
        {
            throw new NotImplementedException(
                string.Format(AmValidationStrings.IsSubsetNotImplementedInX, "CBoolean"));
        }

        #endregion
    }
}