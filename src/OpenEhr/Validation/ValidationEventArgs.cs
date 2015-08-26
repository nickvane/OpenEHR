using System;
using OpenEhr.AM.Archetype.ConstraintModel;
using OpenEhr.DesignByContract;
using OpenEhr.Futures.OperationalTemplate;
using OpenEhr.AM.Archetype.ConstraintModel.Primitive;
using OpenEhr.Utilities;

namespace OpenEhr.Validation
{
    public class ValidationEventArgs : EventArgs
    {
        internal ValidationEventArgs(ArchetypeConstraint constraint, string message)
        {
            Check.Require(constraint != null, "constraint must not be null.");
            Check.Require(!string.IsNullOrEmpty(message), "message must not be null or empty.");

            this.message = message;
            this.constraint = constraint;

            this.path = new Lazy<string>(FullPath);
        }

        private ArchetypeConstraint constraint;

        readonly Lazy<string> path ; 

        public string Path
        {
            get 
            {
                return path.Value;
            }
        }

        private string message;
        public string Message
        {
            get { return message; }
        }

        protected virtual string FullPath()
        {
            string result = "";
            ArchetypeConstraint parent = constraint;

            do
            {
                CArchetypeRoot cArchetypeRoot = parent as CArchetypeRoot;

                if (cArchetypeRoot != null)
                {
                    string rootPath = cArchetypeRoot.Parent != null ? cArchetypeRoot.Path : "";

                    if (result.StartsWith(rootPath))
                        result = result.Substring(rootPath.Length);

                    result = rootPath + CArchetypeRootPredicate(cArchetypeRoot) + result;
                    parent = cArchetypeRoot.Parent;
                }
                else
                {
                    if (string.IsNullOrEmpty(result))
                        result = parent.Path;

                    CObject cObject = parent as CObject;

                    if (cObject != null)
                        parent = cObject.Parent;
                    else
                    {
                        CAttribute cAttribute = parent as CAttribute;

                        if (cAttribute != null)
                            parent = cAttribute.parent;
                    }
                }
            } while (parent != null);

            Check.Ensure(result != null);
            return result;
        }

        protected virtual string CArchetypeRootPredicate(CArchetypeRoot cArchetypeRoot)
        {
            string result = "";
            string name = GetName(cArchetypeRoot);

            if (!string.IsNullOrEmpty(name))
                result = string.Format("[{0} and name/value='{1}']", cArchetypeRoot.ArchetypeId, name);
            else
                result = string.Format("[{0}]", cArchetypeRoot.ArchetypeNodeId);

            Check.Ensure(result != null);
            return result;
        }

        private static string GetName(CComplexObject cComplexObject)
        {
            CComplexObject nameAttribute = GetCObjectByAttributeName(cComplexObject, "name") as CComplexObject;

            if (nameAttribute != null)
            {
                CPrimitiveObject cPrimativeObject = GetCObjectByAttributeName(nameAttribute, "value") as CPrimitiveObject;

                Check.Assert(cPrimativeObject != null);

                CString cString = cPrimativeObject.Item as CString;

                Check.Assert(cString != null);

                foreach(string name in cString.List)
                    return name;
            }

            return string.Empty;
        }

        private static CObject GetCObjectByAttributeName(CComplexObject cComplexObject, string attributeName)
        {
            foreach (CAttribute attribute in cComplexObject.Attributes)
            {
                if (attribute.RmAttributeName == attributeName)
                    return attribute.Children[0];
            }

            return null;
        }
    }
}
