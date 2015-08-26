using System;
using System.Text.RegularExpressions;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;

namespace OpenEhr.AssumedTypes
{
    /// <summary>
    /// Represents an absolute point in time, as measured on the Gregorian calendar, 
    /// and specified only to the day.
    /// </summary>
    [Serializable]
    [RmType("openEHR", "SUPPORT", "ISO8601_DATE")]
    public class Iso8601Date : TimeDefinitions, IComparable
    {
        private string asString;
        private int year = -1;
        private int month = -1;
        private int day = -1;
        //private bool isPartial;
        private bool isExtended;

        // Date format patterns
        private const string yearCaptureName = "yyyy";
        private const string monthCaptureName = "mm";
        private const string dayCaptureName = "dd";
        private const string monthExtendedCaptureName = "mExtended";
        private const string dayExtendedCaptureName = "dExtended";
        // ^(?<yyyy>\d{4})(((?<mm>\d{2})?(?<dd>\d{2})?)|((?<mExtended>-)(?<mm>\d{2}))?((?<dExtended>-)(?<dd>\d{2}))?)?$

        /* ISO8601Date pattern
         * (^(?<yyyy>\d{4})((?<MM>0[1-9]|1[0-2])((?<dd>0[1-9]|1[0-9]|2[0-9]|3[01]))?)?$)
         * |(^(?<yyyy>\d{4})(-(?<MM>0[1-9]|1[0-2])(-(?<dd>0[1-9]|1[0-9]|2[0-9]|3[01]))?)?$)
         * */

        private const string datePattern = @"(^(?<" + yearCaptureName + @">\d{4})((?<"
            + monthCaptureName + @">0[1-9]|1[0-2])((?<"
            + dayCaptureName + @">0[1-9]|1[0-9]|2[0-9]|3[01]))?)?$)|(^(?<"
            + yearCaptureName + @">\d{4})(-(?<" + monthCaptureName + @">0[1-9]|1[0-2])(-(?<"
            + dayCaptureName + @">0[1-9]|1[0-9]|2[0-9]|3[01]))?)?$)";

        // TODO: implements infix"<"(other: like Current): bool
        public static bool operator <(Iso8601Date a, Iso8601Date b)
        {
            DesignByContract.Check.Require((object)a != null && (object)b != null);

            return a.CompareTo(b) < 0;
        }

        public static bool operator >(Iso8601Date a, Iso8601Date b)
        {
            DesignByContract.Check.Require((object)a != null && (object)b != null);

            return a.CompareTo(b) > 0;
        }

        public static bool operator ==(Iso8601Date a, Iso8601Date b)
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

        public static bool operator !=(Iso8601Date a, Iso8601Date b)
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

        #region constructors
        /// <summary>
        /// Constructor. The date string is validated in the constructor
        /// </summary>
        /// <param name="dateString"></param>
        public Iso8601Date(string dateString)
        {
            ParseDate(dateString);
            this.asString = dateString;
        }
        public Iso8601Date(System.DateTime dateTime)
        {
            Check.Require(dateTime != null, "the date time instance must not be null.");

            // %HYYKA%
            // CM: 07/07/08 use DateTimeFormatInfo.InvariantInfo in order to generate string which is culture independent
            //string dateString = dateTime.ToString("yyyyMMdd");
            string dateString = dateTime.ToString("yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo);

            ParseDate(dateString);
        }

        public Iso8601Date(int year, int month, int day)
        {
            Check.Require(ValidYear(year), "Year must be a valid: " + year);
            Check.Require(month <= 0 || ValidMonth(month), "Invalid month: " + month);
            Check.Require(day <= 0 || ValidDay(year, month, day), "Invalid day: " + day);

            this.year = year;
            this.month = month;
            this.day = day;
        }

        #endregion

        #region ValidIso8601Date
        /// <summary>
        /// True is the date string is a valid ISO 8601 date.
        /// </summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        public static bool ValidIso8601Date(string dateString)
        {
            Match thisMatch = Regex.Match(dateString, datePattern, RegexOptions.Compiled | RegexOptions.Singleline);
            if (!thisMatch.Success)
                return false;

            GroupCollection gCollection = thisMatch.Groups;

            // get year value
            string yString = gCollection[yearCaptureName].Value;
            if (string.IsNullOrEmpty(yString))
                return false;

            int yearValue;
            if (!int.TryParse(yString, out yearValue))
                return false;

            // get month value
            string mString = gCollection[monthCaptureName].Value;
            int monthValue = 0;
            if (!string.IsNullOrEmpty(mString))
            {
                if (!int.TryParse(mString, out monthValue))
                    return false;
            }
            // get day
            string dString = gCollection[dayCaptureName].Value;
            if (!string.IsNullOrEmpty(dString))
            {
                int dayValue;
                if (!int.TryParse(dString, out dayValue))
                    return false;
                if (!ValidDay(yearValue, monthValue, dayValue))
                    return false;
            }
            return true;
        }
        #endregion


        #region ParseDate
        /// <summary>
        /// If the date string is a valid ISO 8601 date, this method extract the 
        /// values of year, month, and day and assign these values to the class properties.
        /// </summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        private void ParseDate(string dateString)
        {
            Check.Require(ValidIso8601Date(dateString),
                "Date string (" + dateString + ") must be a valid ISO 8601 date.");

            Match thisMatch = Regex.Match(dateString, datePattern, RegexOptions.Compiled | RegexOptions.Singleline);

            GroupCollection gCollection = thisMatch.Groups;

            // assign values
            // year
            string yString = gCollection[yearCaptureName].Value;
            this.year = int.Parse(yString);

            // month
            string mString = gCollection[monthCaptureName].Value;
            if (!string.IsNullOrEmpty(mString))
            {
                this.month = int.Parse(mString);
            }

            // day
            string dString = gCollection[dayCaptureName].Value;
            if (!string.IsNullOrEmpty(dString))
            {
                this.day = int.Parse(dString);
            }

            // extended format
            if (dateString.IndexOf("-") >= 0)
                this.isExtended = true;
            else
                this.isExtended = false;
        }
        #endregion

        #region ToString
        /// <summary>
        /// returns a date string in the ISO 8601 date format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(this.isExtended);
        }
        #endregion


