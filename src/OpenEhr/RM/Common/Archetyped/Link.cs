using System;

using OpenEhr.DesignByContract;

using OpenEhr.RM.DataTypes.Uri;
using OpenEhr.RM.DataTypes.Text;

namespace OpenEhr.RM.Common.Archetyped
{
    [Serializable]
    public abstract class Link
    {
        public abstract DvText Meaning
        {
            get;
            //set;
        }

        public abstract DvText Type
        {
            get;
            //set;
        }

        public abstract DvEhrUri Target
        {
            get;
            set;
        }

        protected void CheckInvariants()
        {
            Check.Invariant(this.Meaning != null);
            Check.Invariant(this.Target != null);
            Check.Invariant(this.Type != null);
        }
    }
}
