using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.AM.Archetype.Assertion;

namespace OpenEhr.Paths
{
    internal class PredicateExpr
    {
       internal PredicateExpr(ExprOperator predicate)            
        {
            this.predicate = predicate;
        }

        #region class properties
        private ExprOperator predicate;

        public ExprOperator Predicate
        {
            get { return predicate; }
            set { predicate = value; }
        }
        #endregion

        #region Class functions
        internal AssertionContext Evaluate(AssertionContext contextObj)
        {
            if (contextObj == null)
                return null;

            AssumedTypes.IList assumedIList = contextObj.Data as AssumedTypes.IList;
            if (assumedIList != null)
            {
                AssumedTypes.List<object> results = new OpenEhr.AssumedTypes.List<object>();
                foreach (object obj in assumedIList)
                {
                    AssertionContext assertObj = new AssertionContext(obj, contextObj);
                    AssertionContext evaluateResult = Evaluate(assertObj);
                    if (evaluateResult != null)
                    {
                        AssumedTypes.List<object> aList = evaluateResult.Data as AssumedTypes.List<object>;
                        if (aList == null)
                            results.Add(evaluateResult.Data);
                        else
                            foreach (object o in aList)
                                results.Add(o);
                    }
                }

                if (results.Count == 0)
                    return null;

                return new AssertionContext(results, contextObj);
            }

            System.Collections.IList list = contextObj.Data as System.Collections.IList;
            if (list == null)
            {
                return EvaluateSingleAttri(contextObj);
            }
            else
            {
                AssumedTypes.List<object> results = new AssumedTypes.List<object>();
                foreach (object obj in list)
                {
                    AssertionContext assertObj = new AssertionContext(obj, contextObj);
                    AssertionContext evaluateResult = Evaluate(assertObj);
                    if (evaluateResult != null)
                        results.Add(evaluateResult.Data);
                }

                if (results.Count == 0)
                    return null;

                return new AssertionContext(results, contextObj);
            }
        }

        internal AssertionContext EvaluateSingleAttri(AssertionContext contextObj)
        {
            System.Collections.IList list = contextObj.Data as System.Collections.IList;
            if (list != null)
                throw new ApplicationException("contextObj.Data must not be a list.");
           
            AssertionContext obj = this.predicate.Evaluate(contextObj);

            bool boolValue = false;
            if (bool.TryParse(obj.Data.ToString(), out boolValue))
            {
                if (boolValue == true)
                    return contextObj;
                return null;
            }

            throw new ApplicationException("obj must be type of boolean value.");
        }
        #endregion
    }
}
