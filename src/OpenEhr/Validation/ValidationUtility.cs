using System;
using OpenEhr.AM.Archetype.ConstraintModel;
using OpenEhr.Futures.OperationalTemplate;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Support.Terminology;
using OpenEhr.AM.Archetype.Assertion;
using OpenEhr.AM.Archetype.ConstraintModel.Primitive;

namespace OpenEhr.Validation
{
    public static class ValidationUtility
    {
        internal static string GetNameConstraint(CComplexObject cComplexObject)
        {
            var name = string.Empty;

            if (cComplexObject.Attributes == null) return name;
            foreach (CAttribute attribute in cComplexObject.Attributes)
            {
                if (attribute.RmAttributeName != "name" || attribute.Children.Count <= 0) continue;
                var primativeObject = attribute.Children[0] as CPrimitiveObject;
                if (primativeObject == null) continue;
                var cString = primativeObject.Item as CString;
                if (cString == null || cString.List.Count <= 0) continue;
                name = cString.List[0];
                break;
            }
            return name;
        }

        private static string _language = "en";
        internal static string Language
        {
            set
            {
                Check.Require(!string.IsNullOrEmpty(value), "languange must not be null or empty");
                _language = value;
            }
        }

        internal static string AssertionRegExPattern(Assertion assertion)
        {
            Check.Require(assertion != null, "assertion must not be null.");

            string assertionRegex = string.Empty;

            ExprBinaryOperator expression = assertion.Expression as ExprBinaryOperator;
            if (expression != null)
            {
                ExprLeaf rightExpression = expression.RightOperand as ExprLeaf;
                if (rightExpression != null)
                {
                    CString stringConstraint = rightExpression.Item as CString;
                    Check.Assert(stringConstraint != null);
                    Check.Assert(!string.IsNullOrEmpty(stringConstraint.Pattern));
                    Check.Assert(!(stringConstraint.Pattern.StartsWith("/") &&
                        stringConstraint.Pattern.EndsWith("/")), "Regex is enclosed in forward slashes, as with the C_STRING produced by old version of the ADL Parser.");

                    // Cleanse regex of fullstops which are not either preceded by a backslash (\.) or followed by a star (.*)
                    assertionRegex = stringConstraint.Pattern
                        .Replace(".*", " * ")
                        .Replace(@"\.", @" \ ")
                        .Replace(".", @"\.")
                        .Replace(@" \ ", @"\.")
                        .Replace(" * ", ".*");
                }
            }
            return assertionRegex;
        }


        internal static bool ValidValueTermDef(DvCodedText dvCodedText, CAttribute cAttribute, ITerminologyService terminologyService)
        {
            string value = string.Empty;
            if (dvCodedText.DefiningCode.TerminologyId.Value == OpenEhrTerminologyIdentifiers.TerminologyIdOpenehr)

                value = OpenEhrTermDefTerm(dvCodedText.DefiningCode.CodeString, terminologyService);

            else if (dvCodedText.DefiningCode.TerminologyId.Value == "local")
                value = LocalTermDefText(dvCodedText.DefiningCode.CodeString, cAttribute);

            if (!string.IsNullOrEmpty(value)
                && !string.IsNullOrEmpty(dvCodedText.Value)
                && value != dvCodedText.Value)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(dvCodedText.Value))
                dvCodedText.Value = value;

            return true;
        }

        internal static string OpenEhrTermDefTerm(string codeString, ITerminologyService terminologyService)
        {
            Check.Require(!string.IsNullOrEmpty(codeString), "codeString must not be null or Empty.");

            string termDefText = terminologyService.Terminology(OpenEhrTerminologyIdentifiers.TerminologyIdOpenehr)
                        .RubricForCode(codeString, _language);

            Check.Ensure(!string.IsNullOrEmpty(termDefText));

            return termDefText;
        }

        internal static string LocalTermDefText(string codeString, CAttribute cAttribute)
        {
            Check.Require(!string.IsNullOrEmpty(codeString), "codeString must not be null or empty.");
            Check.Require(cAttribute != null, "cAttribute must not be null");

            CComplexObject parent = cAttribute.parent;
            return LocalTermDefText(codeString, parent);
        }

        internal static void PopulateLocatableAttributes(CComplexObject cComplexObject, OpenEhr.RM.Common.Archetyped.Impl.Locatable locatable)
        {
            Check.Require(locatable != null, "locatable must not be null.");
            Check.Require(cComplexObject != null, "cComplexObject must not be null.");

            string codeString = null;

            if (string.IsNullOrEmpty(locatable.ArchetypeNodeId))
            {
                CArchetypeRoot archetypeRoot = cComplexObject as CArchetypeRoot;

                if (archetypeRoot != null)
                {
                    locatable.ArchetypeNodeId = archetypeRoot.ArchetypeId.Value;
                    codeString = cComplexObject.NodeId;
                }
                else
                {
                    locatable.ArchetypeNodeId = cComplexObject.NodeId;
                    codeString = cComplexObject.NodeId;
                }
            }

            if (locatable.Name == null || string.IsNullOrEmpty(locatable.Name.Value))
            {
                if (string.IsNullOrEmpty(codeString))
                    codeString = cComplexObject.NodeId;
                locatable.Name = new DvText(LocalTermDefText(codeString, cComplexObject));
            }

            Check.Ensure(!string.IsNullOrEmpty(locatable.ArchetypeNodeId), "ArchetypeId must not be null or empty.");
            Check.Ensure(locatable.Name != null && !string.IsNullOrEmpty(locatable.Name.Value), "name must not be null.");
        }

        public static string LocalTermDefText(string codeString, CObject cObject)
        {
            Check.Require(!string.IsNullOrEmpty(codeString), "codeString must not be null or empty.");
            Check.Require(cObject != null, "cObject must not be null");

            CArchetypeRoot cArchetypeRoot = GetCArchetypeRoot(cObject);

            Check.Assert(cArchetypeRoot.TermDefinitions.HasKey(codeString));

            string termDefText = cArchetypeRoot.TermDefinitions.Item(codeString).Items.Item("text");

            Check.Ensure(!string.IsNullOrEmpty(termDefText));

            return termDefText;
        }

        internal static CArchetypeRoot GetCArchetypeRoot(CObject cObject)
        {
            Check.Require(cObject != null, "cObject must not be null");

            CObject parentObject = cObject;
            CArchetypeRoot cArchetypeRoot = null;
            while (parentObject != null && (cArchetypeRoot = parentObject as CArchetypeRoot) == null)
            {
                CAttribute cattribute = parentObject.Parent;
                Check.Assert(cattribute != null, "cattribute must not be null");
                parentObject = cattribute.parent;
            }

            if (cArchetypeRoot == null)
                throw new ApplicationException("Operational template must contain CArchetypeRoot");

            return cArchetypeRoot;
        }
    }
}