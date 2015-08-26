using System;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Support.Identification;
using OpenEhr.AM;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;
using OpenEhr.Attributes;
using OpenEhr.Validation;

namespace OpenEhr.Futures.OperationalTemplate
{
    [Serializable]
    [AmType("C_ARCHETYPE_ROOT")]
    public class CArchetypeRoot : AM.Archetype.ConstraintModel.CComplexObject
    {
        ArchetypeId archetypeId;
        public ArchetypeId ArchetypeId
        {
            get { return archetypeId; }
            set { archetypeId = value; }
        }

        internal override string ArchetypeNodeId
        {
            get { return this.archetypeId.Value; }
        }

        TemplateId templateId;
        public TemplateId TemplateId
        {
            get { return templateId; }
            set { templateId = value; }
        }

        AssumedTypes.Hash<AM.Archetype.Ontology.ArchetypeTerm, string> termDefinitions;
        public AssumedTypes.Hash<AM.Archetype.Ontology.ArchetypeTerm, string> TermDefinitions
        {
            get { return termDefinitions; }
            set { termDefinitions = value; }
        }

        AssumedTypes.Hash<AssumedTypes.Hash<CodePhrase, string>, string> termBindings;
        public AssumedTypes.Hash<AssumedTypes.Hash<CodePhrase, string>, string> TermBindings
        {
            get { return termBindings; }
            set { termBindings = value; }
        }

        public override object DefaultValue
        {
            get
            {
                Locatable defaultObject = base.DefaultValue as Locatable;
                if (defaultObject == null)
                    throw new ApplicationException(AmValidationStrings.DefaultArchetypeMustBeTypeLocatable);
                defaultObject.ArchetypeNodeId = this.ArchetypeNodeId;
               
                return defaultObject;
            }
        }

        #region Validation

        public override bool ValidValue(object aValue)
        {
            bool result = true;
            Locatable locatable = aValue as Locatable;

            if (locatable == null)
            {
                result = false;
                ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.ExpectingValueXToBeTypeY, aValue, "Locatable"));
            }

            //TODO: validate template ID - probably need to do this in OperationalTemplate class

            if (!locatable.IsArchetypeRoot)
            {
                result = false;
                ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.ExpectingValueXToBeTypeY, aValue, "CArchetypeRoot"));
            }

            if (locatable.ArchetypeNodeId != archetypeId.Value)
            {
                result = false;
                ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.ExpectingNodeIdXButGotY, archetypeId.Value, locatable.ArchetypeNodeId));
            }

            if (!base.ValidValue(aValue))
                result = false;

            return result;
        }

        [NonSerialized]
        private ValidationContext validationContext;

        public void SetValidationContext(ValidationContext context)
        {
            validationContext = context;
        }

        internal override ValidationContext ValidationContext
        {
            get
            {
                ValidationContext result = validationContext;

                if (result == null)
                {
                    result = base.ValidationContext;

                    if (ConstraintParent == null)
                        validationContext = result;
                }

                Check.Ensure(result != null, string.Format(CommonStrings.XMustNotBeNull, "ValidationContext"));
                Check.Ensure(ConstraintParent != null || validationContext == result, "ConstraintParent == null implies the validationContext field variable must be set, for efficiency");
                return result;
            }
        }

        #endregion //Validation
    }
}