using System;
using OpenEhr.RM.Support.Identification;
using OpenEhr.DesignByContract;

namespace OpenEhr.Validation
{
    public class FetchOperationalObjectEventArgs : EventArgs
    {
        public FetchOperationalObjectEventArgs(ObjectId id)
        {
            Check.Require(id != null, "id must not be null.");
            this.id = id;
        }

        private ObjectId id;
        public ObjectId Id
        {
            get { return id; }
        }

    }
}
