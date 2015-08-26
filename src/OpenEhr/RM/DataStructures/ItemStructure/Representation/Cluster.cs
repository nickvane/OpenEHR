using System;

using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;
using OpenEhr.Factories;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.AssumedTypes.Impl;

namespace OpenEhr.RM.DataStructures.ItemStructure.Representation
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_STRUCTURES", "CLUSTER")]
    public class Cluster : Item, System.Xml.Serialization.IXmlSerializable
    {
        public Cluster()
        { }

        public Cluster(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
            Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit, Item[] items)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit)
        {
            if (items != null)
            {
                this.items = RmFactory.LocatableList<Item>(this, items);
            }

            SetAttributeDictionary();
            CheckInvariants();
        }

        private AssumedTypes.List<Item> items;


        [RmAttribute("items")]
        public AssumedTypes.List<Item> Items
        {
            get
            {
                if(this.items == null)
                    this.items = base.attributesDictionary["items"] as LocatableList<Item>;
                return this.items;
            }
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
            return new System.Xml.XmlQualifiedName("CLUSTER", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            DesignByContract.Check.Assert(reader.LocalName == "items",
                "Expected LocalName is 'items' rather than " + reader.LocalName);

            OpenEhr.AssumedTypes.Impl.LocatableList < Item >  items = new OpenEhr.AssumedTypes.Impl.LocatableList<Item>();
            while (reader.LocalName == "items" && reader.NodeType== System.Xml.XmlNodeType.Element)
            {
                string itemType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                Item anItem = Item.GetLocatableObjectByType(itemType) as Item;
                if (anItem == null)
                    throw new InvalidOperationException("itemType must be type of Item: " + itemType);
                anItem.ReadXml(reader);

                anItem.Parent = this;
                items.Add(anItem);
            }
            this.items = items;
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            foreach (Item anItem in this.Items)
            {
                writer.WriteStartElement(openEhrPrefix, "items", RmXmlSerializer.OpenEhrNamespace);

                string itemType = ((IRmType)anItem).GetRmTypeName();
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    itemType = openEhrPrefix + ":" + itemType;
               
                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, itemType);
                anItem.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();
            base.attributesDictionary["items"] = this.items;
        }


        protected override void CheckInvariants()
        {
            base.CheckInvariants();

            // %HYYKA%
            //DesignByContract.Check.Invariant(this.Items.Count>0, "Items must not be empty.");
        }

        protected void CheckInvariantsDefault()
        {
            base.CheckInvariantsDefault();

            // %HYYKA%
            //DesignByContract.Check.Invariant(this.Items != null, "Items must not be null.");
        }
    }
}
