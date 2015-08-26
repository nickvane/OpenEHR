using System;
using System.Text.RegularExpressions;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;

namespace OpenEhr.AssumedTypes
{
    /// <summary>
    /// Represents an absolute point in time from an origin usually interpreted as 
    /// meaning the start of the current day, specified to the second.
    /// </summary>
    [Serializable]
    [RmType("openEHR", "SUPPORT", "ISO8601_TIME")]
    public class Iso8601Time : TimeDefinitions, IComparable
    {
        private string asString;
        private int hour = -1;
        private int minute = -1;
        private int second = -1;
        private double fractionalSecond = -1.0;
        private Iso8601TimeZone timezone;

        private bool isExtended;
        private bool isDecimalComma = true;

        private const string hourCaptureName = "hh";
        private const string minuteCaptureName = "mm";
        private const string secondCaptureName = "ss";
        private const string decimalCaptureName = "sss"; // this can be decimal number of hh, mm, or ss.

        private const string mmExtendedCaptureName = "mmExtended";
        private const string ssExtendedCaptureName = "ssExtended";

        private const string iso8601TZoneCapturedName = "tZone";
        private const string tZoneHHCapturedName = "zhh";
        private const string tZoneMinCapturedMame = "zmm";
        private const string tZoneDirectionCapturedName = "tD";
        private const string utcTimeCapturedName = "utc";
        private const string gmtCapturedName = "gmt";

        // %HYYKA%
        //^(?<hh>\d{2})((((?<mm>(\d{2}))?(?<ss>\d{2})?))|((?<mmExtended>\:)(?<mm>\d{2})(?<ssExtended>\:)?(?<ss>\d{2})?))(?<sss>\,\d+)?((?<tZone>(?<utc>Z)|(?<gmt>(?<tD>\+|\-)(?<zhh>\d{2})(?<zmm>\d{2}))))?$
        //^(?<hh>\d{2})(?<mmExtended>\:)?(?<mm>(\d{2}))?(?<ssExtended>\:)?(?<ss>\d{2})?
        //(?<sss>\,\d+)?((?<tZone>(?<utc>Z)|(?<gmt>(?<tD>\+|\-)(?<zhh>\d{2})(?<zmm>\d{2}))))?$
        //private const string timePattern = @"^(?<" + hourCaptureName + @">\d{2})((((?<"
        //    + minuteCaptureName + @">(\d{2}))?(?<" + secondCaptureName + @">\d{2})?))|((?<"
        //    + mmExtendedCaptureName + @">\:)(?<" + minuteCaptureName + @">\d{2})(?<"
        //    + ssExtendedCaptureName + @">\:)?(?<" + secondCaptureName + @">\d{2})?))(?<"
        //    + decimalCaptureName + @">\,\d+)?((?<" + iso8601TZoneCapturedName + @">(?<"
        //    + utcTimeCapturedName + @">Z)|(?<" + gmtCapturedName + @">(?<"
        //    + tZoneDirectionCapturedName + @">\+|\-)(?<" + tZoneHHCapturedName + @">\d{2})(?<"
        //    + tZoneMinCapturedMame + @">\d{2}))))?$";

        /* ISO8601Time pattern. This pattern validates the value for hh, min, ss and timezone.
         * It also validate the mixtural of basic format and extended format.
         * (^(?<hh>([01][0-9])|(2[0-3]))((?<mm>[0-5][0-9])
         * ((?<ss>[0-5][0-9])(?<fss>[.,]\d+)?)?)?
         * (?<timeZone>(Z|[+\-](0[0-9]|1[0-2])(00|30)?))?$)|
         * (^(?<hh>([01][0-9])|(2[0-3]))(:(?<mm>[0-5][0-9])
         * (:(?<ss>[0-5][0-9])(?<fss>[.,]\d+)?)?)?
         * (?<timeZone>(Z|[+\-](0[0-9]|1[0-2])(:(00|30))?))?$)
         **/

