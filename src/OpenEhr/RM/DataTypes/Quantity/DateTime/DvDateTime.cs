using System;
using System.Text;

using OpenEhr.DesignByContract;
using OpenEhr.AssumedTypes;
using OpenEhr.Attributes;
using OpenEhr.RM.DataTypes.Text;
using System.Globalization;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Quantity.DateTime
{
    /// <summary>
    /// Represents an absolute point in time, as measured on the Gregorian 
    /// calendar, and specified only to the day. Semantics defined by ISO 8601.
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_DATE_TIME")]
    public class DvDateTime : DvTemporal<DvDateTime>, IFormattable, System.Xml.Serialization.IXmlSerializable
    {       
        private Iso8601DateTime isoDateTime;
        
        #region constructors

        public DvDateTime(string dateTimeString, DvDuration accuracy, string magnitudeStatus, CodePhrase normalStatus,
         DvInterval<DvDateTime> normalRange, ReferenceRange<DvDateTime>[] otherReferenceRanges)
        {
            this.isoDateTime = new Iso8601DateTime(dateTimeString);
            
            base.SetBaseData(accuracy, magnitudeStatus, normalStatus, normalRange, otherReferenceRanges);

            CheckInvariants();
        }

        public DvDateTime()
        {
            this.isoDateTime = new Iso8601DateTime(System.DateTime.Now);

            CheckInvariants();
        }
        
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="dateString"></param>
        public DvDateTime(string dateTimeString)
            : this(dateTimeString, null, null, null, null, null)
        { }

        public DvDateTime(System.DateTime dateTime)
        {
            this.isoDateTime = new Iso8601DateTime(dateTime);

            CheckInvariants();
        }

        public DvDateTime(int year, int month, int day, int hour, int minute,
            int second, double fractionalSecond, int timeZoneSign, int timeZoneHour,
            int timeZoneMinute)
        {
            this.isoDateTime
                = new Iso8601DateTime(year, month, day, hour, minute, second,
                fractionalSecond, timeZoneSign, timeZoneHour, timeZoneMinute);

            CheckInvariants();
        }

        #endregion

        #region class properties

        // CM: 30/05/07
        public double Magnitude
        {
            get
            {
                return this.GetMagnitude();
            }
        }
        protected override double GetMagnitude()
        {
            return this.isoDateTime.GetDateTimeSeconds();
        }     

        public string Value
        {
            get
            {
                return this.isoDateTime.ToString();
            }
            set
            {
                Check.Invariant(value != null, "Value must not be null.");
                this.isoDateTime = new Iso8601DateTime(value);
            }
        }
        #endregion  

        public override string ToString()
        {
            return this.Value;
        }

        public string ToString(bool isExtended, bool isFactionalSecondDelimiterComma)
        {
            return this.isoDateTime.ToString(isExtended, isFactionalSecondDelimiterComma);
        }

        public override DvAbsoluteQuantity<DvDateTime, DvDuration> Add(DvAmount<DvDuration> b)
        {
            DesignByContract.Check.Require(b is DvDuration, "b object must be a DvDuration instance");

            DvDuration duration = b as DvDuration;
            return this.Add(duration);
        }

        public DvDateTime Add(DvDuration duration)
        {
            Iso8601Duration isoDuration = new Iso8601Duration(duration.Value);
            Iso8601DateTime newDateTime = this.isoDateTime.Add(isoDuration);

            return new DvDateTime(newDateTime.ToString());
        }


        public override DvAbsoluteQuantity<DvDateTime, DvDuration> Subtract(DvAmount<DvDuration> b)
        {
            DesignByContract.Check.Require(b is DvDuration, "b object must be a DvDuration instance");

            DvDuration duration = b as DvDuration;
            return this.Subtract(duration);
        }

        public DvDateTime Subtract(DvDuration duration)
        {
            Iso8601Duration isoDuration = new Iso8601Duration(duration.Value);
            Iso8601DateTime newDateTime = this.isoDateTime.Subtract(isoDuration);

            return new DvDateTime(newDateTime.ToString());
        }

        private DvTime AppendTimeZone(DvTime time)
        {
            if (this.isoDateTime.Iso8601TimeZone != null && Math.Abs(this.isoDateTime.Iso8601TimeZone.GetTimeZoneSeconds()) > 0)
            {
                Iso8601Time isoTime = new Iso8601Time(time.Value);
                if (isoTime.HasFractionalSecond)
                    time = new DvTime(isoTime.Hour, isoTime.Minute, isoTime.Second, isoTime.FractionalSecond,
                        this.isoDateTime.Iso8601TimeZone.Sign, this.isoDateTime.Iso8601TimeZone.Hour,
                        this.isoDateTime.Iso8601TimeZone.Minute);
                else
                {
                    if (!isoTime.SecondUnknown)
                        time = new DvTime(isoTime.Hour, isoTime.Minute, isoTime.Second, 0,
                       this.isoDateTime.Iso8601TimeZone.Sign, this.isoDateTime.Iso8601TimeZone.Hour,
                       this.isoDateTime.Iso8601TimeZone.Minute);
                    else
                    {
                        if (!isoTime.MinuteUnknown)
                            time = new DvTime(isoTime.Hour, isoTime.Minute, -1, 0,
                           this.isoDateTime.Iso8601TimeZone.Sign, this.isoDateTime.Iso8601TimeZone.Hour,
                           this.isoDateTime.Iso8601TimeZone.Minute);
                        else
                            throw new ApplicationException("should not get here.");
                    }
                }
            }
            return time;
        }

        private double GetTotalSeconds(DvDateTime dateTime)
        {
            Iso8601DateTime isoDT = new Iso8601DateTime(dateTime.Value);

            double totalTimeSeconds = 0;

            if (isoDT.HasFractionalSecond)
                totalTimeSeconds = isoDT.Hour * TimeDefinitions.minutesInHour * TimeDefinitions.secondsInMinute +
                    isoDT.Minute * TimeDefinitions.secondsInMinute + isoDT.Second + isoDT.FractionalSecond;
            else
            {
                if (!isoDT.SecondUnknown)
                    totalTimeSeconds = isoDT.Hour * TimeDefinitions.minutesInHour * TimeDefinitions.secondsInMinute +
                   isoDT.Minute * TimeDefinitions.secondsInMinute + isoDT.Second;
                else
                {
                    if (!isoDT.MinuteUnknown)
                        totalTimeSeconds = isoDT.Hour * TimeDefinitions.minutesInHour * TimeDefinitions.secondsInMinute +
                       isoDT.Minute * TimeDefinitions.secondsInMinute;
                    else
                    {
                        if (!isoDT.HourUnknown)
                            totalTimeSeconds = isoDT.Hour * TimeDefinitions.minutesInHour * TimeDefinitions.secondsInMinute;
                    }
                }
            }
            return totalTimeSeconds;
        }

        private DvDate GetDvDate(DvDateTime dateTime)
        {
            Iso8601DateTime isoDT = new Iso8601DateTime(dateTime.Value);
            DvDate date = null;

            int year = isoDT.Year;
            int month = 0;
            if (!isoDT.MonthUnknown)
                month = isoDT.Month;
            int day = 0;
            if (!isoDT.DayUnknown)
                day = isoDT.Day;

            date = new DvDate(year, month, day);
            return date;
        }

        public override bool IsStrictlyComparableTo(DvOrdered<DvDateTime> other)
        {
            DesignByContract.Check.Require(other!=null);

            if (other is DvDateTime)
                return true;
            return false;
        }

        public override DvDuration Diff(DvTemporal<DvDateTime> b)
        {
            DesignByContract.Check.Require(b is DvDateTime, "Expected a DvDateTime instance in Diff function.");

            DvDateTime bObj = b as DvDateTime;

            if (bObj == this)
                return new DvDuration("PT0S");

            double fractionalSecondsDiff = 0;
            int secondsDiff = 0;
            int hoursDiff = 0;
            int minutesDiff = 0;
            int daysDiff = 0;
            int weeksDiff = 0;
            int monthsDiff = 0;
            int yearsDiff = 0;

            Iso8601DateTime leftOperand = new Iso8601DateTime(this.Value);
            Iso8601DateTime rightOperand = new Iso8601DateTime(bObj.Value);
            if (leftOperand < rightOperand)
            {
                leftOperand = new Iso8601DateTime(bObj.Value);;
                rightOperand = new Iso8601DateTime(this.Value);
            }

            if (leftOperand.HasFractionalSecond && rightOperand.HasFractionalSecond)
            {
                fractionalSecondsDiff = leftOperand.FractionalSecond - rightOperand.FractionalSecond;
            }

            if (!leftOperand.SecondUnknown && !rightOperand.SecondUnknown)
                secondsDiff = leftOperand.Second - rightOperand.Second;

            if (!leftOperand.MinuteUnknown && !rightOperand.MinuteUnknown)
                minutesDiff = leftOperand.Minute - rightOperand.Minute;

            if (!leftOperand.HourUnknown && !rightOperand.HourUnknown)
                hoursDiff = leftOperand.Hour - rightOperand.Hour;

            if (!leftOperand.DayUnknown && !rightOperand.DayUnknown)
                daysDiff = leftOperand.Day - rightOperand.Day;

            int daysInMonth = 0;
            if (!leftOperand.MonthUnknown && !rightOperand.MonthUnknown)
            {
                monthsDiff = leftOperand.Month - rightOperand.Month;
                if (leftOperand.Month > 1)
                    daysInMonth = System.DateTime.DaysInMonth(leftOperand.Year, leftOperand.Month - 1);
                else
                    daysInMonth = System.DateTime.DaysInMonth(leftOperand.Year - 1, leftOperand.Month - 1 + Iso8601DateTime.monthsInYear);

            }

            yearsDiff = leftOperand.Year - rightOperand.Year;

            Iso8601Duration diff = Date.NormaliseDuration(yearsDiff, monthsDiff, weeksDiff, daysDiff, hoursDiff, minutesDiff, secondsDiff, fractionalSecondsDiff, daysInMonth);

            return new DvDuration(diff.ToString());
        }

        /// <summary>These pattern symbols are derived directly from DateTimeFormatInfo
        /// documentation. The % and \ pattern symbols are automatically pre-parsed
        /// (along with their following character) and therefore not included in this
        /// list.</summary>
        private static string[] patternSymbols = {
            "yyyy", "yy", "y", "MMMM", "MMM", "MM", "M", "dddd", "ddd", "dd", "d",
            "HH", "H", "hh", "h", "mm", "m", "ss", "s",
            "FFFFFFF", "FFFFFF", "FFFFF", "FFFF", "FFF", "FF", "F", "fffffff", "ffffff", "fffff", "ffff", "fff", "ff", "f",
            "tt", "t", "zzz", "zz", "z", "/", ":", "gg"};

        /// <summary>Formats date-time accordig to format string and the culture of the
        /// format provider. If format is null or empty the short date pattern is assumed</summary>
        /// <param name="format">Format string (as per DateTimeFormatInfo class)</param>
        /// <param name="provider">Format provider culture</param>
        /// <returns>Formatted date-time string</returns>
        public string ToString(string format, IFormatProvider provider)
        {
            // Get the DateTimeFormatInfo object
            DateTimeFormatInfo dateFormat;
            if (provider == null)
                dateFormat = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
            else
            {
                dateFormat = provider as DateTimeFormatInfo;
                if (dateFormat == null)
                {
                    CultureInfo culture = provider as CultureInfo;
                    if (culture != null && culture.DateTimeFormat != null)
                        dateFormat = culture.DateTimeFormat;
                    else
                        dateFormat = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat;
                }
            }
            Check.Assert(dateFormat!= null, "Date format object should not be null.");


            // Shortcuts to process the most common format parameter

            if (string.IsNullOrEmpty(format))
                return this.ToString(false, true);

            if (format == "d" || format == dateFormat.ShortDatePattern)
            {
                Check.Assert(!this.isoDateTime.MonthUnknown, "Month must be known to format according to long date pattern.");
                Check.Assert(!this.isoDateTime.DayUnknown, "Day must be known to format according to long date pattern.");

                string[] formatArgs = new string[3];
                formatArgs[0] = this.isoDateTime.Year.ToString();
                formatArgs[1] = this.isoDateTime.Month > 9 ? this.isoDateTime.Month.ToString() : "0" + this.isoDateTime.Month;
                formatArgs[2] = this.isoDateTime.Day > 9 ? this.isoDateTime.Day.ToString() : "0" + this.isoDateTime.Day;

                return string.Format("{2}/{1}/{0}", formatArgs);
            }


            // Get full format pattern as string builder
            StringBuilder formatPattern = null;
            if (format.Length == 1) //then we are dealing with a format CHARACTER
                formatPattern = new StringBuilder(GetPatternForFormatCharacter(format[0], dateFormat));
            else
                formatPattern = new StringBuilder(format);

            // Add display token replacements to list, indexed by corresponding {x} placemarkers in formatPattern buffer

            // First parse out slash-escaped (\x) characters
            System.Collections.Generic.List<string> displayTokens = new System.Collections.Generic.List<string>();
            for (int i = 0; i < formatPattern.Length; i++)
            {
                if (formatPattern[i] == '\\')
                {
                    displayTokens.Insert(displayTokens.Count, formatPattern[i + 1].ToString());
                    formatPattern.Remove(i, 2);
                    formatPattern.Insert(i, "{" + (displayTokens.Count - 1) + "}");
                }
            }

            // Parse out percent-escaped (%x) characters
            for (int i = 0; i < formatPattern.Length; i++)
            {
                if (formatPattern[i] == '%')
                {
                    string replacement = GetPatternReplacement(formatPattern[i + 1].ToString(), dateFormat);                  
                    if(replacement != null) {
                        displayTokens.Insert(displayTokens.Count, replacement);
                        formatPattern.Remove(i, 2);
                        formatPattern.Insert(i, "{" + (displayTokens.Count - 1) + "}");
                        i += 2;
                    }
                }
            }

            foreach (string formatStr in patternSymbols)
            {
                int tokenIndex = formatPattern.ToString().IndexOf(formatStr);
                while(tokenIndex > -1)
                {
                    formatPattern.Replace(formatStr, "{" + displayTokens.Count + "}", tokenIndex, formatStr.Length);
                    displayTokens.Insert(displayTokens.Count, GetPatternReplacement(formatStr, dateFormat));
                    tokenIndex = formatPattern.ToString().IndexOf(formatStr);
                }
            }
           
            // Format with the processed formatPattern and display token replacements
            // CM: 17/02/09 Converted this to regex as was failing on certain timezones
            string dateTimeString = string.Format(formatPattern.ToString(), displayTokens.ToArray());

            return dateTimeString;
        }

        public string ToString(string format)
        {
            return this.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
        }

        public string ToString(IFormatProvider provider)
        {
            if (provider is DateTimeFormatInfo)
                return this.ToString((provider as DateTimeFormatInfo).FullDateTimePattern, provider);
            else
                return this.ToString("u", provider);
        }


        private static string patternReplacementErrorPattern = "Format pattern token '{0}' not supported for DvDateTime formatted ToString.";

        /// <summary>Gets formatted date particle string for this.isoDateTime, based on
        /// date format pattern token specified, and date format information object.
        /// 
        /// Eg. for this.isoDateTime=2003/06/30, pattern=MMM, dateFormat={en-au}, result=Jun</summary>
        /// <param name="patternToken">Format pattern token</param>
        /// <param name="dateFormat">Date format culture information object</param>
        /// <returns>Formatted date particle string</returns>
        private string GetPatternReplacement(string patternToken, DateTimeFormatInfo dateFormat)
        {
            switch (patternToken[0])
            {
                // Year tokens
                case 'y':
                    switch(patternToken)
                    {
                        case "yyyy": return this.isoDateTime.Year.ToString();
                        case "yy":
                            int year = this.isoDateTime.Year % 100;
                            return year > 9 ? year.ToString() : "0" + year;
                        case "y": return (this.isoDateTime.Year % 100).ToString();
                        default: throw new NotSupportedException(
                            string.Format(patternReplacementErrorPattern, patternToken));
                    }

                // Month tokens
                case 'M':
                    if (this.isoDateTime.MonthUnknown) return string.Empty;
                    switch (patternToken)
                    {
                        case "MMMM": return dateFormat.MonthNames[this.isoDateTime.Month - 1];
                        case "MMM": return dateFormat.AbbreviatedMonthNames[this.isoDateTime.Month - 1];
                        case "MM":
                            int month = this.isoDateTime.Month;
                            return month > 9 ? month.ToString() : "0" + month;
                        case "M": return this.isoDateTime.Month.ToString();
                        default: throw new NotSupportedException(
                            string.Format(patternReplacementErrorPattern, patternToken));
                    }

                // Day tokens
                case 'd':
                    if (this.isoDateTime.DayUnknown) return string.Empty;
                    switch(patternToken)
                    {
                        case "dddd":
                            System.DateTime timeDDDD = new System.DateTime(this.isoDateTime.Year,
                                this.isoDateTime.Month, this.isoDateTime.Day);
                            return dateFormat.DayNames[(int)dateFormat.Calendar.GetDayOfWeek(timeDDDD)];
                        case "ddd":
                            System.DateTime timeDDD = new System.DateTime(this.isoDateTime.Year,
                                this.isoDateTime.Month, this.isoDateTime.Day);
                            return dateFormat.AbbreviatedDayNames[(int)dateFormat.Calendar.GetDayOfWeek(timeDDD)];
                        case "dd":
                            int day = this.isoDateTime.Day;
                            return day > 9 ? day.ToString() : "0" + day;
                        case "d": return this.isoDateTime.Day.ToString();
                        default: throw new NotSupportedException(
                            string.Format(patternReplacementErrorPattern, patternToken));
                    }

                // Timezone tokens
                case 'z':
                    if (this.isoDateTime.Iso8601TimeZone == null) return string.Empty;
                    switch (patternToken)
                    {
                        case "zzz":
                            string sign = this.isoDateTime.Iso8601TimeZone.Sign > 0? "+" : "-";
                            string hour = this.isoDateTime.Iso8601TimeZone.Hour > 9 ?
                                this.isoDateTime.Iso8601TimeZone.Hour.ToString() : "0" + this.isoDateTime.Iso8601TimeZone.Hour;
                            string minute = this.isoDateTime.Iso8601TimeZone.Minute > 9 ?
                                this.isoDateTime.Iso8601TimeZone.Minute.ToString() : "0" + this.isoDateTime.Iso8601TimeZone.Minute;
                            return sign + hour + dateFormat.TimeSeparator + minute;
                        case "zz":
                            return ((this.isoDateTime.Iso8601TimeZone.Sign > 0)? "+" : "-") +
                                ((this.isoDateTime.Iso8601TimeZone.Hour > 9)? "" : "0") + 
                                this.isoDateTime.Iso8601TimeZone.Hour;
                        case "z":
                            return (this.isoDateTime.Iso8601TimeZone.Sign > 0 ? "+" : "-") +
                            this.isoDateTime.Iso8601TimeZone.Hour;
                        default: throw new NotSupportedException(
                            string.Format(patternReplacementErrorPattern, patternToken));
                    }

                // Fractional seconds tokens
                case 'F':
                case 'f':
                    if (!this.isoDateTime.HasFractionalSecond) return string.Empty;
                    if(patternToken.Length > 7) // the fractional seconds pattern may have up to 7 Fs
                        throw new NotSupportedException(
                            string.Format(patternReplacementErrorPattern, patternToken));
                    if (this.isoDateTime.HasFractionalSecond)
                    {
                        string fractionAsText = string.Format("{0:F" + patternToken.Length + "}",
                            this.isoDateTime.FractionalSecond);
                        return fractionAsText.Substring(fractionAsText.Length - 2);
                    }
                    return string.Empty; //else omit decimal points altogether -- must not indicate false precision

                // Hour tokens and AM/PM tokens
                case 'H':
                case 'h':
                case 't':
                    if (this.isoDateTime.HourUnknown) return string.Empty;
                    switch (patternToken)
                    {
                        case "HH": return this.isoDateTime.Hour > 9 ?
                            this.isoDateTime.Hour.ToString() : "0" + this.isoDateTime.Hour;
                        case "H": return this.isoDateTime.Hour.ToString();
                        case "hh":
                            int hour = (this.isoDateTime.Hour > 12 || this.isoDateTime.Hour < 1) ?
                            Math.Abs(this.isoDateTime.Hour - 12) : this.isoDateTime.Hour;
                            return hour > 9 ? hour.ToString() : "0" + hour;
                        case "h": return ((this.isoDateTime.Hour > 12 || this.isoDateTime.Hour < 1) ?
                            Math.Abs(this.isoDateTime.Hour - 12) : this.isoDateTime.Hour).ToString();
                        
                        case "tt": return this.isoDateTime.Hour > 12 ?
                            dateFormat.PMDesignator : dateFormat.AMDesignator;
                        case "t": return this.isoDateTime.Hour > 12 ?
                            dateFormat.PMDesignator[0].ToString() : dateFormat.AMDesignator[0].ToString();
                        default: throw new NotSupportedException(
                            string.Format(patternReplacementErrorPattern, patternToken));
                    }
                    

                // Minutes tokens
                case 'm':
                    if (this.isoDateTime.MinuteUnknown) return string.Empty;
                    switch (patternToken)
                    {
                        case "mm": return this.isoDateTime.Minute > 9 ?
                            this.isoDateTime.Minute.ToString() : "0" + this.isoDateTime.Minute;
                        case "m": return this.isoDateTime.Minute.ToString();
                        default: throw new NotSupportedException(
                            string.Format(patternReplacementErrorPattern, patternToken));
                    }

                // Second tokens
                case 's':
                    if (this.isoDateTime.SecondUnknown) return string.Empty;
                    switch (patternToken)
                    {
                        case "ss": return this.isoDateTime.Second > 9 ?
                            this.isoDateTime.Second.ToString() : "0" + this.isoDateTime.Second;
                        case "s": return this.isoDateTime.Second.ToString();
                        default: throw new NotSupportedException(
                            string.Format(patternReplacementErrorPattern, patternToken));
                    }

                // Symbols tokens, misc
                case '/': return dateFormat.DateSeparator;
                case ':': return dateFormat.TimeSeparator;
                case 'g': if(patternToken=="gg") throw new NotSupportedException(
                    "Format pattern token 'gg' is explicitly not supported for DvDateTime formatted ToString.");
                    else throw new NotSupportedException(
                        string.Format(patternReplacementErrorPattern, patternToken));

                // Cover every other kind of token
                default: throw new NotSupportedException(
                    string.Format(patternReplacementErrorPattern, patternToken));

            } // end outermost switch

        } //method GetPatternReplacement


        /// <summary>Returns the form pattern string corresponding to a single-character
        /// format pattern shortcut, based on DateTimeFormatInfo object provided.</summary>
        /// <param name="formatChar">Single-character format pattern</param>
        /// <param name="dateFormat">Date-time formatting information object</param>
        /// <returns>Format pattern string</returns>
        private string GetPatternForFormatCharacter(char formatChar, DateTimeFormatInfo dateFormat) {
            switch (formatChar)
            {
                case 'd': return dateFormat.ShortDatePattern;
                case 'D': return dateFormat.LongDatePattern;
                case 'f': return dateFormat.LongDatePattern + " " + dateFormat.ShortTimePattern;
                case 'F': return dateFormat.FullDateTimePattern;
                case 'g': return dateFormat.ShortDatePattern + " " + dateFormat.ShortTimePattern;
                case 'G': return dateFormat.ShortDatePattern + " " + dateFormat.LongTimePattern;
                case 'm':
                case 'M': return dateFormat.MonthDayPattern;
                case 'r':
                case 'R': return dateFormat.RFC1123Pattern;
                case 's': return dateFormat.SortableDateTimePattern;
                case 't': return dateFormat.ShortTimePattern;
                case 'T': return dateFormat.LongTimePattern;
                case 'u': return dateFormat.UniversalSortableDateTimePattern;
                case 'U': throw new NotImplementedException("Universal full date time formatting not yet supported in DvDateTime.");
                case 'y':
                case 'Y': return dateFormat.YearMonthPattern;
                default: throw new InvalidOperationException("Invalid format character specified: " + formatChar);
            }
        } // method GetPatternForFormatCharacter

        /// <summary>
        /// Returns DvDateTime instance with the given magnitude
        /// </summary>
        /// <param name="magnitude">magnitude is the numeric value of the date as seconds
        /// since the calendar origin point 1/1/0000</param>
        /// <returns></returns>
        internal static DvTemporal<DvDateTime> GetDateTimeByMagnitude(double magnitude)
        {
            DesignByContract.Check.Require(magnitude >= TimeDefinitions.hoursInDay*TimeDefinitions.minutesInHour 
                * TimeDefinitions.secondsInMinute);

            double secondsInDay = TimeDefinitions.secondsInMinute * TimeDefinitions.minutesInHour
            * TimeDefinitions.hoursInDay;

            //int daysInMagnitude = (int)(Math.Truncate(magnitude / secondsInDay));
            double daysInMagnitude = magnitude / secondsInDay;
            DvDate dvDate = DvDate.GetDateByMagnitude(daysInMagnitude);
            Iso8601DateTime isoDateTime = new Iso8601DateTime(dvDate.Value);

            //double remainder = magnitude - daysInMagnitude * secondsInDay;
            double remainder = magnitude - isoDateTime.GetDateTimeSeconds();          

            if (remainder!=0 && !isoDateTime.DayUnknown && isoDateTime.Day > TimeDefinitions.nominalDaysInMonth)
                remainder += (isoDateTime.Day - TimeDefinitions.nominalDaysInMonth) * TimeDefinitions.hoursInDay * TimeDefinitions.minutesInHour * TimeDefinitions.secondsInMinute;

            if (remainder < 0)
            {
                dvDate = DvDate.GetDateByMagnitude((isoDateTime.GetDateTimeSeconds() - secondsInDay) / secondsInDay);
                isoDateTime = new Iso8601DateTime(dvDate.Value);
                remainder = magnitude - isoDateTime.GetDateTimeSeconds();
            }

            if (remainder == 86400)
            {
                dvDate = DvDate.GetDateByMagnitude((isoDateTime.GetDateTimeSeconds()+remainder)/secondsInDay);
                isoDateTime = new Iso8601DateTime(dvDate.Value);

                remainder = magnitude - isoDateTime.GetDateTimeSeconds();

            }

            DvTime dvTime = null;
            if (remainder > 0)
            {
                dvTime = DvTime.GetTimeByMagnitude(remainder);

                return new DvDateTime(dvDate.Value+"T"+dvTime.Value);
            }
            return new DvDateTime(dvDate.Value);
        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("DV_DATE_TIME", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            // Get value
            Check.Require(reader.LocalName == "value", "reader.LocalName must be 'value'");
            this.isoDateTime = new Iso8601DateTime(reader.ReadElementString("value", RmXmlSerializer.OpenEhrNamespace));

            reader.MoveToContent();
        }
        
        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            Check.Assert(!string.IsNullOrEmpty(this.Value), "value must not be null or empty.");

            writer.WriteElementString(prefix, "value", RmXmlSerializer.OpenEhrNamespace, this.Value);
        }

        protected void CheckInvariants()
        {
            base.CheckInvariants();
            Check.Invariant(!string.IsNullOrEmpty(this.Value), "Value must not be null or empty.");
        }

        #region IXmlSerializable Members

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            this.ReadXml(reader);
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            this.WriteXml(writer);
        }

        #endregion
    }
}
