using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.AM.Archetype.Assertion;
using OpenEhr.Paths;
using OpenEhr.DesignByContract;
using OpenEhr.AssumedTypes.Impl;

namespace OpenEhr.Paths
{
    public class Path
    {
        const string attribute = @"((?<anyAttribute>(//\*)|(/\*))|/?(?<attribute>[a-z_]+))";
        const string pathPartAttribute = @"((?<pathPartAttribute>(//\*)|(/\*))|/?(?<pathPartAttribute>[a-z_]+))";
        const string nodeId = @"(?<node_id>at(\d{4}|((\d{4}|0)(\.0)*(\.[1-9]\d*)+)))";
        const string archetypeId = @"(?<archetype_id>[a-zA-Z]+\-[a-zA-Z]+\-[a-zA-Z_]+\.[a-zA-Z0-9_]+(\-[a-zA-Z0-9_]+)*\.v\d+(\.\d+)*)";
        const string archetypeNodeId = @"(?<archetype_node_id>" + nodeId + "|" + archetypeId + ")";
        const string position = @"(?<position>\d+)";
        const string regexPatternInPath = @"(\{/(?<regex_pattern>.*)/\})";
        const string predicatePath = @"(?<predicate_path>((/|//)?([a-z_]+))+)";
        const string standardOperator = @"(?<predicate_path_operator>=|!=|>|<|>=|<=)";
        const string stringCriteria = "\\'(?<stringCriteria>(\\s*\\w*[.,;:|&()'\"/\\\\\\*#+\\$?%-]*)+)\\'";
        const string intCriteria = @"(?<intCriteria>\d+)";
         const string doubleCriteria = @"(?<doubleCriteria>[0-9]+[0-9.]*)";
        const string boolCriteria = @"(?<boolCriteria>True|TRUE|true|false|False|FALSE)";
        const string criteria = "(?<predicate_criteria>" + stringCriteria + "|" + intCriteria + "|" + boolCriteria + 
            "|" + doubleCriteria + ")";
        const string space = @"\s*";       
        const string matchesExp = @"(?<matchesExpr>((" + predicatePath + @"\s+matches\s+)?" + regexPatternInPath + "))";
        const string shortCutNameExpr = @"(?<shortCutName>"+stringCriteria+")";
        const string nonStandardExpr = archetypeNodeId + "|" + position + "|" + shortCutNameExpr;
        const string boolOperator = @"(?<predicate_bool_operator>and|And|AND|OR|or|Or|not|Not|NOT|,)";

        private static string pathPattern;
        private static string PathPattern
        {
            get
            {
                if (string.IsNullOrEmpty(pathPattern))
                    pathPattern = GetPathPattern();
                return pathPattern;
            }
        }

        private static string pathPartPattern;
        private static string PathPartPattern
        {
            get
            {
                if (string.IsNullOrEmpty(pathPartPattern))
                    pathPattern = GetPathPattern();
                if (string.IsNullOrEmpty(pathPartPattern))
                    throw new ApplicationException("pathPartPattern must not be null or empty.");
                return pathPartPattern;
            }
        }

        static string predicatePattern;
        private static string PredicatePattern
        {
            get
            {
                if (string.IsNullOrEmpty(predicatePattern))
                    pathPattern = GetPathPattern();

                return predicatePattern;
            }
        }

