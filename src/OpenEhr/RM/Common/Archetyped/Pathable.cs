using System;
using OpenEhr.DesignByContract;
using OpenEhr.AM.Archetype.ConstraintModel;
using OpenEhr.AssumedTypes;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Common.Archetyped
{
    /// <summary>
    /// Abstract parent of all classes whose instances are reachable by paths, 
    /// and which know how to locate child object by paths. 
    /// The parent feature may be implemented as a function or attribute.
    /// </summary>
    [Serializable]
    public abstract class Pathable : RmType
    {
        /// <summary>
        /// Parent of this node in compositional hierarchy.
        /// </summary>
        private Pathable parent;

        public virtual Pathable Parent
        {
            get { return this.parent; }
            internal set { this.parent = value; }
        }

        public abstract bool PathExists(string path);
        public abstract bool PathUnique(string path);
        public abstract object ItemAtPath(string path);
        public abstract List<object> ItemsAtPath(string path);
        public abstract string PathOfItem(Pathable item);

        protected new CComplexObject Constraint
        {
            get
            {
                CComplexObject constraint = base.Constraint as CComplexObject;

                Check.Ensure(!HasConstraint || constraint != null, "Constraint must be of type CComplexObject");
                return constraint;
            }
            set { base.Constraint = value; }
        }

        protected override void SetAttributeValue(string attributeName, object value)
        {
            // Set Constraint
            if (value != null &&  HasConstraint)
            {
                CAttribute attributeConstraint = Constraint.GetAttribute(attributeName);
                if (attributeConstraint != null)
                {
                    CMultipleAttribute multipleAttributeConstraint = attributeConstraint as CMultipleAttribute;
                    if (multipleAttributeConstraint != null)
                    {
                        IAggregate aggregate = value as IAggregate;
                        Check.Assert(aggregate != null, "value must implement IAggregate when attribute constraint is CMultipleAttribute");
                        aggregate.Constraint = multipleAttributeConstraint;
                    }
                    else
                    {
                        bool isValid = false;
                        foreach(CObject objectConstraint in attributeConstraint.Children)
                        {
                            if (objectConstraint.ValidValue(value))
                            {
                                isValid = true;
                                IRmType rmType = value as IRmType;
                                if (rmType != null)
                                    rmType.Constraint = objectConstraint;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}