using System;
using OpenEhr.AM.Archetype.ConstraintModel;
using OpenEhr.RM.Support.Identification;
using OpenEhr.AssumedTypes;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.OpenehrProfile.DataTypes.Text
{
    /// <summary>
    /// Express constraints on instances of CODE_PHRASE. The terminology_id attribute
    /// may be specified on its own to indicate any term from a specified terminology;
    /// the code_list attribute may be used to limit the codes to a specific list.
    /// </summary>
    [Serializable]
    [AmType("C_CODE_PHRASE")]
    public class CCodePhrase: CDomainType
    {
        #region Constructors
        public CCodePhrase() { }
        public CCodePhrase(TerminologyId terminologyId, List<string> codeList, string rmTypeName, string nodeId, 
            AssumedTypes.Interval<int> occurrences, CAttribute parent, object assumedValue)
            :base(rmTypeName, nodeId, occurrences, parent, assumedValue)
        {
            this.TerminologyId = terminologyId;
            this.CodeList = codeList;
        }
        #endregion

        #region Class properties
        private TerminologyId terminologyId;

        /// <summary>
        /// Syntax string expressing constraint on allowed primary terms
        /// </summary>
        public TerminologyId TerminologyId
        {
            get { return terminologyId; }
            set { terminologyId = value; }
        }

        private List<string> codeList;
        /// <summary>
        /// List of allowed codes; may be empty, meaning any code in the terminology may be used.
        /// </summary>
        public List<string> CodeList
        {
            get { return codeList; }
            set
            {
                DesignByContract.Check.Require(value == null || value.Count > 0,
                    string.Format(CommonStrings.IfXIsNotNullMustBeEmpty, "Codelist value"));
                codeList = value;
            }
        }

        #endregion

        #region Functions

        public override CComplexObject StandardEquivalent()
        {
            throw new Exception(string.Format(
                AmValidationStrings.StandardEquivNotImplementedInX, "CCodePhrase"));
        }

        public override object DefaultValue
        {
            get
            {
                throw new Exception(string.Format(
                    AmValidationStrings.DefaultValueNotImplementedInX, "CCodePhrase"));
            }
        }

        /// <summary>
        /// <summary>
        /// True if any CODE_PHRASE instance allowed.
        /// </summary>
        /// <returns></returns>
        /// </summary>
        /// <returns></returns>
        public override bool AnyAllowed()
        {
            return this.TerminologyId == null && this.CodeList == null;
        }


        #endregion

        #region Validation

        public override bool IsValid()
        {
            if (this.CodeList != null && this.CodeList.IsEmpty())
                return false;
            if (!(this.AnyAllowed() ^ this.TerminologyId != null))
                return false;

            return true;
        }

        public override bool ValidValue(object aValue)
        {
            DesignByContract.Check.Require(aValue != null, string.Format(
                CommonStrings.XMustNotBeNull, "aValue"));
            DesignByContract.Check.Require(this.IsValid(), string.Format(
                AmValidationStrings.ConstraintXIsValidGetsFalse, "CCodePhrase"));

            CodePhrase codePhrase = aValue as CodePhrase;
            if (codePhrase == null)
            {
                this.ValidationContext.AcceptValidationError(this, string.Format(
                    AmValidationStrings.ExpectingValueXToBeTypeY, aValue, "CodePhrase")); //cast error
                return false;
            }

            bool isValidValue = true;

            if (this.TerminologyId.Value != codePhrase.TerminologyId.Value)
            {
                isValidValue = false;
                this.ValidationContext.AcceptValidationError(this,
                    string.Format(AmValidationStrings.CodePhraseTerminologyWrong,
                        this.terminologyId.Value, codePhrase.TerminologyId.Value));

            }

            if (this.CodeList != null && !this.CodeList.Has(codePhrase.CodeString))
            {
                isValidValue = false;
                this.ValidationContext.AcceptValidationError(this,
                        string.Format(AmValidationStrings.CodeStringXNotInList, codePhrase.CodeString));
            }

            return isValidValue;
        }

        #endregion
    }
}