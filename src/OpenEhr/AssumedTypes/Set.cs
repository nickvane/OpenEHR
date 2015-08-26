using System;
using System.ComponentModel;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;

namespace OpenEhr.AssumedTypes
{
    /// <summary>
    /// Unordered container that may not contain duplicates.
    /// </summary>
    /// <typeparam name="T">List item type</typeparam>
    [Serializable]
    [RmType("openEHR", "SUPPORT", "SET")]
    public class Set<T> 
        : Aggregate<T>, System.Collections.Generic.IEnumerable<T>
    {
        /// <summary>
        /// Use Dictionary as the underly data type is to ensure uniqueness of the list.
        /// </summary>

        private System.Collections.Generic.List<T> innerList;

        public Set()
        {
            this.innerList = new System.Collections.Generic.List<T>();
        }

        public Set (System.Collections.Generic.IEnumerable<T> items)
            : this()
        {
            foreach (T eachItem in items)
            {
                if (this.innerList.Contains(eachItem))
                    throw new ArgumentException("Set<T> class doesn't allow duplicates");//, exception);
                this.innerList.Add(eachItem);
            }
        }      

        public override bool Has(T item)
        {
            return this.innerList.Contains(item);

        }

        public virtual T this[int index]
        {
            get { return this.innerList[index]; } 
        }

        public override int Count
        {
            get { return this.innerList.Count; }
        }

        public override bool IsEmpty()
        {
            return this.innerList.Count == 0;
        }

        #region IEnumerable<T> Members

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            return this.innerList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        #endregion

        protected override void AddItem(T item)
        {
            Check.Require(!innerList.Contains(item), "items must be unique within set");

            this.innerList.Add(item);
        }

        protected override void RemoveItem(T item)
        {
            Check.Require(innerList.Contains(item), "items mustexist within set");

            this.innerList.Remove(item);
        }

        protected override T GetItem(string nodePredicate)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override bool Has(string predicate)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void Clear()
        {
            this.innerList.Clear();
        }

    }
}
