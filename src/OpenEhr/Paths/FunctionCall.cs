using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.AM.Archetype.Assertion;

namespace OpenEhr.Paths
{
    internal class FunctionCall : AM.Archetype.Assertion.ExprItem
    {
        internal FunctionCall(string functionName, string returnType, ExprItem[] arglist)
            : base("function")
        {
            this.functionName = functionName;
            this.returnType = returnType;
            this.argList = argList;
        }

        #region class properties
        private string functionName;

        public string FunctionName
        {
            get { return functionName; }
            set { functionName = value; }
        }

        private string returnType;

        public string ReturnType
        {
            get { return returnType; }
            set { returnType = value; }
        }

        private ExprItem[] argList;

        public ExprItem[] ArgList
        {
            get { return argList; }
            set { argList = value; }
        }

        #endregion

        #region Class functions
        internal override AssertionContext Evaluate(AssertionContext contextObj)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(this.functionName), "functionName must not be null or empty.");

            switch (this.functionName)
            {
                case "position":
                    int position = Position(contextObj);
                    return new AssertionContext(position, null);
                default:
                    throw new NotImplementedException(this.functionName + " function is not implemented.");
            }
        }

        private int Position(AssertionContext contextObj)
        {
            DesignByContract.Check.Require(this.functionName.Equals("position", StringComparison.InvariantCultureIgnoreCase),
                "functionName must be position");

            AssertionContext parent = contextObj.Parent;
            System.Collections.IList list = parent.Data as System.Collections.IList;
            if (list != null)
            {
                return list.IndexOf(contextObj.Data);
            }

            AssumedTypes.IList assumedList = parent.Data as AssumedTypes.IList;
            if(assumedList != null)
                for (int i = 0; i < assumedList.Count; i++)
                {
                    if (contextObj.Data.Equals(assumedList[i]))
                        return i+1;
                }

            return -1;
        }
        #endregion

    }
}
