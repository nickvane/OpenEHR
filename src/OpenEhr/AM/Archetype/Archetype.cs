using System;
using System.Linq;
using OpenEhr.RM.Support.Identification;
using OpenEhr.AM.Archetype.ConstraintModel;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Generic;
using OpenEhr.DesignByContract;
using OpenEhr.AM.Archetype.Ontology;
using OpenEhr.AssumedTypes;
using OpenEhr.Resources;
using OpenEhr.Attributes;
using OpenEhr.RM.Support.Terminology;
using OpenEhr.Validation;
using OpenEhr.Serialisation;
using OpenEhr.Paths;
using OpenEhr.RM.Common.Resource;

namespace OpenEhr.AM.Archetype
{
    /// <summary>
    /// Archetype equivalent to ARCHETYPED class in Common reference model. 
    /// Defines semantics of identfication, lifecycle, versioning, composition and specialisation.
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [AmType("ARCHETYPE")]
    public class Archetype : AuthoredResource, System.Xml.Serialization.IXmlSerializable
    {
        #region constructors
        public Archetype() : base() { }

        public Archetype(ArchetypeId archetypeId, string concept, CComplexObject definition,
            ArchetypeOntology ontology, CodePhrase originalLanguage, RevisionHistory revisionHistory,
            bool isControlled)
            : base(originalLanguage, revisionHistory, isControlled)
        {
            Check.Require(archetypeId != null, string.Format(CommonStrings.XMustNotBeNull, "archetypeId"));
            Check.Require(!string.IsNullOrEmpty(concept) != null, string.Format(CommonStrings.XMustNotBeNullOrEmpty, "concept"));
            Check.Require(definition != null, string.Format(CommonStrings.XMustNotBeNull, "definition"));
            Check.Require(ontology != null, string.Format(CommonStrings.XMustNotBeNull, "ontology"));

            this.archetypeId = archetypeId;
            this.definition = definition;
            this.ontology = ontology;
        }
        #endregion

        #region class properties

        string adlVersion;
        /// <summary>
        /// ADL version if archteype was read in from an ADL sharable archetype.
        /// </summary>
        public string AdlVersion
        {
            get { return this.adlVersion; }
            set
            {
                this.adlVersion = value;
            }
        }

