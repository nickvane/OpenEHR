using System;
using OpenEhr.DesignByContract;
using OpenEhr.AM.Archetype.Assertion;
using OpenEhr.RM.Common.Archetyped.Impl;

namespace OpenEhr.Paths
{
    internal class PathExpr: ExprItem
    {
        internal PathExpr(string path, System.Collections.Generic.List<PathStep> pathSteps)
            : base("path")
        {
            Check.Require(!string.IsNullOrEmpty(path), "path must not be null or empty.");

            this.path = path;
            this.pathSteps = pathSteps;
        }

        #region class properties
        private System.Collections.Generic.List<PathStep> pathSteps;

        internal System.Collections.Generic.List<PathStep> PathSteps
        {
            get { return pathSteps; }
        }

        private string path;

        internal string Path
        {
            get { return path; }
        }

        #endregion


        internal override AssertionContext Evaluate(AssertionContext contextObj)
        {
            if (this.pathSteps == null || this.pathSteps.Count == 0)
                return contextObj;

            AssertionContext tempContext = contextObj;
            foreach (PathStep pathStep in this.pathSteps)
            {
                string attributeName = pathStep.Attribute;

                if (attributeName !="//*" && attributeName!="/*")
                {
                    object attrObj = CallGetAttributeObject(attributeName, tempContext.Data);
                    if (attrObj == null)
                        return null;

                    tempContext = new AssertionContext(attrObj, tempContext);

                    tempContext = ProcessPathPartWithAttrObject(tempContext, pathStep);
                }
                else // deal with path starts with wildcard key //*
                {
                    // only consider all properties of the rootObject children
                    if (attributeName == "/*")
                    {
                        // get all attribute
                        object rootObj = tempContext.Data;

                        tempContext = ProcessPathPartWithAllProperties(tempContext, pathStep);
                       
                    }
                    else
                    {
                        if (attributeName != "//*")
                            throw new ApplicationException("anyAttribute must be //*, but it's " + attributeName);

                        if (pathStep.ArchetypeNodeId.StartsWith("openehr", StringComparison.InvariantCultureIgnoreCase)
                            || !string.IsNullOrEmpty(pathStep.NodePattern))
                        {
                            AssertionContext archetypesContext = ProcessPathPartWithWildcardForArId(tempContext, pathStep);
                            if (archetypesContext == null)
                                return null;

                            tempContext = ProcessPathPartWithAttrObject(archetypesContext, pathStep);
                        }
                        else

                            throw new NotSupportedException("//*[none archetypeId] path not supported.");

                    }
                }

                if (tempContext == null)
                    return null;
            }

            return tempContext;
        }

        private AssertionContext ProcessPathPartWithAttrObject(AssertionContext attributeObjContext, PathStep pathStep)
        {
            DesignByContract.Check.Require(attributeObjContext != null && attributeObjContext.Data != null,
                    "attributeObjContext and attributeObjContext.Data must not be null.");

            AssertionContext tempContext = attributeObjContext;

            if (pathStep.Predicates != null)
            {
                foreach (PredicateExpr predicate in pathStep.Predicates)
                {
                    AssertionContext predicateObj = predicate.Evaluate(tempContext);

                    if (predicateObj == null)
                        return null;

                    tempContext = predicateObj;
                }
            }

            return tempContext;
        }

        private AssertionContext ProcessPathPartWithAllProperties(AssertionContext rootObjContext, PathStep pathStep)
        {
            DesignByContract.Check.Require(rootObjContext != null && rootObjContext.Data != null,
               "attributeObjContext and attributeObjContext.Data must not be null.");

            object rootObj = rootObjContext.Data;
           
            AssumedTypes.List<object> objList = new OpenEhr.AssumedTypes.List<object>();

            // go through all properties
            System.Reflection.PropertyInfo[] allProperties = rootObj.GetType().GetProperties(System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.DeclaredOnly
                | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance);
            foreach (System.Reflection.PropertyInfo property in allProperties)
            {
                object propertyValue = property.GetValue(rootObj, null);
                if (propertyValue != null)
                {
                    AssertionContext propertyContext = new AssertionContext(propertyValue, rootObjContext);
                    AssertionContext tempContext = ProcessPathPartWithAttrObject(propertyContext, pathStep);
                    if (tempContext != null && tempContext.Data != null)
                    {
                        objList.Add(tempContext.Data);
                    }
                }
            }

            if (objList.Count == 0)
                return null;
            if (objList.Count == 1)
                return new AssertionContext(objList[0], rootObjContext);

            return new AssertionContext(objList, rootObjContext);
        }

