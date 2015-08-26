using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;

namespace OpenEhr.AM.Archetype.Assertion
{
    /// <summary>
    /// Abstract parent of all expression tree items.
    /// </summary>
    [Serializable]
    public abstract class ExprItem
    {
        #region Constructors        
        protected ExprItem(string type)
        {
            this.Type = type;
        }

        protected ExprItem() { }
        #endregion

        #region Class properties
        private string type;
        /// <summary>
        /// Type name of this item in the mathematical sense. For leaf nodes, must be the name of a primitive type, 
        /// or else a reference model type. The type for any relational or boolean operator will be “Boolean”, 
        /// while the type for any arithmetic operator, will be “Real” or “Integer”.
        /// </summary>
        public string Type
        {
            get { return type; }
            set
            {
                Check.Require(!string.IsNullOrEmpty(value), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Type value"));
                type = value;
            }
        }
        #endregion

        #region class functions
        /// <summary>
        /// An abstract function evaluating the obj against the ExprItem. 
        /// </summary>
        /// <param name="obj">The object to be evaluated</param>
        /// <returns>Can be an object, a list of object or boolean</returns>
        internal object Evaluate(object obj)
        {
            OpenEhr.Paths.AssertionContext contextObj = new OpenEhr.Paths.AssertionContext(obj, null);

            OpenEhr.Paths.AssertionContext returnedObject = Evaluate(contextObj);
            if (returnedObject == null)
                return null;

            object result = returnedObject.Data;
            AssumedTypes.IList iList = result as AssumedTypes.IList;
            if (iList != null && iList.Count == 1)
                return iList[0];

            return result;

        }
        internal abstract OpenEhr.Paths.AssertionContext Evaluate(OpenEhr.Paths.AssertionContext contextObj);

        internal static string GetTypeValue(string stringValue)
        {            
            if (AssumedTypes.Iso8601DateTime.ValidIso8601DateTime(stringValue))
                return "DV_DATE_TIME";
            if (AssumedTypes.Iso8601Time.ValidIso8601Time(stringValue))
                return "DV_TIME";
            if (AssumedTypes.Iso8601Duration.ValidIso8601Duration(stringValue))
                return "DV_DURATION";

            return "String";
        }
        #endregion

    }
}