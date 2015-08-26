using System;
using System.Text;
using OpenEhr.Attributes;
using OpenEhr.AM.Archetype.ConstraintModel;
using OpenEhr.RM.DataTypes.Quantity;
using OpenEhr.AssumedTypes;
using OpenEhr.Futures.OperationalTemplate;
using OpenEhr.DesignByContract;
using OpenEhr.AM.OpenehrProfile.DataTypes.Text;
using OpenEhr.RM.DataTypes.Text;
using System.Reflection;
using OpenEhr.RM.Support.Identification;
using OpenEhr.Resources;
using OpenEhr.Validation;

namespace OpenEhr.AM.OpenehrProfile.DataTypes.Quantity
{
    /// <summary>
    /// Class specifying constraints on instances of DV_ORDINAL. Custom constrainer
    /// type for instances of DV_ORDINAL.
    /// </summary>
    [Serializable]
    [AmType("C_DV_ORDINAL")]
    public class CDvOrdinal: CDomainType
    {
        #region Constructors
        public CDvOrdinal() : base() { }
        public CDvOrdinal(string rmTypeName, string nodeId, AssumedTypes.Interval<int> occurrences,
            CAttribute parent, object assumedValue, Set<DvOrdinal> list)
            : base(rmTypeName, nodeId, occurrences, parent, assumedValue)
        {
            this.List = list;
        }
        #endregion

        #region Class Properties
      
        private Set<DvOrdinal> list;
        /// <summary>
        /// Set of allowed DV_ORDINAL values
        /// </summary>
        public Set<DvOrdinal> List
        {
            get { return list; }
            set
            {
                DesignByContract.Check.Require(value == null || value.Count > 0,
                    string.Format(CommonStrings.IfXIsNotNullMustBeEmpty, "List value"));
                list = value;
            }
        }

        #endregion

        #region Functions
        public override CComplexObject StandardEquivalent()
        {
            throw new Exception(string.Format(
                AmValidationStrings.StandardEquivNotImplementedInX, "CDvOrdinal"));
        }

        public override object DefaultValue
        {
            get
            {
                throw new Exception(string.Format(
                    AmValidationStrings.DefaultValueNotImplementedInX, "CDvOrdinal"));
            }
        }

        public override bool ValidValue(object aValue)
        {
            Check.Require(aValue != null, string.Format(CommonStrings.XMustNotBeNull, "aValue"));
           
            if (this.AnyAllowed())
                return true;

            bool IsValidValue = true;
            
            DvOrdinal aValueDvOrdinal = aValue as DvOrdinal;

            if (aValueDvOrdinal != null)
            {
                bool foundInList = false;

                foreach (DvOrdinal dvOrdinal in this.List)
                {

                    if (dvOrdinal == aValueDvOrdinal)
                    {
                        if (string.IsNullOrEmpty(aValueDvOrdinal.Symbol.DefiningCode.CodeString)
                            || string.IsNullOrEmpty(aValueDvOrdinal.Symbol.DefiningCode.TerminologyId.Value))
                        { 
                            aValueDvOrdinal.Symbol.DefiningCode.CodeString = 
                                dvOrdinal.Symbol.DefiningCode.CodeString;

                             aValueDvOrdinal.Symbol.DefiningCode.TerminologyId =
                                    new TerminologyId(dvOrdinal.Symbol.DefiningCode.TerminologyId.Value);
                        }

                        foundInList = true;
                        break;
                    }
                }

                if (!foundInList)
                {
                    IsValidValue = false;
                    this.ValidationContext.AcceptValidationError(this,
                        string.Format(AmValidationStrings.XNotInCDvOrdinalList, aValueDvOrdinal));

                }
                else
                {
                    if (!ValidationUtility.ValidValueTermDef(aValueDvOrdinal.Symbol, this.Parent, ValidationContext.TerminologyService))
                    {
                        IsValidValue = false;
                        this.ValidationContext.AcceptValidationError(this, string.Format(
                            AmValidationStrings.DvOrdinalSymbolXIncorrectForCodeY,
                            aValueDvOrdinal.Symbol.Value, aValueDvOrdinal.Symbol.DefiningCode.CodeString));
                    }
                }
            }
            else
            {
                IsValidValue = false;
                this.ValidationContext.AcceptValidationError(this,
                    string.Format(AmValidationStrings.InvalidDvOrdinalX, aValue));
            }

            return IsValidValue;
        }

 
        public override bool AnyAllowed()
        {
            return this.List == null;
        }

        public override bool IsValid()
        {
            if (this.List != null && this.List.IsEmpty())
                return false;

            if(!(this.AnyAllowed() ^ this.List != null))
                return false;

            return true;
        }

        #endregion
    }
}
