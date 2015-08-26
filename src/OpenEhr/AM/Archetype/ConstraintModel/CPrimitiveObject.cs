using System;
using OpenEhr.DesignByContract;
using OpenEhr.AM.Archetype.ConstraintModel.Primitive;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.Archetype.ConstraintModel
{
    /// <summary>
    /// Constraint on a primitive type.
    /// </summary>
    [Serializable]
    [AmType("C_PRIMITIVE_OBJECT")]
    public class CPrimitiveObject: CDefinedObject
    {
        public CPrimitiveObject(string rmTypeName, string nodeId, AssumedTypes.Interval<int> occurrences,
            CAttribute parent, object assumedValue)
            : base(rmTypeName, nodeId, occurrences, parent, assumedValue)
        {
        }

        public CPrimitiveObject() { }

        #region Class properties
        private Primitive.CPrimitive item;

        /// <summary>
        /// Object actually defining the constraint
        /// </summary>
        public Primitive.CPrimitive Item
        {
            get { return item; }
            set { item = value; }
        }

        public override object DefaultValue
        {
            get { return this.Item.DefaultValue; }
           
        }

        #endregion

        #region Functions
        public override bool AnyAllowed()
        {
            return this.item == null;
        }

        public override bool IsSubsetOf(ArchetypeConstraint other)
        {
            Check.Require(other != null, string.Format(CommonStrings.XMustNotBeNull, "other"));      

            CPrimitiveObject otherPrimitive = other as CPrimitiveObject;

            if (otherPrimitive == null)
                return false;

            if (this.Item.GetType() != otherPrimitive.Item.GetType())
                return false;

            return this.Item.IsSubsetOf(otherPrimitive.Item);            
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

        public override bool ValidValue(object dataValue)
        {
            Check.Require(dataValue != null, string.Format(CommonStrings.XMustNotBeNull, "dataValue"));

            bool result = true;

            if (!AnyAllowed())
            {
                string errorMessage = item.ValidValue(dataValue);
                result = string.IsNullOrEmpty(errorMessage);

                if (!result)
                    ValidationContext.AcceptValidationError(this, errorMessage);
            }

            return result;
        }

        #endregion
    }
}
