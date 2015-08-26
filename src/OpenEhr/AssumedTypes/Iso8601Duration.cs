using System;
//using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;

namespace OpenEhr.AssumedTypes
{
    /// <summary>
    /// Represents a period of time corresponding 
    /// to a difference between two time-points
    /// </summary>
    [Serializable]
    [RmType("openEHR", "SUPPORT", "ISO8601_DURATION")]
    public class Iso8601Duration : TimeDefinitions, IComparable
    {
        // LMT 05/05/2009 Add nullability to all units EHR-900
        private string asString;
        private int? years = null; 
        private int? months = null;
        private int? weeks = null;
        private int? days = null;
        private int? hours = null;
        private int? minutes = null;
        private int? seconds = null;
        private double? fractionalSecond = null;

        private double durationValueInSeconds = -1.0;

        private static string durationYYCapturedName = "dYY";
        private static string durationMMCapturedName = "dMM";
        private static string durationWWCapturedName = "dww";
        private static string durationDDCapturedName = "dDD";
        private static string durationTTCapturedName = "dtt";
        private static string durationHHCapturedName = "dhh";
        private static string durationMinuteCapturedName = "dmm";
        private static string durationSSCapturedName = "dss";
        private static string durationFSecondsCapturedName = "dsss";
        // a captured name for a year|month|date duration
        private static string durationYMDCapturedName = "dymd";

        // %HYYKA%
        //\b(?<p>P)(?<ydm>(?<YYYY>\d+Y)?(?<MM>\d+M)?(?<ww>\d+W)?(?<dd>\d+D)?)?(?<dt>T(?<hh>\d+H)?(?<mm>\d+M)?(?<ss>\d+S)?(?<ssss>\,\d+)?)?\b
        //^(?<p>P)(?<ydm>((?<YYYY>\d+)Y)?((?<MM>\d+)M)?((?<ww>\d+)W)?((?<dd>\d+)D)?)?(?<dt>T((?<hh>\d+)H)?((?<mm>\d+)M)?(((?<ss>\d+)S)?|((?<ss>\d+)(?<ssss>\,\d+)([S])?)))?$
        //^(?<p>P)(?<ydm>((?<YYYY>\d+)Y)?((?<MM>\d+)M)?((?<ww>\d+)W)?((?<dd>\d+)D)?)?(?<dt>T((?<hh>\d+)H)?((?<mm>\d+)M)?(((?<ss>\d+)S)|((?<ss>\d+)(?<ssss>\,\d+)([S])))?)?$
        //private static string durationPattern = @"^(?<p>P)(?<" + durationYMDCapturedName + @">((?<"
        //    + durationYYCapturedName + @">\d+)Y)?((?<" + durationMMCapturedName + @">\d+)M)?((?<"
        //    + durationWWCapturedName + @">\d+)W)?((?<" + durationDDCapturedName + @">\d+)D)?)?(?<"
        //    + durationTTCapturedName + @">T((?<" + durationHHCapturedName + @">\d+)H)?((?<"
        //    + durationMinuteCapturedName + @">\d+)M)?(((?<" + durationSSCapturedName + @">\d+)S)?|((?<"
        //    + durationSSCapturedName + @">\d+)(?<" + durationFSecondsCapturedName + @">\,\d+)([S])?)))?$";

        //^(P)(((\d+)Y)?((\d+)M)?((\d+)W)?((\d+)D)?)?(T((\d+)H)?((\d+)M)?(((\d+)S)|((\d+)([.,]\d+)S))?)?$
        
        private static string durationPattern = @"^(?<p>P)(?<" + durationYMDCapturedName + @">((?<"
            + durationYYCapturedName + @">\d+)Y)?((?<" + durationMMCapturedName + @">\d+)M)?((?<"
            + durationWWCapturedName + @">\d+)W)?((?<" + durationDDCapturedName + @">\d+)D)?)?(?<"
            + durationTTCapturedName + @">T((?<" + durationHHCapturedName + @">\d+)H)?((?<"
            + durationMinuteCapturedName + @">\d+)M)?(((?<" + durationSSCapturedName + @">\d+)S)|((?<"
            + durationSSCapturedName + @">\d+)(?<" + durationFSecondsCapturedName + @">[,.]\d+)S))?)?$";
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="durationString"></param>
        public Iso8601Duration(string durationString)
        {
            ParseIso8601Duration(durationString);
        }

