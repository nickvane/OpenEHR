using System;
using System.Text;
using OpenEhr.DesignByContract;
using OpenEhr.AssumedTypes;
using OpenEhr.Attributes;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Quantity.DateTime
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_DATE")]
    public class DvDate : DvTemporal<DvDate>, System.Xml.Serialization.IXmlSerializable, IFormattable
    {
        private Iso8601Date isoDate;       

        #region constructors

        public DvDate(string dateString, DvDuration accuracy, string magnitudeStatus, CodePhrase normalStatus,
           DvInterval<DvDate> normalRange, ReferenceRange<DvDate>[] otherReferenceRanges)
            : this()
        {
            Check.Require(Iso8601Date.ValidIso8601Date(dateString), "Date string(" + dateString + ") must be a valid ISO 8601 date.");

            this.isoDate = new Iso8601Date(dateString);

            base.SetBaseData(accuracy, magnitudeStatus, normalStatus, normalRange, otherReferenceRanges);

            CheckInvariants();
        }

        public DvDate()
        {
            this.isoDate = new Iso8601Date(System.DateTime.Now);
            CheckInvariants();
        }

        public DvDate(System.DateTime date)
        {
            this.isoDate = new Iso8601Date(date);
            CheckInvariants();
        }

        public DvDate(int year, int month, int day)
        {
            Check.Require(year >= 0, "Year value must not be less than zero.");
            this.isoDate = new Iso8601Date(year, month, day);

            CheckInvariants();
        }

        public DvDate(string dateString)
            : this(dateString, null, null, null, null, null)
        { } 

        #endregion

        public string Value
        {
            get
            {
                return this.isoDate.ToString();
            }
        }

        public new int Magnitude
        {
            get
            {
                double daysInTotal = GetMagnitude();
                int magnitude = (int)daysInTotal;

                Check.Ensure(magnitude >= 0, "Magnitude value must greater than zero.");
                return magnitude;
            }
        }

        protected override double GetMagnitude()
        {
            int magnitude = -1;
            double daysInTotal = -1.0;
            daysInTotal = isoDate.Year * TimeDefinitions.nominalDaysInYear;
            if (!isoDate.MonthUnknown)
                daysInTotal += isoDate.Month * TimeDefinitions.nominalDaysInMonth;
            if (!isoDate.DayUnknown)
                daysInTotal += isoDate.Day;

            return daysInTotal;
        }

        private const string units = "d";
        public string Units
        {
            get
            {
                return units;
            }
        }

        public override string ToString()
        {
            return this.Value;
        }

        public override DvAbsoluteQuantity<DvDate, DvDuration> Subtract(DvAmount<DvDuration> b)
        {
            DesignByContract.Check.Require(b is DvDuration, "b object must be a DvDuration instance");

            DvDuration duration = b as DvDuration;

            Iso8601Duration isoDuration = new Iso8601Duration(duration.Value);
            Iso8601Date newIsoDate = this.isoDate.Subtract(isoDuration);
            return new DvDate(newIsoDate.ToString());
        }

        public override DvAbsoluteQuantity<DvDate, DvDuration> Add(DvAmount<DvDuration> b)
        {
            DesignByContract.Check.Require(b is DvDuration, "b object must be a DvDuration instance");

            DvDuration duration = b as DvDuration;

            Iso8601Duration isoDuration = new Iso8601Duration(duration.Value);
            Iso8601Date newIsoDate = this.isoDate.Add(isoDuration);
            return new DvDate(newIsoDate.ToString());
        }

        public override DvDuration Diff(DvTemporal<DvDate> b)
        {
            DesignByContract.Check.Require(b is DvDate, "Expected a DvDate instance in Diff function.");

            DvDate bObj = b as DvDate;
            Iso8601Date bObjIsoDate = bObj.isoDate;

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

            Iso8601Date leftOperand = new Iso8601Date(this.Value);
            Iso8601Date rightOperand = new Iso8601Date(bObj.Value);
            if (leftOperand < rightOperand)
            {
                leftOperand = new Iso8601Date(bObj.Value); ;
                rightOperand = new Iso8601Date(this.Value);
            }
                     
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

        public override bool IsStrictlyComparableTo(DvOrdered<DvDate> other)
        {
            DesignByContract.Check.Require(other!=null);
           
            if(other is DvDate)
                return true;
            return false;
        }

        /// <summary>
        /// Returns DvDate instance with the given magnitude
        /// </summary>
        /// <param name="magnitude">magnitude is the numeric value of the date as days
        /// since the calendar origin point 1/1/0000</param>
        /// <returns></returns>
        internal static DvDate GetDateByMagnitude(double magnitude)
        {
            DesignByContract.Check.Require(magnitude>=1);
            
            int resultYear = (int)(Math.Truncate(magnitude / TimeDefinitions.nominalDaysInYear));
            double remainder = magnitude - (resultYear * TimeDefinitions.nominalDaysInYear);
            int resultMonth = (int)(Math.Truncate(remainder / TimeDefinitions.nominalDaysInMonth));
            if (resultMonth == 0 && remainder>0)
            {
                resultMonth = 12;
                resultYear = resultYear - 1;
                remainder = magnitude - (resultYear * TimeDefinitions.nominalDaysInYear);               
            }
            int resultDate = 0;
            if (remainder > 0)
            {
                double date = remainder - resultMonth * TimeDefinitions.nominalDaysInMonth;
                resultDate = (int)(Math.Truncate(date));

                if (resultDate > 1 && resultDate < System.DateTime.DaysInMonth(resultYear, resultMonth) && date - resultDate > 0)
                    resultDate = resultDate + 1;
                else
                {
                    if (resultDate == 0)
                    {
                        if (resultMonth > 1)
                            resultMonth = resultMonth - 1;
                        else
                        {
                            resultMonth = 12;
                            resultYear--;
                        }
                        date = (magnitude - (resultYear * TimeDefinitions.nominalDaysInYear)) - (resultMonth * TimeDefinitions.nominalDaysInMonth);

                        resultDate = (int)(Math.Truncate(date));
                        if (resultDate > 1 && resultDate<System.DateTime.DaysInMonth(resultYear, resultMonth) && date - resultDate > 0)
                            resultDate = resultDate + 1;
                        return GetValidDvDate(magnitude, resultYear, resultMonth, resultDate);
                    }

                    return GetValidDvDate(magnitude, resultYear, resultMonth, resultDate);
                }
            }

            return new DvDate(resultYear, resultMonth, resultDate);
        }

        private static DvDate GetValidDvDate(double totoalDates, int year, int month, int date)
        {
            DesignByContract.Check.Require(date>0, "date must >0");

            int resultDate = date;
            int resultMonth = month;
            int resultYear = year;

            while (!Iso8601Date.ValidDay(resultYear, resultMonth, resultDate))
            {
                int daysInMonth =  System.DateTime.DaysInMonth(resultYear, resultMonth);
                if (resultDate > daysInMonth)
                {
                    resultMonth++;
                    if (resultMonth > 12)
                    {
                        resultMonth = 1;
                        resultYear++;
                    }
                }

               double remain = (totoalDates - (resultYear * TimeDefinitions.nominalDaysInYear)) - (resultMonth * TimeDefinitions.nominalDaysInMonth);

               resultDate = (int)(Math.Truncate(remain));
               if (resultDate < 0)
                   throw new ApplicationException("resultDate must not be <=0");

               if (remain - resultDate > 0)
                   resultDate = resultDate + 1;
              
            }

            return new DvDate(resultYear, resultMonth, resultDate);

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

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("DV_DATE", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            // Get value

            Check.Assert(reader.LocalName == "value", "reader.LocalName must be 'value'");
            string value = reader.ReadElementString("value", RmXmlSerializer.OpenEhrNamespace);
            this.isoDate = new Iso8601Date(value);

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

        /// <summary>These pattern symbols are derived directly from DateTimeFormatInfo
        /// documentation. The % and \ pattern symbols are automatically pre-parsed
        /// (along with their following character) and therefore not included in this
        /// list.</summary>
        private string[] patternSymbols = {
            "yyyy", "yy", "y", "MMMM", "MMM", "MM", "M", "dddd", "ddd", "dd", "d", "/", "gg"};

        /// <summary>Formats date-time accordig to format string and the culture of the
        /// format provider. If format is null or empty the short date pattern is assumed</summary>
        /// <param name="format">Format string (as per DateTimeFormatInfo class)</param>
        /// <param name="provider">Format provider culture</param>
        /// <returns>Formatted date-time string</returns>
        string ToString(string format, IFormatProvider provider)
        {
            // Get the DateTimeFormatInfo object
            System.Globalization.DateTimeFormatInfo dateFormat;
            if (provider == null)
                dateFormat = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
            else
            {
                dateFormat = provider as System.Globalization.DateTimeFormatInfo;
                if (dateFormat == null)
                {
                    System.Globalization.CultureInfo culture = provider as System.Globalization.CultureInfo;
                    if (culture != null && culture.DateTimeFormat != null)
                        dateFormat = culture.DateTimeFormat;
                    else
                        dateFormat = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat;
                }
            }
            Check.Assert(dateFormat != null, "Date format object should not be null.");


            // Shortcuts to process the most common format parameter

            if (string.IsNullOrEmpty(format))
                return this.ToString();

            else if (format == "d" || format == dateFormat.ShortDatePattern)
            {
                Check.Assert(!this.isoDate.MonthUnknown, "Month must be known to format according to long date pattern.");
                Check.Assert(!this.isoDate.DayUnknown, "Day must be known to format according to long date pattern.");

                string[] formatArgs = new string[3];
                formatArgs[0] = this.isoDate.Year.ToString();
                formatArgs[1] = this.isoDate.Month > 9 ? this.isoDate.Month.ToString() : "0" + this.isoDate.Month;
                formatArgs[2] = this.isoDate.Day > 9 ? this.isoDate.Day.ToString() : "0" + this.isoDate.Day;

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
                    if (replacement != null)
                    {
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
                while (tokenIndex > -1)
                {
                    formatPattern.Replace(formatStr, "{" + displayTokens.Count + "}", tokenIndex, formatStr.Length);
                    displayTokens.Insert(displayTokens.Count, GetPatternReplacement(formatStr, dateFormat));
                    tokenIndex = formatPattern.ToString().IndexOf(formatStr);
                }
            }


            // Format with the processed formatPattern and display token replacements
            return string.Format(formatPattern.ToString(), displayTokens.ToArray());
        }

        public string ToString(string format)
        {
            return this.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
        }

        string IFormattable.ToString(string format, IFormatProvider provider)
        {
            return this.ToString(format, provider);
        }

        /// <summary>Gets formatted date particle string for this.isoDate, based on
        /// date format pattern token specified, and date format information object.
        /// 
        /// Eg. for this.isoDate=2003/06/30, pattern=MMM, dateFormat={en-au}, result=Jun</summary>
        /// <param name="patternToken">Format pattern token</param>
        /// <returns>Formatted date particle string</returns>
        private string GetPatternReplacement(string patternToken, System.Globalization.DateTimeFormatInfo dateFormat)
        {
            switch (patternToken[0])
            {
                // Year tokens
                case 'y':
                    switch (patternToken)
                    {
                        case "yyyy": return this.isoDate.Year.ToString();
                        case "yy":
                            int year = this.isoDate.Year % 100;
                            return year > 9 ? year.ToString() : "0" + year;
                        case "y": return (this.isoDate.Year % 100).ToString();
                        default: throw new NotSupportedException("Format pattern token '" + patternToken +
                            "' not supported for DvDateTime formatted ToString.");
                    }

                // Month tokens
                case 'M':
                    switch (patternToken)
                    {
                        case "MMMM": return dateFormat.MonthNames[this.isoDate.Month - 1];
                        case "MMM": return dateFormat.AbbreviatedMonthNames[this.isoDate.Month - 1];
                        case "MM":
                            int month = this.isoDate.Month;
                            return month > 9 ? month.ToString() : "0" + month;
                        case "M": return this.isoDate.Month.ToString();
                        default: throw new NotSupportedException("Format pattern token '" + patternToken +
                            "' not supported for DvDateTime formatted ToString.");
                    }

                // Day tokens
                case 'd':
                    switch (patternToken)
                    {
                        case "dddd":
                            System.DateTime timeDDDD = new System.DateTime(this.isoDate.Year,
                                this.isoDate.Month, this.isoDate.Day);
                            return dateFormat.DayNames[(int)dateFormat.Calendar.GetDayOfWeek(timeDDDD)];
                        case "ddd":
                            System.DateTime timeDDD = new System.DateTime(this.isoDate.Year,
                                this.isoDate.Month, this.isoDate.Day);
                            return dateFormat.AbbreviatedDayNames[(int)dateFormat.Calendar.GetDayOfWeek(timeDDD)];
                        case "dd":
                            int day = this.isoDate.Day;
                            return day > 9 ? day.ToString() : "0" + day;
                        case "d": return this.isoDate.Day.ToString();
                        default: throw new NotSupportedException("Format pattern token '" + patternToken +
                            "' not supported for DvDateTime formatted ToString.");
                    }

                // Cover every other kind of token
                default:
                    switch (patternToken)
                    {
                        // Symbols tokens, misc
                        case "/": return dateFormat.DateSeparator;

                        case "gg": 
                            throw new NotSupportedException(
                                "Format pattern token 'gg' is explicitly not supported for DvDateTime formatted ToString.");
                        default: 
                            throw new NotSupportedException("Format pattern token '" + patternToken +
                                "' not supported for DvDateTime formatted ToString.");
                    } //end inner (default) switch

            } // end outer switch
        } 

        /// <summary>Returns the form pattern string corresponding to a single-character
        /// format pattern shortcut, based on DateTimeFormatInfo object provided.</summary>
        /// <param name="formatChar">Single-character format pattern</param>
        /// <param name="dateFormat">Date-time formatting information object</param>
        /// <returns>Format pattern string</returns>
        private string GetPatternForFormatCharacter(char formatChar, System.Globalization.DateTimeFormatInfo dateFormat)
        {
            switch (formatChar)
            {
                case 'd': 
                    return dateFormat.ShortDatePattern;
                case 'D': 
                    return dateFormat.LongDatePattern;
                case 'm':
                case 'M': 
                    return dateFormat.MonthDayPattern;
                case 'y':
                case 'Y': 
                    return dateFormat.YearMonthPattern;
                default: 
                    throw new InvalidOperationException("Invalid format character specified: " + formatChar);
            }
        } 
    }
}
