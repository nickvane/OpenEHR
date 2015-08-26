using System;
using System.ComponentModel;
using OpenEhr.RM.Common.Archetyped;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.RM.Impl;

namespace OpenEhr.AssumedTypes
{
    interface IList : System.Collections.IEnumerable
    {
        int Count { get;}
        object this[int index] { get;}
    }

    /// <summary>
    /// Ordered container that may contain duplicates
    /// </summary>
    /// <typeparam name="T">List item type</typeparam>
    [Serializable]
    [RmType("openEHR", "SUPPORT", "LIST")]
    public class List<T>
        : Aggregate<T>, System.Collections.Generic.IEnumerable<T>, IListSource, IList, IVisitable
    {
        private BindingList<T> innerList;

        public List()
        {
            innerList = new BindingList<T>();
        }

        public List(System.Collections.Generic.IEnumerable<T> items)
            : this()
        {
            Check.Require(items != null, "items must not be null");

            foreach (T item in items)
                Add(item);
        }

        protected List(BindingList<T> innerList)
        {
            DesignByContract.Check.Require(innerList != null, "inerList must not be null");

            this.innerList = innerList;
        }

        protected BindingList<T> InnerList
        {
            get { return this.innerList; }
        }

        public override int Count
        {
            get { return innerList.Count; }
        }

        public override bool IsEmpty()
        {
            return (innerList.Count == 0);
        }

        public T First
        {
            get
            {
                if (!IsEmpty())
                    return innerList[0];
                else
                    return default(T);  
            }
        }

        public T Last
        {
            get
            {
                if (!IsEmpty())
                    return innerList[innerList.Count - 1];
                else
                    return default(T);  
            }
        }

        object IList.this[int index]
        {
            get { return innerList[index] as object; }
        }

        public T this[int index]
        {
            get { return innerList[index]; }
        }

        public override void Clear()
        {
            innerList.Clear();
        }

        protected override void AddItem(T item)
        {
            innerList.Add(item);
        }

        protected override void RemoveItem(T item)
        {
            innerList.Remove(item);
        }

        #region IEnumerable<T> Members

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        #endregion

        #region IListSource Members

        bool IListSource.ContainsListCollection
        {
            get { return false; }
        }

        System.Collections.IList IListSource.GetList()
        {
            return innerList;
        }

        #endregion

        public override bool Has(T item)
        {
            return innerList.Contains(item);
        }

        protected override T GetItem(string nodePredicate)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override bool Has(string predicate)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #region IVisitable Members

        void IVisitable.Accept(IVisitor visitor)
        {
            foreach (T item in this)
            {
                IVisitable visitable = item as IVisitable;
                if (visitable == null)
                    throw new NotImplementedException("items must be implement IVistitable");
                visitable.Accept(visitor);
            }
        }

        #endregion
    }
}