        public Iso8601Duration(int years, int months, int days, int weeks, int hours,
             int minutes, int seconds, double fractionalSeconds)
        {
            Check.Require(years >= 0 && months >= 0 && days >= 0 && weeks >= 0 && hours >= 0
                && minutes >= 0 && seconds >= 0 && fractionalSeconds >= 0.0,
                "Expect every unil to be greater than or equal to zero");
            Check.Require(fractionalSeconds < 1.0d, "Expect fractional second to be less than 1"); // LMT added 05/05/2009

            this.years = years; //BJP 19/02/2009 Years required to be set
            this.months = months;
            this.days = days;
            this.weeks = weeks;
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
            this.fractionalSecond = fractionalSeconds;

            // LMT 05/05/2009 Add nullability to all units EHR-900
            Check.Ensure(this.years != null && this.months != null && this.days != null
                && this.weeks != null && this.hours != null && this.minutes != null
                && this.seconds != null && fractionalSecond != null, "Expect every unit to be non-null");
        }

        #region properties
        // LMT 05/05/2009 Add nullability to all units EHR-900
        public int Years
        {
            get { return this.years.GetValueOrDefault(0); }
        }

        public int Months
        {
            get { return this.months.GetValueOrDefault(0); }
        }

        public int Weeks
        {
            get { return this.weeks.GetValueOrDefault(0); }

        }

        public int Days
        {
            get { return this.days.GetValueOrDefault(0); }
        }

        public int Hours
        {
            get { return this.hours.GetValueOrDefault(0); }

        }

        public int Minutes
        {
            get { return this.minutes.GetValueOrDefault(0); }

        }
        public int Seconds
        {
            get { return this.seconds.GetValueOrDefault(0); }

        }
        public double FractionalSecond
        {
            get { return this.fractionalSecond.GetValueOrDefault(0.0d); }

        }

        public double DurationValueInSeconds
        {
            get
            {
                if (this.durationValueInSeconds <= 0)
                    GetDurationValue();

                Check.Assert(this.durationValueInSeconds >= 0,
                    "Duration value in seconds must not be less than or equal to zero.");
                return this.durationValueInSeconds;
            }
        }
        #endregion

