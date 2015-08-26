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
    /// ISO 8601-compatible constraint on instances of Time. There is no validity flag
    /// for ‘hour’, since it must always be by definition mandatory in order to have a sensible
    /// time at all. Syntax expressions of instances of this class include “HH:??:xx”
    /// (time with optional minutes and seconds not allowed).
    /// </summary>
    [Serializable]
    [AmType("C_TIME")]
    public class CTime: CPrimitive
    {
        #region Constructors
        public CTime() { }

        #endregion

        #region Class properties
        private ValidityKind minuteValidity;
        /// <summary>
        /// Validity of minute in constrained time.
        /// </summary>
        public ValidityKind MinuteValidity
        {
            get
            {
                if (this.minuteValidity == null && this.pattern != null)
                    SetClassProperty();
                return minuteValidity;
            }
            set { minuteValidity = value; }
        }

        private ValidityKind secondValidity;
        /// <summary>
        /// Validity of second in constrained time.
        /// </summary>
        public ValidityKind SecondValidity
        {
            get
            {
                if (this.secondValidity == null && this.pattern != null)
                    SetClassProperty();
                return secondValidity;
            }

            set { secondValidity = value; }
        }

        private ValidityKind millisecondValidity;
        /// <summary>
        /// Validity of millisecond in constrained time.
        /// </summary>
        public ValidityKind MillisecondValidity
        {
            get { return millisecondValidity; }
            set { millisecondValidity = value; }
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

        private Interval<Iso8601Time> range;
        /// <summary>
        /// Interval of Times specifying constraint
        /// </summary>
        public Interval<Iso8601Time> Range
        {
            get { return range; }
            set { range = value; }
        }

        private string defaultValue;

        /// <summary>
        /// Default Time value as ISO8601 Time string
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
                        return new DvTime().ToString();
                }
                return this.defaultValue;
            }
        }       

        private Iso8601Time assumedValue;
        public override object AssumedValue
        {
            get
            {
                return this.assumedValue;
            }
            set
            {
                this.assumedValue = (Iso8601Time)value;
            }
        }
       
        // this timePatternPattern is obtained from Archetype.xsd. It doesn't show millisecond pattern
        const string timePatternPattern = @"[hH][hH]:(?<minute>[mM?X][mM?X]):(?<second>[sS?X][sS?X])";
        private string pattern;
        internal string Pattern
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
                DesignByContract.Check.Require(this.pattern == null, CommonStrings.PatternMustBeNullBeforeSet);
                DesignByContract.Check.Ensure(Regex.IsMatch(value, timePatternPattern, RegexOptions.Compiled | RegexOptions.Singleline),
                    string.Format(CommonStrings.TimePatternInvalid, value));
               
                this.pattern = value;
            }
        }

        private string GetPattern()
        {
            if (this.MinuteValidity == null && this.SecondValidity == null)
                return null;

            string timePattern = "hh";
            if (this.MinuteValidity != null)
            {
                switch (this.MinuteValidity.Value)
                {
                    case 1001:
                        timePattern += ":mm";
                        break;
                       
                    case 1002:
                    case 1003:
                        timePattern += ":??";
                        break;
                    default:
                        throw new NotSupportedException(string.Format(
                            CommonStrings.InvalidMinuteValidityX, this.MinuteValidity.Value));
                }
            }
            else
                timePattern += ":??";

            if (this.SecondValidity != null)
            {
                switch (this.SecondValidity.Value)
                {
                    case 1001:
                        timePattern += ":ss";
                        break;
                    case 1002:
                    case 1003:
                        timePattern += ":??";
                        break;
                    default:
                        throw new NotSupportedException(string.Format(
                            CommonStrings.InvalidSecondValidityX, this.SecondValidity.Value));
                }
            }
            else
                timePattern += ":??";

            DesignByContract.Check.Ensure(Regex.IsMatch(timePattern, timePatternPattern, RegexOptions.Compiled | RegexOptions.Singleline), 
                "timePattern must be valid pattern: "+timePattern);

            return timePattern;
        }

        private void SetClassProperty()
        {
            if (!string.IsNullOrEmpty(this.Pattern))
            {
                Match match = Regex.Match(this.Pattern, timePatternPattern, RegexOptions.Compiled | RegexOptions.Singleline);
                GroupCollection groups = match.Groups;

                if (groups == null)
                    throw new ApplicationException(CommonStrings.RegexGroupsMustNotBeNull);
                
                this.MinuteValidity = CDateTime.ToValidityKind(groups["minute"].Value);

                this.SecondValidity = CDateTime.ToValidityKind(groups["second"].Value);

            }
        }
        #endregion

        #region Functions

        /// <summary>
        /// True if validity is in the form of a range; useful for developers to check which kind
        /// of constraint has been set.
        /// </summary>
        /// <returns></returns>
        public bool ValidityIsRange()
        {
            return this.Range != null;
        }

        public override bool HasAssumedValue()
        {
            return this.AssumedValue != null;
        }

        internal override string ValidValue(object aValue)
        {
            DesignByContract.Check.Require(aValue != null, string.Format(CommonStrings.XMustNotBeNull, "aValue"));

            string timeString = aValue.ToString();
            if (!Iso8601Time.ValidIso8601Time(timeString))
            {
                return string.Format(AmValidationStrings.InvalidIsoTime, timeString);
            }

            Iso8601Time isoTime = new Iso8601Time(timeString);
            if (isoTime == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "isoTime"));

            if (this.Pattern != null)
            {
                if (!IsMatchPattern(isoTime))
                {
                    return string.Format(AmValidationStrings.TimeXDoesNotMatchPatternY, isoTime, Pattern);
                }
            }
            if (this.Range != null)
                if (!this.Range.Has(isoTime))
                {
                    return string.Format(AmValidationStrings.TimeXOutOfRange, isoTime);
                }

            return string.Empty;
        }

        private bool IsMatchPattern(Iso8601Time isoTime)
        {
            DesignByContract.Check.Require(isoTime != null, string.Format(CommonStrings.XMustNotBeNull, "isoTime"));
            DesignByContract.Check.Require(this.Pattern != null, string.Format(CommonStrings.XMustNotBeNull, "Pattern"));

            if(!CDateTime.IsMatchValidityKind(this.MinuteValidity, isoTime.MinuteUnknown))
                return false;

            if(!CDateTime.IsMatchValidityKind(this.SecondValidity, isoTime.SecondUnknown))
                return false;          

            return true;
        }

        internal override bool IsSubsetOf(CPrimitive other)
        {
            throw new NotImplementedException(
                string.Format(AmValidationStrings.IsSubsetNotImplementedInX, "CTime"));
        }
        #endregion
    }
}
