using System;
using OpenEhr.AssumedTypes;
using System.Text.RegularExpressions;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.Archetype.ConstraintModel.Primitive
{
    /// <summary>
    /// ISO 8601-compatible constraint on instances of Date in the form either of a set of
    /// validity values, or an actual date range. There is no validity flag for ‘year’, since it
    /// must always be by definition mandatory in order to have a sensible date at all.
    /// Syntax expressions of instances of this class include “YYYY-??-??” (date with
    /// optional month and day).
    /// </summary>
    /// <use>Date ranges are probably only useful for historical dates</use>
    [Serializable]
    [AmType("C_DATE")]
    public class CDate: CPrimitive
    {
        #region constructors
        public CDate() { }

        #endregion
        internal const string dayPatternPattern = @"[yY][yY][yY][yY]-(?<month>[mM?X][mM?X])-(?<day>[dD?X][dD?X])";

        #region class properties
        private string pattern;
        internal String Pattern
        {
            get
            {
                if (string.IsNullOrEmpty(this.pattern))
                    this.pattern = GetPattern();

                return this.pattern;
            }
            set
            {
                DesignByContract.Check.Require(!string.IsNullOrEmpty(value),
                    string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Pattern value"));
                DesignByContract.Check.Ensure(Regex.IsMatch(value, dayPatternPattern, RegexOptions.Compiled | RegexOptions.Singleline),
                    string.Format(CommonStrings.DatePatternInvalid, value));
                DesignByContract.Check.Require(this.pattern == null,
                   CommonStrings.PatternMustBeNullBeforeSet);
                this.pattern = value;                
            }
        }

        private string GetPattern()
        {
            if (this.MonthValidity == null && this.DayValidity == null)
                return null;

            string datePattern = "yyyy";
            if (this.MonthValidity != null)
            {
                switch (this.MonthValidity.Value)
                {
                    case 1001:
                        datePattern += "-mm";
                        break;
                    case 1002:
                        datePattern += "-??";
                        break;
                    default:
                        throw new NotSupportedException(string.Format(
                            CommonStrings.InvalidMonthValidityX, this.MonthValidity.Value));
                }
            }
            else
                datePattern += "-??";

            if (this.DayValidity != null)
            {

                switch (this.DayValidity.Value)
                {
                    case 1001:
                        datePattern += "-dd";
                        break;
                    case 1002:
                    case 1003:
                        datePattern += "-??";
                        break;
                    default:
                        throw new NotSupportedException(string.Format(
                            CommonStrings.InvalidDayValidityX, this.DayValidity.Value)); 
                }
            }
            else
                datePattern += "-??";

            DesignByContract.Check.Ensure(Regex.IsMatch(datePattern, dayPatternPattern, RegexOptions.Compiled | RegexOptions.Singleline), "date pattern must be valid pattern");

            return datePattern;
        }

        private void SetClassProperty()
        {
            if (!string.IsNullOrEmpty(this.Pattern))
            {
                Match match = Regex.Match(this.Pattern, dayPatternPattern, RegexOptions.Compiled | RegexOptions.Singleline);
                GroupCollection groups = match.Groups;

                if (groups == null)
                    throw new ApplicationException(CommonStrings.RegexGroupsMustNotBeNull);

                this.MonthValidity = CDateTime.ToValidityKind(groups["month"].Value);
                this.DayValidity = CDateTime.ToValidityKind(groups["day"].Value);
            }
        }

        private ValidityKind monthValidity;

        /// <summary>
        /// Validity of month in constrained date.
        /// </summary>
        public ValidityKind MonthValidity
        {
            get
            {
                if (this.monthValidity == null && this.pattern !=null)
                    this.SetClassProperty();

                return monthValidity;
            }
            set { monthValidity = value; }
        }

        private ValidityKind dayValidity;

        /// <summary>
        /// Validity of day in constrained date.
        /// </summary>
        public ValidityKind DayValidity
        {
            get
            {
                if (this.dayValidity == null && this.pattern != null)
                    this.SetClassProperty();

                return dayValidity;
            }
            set { dayValidity = value; }
        }

        private ValidityKind timezoneValidity;

        /// <summary>
        /// Validity of timezone in constrained date.
        /// </summary>
        public ValidityKind TimezoneValidity
        {
            get { return timezoneValidity; }
            set { timezoneValidity = value; }
        }

        private Interval<Iso8601Date> range;
        /// <summary>
        /// Interval of Dates specifying constraint
        /// </summary>
        public Interval<Iso8601Date> Range
        {
            get { return range; }
            set { range = value; }
        }
        #endregion

        #region functions

        /// <summary>
        /// True if validity is in the form of a range; useful for developers to check which kind
        /// of constraint has been set.
        /// </summary>
        /// <returns></returns>
        public bool ValidityIsRange()
        {
            return this.Range != null;
        }

        private string defaultValue;

        /// <summary>
        /// Default Date value as ISO8601 Date string
        /// </summary>
        public override object DefaultValue
        {
            get 
            {
                if (defaultValue == null)
                {
                    if (Range != null && Range.Lower != null)
                        defaultValue = Range.Lower.ToString();
                    else
                        return new DvDate().ToString();
                }
                return this.defaultValue;
            }
        }

        public override bool HasAssumedValue()
        {
            return this.assumedValue != null;
        }

        private Iso8601Date assumedValue;
        public override object AssumedValue
        {
            get
            {
                return this.assumedValue;
            }
            set
            {
                this.assumedValue = (Iso8601Date)value;
            }
        }

        internal override string ValidValue(object aValue)
        {
            DesignByContract.Check.Require(aValue != null, string.Format(CommonStrings.XMustNotBeNull, "aValue"));

            if (!Iso8601Date.ValidIso8601Date(aValue.ToString()))
                return string.Format(AmValidationStrings.InvalidIsoDateX, aValue);

            Iso8601Date isoDate = new Iso8601Date(aValue.ToString());
            if (isoDate == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "isoDate"));

            if (this.Pattern != null)
            {
                if (!IsMatchPattern(isoDate))
                {
                    return string.Format(AmValidationStrings.DateXDoesNotMatchPatternY, aValue, Pattern);
                }
            }

            if (this.Range != null)
            {
                if (!this.Range.Has(isoDate))
                {
                    return string.Format(AmValidationStrings.DateXOutOfRange, aValue);
                }
            }
            return string.Empty;
        }

        private bool IsMatchPattern(Iso8601Date isoDate)
        {
            DesignByContract.Check.Require(isoDate != null,
                string.Format(CommonStrings.XMustNotBeNull, "isoDate"));
            DesignByContract.Check.Require(this.Pattern != null, 
                string.Format(CommonStrings.XMustNotBeNull, "Pattern"));

            if (!CDateTime.IsMatchValidityKind(this.MonthValidity, isoDate.MonthUnknown))
                return false;

            if (!CDateTime.IsMatchValidityKind(this.DayValidity, isoDate.DayUnknown))
                return false;

            return true;
        }

        internal override bool IsSubsetOf(CPrimitive other)
        {
            throw new NotImplementedException(
                string.Format(AmValidationStrings.IsSubsetNotImplementedInX, "CDate"));
        }
        #endregion
       
    }
}
