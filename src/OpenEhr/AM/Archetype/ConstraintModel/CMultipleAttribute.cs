using System;
using OpenEhr.DesignByContract;
using OpenEhr.Futures.OperationalTemplate;
using OpenEhr.RM.Support.Identification;
using OpenEhr.Resources;
using OpenEhr.Attributes;
using OpenEhr.RM.Impl;
using OpenEhr.RM.Common.Archetyped;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Validation;

namespace OpenEhr.AM.Archetype.ConstraintModel
{
    /// <summary>
    /// Concrete model of constraint on multiply-valued (ie. container) attribute node.
    /// </summary>
    [Serializable]
    [AmType("C_MULTIPLE_ATTRIBUTE")]
    public class CMultipleAttribute : CAttribute
    {
        #region Constructors
        public CMultipleAttribute() { }

        public CMultipleAttribute(string rmAttributeName, AssumedTypes.Interval<int> existence, Cardinality cardinality,
            AssumedTypes.List<CObject> children)
            : base(rmAttributeName, existence, children)
        {
            this.Cardinality = cardinality;
        }
        #endregion

        #region Class properties
        private Cardinality cardinality;
        /// <summary>
        /// Cardinality of this attribute constraint, if it constraints a container attribute.
        /// </summary>
        public Cardinality Cardinality
        {
            get { return this.cardinality; }
            set
            {
                DesignByContract.Check.Require(value != null,string.Format(CommonStrings.XMustNotBeNull, "Cardinality value"));
                this.cardinality = value;
            }
        }
        #endregion

        #region Functions
        /// <summary>
        /// List of constraints representing members of the container value of this attribute within the data. 
        /// Semantics of the uniqueness and ordering of items in the container are given by the cardinality.
        /// </summary>
        /// <returns></returns>
        public AssumedTypes.List<CObject> Members()
        {
            return this.Children;
        }

        public override bool IsSubsetOf(ArchetypeConstraint other)
        {
            throw new NotImplementedException(string.Format(
                AmValidationStrings.IsSubsetNotImplementedInX, "CMultipleAttribute"));
        }
        #endregion

        #region Validation

        public override bool IsValid()
        {
            return base.IsValid();
        }

        public override bool ValidValue(object dataValue)
        {
            Check.Require(dataValue != null, string.Format(CommonStrings.XMustNotBeNull, "dataValue"));

            AssumedTypes.IAggregate aggregate = dataValue as AssumedTypes.IAggregate;
            Check.Require(aggregate != null, string.Format(
                AmValidationStrings.XMustImplementY, dataValue.GetType().ToString(), "IAggregate"));

            bool result = true;

            aggregate.Constraint = this;
            AssumedTypes.IList iList = dataValue as AssumedTypes.IList;

            if (iList == null)
                throw new ApplicationException(string.Format(AmValidationStrings.XMustImplementY, dataValue.GetType().ToString(), "IList"));

            if (!Cardinality.Interval.Has(iList.Count))
            {
                result = false;
                ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.CardinalityOutOfBounds, iList.Count, RmAttributeName));
            }

