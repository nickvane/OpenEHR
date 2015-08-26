using System;
using System.Xml;
using System.Text.RegularExpressions;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.AssumedTypes;

namespace OpenEhr.RM.DataTypes.Text
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_CODED_TEXT")]
    public class DvCodedText : DvText, System.Xml.Serialization.IXmlSerializable
    {
        public DvCodedText()
        { }

        public DvCodedText(string codeString)
            : this("", codeString, "local")
        { }

        public DvCodedText(string value, string codeString, string terminologyId)
            : this()
        {
            this.SetValue(value);
            this.definingCode = new CodePhrase(codeString, terminologyId);
        }

        public DvCodedText(string value, OpenEhr.RM.DataTypes.Uri.DvUri hyperlink, string formatting,
            List<TermMapping> mappings, CodePhrase language, CodePhrase encoding, string codeString, 
            string terminologyId)
            : this()
        {
            this.SetBaseData(value, hyperlink, formatting, mappings, language, encoding);            
            this.definingCode = new CodePhrase(codeString, terminologyId);

            this.CheckInvariants();
        }

        private CodePhrase definingCode;

        public CodePhrase DefiningCode
        {
            get
            {
                return this.definingCode;
            }
            set
            {
                Check.Invariant(value != null, "Defining code must not be null.");
                this.definingCode = value;
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Value))
                return this.DefiningCode.ToString();
            else
                return this.DefiningCode.ToString() + "::" + this.Value;
        }

        const string stringRegExValue = @"^(?<terminology_id>\w+)::(?<code_string>\w+)::(?<value>\w+)$";
        static object stringRegExLockObject = new object();

        static public DvCodedText ParseString(string codedTextString)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(codedTextString), "codedTextString must not be null or empty");
 
            Match match = Regex.Match(codedTextString, stringRegExValue, RegexOptions.Compiled | RegexOptions.Singleline);
            if (!match.Success)
                throw new ApplicationException("Invalid DV_CODED_TEXT string");

            System.Text.RegularExpressions.Group value = match.Groups["value"];
            System.Text.RegularExpressions.Group codeString = match.Groups["code_string"];
            System.Text.RegularExpressions.Group terminologyId = match.Groups["terminology_id"];

            if (value == null)
                throw new ApplicationException("value must not be null");
            if (codeString == null)
                throw new ApplicationException("codeString must not be null");
            if (terminologyId == null)
                throw new ApplicationException("terminologyId must not be null");

            return new DvCodedText(value.Value, codeString.Value, terminologyId.Value);
        }

        public override bool Equals(object obj)
        {
            DvCodedText codedText = obj as DvCodedText;
            if (codedText == null)
                return false;

            if (!base.Equals(obj))
                return false;

            return this.DefiningCode.Equals(codedText.DefiningCode);
        }


        #region IXmlSerializable Members

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void ReadXmlBase(XmlReader reader)
        {
            // Get DV_TEXT data
            base.ReadXmlBase(reader);

            // Get defining_code data
            Check.Assert(reader.LocalName == "defining_code", "reader.LocalName must be 'defining_code'");
            if (this.definingCode == null)
                this.definingCode = new CodePhrase();
            this.definingCode.ReadXml(reader);

        }

        protected override void WriteXmlBase(XmlWriter writer)
        {
            // Serialize the data inherited from DV_TEXT
            base.WriteXmlBase(writer);

            // serialize defining _code
            string openEhrNamespace = RmXmlSerializer.OpenEhrNamespace;
            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer); 

            Check.Assert(this.DefiningCode != null, "DV_CODEDED_TEXT defining_code must not be null.");
            writer.WriteStartElement("defining_code", openEhrNamespace);
            this.DefiningCode.WriteXml(writer);
            writer.WriteEndElement();           
        }

        #endregion

        public static XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new XmlQualifiedName("DV_CODED_TEXT", RmXmlSerializer.OpenEhrNamespace);

        }
        
        #region IXmlSerializable Members


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
            base.CheckInvariants();
            Check.Invariant(this.DefiningCode != null, "DefiningCode must not be null.");
        }
    }
}
