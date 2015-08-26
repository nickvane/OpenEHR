using System;
using System.Collections.Generic;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.DesignByContract;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Common.Resource
{
    /// <summary>
    /// Abstract idea of an online resource created by a human author.
    /// </summary>
    [Serializable]
    public abstract class AuthoredResource
    {
        protected AuthoredResource() { }

        protected AuthoredResource(CodePhrase originalLanguage, Generic.RevisionHistory revisionHistory,
            bool isControlled)
        {
            DesignByContract.Check.Require(originalLanguage!= null, "originalLanguage must not be null.");
            DesignByContract.Check.Require(!isControlled ^ revisionHistory != null, "RevisionHistory is only required if isControlled is true");

            this.originalLanguage = originalLanguage;
            this.IsControlled = isControlled;
            this.revisionHistory = revisionHistory;
        }

        protected AuthoredResource(CodePhrase originalLanguage, AssumedTypes.Hash<TranslationDetails, string> translations,
           ResourceDescription description, Generic.RevisionHistory revisionHistory, bool isControlled):
            this(originalLanguage, revisionHistory, isControlled)
        {
            DesignByContract.Check.Require(translations == null ^ 
                (translations.Keys.Count>0 && !translations.HasKey(this.originalLanguage.CodeString)), 
                "translations is not null means it must not be empty and it must not contains original language.");
            DesignByContract.Check.Require(translations == null ^ (description != null && translations != null),
               "if translations !=null, descriptions must not be null");

            
            this.translations = translations;
            this.description = description;
        }

        private CodePhrase originalLanguage;
        /// <summary>
        /// Language in which this resource was initially authored. Although there is no language primacy
        /// of resources overall, the language of original authoring is required to ensure natural
        /// language translations can preserve quality. Language is relevant in both the description
        /// and ontology sections.
        /// </summary>
        public CodePhrase OriginalLanguage
        {
            get { return this.originalLanguage; }
            set
            {
                DesignByContract.Check.Require(value != null, "originalLanguage value must not be null.");
                this.originalLanguage = value;
            }
        }

        private AssumedTypes.Hash<TranslationDetails, string> translations;
        /// <summary>
        /// List of details for each natural translation made of this resource, keyed by language. For each
        /// translation listed here, there must be corresponding sections in all language-dependent
        /// parts of the resource. The original_language does not appear in this list.
        /// </summary>
        public AssumedTypes.Hash<TranslationDetails, string> Translations
        {
            get { return this.translations; }
            set { this.translations = value; }
        }

        private ResourceDescription description;
        /// <summary>
        /// Description and lifecycle information of the resource.
        /// </summary>
        public ResourceDescription Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        private Generic.RevisionHistory revisionHistory;
        /// <summary>
        /// The revision history of the resource. Only required if is_controlled = True (avoids large
        /// revision histories for informal or private editing situations).
        /// </summary>
        public Generic.RevisionHistory RevisionHistory
        {
            get { return revisionHistory; }
            set { this.revisionHistory = value; }
        }

        internal bool isControlledSet;
        private bool isControlled;
        /// <summary>
        /// True if this resource is under any kind of change control (even file copying), in which
        /// case revision history is created.
        /// </summary>
        public bool IsControlled
        {
            get { return this.isControlled; }
            set
            {
                this.isControlled = value;
                this.isControlledSet = true;
            }
        }

        /// <summary>
        /// Most recent revision in revision_history if is_controlled else “(uncontrolled)”.
        /// </summary>
        /// <returns></returns>
        public string CurrentRevision()
        {
            if (!this.IsControlled)
                return "uncontrolled";

            return this.RevisionHistory.MostRecentVersion();
        }

        private AssumedTypes.List<string> languagesAvailable;
        /// <summary>
        /// Total list of languages available in this resource, derived from original_language and
        /// translations.
        /// </summary>
        public AssumedTypes.List<string> LanguagesAvailable()
        {
            if (this.languagesAvailable != null && this.languagesAvailable.Count >0)
                return this.languagesAvailable;

            this.languagesAvailable = new OpenEhr.AssumedTypes.List<string>();
            this.languagesAvailable.Add(this.OriginalLanguage.CodeString);

            if (this.Translations != null)
            {
                foreach (string language in this.Translations.Keys)
                    this.languagesAvailable.Add(language);
            }

            DesignByContract.Check.Ensure(this.languagesAvailable.Count >0, "Must have at least one available language");
            DesignByContract.Check.Ensure(this.languagesAvailable.Has(this.OriginalLanguage.CodeString),
                "AuthoredResource available language(s) must contrain original language");

            return this.languagesAvailable;
        }

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            Check.Assert(reader.LocalName == "original_language", "Expected LocalName is 'original_language' rather than " + reader.LocalName);
            this.originalLanguage = new CodePhrase();
            this.OriginalLanguage.ReadXml(reader);

            if (reader.LocalName == "is_controlled")
            {
                this.IsControlled = reader.ReadElementContentAsBoolean("is_controlled", RmXmlSerializer.OpenEhrNamespace);

            }
            if (reader.LocalName == "description")
            {
                this.description = new ResourceDescription();
                this.description.ReadXml(reader);
            }

            string translationsNodeName = "translations";
            if (reader.LocalName == translationsNodeName)
            {
                System.Collections.Generic.Dictionary<string, TranslationDetails> translationDic = new Dictionary<string, TranslationDetails>();
                do
                {
                    TranslationDetails transDetails = new TranslationDetails();
                    transDetails.ReadXml(reader);

                    translationDic.Add(transDetails.Language.CodeString, transDetails);
                } while (reader.LocalName == translationsNodeName);

                if (translationDic.Count > 0)
                    this.translations = new OpenEhr.AssumedTypes.Hash<TranslationDetails, string>(translationDic);
            }

            if (reader.LocalName == "revision_history")
            {
                this.revisionHistory = new OpenEhr.RM.Common.Generic.RevisionHistory();
                this.revisionHistory.ReadXml(reader);
            }

        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            Check.Require(this.OriginalLanguage!=null, "AuthoredResource.OriginalLanguage must not be null.");

            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "original_language", RmXmlSerializer.OpenEhrNamespace);
            this.OriginalLanguage.WriteXml(writer);
            writer.WriteEndElement();

            if (this.isControlledSet)
            {
                writer.WriteStartElement(openEhrPrefix, "is_controlled", RmXmlSerializer.OpenEhrNamespace);
                writer.WriteString(this.IsControlled.ToString());
                writer.WriteEndElement();
            }

            if (this.Description != null)
            {
                writer.WriteStartElement(openEhrPrefix, "description", RmXmlSerializer.OpenEhrNamespace);
                this.Description.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.Translations != null)
            {
                foreach (string language in this.Translations.Keys)
                {
                    writer.WriteStartElement(openEhrPrefix, "translations", RmXmlSerializer.OpenEhrNamespace);
                    this.Translations.Item(language).WriteXml(writer);
                    writer.WriteEndElement();
                }
            }

            if (this.RevisionHistory != null)
            {
                writer.WriteStartElement(openEhrPrefix, "revision_history", RmXmlSerializer.OpenEhrNamespace);
                this.RevisionHistory.WriteXml(writer);
                writer.WriteEndElement();
            }
        }
    }
}
