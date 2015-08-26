using System;
using System.ComponentModel;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.DesignByContract;

namespace OpenEhr.AssumedTypes.Impl
{

    interface ILocatableList : IList
    {
        Locatable this[string nodeId, string name] { get;}
        object this[string predicate] { get;}

        Locatable this[string nodeId, string nameTerminologyId, string nameCodeString] { get;}

        bool Contains(Locatable locatable);
        bool Contains(string nodeId, string name);
        bool Contains(string nodeId);
        bool Contains(string nodeId, string nameTerminologyId, string nameCodeString);
       
    }
    [Serializable]
    class LocatableList<T> : List<T>, ILocatableList where T:Locatable
        //where T : OpenEhr.RM.Common.Archetyped.Pathable, OpenEhr.RM.Common.Archetyped.ILocatable
    {
        OpenEhr.RM.Common.Archetyped.Pathable parent;

        private System.Collections.Generic.Dictionary<string, NamedLocatableList<T>> identifiedLocatables;

        internal LocatableList()
            : base(new BindingList<T>())
        {
            identifiedLocatables = new System.Collections.Generic.Dictionary<string, NamedLocatableList<T>>();
        }

        internal LocatableList(OpenEhr.RM.Common.Archetyped.Pathable parent, 
            System.Collections.Generic.IEnumerable<T> items) : this()
        {
            Check.Require(parent != null, "parent must not be null");

            this.parent = parent;

            if (items != null)
                foreach (T item in items)
                {
                    item.Parent = null;
                    AddItem(item);
                }

            Check.Invariant(identifiedLocatables != null, "identifiedLocatables must not be null");
        }

        protected override void AddItem(T item)
        {
            Check.Require(this.parent != null || item.Parent != null,
                "item of type Pathable must have Parent attribute set when list parent not set");

            Check.Invariant(identifiedLocatables != null, "identifiedLocatables must not be null");

            Locatable locatable = item as Locatable;
            Check.Assert(item != null, "item must not be null");

            if (item.Parent == null)
                item.Parent = this.parent;

            else if (this.parent == null)
                this.parent = item.Parent;

            else if (!Object.ReferenceEquals(item.Parent, this.parent))
                throw new ApplicationException("item parent must have same parent as other items");

            NamedLocatableList<T> namedLocatables;
            if (identifiedLocatables.ContainsKey(item.ArchetypeNodeId))
                namedLocatables = identifiedLocatables[item.ArchetypeNodeId];
            else
            {
                namedLocatables = new NamedLocatableList<T>();
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

            NamedLocatableList<T> namedLocatables;
            if (identifiedLocatables.ContainsKey(item.ArchetypeNodeId))
            {
                namedLocatables = identifiedLocatables[item.ArchetypeNodeId];
                namedLocatables.Remove(item);

                if (namedLocatables.Count == 0)
                    identifiedLocatables.Remove(item.ArchetypeNodeId);
            }

            base.RemoveItem(item);
        }

        Locatable ILocatableList.this[string nodeId, string name]
        {
            get { return this[nodeId, name]; }
        }

        public T this[string nodeId, string name]
        {
            get
            {
                Check.Invariant(identifiedLocatables != null, "identifiedLocatables must not be null");

                NamedLocatableList<T> namedLocatables = identifiedLocatables[nodeId];
                return namedLocatables[name];
            }
        }

        public T this[string nodeId, string nameTerminologyId, string nameCodeString]
        {
            get
            {
                Check.Invariant(identifiedLocatables != null, "identifiedLocatables must not be null");

                NamedLocatableList<T> namedLocatables = identifiedLocatables[nodeId];
                return namedLocatables[nameTerminologyId, nameCodeString];
            }
        }

        Locatable ILocatableList.this[string nodeId, string nameTerminologyId, string nameCodeString]
        {
            get { return this[nodeId, nameTerminologyId, nameCodeString]; }
        }

        public object this[string predicate]
        {
            get
            {
                Check.Invariant(identifiedLocatables != null, "identifiedLocatables must not be null");

                Check.Require(!String.IsNullOrEmpty(predicate), "predicate must not be null or empty");

                string[] parts = predicate.Split(',');
                Check.Assert(parts.Length > 0, "parts must have at least 1 item");
                Check.Assert(parts.Length < 3, "parts must have no more than 2 items");

                string nodeId = parts[0].Trim();
                if (parts.Length > 1)
                {
                    string name = parts[1].Trim().Trim(new char[] { '\'' });
                    return this[nodeId, name];
                }

                List<T> namedLocatables = identifiedLocatables[nodeId];

                    return namedLocatables;
            }
        }

        bool ILocatableList.Contains(Locatable locatable)
        {
            return this.InnerList.Contains(locatable as T);
        }

        bool ILocatableList.Contains(string nodeId, string name)
        {
            Check.Invariant(identifiedLocatables != null, "identifiedLocatables must not be null");

            if (((ILocatableList)this).Contains(nodeId))
            {
               List<T> namedLocatableList
                = this.identifiedLocatables[nodeId];// as IList;

               for (int i = 0; i < namedLocatableList.Count; i++ )
               {
                   Locatable locatable = namedLocatableList[i];
                   if (locatable.Name.Value == name)
                       return true;
               }
            }
            return false;          
        }

        bool ILocatableList.Contains(string nodeId, string terminologyId, string codeString)
        {
            Check.Invariant(identifiedLocatables != null, "identifiedLocatables must not be null");

            Check.Require(!string.IsNullOrEmpty(nodeId), "nodeId must not be null or empty.");
            Check.Require(!string.IsNullOrEmpty(codeString), "codeString must not be null or empty.");
            
            if (((ILocatableList)this).Contains(nodeId))
            {
                List<T> namedLocatableList
                 = this.identifiedLocatables[nodeId];

                for (int i = 0; i < namedLocatableList.Count; i++)
                {
                    Locatable locatable = namedLocatableList[i];
                    
                    DvCodedText codedName = locatable.Name as DvCodedText;
                    Check.Assert(codedName!= null, "locatable name must be type of DvCodedText.");

                    if (terminologyId != null)
                    {
                        if (codedName.DefiningCode.CodeString == codeString &&
                            codedName.DefiningCode.TerminologyId.Value == terminologyId)
                            return true;
                    }
                    else
                    {
                        if (codedName.DefiningCode.CodeString == codeString)
                            return true;
                    }
                }
            }
            return false;
        }


        bool ILocatableList.Contains(string nodeId)
        {
            Check.Invariant(identifiedLocatables != null, "identifiedLocatables must not be null");

            return this.identifiedLocatables.ContainsKey(nodeId);
        }

        public override void Clear()
        {
            base.Clear();
            identifiedLocatables.Clear();
        }
    }

}
