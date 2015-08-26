using System;
using OpenEhr.AssumedTypes;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.AM.OpenehrProfile.DataTypes.Quantity
{
    /// <summary>
    /// Constrain instances of DV_QUANTITY.
    /// </summary>
    [Serializable]
    [AmType("C_DV_ITEM")]
    public class CQuantityItem
    {
        #region Constructors
        public CQuantityItem() { }
        public CQuantityItem(Interval<float> magnitude, Interval<int> precision, string units)
        {
            this.Magnitude = magnitude;
            this.Precision = precision;
            this.Units = units;
        }
        #endregion

        #region Class properties       
        
        private Interval<float> magnitude;
        /// <summary>
        /// Constraint on the magnitude of the DV_QUANTITY.
        /// </summary>
        public Interval<float> Magnitude
        {
            get { return magnitude; }
            set { magnitude = value; }
        }

        private Interval<int> precision;
        /// <summary>
        /// Constraint on the precision of the DV_QUANTITY. A value of -1 means that precision is unconstrained.
        /// </summary>
        public Interval<int> Precision
        {
            get { return precision; }
            set { precision = value; }
        }

        private string units;
        /// <summary>
        /// Constraint on units of the DV_QUANTITY
        /// </summary>
        public string Units
        {
            get { return units; }
            set
            {
                DesignByContract.Check.Require(value != null, string.Format(
                    CommonStrings.XMustNotBeNull, "Units value"));
                units = value;
            }
        }
        #endregion

        #region Functions
        /// <summary>
        /// True if no constraint on precision; True if precision = -1.
        /// </summary>
        /// <returns></returns>
        public bool PrecisionUnconstrained()
        {
            return this.Precision == null || (this.Precision.Lower == -1 && this.Precision.Upper == -1);
        }
        #endregion

    }
}
