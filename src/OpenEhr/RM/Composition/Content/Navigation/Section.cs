using System;
using OpenEhr.Attributes;
using OpenEhr.AssumedTypes;
using OpenEhr.Serialisation;
using OpenEhr.Factories;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Impl;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.AssumedTypes.Impl;

namespace OpenEhr.RM.Composition.Content.Navigation
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "EHR", "SECTION")]
    public class Section : ContentItem, System.Xml.Serialization.IXmlSerializable
    {
        public Section() 
        { }

        public Section(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
           Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit, ContentItem[] items)
            :base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit)
        {
            if (items != null)
            {
                this.items = RmFactory.LocatableList<ContentItem>(this, items);
            }

            SetAttributeDictionary();
            CheckInvariants();
        }

        private List<ContentItem> items;

        [RmAttribute("items")]
        public List<ContentItem> Items
        {
            get
            {
                if(this.items ==null)
                    this.items = base.attributesDictionary["items"] as List<ContentItem>;
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
            return new System.Xml.XmlQualifiedName("SECTION", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            // CM: 21/08/09 This code doesn't support a section without items.
            // e.g.    
            /** <items xsi:type="SECTION" archetype_node_id="at0002">
                 <name>
                   <value>Mother</value>
                  </name>           
                </items>
             * 
             * <items xsi:type="SECTION" archetype_node_id="at0002">
                 <name>
                   <value>Mother</value>
                  </name>  
             *      <items/>
                </items>
             * */
            if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.LocalName == "items")
            {
                if (reader.IsEmptyElement)
                    reader.Skip();
                else
                {
                    LocatableList<ContentItem> items = new LocatableList<ContentItem>();
                    do
                    {
                        string itemType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                        ContentItem contentItem = ContentItem.GetLocatableObjectByType(itemType)
                            as ContentItem;
                        if (contentItem == null)
                            throw new InvalidOperationException("itemType must be type of ContentItem: " + itemType);

                        contentItem.ReadXml(reader);
                        contentItem.Parent = this;
                        items.Add(contentItem);

                    } while (reader.LocalName == "items" && reader.NodeType == System.Xml.XmlNodeType.Element);

                    this.items = items;
                }
            }
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);           

            if (this.Items != null && this.Items.Count > 0)
            {
                foreach (ContentItem item in this.Items)
                {
                    writer.WriteStartElement(openEhrPrefix, "items", RmXmlSerializer.OpenEhrNamespace);
                    string itemType = ((IRmType)item).GetRmTypeName();
                    if (!string.IsNullOrEmpty(openEhrPrefix))
                        itemType = openEhrPrefix + ":" + itemType;
                    writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, itemType);
                    item.WriteXml(writer);
                    writer.WriteEndElement();
                }
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
            //DesignByContract.Check.Invariant(this.Items == null || this.Items.Count > 0,
            //    "items /= void implies not items.is_empty");
        }
    }
}
