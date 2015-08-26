using System;
using System.Collections.Generic;
using OpenEhr.RM.Common.Archetyped.Impl;

namespace OpenEhr.AssumedTypes.Impl
{
    [Serializable]
    class NamedLocatableList<T> 
        : List<T> 
        where T : Locatable
    {
        internal NamedLocatableList()
            : base(new LocatableBindingListView<T>())
        {
        }

        protected LocatableBindingListView<T> InnerList
        {
            get { return base.InnerList as LocatableBindingListView<T>; }
        }

        public T this[string name]
        {
            get
            {
                return this.InnerList[name];
            }
        }

        public bool Contains(string name)
        {
            return this.InnerList.Contains(name);
        }

        // CM: 05/03/08 support finding items using codestring and terminologyId
        public T this[string nameTerminologyId, string nameCodeString]
        {
            get
            {
                return this.InnerList[nameTerminologyId, nameCodeString];
            }
        }

        public bool Contains(string nameTerminologyId, string nameCodeString)
        {
            return this.InnerList.Contains(nameTerminologyId, nameCodeString);
        }
    }
}