        #region class properties

        public int Year
        {
            get
            {
                if (this.year <= 0)
                    throw new FormatException("The year value cannot be unknown or zero.");
                return this.year;
            }

        }
        public int Month
        {
            get
            {
                if (this.MonthUnknown)
                    return 0;
                return this.month;
            }

        }
        public int Day
        {
            get
            {
                if (this.DayUnknown)
                    return 0;
                return this.day;
            }


        }
        public bool MonthUnknown
        {
            get
            {
                if (this.month <= 0)
                    return true;
                else
                    return false;
            }

        }
        public bool DayUnknown
        {
            get
            {
                if (this.day <= 0)
                    return true;
                else
                    return false;
            }

        }
        public bool IsPartial
        {
            get
            {
                if (MonthUnknown || DayUnknown)
                {
                    return true;
                }
                else
                    return false;
            }

        }
        public bool IsExtended
        {
            get { return this.isExtended; }

        }
        #endregion



        #region IComparable Members
        /// <summary>
        /// Compare the current date instance with the obj. Returns 0 if they are equal, 
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

            Iso8601Date objDate = obj as Iso8601Date;

            // compare yyyy
            if (this.year < objDate.year)
                return -1;
            else if (this.year > objDate.year)
                return 1;

            // if yyyy is the same, compare month
            if (this.MonthUnknown && objDate.MonthUnknown)
                return 0;
            else if ((!this.MonthUnknown && objDate.MonthUnknown)
                || (this.MonthUnknown && !objDate.MonthUnknown))
                throw new FormatException
                    ("An unknown month cannot be compared with a valid month value.");
            // if the two dates have month value
            else
            {
                if (this.month > objDate.month)
                    return 1;
                else if (this.month < objDate.month)
                    return -1;
                else // if the month is the same, compare day
                {
                    if (this.DayUnknown && objDate.DayUnknown)
                        return 0;
                    else if ((!this.DayUnknown && objDate.DayUnknown) ||
                        (this.DayUnknown && !objDate.DayUnknown))
                        throw new FormatException
                            ("An unknown day cannot be compared with a valid day value.");
                    else
                    {
                        if (this.day > objDate.day)
                            return 1;
                        else if (this.day < objDate.day)
                            return -1;
                        else
                            return 0;

                    }
                }
            }
        }

        #endregion
        // TODO: need to be implemented
        public string ToString(bool isExtended)
        {
            string dateString = this.year.ToString("000#");
            System.Text.StringBuilder sb = new System.Text.StringBuilder(dateString);

            if (!MonthUnknown)
            {
                if (isExtended)
                    sb.Append("-");
                sb.Append(string.Format("{0:00}", this.month));
                if (!DayUnknown)
                {
                    if (isExtended)
                        sb.Append("-");
                    sb.Append(string.Format("{0:00}", this.day));

                }

            }
            Check.Ensure(ValidIso8601Date(sb.ToString()), "Date string ("
                + sb.ToString() + ") is not a valid ISO 8601 date");

            return sb.ToString();
        }

