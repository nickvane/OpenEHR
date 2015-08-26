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
    /// ISO 8601-compatible constraint on instances of Date_Time. There is no validity
    /// flag for ‘year’, since it must always be by definition mandatory in order to have a
    /// sensible date/time at all. Syntax expressions of instances of this class include
    /// “YYYY-MM-DDT??:??:??” (date/time with optional time) and “YYYY-MMDDTHH:
    /// MM:xx” (date/time, seconds not allowed).
    /// </summary>
    [Serializable]
    [AmType("C_DATE_TIME")]
    public class CDateTime: CPrimitive
    {
        // this pattern is obtained from the archeytpe.xsd.
        private const string dateTimePatternPattern = CDate.dayPatternPattern + @"[T](?<hour>[hH?X][hH?X]):(?<minute>[mM?X][mM?X]):(?<second>[sS?X][sS?X])";

        #region constructors
        public CDateTime() { }
        #endregion

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
                DesignByContract.Check.Require(!string.IsNullOrEmpty(value), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Pattern value"));
                DesignByContract.Check.Ensure(Regex.IsMatch(value, dateTimePatternPattern, RegexOptions.Compiled | RegexOptions.Singleline),
                    string.Format(CommonStrings.DatePatternInvalid, value));
                DesignByContract.Check.Require(this.pattern == null, CommonStrings.PatternMustBeNullBeforeSet);
               
                this.pattern = value;
            }
        }

        private string GetPattern()
        {
            if (this.MonthValidity == null && this.DayValidity == null && this.HourValidity == null
                && this.MinuteValidity == null && this.SecondValidity == null)
                return null;

            string dateTimePattern = "yyyy";
            if (this.MonthValidity != null)
            {
                switch (this.MonthValidity.Value)
                {
                    case 1001:
                        dateTimePattern += "-mm";
                        break;
                    case 1002:
                        dateTimePattern += "-??";
                        break;
                    default:
                        throw new NotSupportedException(string.Format(
                            CommonStrings.InvalidMonthValidityX, this.MonthValidity.Value));

                }
            }
            else
                dateTimePattern += "-??";

            if (this.DayValidity != null)
            {

                switch (this.DayValidity.Value)
                {
                    case 1001:
                        dateTimePattern += "-dd";
                        break;
                    case 1002:
                    case 1003:
                        dateTimePattern += "-??";
                        break;
                    default:
                        throw new NotSupportedException(string.Format(
                            CommonStrings.InvalidDayValidityX, this.DayValidity.Value));
                }
            }
            else
                dateTimePattern += "-??";

            DesignByContract.Check.Ensure(Regex.IsMatch(dateTimePattern, CDate.dayPatternPattern, RegexOptions.Compiled | RegexOptions.Singleline),
                "pattern must be valid date pattern: " + dateTimePattern);

            if (this.HourValidity != null)
            {
                switch (this.HourValidity.Value)
                {
                    case 1001:
                        dateTimePattern += ":hh";
                        break;
                    case 1002:
                    case 1003:
                        dateTimePattern += ":??";
                        break;
                    default:
                        throw new NotSupportedException(string.Format(
                            CommonStrings.InvalidHourValidityX, this.HourValidity.Value));

                }
            }
            else
                dateTimePattern += ":??";
           
            if (this.MinuteValidity != null)
            {
                switch (this.MinuteValidity.Value)
                {
                    case 1001:
                        dateTimePattern += ":mm";
                        break;
                    case 1002:
                    case 1003:
                        dateTimePattern += ":??";
                        break;
                    default:
                        throw new NotSupportedException(string.Format(
                            CommonStrings.InvalidMinuteValidityX, this.MinuteValidity.Value));

                }
            }
            else
                dateTimePattern += ":??";

            if (this.SecondValidity != null)
            {
                switch (this.SecondValidity.Value)
                {
                    case 1001:
                        dateTimePattern += ":ss";
                        break;
                    case 1002:
                    case 1003:
                        dateTimePattern += ":??";
                        break;
                    default:
                        throw new NotSupportedException(string.Format(
                            CommonStrings.InvalidSecondValidityX, this.SecondValidity.Value));

                }
            }
            else
                dateTimePattern += ":??";

            DesignByContract.Check.Ensure(Regex.IsMatch(dateTimePattern, dateTimePatternPattern, RegexOptions.Compiled | RegexOptions.Singleline),
                "dateTimePattern must be a valid pattern: "+dateTimePattern);

            return dateTimePattern;
        }

        private void SetClassProperty()
        {
            if (!string.IsNullOrEmpty(this.pattern))
            {
                Match match = Regex.Match(this.pattern, dateTimePatternPattern, RegexOptions.Compiled | RegexOptions.Singleline);
                GroupCollection groups = match.Groups;

                if (groups == null)
                    throw new ApplicationException(CommonStrings.RegexGroupsMustNotBeNull);

                string monthPattern = groups["month"].Value;
                this.MonthValidity = ToValidityKind(monthPattern);

                string dayPattern = groups["day"].Value;
                this.DayValidity = ToValidityKind(dayPattern);

                string hourPattern = groups["hour"].Value;
                this.HourValidity = ToValidityKind(hourPattern);

                string minutePattern = groups["minute"].Value;
                this.MinuteValidity = ToValidityKind(minutePattern);

                this.SecondValidity = ToValidityKind(groups["second"].Value);
            }
        }

        internal static ValidityKind ToValidityKind(string patternString)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(patternString), 
                string.Format(CommonStrings.XMustNotBeNullOrEmpty, "patternString"));

            switch (patternString.ToLower(System.Globalization.CultureInfo.CurrentCulture))
            {
                case "??":
                    return new ValidityKind(1002);
                case "xx":
                    return new ValidityKind(1003);
                case "mm":
                case "dd":
                case "hh":
                case "ss":
                    return new ValidityKind(1001);
                default:
                    throw new NotSupportedException(string.Format(
                        CommonStrings.DatePatternInvalid, patternString));
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
                if (monthValidity == null && this.pattern != null)
                    SetClassProperty();

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
                if (dayValidity == null && this.pattern != null)
                    SetClassProperty();
                return dayValidity;
            }
            set { dayValidity = value; }
        }

        private ValidityKind hourValidity;
        /// <summary>
        /// Validity of hour in constrained time.
        /// </summary>
        public ValidityKind HourValidity
        {
            get
            {
                if (hourValidity == null && this.pattern != null)
                    SetClassProperty();
                return hourValidity;
            }
            set { hourValidity = value; }
        }

        private ValidityKind minuteValidity;
        /// <summary>
        /// Validity of minute in constrained time.
        /// </summary>
        public ValidityKind MinuteValidity
        {
            get
            {
                if (minuteValidity == null && this.pattern != null)
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
                if (secondValidity == null && this.pattern != null)
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

        private Interval<Iso8601DateTime> range;
        /// <summary>
        /// Range of Date_times specifying constraint
        /// </summary>
        public Interval<Iso8601DateTime> Range
        {
            get { return range; }
            set { range = value; }
        }

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
        /// Default DateTime value as ISO8601 DateTime string
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
                        return new DvDateTime().ToString();
                }
                return this.defaultValue;
            }
        }      

        private Iso8601DateTime assumedValue;
        public override object AssumedValue
        {
            get
            {
                return this.assumedValue;
            }
            set
            {
                this.assumedValue = (Iso8601DateTime)value;
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

            string dateTimeString = aValue.ToString();
            if (!Iso8601DateTime.ValidIso8601DateTime(dateTimeString))
            {
                return string.Format(AmValidationStrings.InvalidIsoDateTimeX, dateTimeString);
            }

            Iso8601DateTime isoDateTime = new Iso8601DateTime(dateTimeString);
            if (isoDateTime == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "isoDateTime"));

            if (this.Pattern != null)
            {
                if (!IsMatchPattern(isoDateTime))
                {
                    return string.Format(AmValidationStrings.DateTimeXDoesNotMatchPatternY, dateTimeString, Pattern);
                }
            }

            if (this.Range != null)
            {
                if (!this.Range.Has(isoDateTime))
                {
                    return string.Format(AmValidationStrings.DateTimeXOutOfRange, dateTimeString);
                }
            }

            return string.Empty;
        }
   
        private bool IsMatchPattern(Iso8601DateTime aValue)
        {
            DesignByContract.Check.Require(aValue != null, string.Format(CommonStrings.XMustNotBeNull, "aValue"));
            DesignByContract.Check.Require(this.Pattern != null, string.Format(CommonStrings.XMustNotBeNull, "Pattern"));

            if (!CDateTime.IsMatchValidityKind(this.MonthValidity, aValue.MonthUnknown))
                return false;
            if (!CDateTime.IsMatchValidityKind(this.DayValidity, aValue.DayUnknown))
                return false;
            if(!CDateTime.IsMatchValidityKind(this.HourValidity, aValue.HourUnknown))
                return false;
            if(!CDateTime.IsMatchValidityKind(this.MinuteValidity, aValue.MinuteUnknown))
                return false;
            if(!CDateTime.IsMatchValidityKind(this.SecondValidity, aValue.SecondUnknown))
                return false;
            if(!CDateTime.IsMatchValidityKind(this.MillisecondValidity, !aValue.HasFractionalSecond))
                return false;

            return true;

        }

        internal static bool IsMatchValidityKind(ValidityKind validityKind, bool valueUnknown)
        {
            if (validityKind == null)
                return true;

            if (validityKind.Value == 1001 && valueUnknown)
                return false;
            if (validityKind.Value == 1003 && !valueUnknown)
                return false;

            return true;
        }

        internal override bool IsSubsetOf(CPrimitive other)
        {
            throw new NotImplementedException(
                string.Format(AmValidationStrings.IsSubsetNotImplementedInX, "CDateTime"));
        }
        #endregion
    }
}