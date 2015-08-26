using System;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.Archetype.ConstraintModel
{
    /// <summary>
    /// Reference to a constraint described in the same archetype, but outside the main
    /// constraint structure. This is used to refer to constraints expressed in terms of
    /// external resources, such as constraints on terminology value sets
    /// </summary>
    [Serializable]
    [AmType("CONSTRAINT_REF")]
    public class ConstraintRef: CReferenceObject
    {
        #region Constructors
        public ConstraintRef(string rmTypeName, string nodeId, AssumedTypes.Interval<int> occurrences,
          CAttribute parent, string reference)
            : base(rmTypeName, nodeId, occurrences, parent)
        {
            this.Reference = Reference;
        }

        public ConstraintRef() { }
         #endregion

        #region Class properties
        private string reference;

        /// <summary>
        /// Reference to a constraint in the archetype local ontology.
        /// </summary>
        public string Reference
        {
            get { return reference; }
            set
            {
                DesignByContract.Check.Require(!string.IsNullOrEmpty(value),
                    string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Reference value"));
                reference = value;
            }
        }
         #endregion

        #region Functions
        public override bool IsSubsetOf(ArchetypeConstraint other)
        {
            throw new Exception(string.Format(AmValidationStrings.IsSubsetNotImplementedInX, "ConstraintRef"));
        }
        public override bool ValidValue(object aValue)
        {
            // TODO: support constraint bindings.
            // Look for constraint bindings for this constraint ref and ensure terminology ID is allowed based on the constraint bindings
            // otherwise return true
            return true;
        } 

        protected override System.Collections.Generic.List<string> GetPhysicalPaths()
        {
            return null;
        }

        protected override string GetCurrentNodePath()
        {
            DesignByContract.Check.Require(string.IsNullOrEmpty(this.NodeId),
                string.Format(CommonStrings.XMustNotBeNullOrEmpty, "NodeId"));

            return null;
        }

        #endregion
    }
}
