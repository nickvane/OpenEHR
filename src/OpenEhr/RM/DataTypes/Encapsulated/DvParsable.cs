using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Encapsulated
{
    [Serializable]
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [RmType("openEHR", "DATA_TYPES", "DV_PARSABLE")]
    public class DvParsable : DvEncapsulated, System.Xml.Serialization.IXmlSerializable
    {
        public DvParsable() 
        { }

        public DvParsable(string value, string formalism) 
            : this() 
        {
            DesignByContract.Check.Require(value != null, "value must not be null");  // can be empty
            DesignByContract.Check.Require(!string.IsNullOrEmpty(formalism), "formalism must not be null or empty");

            this.value = value;
            this.formalism = formalism;

            this.CheckInvariants();
        }

        public DvParsable(CodePhrase charset, CodePhrase language, string value, string formalism)
            : this()
        {
            DesignByContract.Check.Require(value != null, "value must not be null");  // can be empty
            DesignByContract.Check.Require(!string.IsNullOrEmpty(formalism), "formalism must not be null or empty");
            
            this.SetBaseData(charset, language);
            this.value = value;
            this.formalism = formalism;

            this.CheckInvariants();
        }

        string value;

        public string Value
        {
            get { return this.value; }
        }

        string formalism;

        public string Formalism
        {
            get { return this.formalism; }
        }

        public override int Size
        {
            get { throw new Exception("The method or operation is not implemented."); }
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
            return new System.Xml.XmlQualifiedName("DV_PARSABLE", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            Check.Assert(reader.LocalName == "value", "Expected LocalName is 'value' rather than " + reader.LocalName);
            this.value = reader.ReadElementString("value", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "formalism", "Expected LocalName is 'formalism' rather than " + reader.LocalName);
            this.formalism = reader.ReadElementString("formalism", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();

        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            Check.Require(this.Value!=null, "value must not be null.");
            Check.Require(this.Formalism!=null, "formalism must not be null.");

            base.WriteXmlBase(writer);

            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            writer.WriteElementString(prefix, "value", RmXmlSerializer.OpenEhrNamespace, this.Value);
            writer.WriteElementString(prefix, "formalism", RmXmlSerializer.OpenEhrNamespace, this.Formalism);
            
        }

        protected override void CheckInvariants()
        {
            base.CheckInvariants();
            Check.Invariant(this.Value != null, "value must not be null.");
            Check.Invariant(this.Formalism!=null, "formalism must not be null.");
        }
    }
}
