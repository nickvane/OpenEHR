using System;
using System.Text.RegularExpressions;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;

namespace OpenEhr.AssumedTypes
{
    /// <summary>
    /// Represents an absolute point in time, specified to the second
    /// </summary>
    [Serializable]
    [RmType("openEHR", "SUPPORT", "ISO8601_DATE_TIME")]
    public class Iso8601DateTime : TimeDefinitions, IComparable
    {
        private int year = 0;
        private int month = 0;
        private int day = 0;
        private int hour = -1;
        private int minute = -1;
        private int second = -1;
        private double fractionalSecond = -1.0;
        private bool isExtended;
        private bool isDecimalSignComma = true;
        private Iso8601TimeZone timeZone;

        private const string yearCaptureName = "yyyy";
        private const string monthCaptureName = "mm";
        private const string dayCaptureName = "dd";
        private const string hourCaptureName = "hh";
        private const string minuteCaptureName = "min";
        private const string secondCaptureName = "ss";
        private const string decimalCaptureName = "sss"; // this can be decimal number of hh, mm, or ss.

        private const string dateCaptureName = "date";
        private const string timeCaptureName = "time";
        private const string iso8601TZoneCapturedName = "tZone";
        private const string tZoneHHCapturedName = "zhh";
        private const string tZoneMinCapturedMame = "zmm";
        private const string tZoneDirectionCapturedName = "tD";
        private const string utcTimeCapturedName = "utc";
        private const string gmtCapturedName = "gmt";

        //(^(?<date>(?<yyyy>\d{4})((?<mExtended>-)(?<mm>\d{2}))?((?<dExtended>-)(?<dd>\d{2}))?)(T(?<time>(?<hh>\d{2})((?<mmExtended>\:)(?<mm>\d{2}))?((?<ssExtended>\:)(?<ss>\d{2}))?(?<sss>\,\d+)?(?<timeZone>(?<utc>Z)|(?<gmt>(?<tD>\+|\-)(?<zhh>\d{2})(?<zmm>\d{2})))?))?$)|(^(?<date>(?<yyyy>\d{4})(?<mm>\d{2})?(?<dd>\d{2})?)(T(?<time>(?<hh>\d{2})(?<mm>(\d{2}))?(?<ss>\d{2})?(?<sss>\,\d+)?(?<timeZone>(?<utc>Z)|(?<gmt>(?<tD>\+|\-)(?<zhh>\d{2})(?<zmm>\d{2})))?))?$)

        //19890715T132430
        //19890715T132430,456

        //1989-01-01T18:04:00
        //1989-01-01T18:04:00Z
        //1989-01-01T18:04:00+1000
        //1989-01-01T18

        //19980909T132430

        /* ISO8601DateTime pattern. This pattern allows partial date and partial time.
         * It can do validation for month, day, hour, minute, seconds and timezone. It can
         * doesn't allow the mixtural of extended format with basic format. The timezone
         * only allowed when this datetime string has a valid time.
         * 
         * (^(?<yyyy>\d{4})((?<MM>0[1-9]|1[0-2])((?<dd>0[1-9]|1[0-9]|2[0-9]|3[01])
         * (T?(?<hh>([01][0-9])|(2[0-3]))((?<mm>[0-5][0-9])((?<ss>[0-5][0-9])
         * (?<fss>[.,]\d+)?)?)?)?)?)?
         * (?<timeZone>Z|[+\-](0[0-9]|1[0-2])(00|30)?)?$)
         * |(^(?<yyyy>\d{4})(-(?<MM>0[1-9]|1[0-2])(-(?<dd>0[1-9]|1[0-9]|2[0-9]|3[01])
         * (T(?<hh>([01][0-9])|(2[0-3]))(:(?<mm>[0-5][0-9])
         * (:(?<ss>[0-5][0-9](?<fss>[.,]\d+)?))?)?)?)?)?
         * (?<timeZone>Z|([+\-](0[0-9]|1[0-2])(:(00|30))?))?$)
         * 
         * */       

        // (^(\d{4})((0[1-9]|1[0-2])((0[1-9]|1[0-9]|2[0-9]|3[01])(T?(([01][0-9])|(2[0-]))(([0-5][0-9])(([0-5][0-9])([.,]\d+)?)?)?)?)?)?(Z|[+\-](0[0-9]|1[0-2])(00|30)?)?$)|(^(\d{4})(-(0[1-9]|1[0-2])(-(0[1-9]|1[0-9]|2[0-9]|3[01])(T(([01][0-9])|(2[0-3]))(:([0-5][0-9])(:([0-5][0-9]([.,]\d+)?))?)?)?)?)?(Z|([+\-](0[0-9]|1[0-2])(:(00|30))?))?$)

