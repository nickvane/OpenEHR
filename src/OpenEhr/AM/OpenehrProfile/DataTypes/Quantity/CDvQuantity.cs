using System;
using OpenEhr.AM.Archetype.ConstraintModel;
using OpenEhr.RM.DataTypes.Quantity;
using OpenEhr.AssumedTypes;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.OpenehrProfile.DataTypes.Quantity
{
    /// <summary>
    /// Constrain instances of DV_QUANTITY
    /// </summary>
    [Serializable]
    [AmType("C_DV_QUANTITY")]
    public class CDvQuantity : CDomainType
    {
        #region Constructors
        public CDvQuantity() : base() { }

        public CDvQuantity(string rmTypeName, string nodeId, AssumedTypes.Interval<int> occurrences,
            CAttribute parent, object assumedValue, List<CQuantityItem> list, CodePhrase property)
            : base(rmTypeName, nodeId, occurrences, parent, assumedValue)
        {
            this.List = list;
            this.Property = property;
        }
        #endregion

        #region Class properties
        private List<CQuantityItem> list;

        /// <summary>
        /// List of value/units pairs.
        /// </summary>
        public List<CQuantityItem> List
        {
            get { return list; }
            set { list = value; }
        }

        private CodePhrase property;

        /// <summary>
        /// Optional constraint on units property
        /// </summary>
        public CodePhrase Property
        {
            get { return property; }
            set { property = value; }
        }


        #endregion

        #region Functions
        public override CComplexObject StandardEquivalent()
        {
            throw new Exception(string.Format(
                AmValidationStrings.StandardEquivNotImplementedInX, "CDvQuantity"));
        }

        public override object DefaultValue
        {
            get
            {
                throw new Exception(string.Format(
                    AmValidationStrings.DefaultValueNotImplementedInX, "CDvQuantity"));
            }
        }

        public override bool ValidValue(object aValue)
        {

            DesignByContract.Check.Require(aValue != null, string.Format(
                CommonStrings.XMustNotBeNull, "aValue"));
            DesignByContract.Check.Require(this.IsValid(), string.Format(
                AmValidationStrings.ConstraintXIsValidGetsFalse, "CDvQuantity"));

            if (this.AnyAllowed())
                return true;

            DvQuantity dvQuantity = aValue as DvQuantity;

            if (dvQuantity != null)
            {
                if (this.list != null)
                {
                    foreach (CQuantityItem item in this.List)
                    {
                        if (item.Units == dvQuantity.Units)
                        {
                            double dvMagnitude = dvQuantity.Magnitude;
                            float floatValue = (float)(dvMagnitude);
                            if (item.Magnitude == null || item.Magnitude.Has(floatValue))
                                return true;
                        }
                    }
                }
                else
                {
                    // TODO: lookup allowed units for specified property
                    return true;
                }
            }
            this.ValidationContext.AcceptValidationError(this,
                    string.Format(AmValidationStrings.InvalidDvQuantityX, dvQuantity));
            
            return false;            
        }

        /// <summary>
        /// True if any DV_QUANTITY instance allowed.
        /// </summary>
        /// <returns></returns>
        public override bool AnyAllowed()
        {
            return this.Property == null && this.List == null;
        }

        public override bool IsValid()
        {
            if (this.List != null && this.List.IsEmpty())
                return false;

            if (!((this.Property != null || this.List != null) ^ AnyAllowed()))
                return false;

            return true;
        }

        #endregion
    }
}