        private static string GetPathPattern()
        {
            string standardPredicateExpr = @"(?<predicate_expre>(" + predicatePath + space + standardOperator + space + criteria + "))";

            string rightSimpleExpr = @"(?<rightSimpleExpr>" + standardPredicateExpr + "|" + matchesExp + "|" + nonStandardExpr + @")";
            string leftSimpleExpr = @"(?<leftSimpleExpr>" + standardPredicateExpr + "|" + matchesExp + "|" + nonStandardExpr + @")";

            string boolExpr = @"(?<genericExpr>(" + leftSimpleExpr + @"(\s*" + boolOperator + @"\s*" + rightSimpleExpr + ")*))";

            predicatePattern = @"\[(?<predicate>(" + boolExpr + @"))\]";

            pathPartPattern = @"(?<path_part>" + attribute + "(" + predicatePattern + @")*)"; // supports multiple predicate

            string pathPattern = @"(" + pathPartPattern + @")+";
            
            string standardLeftOperand = "(?<fullPath>" + pathPattern + ")";
            standardPredicateExpr = @"(?<predicate_expre>(" + standardLeftOperand + space + standardOperator + space + criteria + "))";
            rightSimpleExpr = @"(?<rightSimpleExpr>" + standardPredicateExpr + "|" + matchesExp + "|" + nonStandardExpr + @")";
            leftSimpleExpr = @"(?<leftSimpleExpr>" + standardPredicateExpr + "|" + matchesExp + "|" + nonStandardExpr + @")";

            boolExpr = @"(?<genericExpr>(" + leftSimpleExpr + @"(\s*" + boolOperator + @"\s*" + rightSimpleExpr + ")*))";

            predicatePattern = @"\[(?<predicate>(" + boolExpr + @"))\]";

            pathPartPattern = @"(?<path_part>(" + pathPartAttribute + ")(" + predicatePattern + @")*)"; // supports multiple predicate

            pathPattern = @"^(" + pathPartPattern + @")+$";

            return pathPattern;

        }

        private string path;

        private int currentIndex;
        private List<PathStep> pathSteps;

        /// <summary>
        /// Is path valid against path syntax
        /// </summary>
        /// <param name="pathString"></param>
        /// <returns></returns>
        public static bool IsValidPath(string pathString)
        {
            return Regex.IsMatch(pathString, PathPattern, RegexOptions.Compiled | RegexOptions.Singleline);
        }

        /// <summary>
        /// Path constructor
        /// </summary>
        /// <param name="path"></param>
        public Path(string path)
        {
            this.Value = path;
        }

        private PathExpr pathExpr;

        /// <summary>
        /// Path value
        /// </summary>
        public string Value
        {
            get { return this.path; }
            set
            {
                DesignByContract.Check.Require(!string.IsNullOrEmpty(value), "value must not be empty or null");

                this.path = value;
                if (pathExprCache.ContainsKey(value))
                    this.pathExpr = pathExprCache[value] as PathExpr;
                else
                {
                    Check.Require(IsValidPath(value), "value is not valid path " + value);
                    this.pathExpr = ToPathExpr(value);
                    pathExprCache.Add(value, this.pathExpr);
                }
                this.pathSteps = pathExpr.PathSteps;
            }
        }

        public PathStep Current
        {
            get { return pathSteps[currentIndex]; }
        }

        public bool IsCurrentTerminal
        {
            get
            {
                return pathSteps[currentIndex].IsTerminal;
            }
        }

        public bool IsCurrentWildcard
        {
            get { return this.CurrentAttribute.Contains("*"); }
        }

        public bool IsCurrentIdentified
        {
            get
            {
                return pathSteps[currentIndex].IsIdentified;
            }
        }

        public string CurrentAttribute
        {
            get
            {
                return pathSteps[currentIndex].Attribute;
            }
        }

        public string CurrentArchetypeNodeId
        {
            get
            {
                return pathSteps[currentIndex].ArchetypeNodeId;
            }
        }

        public string CurrentNodeId
        {
            get
            {
                return pathSteps[currentIndex].NodeId;
            }
        }

        public string CurrentNameValue
        {
            get
            {
               return pathSteps[currentIndex].NameValue;
            }
        }

        public bool NextStep()
        {
            bool result = false;

            if (!pathSteps[currentIndex].IsTerminal)
            {
                this.currentIndex++;
                result = true;
            }

            DesignByContract.Check.Invariant(currentIndex < pathSteps.Count, "current index must not exceed pathSteps count");
            DesignByContract.Check.Invariant(currentIndex >= 0, "current index must not be less than zero");

            return result;
        }

        public bool PrecedingStep()
        {
            bool result = false;
            if (this.currentIndex > 0)
            {
                this.currentIndex--;
                result = true;
            }

            DesignByContract.Check.Invariant(currentIndex < pathSteps.Count, "current index must not exceed pathSteps count");
            DesignByContract.Check.Invariant(currentIndex >= 0, "current index must not be less than zero");

            return result;
        }