        private const string dateTimePattern = @"(^(?<" + yearCaptureName + @">\d{4})((?<"
            + monthCaptureName + @">0[1-9]|1[0-2])((?<"
            + dayCaptureName + @">0[1-9]|1[0-9]|2[0-9]|3[01])(T?(?<"
            + hourCaptureName + @">([01][0-9])|(2[0-3]))((?<"
            + minuteCaptureName + @">[0-5][0-9])((?<"
            + secondCaptureName + @">[0-5][0-9])(?<"+decimalCaptureName+@">[.,]\d+)?)?)?)?)?)?(?<"
            + iso8601TZoneCapturedName + @">(" + Iso8601TimeZone.timeZoneRegEx + @"))?$)|(^(?<"
            + yearCaptureName + @">\d{4})(-(?<" + monthCaptureName + @">0[1-9]|1[0-2])(-(?<"
            + dayCaptureName + @">0[1-9]|1[0-9]|2[0-9]|3[01])(T(?<"
            + hourCaptureName + @">([01][0-9])|(2[0-3]))(:(?<"
            + minuteCaptureName + @">[0-5][0-9])(:((?<"
            + secondCaptureName + @">[0-5][0-9])(?<" + decimalCaptureName + @">[.,]\d+)?))?)?)?)?)?(?<"
            // %HYYKA%
            // CM: 07/11/08 EHR-731 The Auckland NZ timezone including daylight saving results in a timezone of GMT+13 
            //+ iso8601TZoneCapturedName + @">Z|([+\-](0[0-9]|1[0-2])(:(00|30))?))?$)";
             //+ iso8601TZoneCapturedName + @">Z|([+\-](0[0-9]|1[0-3])(:(00|30))?))?$)";
            + iso8601TZoneCapturedName +">("+ Iso8601TimeZone.timeZoneRegEx + @"))?$)";


        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="dateTime"></param>
        public Iso8601DateTime(string dateTime)
        {
            ParseDateTime(dateTime);
            // TODO: assign value to asString?
        }

        public Iso8601DateTime(System.DateTime dateTime)
        {
            DesignByContract.Check.Require(dateTime != null);

            string dateTimeString;
            if (dateTime.Kind == DateTimeKind.Utc)
                // CM: 07/07/08 use DateTimeFormatInfo.InvariantInfo in order to generate string which is culture independent
                dateTimeString = dateTime.ToString("yyyyMMddTHHmmss,fffffff", System.Globalization.DateTimeFormatInfo.InvariantInfo) + "Z";
            else
                dateTimeString = dateTime.ToString("yyyyMMddTHHmmss,fffffffzzzz", System.Globalization.DateTimeFormatInfo.InvariantInfo);

            // %HYYKA%
            // the timezone is represented as +10:30. This is extended format. Change this to basic format: +1030
            // basic format: +1030
            // CM: 17/02/09
            ////HSL 7/11/07 Concerted this to regex as was failing on certain timezones
            //dateTimeString = regexTimeZone.Replace(dateTimeString, "$1$2");
            dateTimeString = ToIsoDateTime(dateTimeString);
          
            ParseDateTime(dateTimeString);

        }

        public Iso8601DateTime(int year, int month, int day, int hour, int minute,
            int second, double fractionalSecond, int timeZoneSign, int timeZoneHour,
            int timeZoneMinute)
        {
            //%HYYKA%
            // CM: 16/09/08 need to allow partial dateTime
            //Check.Require(ValidYear(year) && ValidMonth(month) && ValidDay(year, month, day) &&
            //    ValidHour(hour, minute, second) && ValidMinute(minute) && ValidSecond(second) &&
            //    ValidFractionalSecond(fractionalSecond));
            Check.Require(ValidYear(year), "Must be a valid year: "+year);
            Check.Require(month<=0 || ValidMonth(month), "Must be a valid month: " + month);
            Check.Require(day <= 0 || ValidDay(year, month, day), "Must be a valid day: " + day);
            Check.Require(hour < 0 || ValidHour(hour, minute, second), "Must be a valid hour: " + hour);
            Check.Require(minute < 0 || ValidMinute(minute), "Must be a valid minute: " + minute);
            Check.Require(second < 0 || ValidSecond(second), "Must be a valid second: " + second);
            Check.Require(fractionalSecond < 0 || ValidFractionalSecond(fractionalSecond), 
                "Must be a valid fractionalSection: " + fractionalSecond);

            Check.Require(timeZoneSign == 1 || timeZoneSign == -1);
            Check.Require(timeZoneHour >= 0 && timeZoneHour <= 12
                && timeZoneMinute >= 0 && timeZoneMinute <= 30);

            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            this.fractionalSecond = fractionalSecond;

            this.timeZone = new Iso8601TimeZone(timeZoneSign, timeZoneHour, timeZoneMinute);
        }
        public Iso8601DateTime(int year)
        {
            Check.Require(ValidYear(year), "The year (" + year + ") is not a valid ISO8601 year");

            this.year = year;

            this.timeZone = null;
        }

        public Iso8601DateTime(int year, int month)
        {
            Check.Require(ValidYear(year), "The year (" + year + ") is not a valid ISO8601 year.");
            Check.Require(ValidYear(month), "The month (" + month + ") is not a valid ISO8601 month.");

            this.year = year;
            this.month = month;
            this.timeZone = null;
        }

