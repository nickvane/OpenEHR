using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Archetyped;

namespace OpenEhr.AssumedTypes.Impl
{
    public abstract class LocatableSetBase<T> : OpenEhr.AssumedTypes.Set<T> 
        where T : Pathable, ILocatable
    {
        internal LocatableSetBase()
            : base()
        { }

        protected LocatableSetBase(Pathable parent)
            : base()
        {
            Check.Require(parent != null, "parent must not be null");
            this.parent = parent;
            Check.Ensure(this.parent != null, "parent must not be null");
        }

        Pathable parent;

        public virtual Pathable Parent
        {
            get { return this.parent; }
            internal protected set
            {
                this.parent = value;
                foreach (OpenEhr.RM.Common.Archetyped.Pathable item in this)
                    item.Parent = value;
            }
        }
    }

    [Serializable]
    public class LocatableSet<T> : LocatableSetBase<T> 
        where T : Pathable, ILocatable
    {
        private System.Collections.Generic.Dictionary<string, LocatableBindingListView<T>> identifiedLocatables;

        internal LocatableSet()
            : base()
        {
            identifiedLocatables = new System.Collections.Generic.Dictionary<string, LocatableBindingListView<T>>();
        }

        internal LocatableSet(System.Collections.Generic.IEnumerable<T> items)
            : this()
        {
            Check.Invariant(this.identifiedLocatables != null, "identifiedLocatables must not be null");

            if (items != null)
                foreach (T item in items)
                {
                    item.Parent = null;
                    AddItem(item);
                }
        }

        internal LocatableSet(OpenEhr.RM.Common.Archetyped.Pathable parent)
            : this()
        {
            Check.Invariant(this.identifiedLocatables != null, "identifiedLocatables must not be null");

            Check.Require(parent != null, "parent must not be null");

            this.Parent = parent;

            Check.Invariant(this.Parent != null, "parent must not be null");
        }

        internal LocatableSet(OpenEhr.RM.Common.Archetyped.Pathable parent, System.Collections.Generic.IEnumerable<T> items)
            : this(parent)
        {
            Check.Invariant(this.identifiedLocatables != null, "identifiedLocatables must not be null");
            Check.Invariant(this.Parent != null, "parent must not be null");

            if (items != null)
                foreach (T item in items)
                {
                    item.Parent = null;
                    AddItem(item);
                }
        }

        protected override void AddItem(T item)
        {

            Check.Invariant(identifiedLocatables != null, "identifiedLocatables must not be null");

            Pathable locatable = item as Pathable;
            Check.Assert(item != null, "item must not be null");

            if (item.Parent == null)
                item.Parent = this.Parent;

            else if (this.Parent == null)
                this.Parent = item.Parent;

            else if (!Object.ReferenceEquals(item.Parent, this.Parent))
                throw new ApplicationException("item parent must have same parent as other items");

            LocatableBindingListView<T> namedLocatables;
            if (identifiedLocatables.ContainsKey(item.ArchetypeNodeId))
                namedLocatables = identifiedLocatables[item.ArchetypeNodeId];
            else
            {
                namedLocatables = new LocatableBindingListView<T>();
                identifiedLocatables.Add(item.ArchetypeNodeId, namedLocatables);
            }

            DvCodedText codedName = item.Name as DvCodedText;
            if (codedName != null)
            {
                if (namedLocatables.Contains(codedName.DefiningCode.TerminologyId.Value, codedName.DefiningCode.CodeString))
                    throw new ApplicationException(string.Format("locatable ({0}) name ({1}) already existing in the namedLocatable list.",
                       item.ArchetypeNodeId, codedName.ToString()));
            }
            else if (namedLocatables.Contains(item.Name.Value))
                throw new ApplicationException(string.Format("locatable ({0}) name ({1}) already existing in this namedLocatable list",
                    item.ArchetypeNodeId, item.Name.Value));

            namedLocatables.Add(item);

            base.AddItem(item);
        }

        protected override void RemoveItem(T item)
        {
            Check.Invariant(identifiedLocatables != null, "identifiedLocatables must not be null");

            Check.Assert(item != null, "item must not be null");

            LocatableBindingListView<T> namedLocatables;
            if (identifiedLocatables.ContainsKey(item.ArchetypeNodeId))
            {
                namedLocatables = identifiedLocatables[item.ArchetypeNodeId];
                namedLocatables.Remove(item);

                if (namedLocatables.Count == 0)
                    identifiedLocatables.Remove(item.ArchetypeNodeId);
            }

            base.RemoveItem(item);
        }

        public T[] this[string nodeId]
        {
            get
            {
                Check.Require(!string.IsNullOrEmpty(nodeId), "nodeId must not be null or empty");

                T[] items;
                if (identifiedLocatables.ContainsKey(nodeId))
                {
                    System.ComponentModel.BindingList<T> list = identifiedLocatables[nodeId];
                    items = new T[list.Count];
                    list.CopyTo(items, 0);
                }
                else
                    items = new T[] { };
                return items;
            }
        }

        public bool Contains(string nodeId)
        {
            Check.Invariant(identifiedLocatables != null, "identifiedLocatables must not be null");

            Check.Require(!string.IsNullOrEmpty(nodeId), "nodeId must not be null or empty");

            bool result = identifiedLocatables.ContainsKey(nodeId);

            return result;
        }

        public bool Contains(string nodeId, DvText name)
        {
            Check.Invariant(identifiedLocatables != null, "identifiedLocatables must not be null");

            Check.Require(!string.IsNullOrEmpty(nodeId), "nodeId must not be null or empty");
            Check.Require(name != null, "name must not be null");

            LocatableBindingListView<T> namedLocatables = null;
            if (identifiedLocatables.ContainsKey(nodeId))
                namedLocatables = identifiedLocatables[nodeId];

            bool result = false;
            if (namedLocatables != null)
            {
                DvCodedText codedName = name as DvCodedText;
                if (codedName == null)
                    result = namedLocatables.Contains(name.Value);

                else
                {
                    CodePhrase definingCode = codedName.DefiningCode;
                    result = namedLocatables.Contains(definingCode.TerminologyId.Value, definingCode.CodeString);
                }
            }

            return result;
        }

        public T this[string nodeId, DvText name]
        {
            get
            {
                Check.Invariant(identifiedLocatables != null, "identifiedLocatables must not be null");

                Check.Require(!string.IsNullOrEmpty(nodeId), "nodeId must not be null or empty");
                Check.Require(name != null, "name must not be null");

                Check.Assert(Contains(nodeId, name), "Set must contain item with specified name");  // precondition, but more efficient in release

                LocatableBindingListView<T> namedLocatables = null;
                if (identifiedLocatables.ContainsKey(nodeId))
                    namedLocatables = identifiedLocatables[nodeId];

                T result = null;
                if (namedLocatables != null)
                {
                    DvCodedText codedName = name as DvCodedText;
                    if (codedName == null)
                    {
                        string nameValue = name.Value;
                        if (namedLocatables.Contains(nameValue))
                            result = namedLocatables[nameValue];
                    }
                    else
                    {
                        string nameTerminologyId = codedName.DefiningCode.TerminologyId.Value;
                        string nameCodeString = codedName.DefiningCode.CodeString;
                        if (namedLocatables.Contains(nameTerminologyId, nameCodeString))
                            result = namedLocatables[nameTerminologyId, nameCodeString];
                    }
                }
                Check.Ensure(result != null, "result must not be null");
                return result;
            }
        }

        public override void Clear()
        {
            base.Clear();
            identifiedLocatables.Clear();
        } 
    }
}