        public int CurrentIndex
        {
            get
            {
                return this.currentIndex;
            }
            set
            {
                DesignByContract.Check.Require(value >= 0, "index must not be less than zero");
                DesignByContract.Check.Require(value < pathSteps.Count, "index must be less count of path steps");

                this.currentIndex = value;

                DesignByContract.Check.Invariant(currentIndex < pathSteps.Count, "current index must not exceed pathSteps count");
                DesignByContract.Check.Invariant(currentIndex >= 0, "current index must not be less than zero");
            }
        }

        public void MoveLast()
        {
            DesignByContract.Check.Require(pathSteps.Count > 0, "Path steps count must be greater than zero");

            this.CurrentIndex = pathSteps.Count - 1;
        }

        public override string ToString()
        {
            return this.Value;
        }

        #region internal functions to generate full PathExpr tree

        static System.Collections.Hashtable pathExprCache = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());

        private static PathExpr ToPathExpr(string pathString)
        {
            MatchCollection matchCollection = Regex.Matches(pathString, PathPartPattern, RegexOptions.Compiled | RegexOptions.Singleline);

            List<PathStep> pathSteps = new List<PathStep>();
            PathStep precedingStep = null;
            foreach (Match stepMatch in matchCollection)
            {
                List<PredicateExpr> predicateExprs = null;
                CaptureCollection predicateCaptures = stepMatch.Groups["predicate"].Captures;
                foreach (Capture capture in predicateCaptures)
                {
                    if (predicateExprs == null)
                        predicateExprs = new List<PredicateExpr>();

                    string predicateExprString = capture.Value;
                    ExprOperator predicate = ToExprOperator(predicateExprString);
                    PredicateExpr predicateExpr = new PredicateExpr(predicate);
                    predicateExprs.Add(predicateExpr);
                }

                PathStep thisStep = new PathStep(stepMatch, precedingStep, predicateExprs);
                pathSteps.Add(thisStep);
                precedingStep = thisStep;
            }

            PathExpr pathExpr = new PathExpr(pathString, pathSteps);

            return pathExpr;
        }

