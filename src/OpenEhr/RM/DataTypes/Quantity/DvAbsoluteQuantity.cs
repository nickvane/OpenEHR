using System;
using OpenEhr.Attributes;

namespace OpenEhr.RM.DataTypes.Quantity
{
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_ABSOLUTE_QUANTITY")]
    public abstract class DvAbsoluteQuantity<T, U> : DvQuantified<T>
        where T : DvAbsoluteQuantity<T, U>
        where U : DvAmount<U>
    {
        public abstract DvAmount<U> Accuracy
        {
            get;
        }

        public abstract DvAbsoluteQuantity<T, U> Add(DvAmount<U> b);
        public abstract DvAbsoluteQuantity<T, U> Subtract(DvAmount<U> b);
        public abstract DvAmount<U> Diff(DvAbsoluteQuantity<T, U> b);

        public override bool AccuracyUnknown()
        {
            return this.Accuracy == null;
        }

        protected override object GetAccuracy()
        {
            return this.Accuracy;
        }
    }
}
