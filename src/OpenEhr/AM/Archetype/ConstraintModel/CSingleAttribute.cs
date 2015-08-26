using System;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.Archetype.ConstraintModel
{
    /// <summary>
    /// Concrete model of constraint on a single-valued attribute node. 
    /// The meaning of the inherited children attribute is that they are alternatives.
    /// </summary>
    [Serializable]
    [AmType("C_SINGLE_ATTRIBUTE")]
    public class CSingleAttribute : CAttribute
    {
        #region Constructors
        public CSingleAttribute(string rmAttributeName, AssumedTypes.Interval<int> existence,
            AssumedTypes.List<CObject> children): base(rmAttributeName, existence, children)
         {}

        public CSingleAttribute() { }
        #endregion

        #region Class properties
        private AssumedTypes.List<CObject> alternatives;
        /// <summary>
        /// List of alternative constraints for the single child of this attribute within the data.
        /// </summary>
        public AssumedTypes.List<CObject> Alternatives
        {
            get
            {
                if (this.alternatives == null)
                {
                    this.alternatives = this.Children;
                }
                return this.alternatives;
            }
        }

        #endregion

        #region Functions
        public override bool IsSubsetOf(ArchetypeConstraint other)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool IsValid()
        {
            if (this.Children != null)
                foreach (CObject cObject in this.Children)
                    if (cObject.Occurrences.Upper > 1)
                        return false;

            return base.IsValid();
        }
    
        #endregion


        #region Validation

        public override bool ValidValue(object dataValue)
        {
            Check.Require(dataValue != null, string.Format(CommonStrings.XMustNotBeNull, "dataValue"));

            int count = Children == null ? 0 : Children.Count;
            bool result = count == 0;

            bool wereErrorsSuppressed = ValidationContext.IsSuppressingAcceptErrors;

            try
            {
                ValidationContext.IsSuppressingAcceptErrors = count > 1;

                for (int i = 0; !result && i < count; i++)
                    result = Children[i].ValidValue(dataValue);
            }
            finally
            {
                ValidationContext.IsSuppressingAcceptErrors = wereErrorsSuppressed;
            }

            if (!result && count > 1)
                ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.NotAllowedByAttributeXConstraint, RmAttributeName));

            return result;
        }

        #endregion //Validation
    }
}