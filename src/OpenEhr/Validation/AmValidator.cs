using System;
using OpenEhr.RM.Common.Resource;
using OpenEhr.AM.Archetype.Assertion;
using OpenEhr.AM.Archetype;
using OpenEhr.AM.Archetype.ConstraintModel;
using OpenEhr.AM.Archetype.ConstraintModel.Primitive;
using OpenEhr.AM.Archetype.Ontology;
using OpenEhr.AM.OpenehrProfile.DataTypes.Basic;
using OpenEhr.AM.OpenehrProfile.DataTypes.Text;
using OpenEhr.AM.OpenehrProfile.DataTypes.Quantity;
using OpenEhr.RM.DataTypes.Quantity;
using OpenEhr.Resources;
using OpenEhr.Factories;
using OpenEhr.RM.Support.Terminology;

namespace OpenEhr.Validation
{
    /// <summary>
    /// Validate archetype object against openEHR Archetype Reference Model
    /// </summary>
    public class AmValidator : RmValidator
    {
        protected AmValidator(ITerminologyService terminologyService)
            : base(terminologyService) { }

        public static void Validate(Archetype archetype, ITerminologyService terminologyService)
        {
            AmValidator amValidator = new AmValidator(terminologyService);

            amValidator.ValidateArchetype(archetype);
        }

        internal static bool ValidateCObject(CObject cObject, ITerminologyService terminologyService)
        {
            AmValidator amValidator = new AmValidator(terminologyService);

            try
            {
                amValidator.Validate(cObject);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(RmInvariantException))
                    return false;
                else
                    throw ex;
            }

            return true;
        }

        internal static bool ValidateCAttribute(CAttribute cAttribute, ITerminologyService terminologyService)
        {
            AmValidator amValidator = new AmValidator(terminologyService);

            try
            {
                amValidator.Validate(cAttribute);
            }
            catch(Exception ex)
            {
                if (ex.GetType() == typeof(RmInvariantException))
                    return false;
                else
                    throw ex;
            }

            return true;
        }

