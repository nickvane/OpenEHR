using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.DataTypes.Uri;
using OpenEhr.Resources;
using OpenEhr.Validation;

namespace OpenEhr.AM.Archetype.Ontology
{
    /// <summary>
    /// Local ontology of an archetype.
    /// </summary>
    [Serializable]
    public class ArchetypeOntology
    {
        #region Class properties
        private AssumedTypes.Set<string> terminologyesAvailable;
        /// <summary>
        /// List of terminologies to which term or constraint bindings exist in this terminology.
        /// </summary>
        public AssumedTypes.Set<string> TerminologyesAvailable
        {
            get
            {
                if (terminologyesAvailable == null)
                {
                    System.Collections.Generic.List<string> terminologyList = new List<string>();
                    foreach (string terminologyId in this.TermBindings.Keys)
                    {
                        if (!terminologyList.Contains(terminologyId))
                            terminologyList.Add(terminologyId);
                    }

                    if (terminologyList.Count > 0)
                        this.terminologyesAvailable = new OpenEhr.AssumedTypes.Set<string>(terminologyList);
                }

                return this.terminologyesAvailable;
            }

        }

        private bool specialisationDepthSet;
        private int specialisationDepth;       
        /// <summary>
        /// Specialisation depth of this archetype. Unspecialised archetypes have depth 0, 
        /// with each additional level of specialisation adding 1 to the specialisation_depth.
        /// </summary>
        public int SpecialisationDepth
        {
            get
            {
                if (!specialisationDepthSet)
                {
                    string conceptCode = GetConceptCode();

                    this.SpecialisationDepth = ArcheytpeTermCodeTools.SpecialisationDepthFromCode(conceptCode);
                }
                return this.specialisationDepth;
            }
            set
            {
                //    throw new Exceptions.ValidationException("specialisationDepth value to be set must not less than zero.");
                Check.Require(value >= 0, CommonStrings.SpecialisationDepthLessThanZero);

                this.specialisationDepth = value;
                this.specialisationDepthSet = true;
            }
        }

        private string GetConceptCode()
        {
            string conceptCode = "at0000";

            foreach (string code in this.TermCodes)
                if (code.StartsWith("at0000."))
                    return code;

            return conceptCode;
        }

        private AssumedTypes.List<string> termCodes;
        /// <summary>
        /// List of all term codes in the ontology. Most of these correspond to “at” codes in an ADL 
        /// archetype, which are the node_ids on C_OBJECT descendants. There may be an 
        /// extra one, if a different term is used as the overall archetype concept from that used as
        /// the node_id of the outermost C_OBJECT in the definition part.
        /// </summary>
        public AssumedTypes.List<string> TermCodes
        {
            get
            {
                if (this.termCodes == null)
                    this.termCodes = this.TermDefinitions.Item(this.TermDefinitions.Keys[0]).Keys;

                Check.Ensure(this.termCodes!=null && !this.termCodes.IsEmpty());
                return this.termCodes;
            }

        }

        private AssumedTypes.List<string> constraintCodes;
        /// <summary>
        /// List of all term codes in the ontology. These correspond to the “ac” codes in an ADL 
        /// archetype, or equivalently, the CONSTRAINT_REF.reference values in the archetype definition.
        /// </summary>
        public AssumedTypes.List<string> ConstraintCodes
        {
            get
            {
                if (this.constraintCodes == null && this.constraintDefinitions != null)
                    this.constraintCodes = this.constraintDefinitions.Item(this.ConstraintDefinitions.Keys[0]).Keys;

                return this.constraintCodes;
            }
            set
            {
                Check.Require(value != null, string.Format(
                    CommonStrings.XMustNotBeNull, "ConstraintCodes value"));

                this.constraintCodes = value;
            }
        }

        private AssumedTypes.List<string> termAttributeNames;
        /// <summary>
        ///List of ‘attribute’ names in ontology terms, typically includes ‘text’, 
        ///‘description’, ‘provenance’ etc.
        /// </summary>
        public AssumedTypes.List<string> TermAttributeNames
        {
            get
            {
                if (this.termAttributeNames == null)
                {
                    this.termAttributeNames = new OpenEhr.AssumedTypes.List<string>();
                    Check.Ensure(this.TermDefinitions != null, 
                        string.Format(CommonStrings.XMustNotBeNull, "TermDefinitions"));
                    foreach (string language in this.TermDefinitions.Keys)
                    {
                        AssumedTypes.Hash<ArchetypeTerm, string> archetypeTerms = this.TermDefinitions.Item(language);
                        foreach (string termCode in archetypeTerms.Keys)
                        {
                            ArchetypeTerm archetypeTerm = archetypeTerms.Item(termCode);
                            foreach (string attributeName in archetypeTerm.Keys())
                            {
                                if (!termAttributeNames.Has(attributeName))
                                    termAttributeNames.Add(attributeName);
                            }
                        }
                    }                  
                }
                return this.termAttributeNames;
            }

        }