        #region Add/Subtract functions

        internal Iso8601Date Subtract(Iso8601Duration duration)
        {
            DesignByContract.Check.Require(duration != null, "duration must not be null.");

            Iso8601Date newDate = new Iso8601Date(this.ToString());

            Iso8601Duration normalisedDuration = Iso8601Duration.Normalise(duration);           

            if (normalisedDuration.Days > 0)
            {
                if (newDate.DayUnknown)
                    throw new NotSupportedException("Cannot subtract a duration with days when the datetime day unknow.");

                newDate.day -= normalisedDuration.Days;
                NormaliseSubtractedDay(newDate);
            }

            if (normalisedDuration.Months > 0)
            {
                if (newDate.MonthUnknown)
                    throw new NotSupportedException("Cannot subtract a duration with months when the datetime month unknow.");

                newDate.month -= normalisedDuration.Months;
                
                NormaliseSubtractedMonth(newDate);

                if (!newDate.DayUnknown &&(newDate.day>System.DateTime.DaysInMonth(newDate.year, newDate.month)))
                {
                    newDate.day = System.DateTime.DaysInMonth(newDate.year, newDate.month);
                }
            }

            if (normalisedDuration.Years > 0)
            {
                if (normalisedDuration.Years > newDate.year)
                    throw new ApplicationException("duration.Years must not greater than the dateTime.year");

                newDate.year -= normalisedDuration.Years;
            }

            return newDate;
        }      

        internal Iso8601Date Add(Iso8601Duration duration)
        {
            DesignByContract.Check.Require(duration != null, "duration must not be null.");

            Iso8601Date newDate = new Iso8601Date(this.ToString());

            Iso8601Duration normalisedDuration = Iso8601Duration.Normalise(duration);          

            if (normalisedDuration.Months > 0)
            {
                if (newDate.MonthUnknown)
                    throw new NotSupportedException("Cannot add a duration with months when the datetime month unknow.");
                newDate.month += normalisedDuration.Months;

                NormaliseMonth(newDate);

                if (normalisedDuration.Days <= 0)
                {
                    if (!newDate.DayUnknown && newDate.day > System.DateTime.DaysInMonth(newDate.year, newDate.month))
                        newDate.day = System.DateTime.DaysInMonth(newDate.year, newDate.month);
                }
            }

            if (normalisedDuration.Years > 0)
                newDate.year += normalisedDuration.Years;

            if (normalisedDuration.Days > 0)
            {
                if (newDate.DayUnknown)
                    throw new NotSupportedException("Cannot add a duration with days when the datetime day unknow.");

                newDate.day += normalisedDuration.Days;
                NormaliseDay(newDate);
            }

            return newDate;

        }

        private static void NormaliseSubtractedMonth(Iso8601Date isoDate)
        {
            Date date = new Date(isoDate.year, isoDate.month);
            date.NormaliseSubtractedMonth();

            isoDate.year = date.Year;
            isoDate.month = date.Month;
        }

        private static void NormaliseSubtractedDay(Iso8601Date isoDate)
        {
            DesignByContract.Check.Require(!isoDate.MonthUnknown, "isoDate monthUnknown must be false.");

            Date date = new Date(isoDate.year, isoDate.month, isoDate.day);
            date.NormaliseSubtractedDay();

            isoDate.year = date.Year;
            isoDate.month = date.Month;
            isoDate.day = date.Day;
        }

        private static void NormaliseMonth(Iso8601Date isoDate)
        {
            DesignByContract.Check.Require(!isoDate.MonthUnknown, "isoDate monthUnknown must be false.");

            Date date = new Date(isoDate.year, isoDate.month);
            date.NormaliseMonth();

            isoDate.year = date.Year;
            isoDate.month = date.Month;
        }

        private static void NormaliseDay(Iso8601Date isoDate)
        {
            DesignByContract.Check.Require(!isoDate.MonthUnknown, "isoDate monthUnknown must be false.");
            DesignByContract.Check.Require(!isoDate.DayUnknown, "isoDate dayUnknown must be false.");

            Date date = new Date(isoDate.Year, isoDate.Month, isoDate.Day);
            date.NormaliseDay();

            isoDate.year = date.Year;
            isoDate.month = date.Month;
            isoDate.day = date.Day;
        }
     