        public Iso8601DateTime(int year, int month, int day)
        {
            Check.Require(ValidYear(year), "The year (" + year + ") is not a valid ISO8601 year.");
            Check.Require(ValidMonth(month), "The month (" + month + ") is not a valid ISO8601 month.");
            Check.Require(ValidDay(year, month, day), "The month (" + day + ") is not a valid ISO8601 day.");

            this.year = year;
            this.month = month;
            this.day = day;
            this.timeZone = null;
        }


        public Iso8601DateTime(int year, int month, int day, int hr)
        {
            Check.Require(ValidYear(year) && ValidMonth(month) && ValidDay(year, month, day) &&
               ValidHour(hr, 0, 0));
            
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hr;
            
            this.timeZone = null;
        }

        public Iso8601DateTime(int year, int month, int day, int hour, int minute,
            int second)
        {
            Check.Require(ValidYear(year) && ValidMonth(month) && ValidDay(year, month, day) &&
                ValidHour(hour, minute, second) && ValidMinute(minute) && ValidSecond(second));

            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            this.timeZone = null;
        }
        public Iso8601DateTime(int year, int month, int day, int hour, int minute)
        {
            Check.Require(ValidYear(year) && ValidMonth(month) && ValidDay(year, month, day) &&
                ValidHour(hour, minute, second) && ValidMinute(minute));

            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.timeZone = null;
        }
        public Iso8601DateTime(int year, int month, int day, int hour, int minute,
            int second, double fractionalSecond)
        {
            Check.Require(ValidYear(year) && ValidMonth(month) && ValidDay(year, month, day) &&
                ValidHour(hour, minute, second) && ValidMinute(minute) && ValidSecond(second) &&
                ValidFractionalSecond(fractionalSecond));

            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            this.fractionalSecond = fractionalSecond;

            this.timeZone = null;
        }
        #endregion

        #region ValidIso8601DateTime
        /// <summary>
        /// True if the date time is a valid ISO 8601 date time.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static bool ValidIso8601DateTime(string dateTime)
        {
            Check.Require(!string.IsNullOrEmpty(dateTime),
                "date time string must not be null or empty.");

            Match thisMatch = Regex.Match(dateTime, dateTimePattern, RegexOptions.Compiled | RegexOptions.Singleline);

            if (!thisMatch.Success)
                return false;

            GroupCollection gCollection = thisMatch.Groups;

            // validate year value
            string yyyy = gCollection[yearCaptureName].Value;
            if (string.IsNullOrEmpty(yyyy) || yyyy=="0000")
                return false;
            int yearValue;
            if (!int.TryParse(yyyy, out yearValue))
                return false;
            if (!TimeDefinitions.ValidYear(yearValue))
                return false;

            // validate month value
            string MM = gCollection[monthCaptureName].Value;
            if (!string.IsNullOrEmpty(MM))
            {
                int monthValue;
                if (!int.TryParse(MM, out monthValue))
                    return false;
                if (!TimeDefinitions.ValidMonth(monthValue))
                    return false;

                // validate day value
                string dd = gCollection[dayCaptureName].Value;
                if (!string.IsNullOrEmpty(dd))
                {
                    int dayValue;
                    if (!int.TryParse(dd, out dayValue))
                        return false;
                    if (!TimeDefinitions.ValidDay(yearValue, monthValue, dayValue))
                        return false;

                    // validate time
                    // hours
                    string hString = gCollection[hourCaptureName].Value;
                    if (!string.IsNullOrEmpty(hString))
                    {
                        int hourValue;
                        if (!int.TryParse(hString, out hourValue))
                            return false;

                        // minutes
                        string minString = gCollection[minuteCaptureName].Value;
                        // if minute is not unknown
                        if (string.IsNullOrEmpty(minString))
                        {
                            if (!ValidHour(hourValue, 0, 0))
                                return false;
                        }
                        else 
                        {
                            int minuteValue;
                            if (!int.TryParse(minString, out minuteValue))
                                return false;
                            if (!ValidMinute(minuteValue))
                                return false;

                            // second
                            string sString = gCollection[secondCaptureName].Value;
                            // if second is not unknown
                            if (string.IsNullOrEmpty(sString))
                            {
                                if (!ValidHour(hourValue, minuteValue, 0))
                                    return false;
                            }
                            else 
                            {
                                int secondValue;
                                if (!int.TryParse(sString, out secondValue))
                                    return false;
                                if (!ValidSecond(secondValue))
                                    return false;
                                if (!ValidHour(hourValue, minuteValue, secondValue))
                                    return false;

                                // fractional seconds
                                string fsString = gCollection[decimalCaptureName].Value;
                                if (!string.IsNullOrEmpty(fsString))
                                {
                                    double fSecondValue;
                                    //Has to be a , or a . from the regex
                                    //We get problems here depending on the Number Format so we need to make sure the separator is the current culture
                                    fsString = string.Format("{0}{1}", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, fsString.Substring(1));
                                    if (!double.TryParse(fsString, out fSecondValue))
                                        return false;
                                    if (!ValidFractionalSecond(fSecondValue))
                                        return false;
                                }

                            }

                        }

                      }
                }
            }
            // validate time zone [Z|(+|-)hhmm]
            string tZoneStringValue = gCollection[iso8601TZoneCapturedName].Value;
            if (!string.IsNullOrEmpty(tZoneStringValue))
            {
                if (!Iso8601TimeZone.ValidIso8601TimeZone(tZoneStringValue))
                    return false;
            }
                   
            return true;

        }
        #endregion