        internal static ExprOperator ToExprOperator(string predicateExpr)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(predicateExpr), "predicateExpr must not be null or empty.");

            predicateExpr = "[" + predicateExpr + "]";

            Match match = Regex.Match(predicateExpr, predicatePattern, RegexOptions.Compiled | RegexOptions.Singleline);
            string boolOperator = match.Groups["predicate_bool_operator"].Value;

            if (!string.IsNullOrEmpty(boolOperator))
                return ProcessBoolExpr(match);

            return ProcessSimpleExpr(match);
        }

        private static ExprBinaryOperator ProcessBoolExpr(Match match)
        {
            string boolExprString = match.Groups["genericExpr"].Value;
            if (string.IsNullOrEmpty(boolExprString))
                throw new ApplicationException("boolExprString must not be null or empty.");

            ExprBinaryOperator boolExpr = null;
            string leftOperandString = match.Groups["leftSimpleExpr"].Value;
            if (string.IsNullOrEmpty(leftOperandString))
                throw new ApplicationException("leftOperandString must not be null or empty.");

            ExprOperator leftOperand = ToExprOperator(leftOperandString);

            do
            {
                if (boolExpr != null)
                    leftOperand = boolExpr;

                string boolOperatorStr = match.Groups["predicate_bool_operator"].Value;
                OperatorKind boolOperator = new OperatorKind(OperatorKind.GetOperatorKind(boolOperatorStr));
                string rightOperandStr = match.Groups["rightSimpleExpr"].Value;
                if (string.IsNullOrEmpty(rightOperandStr))
                    throw new ApplicationException("rightOperandStr must not be null or empty.");

                ExprOperator rightOperand = ToExprOperator(rightOperandStr);
                boolExpr = new ExprBinaryOperator(leftOperand, rightOperand, "Boolean", boolOperator, false);

                match = match.NextMatch();
            } while (match.Success);

            DesignByContract.Check.Ensure(boolExpr != null, "boolExpr must not be null.");

            return boolExpr;
        }

        private static ExprBinaryOperator ProcessSimpleExpr(Match match)
        {
            string simpleExprString = match.Groups["genericExpr"].Value;

            if (string.IsNullOrEmpty(simpleExprString))
                throw new ApplicationException("simpleExprString must not be null or empty.");

            string standardExpr = match.Groups["predicate_expre"].Value;
            if (!string.IsNullOrEmpty(standardExpr))
            {
                string fullPath = match.Groups["fullPath"].Value;
                if (!string.IsNullOrEmpty(fullPath))
                    return ProcessPredicateWithFullPath(match);

                throw new ApplicationException("fullPath must not be null or empty when predicate_expre is not null and not empty.");
            }
            else
            {
                string archetypeNodeId = match.Groups["archetype_node_id"].Value;
                if (!string.IsNullOrEmpty(archetypeNodeId))
                {
                    return ProcessArchetypeNodeIdExpr(match);
                }
                else
                {
                    string shortCutNameExpr = match.Groups["shortCutName"].Value;
                    if (!string.IsNullOrEmpty(shortCutNameExpr))
                    {
                        return ProcessShortCutName(match);
                    }
                    else
                    {
                        string positionString = match.Groups["position"].Value;
                        if (!string.IsNullOrEmpty(positionString))
                        {
                            return ProcessPositionExpr(match);
                        }
                        else
                        {
                            string matches = match.Groups["matchesExpr"].Value;
                            if (!string.IsNullOrEmpty(matches))
                            {
                                return ProcessMatchesExpr(match);
                            }
                        }
                    }
                }
            }
            throw new ApplicationException("predicate scenarios are not supported: "+match.Value);
        }

        private static ExprBinaryOperator ProcessStandardPredicateExpr(Match match)
        {
            string standardExpr = match.Groups["predicate_expre"].Value;
            if (string.IsNullOrEmpty(standardExpr))
                throw new ApplicationException("standardExpr must not be null or empty.");

            string path = match.Groups["predicate_path"].Value;
            if (string.IsNullOrEmpty(path))
                throw new ApplicationException("predicate_path must not be null or empty");

            ExprLeaf lefOperand = new ExprLeaf(path, "String", "Path");
            string standardExprOperator = match.Groups["predicate_path_operator"].Value;
            OperatorKind operatorKind = new OperatorKind(OperatorKind.GetOperatorKind(standardExprOperator));

            // default value
            object criteriaValue = null;
            string type = GetCriteriaType(match, out criteriaValue);
            ExprLeaf rightOperand = new ExprLeaf(criteriaValue, type, "constraint");
            ExprBinaryOperator binaryOperator = new ExprBinaryOperator(lefOperand, rightOperand, type, operatorKind, false);

            return binaryOperator;
        }

        private static string GetCriteriaType(Match match, out object criteria)
        {
            string type = "String";
            string stringCriteria = match.Groups["stringCriteria"].Value;
            if (!string.IsNullOrEmpty(stringCriteria))
            {

                type = ExprItem.GetTypeValue(stringCriteria);
                criteria = stringCriteria;

                return type;
            }
            else
            {
                string intCriteria = match.Groups["intCriteria"].Value;
                if (!string.IsNullOrEmpty(intCriteria))
                {
                    type = "Integer";
                    int intValue = int.MinValue;
                    if (!int.TryParse(intCriteria, out intValue))
                        throw new ApplicationException(intValue + " is not a valid integer.");
                    criteria = intValue;

                    return type;
                }

                string doubleCriteria = match.Groups["doubleCriteria"].Value;
                if (!string.IsNullOrEmpty(doubleCriteria))
                {
                    type = "Double";
                    double doubleValue = double.MinValue;
                    if (!double.TryParse(doubleCriteria, out doubleValue))
                        throw new ApplicationException(doubleCriteria + " is not a valid double value.");
                    criteria = doubleValue;

                    return type;
                }

                string boolCriteria = match.Groups["boolCriteria"].Value;
                if (!string.IsNullOrEmpty(boolCriteria))
                {
                    type = "Boolean";
                    bool boolValue = true;
                    if (!bool.TryParse(boolCriteria.ToString(), out boolValue))
                        throw new ApplicationException(boolCriteria + " is not a valid boolean value.");
                    criteria = boolValue;
                    return type;
                }

                criteria = stringCriteria;

                return type;
            }

        }

        private static ExprBinaryOperator ProcessArchetypeNodeIdExpr(Match match)
        {
            string archetypeNodeId = match.Groups["archetype_node_id"].Value;
            if (string.IsNullOrEmpty(archetypeNodeId))
                throw new ApplicationException("archetypeNodeId must not be null or empty.");

            ExprLeaf leftOperand = new ExprLeaf("/archetype_node_id", "String", "path");
            ExprLeaf rightOperand = new ExprLeaf(archetypeNodeId, "String", "constraint");
            ExprBinaryOperator binaryOperator = new ExprBinaryOperator(leftOperand, rightOperand, "String", new OperatorKind(OperatorKind.op_eq),
                false);

            return binaryOperator;
        }

        private static ExprBinaryOperator ProcessShortCutName(Match match)
        {
            string shortCutNameExpr = match.Groups["shortCutName"].Value;
            if (string.IsNullOrEmpty(shortCutNameExpr))
                throw new ApplicationException("shortCutNameExpr must not be null or empty.");

            string nameValue = match.Groups["stringCriteria"].Value;

            ExprLeaf leftOperand = new ExprLeaf("/name/value", "String", "path");
            ExprLeaf rightOperand = new ExprLeaf(nameValue, "String", "constraint");
            ExprBinaryOperator binaryOperator = new ExprBinaryOperator(leftOperand, rightOperand, "String",
               new OperatorKind(OperatorKind.op_eq), false);

            return binaryOperator;
        }

        private static ExprBinaryOperator ProcessPositionExpr(Match match)
        {
            string positionString = match.Groups["position"].Value;
            if (string.IsNullOrEmpty(positionString))
                throw new ApplicationException("positionString must not be null or empty.");

            int position = int.Parse(positionString);
            if (position <= 0)
                throw new ApplicationException("Position specified within openehr path must not <= 0.");

            FunctionCall functionCall = new FunctionCall("position", null, null);
            ExprLeaf rightOperand = new ExprLeaf(position, "Integer", "constraint");

            ExprBinaryOperator binaryOperator = new ExprBinaryOperator(functionCall, rightOperand,
                rightOperand.Type, new OperatorKind(OperatorKind.op_eq), false);

            return binaryOperator;
        }

        private static ExprBinaryOperator ProcessPredicateWithFullPath(Match match)
        {
            string fullPath = match.Groups["fullPath"].Value;
            if (string.IsNullOrEmpty(fullPath))
                throw new ApplicationException("fullPath must not be null or empty.");

            PathExpr pathExpr = ToPathExpr(fullPath);
            string standardExprOperator = match.Groups["predicate_path_operator"].Value;
            OperatorKind operatorKind = new OperatorKind(OperatorKind.GetOperatorKind(standardExprOperator));

            string predicateCriteria = match.Groups["predicate_criteria"].Value;
            if (string.IsNullOrEmpty(predicateCriteria))
                throw new ApplicationException("predicateCriteria must not be null or empty.");

            object criteriaValue = null;
            string type = GetCriteriaType(match, out criteriaValue);
            ExprLeaf rightOperand = new ExprLeaf(criteriaValue, type, "constraint");
            ExprBinaryOperator binaryOperator = new ExprBinaryOperator(pathExpr, rightOperand, type, operatorKind, false);
            return binaryOperator;
        }

        private static ExprBinaryOperator ProcessMatchesExpr(Match match)
        {
            string matches = match.Groups["matchesExpr"].Value;
            if (string.IsNullOrEmpty(matches))
                throw new ApplicationException("matches must not be null or empty.");

            string leftPath = match.Groups["predicate_path"].Value;
            ExprLeaf leftOperand = null;
            if (string.IsNullOrEmpty(leftPath))
                leftOperand = new ExprLeaf("/archetype_node_id", "String", "path");
            else
            {
                leftOperand = new ExprLeaf(leftPath, "String", "path");
            }
            OperatorKind operatorKind = new OperatorKind(OperatorKind.op_matches);

            string pattern = match.Groups["regex_pattern"].Value;
            if (string.IsNullOrEmpty(pattern))
                throw new ApplicationException("pattern must not be null or empty.");

            ExprLeaf rightOperand = new ExprLeaf(pattern, "String", "pattern");

            ExprBinaryOperator binaryOperator = new ExprBinaryOperator(leftOperand, rightOperand, rightOperand.Type, operatorKind, false);
            return binaryOperator;
        }
        #endregion

        public object ItemAtPath(object obj)
        {
            object itemAtPath = this.pathExpr.Evaluate(obj);
            AssumedTypes.IList iList = itemAtPath as AssumedTypes.IList;

            // if itemAtPath is not a list, return it.
            if (iList == null)
                return itemAtPath;

            //// if itemAtPath is a list, generate LocatableList when the items are locatable.
            //// otherwise return the list
            LocatableList<Locatable> locatableList = null;


            AssumedTypes.List<object> items = iList as AssumedTypes.List<object>;
            if(items != null)
                return items;
            else
                items = new OpenEhr.AssumedTypes.List<object>();

            foreach (object o in iList)
                items.Add(o);

            return items;
        }        
    }

    public class PathStep
    {
        const string defaultNamePath = "/name/value";
        const string defaultPredicateOperator = "=";
        const string defaultPredicateBoolOperator = "and";

        private Match stepMatch;
        private PathStep precedingStep;

        internal PathStep(Match stepMatch, PathStep precedingStep)
        {
            this.stepMatch = stepMatch;
            this.precedingStep = precedingStep;
        }

        internal PathStep(Match stepMatch, PathStep precedingStep, List<PredicateExpr> predicates)
        {
            this.stepMatch = stepMatch;
            this.precedingStep = precedingStep;
            this.predicates = predicates;
        }

        private List<PredicateExpr> predicates;

        internal List<PredicateExpr> Predicates
        {
            get { return predicates; }
            set { predicates = value; }
        }


        public string Value
        {
            get { return this.stepMatch.Value; }
        }

        public string Path
        {
            get
            {
                if (precedingStep != null)
                    return precedingStep.Path + this.Value;
                else
                    return this.Value;
            }
        }

        public PathStep PrecedingStep
        {
            get { return this.precedingStep; }
        }

        public bool IsTerminal
        {
            get { return (!stepMatch.NextMatch().Success); }
        }

        public bool IsWildcard
        {
            get { return this.Attribute == "*"; }
        }

        public bool HasName
        {
            get { return (this.HasNameCode || this.HasNameValue); }
        }

        public bool HasPredicatePath
        {
            get
            {
                return !string.IsNullOrEmpty(this.PredicatePath) || !string.IsNullOrEmpty(this.PredicateCriteria);
            }
        }

        public bool HasPredicateBoolOperator
        {
            get
            {
                return !string.IsNullOrEmpty(stepMatch.Groups["predicate_bool_operator"].Value);
            }
        }

        public bool HasNameValue
        {
            // %HYYKA%
            //get { return !string.IsNullOrEmpty(stepMatch.Groups["node_name"].Value); }
            // CM: 25/03/09
            //get { return !string.IsNullOrEmpty(stepMatch.Groups["name_value"].Value); }
            get
            {
                return !string.IsNullOrEmpty(this.NameValue);
            }
        }

        public bool HasNameCode
        {
            // %HYYKA%
            // CM: 25/03/09
            //get { return !string.IsNullOrEmpty(stepMatch.Groups["name_code"].Value); }
            get
            {
                return !string.IsNullOrEmpty(this.NameCode);
            }
        }

        public bool IsIdentified
        {
            get
            {
                return !string.IsNullOrEmpty(stepMatch.Groups["archetype_node_id"].Value);
            }
        }

        public string Attribute
        {
            get { return stepMatch.Groups["pathPartAttribute"].Value; }
        }     

        public string ArchetypeNodeId
        {
            get { return stepMatch.Groups["archetype_node_id"].Value; }
        }

        public string NodeId
        {
            get { return stepMatch.Groups["node_id"].Value; }
        }

        public string NodePattern
        {
            get { return stepMatch.Groups["regex_pattern"].Value; }
        }

        public string ArchetypeId
        {
            get { return stepMatch.Groups["archetype_id"].Value; }
        }

        public bool HasNodeIdPattern
        {
            get { return !string.IsNullOrEmpty(this.NodePattern); }
        }

        public bool IsArchetyped
        {
            get { return !string.IsNullOrEmpty(this.ArchetypeId); }
        }

        bool nameValueSet;
        string nameValue;
        public string NameValue
        {
            get
            {
                // %HYYKA%
                //// CM: 25/03/09
                //// the path regex pattern has been changed to a generic pattern. Predicate contains archetype id/archetype node Id,
                //// predicate path, and a predicate criteria. When the predicate path is name/value or predicate path
                //// is empty, then predicate_criteria must be name value
                //if (string.IsNullOrEmpty(this.PredicatePath) || this.PredicatePath == "/name/value")
                //    return this.PredicateCriteria;
                
                //return null;
                //// return stepMatch.Groups["name_value"].Value; 

                if (!nameValueSet)
                {
                    nameValue = GetNameValueCriteria();
                    nameValueSet = true;
                }
                return nameValue;
            }
        }
      
        private string GetNameValueCriteria()
        {
            if (this.Predicates == null || this.Predicates.Count == 0)
                return null;
            foreach (PredicateExpr predicateExpr in this.Predicates)
            {
                ExprOperator predicate = predicateExpr.Predicate;
                if (predicate == null)
                    throw new ApplicationException("predicate must not be null.");

                string nameValue = GetValueCriteriaByPath(predicate, "name/value");
                if (nameValue != null)
                    return nameValue;
            }

            return null;
        }

        private string GetNameCodeCriteria()
        {
            if (this.Predicates == null || this.Predicates.Count == 0)
                return null;
            foreach (PredicateExpr predicateExpr in this.Predicates)
            {
                ExprOperator predicate = predicateExpr.Predicate;
                if (predicate == null)
                    throw new ApplicationException("predicate must not be null.");

                string nameValue = GetValueCriteriaByPath(predicate, "name/defining_code/code_string");
                if (nameValue != null)
                    return nameValue;
            }

            return null;
        }

        internal string GetValueCriteriaByPath(ExprItem exprItem, string requiredPath)
        {
            ExprLeaf leaf = exprItem as ExprLeaf;
           
            ExprBinaryOperator binary = exprItem as ExprBinaryOperator;
            if (binary != null)
            {
                switch (binary.Operator.Value)
                {
                    case OperatorKind.op_eq:
                    case OperatorKind.op_ne:
                        {
                            string path = GetPredicateFullPath(binary.LeftOperand);
                            if (path != null && path.EndsWith(requiredPath, StringComparison.InvariantCultureIgnoreCase))
                            {
                                ExprLeaf rightOperand = binary.RightOperand as ExprLeaf;
                                if (rightOperand == null)
                                    throw new ApplicationException("rightOperand must be typeof ExprLeaf when the leftOperand is name/value path.");

                                if (!rightOperand.ReferenceType.Equals("constraint"))
                                    throw new NotSupportedException(rightOperand.ReferenceType + "rightOperand.ReferenceType is not supported.");

                                return rightOperand.Item.ToString();
                            }
                            return null;
                        }
                    case OperatorKind.op_or:
                    case OperatorKind.op_and:
                    case OperatorKind.op_not:
                        ExprItem left= binary.LeftOperand;
                        ExprItem right = binary.RightOperand;
                        string nameValue = GetValueCriteriaByPath(right, requiredPath);
                        if (nameValue != null)
                            return nameValue;
                        nameValue = GetValueCriteriaByPath(left, requiredPath);
                        return nameValue;

                    default:
                        return null;
                }

            }

            return null;
        }

        internal string GetPredicateFullPath(ExprItem exprItem)
        {
            ExprLeaf leaf = exprItem as ExprLeaf;
            if (leaf != null)
            {
                if (leaf.ReferenceType.IndexOf("path", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    return leaf.Item.ToString();
                }
            }           

            ExprUnaryOperator unarayOperand = exprItem as ExprUnaryOperator;
            if (unarayOperand != null)
            {
                ExprItem operand = unarayOperand.Operand;
                return GetPredicateFullPath(operand);
            }

            PathExpr pathExpr = exprItem as PathExpr;
            if (pathExpr != null)
            {
                return pathExpr.Path;
            }

            return null;
        }

        internal string PredicateFullPath
        {
            get
            {
                return stepMatch.Groups["fullPath"].Value;
            }
        }

        public string PredicatePath
        {
            get
            {
                string predicatePath = stepMatch.Groups["fullPath"].Value;
                if (!string.IsNullOrEmpty(predicatePath) &&!predicatePath.StartsWith("/"))
                    return "/" + predicatePath;

                if (string.IsNullOrEmpty(predicatePath) && !string.IsNullOrEmpty(this.PredicateCriteria))
                    return defaultNamePath;

                return predicatePath;
            }
        }

        public string PredicateCriteria
        {
            get {
                if (!string.IsNullOrEmpty(stepMatch.Groups["stringCriteria"].Value))
                    return stepMatch.Groups["stringCriteria"].Value;

                return stepMatch.Groups["predicate_criteria"].Value;
            }
        }

        public string PredicateOperator
        {
            get
            {
                string predicateOperator = stepMatch.Groups["predicate_path_operator"].Value;

                if (string.IsNullOrEmpty(predicateOperator) && this.HasPredicatePath)
                    return defaultPredicateOperator;

                return predicateOperator;
            }
        }

        public string PredicateBoolOperator
        {
            get {
                string boolOperator= stepMatch.Groups["predicate_bool_operator"].Value;
                if (boolOperator == ",")
                    boolOperator = defaultPredicateBoolOperator;

                return boolOperator;
            }
        }

        public string PredicateRightExpr
        {
            get
            {
                DesignByContract.Check.Require(this.HasPredicateBoolOperator, "HasPredicateBooleanOperator must be true.");
                DesignByContract.Check.Require(stepMatch.Groups["predicate_expre"].Captures.Count == 2,
                    "must have two predicate_expr capture when HasPredicateBooleanOperator is true.");

                string rightExpr = stepMatch.Groups["predicate_expre"].Captures[1].Value;

                if (string.IsNullOrEmpty(rightExpr))
                    throw new ApplicationException("right predicate boolean expression must not be null or empty.");

                return rightExpr;

            }
        }

        public string PredicateLeftExpr
        {
            get
            {
                DesignByContract.Check.Require(this.HasPredicateBoolOperator, "HasPredicateBooleanOperator must be true.");
                DesignByContract.Check.Require(stepMatch.Groups["predicate_expre"].Captures.Count == 2,
                    "must have two predicate_expr capture when HasPredicateBooleanOperator is true.");

                string leftExpr = stepMatch.Groups["predicate_expre"].Captures[0].Value;

                if (string.IsNullOrEmpty(leftExpr))
                    throw new ApplicationException("left predicate boolean expression must not be null or empty.");

                return leftExpr;
            }
        }

        bool nameCodeSet = false;
        string nameCode;
        public string NameCode
        {
            get
            {
                // %HYYKA%
                // CM: 25/03/09
                // the path regex pattern has been changed to a generic pattern. Predicate contains archetype id/archetype node Id,
                // predicate path, and a predicate criteria. When the predicate path is name/defining_code/code_string, 
                // then predicate_criteria must be name code value

                //return stepMatch.Groups["name_code"].Value; 
               //if (this.PredicatePath == "/name/defining_code/code_string")
               //     return this.PredicateCriteria;

               // return null;
                if (!nameCodeSet)
                {
                    nameCode = GetNameCodeCriteria();
                    nameCodeSet = true;
                }
                return nameCode;
            }
        }

        public bool HasPredicate
        {
            get
            {
                return !string.IsNullOrEmpty(stepMatch.Groups["predicate"].Value);
            }
        }

        internal string SimplePredicateExpr
        {
            get
            {
               return stepMatch.Groups["simpleExpr"].Value;
            }
        }

        internal string BoolPredicateExpr
        {
            get
            {
                return stepMatch.Groups["booleanExpr"].Value;
            }
        }

        internal bool IsBooleanExpr
        {
            get
            {
                return !string.IsNullOrEmpty(this.BoolPredicateExpr);
            }
        }    
    }
}
