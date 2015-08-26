using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.Factories;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.DataTypes.Basic
{
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DATA_VALUE")]
    public abstract class DataValue : RmType
    {
        internal void ReadXml(System.Xml.XmlReader reader)
        {
            Check.Require(!reader.IsEmptyElement, reader.LocalName + " element must not be empty.");

            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase(reader);

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
                "Expected endElement after calling ReadXml");
            reader.ReadEndElement();
            reader.MoveToContent();

            this.CheckInvariants();
        }

        protected abstract void ReadXmlBase(System.Xml.XmlReader reader);

       
        protected abstract void WriteXmlBase(System.Xml.XmlWriter writer);

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            this.WriteXmlBase(writer);
        }

        internal static DataTypes.Basic.DataValue CreateInterval(string intervalType)
        {
            Check.Require(!string.IsNullOrEmpty(intervalType), "intervalType must not be null or empty");

            DataTypes.Basic.DataValue interval = null;

            switch (intervalType)
            {
                case "DV_QUANTITY":
                    interval = new DataTypes.Quantity.DvInterval<DataTypes.Quantity.DvQuantity>();
                    break;
                case "DV_COUNT":
                    interval = new DataTypes.Quantity.DvInterval<DataTypes.Quantity.DvCount>();
                    break;
                case "DV_ORDINAL":
                    interval = new DataTypes.Quantity.DvInterval<DataTypes.Quantity.DvOrdinal>();
                    break;
                case "DV_PROPORTION":
                    interval = new DataTypes.Quantity.DvInterval<DataTypes.Quantity.DvProportion>();
                    break;
                case "DV_DURATION":
                    interval = new DataTypes.Quantity.DvInterval<DataTypes.Quantity.DateTime.DvDuration>();
                    break;
                case "DV_DATE_TIME":
                    interval = new DataTypes.Quantity.DvInterval<DataTypes.Quantity.DateTime.DvDateTime>();
                    break;
                case "DV_DATE":
                    interval = new DataTypes.Quantity.DvInterval<DataTypes.Quantity.DateTime.DvDate>();
                    break;
                case "DV_TIME":
                    interval = new DataTypes.Quantity.DvInterval<DataTypes.Quantity.DateTime.DvTime>();
                    break;
                default:
                    throw new NotImplementedException(intervalType);
            }

            Check.Ensure(interval != null, "interval must not be null");
            return interval;
        }

        public static DataValue ReadDataValue(System.Xml.XmlReader reader)
        {
            DataValue dataValue = null;

            string valueType = RmXmlSerializer.ReadXsiType(reader);
            Check.Assert(!string.IsNullOrEmpty(valueType), "value type must not be null or empty.");

            switch (valueType)
            {
                case "DV_INTERVAL":
                    reader.ReadStartElement();      
                    reader.MoveToContent();

                    if(reader.LocalName != "lower" && reader.LocalName != "upper")
                        throw new ApplicationException("interval must have lower or upper element");

                    string intervalType = RmXmlSerializer.ReadXsiType(reader);

                    dataValue = CreateInterval(intervalType);                    
                    dataValue.ReadXmlBase(reader);

                    DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
                        "Expected endElement after calling ReadXml");
                    reader.ReadEndElement();
                    reader.MoveToContent();

                    break;

                default:
                    dataValue = RmFactory.DataValue(valueType);

                    dataValue.ReadXml(reader);
                    break;
            }

            DesignByContract.Check.Ensure(dataValue != null, "dataValue must not be null.");
            return dataValue;
        }

        protected abstract void CheckInvariants();
    }    
}
