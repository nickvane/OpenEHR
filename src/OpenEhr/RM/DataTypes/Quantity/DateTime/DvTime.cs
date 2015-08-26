using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.AssumedTypes;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Quantity.DateTime
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_TIME")]
    public class DvTime : DvTemporal<DvTime>, System.Xml.Serialization.IXmlSerializable
    {
        private AssumedTypes.Iso8601Time isoTime;

        #region constructors

        public DvTime(string timeString, DvDuration accuracy, string magnitudeStatus, CodePhrase normalStatus,
            DvInterval<DvTime> normalRange, ReferenceRange<DvTime>[] otherReferenceRanges)
            : this()
        {
            Check.Require(Iso8601Time.ValidIso8601Time(timeString), "Time string(" + timeString + ") must be a valid ISO 8601 time.");

            this.isoTime = new Iso8601Time(timeString);

            base.SetBaseData(accuracy, magnitudeStatus, normalStatus, normalRange, otherReferenceRanges);

            CheckInvariants();
        }

        public DvTime()
            :base()
        {
            System.DateTime now = System.DateTime.Now.ToUniversalTime();
            this.isoTime = new Iso8601Time(now.Hour, now.Minute, now.Second, now.Millisecond/1000,
                new Iso8601TimeZone("Z"));

            this.CheckInvariants();
        }

        public DvTime(string timeString) 
            : this(timeString, null, null, null, null, null) 
        { }

        public DvTime(int hour, int minute, int second, double fractionalSecond,
            int timeZoneSign, int timeZoneHour, int timeZoneMinute)
        {
            AssumedTypes.Iso8601TimeZone timeZone =
                new OpenEhr.AssumedTypes.Iso8601TimeZone
                (timeZoneSign, timeZoneHour, timeZoneMinute);

            this.isoTime = new OpenEhr.AssumedTypes.Iso8601Time
                (hour, minute, second, fractionalSecond, timeZone);

            this.CheckInvariants();
        }

        #endregion
        
        #region class properties

        /// <summary>
        /// Numeric value of the time as seconds since the start of day.
        /// </summary>
        protected override double GetMagnitude()
        {
            return this.isoTime.GetTimeSeconds();
        }

        // TODO: need to confirm with Heath
        private string units = "s";

        public string Units
        {
            get { return this.units; }
        }

        #endregion

        public string Value
        {
            get { return this.isoTime.ToString(); }
        }

        public override string ToString()
        {
            return this.Value;
        }

        public override DvAbsoluteQuantity<DvTime, DvDuration> Subtract(DvAmount<DvDuration> b)
        {
            DesignByContract.Check.Require(b is DvDuration, "b object must be a DvDuration instance");

            DvDuration duration = b as DvDuration;

            Iso8601Duration isoDuration = new Iso8601Duration(duration.Value);
            Iso8601Time newIsoTime = this.isoTime.Subtract(isoDuration);
            return new DvTime(newIsoTime.ToString());
        }

        public override DvAbsoluteQuantity<DvTime, DvDuration> Add(DvAmount<DvDuration> b)
        {
            DesignByContract.Check.Require(b is DvDuration, "b object must be a DvDuration instance");

            DvDuration duration = b as DvDuration;

            Iso8601Duration isoDuration = new Iso8601Duration(duration.Value);

            Iso8601Time newIsoTime = this.isoTime.Add(isoDuration);
            return new DvTime(newIsoTime.ToString());
        }

        public double Magnitude
        {
            get
            {
                return this.GetMagnitude();
            }
        }

        public override bool IsStrictlyComparableTo(DvOrdered<DvTime> other)
        {
            DesignByContract.Check.Require(other != null);

            if (other is DvTime)
                return true;
            return false;
        }

        public override DvDuration Diff(DvTemporal<DvTime> b)
        {
            DesignByContract.Check.Require(b is DvTime, "Expected a DvTime instance in Diff function.");

            DvTime bObj = b as DvTime;

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

            Iso8601Time leftOperand = new Iso8601Time(this.Value);
            Iso8601Time rightOperand = new Iso8601Time(bObj.Value);
            if (leftOperand < rightOperand)
            {
                leftOperand = new Iso8601Time(bObj.Value); ;
                rightOperand = new Iso8601Time(this.Value);
            }

            if (leftOperand.HasFractionalSecond && rightOperand.HasFractionalSecond)
            {
                fractionalSecondsDiff = leftOperand.FractionalSecond - rightOperand.FractionalSecond;
            }

            if (!leftOperand.SecondUnknown && !rightOperand.SecondUnknown)
                secondsDiff = leftOperand.Second - rightOperand.Second;

            if (!leftOperand.MinuteUnknown && !rightOperand.MinuteUnknown)
                minutesDiff = leftOperand.Minute - rightOperand.Minute;

            hoursDiff = leftOperand.Hour - rightOperand.Hour;

            int daysInMonth = 0;
            
            Iso8601Duration diff = Date.NormaliseDuration(yearsDiff, monthsDiff, weeksDiff, daysDiff, hoursDiff, minutesDiff, secondsDiff, fractionalSecondsDiff, daysInMonth);

            DesignByContract.Check.Assert(diff.Years == 0 && diff.Months == 0 && diff.Days == 0);

            return new DvDuration(diff.ToString());
        }


        /// <summary>
        /// Returns DvTime instance with the given magnitude
        /// </summary>
        /// <param name="magnitude">magnitude is the numeric value of the date as seconds
        /// since the start of day</param>
        /// <returns></returns>
        internal static DvTime GetTimeByMagnitude(double magnitude)
        {
            DesignByContract.Check.Require(magnitude >= 0);

            double secondsInHour = TimeDefinitions.secondsInMinute * TimeDefinitions.minutesInHour;

            int hourInMagnitude = (int)(Math.Truncate(magnitude / secondsInHour));

            double remainder = magnitude - hourInMagnitude * secondsInHour;
            int minutesInMagnitude = (int)(Math.Truncate(remainder / TimeDefinitions.secondsInMinute));

            remainder = remainder - minutesInMagnitude * TimeDefinitions.secondsInMinute;

            int secondsInMagnitude =(int)(Math.Truncate(remainder));
            double factionalSeconds = remainder - secondsInMagnitude;



            return new DvTime(hourInMagnitude, minutesInMagnitude, secondsInMagnitude,
                factionalSeconds, 1, 0, 0); 
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
            return new System.Xml.XmlQualifiedName("DV_TIME", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            // Get value

            Check.Assert(reader.LocalName == "value", "reader.LocalName must be 'value'");
            string value = reader.ReadElementString("value", RmXmlSerializer.OpenEhrNamespace);

            this.isoTime = new Iso8601Time(value);

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
    }
}
