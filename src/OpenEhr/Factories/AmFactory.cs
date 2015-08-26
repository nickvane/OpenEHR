using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.AM.Archetype.ConstraintModel;
using OpenEhr.AM.Archetype.Assertion;
using OpenEhr.AM.Archetype.ConstraintModel.Primitive;
using OpenEhr.AM.OpenehrProfile.DataTypes.Basic;
using OpenEhr.AM.OpenehrProfile.DataTypes.Text;
using OpenEhr.AM.OpenehrProfile.DataTypes.Quantity;
using OpenEhr.Resources;

namespace OpenEhr.Factories
{
    internal class AmFactory
    {       
        internal static CAttribute CAttribute(string typeName)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(typeName), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "typeName"));

            CAttribute cAttribute = null;

            switch (typeName)
            {
                case "C_SINGLE_ATTRIBUTE":
                    cAttribute = new CSingleAttribute();
                    break;
                case "C_MULTIPLE_ATTRIBUTE":
                    cAttribute = new CMultipleAttribute();
                    break;
                default:
                    throw new NotSupportedException("type not supported: " + typeName);
            }

            DesignByContract.Check.Ensure(cAttribute!= null, "cAttribute must not be null.");

            return cAttribute;
        }

        internal static ExprItem ExprItem(string typeName)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(typeName), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "typeName"));

            ExprItem exprItem = null;
            switch (typeName)
            {
                case "EXPR_LEAF":
                    exprItem = new ExprLeaf();
                    break;
                case "EXPR_UNARY_OPERATOR":
                    exprItem = new ExprUnaryOperator();
                    break;
                case "EXPR_BINARY_OPERATOR":
                    exprItem = new ExprBinaryOperator();
                    break;
                default:
                    throw new NotSupportedException("type not supported: " + typeName);
            }

            DesignByContract.Check.Ensure(exprItem != null, "exprItem must not be null.");

            return exprItem;
        }

        internal static CObject CObject(string typeName)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(typeName), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "typeName"));

            CObject cObject = null;
            switch (typeName)
            {
                case "C_COMPLEX_OBJECT":
                    cObject = new CComplexObject();
                    break;
                case "C_PRIMITIVE_OBJECT":
                    cObject = new CPrimitiveObject();
                    break;
                case "ARCHETYPE_INTERNAL_REF":
                    cObject = new ArchetypeInternalRef();
                    break;
                case "CONSTRAINT_REF":
                    cObject = new ConstraintRef();
                    break;
                case "ARCHETYPE_SLOT":
                    cObject = new ArchetypeSlot();
                    break;
                case "C_CODE_PHRASE":
                    cObject = new CCodePhrase();
                    break;
                case "C_DV_STATE":
                    cObject = new CDvState();
                    break;
                case "C_DV_ORDINAL":
                    cObject = new CDvOrdinal();
                    break;
                case "C_DV_QUANTITY":
                    cObject = new CDvQuantity();
                    break;
                default:
                    throw new NotSupportedException("type not supported: " + typeName);
            }

            DesignByContract.Check.Ensure(cObject != null, "cObject must not be null.");

            return cObject;
        }

        internal static OpenEhr.AM.Archetype.ConstraintModel.Primitive.CPrimitive CPrimitive(string typeName)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(typeName), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "typeName"));

            OpenEhr.AM.Archetype.ConstraintModel.Primitive.CPrimitive cPrimitive = null;
            switch (typeName)
            {
                case "C_BOOLEAN":
                    cPrimitive = new CBoolean();
                    break;
                case "C_DATE":
                    cPrimitive = new CDate();
                    break;
                case "C_DATE_TIME":
                    cPrimitive = new CDateTime();
                    break;
                case "C_DURATION":
                    cPrimitive = new CDuration();
                    break;
                case "C_TIME":
                    cPrimitive = new CTime();
                    break;
                case "C_INTEGER":
                    cPrimitive = new CInteger();
                    break;
                case "C_REAL":
                    cPrimitive = new CReal();
                    break;
                case "C_STRING":
                    cPrimitive = new CString();
                    break;
                default:
                    throw new NotSupportedException("type not supported: " + typeName);
            }

            DesignByContract.Check.Ensure(cPrimitive != null, "cObject must not be null.");

            return cPrimitive;
        }

        internal static State State(string typeName)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(typeName), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "typeName"));

            State state = null;

            switch (typeName)
            {
                case "NON_TERMINAL_STATE":
                    state = new NonTerminalState();
                    break;
                case "TERMINAL_STATE":
                    state = new TerminalState();
                    break;
                default:
                    throw new NotSupportedException("type not supported: " + typeName);
            }

            DesignByContract.Check.Ensure(state != null, "state must not be null.");

            return state;
        }

        internal static CComplexObject GetRootDefinition(ArchetypeInternalRef archeytpeInternalRef)
        {
            DesignByContract.Check.Require(archeytpeInternalRef != null, string.Format(CommonStrings.XMustNotBeNull, "archeytpeInternalRef"));
            DesignByContract.Check.Require(archeytpeInternalRef.Parent != null, string.Format(CommonStrings.XMustNotBeNull, "archeytpeInternalRef.Parent"));

            CComplexObject root = null;

            CAttribute parent = archeytpeInternalRef.Parent;
            while (parent != null)
            {
                root = parent.parent;
                parent = root.Parent;
            }

            DesignByContract.Check.Ensure(root != null, "Root definition must not be null.");

            return root;
        }
    }
}