        // (^(([01][0-9])|(2[0-3]))(([0-5][0-9])(([0-5][0-9])([.,]\d+)?)?)?((Z|[+\-](0[0-9]|1[0-2])(00|30)?))?$)|(^(([01][0-9])|(2[0-3]))(:([0-5][0-9])(:([0-5][0-9])([.,]\d+)?)?)?((Z|[+\-](0[0-9]|1[0-2])(:(00|30))?))?$)
        private const string timePattern = @"(^(?<"
            +hourCaptureName+@">([01][0-9])|(2[0-3]))((?<"
            +minuteCaptureName+@">[0-5][0-9])((?<"
            +secondCaptureName+@">[0-5][0-9])(?<"
            +decimalCaptureName+@">[.,]\d+)?)?)?(?<"
            + iso8601TZoneCapturedName + @">(" + Iso8601TimeZone.timeZoneRegEx + @"))?$)|(^(?<"
            +hourCaptureName+@">([01][0-9])|(2[0-3]))(:(?<"
            +minuteCaptureName+@">[0-5][0-9])(:(?<"
            +secondCaptureName+@">[0-5][0-9])(?<"
            +decimalCaptureName+@">[.,]\d+)?)?)?(?<"
            // %HYYKA%
            // CM: 07/11/08 EHR-731 The Auckland NZ timezone including daylight saving results in a timezone of GMT+13 
            //+iso8601TZoneCapturedName+@">(Z|[+\-](0[0-9]|1[0-2])(:(00|30))?))?$)";
             //+ iso8601TZoneCapturedName + @">(Z|[+\-](0[0-9]|1[0-3])(:(00|30))?))?$)";
             //+ iso8601TZoneCapturedName + Iso8601TimeZone.timeZoneRegEx + @"?$)";
              + iso8601TZoneCapturedName + ">(" + Iso8601TimeZone.timeZoneRegEx + @"))?$)";

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="timeString"></param>
        public Iso8601Time(string timeString)
        {
            ParseTime(timeString);
        }

        public Iso8601Time(int hour, int minute, int second, double fractionalSecond, Iso8601TimeZone timeZone)
        {
            Check.Require(ValidHour(hour, minute, second) && ValidMinute(minute) && ValidSecond(second) &&
               ValidFractionalSecond(fractionalSecond));

            this.hour = hour;
            this.minute = minute;
            this.second = second;
            this.fractionalSecond = fractionalSecond;
            this.timezone = timeZone;
        }
           
        #region ValidIso8601Time
        /// <summary>
        /// True if the time string is a valid ISO 8601 time.
        /// </summary>
        /// <param name="timeString"></param>
        /// <returns></returns>
        public static bool ValidIso8601Time(string timeString)
        {
            Match thisMatch = Regex.Match(timeString, timePattern, RegexOptions.Compiled | RegexOptions.Singleline);

            if (!thisMatch.Success)
                return false;

            GroupCollection gCollection = thisMatch.Groups;

            // hours
            string hString = gCollection[hourCaptureName].Value;
            if (string.IsNullOrEmpty(hString))
                return false;

            return true;

        }
        #endregion

