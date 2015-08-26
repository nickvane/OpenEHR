using System;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Common.Resource
{
    /// <summary>
    /// Class providing details of a natural language translation.
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "COMMON", "TRANSLATION_DETAILS")]
    public class TranslationDetails : System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>
        /// A default constructor.
        /// </summary>
        public TranslationDetails() { }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="language">Language of translation</param>
        /// <param name="author">Translator name and other demographic details</param>
        public TranslationDetails(CodePhrase language, AssumedTypes.Hash<string, string> author)
        {
            this.language = language;
            this.author = author;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="language">Language of translation</param>
        /// <param name="author">Translator name and other demographic details</param>
        /// <param name="accreditation">Accreditation of translator, usually a national translator’s association id</param>
        /// <param name="otherDetails">Any other meta-data</param>
        public TranslationDetails(CodePhrase language, AssumedTypes.Hash<string, string> author,
            string accreditation, AssumedTypes.Hash<string, string> otherDetails)
        {
            this.language = language;
            this.author = author;
            this.accreditation = accreditation;
            this.otherDetails = otherDetails;
        }

        private CodePhrase language;
        /// <summary>
        /// Language of translation
        /// </summary>
        public CodePhrase Language
        {
            get
            {
                return language;
            }
        }

        private AssumedTypes.Hash<string, string> author;
        /// <summary>
        /// Translator name and other demographic details
        /// </summary>
        public AssumedTypes.Hash<string, string> Author
        {
            get
            {
                return this.author;
            }
        }

        private string accreditation;
        /// <summary>
        /// Accreditation of translator, usually a national translator’s association id
        /// </summary>
        public string Accreditation
        {
            get { return this.accreditation; }
        }

        private AssumedTypes.Hash<string, string> otherDetails;
        /// <summary>
        /// Any other meta-data
        /// </summary>
        public AssumedTypes.Hash<string, string> OtherDetails
        {
            get { return this.otherDetails; }
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
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("TRANSLATION_DETAILS", RmXmlSerializer.OpenEhrNamespace);
        }

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "language", "Expected local name is 'language', not " + reader.LocalName);
            this.language = new CodePhrase();
            this.language.ReadXml(reader);

            Check.Assert(reader.LocalName == "author", "Expected local name is 'author', not " + reader.LocalName);

            System.Collections.Generic.Dictionary<string, string> authorDic 
                = new System.Collections.Generic.Dictionary<string, string>();
            while (reader.LocalName == "author")
            {
                string id = reader.GetAttribute("id");
                Check.Assert(!string.IsNullOrEmpty(id), "attribute 'id' must not be null or empty for author");
                string value = reader.ReadElementContentAsString();

                authorDic.Add(id, value);
               
            }
            Check.Assert(authorDic.Count>0, "Expected at least one author item in the dictionary.");
            this.author = new OpenEhr.AssumedTypes.Hash<string, string>(authorDic);

            if (reader.LocalName == "accreditation")
            {
                this.accreditation = reader.ReadElementString("accreditation", RmXmlSerializer.OpenEhrNamespace);
            }

            System.Collections.Generic.Dictionary<string, string> otherDetailsDic 
                = new System.Collections.Generic.Dictionary<string,string>();
            while (reader.LocalName == "other_details")
            {                
                string id = reader.GetAttribute("id");
                Check.Assert(!string.IsNullOrEmpty(id), "attribute 'id' must not be null or empty for other_details");
                string value = reader.ReadElementString("other_details", RmXmlSerializer.OpenEhrNamespace);

                otherDetailsDic.Add(id, value);

            }
            if (otherDetailsDic.Count > 0)
                this.otherDetails = new OpenEhr.AssumedTypes.Hash<string, string>(otherDetailsDic);

            reader.MoveToContent();

            if (!reader.IsStartElement())
            {
                reader.ReadEndElement();
                reader.MoveToContent();
            }
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            Check.Require(this.Language!= null, "Language must not be null.");
            Check.Require(this.Author != null, "Author must not be null.");

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            
            writer.WriteStartElement(openEhrPrefix, "language", RmXmlSerializer.OpenEhrNamespace);
            this.Language.WriteXml(writer);
            writer.WriteEndElement();

            foreach (string id in this.Author.Keys)
            {
                writer.WriteStartElement(openEhrPrefix, "author", RmXmlSerializer.OpenEhrNamespace);
                writer.WriteAttributeString("id", id);
                writer.WriteString(this.Author.Item(id).ToString());
                writer.WriteEndElement();
            }

            if (!string.IsNullOrEmpty(this.Accreditation))
            {
                writer.WriteStartElement(openEhrPrefix, "accreditation", RmXmlSerializer.OpenEhrNamespace);
                writer.WriteString(this.Accreditation);
                writer.WriteEndElement();
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

    }
}
