using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.Resources;

namespace OpenEhr.AM.Archetype.Assertion
{
    /// <summary>
    /// Abstract parent of operator types.
    /// </summary>
    [Serializable]
    public abstract class ExprOperator: ExprItem
    {
        #region Constructors
        protected ExprOperator(string type, OperatorKind anOperator, bool precedenceOverriden)
            : base(type)
        {
            this.Operator = anOperator;
            this.PrecedenceOverriden = precedenceOverriden;
        }

        protected ExprOperator() { }
        #endregion

        #region Class properties
        private OperatorKind anOperator;

        /// <summary>
        /// Code of operator
        /// </summary>
        public OperatorKind Operator
        {
            get { return anOperator; }
            set
            {
                DesignByContract.Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "Operator value"));
                anOperator = value;
            }	
        }

        private bool precedenceOverriden;
        /// <summary>
        /// True if the natural precedence of operators is overridden in the expression represented by
        /// this node of the expression tree. If True, parentheses should be introduced around the
        /// totality of the syntax expression corresponding to this operator node and its operands.
        /// </summary>
        public bool PrecedenceOverriden
        {
            get { return precedenceOverriden; }
            set { precedenceOverriden = value; }
        }
        #endregion

    }
}