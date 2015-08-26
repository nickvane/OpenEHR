using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEhr.AssumedTypes.Impl
{
    [Serializable]
    class BindingListView<T> : BindingList<T> 
    {
        public BindingListView() : base() { }
        public BindingListView(List<T> list) : base(list as IList<T>) { }

        protected override bool SupportsSearchingCore
        {
            get { return true; }
        }

        protected override int FindCore(PropertyDescriptor property, object key)
        {
            DesignByContract.Check.Require(property != null, "property must not be null");
            DesignByContract.Check.Require(key != null, "key must not be null");

            for (int i = 0; i < this.Count; i++)
            {
                T item = this[i];

                // %HYYKA%
                //// CM: 27/02/07
                //if (key.GetType() == typeof(OpenEhrV1.DataTypes.Text.DvText))
                //{
                //    OpenEhrV1.DataTypes.Text.DvText keyDvText = key as OpenEhrV1.DataTypes.Text.DvText;
                //    OpenEhrV1.DataTypes.Text.DvText itemDvText = property.GetValue(item) as OpenEhrV1.DataTypes.Text.DvText;

                //    if (keyDvText.Value == itemDvText.Value)
                //        return i;
                //}
                if (property.GetValue(item).Equals(key))
                    return i;
            }
            return -1; // not found
        }
    }
}
