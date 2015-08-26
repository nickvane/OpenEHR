using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Common.Archetyped;
using OpenEhr.RM.DataTypes.Text;

namespace OpenEhr.AssumedTypes.Impl
{
    [Serializable]
    class LocatableBindingListView<T> : OpenEhr.AssumedTypes.Impl.BindingListView<T> 
        where T : Pathable, ILocatable
    {
        private int Find(string name)
        {
            Check.Require(!string.IsNullOrEmpty(name), "name must not be null or empty");

            for (int i = 0; i < this.Count; i++)
            {
                ILocatable item = this[i] as ILocatable;
                if (item.Name.Value == name)
                    return i;
            }
            return -1; // not found
        }

        public bool Contains(string name)
        {
            int index = this.Find(name);

            if (index < 0)
                return false;
            else
                return true;
        }

        public T this[string name]
        {
            get
            {
                int index = this.Find(name);

                if (index < 0)
                    return null;

                return this[index];
            }
        }

        private int Find(string nameTerminologyId, string nameCodeString)
        {
            Check.Require(!string.IsNullOrEmpty(nameCodeString), "nameCodeString must not be null or empty");

            for (int i = 0; i < this.Count; i++)
            {
                ILocatable item = this[i] as ILocatable;

                DvCodedText codeName = item.Name as DvCodedText;
                Check.Assert(codeName != null, "Item name must be coded name.");

                if (nameTerminologyId != null)
                {
                    if (codeName.DefiningCode.CodeString == nameCodeString &&
                        codeName.DefiningCode.TerminologyId.Value == nameTerminologyId)
                        return i;
                }
                else // assume that the nameTerminologyId is local.
                {
                    if (codeName.DefiningCode.CodeString == nameCodeString)
                        return i;
                }
            }
            return -1; // not found
        }

        public T this[string nameTerminologyId, string nameCodeString]
        {
            get
            {
                int index = this.Find(nameTerminologyId, nameCodeString);

                if (index < 0)
                    return null;

                return this[index];
            }
        }

        public bool Contains(string nameTerminologyId, string nameCodeString)
        {
            int index = this.Find(nameTerminologyId, nameCodeString);

            if (index < 0)
                return false;
            else
                return true;
        }
    }
}