        #endregion
    }

    internal class Date
    {       
        internal Date(int year, int month)
        {
            DesignByContract.Check.Require(year > 0, "year must be greater than zero.");

            this.year = year;
            this.month = month;
            monthSet = true;
            yearSet = true;
        }


        internal Date(int year, int month, int day)
        {
            DesignByContract.Check.Require(year > 0, "year must be greater than zero.");

            this.year = year;
            yearSet = true;
            this.month = month;
            monthSet = true;
            this.day = day;
            daySet = true;
        }

        private int day;
        bool daySet;
        private int month;
        bool monthSet;
        private int year;
        bool yearSet;

        internal int Year
        {
            get
            {
                DesignByContract.Check.Require(yearSet, "year must have been set.");

                return this.year;
            }
        }

        internal int Month
        {
            get
            {
                DesignByContract.Check.Require(monthSet, "month must have been set.");

                return this.month;
            }
        }

        internal int Day
        {
            get
            {
                DesignByContract.Check.Require(daySet, "day must have been set.");

                return this.day;
            }
        }

        internal void NormaliseSubtractedMonth()
        {
            DesignByContract.Check.Require(yearSet && monthSet, "year and month value must have been set.");

            if (month == 0)
            {
                month = 12;
                year--;
            }

            if (month < 0)
            {
                year--;
                month = month + 12;

                if (month <= 0)
                    NormaliseSubtractedMonth();
            }
        }


        internal void NormaliseMonth()
        {
            DesignByContract.Check.Require(yearSet && monthSet, "year and month value must have been set.");
            DesignByContract.Check.Require(this.month > 0, "month must be greater than zero.");

            if (month > 12)
            {
                year += month / 12;
                month = month % 12;
            }

        }

        internal void NormaliseDay()
        {
            DesignByContract.Check.Require(yearSet && monthSet && daySet, "year, month and day value must have been set.");
            DesignByContract.Check.Require(this.month > 0, "month must be greater than zero.");
            DesignByContract.Check.Require(this.day > 0, "day must be greater than zero.");

            int daysInMonth = System.DateTime.DaysInMonth(year, month);
            if (day > daysInMonth)
            {
                while (day > daysInMonth)
                {
                    day = day - daysInMonth;
                    month++;
                    NormaliseMonth();

                    daysInMonth = System.DateTime.DaysInMonth(year, month);
                }
            }
        }


        internal void NormaliseSubtractedDay()
        {
            DesignByContract.Check.Require(yearSet && monthSet && daySet, "year, month, and day value must have been set.");
           
            if (day > System.DateTime.DaysInMonth(year, month))
                day = System.DateTime.DaysInMonth(year, month);

            if (day == 0)
            {
                if (month == 1)
                {
                    month = 12;
                    year--;
                }
                else
                    month--;

                int daysInMonth = System.DateTime.DaysInMonth(year, month);

                day = daysInMonth;

            }

            else if (day < 0)
            {
                if (month == 1)
                {
                    month = 12;
                    year--;
                }
                else
                    month--;

                int daysInMonth = System.DateTime.DaysInMonth(year, month);
                day += daysInMonth;

                if (day <= 0)
                    NormaliseSubtractedDay();
            }

        }

        internal static Iso8601Duration NormaliseDuration(int yearsDiff, int monthsDiff, int weeksDiff,
            int daysDiff, int hoursDiff, int minutesDiff, int secondsDiff, double fractionalSecondsDiff, 
            int daysInMonth)
        {
            if (fractionalSecondsDiff < 0)
            {
                fractionalSecondsDiff += 1;
                secondsDiff -= 1;
            }

            if (secondsDiff < 0)
            {
                secondsDiff += Iso8601DateTime.secondsInMinute;
                minutesDiff--;
            }

            if (minutesDiff < 0)
            {
                minutesDiff += Iso8601DateTime.minutesInHour;
                hoursDiff--;
            }

            if (hoursDiff < 0)
            {
                hoursDiff += Iso8601DateTime.hoursInDay;
                daysDiff--;
            }

            if (daysDiff < 0)
            {
                daysDiff += daysInMonth;

                monthsDiff--;
            }

            if (monthsDiff < 0)
            {
                monthsDiff += Iso8601DateTime.monthsInYear;

                if (yearsDiff <= 0)
                    throw new ApplicationException("yearsDiff must greater than 1.");
                yearsDiff--;
            }

            if (daysDiff > 0 && daysDiff % Iso8601DateTime.daysInWeek == 0)
            {
                weeksDiff = daysDiff / Iso8601DateTime.daysInWeek;
                daysDiff = 0;
            }

            Iso8601Duration diff = new Iso8601Duration(yearsDiff, monthsDiff, daysDiff, weeksDiff, hoursDiff, minutesDiff, secondsDiff, fractionalSecondsDiff);

            return diff;

        }
    }
}