        private Archetype parent;
        /// <summary>
        /// Archetype which owns this ontology.
        /// </summary>
        public Archetype ParentArchetype
        {
            get { return this.parent; }
            set
            {
                Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "ParentArchetype value"));

                this.parent = value;
            }
        }
        #endregion

        #region Internal class properties
        private AssumedTypes.Hash<AssumedTypes.Hash<ArchetypeTerm, string>, string> termDefinitions;
        /// <summary>
        /// Directory of term definitions as a two-level table. The outer hash keys are term codes,
        /// e.g. “at0004”, and the inner hash key are term attribute names, e.g. “text”, “description” etc.
        /// </summary>
        internal AssumedTypes.Hash<AssumedTypes.Hash<ArchetypeTerm, string>, string> TermDefinitions
        {
            get { return this.termDefinitions; }
            set
            {
                if (value == null)
                    throw new ValidationException(
                        string.Format(CommonStrings.XIsNull, "TermDefinitions value"));

                this.termDefinitions = value;
            }
        }

        private AssumedTypes.Hash<AssumedTypes.Hash<ArchetypeTerm, string>, string> constraintDefinitions;
        /// <summary>
        /// Directory of constraint definitions as a twolevel table. The outer hash keys are constraint
        /// codes, e.g. “ac0004”, and the inner hash keys are constraint attribute names, e.g. “text”, “description” etc.
        /// </summary>
        internal AssumedTypes.Hash<AssumedTypes.Hash<ArchetypeTerm, string>, string> ConstraintDefinitions
        {
            get { return this.constraintDefinitions; }
            set { this.constraintDefinitions = value; }
        }

        private AssumedTypes.Hash<AssumedTypes.Hash<CodePhrase, string>, string> termBindings;
        /// <summary>
        /// Directory of term bindings as a two-level table. The outer hash keys are terminology ids, 
        /// e.g. “SNOMED-CT”, and the inner hash keys are term codes, e.g. “at0004” etc. 
        /// The indexed CODE_PHRASE objects represent the bound external codes, e.g. Snomed or ICD
        /// codes in string form, e.g. “SNOMEDCT::10094842”.
        /// </summary>
        internal AssumedTypes.Hash<AssumedTypes.Hash<CodePhrase, string>, string> TermBindings
        {
            get { return this.termBindings; }
            set { this.termBindings = value; }
        }

        private AssumedTypes.Hash<AssumedTypes.Hash<DvUri, string>, string> constraintBindings;
        /// <summary>
        /// Directory of constraint bindings as a twolevel table. The outer hash keys are terminology
        /// ids, e.g. “SNOMED-CT”, and the inner hash keys are constraint codes, e.g. “ac0004” etc. 
        /// The indexed URI objects represent references to externally defined resources, usually a terminology subset.
        /// </summary>
        internal AssumedTypes.Hash<AssumedTypes.Hash<DvUri, string>, string> ConstraintBindings
        {
            get { return this.constraintBindings; }
            set { this.constraintBindings = value; }
        }
        #endregion

        #region Functions
        /// <summary>
        /// True if term_codes has a_code.
        /// </summary>
        /// <param name="aCode"></param>
        /// <returns></returns>
        public bool HasTermCode(string aCode)
        {
            return this.TermCodes.Has(aCode);
        }

        /// <summary>
        /// True if constraint_codes has a_code.
        /// </summary>
        /// <param name="aCode"></param>
        /// <returns></returns>
        public bool HasConstraintCode(string aCode)
        {
            Check.Require(!string.IsNullOrEmpty(aCode), string.Format(
                CommonStrings.XMustNotBeNullOrEmpty, "aCode"));

            return this.ConstraintCodes.Has(aCode);
        }

        /// <summary>
        /// Term definition for a code, in a specified language.
        /// </summary>
        /// <param name="aLang"></param>
        /// <param name="aCode"></param>
        /// <returns></returns>
        public ArchetypeTerm TermDefinition(string aLang, string aCode)
        {
            Check.Require(this.HasLanguage(aLang), string.Format(CommonStrings.OntologyMissingLanguageX, aLang));
            Check.Require(this.HasTermCode(aCode), string.Format(CommonStrings.OntologyMissingCode, aCode));

            return this.TermDefinitions.Item(aLang).Item(aCode);

        }

        /// <summary>
        /// Constraint definition for a code, in a specified language.
        /// </summary>
        /// <param name="aLang"></param>
        /// <param name="aCode"></param>
        /// <returns></returns>
        public ArchetypeTerm ConstraintDefinition(string aLang, string aCode)
        {
            Check.Require(this.HasLanguage(aLang), string.Format(CommonStrings.OntologyMissingLanguageX, aLang));
            Check.Require(this.HasTermCode(aCode), string.Format(CommonStrings.OntologyMissingCode, aCode));

            AssumedTypes.Hash<ArchetypeTerm, string> languageSpecificConstraintDefinitions = this.TermDefinitions.Item(aLang);

            if (languageSpecificConstraintDefinitions == null)
                throw new ApplicationException(string.Format(
                    CommonStrings.XMustNotBeNull, "languageSpecificConstraintDefinitions"));

            return languageSpecificConstraintDefinitions.Item(aCode);
        }

        /// <summary>
        /// Binding of term corresponding to a_code in target external terminology a_terminology_id as a CODE_PHRASE.
        /// </summary>
        /// <param name="aTerminologyId"></param>
        /// <param name="aCode"></param>
        /// <returns></returns>
        public CodePhrase TermBinding(string aTerminologyId, string aCode)
        {
            Check.Require(this.HasTerminology(aTerminologyId), string.Format(CommonStrings.OntologyMissingTerminologyX, aTerminologyId));
            Check.Require(this.HasTermCode(aCode), string.Format(CommonStrings.OntologyMissingCode, aCode));

            AssumedTypes.Hash<CodePhrase, string> terminologySpecificTermBinding = this.TermBindings.Item(aTerminologyId);

            if (terminologySpecificTermBinding == null)
                throw new ApplicationException(string.Format(
                    CommonStrings.XMustNotBeNull, "terminologySpecificTermBinding"));

            return terminologySpecificTermBinding.Item(aCode);
        }

        /// <summary>
        /// Binding of constraint corresponding to a_code in target external terminology a_terminology_id, 
        /// as a string, which is usually a formal query expression.
        /// </summary>
        /// <param name="aTerminologyId"></param>
        /// <param name="aCode"></param>
        /// <returns></returns>
        public string ConstraintBinding(string aTerminologyId, string aCode)
        {
            Check.Require(this.HasTerminology(aTerminologyId), string.Format(CommonStrings.OntologyMissingTerminologyX, aTerminologyId));
            Check.Require(this.HasTermCode(aCode), string.Format(CommonStrings.OntologyMissingCode, aCode));

            AssumedTypes.Hash<DvUri, string> terminologySpecificConstraintBinding = this.ConstraintBindings.Item(aTerminologyId);

            if (terminologySpecificConstraintBinding == null)
                throw new ApplicationException(string.Format(
                    CommonStrings.XMustNotBeNull, "terminologySpecificConstraintBinding"));

            return terminologySpecificConstraintBinding.Item(aCode).Value;
        }

        /// <summary>
        /// True if language ‘a_lang’ is present in archetype ontology.
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public bool HasLanguage(string language)
        {
            Check.Require(this.ParentArchetype != null,
                string.Format(CommonStrings.XMustNotBeNull, "ArchetypeOntology.ParentArchetype"));

            return this.ParentArchetype.LanguagesAvailable().Has(language);
        }

        /// <summary>
        /// True if terminology ‘a_terminology’ is present in archetype ontology.
        /// </summary>
        /// <param name="terminology"></param>
        /// <returns></returns>
        public bool HasTerminology(string aTerminologyId)
        {
            Check.Require(!string.IsNullOrEmpty(aTerminologyId), string.Format(
                CommonStrings.XMustNotBeNullOrEmpty, "aTerminologyId"));

            return this.TerminologyesAvailable.Has(aTerminologyId);
        }
        #endregion

    }
}