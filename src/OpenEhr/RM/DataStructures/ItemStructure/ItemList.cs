using System;
using System.Xml;
using OpenEhr.Attributes;
using OpenEhr.Factories;
using OpenEhr.RM.DataStructures.ItemStructure.Representation;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.AssumedTypes.Impl;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataStructures.ItemStructure
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_STRUCTURES", "ITEM_LIST")]
    public class ItemList : ItemStructure, System.Xml.Serialization.IXmlSerializable
    {
        public ItemList()
        { }

        public ItemList(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
            Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit, Element[] items)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit)
        {
            if (items != null)
            {
                this.items = RmFactory.LocatableList<Element>(this, items);
            }

            SetAttributeDictionary();
            CheckInvariants();
        }

        private AssumedTypes.List<Representation.Element> items;

        /// <summary>
        /// Physical representation of the list
        /// </summary>
        [RmAttribute("items")]
        public AssumedTypes.List<Representation.Element> Items
        {
            get
            {
                if(this.items == null)
                    this.items = base.attributesDictionary["items"] as LocatableList<Element>;
                return this.items;
            }
        }

        #region Class Functions
        /// <summary>
        /// Count of all items. Returns 0 if the Items is null or empty.
        /// </summary>
        /// <returns></returns>
        public int ItemCount()
        {
            // CM: if this.Items is null, return 0.
            if (this.Items == null)
                return 0;

            return this.Items.Count;
        }

        private AssumedTypes.List<DataTypes.Text.DvText> names;

        /// <summary>
        /// Retrieve the names of all items. If the Items is null or empty, returns an empty list of DvText.
        /// </summary>
        /// <returns></returns>
        public AssumedTypes.List<DataTypes.Text.DvText> Names()
        {
            // CM: 11/08/08 when this.ItemCount() == 0, return an empty list
            if (names == null)
            {
                names = new OpenEhr.AssumedTypes.List<OpenEhr.RM.DataTypes.Text.DvText>();

                if (this.ItemCount() > 0)
                {
                    foreach (Locatable l in this.Items)
                    {
                        names.Add(l.Name);
                    }
                }
            }

            return names;
        }

        /// <summary>
        /// Retrieve the item with name 'aName'
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Element NamedItem(string aName)
        {
            // CM: 11/08/08 return null if no item with the name aName
            // CM: 25/11/09 
            Check.Require(!string.IsNullOrEmpty(aName), "aName must not be null or empty.");

            foreach (Element element in this.Items)
            {
                if (element.Name.Value == aName)
                    return element;
            }

            return null;
        }

        /// <summary>
        /// Retrieve the i-th item with index i
        /// </summary>
        /// <param name="i">index</param>
        /// <returns>The element associated with index i</returns>
        public Element IthItem(int i)
        {
            DesignByContract.Check.Require(i >= 0, "i must be >=0 ");

            // CM: 11/08/08 if the Items is null, return null.
            if (this.ItemCount() == 0)
                return null;

            if (i > this.ItemCount())
                throw new ArgumentOutOfRangeException("index i must be less than ItemCount.");
           
            return this.Items[i] as Element;
        }

        #endregion

        // CM: 16/04/2008
        public override Item AsHierarchy()
        {
            if (this.Items == null || this.Items.Count == 0)
                return null;

            throw new NotImplementedException();
        }

        #region IXmlSerializable Members

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(XmlReader reader)
        {
            this.ReadXml(reader);
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(XmlWriter writer)
        {
            this.WriteXml(writer);
        }

        #endregion

        public static XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadCompositionSchema(xs);
            return new XmlQualifiedName("ITEM_LIST", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            if (reader.LocalName == "items")
            {
                LocatableList<Element> items = new LocatableList<Element>();

                do
                {
                    Element element = new Element();
                    element.ReadXml(reader);
                    element.Parent = this;
                    items.Add(element);
                // Added checking on reader.NodeType == Element is because ItemList xml instance
                // is something like: <items> <items xsi:type='ELEMENT'>..</items></items>.
                // After reading all element contents, the reader.LocalName is still items,
                // but the reader NodeType is EndElement. This situation is similar as Section, ItemTable
                } while (reader.LocalName == "items" && reader.NodeType == System.Xml.XmlNodeType.Element);

                this.items = items;
            }

            reader.MoveToContent();

        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            if (this.Items != null && this.Items.Count > 0)
            {
                foreach (Item anItem in this.Items)
                {
                    writer.WriteStartElement(openEhrPrefix, "items", RmXmlSerializer.OpenEhrNamespace);
                    anItem.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
                
        }


        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();
            base.attributesDictionary["items"]= this.items;

        }
    }
}
