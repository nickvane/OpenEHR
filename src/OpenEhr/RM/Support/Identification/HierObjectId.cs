using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Support.Identification
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "SUPPORT", "HIER_OBJECT_ID")]
    public class HierObjectId : UidBasedId, System.Xml.Serialization.IXmlSerializable
    {
        public static HierObjectId NewObjectId()
        {
            return new HierObjectId(Guid.NewGuid().ToString());
        }

        public static bool IsValid(string value)
        {
            Check.Require(value != null, "value must not be null");

            string[] strings = value.Split(new string[] { "::" }, 2, StringSplitOptions.RemoveEmptyEntries);
            
            if (strings.Length == 0)
                return false;

            if (!Uid.IsValid(strings[0]))
                return false;

            // check for ObjectVersionId delimiters
            if (strings.Length > 1 && strings[1].Contains("::"))
                return false;

            return true;
        }

        public HierObjectId()
        { }

        public HierObjectId(string value)
            : this()
        {
            SetBaseData(value);
        }

        public HierObjectId(Uid root, string extension)
            : this(root != null ? root.Value + (string.IsNullOrEmpty(extension) ? "" : "::" + extension) : null)
        { }

        protected override bool IsValidValue(string value)
        {
            return IsValid(value);
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
            return new System.Xml.XmlQualifiedName(RmTypeName, RmXmlSerializer.OpenEhrNamespace);
        }

        const string RmTypeName = "HIER_OBJECT_ID";
    }
}
