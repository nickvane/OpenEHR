using System;
using System.Text.RegularExpressions;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Support.Identification
{
    /// <summary>
    /// Identifier for archetypes.
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "SUPPORT", "ARCHETYPE_ID")]
    public class ArchetypeId : ObjectId, System.Xml.Serialization.IXmlSerializable
    {
        public static bool IsValid(string value)
        {
            Check.Require(value != null, "value must not be null");

            return Regex.IsMatch(value, archetypeIdPattern, RegexOptions.Compiled | RegexOptions.Singleline);
        }

        public ArchetypeId() 
        { }
        
        public ArchetypeId(string value) : this() 
        {
            SetBaseData(value);

            this.Initialise();
        }
        

        const string V_ALPHANUMERIC_NAME = "[a-zA-Z][a-zA-Z0-9_]+";        
        const string V_NUMBER = "[0-9]*";
        const string V_NONZERO_DIGIT = @"[1-9]";
        const string versionId = @"(?<VersionId>(v" + V_NONZERO_DIGIT + V_NUMBER + "))";
        const string specialisation = @"(?<Specialisation>" + V_ALPHANUMERIC_NAME + ")";
        const string conceptName = V_ALPHANUMERIC_NAME;
        const string rmEntity = @"(?<RmEntity>" + V_ALPHANUMERIC_NAME + ")";
        const string domainConcept = @"(?<DomainConcept>(" + conceptName + "([-]" + specialisation + ")*))";
        const string rmName = @"(?<RmName>" + V_ALPHANUMERIC_NAME + ")";
        const string rmOriginator = @"(?<RmOriginator>" + V_ALPHANUMERIC_NAME + ")";
        const string qualifiedRmEntity = @"(?<QualifiedRmEntity>(" + rmOriginator + "-" + rmName + "-" + rmEntity+"))";
        const string archetypeIdPattern = @"^(?<ArchetypeId>(" + qualifiedRmEntity + @"\." + domainConcept + @"\." + versionId + "))$";
        
       //[a-zA-Z][a-zA-Z0-9_]+(-[a-zA-Z][a-zA-Z0-9_]+){2}\.[a-zA-Z][a-zA-Z0-9_]+(-[azA-Z][a-zA-Z0-9_]+)*\.v[1-9][0-9]*
        System.Text.RegularExpressions.GroupCollection matchGroups;

        private void Initialise()
        {
            System.Text.RegularExpressions.Match match
                = System.Text.RegularExpressions.Regex.Match(this.Value, archetypeIdPattern, RegexOptions.Compiled | RegexOptions.Singleline);

            if (!match.Success)
                throw new InvalidOperationException("Archetype ID not valid format");

            this.matchGroups = match.Groups;
        }

        /// <summary>
        /// Globally qualified reference model entity,
        /// e.g. “openehr-composition-OBSERVATION”.
        /// </summary>
        public string QualifiedRmEntity
        {
            get { return RmOriginator + "-" + RmName + "-" + RmEntity; }
        }

        /// <summary>
        /// Organisation originating the reference model on which this archetype 
        /// is based, e.g.“openehr”, “cen”, “hl7”.
        /// </summary>
        public string RmOriginator
        {
            get { return this.matchGroups["RmOriginator"].ToString(); }
        }

        /// <summary>
        /// Name of the reference model, e.g. “rim”, “ehr_rm”, “en13606”.
        /// </summary>
        public string RmName
        {
            get { return matchGroups["RmName"].ToString(); }
        }

        /// <summary>
        /// Name of the ontological level within the reference model to 
        /// which this archetype is targeted, e.g. for openEHR, “folder”,
        /// “composition”, “section”, “entry”.
        /// </summary>
        public string RmEntity
        {
            get { return matchGroups["RmEntity"].ToString(); }
        }

        /// <summary>
        /// Name of the concept represented by this archetype, including 
        /// specialisation, e.g. “biochemistry_result-cholesterol”.
        /// </summary>
        public string DomainConcept
        {
            get { return matchGroups["DomainConcept"].ToString(); }
        }

        /// <summary>
        /// Name of specialisation of concept, if this archetype is a 
        /// specialisation of another archetype, e.g. “cholesterol”.
        /// </summary>
        public string Specialisation
        {
            get
            {
                if (matchGroups["Specialisation"].Success)
                    return matchGroups["Specialisation"].ToString();
                else
                    return null;
            }
        }

        /// <summary>
        /// Version of this archetype.
        /// </summary>
        public string VersionId
        {
            get { return matchGroups["VersionId"].ToString(); }
        }


        const string RmTypeName = "ARCHETYPE_ID";

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

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);
            //this.SetInnerData();
            this.Initialise();
        }        

        #endregion

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName(RmTypeName, RmXmlSerializer.OpenEhrNamespace);
        }

        protected override bool IsValidValue(string value)
        {
            return IsValid(value);
        }

    }
}
