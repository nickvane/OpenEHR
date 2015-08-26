using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Support.Identification;

namespace OpenEhr.RM.Common.Archetyped
{
    [Serializable]
    public abstract class Archetyped
    {
        public abstract ArchetypeId ArchetypeId
        {
            get;
        }

        public abstract string RmVersion
        {
            get;
        }

        public abstract TemplateId TemplateId
        {
            get;
        }

        protected void CheckInvariants()
        {
            Check.Invariant(this.ArchetypeId != null);
            Check.Invariant(!string.IsNullOrEmpty(this.RmVersion));
        }
    }
}
