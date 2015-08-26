using System;
using OpenEhr.Resources;
using OpenEhr.Attributes;
using OpenEhr.Paths;

namespace OpenEhr.AM.Archetype.Assertion
{
    /// <summary>
    /// Expression tree leaf item. This can represent one of:
    /// • a manifest constant of any primitive type (Integer, Real, Boolean, String,
    /// Character, Date, Time, Date_time, Duration), or (in future) of any complex
    /// reference model type, e.g. a DV_CODED_TEXT;
    /// • a path referring to a value in the archetype (paths with a leading ‘/’ are in the
    /// definition section; paths with no leading ‘/’ are in the outer part of the archetype,
    /// e.g. “archetype_id/value” refers to the String value of the archetype_id
    /// attribute of the enclosing archetype;
    /// • a constraint, expressed in the form of concrete subtype of C_OBJECT; most
    /// often this will be a C_PRIMITIVE_OBJECT.
    /// </summary>
    [AmType("EXPR_LEAF")]
    [Serializable]
    public class ExprLeaf: ExprItem
    {
        #region Constructors     
       
        public ExprLeaf(object item, string type, string referenceType)
            : base(type)
        {
            this.Item = item;
            this.ReferenceType = referenceType;
        }

        public ExprLeaf() { }
        #endregion

        #region Class properties
        private object item;
        /// <summary>
        /// The value referred to; a manifest constant, an attribute path (in the form of a String), 
        /// or for the right-hand side of a ‘matches’ node, a constraint, often a C_PRIMITIVE_OBJECT.
        /// [Future: paths including function names as well, even if not constrained in the archetype
        /// - as long as they are in the reference model].
        /// </summary>
        public object Item
        {
            get { return item; }
            set
            {
                DesignByContract.Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "Item value"));
                item = value;
            }
        }

        private string referenceType;

        /// <summary>
        /// Type of reference: “constant”, “attribute”, “function”, “constraint”. The first three are
        /// used to indicate the referencing mechanism for an operand. The last is used to indicate a
        /// constraint operand, as happens in the case of the right-hand operand of the ‘matches’ operator.
        /// </summary>
        public string ReferenceType
        {
            get { return referenceType; }
            set
            {
                DesignByContract.Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "ReferenceType value"));
                referenceType = value;
            }
        }
        #endregion     

        #region Class functions
        internal override OpenEhr.Paths.AssertionContext Evaluate
            (OpenEhr.Paths.AssertionContext obj)
        {
            switch (this.ReferenceType.ToLower(System.Globalization.CultureInfo.InvariantCulture))
            {
                case "constraint":
                case "pattern":
                    return new OpenEhr.Paths.AssertionContext(this.Item, obj);
                case "path":                    
                    string path = this.item.ToString();
                    Path pathProcessor = new Path(path);
                    object objectAtPath = pathProcessor.ItemAtPath(obj.Data);
                    return new OpenEhr.Paths.AssertionContext(objectAtPath, obj);

                default:
                    throw new ApplicationException(string.Format(
                        AmValidationStrings.UnsupportedExprLeafReferenceType, this.ReferenceType));
            }
        }
        #endregion
    }
}