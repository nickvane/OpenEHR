using System;
using System.ComponentModel;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.AssumedTypes;
using OpenEhr.RM.DataTypes.Text.Impl;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Text
{
    [TypeConverter(typeof(TextValueTypeConverter))]
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_TEXT")]
    public class DvText : OpenEhr.RM.DataTypes.Basic.DataValue, System.Xml.Serialization.IXmlSerializable, IFormattable
    {
        public DvText()             
        { }

        public DvText(string value) 
            : this() 
        {
            this.SetValue(value);

            this.CheckInvariants();
        }

        public DvText(string value, OpenEhr.RM.DataTypes.Uri.DvUri hyperlink, string formatting,
            List<TermMapping> mappings, CodePhrase language, CodePhrase encoding)
            : this()
        {
            this.SetBaseData(value, hyperlink, formatting, mappings, language, encoding);

            this.CheckInvariants();
        }

        private string value;

        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        protected void SetValue(string value)
        {
            Check.Require(value != null, "value must not be null");

            this.value = value;
        }

        protected void SetBaseData(string value, OpenEhr.RM.DataTypes.Uri.DvUri hyperlink, string formatting,
            List<TermMapping> mappings, CodePhrase language, CodePhrase encoding)
        {
            this.value = value;
            this.hyperlink = hyperlink;
            this.formatting = formatting;

            if (mappings != null && mappings.Count > 0)
            {
                if (this.mappings == null)
                    this.mappings = new OpenEhr.AssumedTypes.List<TermMapping>();
                foreach (TermMapping mapping in mappings)
                {
                    this.mappings.Add(mapping);
                }
            }
            this.language = language;
            this.encoding = encoding;
        }

        private string formatting;

        [DefaultValue(null)]
        [Browsable(false)]
        public string Formatting
        {
            get { return this.formatting; }
        }

        private CodePhrase language;

        [DefaultValue(null)]
        [Browsable(false)]
        [RmAttribute("language")]
        [RmCodeset("languages", "ISO_639-1")]
        public CodePhrase Language
        {
            get
            {
                return this.language;
            }
        }

        private CodePhrase encoding;

        [DefaultValue(null)]
        [Browsable(false)]
        [RmAttribute("encoding")]
        [RmCodeset("character sets", "IANA_character-sets")]
        public CodePhrase Encoding
        {
            get
            {
                return this.encoding;
            }
        }

        // 26/05/09 change to use Support.Assumed.List, otherwise, mappings would be
        // treated as non-openehr type. This would affect the datatype conversion in query result set.
        private AssumedTypes.List<TermMapping> mappings;
        [Browsable(false)]
        public AssumedTypes.List<TermMapping> Mappings
        {
            get
            {
                return this.mappings;
            }
        }
        
        private Uri.DvUri hyperlink;

        [DefaultValue(null)]
        [Browsable(false)]
        public Uri.DvUri Hyperlink
        {
            get {
                return this.hyperlink;
            } 
        }

        public override string ToString()
        {
            return this.Value;
        }

        public override bool Equals(object obj)
        {
            DvText textValue = obj as DvText;
            if (textValue == null)
                return false;

            if (textValue.GetType() != this.GetType())
                return false;

            return this.Value.Equals(textValue.Value);
        }

        #region IXmlSerializable Members
       

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            //reader.ReadStartElement();
            //reader.MoveToContent();

            string openEhrNamespace = RmXmlSerializer.OpenEhrNamespace;

            Check.Assert(reader.LocalName == "value", "reader.LocalName must be 'value'");
            //this.value = reader.ReadElementString("value", openEhrNamespace);
            //this.valueSet = true;
            SetValue(reader.ReadElementString("value", openEhrNamespace));

            reader.MoveToContent();

            if (reader.LocalName == "hyperlink")
            {
                string linkType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                if (linkType != null)
                    this.hyperlink = new OpenEhr.RM.DataTypes.Uri.DvEhrUri();
                else
                    this.hyperlink = new OpenEhr.RM.DataTypes.Uri.DvUri();
                this.hyperlink.ReadXml(reader);                
            }

            if (reader.LocalName == "formatting")
            {
                this.formatting = reader.ReadElementString("formatting", openEhrNamespace);
                //this.formattingSet = true;
            }

           // TODO: TermMapping
            if (reader.LocalName == "mappings")
            {
                this.mappings = new OpenEhr.AssumedTypes.List<TermMapping>(); //new List<TermMapping>();
                do
                {
                    TermMapping mapping = new TermMapping();
                    mapping.ReadXml(reader);
                    this.mappings.Add(mapping);

                } while (reader.LocalName == "mapping" && reader.NodeType == System.Xml.XmlNodeType.Element);
            }

            // language
            if (reader.LocalName == "language")
            {
                if (this.language == null)
                    this.language = new CodePhrase();
                this.language.ReadXml(reader);
                //this.languageSet = true;                
            }

            // encoding  
            if (reader.LocalName == "encoding")
            {
                if (this.encoding == null)
                    this.encoding = new CodePhrase();
                this.encoding.ReadXml(reader);
                //this.encodingSet = true;
            } 
        }
        
        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {            
            string openEhrNamespace = RmXmlSerializer.OpenEhrNamespace;
            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer); 
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            writer.WriteElementString(prefix, "value", openEhrNamespace,this.Value);

            if (this.Hyperlink != null)
            {
                writer.WriteStartElement(prefix, "hyperlink", openEhrNamespace);
                if (this.hyperlink.GetType() == typeof(OpenEhr.RM.DataTypes.Uri.DvEhrUri))
                {
                    string hyperLinkType = "DV_EHR_URI";
                    if (!string.IsNullOrEmpty(prefix))
                        hyperLinkType = prefix + ":" + hyperLinkType;
                    writer.WriteAttributeString(xsiPrefix, "type", openEhrNamespace, hyperLinkType);
                }
                this.Hyperlink.WriteXml(writer);
                writer.WriteEndElement();               
            }
            if (this.Formatting != null)
                writer.WriteElementString(prefix, "formatting", openEhrNamespace, this.formatting);
            if (this.Mappings != null && this.Mappings.Count > 0)
            {
                foreach (TermMapping mapping in this.Mappings)
                {
                    writer.WriteStartElement(prefix, "mappings", openEhrNamespace);
                    mapping.WriteXml(writer);
                    writer.WriteEndElement();  
                }
            }
            if (this.Language != null)
            {
                writer.WriteStartElement("language", openEhrNamespace);
                this.Language.WriteXml(writer);
                writer.WriteEndElement();
            }
            if (this.Encoding != null)
            {
                writer.WriteStartElement("encoding", openEhrNamespace);
                this.Encoding.WriteXml(writer);
                writer.WriteEndElement();
            }
          
        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            //return GetXmlSchema(xs, "DV_TEXT");    
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("DV_TEXT", RmXmlSerializer.OpenEhrNamespace);

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

        #endregion

        protected override void CheckInvariants()
        {
            Check.Invariant(this.Value != null, "Value must not be null.");
            Check.Invariant(this.Mappings == null || this.Mappings.Count > 0, "Mappings is not null implies mappings is not empty.");
            Check.Invariant(this.Formatting == null || this.Formatting.Length > 0, "formatting /= void implies not formatting.is_empty");
        }

        #region IFormattable Members

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return this.ToString();
        }

        #endregion
    }
}
