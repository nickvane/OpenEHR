using System;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.Archetype.Assertion
{
    /// <summary>
    /// Structural model of a typed first order predicate logic assertion, in the form of an
    /// expression tree, including optional variable definitions.
    /// </summary>
    [AmType("ASSERTION")]
    [Serializable]
    public class Assertion
    {
        #region Constructors
        /// <summary>
        /// default constructor
        /// </summary>
        public Assertion() { }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="expresion">Root of expression tree.</param>
        public Assertion(ExprItem expresion)
        {
            this.Expression = expresion;
        }
        #endregion

        #region Class properties

        private string tag;
        /// <summary>
        /// Get or set expression tag, used for differentiating multiple assertions.
        /// </summary>
        public string Tag
        {
            get { return tag; }
            set
            {
                DesignByContract.Check.Require(value == null || value != string.Empty,
                    string.Format(CommonStrings.IfXIsNotNullMustBeEmpty, "Tag value"));
                tag = value;
            }
        }

        private ExprItem expression;

        /// <summary>
        /// Get or set root of expression tree.
        /// </summary>
        public ExprItem Expression
        {
            get { return expression; }
            set
            {
                DesignByContract.Check.Require(value != null, 
                    string.Format(CommonStrings.XMustNotBeNull, "Expression value"));
                DesignByContract.Check.Require(value.Type.Equals("BOOLEAN", StringComparison.CurrentCultureIgnoreCase),
                    AmValidationStrings.ExpressionTypeNotBoolean);

                expression = value;
            }
        }

        private string stringExpression;

        /// <summary>
        /// Get or set string form of expression, in case an expression evaluator taking String expressions is
        /// used for evaluation.
        /// </summary>
        public string StringExpression
        {
            get { return stringExpression; }
            set { stringExpression = value; }
        }

        private AssumedTypes.List<AssertionVariable> variables;

        /// <summary>
        /// Get or set definitions of variables used in the assertion expression.
        /// </summary>
        public AssumedTypes.List<AssertionVariable> Variables
        {
            get { return variables; }
            set { variables = value; }
        }

        #endregion

    }
}