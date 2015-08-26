using System;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.Archetype.Assertion
{    
    /// <summary>
    /// Enumeration type for operator types in assertion expressions.
    /// Use as the type of operators in the Assertion package, or for related uses.
    /// </summary>
    [AmType("OPERATOR_KIND")]
    [Serializable]
    public class OperatorKind
    {
        #region Constructors
        public OperatorKind(int anOperator)
        {
            this.Value = anOperator;
        }

        public OperatorKind() { }

        #endregion 

        #region constants
        /// <summary>
        /// Equals operator (‘=’ or ‘==’)
        /// </summary>
        public const int op_eq = 2001;

        /// <summary>
        /// Not equals operator (‘!=’ or ‘/=’ or ‘<>’)
        /// </summary>
        public const int op_ne = 2002;

        /// <summary>
        /// Less-than or equals operator (‘<=’)
        /// </summary>
        public const int op_le = 2003;

        /// <summary>
        /// Less-than operator (‘<’)
        /// </summary>
        public const int op_lt = 2004;

        /// <summary>
        /// Greater-than or equals operator (‘>=’)
        /// </summary>
        public const int op_ge = 2005;

        /// <summary>
        /// Greater-than operator (‘>’)
        /// </summary>
        public const int op_gt = 2006;

        /// <summary>
        /// Matches operator (‘matches’ or ‘is_in’)
        /// </summary>
        public const int op_matches = 2007;

        /// <summary>
        /// Not logical operator
        /// </summary>
        public const int op_not = 2010;

        /// <summary>
        /// And logical operator
        /// </summary>
        public const int op_and = 2011;

        /// <summary>
        /// Or logical operator
        /// </summary>
        public const int op_or = 2012;

        /// <summary>
        /// Xor logical operator
        /// </summary>
        public const int op_xor = 2013;

        /// <summary>
        /// Implies logical operator
        /// </summary>
        public const int op_implies = 2014;

        /// <summary>
        /// For-all quantifier operator
        /// </summary>
        public const int op_for_all = 2015;

        /// <summary>
        /// Exists quantifier operator
        /// </summary>
        public const int op_exists = 2016;

        /// <summary>
        /// Plus operator (‘+’)
        /// </summary>
        public const int op_plus = 2020;

        /// <summary>
        /// Minus operator (‘-’)
        /// </summary>
        public const int op_minus = 2021;

        /// <summary>
        /// Multiply operator (‘*’
        /// </summary>
        public const int op_multiply = 2022;

        /// <summary>
        /// Divide operator (‘/’)
        /// </summary>
        public const int op_devide = 2023;

        /// <summary>
        /// Exponent operator (‘^’)
        /// </summary>
        public const int op_exp = 2024;
        #endregion

        #region Class properties
        private int _value;

        /// <summary>
        /// Actual value of this instance
        /// </summary>
        public int Value
        {
            get { return _value; }
            set
            {
                DesignByContract.Check.Require(ValidOperator(value), string.Format(
                    AmValidationStrings.XMustBeValidY, "OperatorKind.Value", "operator"));
                _value = value;
            }
        }
        #endregion

        #region Functions
        public static bool ValidOperator(int anOperator)
        {
            return anOperator >= op_eq && anOperator <= op_exp;
        }
        #endregion

        internal static int GetOperatorKind(string operatorKindString)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(operatorKindString), 
                string.Format(CommonStrings.XMustNotBeNullOrEmpty, "operatorKindString"));

            switch (operatorKindString)
            {
                case "=": return OperatorKind.op_eq;
                case ">=": return OperatorKind.op_ge;
                case ">": return OperatorKind.op_gt;
                case "<": return OperatorKind.op_lt;
                case "<=": return OperatorKind.op_le;
                case "!=": 
                case "/=":
                case "<>":
                    return OperatorKind.op_ne;
                case "matches": return OperatorKind.op_matches;
                case "And":
                case "AND":
                case ",":
                case "and": return OperatorKind.op_and;
                case "Or":
                case "OR":
                case "or": return OperatorKind.op_or;
                default:
                    throw new ApplicationException(string.Format(
                        AmValidationStrings.UnsupportedExpressionOperator, operatorKindString));
            }
        }
    }
}