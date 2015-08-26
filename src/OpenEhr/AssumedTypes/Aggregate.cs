using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Common.Archetyped;
using OpenEhr.Paths;
using OpenEhr.RM.Impl;

namespace OpenEhr.AssumedTypes
{
    public interface IAggregate
    {
        // contains an object with attribute values that match the predicate
        bool Has(string predicate);

        // gets object with attribute values that match the predicate
        object GetItem(string nodePredicate);

        void Add(object item);
        void Remove(object item);

        void BuildPath(Path path);

        AM.Archetype.ConstraintModel.CMultipleAttribute Constraint
        { get; set; }
    }

    [Serializable]
    public abstract class Aggregate<T>
        : IAggregate
    {

        public abstract bool Has(T item);

        public abstract int Count
        {
            get;
        }

        public abstract bool IsEmpty();

        public void Add(T item)
        {
            Check.Require(item != null, "item must not be null");

            this.AddItem(item);

            if (this.Constraint != null)
                this.Constraint.ValidValue(this);
        }

        protected abstract void AddItem(T item);

        public void Remove(T item)
        {
            Check.Require(item != null, "item must not be null");

            this.RemoveItem(item);

            if (this.Constraint != null)
                this.Constraint.ValidValue(this);
        }

        public abstract void Clear();

        protected abstract void RemoveItem(T item);

        protected abstract bool Has(string predicate);

        protected abstract T GetItem(string nodePredicate);

        AM.Archetype.ConstraintModel.CMultipleAttribute constraint;

        protected AM.Archetype.ConstraintModel.CMultipleAttribute Constraint
        {
            get { return this.constraint; }
        }

        #region IAggregate Members

        bool IAggregate.Has(string predicate)
        {
            return this.Has(predicate);
        }

        object IAggregate.GetItem(string nodePredicate)
        {
            return GetItem(nodePredicate);
        }

        void IAggregate.Add(object item)
        {
            Check.Require(item != null, "item must not be null");

            T newItem = AsT(item);
            if (newItem == null)
                throw new ApplicationException("Item must be of type T");

            this.Add(newItem);
        }

        void IAggregate.Remove(object item)
        {
            Check.Require(item != null, "item must not be null");

            T tItem = AsT(item);
            if (tItem == null)
                throw new ApplicationException("Item must be of type T");

            this.Remove(tItem);
        }

        T AsT(object value)
        {
            Type tType = typeof(T);
            T t = default(T);

            if (tType.IsAssignableFrom(value.GetType()))
                t = (T)value;

            return t;
        }

        void IAggregate.BuildPath(Path path)
        {
            Check.Require(constraint != null, "constraint must not be null");
            Check.Require(path != null, "path must not be null");
            Check.Require(path.Current != null, "path.Current must not be null");
            Check.Require(path.Current.HasPredicatePath, "Current path step must have predicate");

            T value;

            if (Has(path.Current.PredicatePath))
                value = this.GetItem(path.Current.PredicatePath);
            else
            {
                AM.Archetype.ConstraintModel.CObject objectConstraint
                    = this.Constraint.GetChildObjectConstraint(path.Current.PredicatePath);
                if (objectConstraint == null)
                    throw new ApplicationException("Constrain not found for current node predicate");
                AM.Archetype.ConstraintModel.CDefinedObject definedObjectConstraint
                    = objectConstraint as AM.Archetype.ConstraintModel.CDefinedObject;
                if (definedObjectConstraint == null)
                    throw new ApplicationException("Constrain must be a CDefinedObject");

                value = AsT(definedObjectConstraint.DefaultValue);
                if (value == null)
                    throw new ApplicationException("value must be of type T");

                this.Add(value);
            }

            if (path.NextStep())
            {
                IRmType rmType = value as IRmType;
                if (rmType == null)
                        throw new ApplicationException("expected IRmType");
                else
                    rmType.BuildPath(path);
            }
        }

        OpenEhr.AM.Archetype.ConstraintModel.CMultipleAttribute IAggregate.Constraint
        {
            get { return this.constraint; }
            set { this.constraint = value; }
        }

        #endregion
    }
}
