using System;
using System.Text.RegularExpressions;

using OpenEhr.DesignByContract;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.AssumedTypes;
using OpenEhr.AssumedTypes.Impl;

namespace OpenEhr.Paths
{
    public class ArchetypedPathProcessor : IPathProcessor
    {
        private Locatable rootLocatable;
        private System.Collections.Generic.Dictionary<string, Locatable> pathMap;

        public ArchetypedPathProcessor(Locatable rootLocatable)
        {
            Check.Require(rootLocatable != null, "rootLocatable must not be null");

            this.rootLocatable = rootLocatable;
        }

        void BuildPathMap(Locatable locatable)
        {
            foreach(object value in locatable.GetAllAttributeValues())
            {
                ILocatableList locatableList 
                    = value as ILocatableList;
                if (locatableList != null)
                {
                    foreach (Locatable locatableValue in locatableList)
                    {
                        if (locatableValue.IsArchetypeRoot)
                        {
                            string archetypedPath = rootLocatable.PathOfItem(locatableValue);

                            // %HYYKA%
                            // CM: 16/04/09 in normal case, pathMap should not contain duplicated keys. 
                            // otherwise, the data instance is invalid. Since this is not for validation purpose, 
                            // when there are duplicated path in pathMap, not throw exception.
                            
                            //pathMap.Add(archetypedPath, locatableValue);
                            
                            if(!pathMap.ContainsKey(archetypedPath))
                               pathMap.Add(archetypedPath, locatableValue);
                        }

                        BuildPathMap(locatableValue);
                    }
                }
                else
                {
                    Locatable locatableValue = value as Locatable;
                    if (locatableValue != null)
                    {
                        if (locatableValue.IsArchetypeRoot)
                        {
                            string archetypedPath = rootLocatable.PathOfItem(locatableValue);
                            pathMap.Add(archetypedPath, locatableValue);
                        }

                        BuildPathMap(locatableValue);
                    }
                }
            }
        }

        static Regex anyArchetypeRegex = new Regex(@"^//\*\[\s*("
            + @"(?<qualified_rm_entity>\w+-\w+-\w+).(?<domain_concept>\w+(-\w+)*).(?<version_id>[vV]\d*)"
            + @"|{\s*/(?<archetypeId>[^/]*)/\s*}"
            + @")\s*\]$", RegexOptions.Compiled | RegexOptions.Singleline);

        List<object> FindMatches(string path)
        {
            if (pathMap == null)
            {
                pathMap = new System.Collections.Generic.Dictionary<string, Locatable>();
                BuildPathMap(rootLocatable);
            }

            List<object> matchList = new List<object>();

            string matchExpression = null;

            Match anyArchetypeMatch = anyArchetypeRegex.Match(path);
            if (anyArchetypeMatch.Success)
            {
                Group archetypeId = anyArchetypeMatch.Groups["archetypeId"];
                if (archetypeId.Success)
                    matchExpression = anyArchetypeMatch.Result(@"\[\s*${archetypeId}[^\]]*\]$");
                else
                    matchExpression = anyArchetypeMatch.Result(@"\[\s*${qualified_rm_entity}\.${domain_concept}\.${version_id}[^\]]*\]$");
            }

            if (matchExpression == null)
                throw new NotSupportedException("matchExpression not supported for path: " + path);

            foreach (System.Collections.Generic.KeyValuePair<string, Locatable> keyValue in pathMap)
            {
                if (Regex.IsMatch(keyValue.Key, matchExpression, RegexOptions.Compiled | RegexOptions.Singleline))
                    matchList.Add(keyValue.Value);
            }

            return matchList;
        }

        #region IPathProcessor Members

        public object ItemAtPath(string path)
        {
            OpenEhr.AssumedTypes.IList items = FindMatches(path);
            if (items.Count > 0)
            {
                if (items.Count == 1)
                    return items[0];
                else
                    throw new ApplicationException("Path is not unique: " + path);
            }
            else
                throw new ApplicationException("Path not exists: " + path);
        }

        public List<object> ItemsAtPath(string path)
        {
            List<object> items = FindMatches(path);
            if (items.Count > 0)
                return items;
            else
                throw new ApplicationException("Path not exists: " + path);
        }

        public bool PathExists(string path)
        {
            OpenEhr.AssumedTypes.IList items = FindMatches(path);
            if (items.Count > 0)
                return true;
            else
                return false;
        }

        public bool PathUnique(string path)
        {
            OpenEhr.AssumedTypes.IList items = FindMatches(path);
            if (items.Count > 0)
            {
                return (items.Count == 1);
            }
            else
                throw new ApplicationException("Path not exists: " + path);
        }

        #endregion
    }
}
