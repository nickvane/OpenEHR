using System;
using System.Text.RegularExpressions;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;

namespace OpenEhr.AssumedTypes
{
    /// <summary>
    /// Represents a timezone as used in ISO 8601
    /// </summary>
    [Serializable]
    [RmType("openEHR", "SUPPORT", "ISO8601_TIMEZONE")]
    public class Iso8601TimeZone : TimeDefinitions
    {
        private int hour = 0;
        private int minute = 0;
        private int sign = 0;
        private bool isGmt;

        private const string tZoneHHCapturedName = "zhh";
        private const string tZoneMinCapturedMame = "zmm";
        private const string tZoneDirectionCapturedName = "tD";
        private const string utcTimeCapturedName = "utc";
        private const string gmtCapturedName = "gmt";

        // %HYYKA%
        //\b(?<utc>Z)|(?<gmt>(?<tD>\+|\-)(?<zhh>\d{2})(?<zmm>\d{2}))\b
        //^(?<utc>Z)|(?<gmt>(?<tD>[+\-])(?<zhh>0[0-9]|1[0-2]):?(?<zmm>00|30)?)$
        //private static string timeZonePattern = @"\b(?<"
        //    + utcTimeCapturedName + @">Z)|(?<"
        //    + gmtCapturedName + @">(?<"
        //    + tZoneDirectionCapturedName + @">\+|\-)(?<"
        //    + tZoneHHCapturedName + @">\d{2})(?<"
        //    + tZoneMinCapturedMame + @">\d{2}))\b";

        // CM: 07/11/08 EHR-731 The Auckland NZ timezone including daylight saving results in a timezone of GMT+13 
        //private static string timeZonePattern = @"^(?<"+utcTimeCapturedName+@">Z)|(?<"
        //    +gmtCapturedName+@">(?<"+tZoneDirectionCapturedName+@">[+\-])(?<"
        //    +tZoneHHCapturedName+@">0[0-9]|1[0-2]):?(?<"+tZoneMinCapturedMame+@">00|30)?)$";
        internal const string timeZoneRegEx = @"(?<" + utcTimeCapturedName + @">Z)|(?<"
           + gmtCapturedName + @">(?<" + tZoneDirectionCapturedName + @">[+\-])(?<"
           + tZoneHHCapturedName + @">0[0-9]|1[0-3]):?(?<" + tZoneMinCapturedMame + @">00|30)?)";

        // %HYYKA%
        //// CM: EHR-951 allow timezone to be upto 15 hours
        //internal const string timeZoneRegEx = @"(?<" + utcTimeCapturedName + @">Z)|(?<"
        //  + gmtCapturedName + @">(?<" + tZoneDirectionCapturedName + @">[+\-])(?<"
        //  + tZoneHHCapturedName + @">0[0-9]|1[0-5]):?(?<" + tZoneMinCapturedMame + @">00|30)?)";
        private static string timeZonePattern = @"^" + timeZoneRegEx + @"$";


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="timeZoneString"></param>
        public Iso8601TimeZone(string timeZoneString)
        {
            ParseTimeZone(timeZoneString);
        }

        public Iso8601TimeZone(int timeZoneSign, int hour, int minute)
        {
            Check.Require(timeZoneSign == 1 || timeZoneSign == -1,
                "Time zone sign must be either 1 or -1.");
            Check.Require(hour >= 0 && hour <= 12,
                    "Time zone hour must be in the range of 00-12");
            Check.Require(minute >= 0 && minute <= 30,
                    "Time zone minutes must be in the range of 00-30.");

            this.sign = timeZoneSign;
            this.hour = hour;
            this.minute = minute;

        }

        /// <summary>
        /// True if the time zone string is in ISO 8601 time zone format
        /// </summary>
        /// <param name="timeZoneString"></param>
        /// <returns></returns>
        public static bool ValidIso8601TimeZone(string timeZoneString)
        {
            Match thisMatch = Regex.Match(timeZoneString, timeZonePattern, RegexOptions.Compiled | RegexOptions.Singleline);

            if (!thisMatch.Success)
                return false;
            return true;
            
          
        }

        private void ParseTimeZone(string timeZoneString)
        {

            Match match = Regex.Match(timeZoneString, timeZonePattern, RegexOptions.Compiled | RegexOptions.Singleline);
            Check.Require(match.Success, "The time zone string (" +
                timeZoneString + " must be in ISO 8601 time zone format.");
            
            GroupCollection gCollection = match.Groups;

            // utc 
            string utcString = gCollection[utcTimeCapturedName].Value;
            if (!string.IsNullOrEmpty(utcString))
            {
                this.isGmt = true;
            }
            else
            {
                string gmtTZoneString = gCollection[gmtCapturedName].Value;
                if (!string.IsNullOrEmpty(gmtTZoneString))
                {
                    this.isGmt = false;

                    // direction of timezone
                    string tZoneDirection = gCollection[tZoneDirectionCapturedName].Value;
                    Check.Require(!string.IsNullOrEmpty(tZoneDirection),
                        "time zone direction sign must not be null or empty.");
                    if (tZoneDirection == "+")
                        this.sign = 1;
                    else if (tZoneDirection == "-")
                        this.sign = -1;
                    else
                        throw new InvalidOperationException("Time zone direction string must be either + or -.");

                    // time zone hour
                    string tZoneHour = gCollection[tZoneHHCapturedName].Value;
                    Check.Require(!string.IsNullOrEmpty(tZoneHour),
                       "time zone HH must not be null or empty.");
                    this.hour = int.Parse(tZoneHour);

                    // time zone minute
                    string tZoneMinute = gCollection[tZoneMinCapturedMame].Value;
                    if(!string.IsNullOrEmpty(tZoneMinute))
                        this.minute = int.Parse(tZoneMinute);

                }
                else
                    throw new InvalidOperationException("Time zone string (" +
                        timeZoneString + ") is not a valid ISO 8601 time zone.");
            }
        }

        public string ToString(bool isExtended)
        {
            if (this.isGmt)
                return "Z";
            else
            {
                // time zone direction
                Check.Require(this.sign == -1 || this.sign == 1,
                    "Time zone direction must equal to either -1 or 1.");

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                if (this.sign == 1)
                    sb.Append("+");
                else
                    sb.Append("-");

                // time zone hour
                Check.Require(this.hour > -1, "The time zone hour must be greater than -1.");
                sb.Append(string.Format("{0:00}", this.hour));

                if(isExtended)
                    sb.Append(":");

                // time zone minute
                Check.Require(this.minute > -1, "The time zone hour must be greater than -1.");
                sb.Append(string.Format("{0:00}", this.minute));

                Check.Ensure(sb.ToString() != null, "Time zone string must not be null.");

                return sb.ToString();
            }
        }

        internal double GetTimeZoneSeconds()
        {
            int timezoneSeconds = 0;
            if (!this.IsGmt)
            {
                int timezoneMinutes = this.Hour * TimeDefinitions.minutesInHour + this.Minute;
                timezoneSeconds = timezoneMinutes * TimeDefinitions.secondsInMinute;
                if (this.Sign == 1)
                    timezoneSeconds = timezoneSeconds * -1;               
            }

            return timezoneSeconds;
        }

        #region class properties
        public int Hour
        {
            get { return this.hour; }

        }
        public int Minute
        {
            get { return this.minute; }

        }
        public int Sign
        {
            get { return this.sign; }

        }
        public bool IsGmt
        {
            get
            {
                if (this.hour <= 0 && this.minute <= 0)
                    this.isGmt = true;
                else
                    this.isGmt = false;

                return this.isGmt;
            }
        }
        #endregion
    }
}