        #region ValidIso8601Duration
        /// <summary>
        /// True if the duration string is in ISO 8601 duration format.
        /// </summary>
        /// <param name="durationString"></param>
        /// <returns></returns>
        public static bool ValidIso8601Duration(string durationString)
        {
            if (string.IsNullOrEmpty(durationString))
                return false;

            Match thisMatch = Regex.Match(durationString, durationPattern, RegexOptions.Compiled | RegexOptions.Singleline);

            if (!thisMatch.Success)
                return false;

            GroupCollection gCollection = thisMatch.Groups;

            //if the number of years, months, days, hours, minutes, seconds 
            //equals zero, the number and the corresponding disignator 
            //may be absent; however, at least one number and its designator 
            //shall be present.
            string yymmddDuration = gCollection[durationYMDCapturedName].Value;
            string timeDuration = gCollection[durationTTCapturedName].Value;
            if (string.IsNullOrEmpty(yymmddDuration) && string.IsNullOrEmpty(timeDuration))
                return false;

            if (!string.IsNullOrEmpty(yymmddDuration))
            {
                string dYearString = gCollection[durationYYCapturedName].Value;
                string dMonthString = gCollection[durationMMCapturedName].Value;
                string dWeekString = gCollection[durationWWCapturedName].Value;
                string dDayString = gCollection[durationDDCapturedName].Value;

                if (string.IsNullOrEmpty(dYearString) && string.IsNullOrEmpty(dMonthString)
                     && string.IsNullOrEmpty(dDayString) && string.IsNullOrEmpty(dWeekString))
                    return false;

                if (!string.IsNullOrEmpty(dYearString))
                {
                    int yearValue;
                    if (!int.TryParse(dYearString, out yearValue))
                        return false;
                    if (yearValue < 0)
                        return false;
                }
                if (!string.IsNullOrEmpty(dMonthString))
                {
                    int monthValue;
                    if (!int.TryParse(dMonthString, out monthValue))
                        return false;
                    if (monthValue < 0)
                        return false;
                }
                if (!string.IsNullOrEmpty(dDayString))
                {
                    int dayValue;
                    if (!int.TryParse(dDayString, out dayValue))
                        return false;
                    if (dayValue < 0)
                        return false;
                }
                if (!string.IsNullOrEmpty(dWeekString))
                {
                    int weekValue;
                    if (!int.TryParse(dWeekString, out weekValue))
                        return false;
                    if (weekValue < 0)
                        return false;
                }
            }

            if (!string.IsNullOrEmpty(timeDuration))
            {
                string dHString = gCollection[durationHHCapturedName].Value;
                string dMinString = gCollection[durationMinuteCapturedName].Value;
                string dSecondString = gCollection[durationSSCapturedName].Value;
                string dFractionalSecondString = gCollection[durationFSecondsCapturedName].Value;

                if (string.IsNullOrEmpty(dHString) && string.IsNullOrEmpty(dMinString)
                    && string.IsNullOrEmpty(dSecondString))
                    return false;

                if (!string.IsNullOrEmpty(dHString))
                {
                    int hourValue;
                    if (!int.TryParse(dHString, out hourValue))
                        return false;
                    if (hourValue < 0)
                        return false;
                }
                if (!string.IsNullOrEmpty(dMinString))
                {
                    int minValue;
                    if (!int.TryParse(dMinString, out minValue))
                        return false;
                    if (minValue < 0)
                        return false;
                }
                if (!string.IsNullOrEmpty(dSecondString))
                {
                    int secondValue;
                    if (!int.TryParse(dSecondString, out secondValue))
                        return false;
                    if (secondValue < 0)
                        return false;

                }
                if (!string.IsNullOrEmpty(dFractionalSecondString))
                {
                    double fSecondValue;
                    if (!double.TryParse(dFractionalSecondString.Replace(',', '.'), out fSecondValue))
                        return false;
                    if (fSecondValue >= 1.0 || fSecondValue < 0.0)
                        return false;

                }
            }
            return true;
        }
        #endregion

        #region ParseIso8601Duration
        /// <summary>
        /// Extract the values from an ISO 8601 duration string and assing these
        /// values to the class properties
        /// </summary>
        /// <param name="durationString"></param>
        private void ParseIso8601Duration(string durationString)
        {
            Check.Require(!string.IsNullOrEmpty(durationString), "The duration string must not be null or empty");
            Check.Require(ValidIso8601Duration(durationString),
                "The duration string (" + durationString + ") must be in ISO 8601 duration format.");

            Match match = Regex.Match(durationString, durationPattern, RegexOptions.Compiled | RegexOptions.Singleline);
   
            string yymmddDuration = match.Groups[durationYMDCapturedName].Value;
            if (!string.IsNullOrEmpty(yymmddDuration))
            {
                string dYearString = match.Groups[durationYYCapturedName].Value;
                if (!string.IsNullOrEmpty(dYearString))
                    this.years = int.Parse(dYearString);

                string dMonthString = match.Groups[durationMMCapturedName].Value;
                if (!string.IsNullOrEmpty(dMonthString))
                    this.months = int.Parse(dMonthString);

                string dWeekString = match.Groups[durationWWCapturedName].Value;
                if (!string.IsNullOrEmpty(dWeekString))
                    this.weeks = int.Parse(dWeekString); //BJP 19/02/2009

                string dDayString = match.Groups[durationDDCapturedName].Value;
                if (!string.IsNullOrEmpty(dDayString))
                    this.days = int.Parse(dDayString);

                // LMT 05/05/2009 Add nullability to all units EHR-900
                Check.Assert(!(this.years == null && this.months == null && this.weeks == null
                    && this.days == null), "Expect at least one of [years, months, weeks, days] to be non-null.");
            }

            string timeDuration = match.Groups[durationTTCapturedName].Value;
            if (!string.IsNullOrEmpty(timeDuration))
            {
                string dhhString = match.Groups[durationHHCapturedName].Value;
                string dmmString = match.Groups[durationMinuteCapturedName].Value;
                string dssString = match.Groups[durationSSCapturedName].Value;
                string dfsString = match.Groups[durationFSecondsCapturedName].Value;

                if (!string.IsNullOrEmpty(dhhString))
                    this.hours = int.Parse(dhhString);

                if (!string.IsNullOrEmpty(dmmString))
                    this.minutes = int.Parse(dmmString);

                if (!string.IsNullOrEmpty(dssString))
                    this.seconds = int.Parse(dssString);

                // TODO Use invariant culture to parse double.
                if (!string.IsNullOrEmpty(dfsString))
                    this.fractionalSecond = double.Parse(dfsString.Replace(',', '.'));

                // LMT 05/05/2009 Add nullability to all units EHR-900
                Check.Assert(!(this.hours == null && this.minutes == null
                    && this.seconds == null && this.fractionalSecond == null),
                    "Expect at least one of [years, months, weeks, days] to be non-null.");
            }
            // assign duration value in seconds
            GetDurationValue();
        }
        #endregion

