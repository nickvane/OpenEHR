using System;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataTypes.Quantity.DateTime;

namespace OpenEhr.RM.DataStructures.History
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_STRUCTURES", "POINT_EVENT")]
    public class PointEvent<T> : Event<T>, System.Xml.Serialization.IXmlSerializable where T : DataStructures.ItemStructure.ItemStructure
    {
        public PointEvent() 
        { }

        public PointEvent(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
            Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit,
            DvDateTime time, T data,
            ItemStructure.ItemStructure state)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit, time, data, state)
        {
            SetAttributeDictionary();
            CheckInvariants();
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
            RmXmlSerializer.LoadCompositionSchema(xs);
            return new System.Xml.XmlQualifiedName("POINT_EVENT", RmXmlSerializer.OpenEhrNamespace);
        }
    }
}
