using System;
//using System.Collections.Generic;
//using System.Text;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.Archetype.Assertion
{
    [AmType("EXPR_UNARY_OPERATOR")]
    [Serializable]
    public class ExprUnaryOperator: ExprOperator
    {
        #region Constructors
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="operand"> Operand node.</param>
        /// <param name="type">Type name of this item in the mathematical sense.</param>
        /// <param name="anOperator">Code of operator.</param>
        /// <param name="precedenceOverriden">True if the natural precedence of operators is 
        /// overridden in the expression represented by this node of the expression tree.</param>
        public ExprUnaryOperator(ExprItem operand, string type, OperatorKind anOperator, bool precedenceOverriden)
            :
            base(type, anOperator, precedenceOverriden)
        {
            this.Operand = operand;
        }

        public ExprUnaryOperator() { }
        #endregion

        #region Class properties
        private ExprItem operand;

        /// <summary>
        /// Operand node.
        /// </summary>
        public ExprItem Operand
        {
            get { return operand; }
            set
            {
                DesignByContract.Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "Operand value"));
                operand = value;
            }
        }
         #endregion

        #region Class functions
        internal override OpenEhr.Paths.AssertionContext Evaluate(OpenEhr.Paths.AssertionContext contextObj)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion
       
    }
}