        #region ToString
        /// <summary>Provides ISO-compliant string representation of duration</summary>
        /// <returns>String representation of duration</returns>
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("P");
            if (this.years != null)
                sb.Append(this.years.ToString() + "Y");
            if (this.months != null)
                sb.Append(this.months.ToString() + "M");
            if (this.weeks != null)
                sb.Append(this.weeks.ToString() + "W");
            if (this.days != null)
                sb.Append(this.days.ToString() + "D");
            if (this.hours != null || this.minutes != null || this.seconds != null
                || this.fractionalSecond != null)
            {
                sb.Append("T");
                if (this.hours != null)
                    sb.Append(this.hours.ToString() + "H");
                if (this.minutes != null)
                    sb.Append(this.minutes.ToString() + "M");
                if (this.seconds != null)
                {
                    sb.Append(this.seconds.ToString());
                    if (this.fractionalSecond > 0)
                        sb.Append(this.fractionalSecond.ToString().Substring(1));
                    sb.Append("S");
                }
            }

            // CM: 15/04/2008 should allow 0 duration. The ISO 8601:2004 specification indicates: 
            // "If the number of years, months, days, hours, minutes, or seconds in any of these expressions 
            // equals zero, the number and the designator may be absent; however, at least one number and its 
            // designator shall be present."
            string durationString = sb.ToString();
            if (durationString == "P")
                durationString = "PT0S"; //LMT 05/05/2009 if there's no preceding unit append PT0S

            Check.Ensure(ValidIso8601Duration(durationString), string.Format(
                "The generated duration string ({0}) is not a valid ISO duration string.",
                durationString));
            return durationString;
        }

        #endregion

        #region IComparable Members
        /// <summary>
        /// Compare the current date time instance with the obj. Returns 0 if they are equal, 
        /// -1 if the current instance less than obj, 1 if the current instance greater than obj.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            // precondition is that the current instance must be the same type as obj
            Check.Require(this.GetType() == obj.GetType(),
                "The current instance (" + this.GetType().ToString() + ") must be the same type as the obj("
                + obj.GetType().ToString() + ")");

            Iso8601Duration objDuration = obj as Iso8601Duration;

