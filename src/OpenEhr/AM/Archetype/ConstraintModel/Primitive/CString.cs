using System;
using OpenEhr.DesignByContract;
using OpenEhr.AssumedTypes;
using System.Text.RegularExpressions;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.Archetype.ConstraintModel.Primitive
{
    /// <summary>
    /// Constraint on instances of STRING.
    /// </summary>
    [Serializable]
    [AmType("C_STRING")]
    public class CString: CPrimitive
    {
        #region Constructors
        public CString(string pattern, Set<string> list, string assumedValue)
        {
            DesignByContract.Check.Require((pattern!=null ^ list!=null) || (pattern == null && list == null),
                CommonStrings.CStringListAndPatternNotNull);

            this.Pattern = pattern;
            this.List = list;
            this.AssumedValue = assumedValue;
        }

        public CString() { }
        #endregion

        #region class properties
        private string pattern;

        /// <summary>
        /// Regular expression pattern for proposed instances of String to match.
        /// </summary>
        public string Pattern
        {
            get { return pattern; }
            set
            {
                Check.Require(value == null || value != string.Empty,
                    string.Format(CommonStrings.IfXIsNotNullMustBeEmpty, "CString.Pattern"));
                Check.Require(value == null || (value != null ^ this.List != null),
                    string.Format(CommonStrings.IfXIsNotNullMustBeEmpty, "CString.List"));

                pattern = value;
            }
        }

        private Set<string> list;

        /// <summary>
        /// Set of Strings specifying constraint
        /// </summary>
        public Set<string> List
        {
            get { return list; }
            set
            {
                Check.Require(value == null || (value != null ^ this.Pattern != null),
                    CommonStrings.CStringListAndPatternNotNull);
                list = value;
            }
        }

        internal bool listOpenSet;
        private bool listOpen;
        /// <summary>
        /// True if the list is being used to specify the constraint but is not considered exhaustive.
        /// </summary>
        public bool ListOpen
        {
            get { return listOpen; }
            set
            {
                listOpen = value;
                listOpenSet = true;
            }
        }

        private string defaultValue = string.Empty;

        public override object DefaultValue
        {
            get { return this.defaultValue ;}

        }       

        private string assumedValue;
        public override object AssumedValue
        {
            get
            {
                return this.assumedValue;
            }
            set
            {
               this.assumedValue = (string)value;
            }
        }

        #endregion

        #region Functions
        public override bool HasAssumedValue()
        {
            return this.AssumedValue != null;
        }

        internal override string ValidValue(object aValue)
        {

            DesignByContract.Check.Require(aValue != null, string.Format(CommonStrings.XMustNotBeNull, "aValue"));

            string stringValue = aValue as string;

            if (stringValue != null)
            {

                if (!string.IsNullOrEmpty(this.pattern))
                {
                    if (!Regex.IsMatch(stringValue, this.pattern, RegexOptions.Compiled | RegexOptions.Singleline))
                    {
                        return string.Format(AmValidationStrings.StringXDoesNotMatchPatternY,
                            stringValue, pattern);
                    }
                }

                if (this.list != null && this.list.Count > 0)
                {
                    if (!this.list.Has(stringValue))
                    {
                        return string.Format(AmValidationStrings.XNotInCStringList, stringValue);
                    }
                }

                return string.Empty;
            }
            return string.Format(AmValidationStrings.InvalidStringX, aValue);
        }

        internal override bool IsSubsetOf(CPrimitive other)
        {
            throw new NotImplementedException(
                string.Format(AmValidationStrings.IsSubsetNotImplementedInX, "CString"));
        }
        #endregion
    }
}