using System;
using OpenEhr.Attributes;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Quantity.DateTime
{
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_TEMPORAL")]
    public abstract class DvTemporal<T> : DvAbsoluteQuantity<T, DvDuration>
        where T: DvTemporal<T>
    {
        private DvDuration accuracy;

        public override DvAmount<DvDuration> Accuracy
        {
            get
            {
                return this.accuracy;
            }
        }

        public override DvAmount<DvDuration> Diff(DvAbsoluteQuantity<T, DvDuration> b)
        {
            return Diff(b as DvTemporal<T>);
        }

        public abstract DvDuration Diff(DvTemporal<T> b);

        protected void SetBaseData(DvDuration accuracy, string magnitudeStatus, CodePhrase normalStatus, DvInterval<T> normalRange,
           ReferenceRange<T>[] otherReferenceRanges)
        {
            this.accuracy = accuracy;
            base.SetBaseData(magnitudeStatus, normalStatus, normalRange, otherReferenceRanges);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            if (reader.LocalName == "accuracy")
            {
                this.accuracy = new DvDuration();
                this.accuracy.ReadXml(reader);                
            }

            reader.MoveToContent();

        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);
            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            if (this.Accuracy != null)
            {
                writer.WriteStartElement(prefix, "accuracy", RmXmlSerializer.OpenEhrNamespace);
                this.Accuracy.WriteXml(writer);
                writer.WriteEndElement();
            }

        }
    }
}