        #region ParseTime
        private void ParseTime(string timeString)
        {
            Check.Require(ValidIso8601Time(timeString), "The time string (" +
                timeString + ") must be a valid ISO 8601 time.");

            //Regex rg = new Regex(timePattern);
            //Match thisMatch = rg.Match(timeString);
            Match thisMatch = Regex.Match(timeString, timePattern, RegexOptions.Compiled | RegexOptions.Singleline);
            
            GroupCollection gCollection = thisMatch.Groups;

            // assign values
            // hour
            string hString = gCollection[hourCaptureName].Value;
            Check.Require(!string.IsNullOrEmpty(hString), "Hour value must not be null or empty");
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
                        if (fsString.IndexOf(",") >= 0)
                        {
                            this.isDecimalComma = true;
                            this.fractionalSecond = double.Parse(fsString.Replace(',', '.'));
                        }
                        else if (fsString.IndexOf(".") >= 0)
                        {
                            this.isDecimalComma = false;
                            this.fractionalSecond = double.Parse(fsString);
                        }
                    }
                }
            }
            if (timeString.IndexOf(":")>=0)
                this.isExtended = true;
            else
                this.isExtended = false;

            // time zone
            string tZoneString = gCollection[iso8601TZoneCapturedName].Value;
            if (!string.IsNullOrEmpty(tZoneString))
                this.timezone = new Iso8601TimeZone(tZoneString);
        }
        #endregion

        /// <summary>
        /// returns a time string in the ISO 8601 time format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(this.IsExtended);
        }

        // TODO: need to be implemented
        public string ToString(bool isExtended)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(string.Format("{0:00}", this.hour));

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
                        if (IsDecimalSignComma)
                            sb.Append(",");
                        else
                            sb.Append(".");
                        sb.Append(this.fractionalSecond.ToString(".0######").Substring(1));
                    }
                }
            }

            // Append time zone string
            if (this.timezone != null)
                sb.Append(this.timezone.ToString(isExtended));

            Check.Ensure(ValidIso8601Time(sb.ToString()),
                 "The string (" + sb.ToString() + ") is not a valid Iso8601 time.");

            return sb.ToString();
        }

        #region class properties
        public int Hour
        {
            get
            {
                if (this.hour < 0)
                    throw new FormatException("Hour value must not be unknown or 0.");
                return this.hour;
            }

        }

        public int Minute
        {
            get
            {
                if (MinuteUnknown)
                    return 0;
                return this.minute;
            }

        }
        public int Second
        {
            get
            {
                if (SecondUnknown)
                    return 0;
                return this.second;
            }

        }
        public double FractionalSecond
        {
            get
            {
                if (!HasFractionalSecond)
                    return 0.0;
                return this.fractionalSecond;
            }

        }
        public Iso8601TimeZone TimeZone
        {
            get
            {
                return this.timezone;
            }

        }
        public bool HasFractionalSecond
        {
            get
            {
                if (this.fractionalSecond > -1.0)
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
        public bool IsPartial
        {
            get
            {
                if (this.MinuteUnknown || this.SecondUnknown)
                    return true;
                else
                    return false;
            }
        }
        public bool IsExtended
        {
            get { return this.isExtended; }

        }
        public bool IsDecimalSignComma
        {
            get
            {
                return this.isDecimalComma;
            }
        }
        #endregion

        #region IComparable Members
        /// <summary>
        /// Compare the current time instance with the obj. Returns 0 if they are equal, 
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

            Iso8601Time objTime = obj as Iso8601Time;
            if (this.GetTimeSeconds() > objTime.GetTimeSeconds())
                return 1;
            else if (this.GetTimeSeconds() < objTime.GetTimeSeconds())
                return -1;
            else
                return 0;
        }

        internal double GetTimeSeconds()
        {
            double magnitude = -1.0;
            magnitude = this.hour * TimeDefinitions.minutesInHour
                * TimeDefinitions.secondsInMinute;
            if (!this.MinuteUnknown)
                magnitude += this.Minute * TimeDefinitions.secondsInMinute;
            if (!this.SecondUnknown)
                magnitude += this.Second;
            if (this.HasFractionalSecond)
                magnitude += this.fractionalSecond;

            if (this.timezone != null)
            {
                magnitude += this.timezone.GetTimeZoneSeconds();
            }

            return magnitude;
        }
        #endregion

        #region operator implementations
        // TODO: implements infix"<"(other: like Current): bool
        public static bool operator <(Iso8601Time a, Iso8601Time b)
        {
            DesignByContract.Check.Require((object)a != null && (object)b != null);

            return a.CompareTo(b) < 0;//((IComparable)a).CompareTo(b) < 0
        }

        public static bool operator >(Iso8601Time a, Iso8601Time b)
        {
            DesignByContract.Check.Require((object)a != null && (object)b != null);

            return a.CompareTo(b) > 0;// ((IComparable)a).CompareTo(b) > 0;
        }

        public static bool operator ==(Iso8601Time a, Iso8601Time b)
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

        public static bool operator !=(Iso8601Time a, Iso8601Time b)
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

        #region add/subtract function
        internal Iso8601Time Add(Iso8601Duration duration)
        {
            DesignByContract.Check.Require(duration != null, "duration must not be null.");

            Iso8601Time newTime = new Iso8601Time(this.ToString());

            Iso8601Duration normalisedDuration = Iso8601Duration.Normalise(duration);

            if (normalisedDuration.FractionalSecond > 0)
            {
                if (newTime.SecondUnknown)
                    throw new NotSupportedException("Cannot add a duration with fractionalSeconds when the datetime seconds unknow.");

                if (newTime.HasFractionalSecond)
                {
                    newTime.fractionalSecond += normalisedDuration.FractionalSecond;
                    NormaliseFractionalSecond(newTime);
                }
                else
                    newTime.fractionalSecond = normalisedDuration.FractionalSecond;
            }
          
            if (normalisedDuration.Seconds > 0)
            {
                if (newTime.SecondUnknown)
                    throw new NotSupportedException("Cannot add a duration with seconds when the time seconds unknow.");
                newTime.second += normalisedDuration.Seconds;
                NormaliseSecond(newTime);
            }

            if (normalisedDuration.Minutes > 0)
            {
                if (newTime.MinuteUnknown)
                    throw new NotSupportedException("Cannot add a duration with minutes when the time minutes unknow.");
                newTime.minute += normalisedDuration.Minutes;
                NormaliseMinute(newTime);
            }


            if (normalisedDuration.Hours > 0)
            {
                newTime.hour += normalisedDuration.Hours;

                if (newTime.hour >= 24)
                    throw new ApplicationException("Invalid durtion results in the hours value greater or equal to 24.");
            }

            return newTime;
        }

        internal Iso8601Time Subtract(Iso8601Duration duration)
        {
            DesignByContract.Check.Require(duration != null, "duration must not be null.");

            Iso8601Time newTime = new Iso8601Time(this.ToString());

            Iso8601Duration normalisedDuration = Iso8601Duration.Normalise(duration);

            if (normalisedDuration.FractionalSecond > 0)
            {
                if (newTime.SecondUnknown)
                    throw new NotSupportedException("Cannot subtract a duration with fractionalSeconds when the time seconds unknow.");

                if (newTime.HasFractionalSecond)
                    newTime.fractionalSecond -= normalisedDuration.FractionalSecond;
                else
                    newTime.fractionalSecond = (normalisedDuration.FractionalSecond)*-1;

                NormaliseSubtractedFractionalSecond(newTime);
            }

            if (normalisedDuration.Seconds > 0)
            {
                if (newTime.SecondUnknown)
                    throw new NotSupportedException("Cannot subtract a duration with seconds when the time seconds unknow.");
               
                newTime.second -= normalisedDuration.Seconds;
                NormaliseSubtractedSecond(newTime);
            }

            if (normalisedDuration.Minutes > 0)
            {
                if (newTime.MinuteUnknown)
                    throw new NotSupportedException("Cannot subtract a duration with minutes when the time minutes unknow.");
                newTime.minute -= normalisedDuration.Minutes;
                NormaliseSubtractedMinute(newTime);
            }


            if (normalisedDuration.Hours > 0)
            {
                newTime.hour -= normalisedDuration.Hours;

                if (newTime.hour < 0)
                    throw new ApplicationException("Invalid duration results the Iso8601Time hour less than 0 after subtraction.");
            }

            return newTime;
        }   

        private static void NormaliseMinute(Iso8601Time isoTime)
        {
            DesignByContract.Check.Require(!isoTime.MinuteUnknown, "isoTime.MinuteUnknow must be false.");

            Time time = new Time(isoTime.hour, isoTime.minute);
            time.NormaliseMinute();

            isoTime.hour = time.Hour;
            isoTime.minute = time.Minute;

        }

        private static void NormaliseSecond(Iso8601Time isoTime)
        {
            DesignByContract.Check.Require(!isoTime.MinuteUnknown, "isoTime.MinuteUnknow must be false.");
            DesignByContract.Check.Require(!isoTime.SecondUnknown, "isoTime.SecondUnknown must be false.");

            Time time = new Time(isoTime.hour, isoTime.minute, isoTime.second);
            time.NormaliseSecond();

            isoTime.hour = time.Hour;
            isoTime.minute = time.Minute;
            isoTime.second = time.Second;

        }

        private static void NormaliseFractionalSecond(Iso8601Time isoTime)
        {
            DesignByContract.Check.Require(!isoTime.MinuteUnknown, "isoTime.MinuteUnknow must be false.");
            DesignByContract.Check.Require(!isoTime.SecondUnknown, "isoTime.SecondUnknown must be false.");
            DesignByContract.Check.Require(isoTime.HasFractionalSecond, "isoTime.HasFractionalSecond must be true.");

            Time time = new Time(isoTime.hour, isoTime.minute, isoTime.second, isoTime.fractionalSecond);
            time.NormaliseFractionalSecond();

            isoTime.hour = time.Hour;
            isoTime.minute = time.Minute;
            isoTime.second = time.Second;
            isoTime.fractionalSecond = time.FractionalSeconds;

        }

        private static void NormaliseSubtractedFractionalSecond(Iso8601Time isoTime)
        {
            Time time = new Time(isoTime.hour, isoTime.minute, isoTime.second, isoTime.fractionalSecond);
            time.NormaliseSubtractedFractionalSecond();

            isoTime.hour = time.Hour;
            isoTime.minute = time.Minute;
            isoTime.second = time.Second;
            isoTime.fractionalSecond = time.FractionalSeconds;

        }

        private static void NormaliseSubtractedSecond(Iso8601Time isoTime)
        {           
            Time time = new Time(isoTime.hour, isoTime.minute, isoTime.second);
            time.NormaliseSubtractedSecond();

            isoTime.hour = time.Hour;
            isoTime.minute = time.Minute;
            isoTime.second = time.Second;

        }

        private static void NormaliseSubtractedMinute(Iso8601Time isoTime)
        {
            Time time = new Time(isoTime.hour, isoTime.minute);
            time.NormaliseSubtractedMinutes();

            isoTime.hour = time.Hour;
            isoTime.minute = time.Minute;
        }

        #endregion

    }

    internal class Time
    {
         internal Time(int hour, int minute)
        {
            this.hour = hour;
            this.minute = minute;
            minuteSet = true;
            hourSet = true;
        }


        internal Time(int hour, int minute, int second)
        {           
            this.hour = hour;
            hourSet = true;
            this.minute = minute;
            minuteSet = true;
            this.second = second;
            secondSet = true;
        }

        internal Time(int hour, int minute, int second, double fractionalSeconds)
        {
            this.hour = hour;
            hourSet = true;
           
            this.minute = minute;
            minuteSet = true;
           
            this.second = second;
            secondSet = true;

            this.fractionalSeconds = fractionalSeconds;
            this.fractionalSecondsSet = true;
        }

        private int second;
        bool secondSet;
        private int minute;
        bool minuteSet;
        private int hour;
        bool hourSet;

        double fractionalSeconds;
        bool fractionalSecondsSet;

        internal int Hour
        {
            get
            {
                DesignByContract.Check.Require(hourSet, "hour must have been set.");

                return this.hour;
            }
        }

        internal int Minute
        {
            get
            {
                DesignByContract.Check.Require(minuteSet, "minute must have been set.");

                return this.minute;
            }
        }

        internal int Second
        {
            get
            {
                DesignByContract.Check.Require(secondSet, "second must have been set.");

                return this.second;
            }
        }

        internal double FractionalSeconds
        {
            get
            {
                DesignByContract.Check.Require(fractionalSecondsSet, "fractionalSeconds must have been set.");

                return this.fractionalSeconds;
            }
        }

        internal void NormaliseFractionalSecond()
        {
            DesignByContract.Check.Require(secondSet && minuteSet && hourSet && fractionalSecondsSet, 
                "fractionalSeconds, second, minute and hour values must have been set.");

            if (fractionalSeconds >= 1.0)
            {
                second += (int)(fractionalSeconds) / 1;
                fractionalSeconds = fractionalSeconds % 1;

                NormaliseSecond();
            }

        }

        internal void NormaliseSecond()
        {
            DesignByContract.Check.Require(secondSet && minuteSet && hourSet, "second, minute and hour values must have been set.");

            if (second >= 60)
            {
                minute += second / 60;
                second = second % 60;
                NormaliseMinute();
            }

        }

        internal void NormaliseMinute()
        {
            DesignByContract.Check.Require(minuteSet && hourSet, "minute and hour values must have been set.");

            if (minute >= 60)
            {
                hour += minute / 60;               
                minute = minute % 60;               
            }

        }

        internal void NormaliseSubtractedSecond()
        {
            DesignByContract.Check.Require(secondSet && minuteSet && hourSet, "second, minute and hour values must have been set.");

            if (second < 0)
            {
                minute--;
                NormaliseSubtractedMinutes();
                second += 60;
            }

        }

        internal void NormaliseSubtractedFractionalSecond()
        {
            DesignByContract.Check.Require(secondSet && minuteSet && hourSet && fractionalSecondsSet,
                "fractionalSeconds, second, minute and hour values must have been set.");

            while (fractionalSeconds < 0)
            {
                second--;
                fractionalSeconds += 1;

                NormaliseSecond();
            }

        }

        internal void NormaliseSubtractedMinutes()
        {
            DesignByContract.Check.Require(minuteSet && hourSet, "minute and hour values must have been set.");

            if (minute < 0)
            {
                minute += 60;
                hour--;

            }

        }     
      

    }
    
}