        /// <summary>
        /// Take a DvDateTime and convert it to a System.DateTime
        /// </summary>
        /// <param name="dvDateTime"></param>
        /// <returns></returns>
        public static System.DateTime ToDateTime(Iso8601DateTime isoDateTime)
        {
            if (isoDateTime == null)
                return System.DateTime.MinValue; // All we can do, really!

            string timeString = isoDateTime.ToString(true, false);

            // pad out years < 1000
            int i = timeString.IndexOf('-');
            if (i < 0)
                i = timeString.Length;
            if (i < 4)
            {
                timeString = "000".Substring(0, 4 - i) + timeString;
            }

            // pad out partial datetime
            int timeDelimiter = timeString.IndexOf('T');

            if (timeDelimiter < 0)
            {
                if (timeString.Length < 5)
                    timeString += "-01";
                if (timeString.Length < 8)
                    timeString += "-01";
                timeString += "T00:00:00";
            }
            else
            {
                int timezone = timeString.IndexOfAny(new char[] { 'Z', 'z', '+', '-' }, timeDelimiter);
                int minuteDelimiter = timeString.IndexOf(':');
                if (minuteDelimiter < 0)
                {
                    if (timezone < 0)
                        timeString += ":00:00";
                    else
                        timeString = timeString.Substring(0, timeDelimiter + 3) + ":00:00" + timeString.Substring(timezone);
                }
                else
                {
                    if (timezone < 0)
                        i = timeString.IndexOf(':', minuteDelimiter + 1);
                    else
                        i = timeString.IndexOf(':', minuteDelimiter + 1, timezone - minuteDelimiter);

                    if (i < 0)
                    {
                        if (timezone < 0)
                            timeString += ":00";
                        else
                            timeString = timeString.Substring(0, minuteDelimiter + 3) + ":00" + timeString.Substring(timezone);
                    }
                }
            }
            System.DateTime dateTime = System.Xml.XmlConvert.ToDateTime(timeString,
                System.Xml.XmlDateTimeSerializationMode.RoundtripKind);

            // NOTE: timezone is only preserved when in local timezone or UTC. 
            // Unspecified timezone is assumed to be local
            // A date/time with non-local and non-UTC timezone will be converted into local time 
            // i.e the datetime value will be different and in the local time zone unless in 
            // local or UTC timezone (or unspecified)

            return dateTime;
        }

        static Regex dateTimeRegex = new Regex(dateTimePattern, RegexOptions.Compiled | RegexOptions.Singleline);

        #region ParseDateTime
        /// <summary>
        /// Parse the date time string, extracts values from the string and assign these 
        /// values to the class properties.
        /// </summary>
        /// <param name="dateTime"></param>
        private void ParseDateTime(string dateTime)
        {
            // %HYYKA%
            // The date time must be a valid ISO 8601 date time.
            //Check.Require(ValidIso8601DateTime(dateTime),
            //    "Date time string(" + dateTime + ") must be a valid ISO 8601 date time.");

            Match thisMatch = dateTimeRegex.Match(dateTime);

            Check.Require(thisMatch.Success,
                "Date time string (" + dateTime + ") must be a valid ISO 8601 date time.");

            GroupCollection gCollection = thisMatch.Groups;

            // assign values
            // year
            string yString = gCollection[yearCaptureName].Value;
            this.year = int.Parse(yString);

            // month
            string monthString = gCollection[monthCaptureName].Value;
            if (!string.IsNullOrEmpty(monthString))
            {
                this.month = int.Parse(monthString);
            }

            // day
            string dString = gCollection[dayCaptureName].Value;
            if (!string.IsNullOrEmpty(dString))
            {
                this.day = int.Parse(dString);
            }

            // CM: 11/05/08 fixed a bug
            if (!string.IsNullOrEmpty(gCollection[monthCaptureName].Value))
            {           
                string monthExtendedSign = dateTime.Substring(gCollection[monthCaptureName].Index - 1, 1);
                if (monthExtendedSign == "-")
                    this.isExtended = true;
                else
                    this.isExtended = false;
            }

            // time
            // assign values
            // hour
            string hString = gCollection[hourCaptureName].Value;
            if (!string.IsNullOrEmpty(hString))
            {
                this.hour = int.Parse(hString);

                // minutes
                string mString = gCollection[minuteCaptureName].Value;
                if (!string.IsNullOrEmpty(mString))
                {
                    this.minute = int.Parse(mString);
                    // seconds
                    string sString = gCollection[secondCaptureName].Value;
                    if (!string.IsNullOrEmpty(sString))
                    {
                        this.second = int.Parse(sString);
                        // fraction seconds
                        string fsString = gCollection[decimalCaptureName].Value;
                        if (!string.IsNullOrEmpty(fsString))
                        {
                            if (fsString.StartsWith(","))
                                this.isDecimalSignComma = true;
                            else
                                this.isDecimalSignComma = false;

                            if(fsString.Substring(0,1) != System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                            {
                                fsString = string.Format("{0}{1}",System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, fsString.Substring(1));
                            }
                            
                            this.fractionalSecond = double.Parse(fsString);
                        }
                    }
                }


                // time zone
                string tZoneString = gCollection[iso8601TZoneCapturedName].Value;
                if (!string.IsNullOrEmpty(tZoneString))
                    this.timeZone = new Iso8601TimeZone(tZoneString);
            }
        }
        #endregion

        /// <summary>
        /// Returns a date time string in ISO 8601 format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(this.IsExtended, this.IsDecimalSignComma);
        }

