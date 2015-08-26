using System;
using OpenEhr.Attributes;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.DataTypes.Quantity;
using System.ComponentModel;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Support.Terminology.Impl;
using OpenEhr.RM.Support.Terminology;
using OpenEhr.Futures.OperationalTemplate;
using OpenEhr.AM.OpenehrProfile.DataTypes.Text;
using System.Reflection;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.RM.DataStructures;
using System.Collections;
using OpenEhr.Resources;
using OpenEhr.Factories;
using OpenEhr.RM.Impl;
using OpenEhr.RM.Common.Archetyped;
using OpenEhr.Validation;

namespace OpenEhr.AM.Archetype.ConstraintModel
{
    /// <summary>
    /// Constraint on complex objects, i.e. any object that consists of other object constraints.
    /// </summary>
    [Serializable]
    [AmType("C_COMPLEX_OBJECT")]
    public class CComplexObject: CDefinedObject
    {
        #region Constructors
        public CComplexObject(string rmTypeName, string nodeId, AssumedTypes.Interval<int> occurrences, 
            CAttribute parent, object assumedValue, AssumedTypes.Set<CAttribute> attributes)
            : base(rmTypeName, nodeId, occurrences, parent, assumedValue)
        {
            this.Attributes = attributes;
        }

        public CComplexObject() { }
        #endregion

        #region Properties
        private AssumedTypes.Set<CAttribute> attributes;

        /// <summary>
        /// List of constraints on attributes of the reference model type represented by this object.
        /// </summary>
        public AssumedTypes.Set<CAttribute> Attributes
        {
            get { return this.attributes; }
            set { this.attributes = value; }
        }
        
        /// <summary>
        /// Generate a default value from this constraint object
        /// </summary>
        /// 

        public override object DefaultValue
        {
            get 
            {
                if (string.IsNullOrEmpty(this.RmTypeName))
                    return null;

                Type rmType = RmFactory.GetOpenEhrV1Type(this.RmTypeName);
                if (rmType == null)
                    throw new ArgumentException(string.Format(
                        AmValidationStrings.CannotResolveRmTypeName, this.RmTypeName));

                ConstructorInfo constructor =
                    rmType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);

                object defaultValue = constructor.Invoke(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, 
                    null, new object[] { } , null);

                IRmType defaultRmType = defaultValue as IRmType;
                if (defaultRmType != null)
                    defaultRmType.Constraint = this;

                if (this.attributes != null)
                {
                    foreach (CAttribute @attribute in this.attributes)
                    {
                        if (@attribute.RmAttributeName != "name" 
                            && @attribute.Children != null && @attribute.Children.Count == 1)
                        {
                            CDefinedObject definedObject = @attribute.Children[0] as CDefinedObject;
                            if (definedObject != null)
                            {
                                object attributeDefault = definedObject.DefaultValue;

                                defaultRmType.SetAttributeValue(@attribute.RmAttributeName, attributeDefault);
                            }
                        }
                    }
                }

                OpenEhr.RM.Common.Archetyped.Impl.Locatable locatable 
                    = defaultValue as OpenEhr.RM.Common.Archetyped.Impl.Locatable;
                if (locatable != null)
                {
                    locatable.ArchetypeNodeId = this.NodeId;
                    // if attributes has "name", set name to the name constraint
                    CAttribute nameAttribute = GetAttribute("name");
                    if (nameAttribute != null)
                    {
                        if (nameAttribute.Children.Count == 1)
                        {
                            CDefinedObject nameConstraint = nameAttribute.Children[0] as CDefinedObject;
                            if (nameConstraint == null)
                                throw new ApplicationException(AmValidationStrings.CDefinedObjectNameChildExpected);
                            locatable.Name = nameConstraint.DefaultValue as DvText;
                        }
                        else if (nameAttribute.Children.Count > 1)
                            throw new NotImplementedException();
                        else
                            throw new ApplicationException(AmValidationStrings.NameConstraintExpected);
                    }
                    // otherwise set name to node ontology text
                    else
                        locatable.Name = new DvText(ValidationUtility.LocalTermDefText(this.NodeId, this));
                }

                Check.Ensure(defaultValue != null);
                return defaultValue;
            }
        }
        #endregion