            if (this.durationValueInSeconds < objDuration.durationValueInSeconds)
            {
                return -1;
            }
            else if (this.durationValueInSeconds == objDuration.durationValueInSeconds)
            {
                return 0;
            }
            else
                return 1;
        }

        #endregion

        #region operator implementations
        // TODO: implements infix"<"(other: like Current): bool
        public static bool operator <(Iso8601Duration a, Iso8601Duration b)
        {
            DesignByContract.Check.Require((object)a != null && (object)b != null);

            return a.CompareTo(b) < 0;//((IComparable)a).CompareTo(b) < 0
        }

        public static bool operator >(Iso8601Duration a, Iso8601Duration b)
        {
            DesignByContract.Check.Require((object)a != null && (object)b != null);

            return a.CompareTo(b) > 0;// ((IComparable)a).CompareTo(b) > 0;
        }

        public static bool operator ==(Iso8601Duration a, Iso8601Duration b)
        {
            if ((object)a == null && (object)b == null)
                return true;

            else if ((object)a != null && (object)b != null)
            {
                return a.CompareTo(b) == 0; //((IComparable)a).CompareTo(b) == 0;
            }
            else
                return false;
        }

        public static bool operator !=(Iso8601Duration a, Iso8601Duration b)
        {
            if ((object)a == null && (object)b == null)
                return false;

            else if ((object)a != null && (object)b != null)
            {
                return a.CompareTo(b) != 0; //((IComparable)a).CompareTo(b) != 0;
            }
            else
                return true;
        }
        #endregion

        private int CompareTwoInt(int a, int b)
        {
            if (a > b)
                return 1;
            else if (a < b)
                return -1;
            else
                return 0;

        }

        /// <summary>
        /// Assign the duration value in seconds
        /// </summary>
        /// <returns></returns>
        private void GetDurationValue()
        {
            // LMT 05/05/2009 Add nullability to all units EHR-900
            double totalDaysInDate = 0.0d;
            if (this.years != null) totalDaysInDate += this.years.Value * nominalDaysInYear;
            if (this.months != null) totalDaysInDate += this.months.Value * nominalDaysInMonth;
            if (this.weeks != null) totalDaysInDate += this.weeks.Value * daysInWeek;
            if (this.days != null) totalDaysInDate += this.days.Value;

            // %HYYKA%
            // LMT changed again 05/05/2009 Add nullability to all units EHR-900
            //// CM changed 05/06/2007
            ////this.durationValueInSeconds = 
            ////  (((totalDaysInDate * hoursInDay + this.hours) + this.minutes) * secondsInMinute
            ////  + this.seconds + this.fractionalSecond);
            //this.durationValueInSeconds = (((totalDaysInDate * hoursInDay + this.hours)*minutesInHour + this.minutes) * secondsInMinute
            //    + this.seconds + this.fractionalSecond);

            double totalSecondsInTime = 0.0d;
            if (this.hours != null) totalSecondsInTime += this.hours.Value * minutesInHour * secondsInMinute;
            if (this.minutes != null) totalSecondsInTime += this.minutes.Value * secondsInMinute;
            if (this.seconds != null) totalSecondsInTime += this.seconds.Value;
            if (this.fractionalSecond != null) totalSecondsInTime += this.fractionalSecond.Value;

            this.durationValueInSeconds = totalSecondsInTime
                + (totalDaysInDate * hoursInDay * minutesInHour * secondsInMinute);

            Check.Ensure(this.durationValueInSeconds >= 0.0d, "Duration value should be greater than or equal to zero."); //Added by LMT 05/05/2009
        }

        internal static Iso8601Duration Normalise(Iso8601Duration duration)
        {
            Check.Require(duration != null, "duration must not be null.");
         
            int seconds = duration.Seconds;
            int minutes = duration.Minutes;
            int hours = duration.Hours;
            int days = duration.Days + duration.Weeks *7;
            int months = duration.Months;
            int years = duration.Years;
           
            if (seconds >= 60)
            {
                minutes += seconds / 60;
                seconds = seconds % 60;
            }
            if (minutes >= 60)
            {
                hours += minutes / 60;
                minutes = minutes % 60;
            }
            if (hours >= 24)
            {
                days += hours / 24;
                hours = hours % 24;
            }            
            if (months > 12)
            {
                years += months / 12;
                months = months % 12;
            }

            return new Iso8601Duration(years, months, days, 0,
                hours, minutes, seconds, duration.FractionalSecond);
        }
  
    }
}