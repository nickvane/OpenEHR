using System;
using System.ComponentModel;
using OpenEhr.DesignByContract;
using System.Text.RegularExpressions;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Support.Identification
{
    [TypeConverter(typeof(TerminologyIdTypeConverter))]
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "SUPPORT", "TERMINOLOGY_ID")]
    public class TerminologyId : ObjectId, System.Xml.Serialization.IXmlSerializable
    {
        // 03/08/09 needs to allow a single character as terminology id value. Same for version as well.
        const string pattern = @"^(?<name>[a-zA-Z][a-zA-Z0-9_\-/+]*)(\((?<version>[a-zA-Z0-9][a-zA-Z0-9_\-/.]*)\))?$";

        public TerminologyId() 
        { }

        public TerminologyId(string value) 
            : this() 
        {
            SetBaseData(value);
        }

        Match valueMatch;

        private Match ValueMatch
        {
            get
            {
                if (valueMatch == null)
                {
                    valueMatch = Regex.Match(Value, pattern, RegexOptions.Compiled | RegexOptions.Singleline);

                    Check.Ensure(valueMatch != null, "valueMatch must not be null");
                    Check.Ensure(valueMatch.Success, "valueMatch success must be true");
                    Check.Ensure(valueMatch.Groups["name"] != null, "valueMatch must contain name group");
                    Check.Ensure(!string.IsNullOrEmpty(valueMatch.Groups["name"].Value), "name group value must not be null or empty");
                }
                return valueMatch;
            }
        }

        [Browsable(false)]
        public string Name
        {
            get 
            { 
                return ValueMatch.Groups["name"].Value;
            }
        }

        [Browsable(false)]
        public string VersionId
        {
            get
            {
                Group group = ValueMatch.Groups["version"];
                return group.Value;
            }
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

        const string RmTypeName = "TERMINOLOGY_ID";

        protected override bool IsValidValue(string value)
        {
            return IsValid(value);
        }

        public static bool IsValid(string value)
        {
            Check.Require(value != null, "value must not be null");

            return Regex.IsMatch(value, pattern, RegexOptions.Compiled | RegexOptions.Singleline);
        }
    }
}