        private AssertionContext ProcessPathPartWithWildcardForArId(AssertionContext contextObj, PathStep pathStep)
        {
            DesignByContract.Check.Require(pathStep.Attribute == "//*", "anyAttribute value must be //*.");

            Locatable locatable = contextObj.Data as Locatable;
            if (locatable != null)
            {

                ArchetypedPathProcessor archetypePathProcessor = new ArchetypedPathProcessor(locatable);
                string archetypePathWithWildcardKey = null;
                if (!string.IsNullOrEmpty(pathStep.ArchetypeNodeId))
                    archetypePathWithWildcardKey = pathStep.Attribute + "[" + pathStep.ArchetypeNodeId + "]";
                else if (!string.IsNullOrEmpty(pathStep.NodePattern))
                    archetypePathWithWildcardKey = pathStep.Attribute + "[{/" + pathStep.NodePattern + "/}]";
                else
                    throw new NotSupportedException(pathStep.Value+" path not supported");
                object obj = null;
                if (!archetypePathProcessor.PathExists(archetypePathWithWildcardKey))
                    return null;

                if (archetypePathProcessor.PathUnique(archetypePathWithWildcardKey))
                    obj = archetypePathProcessor.ItemAtPath(archetypePathWithWildcardKey);
                else
                    obj = archetypePathProcessor.ItemsAtPath(archetypePathWithWildcardKey);

                if (obj == null)
                    throw new ApplicationException("obj must not be null.");

                return new AssertionContext(obj, contextObj);
            }

            AssumedTypes.IList ilist = contextObj.Data as AssumedTypes.IList;
            if (ilist == null)
                throw new ApplicationException("only support either locatable or ilist");
            AssumedTypes.List<object> results = new OpenEhr.AssumedTypes.List<object>();
            foreach (Locatable locatableItem in ilist)
            {
                AssertionContext assertionContext = new AssertionContext(locatableItem, contextObj);
                AssertionContext result = ProcessPathPartWithWildcardForArId(assertionContext, pathStep);
                if (result != null && result.Data != null)
                    results.Add(result.Data);
            }

            if (results.Count > 0)
                return new AssertionContext(results, contextObj);

            return null;
        }


        #region GetAttributeObject visitor functions

        private object CallGetAttributeObject(string attributeName, object obj)
        {
            AttributeDictionaryPathable pathable = obj as AttributeDictionaryPathable;
            if (pathable != null)
                return pathable.GetAttributeValue(attributeName);

            AssumedTypes.IList assumedList = obj as AssumedTypes.IList;
            if(assumedList != null)
                return GetAttributeObject(attributeName, assumedList);

            System.Collections.IList list = obj as System.Collections.IList;
            if (list != null)
                return GetAttributeObject(attributeName, list);

            string openEhrV1AttributeName = GetOpenEhrV1AttributeName(attributeName, obj);
            System.ComponentModel.PropertyDescriptorCollection propertyDescriptorCollection = 
                System.ComponentModel.TypeDescriptor.GetProperties(obj);

            System.ComponentModel.PropertyDescriptor property =
                propertyDescriptorCollection.Find(openEhrV1AttributeName, true);

            if (property == null)
                property = propertyDescriptorCollection.Find(attributeName, false);

            if (property == null)
                return null;

            object attributeObj = property.GetValue(obj);

            return attributeObj;

        }

        private static string GetOpenEhrV1AttributeName(string attributeName, object obj)
        {
            if ((attributeName == "ehrid" || attributeName == "ehruid") && obj.GetType() == typeof(OpenEhr.RM.Support.Identification.HierObjectId))
            {
                attributeName = attributeName.Replace("_", "");
            }
            else if (attributeName == "precedingversionuid")
                attributeName = "precedingversionid";

            return attributeName.Replace("_", "");
        }

        private object GetAttributeObject(string attributeName, System.Collections.IList list)
        {
            if (list == null || list.Count == 0)
                return null;

            System.Collections.Generic.List<object> attrObjects 
                = new System.Collections.Generic.List<object>();
            foreach (object obj in list)
            {
                object attributeObj = CallGetAttributeObject(attributeName, obj);
                if (attributeObj != null)
                    attrObjects.Add(attributeObj);
            }

            if (attrObjects.Count == 1)
                return attrObjects[0];

            if (attrObjects.Count > 1)
                return attrObjects;

            return null;
        }

        private object GetAttributeObject(string attributeName, AssumedTypes.IList list)
        {
            if (list == null || list.Count == 0)
                return null;

            AssumedTypes.List<object> attrObjects = new OpenEhr.AssumedTypes.List<object>();
            foreach (object obj in list)
            {
                object attributeObj = CallGetAttributeObject(attributeName, obj);
                if (attributeObj != null)
                    attrObjects.Add(attributeObj);
            }

            if (attrObjects.Count > 1)
                return attrObjects;
            if (attrObjects.Count == 1)
                return attrObjects[0];

            return null;
        }
        #endregion
    }
}
