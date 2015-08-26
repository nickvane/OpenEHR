using System;
using OpenEhr.RM.Composition.Content;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.Attributes;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Generic;
using OpenEhr.Serialisation;
using OpenEhr.Factories;
using OpenEhr.RM.Impl;
using OpenEhr.AssumedTypes.Impl;
using OpenEhr.RM.Support.Identification;

namespace OpenEhr.RM.Composition
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmTypeAttribute("openEHR", "EHR", "COMPOSITION")]
    public class Composition : Locatable, IComposition, IVisitable,
        System.Xml.Serialization.IXmlSerializable
    {
        public Composition()
        { }

        public Composition(DvText name, string archetypeNodeId ,Support.Identification.UidBasedId uid ,
            Link[] links, Archetyped archetypeDetails,FeederAudit feederAudit, 
            CodePhrase language, CodePhrase territory, DvCodedText category, EventContext context,
            Content.ContentItem[] content, PartyProxy composer): 
            base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit)
        {
            Check.Require(language != null, "language must not be null");
            Check.Require(territory != null, "territory must not be null");
            Check.Require(category != null, "category must not be null");
            Check.Require(composer != null, "composer must not be null");
            

            this.language = language;
            this.territory = territory;
            this.category = category;
            this.context = context;
            if (this.context != null)
                this.context.Parent = this;
            if (content != null)
            {
                this.content = RmFactory.LocatableList<ContentItem>(this, content);
            }

            this.composer = composer;

            SetAttributeDictionary();
            this.CheckInvariants();
        }

        private LocatableList<Content.ContentItem> content;

        [RmAttribute("content")]
        public AssumedTypes.List<Content.ContentItem> Content
        {
            get
            {
                if(this.content == null)
                    this.content = base.attributesDictionary["content"] as LocatableList<Content.ContentItem>;
                return this.content;
            }
        }

        private DataTypes.Text.DvCodedText category;

        [RmAttribute("category", 1)]
        [RmTerminology("composition category")]
        public DataTypes.Text.DvCodedText Category
        {
            get
            {
                if(this.category == null)
                    this.category = base.attributesDictionary["category"] as DataTypes.Text.DvCodedText;
                return this.category;
            }
            set
            {
                Check.Invariant(value != null, "Category is mandatory.");
                this.category = value;
                base.attributesDictionary["category"] = this.category;
            }
        }

        private DataTypes.Text.CodePhrase territory;

        [RmAttribute("territory", 1)]
        [RmCodeset("countries", "ISO_3166-1")]
        public DataTypes.Text.CodePhrase Territory
        {
            get
            {
                if(this.territory == null)
                    this.territory = base.attributesDictionary["territory"] as DataTypes.Text.CodePhrase;
                return this.territory;
            }
            set
            {
                Check.Invariant(value != null, "Territory is mandatory.");
                this.territory = value;
                base.attributesDictionary["territory"] = this.territory;
            }
        }

        private DataTypes.Text.CodePhrase language;

        [RmAttribute("language", 1)]
        [RmCodeset("languages", "ISO_639-1")]
        public DataTypes.Text.CodePhrase Language
        {
            get
            {
                if(this.language == null)
                    this.language = base.attributesDictionary["language"] as DataTypes.Text.CodePhrase;
                return this.language;
            }
            set
            {
                Check.Invariant(value != null, "Language is mandatory.");
                this.language = value;
                base.attributesDictionary["language"] = this.language;
            }
        }

        private EventContext context;

        [RmAttribute("context")]
        public EventContext Context
        {
            get
            {
                if(this.context == null)
                    this.context = base.attributesDictionary["context"] as EventContext;
                return this.context;
            }
            set
            {
                this.context = value;
                base.attributesDictionary["context"] = this.context;
            }
        }

        private Common.Generic.PartyProxy composer;

        [RmAttribute("composer", 1)]
        public Common.Generic.PartyProxy Composer
        {
            get
            {
                if(this.composer == null)
                    this.composer = base.attributesDictionary["composer"] as Common.Generic.PartyProxy;
                return this.composer;
            }
            set
            {
                Check.Invariant(value != null, "Composer is mandatory.");
                this.composer = value;
                base.attributesDictionary["composer"] = this.composer;
            }
        }

        /// <summary>
        /// True if category is a “persistent” type,
        /// False otherwise. Useful for finding Compositions
        /// in an EHR which are guaranteed
        /// to be of interest to most users.
        /// </summary>
        /// <remarks>Is_persistent_validity: is_persistent implies context = Void</remarks>
        /// <returns></returns>
        public bool IsPersistent()
        {
            if (this.Category.DefiningCode.CodeString == "431"
                && this.category.DefiningCode.TerminologyId.Value == "openehr")
                return true;
            return false;
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
            return new System.Xml.XmlQualifiedName("COMPOSITION", RmXmlSerializer.OpenEhrNamespace);
        }

        internal protected override void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.NodeType == System.Xml.XmlNodeType.None)
                reader.MoveToContent();

            string archetypeNodeId = reader.GetAttribute("archetype_node_id");
            Check.Require(ArchetypeId.IsValid(archetypeNodeId), 
                "Composition archetype node Id must be an archetype ID");
            base.ReadXml(reader);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            DesignByContract.Check.Assert(reader.LocalName == "language",
                "Expected LocalName is 'language', but it is " + reader.LocalName);
            this.language = new OpenEhr.RM.DataTypes.Text.CodePhrase();
            this.language.ReadXml(reader);

            DesignByContract.Check.Assert(reader.LocalName == "territory",
                "Expected LocalName is 'territory', but it is " + reader.LocalName);
            this.territory = new OpenEhr.RM.DataTypes.Text.CodePhrase();
            this.territory.ReadXml(reader);

            DesignByContract.Check.Assert(reader.LocalName == "category",
               "Expected LocalName is 'category', but it is " + reader.LocalName);
            this.category = new OpenEhr.RM.DataTypes.Text.DvCodedText();
            this.category.ReadXml(reader);

            DesignByContract.Check.Assert(reader.LocalName == "composer",
              "Expected LocalName is 'composer', but it is " + reader.LocalName);
            string composerType = RmXmlSerializer.ReadXsiType(reader);
            this.composer = RmFactory.PartyProxy(composerType);
            
            this.composer.ReadXml(reader);            

            if (reader.LocalName == "context")
            {
                this.context = new EventContext();
                this.context.ReadXml(reader);
                this.context.Parent = this;
            }

            if (reader.LocalName == "content")
            {
                OpenEhr.AssumedTypes.Impl.LocatableList<ContentItem> contents = 
                    new OpenEhr.AssumedTypes.Impl.LocatableList<ContentItem>();
                do
                {
                    string contentType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                    Check.Assert(!string.IsNullOrEmpty(contentType), "content type must not be null or empty.");

                    ContentItem aContentItem = ContentItem.GetLocatableObjectByType(contentType)
                        as ContentItem;
                    if (aContentItem == null)
                        throw new InvalidOperationException("Composition contentType must be type of ContentItem: "+contentType);
                    aContentItem.ReadXml(reader);

                    aContentItem.Parent = this;
                    contents.Add(aContentItem);
                } while (reader.LocalName == "content" && reader.NodeType == System.Xml.XmlNodeType.Element);

                this.content = contents;
            }

           }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "language", RmXmlSerializer.OpenEhrNamespace);
            this.Language.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement(openEhrPrefix, "territory", RmXmlSerializer.OpenEhrNamespace);
            this.Territory.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement(openEhrPrefix, "category", RmXmlSerializer.OpenEhrNamespace);
            this.Category.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement(openEhrPrefix, "composer", RmXmlSerializer.OpenEhrNamespace);
            string composerType = ((IRmType)this.Composer).GetRmTypeName();
            if (!string.IsNullOrEmpty(openEhrPrefix))
                composerType = openEhrPrefix + ":" + composerType;
            writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, composerType);
            this.Composer.WriteXml(writer);
            writer.WriteEndElement();

            if (this.Context != null)
            {
                writer.WriteStartElement(openEhrPrefix, "context", RmXmlSerializer.OpenEhrNamespace);
                this.Context.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.Content != null && this.Content.Count>0)
            {
                foreach (ContentItem contentItem in this.Content)
                {
                    writer.WriteStartElement(openEhrPrefix, "content", RmXmlSerializer.OpenEhrNamespace);
                    string contentType = ((IRmType)contentItem).GetRmTypeName();
                    if (!string.IsNullOrEmpty(openEhrPrefix))
                        contentType = openEhrPrefix + ":" + contentType;
                    writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, contentType);
                    contentItem.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }


        protected override void CheckInvariants()
        {
            base.CheckInvariants();

            // %HYYKA%
            //Check.Invariant(this.Category!=null, "category is mandatory.");
            //Check.Invariant(this.Territory != null, "Territory is mandatory.");
            //Check.Invariant(this.Language != null, "Language is mandatory.");
            //Check.Invariant(this.Composer != null, "Composer is mandatory.");
            //Check.Invariant(this.IsArchetypeRoot, "is archetype root.");
            //Check.Invariant(this.Parent == null, "no parent");

            // CM: 17/03/09 invalid check. Invariant is if composition is persistent, context must be null.
            //Check.Invariant(this.IsPersistent() || this.Context!=null, "is_persistent implies context is null.");
            //Check.Invariant(!this.IsPersistent() || (this.IsPersistent() && this.Context == null), 
            //    "is_persistent implies context is null.");
        }

        protected override void CheckInvariantsDefault()
        {
            base.CheckInvariantsDefault();
            Check.Invariant(this.Category != null, "category is mandatory.");
            Check.Invariant(this.Territory != null, "Territory is mandatory.");
            Check.Invariant(this.Language != null, "Language is mandatory.");

            // %HYYKA%
            //Check.Invariant(this.Composer != null, "Composer is mandatory.");
            //Check.Invariant(this.IsArchetypeRoot, "is archetype root.");
            //Check.Invariant(this.Parent == null, "no parrent");
            //Check.Invariant(this.IsPersistant() || this.Context != null, "is_persistent implies context is null.");
        }

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();

            base.attributesDictionary["language"] = this.language;
            base.attributesDictionary["category"]= this.category;
            base.attributesDictionary["terriroty"]= this.territory;
            base.attributesDictionary["composer"]= this.composer;
            base.attributesDictionary["context"]= this.context;
            base.attributesDictionary["content"]= this.content;
        }

        #region IVisitable Members

        void IVisitable.Accept(IVisitor visitor)
        {
            visitor.VisitComposition(this);
        }

        #endregion
    }
}