            result &= Cardinality.IsOrdered ? IsOrderedChildrenValid(iList) : IsUnorderedChildrenValid(iList);
            result &= IsOccurrencesValid(iList);
            return result;
        }

        private bool IsOccurrencesValid(AssumedTypes.IList dataChildren)
            {
            Check.Require(dataChildren != null, string.Format(CommonStrings.XMustNotBeNull, "dataChildren"));

            bool result = true;
            System.Collections.Generic.List<MatchingItems> matches = new System.Collections.Generic.List<MatchingItems>();
            AcceptValidationError previousErrorDelegate = ValidationContext.AcceptError;

            try
            {
                ValidationContext.AcceptError = null;

                foreach (CObject constraint in Children)
                {
                    if (constraint.Occurrences.Lower > 0 || !constraint.Occurrences.UpperUnbounded)
                    {
                        matches.Add(new MatchingItems(dataChildren, constraint));
            }
                }
            }
            finally
            {
                ValidationContext.AcceptError = previousErrorDelegate;
            }

            foreach (MatchingItems match in matches)
            {
                match.RemoveItemsAlreadyMatchedByNameAndNodeId(matches);

                CObject constraint = match.Constraint;
                int lower = match.Lower;
                long upper = match.Upper;
                int actual = match.Count;

                if (actual < lower)
                {
                    result = false;
                    ValidationContext.AcceptValidationError(constraint, string.Format(AmValidationStrings.NotEnoughOccurrences, constraint.NodeId, lower, actual));
        }

                if (actual > upper)
        {
                    result = false;
                    ValidationContext.AcceptValidationError(constraint, string.Format(AmValidationStrings.TooManyOccurrences, constraint.NodeId, upper, actual));
                }
            }

            return result;
        }

        private class MatchingItems : System.Collections.Generic.List<object>
            {
            public readonly CObject Constraint;

            public readonly CAttribute NameAttribute;

            public readonly string NodeId;

            public readonly int Lower;

            public readonly long Upper;

            public MatchingItems(AssumedTypes.IList dataChildren, CObject constraint) : base()
                {
                Check.Require(dataChildren != null, string.Format(CommonStrings.XMustNotBeNull, "dataChildren"));
                Check.Require(constraint != null, string.Format(CommonStrings.XMustNotBeNull, "constraint"));

                Constraint = constraint;
                NameAttribute = NameAttributeConstraint(constraint);
                Lower = constraint.Occurrences.Lower;
                Upper = constraint.Occurrences.UpperUnbounded ? long.MaxValue : constraint.Occurrences.Upper;

                CArchetypeRoot archetypeRoot = constraint as CArchetypeRoot;
                NodeId = archetypeRoot != null ? archetypeRoot.ArchetypeId.Value : constraint.NodeId;

                if (!(constraint is ArchetypeSlot))
                    {
                    OpenEhr.AssumedTypes.Impl.ILocatableList locatableItems = dataChildren as OpenEhr.AssumedTypes.Impl.ILocatableList;

                    if (locatableItems != null)
                    {
                        Check.Assert(!string.IsNullOrEmpty(NodeId), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "NodeId"));

                        if (locatableItems.Contains(NodeId))
                        {
                            foreach (OpenEhr.RM.Common.Archetyped.Impl.Locatable locatable in (System.Collections.IEnumerable)locatableItems[NodeId])
                                if (NameAttribute == null || NameAttribute.ValidValue(locatable.Name))
                                    Add(locatable);
                    }
                }
                    else
                {
                        foreach (object item in dataChildren)
                    {
                            IRmType rmType = item as IRmType;

                            if (rmType != null && constraint.IsSameRmType(rmType))
                                Add(item);
                    }
                }
            }

                Check.Ensure(Constraint == constraint);
        }

            public virtual void RemoveItemsAlreadyMatchedByNameAndNodeId(System.Collections.Generic.IEnumerable<MatchingItems> otherMatches)
            {
                Check.Require(otherMatches != null, string.Format(CommonStrings.XMustNotBeNull, "others"));

                if (NameAttribute == null && !string.IsNullOrEmpty(NodeId))
                {
                    foreach (MatchingItems other in otherMatches)
                    {
                        if (other != this && other.NameAttribute != null && other.NodeId == NodeId)
                            foreach (object item in other)
                                Remove(item);
                    }
                }
            }
        }

        private bool IsUnorderedChildrenValid(AssumedTypes.IList dataChildren)
        {
            Check.Require(dataChildren != null, string.Format(CommonStrings.XMustNotBeNull, "children"));

            bool result = true;

            foreach (object dataItem in dataChildren)
            {
                System.Collections.Generic.List<CObject> matchedChildren 
                    = new System.Collections.Generic.List<CObject>();
                System.Collections.Generic.List<ArchetypeSlot> slots 
                    = new System.Collections.Generic.List<ArchetypeSlot>();

                IRmType rmType = dataItem as IRmType;
                Check.Assert(rmType != null, string.Format(AmValidationStrings.XMustImplementY, dataItem.GetType().ToString(), "IRmType"));

                ILocatable locatable = dataItem as ILocatable;

                // get all child constraint objects with this data item's node_id
                foreach (CObject eachChild in Children)
                {
                    if (locatable == null || locatable.ArchetypeNodeId == eachChild.ArchetypeNodeId)
                        if (eachChild.IsSameRmType(rmType))
                        matchedChildren.Add(eachChild);
                    }

                bool matchedWithSlot = false;

                if (matchedChildren.Count == 0)
                {
                    bool validationResult = true;
                    matchedWithSlot = MatchedWithSlot(locatable, out validationResult);

                    if (matchedWithSlot)
                    {
                        result &= validationResult;
                    }
                    else
                    {
                        // child constraint object not found for this data item
                        result = false;
                        string errorRmTypeName = rmType.GetRmTypeName();
                        ILocatable locatableDataItem = dataItem as ILocatable;

                        if (locatableDataItem != null)
                            ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.ItemXWithIdYNotAllowedByAttributeZ, errorRmTypeName, locatableDataItem.ArchetypeNodeId, RmAttributeName));
                        else 
                            ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.ItemXNotAllowedByAttributeY, errorRmTypeName, RmAttributeName));
                    }
                }

                CObject unnamedMatchedObject = null;
                bool validResult = false;

                // attempt to match data item against child constraint objects with a name attribute constraint
                foreach (CObject matchedObject in matchedChildren)
                {
                    CComplexObject complexObject = matchedObject as CComplexObject;

                    if (complexObject == null)
                        throw new ApplicationException(AmValidationStrings.MultiAttributeChildNotComplexObj);

                    CAttribute nameAttribute = NameAttributeConstraint(complexObject);

                    if (nameAttribute != null)
                    {
                        bool nameAttributeFound = false;
                        AcceptValidationError previousErrorDelegate = ValidationContext.AcceptError;

                        try
                        {
                            ValidationContext.AcceptError = null;
                            nameAttributeFound = nameAttribute.ValidValue(locatable.Name);
                        }
                        finally
                        {
                            ValidationContext.AcceptError = previousErrorDelegate;
                        }

                        if (nameAttributeFound)
                        {
                            validResult = matchedObject.ValidValue(dataItem);

                            if (validResult)
                                break;
                            else
                                result = false;
                        }
                    }
                    else
                    {
                        // keep child constraint object with no name attribute constraint for later
                        if (unnamedMatchedObject != null)
                            throw new ApplicationException(AmValidationStrings.ExpectingOnlyOneUnnamedChild);

                        unnamedMatchedObject = matchedObject;
                    }
                }

                // no matching named object constraint, so attempt to validate against unnamed object constraint 
                if (!validResult && !matchedWithSlot)
                {
                    if (unnamedMatchedObject == null)
                    {
                        result = false;
                        ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.NotAllowedByAttributeXConstraint, RmAttributeName));
                    }
                    else if (!unnamedMatchedObject.ValidValue(dataItem))
                        result = false;
                }
            }

            return result;
        }

        private bool MatchedWithSlot(ILocatable dataItem, out bool isValid)
        {
            isValid = true;

            foreach (CObject child in Children)
            {
                ArchetypeSlot slot = child as ArchetypeSlot;

                if (slot != null && slot.CanFillWith(dataItem.ArchetypeNodeId) && !slot.IsFull)
                {
                    CArchetypeRoot cArchetypeRoot = ValidationContext.FetchOperationalObject(this, new ArchetypeId(dataItem.ArchetypeNodeId));

                    if (cArchetypeRoot == null)
                    {
                        isValid = false;
                        ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.ArchetypeXLoadFailed, dataItem.ArchetypeNodeId));
                    }
                    else
                    {
                        slot.AddSlotFiller(cArchetypeRoot);
                        cArchetypeRoot.SetValidationContext(ValidationContext);
                        isValid = cArchetypeRoot.ValidValue(dataItem);
                    }

                    return true;
                }
            }

            return false;
        }

        private bool IsOrderedChildrenValid(AssumedTypes.IList dataChildren)
        {
            Check.Require(dataChildren != null, string.Format(CommonStrings.XMustNotBeNull, "children"));

            int n = 0;
            bool result = true;

            foreach (object dataItem in dataChildren)
            {
                int startingPoint = n;
                CObject matchedCObject = null;
                string dataItemRmType = ((IRmType)dataItem).GetRmTypeName();
                ILocatable locatable = dataItem as ILocatable;

                while (n < Children.Count)
                {
                    CObject eachChild = Children[n];

                    if (locatable == null || locatable.ArchetypeNodeId == eachChild.ArchetypeNodeId)
                        if (eachChild.IsSameRmType(dataItem as IRmType))
                        {
                            matchedCObject = eachChild;
                            CComplexObject complexObject = eachChild as CComplexObject;

                            if (complexObject != null)
                            {
                                CAttribute nameAttribute = NameAttributeConstraint(complexObject);

                                if (nameAttribute != null)
                                {
                                    bool nameMatched = false;
                                    AcceptValidationError previousErrorDelegate = ValidationContext.AcceptError;

                                    try
                                    {
                                        ValidationContext.AcceptError = null;
                                        nameMatched = nameAttribute.ValidValue(locatable.Name);
                                    }
                                    finally
                                    {
                                        ValidationContext.AcceptError = previousErrorDelegate;
                                    }

                                    if (nameMatched)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        n++;
                                        continue;
                                    }
                                }
                            }

                            break;
                        }

                    n++;
                }

                if (matchedCObject == null)
                {
                    n = startingPoint;
                    bool validationResult = true;

                    if (MatchedWithSlot(locatable, out validationResult))
                    {
                        result = validationResult;
                    }
                    else
                    {
                        result = false;
                        ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.YNotAllowedByAttributeXConstraint, RmAttributeName, dataItemRmType));
                    }
                }
                else if (!matchedCObject.ValidValue(dataItem))
                    result = false;
            }

            return result;
        }

        static private CAttribute NameAttributeConstraint(CObject objectConstraint)
        {
            Check.Require(objectConstraint != null, string.Format(CommonStrings.XMustNotBeNull, "objectConstraint"));

            CComplexObject cComplexObject = objectConstraint as CComplexObject;

            if (cComplexObject != null && cComplexObject.Attributes != null)
            {
                foreach (CAttribute attribute in cComplexObject.Attributes)
                {
                    if (attribute.RmAttributeName == "name")
                        return attribute;
                }
            }

            return null;
        }

        public static bool HasNameAttributeConstraint(CObject objectConstraint, DvText name)
        {
            return HasNameAttributeConstraint(objectConstraint, name, false);
        }

        public static bool HasNameAttributeConstraint(CObject objectConstraint, DvText name, bool requireNameConstraint)
        {
            CAttribute nameConstraint = NameAttributeConstraint(objectConstraint);

            if (nameConstraint == null)
            {
                if (requireNameConstraint)
                    return false;

                return true;
            }

            bool nameFound = nameConstraint.ValidValue(name);

            return nameFound;

        }
        #endregion

        internal AssumedTypes.IAggregate CreateAggregate(Type itemType)
        {
            Check.Require(itemType != null, string.Format(CommonStrings.XMustNotBeNull, "itemType"));

            Type listType = typeof(AssumedTypes.List<>);

            listType = listType.MakeGenericType(new Type[] { itemType });
            return Activator.CreateInstance(listType) as AssumedTypes.IAggregate;
        }

        // gets CObject from children that has attribute constraints that matche the predicate
        internal CObject GetChildObjectConstraint(string predicate)
        {
            throw new NotImplementedException();
        }
    }
}
