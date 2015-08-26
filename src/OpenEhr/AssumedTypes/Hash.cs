using System;
using System.ComponentModel;
using OpenEhr.Attributes;

namespace OpenEhr.AssumedTypes
{
    /// <summary> 
    /// Type representing a keyed table of values. T is the value type, and U the type of the keys.
    /// </summary>
    /// <typeparam name="T">List item type</typeparam>
    /// 
    public interface IHash<T, U>
    {
        T Item(U key);
        bool HasKey(U key);
    }

    /// <summary>
    /// Type representing a keyed table of values. T is the value type, and U the type of the keys.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    [Serializable]
    [RmType("openEHR", "SUPPORT", "HASH")]
    public class Hash<T, U> 
        : Aggregate<T>, IHash<T, U>, System.Collections.IEnumerable
        where U : System.IComparable
    {
        private System.Collections.Generic.Dictionary<U, T> innerDictionnary;

        public Hash(System.Collections.Generic.Dictionary<U, T> dictionnary)
        {
            this.innerDictionnary = dictionnary;
        }
        
        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.innerDictionnary.GetEnumerator();
        }

        #endregion

        #region IHash<T,U> Members

        /// <summary>
        /// Return item for key ‘a_key’. Equivalent to ISO 11404 fetch operation.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Item(U key)
        {
            return this.innerDictionnary[key];
        }

        /// <summary>
        /// Test for membership of a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(U key)
        {
            return this.innerDictionnary.ContainsKey(key);
        }

        internal AssumedTypes.List<U> Keys
        {
            get
            {
                AssumedTypes.List<U> keyList = new List<U>();
                foreach (U key in this.innerDictionnary.Keys)
                    keyList.Add(key);
                return keyList;
            }
        }
        #endregion

        public override void Clear()
        {
            innerDictionnary.Clear();
        }

        public override bool Has(T item)
        {
            return this.innerDictionnary.ContainsValue(item);          
        }

        public override int Count
        {
            get
            {
                return this.innerDictionnary.Count;
            }
        }

        public override bool IsEmpty()
        {
            return this.innerDictionnary.Count == 0;
        }

        protected override void AddItem(T item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void RemoveItem(T item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override T GetItem(string nodePredicate)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override bool Has(string predicate)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
