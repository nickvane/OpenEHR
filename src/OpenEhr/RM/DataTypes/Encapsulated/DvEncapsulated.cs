using System;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Attributes;

namespace OpenEhr.RM.DataTypes.Encapsulated
{
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_ENCAPSULATED")]
    public abstract class DvEncapsulated : DataTypes.Basic.DataValue
    {
        private Text.CodePhrase charset;

        [RmAttribute("charset")]
        [RmCodeset("character sets", "IANA_character-sets")]
        public Text.CodePhrase Charset
        {
            get
            {
                return this.charset;
            }
        }

        private Text.CodePhrase language;

        [RmAttribute("language")]
        [RmCodeset("languages", "ISO_639-1")]
        public Text.CodePhrase Language
        {
            get
            {
                return this.language;
            }
        }

        abstract public int Size
        {
            get;
        }

        protected void SetBaseData(CodePhrase charset, CodePhrase language)
        {
            this.charset = charset;
            this.language = language;
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {           
            if (reader.LocalName == "charset")
            {
                this.charset = new OpenEhr.RM.DataTypes.Text.CodePhrase();
                this.charset.ReadXml(reader);
            }

            if (reader.LocalName == "language")
            {
                this.language = new OpenEhr.RM.DataTypes.Text.CodePhrase();
                this.language.ReadXml(reader);
            }
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {           
            if (this.Charset != null)
            {
                this.Charset.WriteXml(writer);
            }
            if (this.Language != null)
                this.Language.WriteXml(writer);
        }

        protected override void CheckInvariants()
        {
            // %HYYKA%
            //Check.Invariant(this.Size >= 0, "size must be greater or equal to zero.");
        }
    }
}
