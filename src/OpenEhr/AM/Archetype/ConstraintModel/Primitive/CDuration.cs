using System;
using OpenEhr.AssumedTypes;
using System.Text.RegularExpressions;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;
using OpenEhr.Attributes;
using OpenEhr.Validation;

namespace OpenEhr.AM.Archetype.ConstraintModel.Primitive
{
    /// <summary>
    /// ISO 8601-compatible constraint on instances of Duration. In ISO 8601 terms,
    /// constraints might are of the form “PWD” (weeks and/or days), “PDTHMS” (days,
    /// hours, minutes, seconds) and so on. In official ISO 8601:2004, the ‘W’ (week)
    /// designator cannot be mixed in; allowing it is an openEHR-wide exception.
    /// </summary>
    [Serializable]
    [AmType("C_DURATION")]
    public class CDuration : CPrimitive
    {
        private const string durationPatternPattern = @"(P(?<years>[yY])?(?<months>[mM])?(?<weeks>[wW])?(?<days>[dD])?T(?<hours>[hH])?(?<minutes>[mM])?(?<seconds>[sS])?)|(P(?<years>[yY])?(?<months>[mM])?(?<weeks>[wW])?(?<days>[dD])?)";

        #region Constructors
        public CDuration() { }
        #endregion

        #region Class properties
        string pattern;
        internal string Pattern
        {
            get
            {
                if (pattern == null)
                    pattern = GetPattern();

                return pattern;
            }
            set
            {
                DesignByContract.Check.Require(!string.IsNullOrEmpty(value), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Pattern value"));
                DesignByContract.Check.Require(Regex.IsMatch(value, durationPatternPattern, RegexOptions.Compiled | RegexOptions.Singleline),
                    string.Format(CommonStrings.DurationPatternInvalid, value));

                this.pattern = value;
            }
        }

        private string GetPattern()
        {
            string durationPattern = "P";
            if (this.yearsAllowedSet)
            {
                if (this.YearsAllowed)
                    durationPattern += "Y";
            }

            if (this.monthsAllowedSet)
            {
                if (this.MonthsAllowed)
                    durationPattern += "M";
            }

            if (this.weeksAllowedSet)
            {
                if (this.WeeksAllowed)
                    durationPattern += "W";
            }

            if (this.daysAllowedSet)
            {
                if (this.DaysAllowed)
                    durationPattern += "D";
            }

            if (this.hoursAllowedSet)
            {
                if (this.HoursAllowed)
                {
                    if (durationPattern.IndexOf("T", StringComparison.CurrentCultureIgnoreCase) < 0)
                        durationPattern += "T";
                    durationPattern += "h";
                }
            }

            if (this.minutesAllowedSet)
            {
                if (this.MinutesAllowed)
                {
                    if (durationPattern.IndexOf("T", StringComparison.CurrentCultureIgnoreCase) < 0)
                        durationPattern += "T";
                    durationPattern += "m";
                }
            }

            if (this.secondsAllowedSet)
            {
                if (this.SecondsAllowed)
                {
                    if (durationPattern.IndexOf("T", StringComparison.CurrentCultureIgnoreCase) < 0)
                        durationPattern += "T";
                    durationPattern += "s";
                }
            }

            if (durationPattern == "P")
                return null;

            DesignByContract.Check.Ensure(Regex.IsMatch(durationPattern, durationPatternPattern, RegexOptions.Compiled | RegexOptions.Singleline),
                "durationPattern must be a valid pattern: " + durationPattern);

            return durationPattern;

        }

        private void SetClassProperty()
        {
            if (this.pattern != null)
            {
                Match match = Regex.Match(this.pattern, durationPatternPattern, RegexOptions.Compiled | RegexOptions.Singleline);
                GroupCollection groups = match.Groups;

                if (groups["years"].Value.ToLower() == "y")
                {
                    this.YearsAllowed = true;
                }
                else
                    this.YearsAllowed = false;

                if (groups["months"].Value.ToLower() == "m")
                {
                    this.MonthsAllowed = true;
                }
                else
                    this.MonthsAllowed = false;

                if (groups["weeks"].Value.ToLower() == "w")
                {
                    this.WeeksAllowed = true;
                }
                else
                    this.WeeksAllowed = false;

                if (groups["hours"].Value.ToLower() == "h")
                {
                    this.HoursAllowed = true;
                }
                else
                    this.HoursAllowed = false;

                if (groups["minutes"].Value.ToLower() == "m")
                {
                    this.MinutesAllowed = true;
                }
                else
                    this.MinutesAllowed = false;

                if (groups["seconds"].Value.ToLower() == "s")
                {
                    this.SecondsAllowed = true;
                }
                else
                    this.SecondsAllowed = false;
            }
        }

        private bool yearsAllowedSet;
        private bool yearsAllowed;
        /// <summary>
        /// True if years are allowed in the constrained Duration.
        /// </summary>
        public bool YearsAllowed
        {
            get { return yearsAllowed; }
            set
            {
                yearsAllowed = value;
                yearsAllowedSet = true;
            }
        }

        private bool monthsAllowedSet;

        private bool monthsAllowed;
        /// <summary>
        /// True if months are allowed in the constrained Duration.
        /// </summary>
        public bool MonthsAllowed
        {
            get { return monthsAllowed; }
            set
            {
                monthsAllowed = value;
                monthsAllowedSet = true;
            }
        }

        private bool weeksAllowedSet;
        private bool weeksAllowed;
        /// <summary>
        /// True if weeks are allowed in the constrained Duration.
        /// </summary>
        public bool WeeksAllowed
        {
            get { return weeksAllowed; }
            set
            {
                weeksAllowed = value;
                weeksAllowedSet = true;
            }
        }

        private bool daysAllowedSet;
        private bool daysAllowed;
        /// <summary>
        /// True if days are allowed in the constrained Duration.
        /// </summary>
        public bool DaysAllowed
        {
            get { return daysAllowed; }
            set
            {
                daysAllowed = value;
                daysAllowedSet = true;
            }
        }

        private bool hoursAllowedSet;
        private bool hoursAllowed;
        /// <summary>
        /// True if hours are allowed in the constrained Duration.
        /// </summary>
        public bool HoursAllowed
        {
            get { return hoursAllowed; }
            set
            {
                hoursAllowed = value;
                hoursAllowedSet = true;
            }
        }

        private bool minutesAllowedSet;
        private bool minutesAllowed;
        /// <summary>
        /// True if minutes are allowed in the constrained Duration.
        /// </summary>
        public bool MinutesAllowed
        {
            get { return minutesAllowed; }
            set
            {
                minutesAllowed = value;
                minutesAllowedSet = true;
            }
        }

        private bool secondsAllowedSet;
        private bool secondsAllowed;
        /// <summary>
        /// True if seconds are allowed in the constrained Duration.
        /// </summary>
        public bool SecondsAllowed
        {
            get { return secondsAllowed; }
            set
            {
                secondsAllowed = value;
                secondsAllowedSet = true;
            }
        }


        private bool fractionalSecondsAllowed = true;
        /// <summary>
        /// True if fractional seconds are allowed in the constrained Duration.
        /// </summary>
        public bool FractionalSecondsAllowed
        {
            get { return fractionalSecondsAllowed; }
            set { fractionalSecondsAllowed = value; }
        }

        private Interval<Iso8601Duration> range;
        /// <summary>
        /// Range of Durations specifying constraint
        /// </summary>
        public Interval<Iso8601Duration> Range
        {
            get { return this.range; }
            set { this.range = value; }
        }

        private string defaultValue;

        /// <summary>
        /// Duration default value as ISO8601 duration string
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
                        defaultValue = new DvDuration().ToString();
                }
                return this.defaultValue;
            }
        }     

        private Iso8601Duration assumedValue;

        /// <summary>
        /// Duration assumed value as Iso8601Duration 
        /// </summary>
        public override object AssumedValue
        {
            get
            {
                return this.assumedValue;
            }
            set
            {
                this.assumedValue = value as Iso8601Duration;
                Check.Ensure(value == null || this.assumedValue != null, "value must be of type Iso8601Duration");
            }
        }

        #endregion

        #region functions
        public override bool HasAssumedValue()
        {
            return this.AssumedValue != null;
        }

        internal override string ValidValue(object aValue)
        {
            DesignByContract.Check.Require(aValue != null, string.Format(CommonStrings.XMustNotBeNull, "aValue"));

            if (!Iso8601Duration.ValidIso8601Duration(aValue.ToString()))
                return string.Format(AmValidationStrings.InvalidIsoDurationX, aValue);

            Iso8601Duration isoDuration = new Iso8601Duration(aValue.ToString());
            if (isoDuration == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, isoDuration));

            if (this.Range != null)
            {
                if (!this.range.Has(isoDuration))
                {
                    return string.Format(AmValidationStrings.DurationXOutOfRange, aValue);
                }
            }
            
            if ( this.Range == null && this.Pattern == null)
                throw new ValidationException(CommonStrings.CDurationPatternAndRangeNull);

            if (this.range != null)
            {
                if(!IsMatchPattern(isoDuration))
                {
                    return string.Format(AmValidationStrings.DurationXDoesNotMatchPatternY, aValue, Pattern);
                }
            }

            return string.Empty;
        }

        internal void AllowAny()
        {
            this.YearsAllowed = true;
            this.WeeksAllowed = true;
            this.MonthsAllowed = true;
            this.DaysAllowed = true;
            this.HoursAllowed = true;
            this.MinutesAllowed = true;
            this.SecondsAllowed = true;            
        }

        private bool IsMatchPattern(Iso8601Duration isoDuration)
        {
            DesignByContract.Check.Require(isoDuration != null, string.Format(CommonStrings.XMustNotBeNull, "isoDuration"));

            if (this.yearsAllowedSet && !this.YearsAllowed && isoDuration.Years > 0)
                return false;

            if (this.monthsAllowedSet && !this.MonthsAllowed && isoDuration.Months > 0)
                return false;

            if (this.weeksAllowedSet && !this.WeeksAllowed && isoDuration.Weeks > 0)
                return false;

            if (this.daysAllowedSet && !this.DaysAllowed && isoDuration.Days > 0)
                return false;

            if (this.hoursAllowedSet && !this.HoursAllowed && isoDuration.Hours > 0)
                return false;

            if (this.minutesAllowedSet && !this.MinutesAllowed && isoDuration.Months > 0)
                return false;

            if (this.secondsAllowedSet && !this.SecondsAllowed && isoDuration.Seconds > 0)
                return false;

            if (!this.fractionalSecondsAllowed && isoDuration.FractionalSecond > 0)
                return false;

            return true;
        }

        internal override bool IsSubsetOf(CPrimitive other)
        {
            throw new NotImplementedException(
                string.Format(AmValidationStrings.IsSubsetNotImplementedInX, "CDuration"));
        }
        #endregion
    }
}
