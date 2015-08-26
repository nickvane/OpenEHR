using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataStructures.ItemStructure.Representation;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataStructures.ItemStructure
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_STRUCTURES", "ITEM_SINGLE")]
    public class ItemSingle : ItemStructure, System.Xml.Serialization.IXmlSerializable
    {
        public ItemSingle()
        { }

        public ItemSingle(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
            Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit, Element item)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit)
        {
            Check.Require(item != null, "item must not be null");

            this.item = item;
            if (this.item != null)
                this.item.Parent = this;

            SetAttributeDictionary();
            CheckInvariants();
        }

        private Representation.Element item;

        [RmAttribute("item", 1)]
        public Representation.Element Item
        {
            get
            {
                if(this.item == null)
                    this.item = base.attributesDictionary["item"] as Representation.Element;
                return this.item;
            }
            set
            {
                Check.Require(value != null, "value must not be null.");
                if (this.item != null)
                    this.item.Parent = null;
                this.item = value;
                this.item.Parent = this;
                base.attributesDictionary["item"] = this.item;
            }
        }

        public override Item AsHierarchy()
        {
            throw new NotImplementedException();
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
            return new System.Xml.XmlQualifiedName("ITEM_SINGLE", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            if(reader.LocalName != "item")
                throw new ApplicationException("Expected Element 'item', but it is " + reader.NodeType.ToString() + " '" + reader.LocalName + "'");

            this.item = new OpenEhr.RM.DataStructures.ItemStructure.Representation.Element();
            this.item.ReadXml(reader);
            this.item.Parent = this;

        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "item", RmXmlSerializer.OpenEhrNamespace);
            this.Item.WriteXml(writer);
            writer.WriteEndElement();
        }

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();
            base.attributesDictionary["item"] = this.item;
        }

        protected override void CheckInvariants()
        {
            base.CheckInvariants();
        }
    }
}
