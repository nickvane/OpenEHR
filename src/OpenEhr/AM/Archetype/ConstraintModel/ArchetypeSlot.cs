using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.Futures.OperationalTemplate;
using OpenEhr.DesignByContract;
using System.Text.RegularExpressions;
using OpenEhr.AssumedTypes;
using OpenEhr.AM.Archetype.Assertion;
using OpenEhr.Resources;
using OpenEhr.Attributes;
using OpenEhr.Validation;


namespace OpenEhr.AM.Archetype.ConstraintModel
{
    /// <summary>
    /// Constraint describing a ‘slot’ where another archetype can occur.
    /// </summary>
    [Serializable]
    [AmType("ARCHETYPE_SLOT")]
    public class ArchetypeSlot: CReferenceObject//, System.Xml.Serialization.IXmlSerializable
    {

        #region Constructors
        public ArchetypeSlot(string rmTypeName, string nodeId, AssumedTypes.Interval<int> occurrences,
          CAttribute parent, AssumedTypes.Set<Assertion.Assertion> includes, 
            AssumedTypes.Set<Assertion.Assertion> excludes): base(rmTypeName, nodeId, occurrences, parent)
        {
            this.Includes = includes;
            this.Excludes = excludes;
        }

        public ArchetypeSlot() { }
        #endregion

        #region Class properties

        private AssumedTypes.Set<Assertion.Assertion> includes;

        /// <summary>
        /// List of constraints defining other archetypes that could be included at this point.
        /// </summary>
        public AssumedTypes.Set<Assertion.Assertion> Includes
        {
            get { return includes; }
            set
            {
                Check.Require(value == null || !value.IsEmpty(), string.Format(
                    CommonStrings.IfXIsNotNullMustBeEmpty, "Includes value"));
                includes = value;
            }
        }

        private AssumedTypes.Set<Assertion.Assertion> excludes;

        /// <summary>
        /// List of constraints defining other archetypes that could be included at this point.
        /// </summary>
        public AssumedTypes.Set<Assertion.Assertion> Excludes
        {
            get { return excludes; }
            set
            {
                Check.Require(value == null || !value.IsEmpty(), string.Format(
                    CommonStrings.IfXIsNotNullMustBeEmpty, "Excludes value"));
                excludes = value;
            }
        }
        #endregion

        #region Functions
        public override bool IsSubsetOf(ArchetypeConstraint other)
        {
            throw new NotImplementedException(
                string.Format(AmValidationStrings.IsSubsetNotImplementedInX, "ArchetypeSlot"));
        }

        private bool AnyAllowed()
        {
            return this.Includes == null && this.Excludes != null;
        }

        protected override System.Collections.Generic.List<string> GetPhysicalPaths()
        {
            return null;
        }
      
        protected override string GetCurrentNodePath()
        {
            return null;
        }
        #endregion


        #region Validation

        public override bool ValidValue(object aValue)
        {
            throw new NotImplementedException(AmValidationStrings.ArchetypeSlotValidationNotImplemented);
        }

        [NonSerialized]
        private int numberOfFillers = 0;

        internal void AddSlotFiller(CArchetypeRoot filler)
        {
            Check.Require(filler != null, string.Format(CommonStrings.XMustNotBeNull, "filler"));
            Check.Require(!IsFull, AmValidationStrings.SlotMustNotBeFull);

            numberOfFillers++;
        }

        internal bool CanFillWith(string archetypeId)
        {
            Check.Require(!string.IsNullOrEmpty(archetypeId), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "archetypeId"));

            return Assert(includes, archetypeId) || !Assert(excludes, archetypeId);
        }

        internal bool IsFull
        {
            get { return !Occurrences.Has(numberOfFillers + 1); }
        }

        private bool Assert(Set<OpenEhr.AM.Archetype.Assertion.Assertion> assertions, string id)
        {
            if (assertions != null)
            {
                foreach (OpenEhr.AM.Archetype.Assertion.Assertion assertion in assertions)
                {
                    string pattern = ValidationUtility.AssertionRegExPattern(assertion);
                    if (pattern == null) continue;

                    if (Regex.Match(id, pattern, RegexOptions.Compiled).Success)
                        return true;
                }
            }
            return false;
        }

        #endregion
    }
}