        #region Functions

        Hashtable constraintAtPath = Hashtable.Synchronized(new Hashtable());

        public CObject ConstraintAtPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return this;

            if (constraintAtPath.Contains(path))
                return constraintAtPath[path] as CObject;

            CObject constraintFound = null;
            ConstraintPath constraintPath = new ConstraintPath(path);

            if (path.StartsWith("/") && this.Parent != null)
            {
                ArchetypeConstraint rootConstraint = this.ConstraintParent;
                while (rootConstraint.ConstraintParent != null)
                    rootConstraint = rootConstraint.ConstraintParent;

                CComplexObject cComplexObject = rootConstraint as CComplexObject;
                if (cComplexObject == null)
                    throw new ArgumentException(AmValidationStrings.RootConstraintInvalid);

                constraintFound = cComplexObject.ConstraintAtPath(path);

            }
            else
            {
                foreach (CAttribute attribute in this.attributes)
                {
                    if (attribute.RmAttributeName == constraintPath.FirstStepAttributeName)
                    {
                        System.Collections.Generic.IList<CObject> matchedChildren 
                            = new System.Collections.Generic.List<CObject>();
                        foreach (CObject cObject in attribute.Children)
                        {
                            if (cObject.NodeId == constraintPath.FirstStepNodeId)
                                matchedChildren.Add(cObject);
                            else if (cObject is CArchetypeRoot && cObject.ArchetypeNodeId == constraintPath.FirstStepNodeId)
                                matchedChildren.Add(cObject);
                        }
                        if (matchedChildren.Count <= 0)
                            throw new ArgumentException(string.Format(AmValidationStrings.
                                MissingChildrenWithNodeIdX, constraintPath.FirstStepNodeId));
                        else if (matchedChildren.Count == 1)
                            constraintFound = matchedChildren[0];

                        else if (!constraintPath.HasNameConstraint())
                            throw new ArgumentException(string.Format(AmValidationStrings.PathYNotUniqueAtX,
                                constraintPath.ToString(), constraintPath.FirstStepNodeId));
                        else
                        {
                            DvText name = (!string.IsNullOrEmpty(constraintPath.FirstStepNameValue) ?
                                new DvText(constraintPath.FirstStepNameValue) :
                                new DvCodedText(constraintPath.FirstStepName));

                            foreach (CObject cObject in matchedChildren)
                            {
                                if (CMultipleAttribute.HasNameAttributeConstraint(cObject, name))
                                {
                                    constraintFound = cObject;
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }

            if (constraintFound == null)
                throw new ArgumentException(string.Format(AmValidationStrings.NoConstraintForPathX, path));

            CComplexObject complexObject = constraintFound as CComplexObject;
            
            if (complexObject != null)
                constraintFound = complexObject.ConstraintAtPath(constraintPath.NextSteps);

            this.constraintAtPath.Add(path, constraintFound);

            Check.Ensure(constraintFound != null);

            return constraintFound;
        }


        private object GetAttributeValue(object obj, string attributeName)
        {
            Check.Require(obj != null, string.Format(CommonStrings.XMustNotBeNull, "obj"));

            System.ComponentModel.PropertyDescriptorCollection propertyDescriptorCollection =
            System.ComponentModel.TypeDescriptor.GetProperties(obj);

            System.ComponentModel.PropertyDescriptor property =
                propertyDescriptorCollection.Find(attributeName, true);

            if (property == null)
            {
                return null;
            }
          
            object attributeObj = property.GetValue(obj);

            return attributeObj;
        }

        /// <summary>
        /// True if any value of the reference model type being constrained is allowed.
        /// </summary>
        /// <returns></returns>
        public override bool AnyAllowed()
        {
            return !(this.Attributes != null && !this.Attributes.IsEmpty());
        }

        public override bool IsSubsetOf(ArchetypeConstraint other)
        {
            throw new NotImplementedException(
                string.Format(AmValidationStrings.IsSubsetNotImplementedInX, "CComplexObject"));
        }

        protected override System.Collections.Generic.List<string> GetPhysicalPaths()
        {
            if (this.Attributes == null || this.Attributes.Count <= 0)
                return null;

            System.Collections.Generic.List<string> physicalPaths = 
                new System.Collections.Generic.List<string>();

            if (this.Parent == null)
                physicalPaths.Add("/");

            if (this.Attributes != null || this.Attributes.Count > 0)
            {
                foreach (CAttribute attr in this.Attributes)
                {
                    physicalPaths.AddRange(attr.PhysicalPaths);
                }
            }

            return physicalPaths;
        }
       
        protected override string GetCurrentNodePath()
        {
            if (string.IsNullOrEmpty(this.NodeId) || this.NodeId == "at0000")
                return null;

            return "[" + this.NodeId + "]";
        }
        #endregion

        #region Validation

        public override bool ValidValue(object dataValue)
        {
            Check.Require(dataValue != null, string.Format(CommonStrings.XMustNotBeNull, "dataValue"));
            IRmType rmType = dataValue as IRmType;
            Check.Require(rmType != null, string.Format(AmValidationStrings.ValueMustImplementIRmType, dataValue.GetType().ToString()));

            bool result = true;
            rmType.Constraint = this;

            if (!IsSameRmType(rmType))
            {
                result = false;
                ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.IncorrectRmType, RmTypeName, rmType.GetRmTypeName()));
            }

            if (!result || !AnyAllowed())
            {
                OpenEhr.RM.Common.Archetyped.Impl.Locatable locatable = dataValue as OpenEhr.RM.Common.Archetyped.Impl.Locatable;

                if (locatable != null)
                {
                ValidationUtility.PopulateLocatableAttributes(this, locatable);
            
                    if (Parent != null && ArchetypeNodeId != locatable.ArchetypeNodeId)
            {
                        result = false;
                        ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.IncorrectNodeId, ArchetypeNodeId, locatable.ArchetypeNodeId));
            }
                }

                System.ComponentModel.PropertyDescriptorCollection propertyDescriptorCollection = System.ComponentModel.TypeDescriptor.GetProperties(dataValue);

                if (Attributes != null)
            {
                    foreach (CAttribute cAttribute in Attributes)
                {
                    object attributeObject = null;
                        string attributeName = RmFactory.GetOpenEhrV1RmName(cAttribute.RmAttributeName);
                        System.ComponentModel.PropertyDescriptor property = propertyDescriptorCollection.Find(attributeName, true);

                    // if the attributeName is not a class property, it must be a class function.
                    if (property == null)
                    {
                            System.Reflection.MethodInfo method = dataValue.GetType().GetMethod(attributeName);

                        if (method == null)
                        {
                                result = false;
                                ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.UnexpectedAttributeX, attributeName));
                            continue;
                        }
                        else
                                attributeObject = method.Invoke(dataValue, null);
                    }
                    else
                            attributeObject = property.GetValue(dataValue);

                        if (attributeObject == null)
                    {
                            if (cAttribute.Existence.Lower > 0)
                            {
                                result = false;
                                ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.TmExpectedConstraintMissing, cAttribute.RmAttributeName));
                            }
                        }
                        else if (cAttribute.Existence.Upper == 0)
                        {
                            result = false;
                            ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.TmForbiddenConstraint, cAttribute.RmAttributeName));
                        }
                    else if (!cAttribute.ValidValue(attributeObject))
                    {
                            result = false;
                    }
                    else
                    {
                            DvCodedText codedText = dataValue as DvCodedText;

                            if (codedText != null && cAttribute.RmAttributeName == "defining_code")
                        {
                                // validate the code string before validating the coded value
                                if (codedText.DefiningCode.TerminologyId.Value == "local")
                                {
                                    CObject parentObject = cAttribute.parent;
                                    CArchetypeRoot cArchetypeRoot = ValidationUtility.GetCArchetypeRoot(parentObject);

                                    if (!cArchetypeRoot.TermDefinitions.HasKey(codedText.DefiningCode.CodeString))
                                    {
                                        result = false;
                                        string code = codedText.DefiningCode == null ? "" : codedText.DefiningCode.CodeString;
                                        ValidationContext.AcceptValidationError(this, string.Format("code {0} is not existing archetype term", code));
                                    }
                                }
                                if (result && !ValidationUtility.ValidValueTermDef(codedText, cAttribute, ValidationContext.TerminologyService))
                                {
                                    result = false;
                                    string code = codedText.DefiningCode == null ? "" : codedText.DefiningCode.CodeString;
                                    ValidationContext.AcceptValidationError(this, string.Format(AmValidationStrings.TextValueXInvalidForCodeY, codedText.Value, code));
                                }
                        }
                    }
                }
            }
        }

            return result;
        }

        #endregion

        #region Inner Types

        private struct ConstraintPath
        {
            public ConstraintPath(string path)
            {
                Check.Require(!path.EndsWith("/"), string.Format(
                    AmValidationStrings.ConstraintPathBadEnding, path));
                if (path.StartsWith("/"))
                    path = path.TrimStart(new char[] { '/' });

                NextSteps = null;
                FirstStepNodeId = null;
                FirstStepAttributeName = null;
                FirstStepName = null;
                FirstStepNameValue = null;

                try
                {
                    string firstStep = null;

                    if (!path.Contains("/"))
                        firstStep = path;
                    else
                    {
                        int slashPos = path.IndexOf('/');
                        firstStep = path.Substring(0, slashPos);
                        NextSteps = path.Remove(0, slashPos + 1);
                    }

                    if (!firstStep.Contains("["))
                        FirstStepAttributeName = firstStep;
                    else
                    {
                        int leftBracketPos = firstStep.IndexOf("[");
                        FirstStepAttributeName = firstStep.Substring(0, leftBracketPos);

                        string predicate = firstStep.Remove(0, leftBracketPos + 1);

                        if (!predicate.Contains("and name"))
                            FirstStepNodeId = predicate.Replace("]", "");
                        else
                        {
                            int quotePos = predicate.IndexOf('\'');
                            string name = predicate.Substring(quotePos + 1).Replace("']", "");
                            if (predicate.Contains("name/value"))
                                FirstStepNameValue = name;
                            else
                                FirstStepName = name;
                        }
                    }

                    Check.Ensure(
                        (string.IsNullOrEmpty(FirstStepNameValue) && !string.IsNullOrEmpty(FirstStepName)) ||
                        (string.IsNullOrEmpty(FirstStepName) && !string.IsNullOrEmpty(FirstStepNameValue)) ||
                        (string.IsNullOrEmpty(FirstStepName) && string.IsNullOrEmpty(FirstStepNameValue)));
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(string.Format(
                        AmValidationStrings.ConstraintPathMalformed, path), ex);
                }
            }

            public bool HasNameConstraint()
            {
                return !string.IsNullOrEmpty(FirstStepName) || !string.IsNullOrEmpty(FirstStepNameValue);
            }

            public readonly string NextSteps ;
            public readonly string FirstStepNodeId;
            public readonly string FirstStepAttributeName ;
            public readonly string FirstStepName ;
            public readonly string FirstStepNameValue;
        }

        #endregion

        internal AM.Archetype.ConstraintModel.CAttribute GetAttribute(string attributeName)
        {
            if (attributes != null)
            {
                foreach (CAttribute attribute in attributes)
                {
                    if (attribute.RmAttributeName == attributeName)
                        return attribute;
                }
            }
            return null;
        }

        public static CAttribute GetAttribute(CComplexObject objConstraint, string attributeName)
        {
            return objConstraint.GetAttribute(attributeName);
        }
        
    }
}