        protected void ValidateArchetype(Archetype archetype)
        {
            this.Validate((AuthoredResource)archetype);

            Invariant(archetype.ArchetypeId != null, string.Format(
                CommonStrings.XMustNotBeNull, "Archetype.ArchetypeId"));
            this.Validate(archetype.ArchetypeId);

            Invariant(archetype.Ontology.HasTermCode(archetype.Definition.NodeId),
                AmValidationStrings.OntologyMissingDefinitionTerm);
            Invariant(archetype.Uid == null || !string.IsNullOrEmpty(archetype.Uid.Value),
                string.Format(CommonStrings.IfXIsNotNullMustBeEmpty, "Archetype.Uid"));
            if (archetype.Uid != null)
                this.Validate(archetype.Uid);

            Invariant(!string.IsNullOrEmpty(archetype.Concept),
                string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Archetype.Concept"));

            if (archetype.ParentArchetypeId != null)
                this.Validate(archetype.ParentArchetypeId);

            Invariant(archetype.Version() != null, string.Format(CommonStrings.XMustNotBeNull, "Archetype.Version()"));
            Invariant(archetype.Version() == archetype.ArchetypeId.VersionId, AmValidationStrings.ArchetypeVersionNotEqual);

            Invariant(archetype.OriginalLanguage!=null, 
                string.Format(CommonStrings.XMustNotBeNull, "Archetype.OriginalLanguage"));
            this.Validate(archetype.OriginalLanguage);

            Invariant(archetype.Description != null,
                string.Format(CommonStrings.XMustNotBeNull, "Archetype.Description"));

            Invariant(archetype.Definition!= null,
                string.Format(CommonStrings.XMustNotBeNull, "Archetype.Definition"));
            this.Validate(archetype.Definition);

            Invariant(archetype.Ontology != null, "Archetype.Ontology must not be null.");
            this.Validate(archetype.Ontology);

            Invariant(!archetype.IsSpecialised() ^ (archetype.IsSpecialised() && archetype.SpecialisationDepth() > 0),
                AmValidationStrings.ArchetypeSpecialisationInvariantFail);

            Invariant(archetype.Invariants == null || !archetype.Invariants.IsEmpty(),
                string.Format(CommonStrings.IfXIsNotNullMustBeEmpty, "Archetype.Invariants"));
            if (archetype.Invariants != null)
            {
                foreach (Assertion eachInvariant in archetype.Invariants)
                    this.Validate(eachInvariant);
            }
            
        }

        protected void Validate(ValidityKind validityKind)
        {
            Invariant(ValidityKind.ValidValidity(validityKind.Value), string.Format(
                AmValidationStrings.XMustBeValidY, validityKind.Value, "ValidityKind"));
        }

        #region Constraint model
        private System.Reflection.MethodInfo lastCObjectMethod = null;
        private CObject lastCObject = null;
        protected void Validate(CObject cObject)
        {
            if (cObject == null) throw new ArgumentNullException(string.Format(
                CommonStrings.XMustNotBeNull, "cObject"));

            const string methodName = "Validate";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { cObject.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    // Avoid StackOverflow exceptions by executing only if the method and visitable  
                    // are different from the last parameters used.
                    if (method != lastCObjectMethod || cObject != lastCObject)
                    {
                        lastCObjectMethod = method;
                        lastCObject = cObject;

                        method.Invoke(this, new Object[] { cObject });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated,
                            methodName, cObject.GetType().ToString());
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY,
                        methodName, cObject.GetType().ToString());
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    if (ex.InnerException is ApplicationException && ex.InnerException.InnerException != null
                            && ex.InnerException.Message == ex.InnerException.InnerException.Message)
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException.InnerException);
                    else
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        protected void Validate(ArchetypeConstraint archetypeConstraint)
        {
            Invariant(!string.IsNullOrEmpty(archetypeConstraint.Path), string.Format(
                CommonStrings.XMustNotBeNullOrEmpty, "ArchetypeConstraint.Path"));
        }

        private System.Reflection.MethodInfo lastCAttributeMethodRead = null;
        private CAttribute lastCAttribute = null;
        protected void Validate(CAttribute cAttribute)
        {
            if (cAttribute == null) throw new ArgumentNullException(string.Format(
                CommonStrings.XIsNull, "cAttribute"));

            const string methodName = "Validate";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { cAttribute.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    if (method != lastCAttributeMethodRead || cAttribute != lastCAttribute)
                    {
                        lastCAttributeMethodRead = method;
                        lastCAttribute = cAttribute;

                        method.Invoke(this, new Object[] { cAttribute });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated,
                            methodName, cAttribute.GetType().ToString());
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY,
                        methodName, cAttribute.GetType().ToString());
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    if (ex.InnerException is ApplicationException && ex.InnerException.InnerException != null
                            && ex.InnerException.Message == ex.InnerException.InnerException.Message)
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException.InnerException);
                    else
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        protected void ValidateBase(CAttribute cAttribute)
        {
            this.Validate((ArchetypeConstraint)cAttribute);

            Invariant(!string.IsNullOrEmpty(cAttribute.RmAttributeName), string.Format(
                CommonStrings.XMustNotBeNullOrEmpty, "CAttribute.RmAttributeName"));
            Invariant(cAttribute.Existence != null,  string.Format(
                CommonStrings.XMustNotBeNullOrEmpty, "CAttribute.Existence"));
            this.Validate(cAttribute.Existence);
            Invariant(cAttribute.Existence.Lower >= 0 && cAttribute.Existence.Upper <= 1,
                AmValidationStrings.ExistenceOutOfRange);

            // TODO: Children_validity: any_allowed xor children /= Void

            if (cAttribute.Children != null)
            {
                foreach (CObject cObj in cAttribute.Children)
                    this.Validate(cObj);
            }
           
        }

        protected void Validate(CSingleAttribute cSingleAttribute)
        {
            this.ValidateBase((CAttribute)cSingleAttribute);
        }

        protected void Validate(CMultipleAttribute cMultipleAttribute)
        {
            this.ValidateBase((CAttribute)cMultipleAttribute);

            Invariant(cMultipleAttribute.Cardinality != null, string.Format(
                CommonStrings.XMustNotBeNull, "CMultipleAttribute.Cardinality"));
            this.Validate(cMultipleAttribute.Cardinality);

            if (cMultipleAttribute.Members() != null)
            {
                foreach (CObject member in cMultipleAttribute.Members())
                    Invariant(member.Occurrences.Upper <= 1,
                        AmValidationStrings.MultiAttributeMembersBad);
            }
        }

        protected void Validate(Cardinality cardinality)
        {
            Invariant(cardinality.Interval != null, string.Format(
                CommonStrings.XMustNotBeNull, "Cardinality.Interval"));
            Invariant(!cardinality.Interval.LowerUnbounded, AmValidationStrings.CardinalityLowerUnbounded);
        }

        protected void ValidateBase(CObject cObject)
        {
            this.Validate((ArchetypeConstraint)cObject);

            Invariant(!string.IsNullOrEmpty(cObject.RmTypeName), string.Format(
                CommonStrings.XMustNotBeNullOrEmpty, "CObject.RmTypeName"));
            Invariant(cObject.Occurrences!= null,  string.Format(
                CommonStrings.XMustNotBeNull, "CObject.Occurrences"));
            this.Validate(cObject.Occurrences);

            // TODO: nodeId invariants checking

            if (cObject.Parent != null)
            {
                Invariant(cObject.Parent.GetType() != typeof(CSingleAttribute) || cObject.Occurrences.Upper <= 1,
                   AmValidationStrings.SingleParentOccurrencesBad);
            }
        }

        protected void Validate(CDefinedObject cDefinedObject)
        {
            this.ValidateBase((CObject)cDefinedObject);

            if (cDefinedObject.HasAssumedValue())
            {
                Invariant(cDefinedObject.ValidValue(cDefinedObject.AssumedValue), AmValidationStrings.AssumedValueInvalid);
            }
        }

        protected void Validate(CComplexObject cComplexObject)
        {
            this.Validate((CDefinedObject)cComplexObject);

            if (cComplexObject.Attributes != null)
            {
                foreach (CAttribute attri in cComplexObject.Attributes)
                    this.Validate(attri);
            }

            Invariant(cComplexObject.AnyAllowed() || (cComplexObject.Attributes != null && !cComplexObject.Attributes.IsEmpty()),
                AmValidationStrings.CComplexObjectAllowAnyXor);
        }

        protected void Validate(CPrimitiveObject cPrimitiveObject)
        {
            this.Validate((CDefinedObject)cPrimitiveObject);

            Invariant(cPrimitiveObject.AnyAllowed() ^ cPrimitiveObject.Item != null,
                AmValidationStrings.CPrimitiveObjectAllowAnyXor);

            if (cPrimitiveObject.Item != null)
                this.Validate(cPrimitiveObject.Item);
        }

        protected void Validate(CDomainType cDomainType)
        {
            this.Validate((CDefinedObject)cDomainType);          
        }

        protected void Validate(ArchetypeSlot archetypeSlot)
        {
            this.ValidateBase((CObject)archetypeSlot);

            Invariant(archetypeSlot.Includes == null || !archetypeSlot.Includes.IsEmpty(),
                string.Format(CommonStrings.IfXIsNotNullMustBeEmpty, "ArchetypeSlot.Includes"));

            Invariant(archetypeSlot.Excludes == null || !archetypeSlot.Excludes.IsEmpty(),
                string.Format(CommonStrings.IfXIsNotNullMustBeEmpty, "ArchetypeSlot.Excludes"));

            // TODO: validity: any_allowed xor (includes /= Void or excludes /= Void)

            if (archetypeSlot.Includes != null)
            {
                foreach (Assertion assertion in archetypeSlot.Includes)
                    this.Validate(assertion);
            }

            if (archetypeSlot.Excludes != null)
            {
                foreach (Assertion assertion in archetypeSlot.Excludes)
                    this.Validate(assertion);
            }
        }

        protected void Validate(ArchetypeInternalRef archetypeInternalRef)
        {
            this.ValidateBase((CObject)archetypeInternalRef);

            Invariant(!string.IsNullOrEmpty(archetypeInternalRef.TargetPath), string.Format(
                CommonStrings.XMustNotBeNullOrEmpty, "AarchetypeInternalRef.TargetPath"));

            // TODO: Consistency: not any_allowed
            CComplexObject rootDefinition = AmFactory.GetRootDefinition(archetypeInternalRef);
            Invariant(rootDefinition.HasPath(archetypeInternalRef.TargetPath),
                AmValidationStrings.ArchetypeInternalRefTargetPathMissing);
        }

        protected void Validate(ConstraintRef constraintRef)
        {
            this.ValidateBase((CObject)constraintRef);

            Invariant(constraintRef.Reference != null, string.Format(
                CommonStrings.XMustNotBeNull, "ConstraintRef.Reference"));

            // TODO: Consistency: not any_allowed           
        }
        #endregion

        #region Assertion package classes
        protected void Validate(Assertion assertion)
        {
            Invariant(assertion.Tag == null || assertion.Tag != string.Empty,
                string.Format(CommonStrings.IfXIsNotNullMustBeEmpty, "Assertion.Tag"));

            Invariant(assertion.Expression != null, string.Format(CommonStrings.XMustNotBeNull, "Assertion.Expression"));
            Invariant(assertion.Expression.Type.Equals("BOOLEAN", 
                StringComparison.CurrentCultureIgnoreCase), CommonStrings.AssertionExpressMustBeBool);

            if (assertion.Variables != null)
            {
                foreach (AssertionVariable var in assertion.Variables)
                    this.Validate(var);
            }
        }

        protected void Validate(AssertionVariable assertionVariable)
        {
            Invariant(!string.IsNullOrEmpty(assertionVariable.Name), string.Format(
                CommonStrings.XMustNotBeNull, "AssertionVariable.Name"));
            Invariant(!string.IsNullOrEmpty(assertionVariable.Definition), string.Format(
                CommonStrings.XMustNotBeNull, "AssertionVariable.Definition"));
        }

        private System.Reflection.MethodInfo lastExprItemMethodRead = null;
        private ExprItem lastExprItem = null;
        protected void Validate(ExprItem exprItem)
        {
            if (exprItem == null) throw new ArgumentNullException(string.Format(
                CommonStrings.XMustNotBeNull, "exprItem"));

            const string methodName = "Validate";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { exprItem.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    if (method != lastExprItemMethodRead || exprItem != lastExprItem)
                    {
                        lastExprItemMethodRead = method;
                        lastExprItem = exprItem;

                        method.Invoke(this, new Object[] { exprItem });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated,
                            methodName, exprItem.GetType().ToString());
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY,
                        methodName, exprItem.GetType().ToString());
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    if (ex.InnerException is ApplicationException && ex.InnerException.InnerException != null
                            && ex.InnerException.Message == ex.InnerException.InnerException.Message)
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException.InnerException);
                    else
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        protected void ValidateBase(ExprItem exprItem)
        {
            Invariant(!string.IsNullOrEmpty(exprItem.Type), string.Format(
                CommonStrings.XMustNotBeNull, "ExprItem.Type"));
        }

        protected void Validate(ExprLeaf exprLeaf)
        {
            this.ValidateBase((ExprItem)exprLeaf);

            Invariant(exprLeaf.Item != null, string.Format(CommonStrings.XMustNotBeNull, "ExprLeaf.Item"));
            Invariant(exprLeaf.ReferenceType != null, string.Format(CommonStrings.XMustNotBeNull, "ExprLeaf.ReferenceType"));
        }

        private System.Reflection.MethodInfo lastExprOperatorMethod = null;
        private ExprOperator lastExprOperator = null;
        protected void Validate(ExprOperator exprOperator)
        {
            if (exprOperator == null) throw new ArgumentNullException(
                string.Format(CommonStrings.XIsNull, "exprOperator"));

            const string methodName = "Validate";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { exprOperator.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                   if (method != lastExprOperatorMethod || exprOperator != lastExprOperator)
                    {
                        lastExprOperatorMethod = method;
                        lastExprOperator = exprOperator;

                        method.Invoke(this, new Object[] { exprOperator });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated,
                             methodName, exprOperator.GetType().ToString());
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY,
                             methodName, exprOperator.GetType().ToString());
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    if (ex.InnerException is ApplicationException && ex.InnerException.InnerException != null
                            && ex.InnerException.Message == ex.InnerException.InnerException.Message)
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException.InnerException);
                    else
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        protected void ValidateBase(ExprOperator exprOperator)
        {
            this.ValidateBase((ExprItem)exprOperator);

            Invariant(exprOperator.Operator != null, 
                string.Format(CommonStrings.XMustNotBeNull, "ExprOperator.Operator"));
            this.Validate(exprOperator.Operator);
        }

        protected void Validate(ExprUnaryOperator unaryOperator)
        {
            this.ValidateBase((ExprOperator)unaryOperator);

            Invariant(unaryOperator.Operand != null,
                string.Format(CommonStrings.XMustNotBeNull, "unaryOperator.Operand"));
            this.Validate(unaryOperator.Operand);
        }

        protected void Validate(ExprBinaryOperator binaryOperator)
        {
            this.ValidateBase((ExprOperator)binaryOperator);

            Invariant(binaryOperator.LeftOperand != null,
                string.Format(CommonStrings.XMustNotBeNull, "binaryOperator.LeftOperand"));
            this.Validate(binaryOperator.LeftOperand);
            Invariant(binaryOperator.RightOperand != null,
                string.Format(CommonStrings.XMustNotBeNull, "binaryOperator.RightOperand"));
            this.Validate(binaryOperator.RightOperand);
        }

        protected void Validate(OperatorKind operatorKind)
        {
            Invariant(OperatorKind.ValidOperator(operatorKind.Value),
                string.Format(AmValidationStrings.XMustBeValidY, "OperatorKind.Value", "OperatorKind"));
        }
        #endregion

        #region Primitive package classes
        private System.Reflection.MethodInfo lastCPrimitiveMethod = null;
        private CPrimitive lastCPrimitive = null;
        protected void Validate(CPrimitive cPrimitive)
        {
            if (cPrimitive == null) throw new ArgumentNullException(string.Format(
                CommonStrings.XMustNotBeNull, "cPrimitive"));

            const string methodName = "Validate";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { cPrimitive.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    if (method != lastCPrimitiveMethod || cPrimitive != lastCPrimitive)
                    {
                        lastCPrimitiveMethod = method;
                        lastCPrimitive = cPrimitive;

                        method.Invoke(this, new Object[] { cPrimitive });

                    }
                    else
                    {
                        string message = string.Format(CommonStrings.LoopingMethodTerminated,
                            methodName, cPrimitive.GetType().ToString());
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = string.Format(CommonStrings.MethodXNotImplementedForParamTypeY,
                        methodName, cPrimitive.GetType().ToString());
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    if (ex.InnerException is ApplicationException && ex.InnerException.InnerException != null
                            && ex.InnerException.Message == ex.InnerException.InnerException.Message)
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException.InnerException);
                    else
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        protected void ValidateBase(CPrimitive cPrimitive)
        {
            Invariant(!cPrimitive.HasAssumedValue() || 
                string.IsNullOrEmpty(cPrimitive.ValidValue(cPrimitive.AssumedValue)),
                AmValidationStrings.AssumedValueInvalid);
        }

        protected void Validate(CBoolean cBoolean)
        {
            this.ValidateBase((CPrimitive)cBoolean);
            Invariant(cBoolean.TrueValid || cBoolean.FalseValid, CommonStrings.CBooleanInvariant);

            Invariant(((bool)(cBoolean.DefaultValue) && cBoolean.TrueValid) ||
                (!((bool)(cBoolean.DefaultValue)) && cBoolean.FalseValid), CommonStrings.CBooleanDefaultInvalid);
        }

        protected void Validate(CString cString)
        {
            this.ValidateBase((CPrimitive)cString);

            Invariant(cString.Pattern != null ^ cString.List != null,
                CommonStrings.CStringListXorPattern);

            Invariant(cString.Pattern == null || cString.Pattern != string.Empty,
                string.Format(CommonStrings.IfXIsNotNullMustBeEmpty, "CString.Pattern"));
        }

        protected void Validate(CInteger cInteger)
        {
            this.ValidateBase((CPrimitive)cInteger);

            Invariant(cInteger.List != null ^ cInteger.Range != null, CommonStrings.CIntegerListXorRange);

            if (cInteger.Range != null)
                this.Validate(cInteger.Range);
        }

        protected void Validate(CReal cReal)
        {
            this.ValidateBase((CPrimitive)cReal);

            Invariant(cReal.List != null ^ cReal.Range != null, CommonStrings.CRealListXorRange);

            if (cReal.Range != null)
                this.Validate(cReal.Range);
        }

        protected void Validate(CDate cDate)
        {
            this.ValidateBase((CPrimitive)cDate);

            if (cDate.MonthValidity != null)
            {
                this.Validate(cDate.MonthValidity);
                Invariant(cDate.MonthValidity.value != 1002 || (cDate.DayValidity.value == 1002 || cDate.DayValidity.value == 1003),
                    "cDate.MonthValidity is optional, means DayValidity must be either optional or disallowed.");

                Invariant(cDate.MonthValidity.value != 1003 || cDate.DayValidity.value == 1003,
                    "cDate.MonthValidity is disallowed, implies cDate.DayValidity must be disallowed.");
            }
        }

        protected void Validate(CTime cTime)
        {
            this.ValidateBase((CPrimitive)cTime);

            if (cTime.MinuteValidity != null)
            {
                this.Validate(cTime.MinuteValidity);
                Invariant(cTime.MinuteValidity.value != 1002 || (cTime.SecondValidity.value == 1002 || cTime.SecondValidity.value == 1003),
                    "cTime.MinuteValidity.valueis optional, means DayValidity must be either optional or disallowed.");

                Invariant(cTime.MinuteValidity.value != 1003 || cTime.SecondValidity.value == 1003,
                    "cTime.MinuteValidity is disallowed, implies cTime.SecondValidity must be disallowed.");
            }

            if (cTime.SecondValidity != null)
            {
                this.Validate(cTime.SecondValidity);
                Invariant(cTime.SecondValidity.value != 1002 || (cTime.MillisecondValidity.value == 1002 || cTime.MillisecondValidity.value == 1003),
                    "cTime.SecondValidity is optional, means SecondValidity must be either optional or disallowed.");

                Invariant(cTime.SecondValidity.value != 1003 || cTime.MillisecondValidity.value == 1003,
                  "cTime.SecondValidity is disallowed, implies cTime.MillisecondValidity must be disallowed.");
            }          
        }

        protected void Validate(CDateTime cDateTime)
        {
            this.ValidateBase((CPrimitive)cDateTime);

            if (cDateTime.MonthValidity != null)
            {
                this.Validate(cDateTime.MonthValidity);
                Invariant(cDateTime.MonthValidity.value != 1002 || (cDateTime.DayValidity.value == 1002 || cDateTime.DayValidity.value == 1003),
                    "cDateTime.MonthValidity is optional, means DayValidity must be either optional or disallowed.");

                Invariant(cDateTime.MonthValidity.value != 1003 || cDateTime.DayValidity.value == 1003,
                    "cDateTime.MonthValidity is disallowed, implies DayValidity must be disallowed.");
            }

            if (cDateTime.DayValidity != null)
            {
                this.Validate(cDateTime.DayValidity);
                
                Invariant(cDateTime.DayValidity.value != 1002 || (cDateTime.HourValidity.value == 1002 || cDateTime.HourValidity.value == 1003),
                    "cDateTime.DayValidity is optional, means HourValidity must be either optional or disallowed.");

                Invariant(cDateTime.MonthValidity.value != 1003 || cDateTime.DayValidity.value == 1003,
                    "cDateTime.DayValidity is disallowed, implies HourValidity must be disallowed.");
            }

            if (cDateTime.HourValidity != null)
            {
                this.Validate(cDateTime.HourValidity);

                Invariant(cDateTime.HourValidity.value != 1002 || (cDateTime.MinuteValidity.value == 1002 || cDateTime.MinuteValidity.value == 1003),
                    "cDateTime.HourValidity is optional, means MinuteValidity must be either optional or disallowed.");

                Invariant(cDateTime.HourValidity.value != 1003 || cDateTime.MinuteValidity.value == 1003,
                    "cDateTime.HourValidity is disallowed, implies MinuteValidity must be disallowed.");
            }

            if (cDateTime.MinuteValidity != null)
            {
                this.Validate(cDateTime.MinuteValidity);
                Invariant(cDateTime.MinuteValidity.value != 1002 || (cDateTime.SecondValidity.value == 1002 || cDateTime.SecondValidity.value == 1003),
                    "cDateTime.MinuteValidity.valueis optional, means DayValidity must be either optional or disallowed.");

                Invariant(cDateTime.MinuteValidity.value != 1003 || cDateTime.SecondValidity.value == 1003,
                    "cDateTime.MinuteValidity is disallowed, implies cDateTime.SecondValidity must be disallowed.");
            }

            if (cDateTime.SecondValidity != null)
            {
                this.Validate(cDateTime.SecondValidity);
                Invariant(cDateTime.SecondValidity.value != 1002 || (cDateTime.MillisecondValidity.value == 1002 || cDateTime.MillisecondValidity.value == 1003),
                    "cDate.SecondValidity is optional, means SecondValidity must be either optional or disallowed.");

                Invariant(cDateTime.SecondValidity.value != 1003 || cDateTime.MillisecondValidity.value == 1003,
                  "cDate.SecondValidity is disallowed, implies cDate.MillisecondValidity must be disallowed.");
            }
        }

        protected void Validate(CDuration cDuration)
        {
            Invariant(cDuration.Range != null ||(cDuration.YearsAllowed || cDuration.MonthsAllowed || cDuration.WeeksAllowed
                || cDuration.DaysAllowed || cDuration.HoursAllowed || cDuration.MinutesAllowed || cDuration.SecondsAllowed ||
                cDuration.FractionalSecondsAllowed), "cDuration range is null, implies years_allowed or months_allowed or "+
                "weeks_allowed or days_allowed or hours_allowed or minutes_allowed or "+
                "seconds_allowed or fractional_seconds_allowed");
        }

        #endregion

        #region Ontology package
        protected void Validate(ArchetypeOntology archetypeOntology)
        {
            Invariant(archetypeOntology.TerminologyesAvailable != null, "archetypeOntology.TerminologesAvailable must not be null.");
            Invariant(archetypeOntology.SpecialisationDepth >= 0, "archetypeOntology.SpecialisationDepth must be >=0.");
            Invariant(archetypeOntology.TermCodes != null, "archetypeOntology.TermCodes must not be null.");
            Invariant(archetypeOntology.ConstraintCodes != null, "archetypeOntology.ConstraintCodes must not be null.");
            Invariant(archetypeOntology.TermAttributeNames != null, "archetypeOntology.TermAttributeNames must not be null.");
            Invariant(archetypeOntology.ParentArchetype != null, "archetypeOntology.PaentArchetype must not be null.");
            Invariant(archetypeOntology.TermAttributeNames.Has("text"), "archetypeOntology.TermAttributeNames must have 'text'");
            Invariant(archetypeOntology.TermAttributeNames.Has("description"), "archetypeOntology.TermAttributeNames must have 'description'");
        }

        protected void Validate(ArchetypeTerm archetypeTerm)
        {
            Invariant(!string.IsNullOrEmpty(archetypeTerm.Code), "archetypeTerm.Code must not be null or empty.");
            Invariant(archetypeTerm.Keys()!= null, "archeytpeTerm.Keys() must not be null.");
        }
        #endregion

        #region openEHR Profile package
        protected void Validate(CDvState cDvState)
        {
            this.Validate((CDomainType)cDvState);
            Invariant(cDvState.Value != null, "cDvState.Value must not be null.");
            this.Validate(cDvState.Value);
        }

        protected void Validate(StateMachine stateMachine)
        {
            Invariant(stateMachine.States != null && !stateMachine.States.IsEmpty(), "stateMachine.States must not be null or empty.");
            foreach (State state in stateMachine.States)
                this.Validate(state);
        }

        private System.Reflection.MethodInfo lastStateMethod = null;
        private State lastState = null;
        protected void Validate(State state)
        {
            if (state == null) throw new ArgumentNullException("state must not be null.");

            const string methodName = "Validate";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { state.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    if (method != lastStateMethod || state != lastState)
                    {
                        lastStateMethod = method;
                        lastState = state;

                        method.Invoke(this, new Object[] { state });

                    }
                    else
                    {
                        string message = "The method '" + methodName + "' with parameter type '"
                            + state.GetType().ToString() + "' is looping and is terminated.";
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = "The method '" + methodName + "' with parameter type '"
                        + state.GetType().ToString() + "' is not implemented.";
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    if (ex.InnerException is ApplicationException && ex.InnerException.InnerException != null
                            && ex.InnerException.Message == ex.InnerException.InnerException.Message)
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException.InnerException);
                    else
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        protected void ValidateBase(State state)
        {
            Invariant(!string.IsNullOrEmpty(state.Name), "state.Name must not be null or empty.");
        }

        protected void Validate(NonTerminalState nonTerminalState)
        {
            this.ValidateBase((State)nonTerminalState);
            Invariant(nonTerminalState.Transitions!= null && !nonTerminalState.Transitions.IsEmpty(),
                "nonTerminalState.Transitions must not be null or empty.");
            foreach (Transition transition in nonTerminalState.Transitions)
                this.Validate(transition);
        }

        protected void Validate(TerminalState terminalState)
        {
            this.ValidateBase((State)terminalState);
        }

        protected void Validate(Transition transition)
        {
            Invariant(!string.IsNullOrEmpty(transition.Event), "transition.Event must not be null or empty.");
            
            Invariant(transition.Action == null || transition.Action!=string.Empty, 
                "transition.Action is not null, implies it must not be empty.");

            Invariant(transition.Guard == null || transition.Guard != string.Empty,
              "transition.Guard is not null, implies it must not be empty.");

            Invariant(transition.NextState != null, "transition.NextState must not be null.");
        }

        protected void Validate(CCodePhrase cCodePhrase)
        {
            this.Validate((CDomainType)cCodePhrase);

            Invariant(cCodePhrase.CodeList == null || (!cCodePhrase.CodeList.IsEmpty() && cCodePhrase.TerminologyId != null),
                "cCodePhrase.CodeList is not null, implies it must not be empty and terminologyId must not be null.");

            Invariant(cCodePhrase.AnyAllowed() ^ cCodePhrase.TerminologyId != null,
                "cCodePhrase.AnyAllowed xor cCodePhrase.TerminologyId must not be null.");
        }

        protected void Validate(CDvOrdinal cDvOrdinal)
        {
            this.Validate((CDomainType)cDvOrdinal);

            Invariant(cDvOrdinal.List == null || !cDvOrdinal.List.IsEmpty(), "cDvOrdinal.List is not null, implies it must not be empty.");
            Invariant(cDvOrdinal.AnyAllowed() ^ cDvOrdinal.List != null, "cDvOrdinal.AnyAllowed() XOR cDvOrdinal.List != null.");

            if (cDvOrdinal.List != null)
            {
                foreach (DvOrdinal dvOrdinal in cDvOrdinal.List)
                    this.Validate(dvOrdinal);
            }
        }

        protected void Validate(CDvQuantity cDvQuantity)
        {
            this.Validate((CDomainType)cDvQuantity);

            Invariant(cDvQuantity.List == null || !cDvQuantity.List.IsEmpty(), "cDvQuantity.List is not null, implies it must not be empty.");
            Invariant(cDvQuantity.AnyAllowed() ^ (cDvQuantity.List != null || cDvQuantity.Property!=null),
                "cDvQuantity.AnyAllowed() XOR (cDvQuantity.List != null or cDvQuantity.Property != null.)");

            if (cDvQuantity.List != null)
            {
                foreach (CQuantityItem quantityItem in cDvQuantity.List)
                    this.Validate(quantityItem);
            }

            if (cDvQuantity.Property != null)
                this.Validate(cDvQuantity.Property);
        }

        protected void Validate(CQuantityItem cQuantityItem)
        {
            Invariant(!string.IsNullOrEmpty(cQuantityItem.Units), "cquantityItem.Units must not be null or empty.");
        }

        #endregion
    }
}
