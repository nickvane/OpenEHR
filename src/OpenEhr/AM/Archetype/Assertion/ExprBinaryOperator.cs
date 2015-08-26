using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.Archetype.Assertion
{
    /// <summary>
    /// Binary operator expression node.
    /// </summary>
    [AmType("EXPR_BINARY_OPERATOR")]
    [Serializable]
    public class ExprBinaryOperator: ExprOperator
    {
        #region Constructors
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="leftOperand">Left operand node.</param>
        /// <param name="rightOperand">Right operand node.</param>
        /// <param name="type">Type name of this item in the mathematical sense.</param>
        /// <param name="anOperator">Code of operator.</param>
        /// <param name="precedenceOverriden">True if the natural precedence of operators is 
        /// overridden in the expression represented by this node of the expression tree.</param>
        public ExprBinaryOperator(ExprItem leftOperand, ExprItem rightOperand, string type,
            OperatorKind anOperator, bool precedenceOverriden)
            : base(type, anOperator, precedenceOverriden)
        {
            this.LeftOperand = leftOperand;
            this.RightOperand = rightOperand;
        }

        public ExprBinaryOperator() { }

        #endregion

        #region Class properties
        private ExprItem leftOperand;

        /// <summary>
        /// Left operand node.
        /// </summary>
        public ExprItem LeftOperand
        {
            get { return leftOperand; }
            set
            {
                DesignByContract.Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "LeftOperand value"));
                leftOperand = value;
            }
        }

        private ExprItem rightOperand;

        /// <summary>
        /// Right operand node.
        /// </summary>
        public ExprItem RightOperand
        {
            get { return rightOperand; }
            set
            {
                DesignByContract.Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "RightOperand value"));
                rightOperand = value;
            }
        }

        #endregion

        #region Class functions
        internal override OpenEhr.Paths.AssertionContext Evaluate(OpenEhr.Paths.AssertionContext contextObj)
        {
            DesignByContract.Check.Require(contextObj != null, string.Format(CommonStrings.XMustNotBeNull, "contextObj"));
         
            switch (this.Operator.Value)
            {
                case OperatorKind.op_eq:
                    return new OpenEhr.Paths.AssertionContext(Equals(contextObj), contextObj);
                case OperatorKind.op_matches:
                    return new OpenEhr.Paths.AssertionContext(Matches(contextObj), contextObj);
                case OperatorKind.op_lt:
                    return new OpenEhr.Paths.AssertionContext(Less(contextObj), contextObj);
                case OperatorKind.op_le:
                    return new OpenEhr.Paths.AssertionContext(LessEquals(contextObj), contextObj);
                case OperatorKind.op_gt:
                    return new OpenEhr.Paths.AssertionContext(Greater(contextObj), contextObj);
                case OperatorKind.op_ge:
                    return new OpenEhr.Paths.AssertionContext(GreaterEquals(contextObj), contextObj);
                case OperatorKind.op_and:
                    return new OpenEhr.Paths.AssertionContext(And(contextObj), contextObj);
                case OperatorKind.op_or:
                    return new OpenEhr.Paths.AssertionContext(Or(contextObj), contextObj);
                case OperatorKind.op_ne:
                    return new OpenEhr.Paths.AssertionContext(NotEquals(contextObj), contextObj);
                default:
                    throw new NotSupportedException(this.Operator.ToString()+"operator not supported");
            }

        }

        private bool Equals(OpenEhr.Paths.AssertionContext contextObj)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(this.Type), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Type"));

            OpenEhr.Paths.AssertionContext leftObjContext = this.leftOperand.Evaluate(contextObj);
            OpenEhr.Paths.AssertionContext rightObjContext = this.rightOperand.Evaluate(contextObj);
           
            if (leftObjContext == null || leftObjContext.Data == null)
                return false;

            object rightObj = rightObjContext.Data;
            object leftObj = leftObjContext.Data;

            this.Type = this.Type.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            switch (this.Type)
            {
                case "string":
                    return leftObj.ToString() == rightObj.ToString();
                case "integer":
                    {
                        int leftInt = ToInt(leftObj);
                        int rightInt = ToInt(rightObj);                       
                        return leftInt == rightInt;
                    }
                case "double":
                case "float":
                    {
                        double leftDouble = ToDouble(leftObj);
                        double rightDouble = ToDouble(rightObj);
                       
                        return leftDouble == rightDouble;
                    }
                case "boolean":
                case "bool":
                    {
                        bool leftBool = ToBoolean(leftObj);
                        bool rightBool = ToBoolean(rightObj);

                        return leftBool == rightBool;
                    }
                case "date_time":
                case "dv_date_time":
                case "date":
                case "dv_date":
                case "iso8601datetime":
                    {
                        OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime leftDateTime = ToDvDateTime(leftObj);

                        OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime rightDateTime = ToDvDateTime(rightObj);

                        return leftDateTime == rightDateTime;
                    }
                case "duration":
                case "dv_duration":
                case "iso8601duration":
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration leftDuration = ToDvDuration(leftObj);
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration rightDuration = ToDvDuration(rightObj);
                    return leftDuration == rightDuration;

                case "time":
                case "dv_time":
                case "iso8601time":
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime leftTime = ToDvTime(leftObj);
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime rightTime = ToDvTime(rightObj);
                    return leftTime == rightTime;
                default:
                    throw new ApplicationException(string.Format(
                        AmValidationStrings.TypeXNotSupportedByYOperator, this.Type, "Equals"));
            }
        }

        private bool NotEquals(OpenEhr.Paths.AssertionContext contextObj)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(this.Type), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Type"));

            OpenEhr.Paths.AssertionContext leftObjContext = this.leftOperand.Evaluate(contextObj);
            OpenEhr.Paths.AssertionContext rightObjContext = this.rightOperand.Evaluate(contextObj);
            object rightObj = rightObjContext.Data;
            object leftObj = leftObjContext.Data;

            this.Type = this.Type.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            switch (this.Type)
            {
                case "string":
                    return leftObj.ToString() != rightObj.ToString();
                case "integer":
                    {
                        int leftInt = ToInt(leftObj);
                        int rightInt = ToInt(rightObj);
                        return leftInt != rightInt;
                    }
                case "double":
                case "float":
                    {
                        double leftDouble = ToDouble(leftObj);
                        double rightDouble = ToDouble(rightObj);

                        return leftDouble != rightDouble;
                    }
                case "boolean":
                case "bool":
                    {
                        bool leftBool = ToBoolean(leftObj);
                        bool rightBool = ToBoolean(rightObj);

                        return leftBool != rightBool;
                    }
                case "date_time":
                case "dv_date_time":
                case "date":
                case "dv_date":
                case "iso8601datetime":
                    {
                        OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime leftDateTime = ToDvDateTime(leftObj);

                        OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime rightDateTime = ToDvDateTime(rightObj);

                        return leftDateTime != rightDateTime;
                    }
                case "duration":
                case "dv_duration":
                case "iso8601duration":
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration leftDuration = ToDvDuration(leftObj);
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration rightDuration = ToDvDuration(rightObj);
                    return leftDuration != rightDuration;

                case "time":
                case "dv_time":
                case "iso8601time":
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime leftTime = ToDvTime(leftObj);
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime rightTime = ToDvTime(rightObj);
                    return leftTime != rightTime;
                default:
                    throw new ApplicationException(string.Format(
                        AmValidationStrings.TypeXNotSupportedByYOperator, this.Type, "NotEquals"));
            }
        }

        private bool GreaterEquals(OpenEhr.Paths.AssertionContext contextObj)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(this.Type), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Type"));

            OpenEhr.Paths.AssertionContext leftObjContext = this.leftOperand.Evaluate(contextObj);
            if (leftObjContext == null || leftObjContext.Data == null)
                return false;

            OpenEhr.Paths.AssertionContext rightObjContext = this.rightOperand.Evaluate(contextObj);
            if (rightObjContext == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "rightObjContext"));
            if(rightObjContext.Data == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "rightObjContext.Data"));

            object rightObj = rightObjContext.Data;
            object leftObj = leftObjContext.Data;

            this.Type = this.Type.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            switch (this.Type)
            {
                case "date_time":
                case "dv_date_time":
                case "date":
                case "dv_date":
                case "iso8601datetime":
                    {
                        OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime leftDateTime = ToDvDateTime(leftObj);
                       
                        OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime rightDateTime = ToDvDateTime(rightObj);

                        return leftDateTime >= rightDateTime;
                    }
                case "integer":
                    {
                        int leftInt = ToInt(leftObj);
                        int rightInt = ToInt(rightObj);  
                        return leftInt >= rightInt;
                    }
                case "double":
                case "float":
                    {
                        double leftDouble = ToDouble(leftObj);
                        double rightDouble = ToDouble(rightObj);
                        return leftDouble >= rightDouble;
                    }
                case "duration":
                case "dv_duration":
                case "iso8601duration":
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration leftDuration = ToDvDuration(leftObj);
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration rightDuration = ToDvDuration(rightObj);
                    return leftDuration >= rightDuration;

                case "time":
                case "dv_time":
                case "iso8601time":
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime leftTime = ToDvTime(leftObj);
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime rightTime = ToDvTime(rightObj);
                    return leftTime >= rightTime;

                case "string":
                    if (AssumedTypes.Iso8601DateTime.ValidIso8601DateTime(rightObj.ToString()))
                        this.Type = "DV_DATE_TIME";
                    else if (AssumedTypes.Iso8601Time.ValidIso8601Time(rightObj.ToString()))
                        this.Type = "DV_TIME";
                    else if (AssumedTypes.Iso8601Duration.ValidIso8601Duration(rightObj.ToString()))
                        this.Type = "DV_DURATION";
                    else
                        throw new ApplicationException(string.Format(
                            AmValidationStrings.OperatorXInvalidForTypeY, ">=", "string"));
                    return GreaterEquals(contextObj);
               
                default:
                    throw new ApplicationException(string.Format(
                        AmValidationStrings.TypeXNotSupportedByYOperator, this.Type, "GreaterEquals"));
            }
        }
        private bool Greater(OpenEhr.Paths.AssertionContext contextObj)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(this.Type), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Type"));

            OpenEhr.Paths.AssertionContext leftObjContext = this.leftOperand.Evaluate(contextObj);
            if (leftObjContext == null || leftObjContext.Data == null)
                return false;

            OpenEhr.Paths.AssertionContext rightObjContext = this.rightOperand.Evaluate(contextObj);
            if (rightObjContext == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "rightObjContext"));
            if (rightObjContext.Data == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "rightObjContext.Data"));

            object rightObj = rightObjContext.Data;
            object leftObj = leftObjContext.Data;

            this.Type = this.Type.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            switch (this.Type)
            {
                case "date_time":
                case "dv_date_time":
                case "date":
                case "dv_date":
                case "iso8601datetime":
                    {
                        OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime leftDateTime = ToDvDateTime(leftObj);

                        OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime rightDateTime = ToDvDateTime(rightObj);

                        return leftDateTime > rightDateTime;
                    }
                case "integer":
                    {
                        int leftInt = ToInt(leftObj);
                        int rightInt = ToInt(rightObj); 
                        return leftInt > rightInt;
                    }
                case "double":
                case "float":
                    {
                        double leftDouble = ToDouble(leftObj);
                        double rightDouble = ToDouble(rightObj);
                        return leftDouble > rightDouble;
                    }

                case "duration":
                case "dv_duration":
                case "iso8601duration":
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration leftDuration = ToDvDuration(leftObj);
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration rightDuration = ToDvDuration(rightObj);
                    return leftDuration > rightDuration;

                case "time":
                case "dv_time":
                case "iso8601time":
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime leftTime = ToDvTime(leftObj);
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime rightTime = ToDvTime(rightObj);
                    return leftTime > rightTime;

                case "string":
                    if (AssumedTypes.Iso8601DateTime.ValidIso8601DateTime(rightObj.ToString()))
                        this.Type = "DV_DATE_TIME";
                    else if (AssumedTypes.Iso8601Time.ValidIso8601Time(rightObj.ToString()))
                        this.Type = "DV_TIME";
                    else if (AssumedTypes.Iso8601Duration.ValidIso8601Duration(rightObj.ToString()))
                        this.Type = "DV_DURATION";
                    else
                        throw new ApplicationException(string.Format(
                            AmValidationStrings.OperatorXInvalidForTypeY, ">", "string"));
                    return Greater(contextObj);

                default:
                    throw new ApplicationException(string.Format(
                        AmValidationStrings.TypeXNotSupportedByYOperator, this.Type, "Greater"));

            }
        }
        private bool LessEquals(OpenEhr.Paths.AssertionContext contextObj)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(this.Type), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Type"));

            OpenEhr.Paths.AssertionContext leftObjContext = this.leftOperand.Evaluate(contextObj);
            if (leftObjContext == null || leftObjContext.Data == null)
                return false;

            OpenEhr.Paths.AssertionContext rightObjContext = this.rightOperand.Evaluate(contextObj);
            if (rightObjContext == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "rightObjContext"));
            if (rightObjContext.Data == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "rightObjContext.Data"));

            object rightObj = rightObjContext.Data;
            object leftObj = leftObjContext.Data;

            this.Type = this.Type.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            switch (this.Type)
            {
                case "date_time":
                case "dv_date_time":
                case "date":
                case "dv_date":
                case "iso8601datetime":
                    {
                        OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime leftDateTime = ToDvDateTime(leftObj);

                        OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime rightDateTime = ToDvDateTime(rightObj);

                        return leftDateTime <= rightDateTime;
                    }
                case "integer":
                    {
                        int leftInt = ToInt(leftObj);
                        int rightInt = ToInt(rightObj);
                        return leftInt <= rightInt;
                    }
                case "double":
                case "float":
                    {
                        double leftDouble = ToDouble(leftObj);
                        double rightDouble = ToDouble(rightObj);
                        return leftDouble <= rightDouble;
                    }

                case "duration":
                case "dv_duration":
                case "iso8601duration":
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration leftDuration = ToDvDuration(leftObj);
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration rightDuration = ToDvDuration(rightObj);
                    return leftDuration <= rightDuration;

                case "time":
                case "dv_time":
                case "iso8601time":
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime leftTime = ToDvTime(leftObj);
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime rightTime = ToDvTime(rightObj);
                    return leftTime <= rightTime;

                case "string":
                    if (AssumedTypes.Iso8601DateTime.ValidIso8601DateTime(rightObj.ToString()))
                        this.Type = "DV_DATE_TIME";
                    else if (AssumedTypes.Iso8601Time.ValidIso8601Time(rightObj.ToString()))
                        this.Type = "DV_TIME";
                    else if (AssumedTypes.Iso8601Duration.ValidIso8601Duration(rightObj.ToString()))
                        this.Type = "DV_DURATION";
                    else
                        throw new ApplicationException(string.Format(
                            AmValidationStrings.OperatorXInvalidForTypeY, "<=", "string"));
                    return LessEquals(contextObj);

                default:
                    throw new ApplicationException("Type is not supported in equal operator: " + this.Type);
            }
        }
        private bool Less(OpenEhr.Paths.AssertionContext contextObj)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(this.Type), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Type"));

            OpenEhr.Paths.AssertionContext leftObjContext = this.leftOperand.Evaluate(contextObj);
            if (leftObjContext == null || leftObjContext.Data == null)
                return false;

            OpenEhr.Paths.AssertionContext rightObjContext = this.rightOperand.Evaluate(contextObj);
            if (rightObjContext == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "rightObjContext"));
            if (rightObjContext.Data == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "rightObjContext.Data"));

            object rightObj = rightObjContext.Data;
            object leftObj = leftObjContext.Data;


            this.Type = this.Type.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            switch (this.Type)
            {
                case "date_time":
                case "dv_date_time":
                case "date":
                case "dv_date":
                case "iso8601datetime":
                    {
                        OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime leftDateTime = ToDvDateTime(leftObj);

                        OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime rightDateTime = ToDvDateTime(rightObj);

                        return leftDateTime < rightDateTime;
                    }
                case "integer":
                    {
                        int leftInt = ToInt(leftObj);
                        int rightInt = ToInt(rightObj);
                        return leftInt < rightInt;
                    }
                case "double":
                case "float":
                    {
                        double leftDouble = ToDouble(leftObj);
                        double rightDouble = ToDouble(rightObj);
                        return leftDouble < rightDouble;
                    }

                case "duration":
                case "dv_duration":
                case "iso8601duration":
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration leftDuration = ToDvDuration(leftObj);
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration rightDuration = ToDvDuration(rightObj);
                    return leftDuration < rightDuration;

                case "time":
                case "dv_time":
                case "iso8601time":
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime leftTime = ToDvTime(leftObj);
                    OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime rightTime = ToDvTime(rightObj);
                    return leftTime < rightTime;

                case "string":
                    if (AssumedTypes.Iso8601DateTime.ValidIso8601DateTime(rightObj.ToString()))
                        this.Type = "DV_DATE_TIME";
                    else if (AssumedTypes.Iso8601Time.ValidIso8601Time(rightObj.ToString()))
                        this.Type = "DV_TIME";
                    else if (AssumedTypes.Iso8601Duration.ValidIso8601Duration(rightObj.ToString()))
                        this.Type = "DV_DURATION";
                    else
                        throw new ApplicationException(string.Format(
                            AmValidationStrings.OperatorXInvalidForTypeY, "<", "string"));
                    return Less(contextObj);

                default:
                    throw new ApplicationException("Type is not supported in equal operator: " + this.Type);
            }
        }
        private bool Matches(OpenEhr.Paths.AssertionContext contextObj)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(this.Type), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Type"));

            OpenEhr.Paths.AssertionContext leftObjContext = this.leftOperand.Evaluate(contextObj);
            if (leftObjContext == null || leftObjContext.Data == null)
                return false;

            OpenEhr.Paths.AssertionContext rightObjContext = this.rightOperand.Evaluate(contextObj);
            if (rightObjContext == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "rightObjContext"));
            if (rightObjContext.Data == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "rightObjContext.Data"));

            object rightObj = rightObjContext.Data;
            object leftObj = leftObjContext.Data;

            string referenceTypeFromRightExpr = ((ExprLeaf)(this.RightOperand)).ReferenceType;
            referenceTypeFromRightExpr = referenceTypeFromRightExpr.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            switch (referenceTypeFromRightExpr)
            {               
                case "pattern":
                    {
                        return Regex.Match(leftObj.ToString(), rightObj.ToString(), RegexOptions.Compiled | RegexOptions.Singleline).Success;
                    }

                    // TODO: LIST, INTERVAL ETC.
                default:
                    throw new ApplicationException(string.Format(
                        AmValidationStrings.TypeXNotSupportedByYOperator, referenceTypeFromRightExpr + " (referenceTypeFromRightExpr)", "Matches"));
            }
        }
        private bool And(OpenEhr.Paths.AssertionContext contextObj)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(this.Type), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Type"));

            OpenEhr.Paths.AssertionContext leftObjContext = this.leftOperand.Evaluate(contextObj);
            if (leftObjContext == null || leftObjContext.Data == null)
                return false;
            object leftObj = leftObjContext.Data;
            bool leftBoolValue = ToBoolean(leftObj);
            if (!leftBoolValue)
                return false;

            OpenEhr.Paths.AssertionContext rightObjContext = this.rightOperand.Evaluate(contextObj);
            if (rightObjContext == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "rightObjContext"));
            if (rightObjContext.Data == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "rightObjContext.Data"));

            object rightObj = rightObjContext.Data;
            bool rightBoolValue = ToBoolean(rightObj);
            if (!rightBoolValue)
                return false;

            return true;
        }

        private bool Or(OpenEhr.Paths.AssertionContext contextObj)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(this.Type), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Type"));

            OpenEhr.Paths.AssertionContext leftObjContext = this.leftOperand.Evaluate(contextObj);
            if (leftObjContext == null || leftObjContext.Data == null)
                return false;
            object leftObj = leftObjContext.Data;
            bool leftBoolValue = ToBoolean(leftObj);
            if (leftBoolValue)
                return true;

            OpenEhr.Paths.AssertionContext rightObjContext = this.rightOperand.Evaluate(contextObj);
            if (rightObjContext == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "rightObjContext"));
            if (rightObjContext.Data == null)
                throw new ApplicationException(string.Format(CommonStrings.XIsNull, "rightObjContext.Data"));

            object rightObj = rightObjContext.Data;
            bool rightBoolValue = ToBoolean(rightObj);

            return rightBoolValue;

        }

        private int ToInt(object value)
        {
            DesignByContract.Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "value"));
            int intValue = int.MinValue;

            if (!int.TryParse(value.ToString(), out intValue))
                throw new ApplicationException(string.Format(
                    AmValidationStrings.XIsNotTypeYValue, value.ToString(), "integer"));

            return intValue;
        }

        private OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime ToDvDateTime(object value)
        {
            DesignByContract.Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "value"));
            OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime dateTime = null;

            if (AssumedTypes.Iso8601DateTime.ValidIso8601DateTime(value.ToString()))
                dateTime = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime(value.ToString());
            else
                throw new ApplicationException(string.Format(
                    AmValidationStrings.XIsNotTypeYValue, value.ToString(), "Iso8601DateTime"));

            return dateTime;
        }

        private double ToDouble(object value)
        {
            DesignByContract.Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "value"));
            double doubleValue = double.MinValue;

            if (!double.TryParse(value.ToString(), out doubleValue))
                throw new ApplicationException(string.Format(
                    AmValidationStrings.XIsNotTypeYValue, value.ToString(), "double"));

            return doubleValue;
        }

        private OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration ToDvDuration(object value)
        {
            DesignByContract.Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "value"));
            OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration duration = null;

            if (AssumedTypes.Iso8601Duration.ValidIso8601Duration(value.ToString()))
                duration = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration(value.ToString());
            else
                throw new ApplicationException(string.Format(
                    AmValidationStrings.XIsNotTypeYValue, value.ToString(), "Iso8601Duration"));

            return duration;
        }

        private OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime ToDvTime(object value)
        {
            DesignByContract.Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "value"));
            OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime time = null;

            if (AssumedTypes.Iso8601Time.ValidIso8601Time(value.ToString()))
                time = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime(value.ToString());
            else
                throw new ApplicationException(string.Format(
                    AmValidationStrings.XIsNotTypeYValue, value.ToString(), "Iso8601Time"));

            return time;
        }
        private bool ToBoolean(object value)
        {
            DesignByContract.Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "value"));

            bool boolValue = false;

            if (!bool.TryParse(value.ToString(), out boolValue))
                throw new ApplicationException(string.Format(
                    AmValidationStrings.XIsNotTypeYValue, value.ToString(), "boolean"));

            return boolValue;
        }
        #endregion

    }
}