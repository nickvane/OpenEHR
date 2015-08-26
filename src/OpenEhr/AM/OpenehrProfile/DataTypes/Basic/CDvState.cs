using System;
using OpenEhr.AM.Archetype.ConstraintModel;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.OpenehrProfile.DataTypes.Basic
{
    /// <summary>
    /// Constrainer type for DV_STATE instances. The attribute c_value defines a
    /// state/event table which constrains the allowed values of the attribute value in a
    /// DV_STATE instance, as well as the order of transitions between values.
    /// </summary>
    [Serializable]
    [AmType("C_DV_STATE")]
    public class CDvState: CDomainType
    {
        #region Constructors
        public CDvState():base() { }

        public CDvState(string rmTypeName, string nodeId, AssumedTypes.Interval<int> occurrences,
            CAttribute parent, object assumedValue, StateMachine value)
            :
            base(rmTypeName, nodeId, occurrences, parent, assumedValue)
        {
            this.Value = value;
        }
        #endregion

        #region Class properties
        private StateMachine value;

        public StateMachine Value
        {
            get { return value; }
            set
            {
                DesignByContract.Check.Require(value != null, string.Format(
                    CommonStrings.XMustNotBeNull, "CDvState.Value value"));
                this.value = value;
            }
        }

        #endregion

        #region Implemented abstract functions
        public override CComplexObject StandardEquivalent()
        {
            throw new Exception(string.Format(
                AmValidationStrings.StandardEquivNotImplementedInX, "CDvState"));
        }

        private CDvState defaultValue;
        public override object DefaultValue
        {
            get { return this.defaultValue; }
        }

        public override bool ValidValue(object aValue)
        {
            Check.Require(aValue != null, string.Format(CommonStrings.XMustNotBeNull, "aValue"));

            State aValueState = aValue as State;

            if (aValueState != null && this.Value.States.Has(aValueState))
                return true;

            this.ValidationContext.AcceptValidationError(this,
                string.Format(AmValidationStrings.InvalidDvStateX, aValueState));

            return false;

        }

        public override bool AnyAllowed()
        {
            return false;
        }

        public override bool IsValid()
        {
            if (this.Value == null)
                return false;

            return true;
        }

        #endregion
    }
}