        public string ToString(bool isExtended, bool isDecimalSignComma)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            string dateString = this.year.ToString("000#");
            sb.Append(dateString);

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

            Check.Ensure(Iso8601Date.ValidIso8601Date(sb.ToString()), "Date string ("
                + sb.ToString() + ") is not a valid ISO 8601 date");

            if (this.HourUnknown)
                return sb.ToString();
            else
            {
                sb.Append("T");
                sb.Append(string.Format("{0:00}", this.hour));
                if (!MinuteUnknown)
                {
                    if (isExtended)
                        sb.Append(":");
                    sb.Append(string.Format("{0:00}", this.minute));
                    if (!SecondUnknown)
                    {
                        if (isExtended)
                            sb.Append(":");
                        sb.Append(string.Format("{0:00}", this.second));

                        if (HasFractionalSecond)
                        {
                            if (isDecimalSignComma)
                                sb.Append(",");
                            else
                                sb.Append(".");
                            sb.Append(this.fractionalSecond.ToString(".0######").Substring(1));
                        }
                    }

                }

                // Append time zone string
                if (this.timeZone != null)
                    sb.Append(this.timeZone.ToString(isExtended));

                Check.Ensure(ValidIso8601DateTime(sb.ToString()),
                     "The date time string (" + sb.ToString() + ") is not a valid Iso8601 time.");

                return sb.ToString();
            }
        }

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
                    throw new NotSupportedException("Month unknown.");
                return this.month;
            }

        }
        public int Day
        {
            get
            {
                if (this.DayUnknown)
                    throw new NotSupportedException("Day unknown.");
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
                if (MonthUnknown || DayUnknown || MinuteUnknown || SecondUnknown)
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
        public int Hour
        {
            get
            {
                if (this.HourUnknown)
                    throw new NotSupportedException("Hour unknown.");
                return this.hour;
            }

        }

        public int Minute
        {
            get
            {
                if (this.MinuteUnknown)
                    throw new NotSupportedException("Minute unknown.");
                return this.minute;

            }
        }
        public int Second
        {
            get
            {
                if (this.SecondUnknown)
                    throw new NotSupportedException("Second unknown.");
                return this.second;
            }

        }
        public double FractionalSecond
        {
            get
            {
                if (!this.HasFractionalSecond)
                    throw new NotSupportedException("Doesn't have fractional second.");
                return this.fractionalSecond;
            }

        }
        public Iso8601TimeZone Iso8601TimeZone
        {
            get { return this.timeZone; }

        }
        public bool HasFractionalSecond
        {
            get
            {
                if (this.fractionalSecond < 0)
                    return false;
                return true;
            }

        }

        public bool HourUnknown
        {
            get
            {
                if (this.hour < 0)
                    return true;
                else
                    return false;
            }
        }
        public bool MinuteUnknown
        {
            get
            {
                if (this.minute < 0)
                    return true;
                else
                    return false;
            }

        }
        public bool SecondUnknown
        {
            get
            {
                if (this.second < 0)
                    return true;
                else
                    return false;
            }

        }

        // TODO: check with Heath
        public bool IsDecimalSignComma
        {
            get
            {
                return this.isDecimalSignComma;
            }
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

            Iso8601DateTime objDateTime = obj as Iso8601DateTime;

            if (this.GetDateTimeSeconds() > objDateTime.GetDateTimeSeconds())
                return 1;
            else if (this.GetDateTimeSeconds() < objDateTime.GetDateTimeSeconds())
                return -1;
            else
                return 0;
        }

        #endregion
        #region operator implementations
        // TODO: implements infix"<"(other: like Current): bool
        public static bool operator <(Iso8601DateTime a, Iso8601DateTime b)
        {
            DesignByContract.Check.Require((object)a != null && (object)b != null);

            return a.CompareTo(b) < 0;//((IComparable)a).CompareTo(b) < 0
        }

        public static bool operator >(Iso8601DateTime a, Iso8601DateTime b)
        {
            DesignByContract.Check.Require((object)a != null && (object)b != null);

            return a.CompareTo(b) > 0;// ((IComparable)a).CompareTo(b) > 0;
        }

        public static bool operator ==(Iso8601DateTime a, Iso8601DateTime b)
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

        public static bool operator !=(Iso8601DateTime a, Iso8601DateTime b)
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

        internal double GetDateTimeSeconds()
        {
            double magnitude = -1.0;
            double daysInTotal = this.year * TimeDefinitions.nominalDaysInYear;
            if (!this.MonthUnknown)
            {
                daysInTotal += this.Month * TimeDefinitions.nominalDaysInMonth;
                if (!this.DayUnknown)
                    daysInTotal += this.Day;
            }
            
            // %HYYKA%
            //// TODO: check with Heath. CM: 5/6/7
            //daysInTotal = Math.Truncate(daysInTotal);

            magnitude = daysInTotal * TimeDefinitions.hoursInDay * TimeDefinitions.minutesInHour * TimeDefinitions.secondsInMinute;

            if (!this.HourUnknown)
            {
                // %HYYKA%
                // CM: 5/6/07
                //magnitude += this.Hour * TimeDefinitions.minutesInHour + TimeDefinitions.secondsInMinute;
                magnitude += this.Hour * TimeDefinitions.minutesInHour*TimeDefinitions.secondsInMinute;
                /////////////////////////
                if (!this.MinuteUnknown)
                    magnitude += this.Minute * TimeDefinitions.secondsInMinute;
                // CM: 5/6/7
                if (!this.SecondUnknown)
                    magnitude += this.second;
                if (this.HasFractionalSecond)
                    magnitude += this.FractionalSecond;

                // timezone
                if (this.Iso8601TimeZone != null)
                {
                    Iso8601TimeZone timezone = this.Iso8601TimeZone;
                    magnitude += this.Iso8601TimeZone.GetTimeZoneSeconds();
                }
            }

            if (magnitude < 0)
                throw new PostconditionException
                    ("Magnitude results in DvDateTime must not be smaller than zero.");
            return magnitude;
        }

        internal static string ToIsoDateTime(string dotNetDateTime)
        {
            return Regex.Replace(dotNetDateTime, @"([-+Zz]\d\d):(\d\d)", "$1$2", RegexOptions.Compiled | RegexOptions.Singleline);
        }

        #region Add/Subtract functions

        internal Iso8601DateTime Subtract(Iso8601Duration duration)
        {
            DesignByContract.Check.Require(duration != null, "duration must not be null.");

            Iso8601DateTime newDateTime = new Iso8601DateTime(this.ToString());

            Iso8601Duration normalisedDuration = Iso8601Duration.Normalise(duration);       
         

            if (normalisedDuration.FractionalSecond > 0)
            {
                if (newDateTime.SecondUnknown)
                    throw new NotSupportedException("Cannot subtract a duration with fractionalSeconds when the datetime seconds unknow.");

                if (newDateTime.HasFractionalSecond)
                    newDateTime.fractionalSecond -= normalisedDuration.FractionalSecond;
                else
                    newDateTime.fractionalSecond = (normalisedDuration.FractionalSecond) * -1;

                NormaliseSubtractedFractionalSecond(newDateTime);
            }


            if (normalisedDuration.Seconds > 0)
            {
                if (newDateTime.SecondUnknown)
                    throw new NotSupportedException("Cannot subtract a duration with seconds when the datetime seconds unknow.");
                newDateTime.second -= normalisedDuration.Seconds;
                NormaliseSubtractedSecond(newDateTime);
            }

            if (normalisedDuration.Minutes > 0)
            {
                if (newDateTime.MinuteUnknown)
                    throw new NotSupportedException("Cannot subtract a duration with minutes when the datetime minutes unknow.");
                newDateTime.minute -= normalisedDuration.Minutes;
                NormaliseSubtractedMinute(newDateTime);
            }


            if (normalisedDuration.Hours > 0)
            {
                if (newDateTime.HourUnknown)
                    throw new NotSupportedException("Cannot subtract a duration with hours when the datetime hour unknow.");
                newDateTime.hour -= normalisedDuration.Hours;
                NormaliseSubtractedHour(newDateTime);
            }

            if (normalisedDuration.Days > 0)
            {
                if (newDateTime.DayUnknown)
                    throw new NotSupportedException("Cannot subtract a duration with days when the datetime day unknow.");

                newDateTime.day -= normalisedDuration.Days;
                NormaliseSubtractedDay(newDateTime);
            }

            if (normalisedDuration.Months > 0)
            {
                if (newDateTime.MonthUnknown)
                    throw new NotSupportedException("Cannot subtract a duration with months when the datetime month unknow.");

                newDateTime.month -= normalisedDuration.Months;

                NormaliseSubtractedMonth(newDateTime);

                if (!newDateTime.DayUnknown &&(newDateTime.day>System.DateTime.DaysInMonth(newDateTime.year, newDateTime.month)))
                {
                    newDateTime.day = System.DateTime.DaysInMonth(newDateTime.year, newDateTime.month);
                }
            }

            if (normalisedDuration.Years > 0)
            {
                if (normalisedDuration.Years > newDateTime.year)
                    throw new ApplicationException("duration.Years must not greater than the dateTime.year");

                newDateTime.year -= normalisedDuration.Years;
            }

            return newDateTime;
        }       

        internal Iso8601DateTime Add(Iso8601Duration duration)
        {
            DesignByContract.Check.Require(duration != null, "duration must not be null.");

            Iso8601DateTime newDateTime = new Iso8601DateTime(this.ToString());

            Iso8601Duration normalisedDuration = Iso8601Duration.Normalise(duration);         

            if (normalisedDuration.FractionalSecond > 0)
            {
                if (newDateTime.SecondUnknown)
                    throw new NotSupportedException("Cannot add a duration with fractionalSeconds when the datetime seconds unknow.");

                if (newDateTime.HasFractionalSecond)
                {
                    newDateTime.fractionalSecond += normalisedDuration.FractionalSecond;
                    NormaliseFractionalSecond(newDateTime);
                }
                else
                    newDateTime.fractionalSecond = normalisedDuration.FractionalSecond;
            }

            if (normalisedDuration.Seconds > 0)
            {
                if (newDateTime.SecondUnknown)
                    throw new NotSupportedException("Cannot add a duration with seconds when the datetime seconds unknow.");
                newDateTime.second += normalisedDuration.Seconds;
                NormaliseSecond(newDateTime);
            }

            if (normalisedDuration.Minutes > 0)
            {
                if (newDateTime.MinuteUnknown)
                    throw new NotSupportedException("Cannot add a duration with minutes when the datetime minutes unknow.");
                newDateTime.minute += normalisedDuration.Minutes;
                NormaliseMinute(newDateTime);
            }


            if (normalisedDuration.Hours > 0)
            {
                if (newDateTime.HourUnknown)
                    throw new NotSupportedException("Cannot add a duration with hours when the datetime hour unknow.");
                newDateTime.hour += normalisedDuration.Hours;
                NormaliseHour(newDateTime);
                NormaliseDay(newDateTime);
            }
                     
            if (normalisedDuration.Months > 0)
            {
                if (newDateTime.MonthUnknown)
                    throw new NotSupportedException("Cannot add a duration with months when the datetime month unknow.");
                newDateTime.month += normalisedDuration.Months;

                NormaliseMonth(newDateTime);

                if (normalisedDuration.Days <= 0)
                {
                    if (!newDateTime.DayUnknown && (newDateTime.day > System.DateTime.DaysInMonth(newDateTime.year, newDateTime.month)))
                    {
                        newDateTime.day = System.DateTime.DaysInMonth(newDateTime.year, newDateTime.month);
                    }
                }
            }

            if (normalisedDuration.Years > 0)
                newDateTime.year += normalisedDuration.Years;
           

            if (normalisedDuration.Days > 0)
            {
                if (newDateTime.DayUnknown)
                    throw new NotSupportedException("Cannot add a duration with days when the datetime day unknow.");

                newDateTime.day += normalisedDuration.Days;
                NormaliseDay(newDateTime);
            }
           
            return newDateTime;
          
        }

        private static void NormaliseFractionalSecond(Iso8601DateTime isoDateTime)
        {
            DesignByContract.Check.Require(!isoDateTime.MinuteUnknown, "isoDateTime.MinuteUnknow must be false.");
            DesignByContract.Check.Require(!isoDateTime.SecondUnknown, "isoDateTime.SecondUnknown must be false.");
            DesignByContract.Check.Require(isoDateTime.HasFractionalSecond, "isoDateTime.HasFractionalSecond must be true.");

            Time time = new Time(isoDateTime.hour, isoDateTime.minute, isoDateTime.second, isoDateTime.fractionalSecond);
            time.NormaliseFractionalSecond();

            isoDateTime.hour = time.Hour;
            isoDateTime.minute = time.Minute;
            isoDateTime.second = time.Second;
            isoDateTime.fractionalSecond = time.FractionalSeconds;

            NormaliseHour(isoDateTime);

        }

        private static void NormaliseSubtractedHour(Iso8601DateTime isoDateTime)
        {
            if (isoDateTime.hour < 0)
            {
                isoDateTime.hour += 24;
                isoDateTime.day--;
                NormaliseSubtractedDay(isoDateTime);
            }

        }      

        private static void NormaliseMinute(Iso8601DateTime isoDateTime)
        {
            DesignByContract.Check.Require(!isoDateTime.HourUnknown, "isoDateTime.HourUnknown must be false.");
            DesignByContract.Check.Require(!isoDateTime.MinuteUnknown, "isoDateTime.MinuteUnknow must be false.");

            Time time = new Time(isoDateTime.hour, isoDateTime.minute);
            time.NormaliseMinute();

            isoDateTime.hour = time.Hour;
            isoDateTime.minute = time.Minute;

            NormaliseHour(isoDateTime);

        }

        private static void NormaliseHour(Iso8601DateTime isoDateTime)
        {
            if (isoDateTime.hour >= 24)
            {
                isoDateTime.day += isoDateTime.hour / 24;
                isoDateTime.hour = isoDateTime.hour % 24;
                NormaliseDay(isoDateTime);
            }
        }

       
        private static void NormaliseSecond(Iso8601DateTime isoDateTime)
        {
            DesignByContract.Check.Require(!isoDateTime.HourUnknown, "isoDateTime.HourUnknown must be false.");
            DesignByContract.Check.Require(!isoDateTime.MinuteUnknown, "isoDateTime.MinuteUnknow must be false.");
            DesignByContract.Check.Require(!isoDateTime.SecondUnknown, "isoDateTime.SecondUnknown must be false.");

            Time time = new Time(isoDateTime.hour, isoDateTime.minute, isoDateTime.second);
            time.NormaliseSecond();

            isoDateTime.hour = time.Hour;
            isoDateTime.minute = time.Minute;
            isoDateTime.second = time.Second;

            NormaliseHour(isoDateTime);
        }

        private static void NormaliseSubtractedSecond(Iso8601DateTime isoDateTime)
        {
            DesignByContract.Check.Require(!isoDateTime.HourUnknown, "isoDateTime.HourUnknown must be false.");

            Time time = new Time(isoDateTime.hour, isoDateTime.minute, isoDateTime.second);
            time.NormaliseSubtractedSecond();

            isoDateTime.hour = time.Hour;
            isoDateTime.minute = time.Minute;
            isoDateTime.second = time.Second;

            NormaliseSubtractedHour(isoDateTime);

        }

        private static void NormaliseSubtractedFractionalSecond(Iso8601DateTime isoDateTime)
        {
            DesignByContract.Check.Require(!isoDateTime.HourUnknown, "isoDateTime.HourUnknown must be false.");

            Time time = new Time(isoDateTime.hour, isoDateTime.minute, isoDateTime.second, isoDateTime.fractionalSecond);
            time.NormaliseSubtractedFractionalSecond();

            isoDateTime.hour = time.Hour;
            isoDateTime.minute = time.Minute;
            isoDateTime.second = time.Second;
            isoDateTime.fractionalSecond = time.FractionalSeconds;

            NormaliseSubtractedHour(isoDateTime);

        }

        private static void NormaliseSubtractedMinute(Iso8601DateTime isoDateTime)
        {
            DesignByContract.Check.Require(!isoDateTime.HourUnknown, "isoDateTime.HourUnknown must be false.");

            Time time = new Time(isoDateTime.hour, isoDateTime.minute);
            time.NormaliseSubtractedMinutes();

            isoDateTime.hour = time.Hour;
            isoDateTime.minute = time.Minute;

            NormaliseSubtractedHour(isoDateTime);
        }

        private static void NormaliseDay(Iso8601DateTime isoDateTime)
        {
            DesignByContract.Check.Require(!isoDateTime.MonthUnknown, "isoDateTime monthUnknown must be false.");
            DesignByContract.Check.Require(!isoDateTime.DayUnknown, "isoDateTime dayUnknown must be false.");

            Date date = new Date(isoDateTime.year, isoDateTime.month, isoDateTime.day);
            date.NormaliseDay();
            isoDateTime.year = date.Year;
            isoDateTime.month = date.Month;
            isoDateTime.day = date.Day;
        }

        private static void NormaliseMonth(Iso8601DateTime isoDateTime)
        {
            DesignByContract.Check.Require(!isoDateTime.MonthUnknown, "isoDateTime monthUnknown must be false.");
            
            Date date = new Date(isoDateTime.year, isoDateTime.month);
            date.NormaliseMonth();
            isoDateTime.year = date.Year;
            isoDateTime.month = date.Month;            
        }

        private static void NormaliseSubtractedMonth(Iso8601DateTime isoDateTime)
        {           
            Date date = new Date(isoDateTime.year, isoDateTime.month);
            date.NormaliseSubtractedMonth();
            isoDateTime.year = date.Year;
            isoDateTime.month = date.Month;    
        }

        private static void NormaliseSubtractedDay(Iso8601DateTime isoDateTime)
        {
            DesignByContract.Check.Require(!isoDateTime.MonthUnknown, "isoDateTime monthUnknown must be false.");
            
            Date date = new Date(isoDateTime.year, isoDateTime.month, isoDateTime.day);
            date.NormaliseSubtractedDay();
            isoDateTime.year = date.Year;
            isoDateTime.month = date.Month;
            isoDateTime.day = date.Day;
        }    
      
        #endregion
    }
}
