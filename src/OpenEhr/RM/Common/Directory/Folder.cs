using System;

using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Factories;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Support.Identification;
using OpenEhr.AssumedTypes;
using OpenEhr.AssumedTypes.Impl;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Common.Directory
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "COMMON", "FOLDER")]
    public class Folder : Locatable, System.Xml.Serialization.IXmlSerializable
    {
        Folder()
        {
            SetAttributeDictionary();
        }

        public Folder(DvText name, string archetypeNodeId, UidBasedId uid,
            List<Link> links, 
            Archetyped.Impl.Archetyped archetypeDetails, FeederAudit feederAudit, 
            System.Collections.Generic.IEnumerable<Folder> folders,
            System.Collections.Generic.IEnumerable<ObjectRef> items)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit)
        {
            if (folders != null)
                this.folders = RmFactory.List<Folder>(this, folders) as LocatableList<Folder>;
            if (items != null)
                this.items = new List<ObjectRef>(items);

            SetAttributeDictionary();
        }

        public Folder(DvText name, string archetypeNodeId, UidBasedId uid,
            List<Link> links, Archetyped.Impl.Archetyped archetypeDetails,FeederAudit feederAudit, 
            List<Folder> folders, List<ObjectRef> items)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit)
        {
            LocatableList<Folder> locatableList = folders as LocatableList<Folder>;
            // LocatableList can not be constructed without a parent, hence will not have a parent of this object is it is yet to be constructed
            Check.Require(locatableList == null, "folders must not be of type LocatableList with another parent");

            if (folders != null)
                this.folders = RmFactory.List<Folder>(this, folders) as LocatableList<Folder>;
            this.items = items;

            SetAttributeDictionary();
        }

        LocatableList<Folder> folders;

        [RmAttribute("folders")]
        public List<Folder> Folders
        {
            get { return folders; }
            set
            {
                // folders must not already be set except when value is null
                Check.Require(folders == null || value == null, "folders must not already be set");
                // value must be an empty locatablelist or null.
                Check.Require(value == null || value.Count == 0, "value must be empty");

                LocatableList<Folder> locatableList = value as LocatableList<Folder>;

                Check.Require(value == null || locatableList != null,
                      "value must be of type LocatableList<Folder>, use RmFactory.List<T> factory method");

                this.folders = locatableList;

                SetAttributeValue("folders", locatableList);
            }
        }

        List<ObjectRef> items;

        [RmAttribute("items")]
        public List<ObjectRef> Items
        {
            get { return items; }
            set
            {
                Check.Require(items == null || value == null, "items must not already be set");
                Check.Require(items == null || value.Count == 0, "value must be empty");

                items = value;
                SetAttributeValue("items", value);
            }
        }

        const string RmTypeName = "FOLDER";

        #region IXmlSerializable Members

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadStructureSchema(xs);
            return new System.Xml.XmlQualifiedName("FOLDER", RmXmlSerializer.OpenEhrNamespace);
        }

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

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.LocalName == "folders")
            {
                LocatableList<Folder> folders = new LocatableList<Folder>();
                do
                {
                    Folder folder = new Folder();

                    folder.ReadXml(reader);

                    folder.Parent = this;
                    folders.Add(folder);
                } while (reader.LocalName == "folders" && reader.NodeType == System.Xml.XmlNodeType.Element);

                this.folders = folders;
            }

            if (reader.LocalName == "items")
            {
                AssumedTypes.List<ObjectRef> items = new AssumedTypes.List<ObjectRef>();
                do
                {
                    ObjectRef item = new ObjectRef();

                    item.ReadXml(reader);
                    items.Add(item);
                } while (reader.LocalName == "items" && reader.NodeType == System.Xml.XmlNodeType.Element);

                this.items = items;
            }
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            if (this.Folders != null && this.Folders.Count > 0)
            {
                foreach (Folder folder in this.Folders)
                {
                    writer.WriteStartElement(openEhrPrefix, "folders", RmXmlSerializer.OpenEhrNamespace);
                    folder.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }

            if (this.Items != null && this.Items.Count > 0)
            {
                foreach (ObjectRef item in this.Items)
                {
                    writer.WriteStartElement(openEhrPrefix, "items", RmXmlSerializer.OpenEhrNamespace);
                    item.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }

        #endregion

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();

            base.attributesDictionary["folders"] = this.folders;
            base.attributesDictionary["items"] = this.items;
        }
    }
}
