using System;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.Validation;

namespace OpenEhr.RM.Common.Resource
{
    /// <summary>
    /// Language-specific detail of resource description. When a resource is translated 
    /// for use in another language environment, each RESOURCE_DESCRIPTION_ITEM 
    /// needs to be copied and translated into the new language.
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "COMMON", "RESOURCE_DESCRIPTION_ITEM")]
    public class ResourceDescriptionItem : System.Xml.Serialization.IXmlSerializable
    {
        #region Constructors
        /// <summary>
        /// default constructor.
        /// </summary>
        public ResourceDescriptionItem() { }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="language">The localised language in which the items in this description item are written. Coded from
        /// openEHR Code Set “languages”.</param>
        /// <param name="purpose">Purpose of the resource.</param>
        public ResourceDescriptionItem(CodePhrase language, string purpose)
        {
            Check.Require(language != null, "language must not be null.");
            Check.Require(!string.IsNullOrEmpty(purpose), "purpose must not be null or empty.");

            this.language = language;
            this.purpose = purpose;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="language">The localised language in which the items in this description item are written. Coded from
        /// openEHR Code Set “languages”.</param>
        /// <param name="purpose">Purpose of the resource.</param>
        /// <param name="keywords">Keywords which characterise this resource, used e.g. for indexing and searching.</param>
        /// <param name="use">Description of the uses of the resource, i.e. contexts in which it could be used.</param>
        /// <param name="misuse">Description of any misuses of the resource, i.e. contexts in which it should not be used.</param>
        /// <param name="copyright">Optional copyright statement for the resource as a knowledge resource.</param>
        /// <param name="originalResourceUri">URIs of original clinical document(s) or description of which resource is a formalisation,
        /// in the language of this description item; keyed by meaning.</param>
        /// <param name="otherDetails">Additional language-senstive resource metadata, as a list of name/value pairs.</param>
        public ResourceDescriptionItem(CodePhrase language, string purpose, AssumedTypes.List<string> keywords,
            string use, string misuse, string copyright, AssumedTypes.Hash<string, string> originalResourceUri,
            AssumedTypes.Hash<string, string> otherDetails):this(language, purpose)
        {
            Check.Require(use == null || use != string.Empty, "if use is not null, it must not be empty");
            Check.Require(misuse == null || misuse != string.Empty, "if misuse is not null, it must not be empty");
            Check.Require(copyright == null || copyright != string.Empty, "if copyright is not null, it must not be empty");

            this.language = language;
            this.purpose = purpose;
            this.purpose = purpose;
            this.keywords = keywords;
            this.use = use;
            this.misuse = misuse;
            this.copyright = copyright;
            this.originalResourceUri = originalResourceUri;
            this.otherDetails = otherDetails;
        }
        #endregion

        #region class properties
        private CodePhrase language;
        /// <summary>
        /// The localised language in which the items in this description item are written. Coded from openEHR Code Set “languages”.
        /// </summary>
        public CodePhrase Language
        {
            get { return this.language; }
            set
            {
                Check.Require(value!= null, "language value must not be null.");
                this.language = value;
            }
        }

        private string purpose;
        /// <summary>
        /// Purpose of the resource.
        /// </summary>
        public string Purpose
        {
            get { return this.purpose; }
            set
            {
                // TODO need the first precondition checking when the xml issue is fixed
                Check.Require(value != null, "purpose value must not be null.");
                this.purpose = value;
            }
        }

        private AssumedTypes.List<string> keywords;
        /// <summary>
        /// Keywords which characterise this resource, used e.g. for indexing and searching.
        /// </summary>
        public AssumedTypes.List<string> Keywords
        {
            get { return this.keywords; }
            set
            {
                 this.keywords = value;
            }
        }

        private string use;
        /// <summary>
        /// Description of the uses of the resource, i.e. contexts in which it could be used.
        /// </summary>
        public string Use
        {
            get { return this.use; }
            set
            {
                Check.Require(value == null || value!= string.Empty, "use is not null, implies it must not be empty.");
                this.use = value;
            }
        }

        private string misuse;
        /// <summary>
        /// Description of any misuses of the resource, i.e. contexts in which it should not be used.
        /// </summary>
        public string Misuse
        {
            get { return this.misuse; }
            set
            {
                Check.Require(value == null || value != string.Empty, "misuse is not null, implies it must not be empty.");
                this.misuse = value;
            }
        }

        private string copyright;
        /// <summary>
        /// Optional copyright statement for the resource as a knowledge resource.
        /// </summary>
        public string Copyright
        {
            get { return this.copyright; }
            set
            {
                Check.Require(value == null || value != string.Empty, "copyright is not null, implies it must not be empty.");
                this.copyright = value;
            }
        }

        private AssumedTypes.Hash<string, string> originalResourceUri;
        /// <summary>
        /// URIs of original clinical document(s) or description of which resource is a formalisation, 
        /// in the language of this description item; keyed by meaning.
        /// </summary>
        public AssumedTypes.Hash<string, string> OriginalResourceUri
        {
            get { return this.originalResourceUri; }
            set
            {
                this.originalResourceUri = value;
            }
        }

        private AssumedTypes.Hash<string, string> otherDetails;
        /// <summary>
        /// Additional language-senstive resource metadata, as a list of name/value pairs.
        /// </summary>
        public AssumedTypes.Hash<string, string> OtherDetails
        {
            get { return this.otherDetails; }
            set
            {
                this.otherDetails = value;
            }
        }

        #endregion

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

        #region Internal Serializable functions

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("RESOURCE_DESCRIPTION_ITEM", RmXmlSerializer.OpenEhrNamespace);
        }

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "language", "Expected local name is 'language', not " + reader.LocalName);
            this.language = new CodePhrase();
            this.language.ReadXml(reader);

            Check.Assert(reader.LocalName == "purpose", "Expected local name is 'purpose', not " + reader.LocalName);
            this.purpose = reader.ReadElementString("purpose", RmXmlSerializer.OpenEhrNamespace);           
            reader.MoveToContent();

            while (reader.LocalName == "keywords")
            {
                if (this.keywords == null)
                    this.keywords = new OpenEhr.AssumedTypes.List<string>();
                this.keywords.Add(reader.ReadElementString("keywords", RmXmlSerializer.OpenEhrNamespace));
                reader.MoveToContent();
            }

            if (reader.LocalName == "use")
            {
                // TODO don't need this when the xml instance is fixed
                if (reader.IsEmptyElement)
                    reader.Skip();
                else
                    this.use = reader.ReadElementString("use", RmXmlSerializer.OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "misuse")
            {
                // TODO don't need this when the xml instance is fixed
                if (reader.IsEmptyElement)
                    reader.Skip();
                else
                    this.misuse = reader.ReadElementString("misuse", RmXmlSerializer.OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "copyright")
            {
                // TODO don't need this when the xml instance is fixed
                if (reader.IsEmptyElement)
                    reader.Skip();
                else
                    this.Copyright = reader.ReadElementString("copyright", RmXmlSerializer.OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "original_resource_uri")
            {
                System.Collections.Generic.Dictionary<string, string> originalResourceUriDic =
                    new System.Collections.Generic.Dictionary<string, string>();
                while (reader.LocalName == "original_resource_uri")
                {
                    string id = reader.GetAttribute("id");
                    if (string.IsNullOrEmpty(id))
                        throw new ValidationException("original_resource_uri must have id attribute and its value must not be null or empty.");
                    string value = reader.ReadElementString("original_resource_uri", RmXmlSerializer.OpenEhrNamespace);
                    reader.MoveToContent();
                    if (string.IsNullOrEmpty(value))
                        throw new ValidationException("original_resource_uri value must not be null or empty.");

                    originalResourceUriDic.Add(id, value);
                }
                if (originalResourceUriDic.Count <= 0)
                    throw new ValidationException("originalResourceUriDic must not be empty.");

                this.originalResourceUri = new OpenEhr.AssumedTypes.Hash<string, string>(originalResourceUriDic);
            }

            if (reader.LocalName == "other_details")
            {
                System.Collections.Generic.Dictionary<string, string> otherDetailsUriDic =
                    new System.Collections.Generic.Dictionary<string, string>();
                while (reader.LocalName == "other_details")
                {
                    string id = reader.GetAttribute("id");
                    if (string.IsNullOrEmpty(id))
                        throw new ValidationException("original_resource_uri must have id attribute and its value must not be null or empty.");
                    string value = reader.ReadElementString("original_resource_uri", RmXmlSerializer.OpenEhrNamespace);
                    reader.MoveToContent();
                    if (string.IsNullOrEmpty(value))
                        throw new ValidationException("original_resource_uri value must not be null or empty.");

                    otherDetailsUriDic.Add(id, value);
                }
                if (otherDetailsUriDic.Count <= 0)
                    throw new ValidationException("originalResourceUriDic must not be empty.");

                this.otherDetails = new OpenEhr.AssumedTypes.Hash<string, string>(otherDetailsUriDic);
            }

            if (!reader.IsStartElement())
            {
                reader.ReadEndElement();
                reader.MoveToContent();
            }           
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            Check.Require(this.Language != null, "Language must not be null.");
            Check.Require(this.Purpose!= null, "purpose must not be null.");
            Check.Require(this.Use == null || this.Use!=string.Empty, "use is not null, implies it must not be empty.");
            Check.Require(this.Copyright == null || this.Copyright != string.Empty, "Copyright is not null, implies it must not be empty.");
            Check.Require(this.Misuse == null || this.Misuse != string.Empty, "Misuse is not null, implies it must not be empty.");

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "language", RmXmlSerializer.OpenEhrNamespace);
            this.Language.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement(openEhrPrefix, "purpose", RmXmlSerializer.OpenEhrNamespace);
            writer.WriteString(this.Purpose);
            writer.WriteEndElement();

            if (this.Keywords != null)
            {
                foreach (string keyword in this.Keywords)
                {
                    writer.WriteStartElement(openEhrPrefix, "keywords", RmXmlSerializer.OpenEhrNamespace);
                    writer.WriteString(keyword);
                    writer.WriteEndElement();
                }
            }

            if (this.Use != null)
            {
                writer.WriteStartElement(openEhrPrefix, "use", RmXmlSerializer.OpenEhrNamespace);
                writer.WriteString(this.Use);
                writer.WriteEndElement();
            }

            if (this.Misuse != null)
            {
                writer.WriteStartElement(openEhrPrefix, "misuse", RmXmlSerializer.OpenEhrNamespace);
                writer.WriteString(this.Misuse);
                writer.WriteEndElement();
            }

            if (this.Copyright != null)
            {
                writer.WriteStartElement(openEhrPrefix, "copyright", RmXmlSerializer.OpenEhrNamespace);
                writer.WriteString(this.Copyright);
                writer.WriteEndElement();
            }

            if (this.OriginalResourceUri != null)
            {
                foreach (string id in this.OriginalResourceUri.Keys)
                {
                    writer.WriteStartElement(openEhrPrefix, "original_resource_uri", RmXmlSerializer.OpenEhrNamespace);
                    writer.WriteAttributeString("id", id);
                    writer.WriteString(this.OriginalResourceUri.Item(id).ToString());
                    writer.WriteEndElement();
                }
            }

            if (this.OtherDetails != null)
            {
                foreach (string id in this.OtherDetails.Keys)
                {
                    writer.WriteStartElement(openEhrPrefix, "other_details", RmXmlSerializer.OpenEhrNamespace);
                    writer.WriteAttributeString("id", id);
                    writer.WriteString(this.OtherDetails.Item(id).ToString());
                    writer.WriteEndElement();
                }
            }
        }
        #endregion

    }
}