        private ArchetypeId archetypeId;
        /// <summary>
        /// Multi-axial identifier of this archetype in archetype space.
        /// </summary>
        public ArchetypeId ArchetypeId
        {
            get { return this.archetypeId; }
            set
            {
                Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "ArchetypeId value"));
                this.archetypeId = value;
            }
        }

        private HierObjectId uid;
        /// <summary>
        /// OID identifier of this archetype.
        /// </summary>
        public HierObjectId Uid
        {
            get { return this.uid; }
            set
            {
                this.uid = value;
            }
        }

        private string concept;
        /// <summary>
        /// The normative meaning of the archetype as a whole, 
        /// expressed as a local archetype code, typically “at0000”.
        /// </summary>
        public string Concept
        {
            get { return this.concept; }
            set
            {
                Check.Require(!string.IsNullOrEmpty(value), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Concept value"));
                this.concept = value;
            }
        }

        private ArchetypeId parentArchetypeId;
        /// <summary>
        /// Identifier of the specialisation parent of this archetype.
        /// </summary>
        public ArchetypeId ParentArchetypeId
        {
            get { return this.parentArchetypeId; }
            set { this.parentArchetypeId = value; }
        }

        private CComplexObject definition;
        /// <summary>
        /// Root node of this archetype
        /// </summary>
        public CComplexObject Definition
        {
            get { return this.definition; }
            set
            {
                Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "Definition value"));
                this.definition = value;
            }
        }

        private ArchetypeOntology ontology;
        public ArchetypeOntology Ontology
        {
            get { return this.ontology; }
            set
            {
                Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "Ontology value"));
                this.ontology = value;
            }
        }

        private Set<Assertion.Assertion> invariants;
        /// <summary>
        /// Invariant statements about this object. Statements are expressed in first order predicate
        /// logic, and usually refer to at least two attributes.
        /// </summary>
        public Set<Assertion.Assertion> Invariants
        {
            get { return invariants; }
            set
            {
                Check.Require(value == null || !value.IsEmpty(),
                    string.Format(CommonStrings.IfXIsNotNullMustBeEmpty, "Invariants value"));
                invariants = value;
            }
        }

        private System.Collections.Generic.Dictionary<string, CDefinedObject> constraintRepository;
        /// <summary>
        /// An internal property used to hold all archetypeConstraint objects of this archetype.
        /// This dictionary keyed by archetype path.
        /// </summary>
        internal System.Collections.Generic.Dictionary<string, CDefinedObject> ConstraintRepository
        {
            get
            {
                if (constraintRepository == null)
                    constraintRepository = new System.Collections.Generic.Dictionary<string, CDefinedObject>();
                return constraintRepository;
            }
        }

        #endregion

        #region Functions
        /// <summary>
        /// Version of this archetype, extracted from id.
        /// </summary>
        /// <returns></returns>
        public string Version()
        {
            Check.Require(this.ArchetypeId != null, string.Format(CommonStrings.XMustNotBeNull, "ArchetypeId"));

            return this.ArchetypeId.VersionId;
        }

        /// <summary>
        /// Version of predecessor archetype of this archetype, if any.
        /// </summary>
        /// <returns></returns>
        public string PreviousVersion()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The short concept name of the archetype extracted from the archetype_id.
        /// </summary>
        /// <returns></returns>
        public string ShortConceptName()
        {
            Check.Require(this.ArchetypeId != null, string.Format(CommonStrings.XMustNotBeNull, "ArchetypeId"));
            return this.ArchetypeId.DomainConcept;
        }

        /// <summary>
        /// The concept name of the archetype in language a_lang; corresponds to the term definition of
        /// the concept attribute in the archetype ontology.
        /// </summary>
        /// <param name="aLanguage">language</param>
        /// <returns></returns>
        public string ConceptName(string aLanguage)
        {
            Check.Require(!string.IsNullOrEmpty(aLanguage), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "aLanguage"));
            Check.Require(this.Definition != null, string.Format(CommonStrings.XMustNotBeNull, "Definition"));

            ArchetypeTerm termDefinition = this.Ontology.TermDefinition(aLanguage, this.Definition.NodeId);

            if (termDefinition == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "termDefinition"));

            return termDefinition.Items.Item("text");
        }

        private AssumedTypes.Set<string> physicalPaths;

        /// <summary>
        /// Set of language-independent paths extracted from archetype. Paths obey Xpath-like syntax
        /// and are formed from alternations of C_OBJECT.node_id and C_ATTRIBUTE.rm_attribute_name values.
        /// </summary>
        /// <returns></returns>
        public AssumedTypes.Set<string> PhysicalPaths()
        {
            Check.Require(this.Definition != null, string.Format(CommonStrings.XMustNotBeNull, "Definition"));

            if (this.physicalPaths == null)
            {
                this.physicalPaths = new Set<string>(this.Definition.PhysicalPaths);
            }

            Check.Ensure(this.physicalPaths != null, "this.physicalPath must not be null.");

            return this.physicalPaths;
        }

        Set<string> logicalPaths;

        /// <summary>
        /// Set of language-dependent paths extracted from archetype. Paths obey the same syntax as
        /// physical_paths, but with node_ids replaced by their meanings from the ontology. 
        /// </summary>
        /// <param name="aLanguage">language</param>
        /// <returns></returns>
        public Set<String> LogicalPaths(string aLanguage)
        {
            Check.Require(this.ArchetypeId != null, string.Format(CommonStrings.XMustNotBeNull, "ArchetypeId"));
            Check.Require(this.ArchetypeId.Value != null, string.Format(CommonStrings.XMustNotBeNull, "ArchetypeId.Value"));
            
            Check.Require(this.Ontology.HasLanguage(aLanguage), string.Format(
                AmValidationStrings.OntologyMustContainLanguageY, this.ArchetypeId.Value, aLanguage));

            if (this.logicalPaths == null)
            {
                AssumedTypes.Set<string> physicalPath = this.PhysicalPaths();
                System.Collections.Generic.List<string> logicalPathList = new System.Collections.Generic.List<string>();
                foreach (string aPath in physicalPath)
                {
                    if (aPath == "/")
                        logicalPathList.Add(aPath);
                    else
                    {
                        string logicalPath = ToLogicalPath(aLanguage, aPath);
                        logicalPathList.Add(logicalPath);
                    }
                }

                this.logicalPaths = new Set<string>(logicalPathList);
            }

            DesignByContract.Check.Ensure(this.logicalPaths != null && !this.logicalPaths.IsEmpty(),
                "logicalPaths must not be null or empty.");

            return this.logicalPaths;
        }

        /// <summary>
        /// True if this archetype is a specialisation of another.
        /// </summary>
        /// <returns></returns>
        public bool IsSpecialised()
        {
            return this.ParentArchetypeId != null;
        }

        /// <summary>
        /// Specialisation depth of this archetype; larger than 0 if this archetype has a parent. Derived
        /// from ontology.specialisation_depth.
        /// </summary>
        /// <returns></returns>
        public int SpecialisationDepth()
        {
            Check.Require(this.Ontology != null, string.Format(CommonStrings.XMustNotBeNull, "Ontology"));

            return this.Ontology.SpecialisationDepth;
        }

        /// <summary>
        /// True if every node_id found on a C_OBJECT node is found in ontology.term_codes.
        /// </summary>
        /// <returns></returns>
        public bool NodeIdsValid()
        {
            Check.Require(this.Definition != null, string.Format(CommonStrings.XMustNotBeNull, "Definition"));
            Check.Require(this.Ontology != null, string.Format(CommonStrings.XMustNotBeNull, "Ontology"));

            if (!this.Ontology.HasTermCode(this.Definition.NodeId))
                return false;

            return NodeIdsValid(this.Definition);
        }      

        /// <summary>
        /// True if every ARCHETYPE_INTERNAL_REF.target_path refers to a legitimate node in the
        /// archetype definition.
        /// </summary>
        /// <returns></returns>
        public bool InternalReferencesValid()
        {
            Check.Require(this.Definition != null, string.Format(CommonStrings.XMustNotBeNull, "Definition"));

            if (this.Definition.Attributes != null)
            {
                foreach (CAttribute attri in this.Definition.Attributes)
                {
                    if (!InternalReferencesValid(attri))
                        return false;
                }
            }

            return true;
        }       
       
        /// <summary>
        /// True if every CONSTRAINT_REF.reference found on a C_OBJECT node in the archetype 
        /// definition is found in ontology.constraint_codes.
        /// </summary>
        /// <returns></returns>
        public bool ConstraintReferencesValid()
        {
            Check.Require(this.Definition != null, string.Format(CommonStrings.XMustNotBeNull, "Definition"));
            Check.Require(this.Ontology != null, string.Format(CommonStrings.XMustNotBeNull, "Ontology"));

            var cComplexObject = this.Definition;
            if (cComplexObject != null && cComplexObject.Attributes != null)
            {
                return cComplexObject.Attributes.All(this.ConstraintReferencesValid);
            }

            return true;
        }
       
        /// <summary>
        /// True if the archetype is valid overall; various tests should be used, including checks on
        /// node_ids, internal references, and constraint references.
        /// </summary>
        /// <returns></returns>
        public bool IsValid(ITerminologyService terminologyService)
        {
            if (!(this.NodeIdsValid() && this.InternalReferencesValid() && this.ConstraintReferencesValid()))
                return false;
            try
            {
                AmValidator.Validate(this, terminologyService);
            }
            catch(Exception ex)
            {
                if (ex.GetType() == typeof(RmInvariantException))
                    return false;

                throw ex;
            }

            return true;
        }

        #endregion

        #region helper functions

        /// <summary>
        /// Convert physicalPath to a logical path
        /// </summary>
        /// <param name="aLanguage"></param>
        /// <param name="physicalPath"></param>
        /// <returns></returns>
        private string ToLogicalPath(string aLanguage, string physicalPath)
        {
            Path pathProcessor = new Path(physicalPath);
            string logicalPath = null;
            do
            {
                logicalPath += "/" + pathProcessor.CurrentAttribute;

                if (pathProcessor.IsCurrentIdentified)
                {
                    ArchetypeTerm archetypeTerm = this.Ontology.TermDefinition(aLanguage, pathProcessor.CurrentNodeId);
                    string name = archetypeTerm.Items.Item("text");
                    if (string.IsNullOrEmpty(name))
                        throw new ApplicationException(string.Format(CommonStrings.XIsNullOrEmpty, name));
                    logicalPath += "[" + name + "]";
                }
            } while (pathProcessor.NextStep());

            Check.Ensure(!string.IsNullOrEmpty(logicalPath), "logicalPath must not be null or empty.");

            return logicalPath;
        }

        /// <summary>
        /// returns true if all NodeIds valid for an CAttribute object
        /// </summary>
        /// <param name="attri"></param>
        /// <returns></returns>
        private bool NodeIdsValid(CAttribute attri)
        {
            if (attri.Children != null)
            {
                foreach (CObject child in attri.Children)
                {
                    if (!string.IsNullOrEmpty(child.NodeId) && !this.Ontology.HasTermCode(child.NodeId))
                        return false;
                    CComplexObject cComplexObj = child as CComplexObject;
                    if (cComplexObj != null)
                        if (!NodeIdsValid(cComplexObj))
                            return false;
                }
            }

            return true;
        }

        /// <summary>
        /// returns true if all Nodeids valid for a CComplex object
        /// </summary>
        /// <param name="cComplexObj"></param>
        /// <returns></returns>
        private bool NodeIdsValid(CComplexObject cComplexObj)
        {
            if (cComplexObj.Attributes != null)
            {
                foreach (CAttribute attribute in cComplexObj.Attributes)
                {
                    if (!NodeIdsValid(attribute))
                        return false;
                }
            }

            return true;
        }

        private bool InternalReferencesValid(CAttribute cAttribute)
        {
            Check.Require(cAttribute != null, string.Format(CommonStrings.XMustNotBeNull, "cAttribute"));

            if (cAttribute.Children != null)
            {
                foreach (CObject cObj in cAttribute.Children)
                {
                    ArchetypeInternalRef archetypeInternalRef = cObj as ArchetypeInternalRef;
                    if (archetypeInternalRef != null)
                    {
                        CObject legitimateNode = GetCObjectAtTargetPath(this.Definition, archetypeInternalRef.TargetPath);
                        if (legitimateNode == null)
                            return false;
                    }

                    else
                    {
                        CComplexObject cComplexObj = cObj as CComplexObject;
                        if (cComplexObj != null && cComplexObj.Attributes != null)
                        {
                            foreach (CAttribute attri in cComplexObj.Attributes)
                                if (!InternalReferencesValid(attri))
                                    return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// an internal static method returning the targetPath corresponding CObject.
        /// Returns null if the targetPath doesn't have any associated CObject
        /// </summary>
        /// <param name="archetypeDefinition"></param>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        internal static CObject GetCObjectAtTargetPath(CComplexObject archetypeDefinition, string targetPath)
        {
            CComplexObject cObj = archetypeDefinition;
            CAttribute attribute = null;

            Path pathProcessor = new Path(targetPath);

            do
            {
                foreach (CAttribute cAttri in cObj.Attributes)
                {
                    if (cAttri.RmAttributeName == pathProcessor.CurrentAttribute)
                    {
                        attribute = cAttri;
                        break;
                    }
                }

                Check.Assert(attribute != null, string.Format(CommonStrings.XMustNotBeNull, "attribute"));

                if (attribute.RmAttributeName != pathProcessor.CurrentAttribute)
                    return null;

                foreach (CObject obj in attribute.Children)
                {
                    if (obj.NodeId == pathProcessor.CurrentNodeId)
                    {
                        cObj = obj as CComplexObject;
                        break;
                    }
                }

                Check.Assert(cObj != null, string.Format(CommonStrings.XMustNotBeNull, "cObj"));

                if (cObj.NodeId != pathProcessor.CurrentNodeId)
                    return null;

            } while (pathProcessor.NextStep());

            Check.Ensure(cObj.Path == targetPath, "cObj.Path must be the same as this.TargetPath");

            return cObj;
        }

        internal CDefinedObject GetCDefinedObjectAtPath(string targetPath)
        {
            Check.Require(!string.IsNullOrEmpty(targetPath), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "targetPath"));

            if (this.ConstraintRepository.ContainsKey(targetPath))
                return this.ConstraintRepository[targetPath];

            return GetCObjectAtTargetPath(this.Definition, targetPath) as CDefinedObject;
        }

        private bool ConstraintReferencesValid(CAttribute cAttribute)
        {
            Check.Require(cAttribute != null, string.Format(CommonStrings.XMustNotBeNull, "cAttribute"));

            if (cAttribute.Children != null)
            {
                foreach (CObject cObj in cAttribute.Children)
                {
                    ConstraintRef constraintRef = cObj as ConstraintRef;
                    if (constraintRef != null)
                    {
                        string reference = constraintRef.Reference;
                        if (this.Ontology.ConstraintCodes == null || !this.Ontology.ConstraintCodes.Has(reference))
                            return false;
                    }

                    else
                    {
                        CComplexObject cComplexObj = cObj as CComplexObject;
                        if (cComplexObj != null && cComplexObj.Attributes != null)
                        {
                            foreach (CAttribute attri in cComplexObj.Attributes)
                                if (!ConstraintReferencesValid(attri))
                                    return false;
                        }
                    }
                }
            }

            return true;
        }

        #endregion

        #region IXmlSerializable Members

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            AmXmlSerializer serializer = new AmXmlSerializer();
            serializer.ReadArchetype(reader, this);
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            AmXmlSerializer serializer = new AmXmlSerializer();
            serializer.WriteArchetype(writer, this);
        }


        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            AmXmlSerializer.LoadArchetypeSchema(xs);
            return new System.Xml.XmlQualifiedName("ARCHETYPE", RmXmlSerializer.OpenEhrNamespace);

        }
        #endregion
    }
}
