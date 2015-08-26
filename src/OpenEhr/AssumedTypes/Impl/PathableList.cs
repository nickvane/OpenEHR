using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Archetyped;

namespace OpenEhr.AssumedTypes.Impl
{
    [Serializable]
    public class PathableList<T>
        : OpenEhr.AssumedTypes.List<T> where T : Pathable
    {
        Pathable parent;

        public PathableList()
            : base()
        { }

        internal PathableList(Pathable parent)
            : this()
        {
            Check.Require(parent != null, "parent must not be null");

            this.parent = parent;

            Check.Invariant(this.parent != null, "parent must not be null");
        }

        public PathableList(System.Collections.Generic.IEnumerable<T> items)
            : this()
        {
            if (items != null)
                foreach (T item in items)
                {
                    item.Parent = null;
                    AddItem(item);
                }
        }

        internal PathableList(Pathable parent, System.Collections.Generic.IEnumerable<T> items)
            : this(parent)
        {
            if (items != null)
                foreach (T item in items)
                {
                    item.Parent = null;
                    AddItem(item);
                }

            Check.Invariant(this.parent != null, "parent must not be null");
        }

        public virtual Pathable Parent
        {
            get { return this.parent; }
            internal set
            {
                this.parent = value;
                foreach(Pathable item in this)
                    item.Parent = value;
            }
        }

        protected override void AddItem(T item)
        {
            Pathable pathable = item as Pathable;
            Check.Assert(item != null, "item must not be null");

            if (item.Parent == null)
                item.Parent = this.parent;

            else if (this.parent == null)
                this.parent = item.Parent;

            else if (!Object.ReferenceEquals(item.Parent, this.parent))
                throw new ApplicationException("item parent must have same parent as other items");

            base.AddItem(item);
        }
    }
}