using OpenEhr.AssumedTypes;

using OpenEhr.RM.Support.Identification;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Common.Generic;
using OpenEhr.RM.Impl;
using OpenEhr.Attributes;
using OpenEhr.Factories;

namespace OpenEhr.RM.Common.ChangeControl
{
    [RmType("openEHR", "COMMON", "CONTRIBUTION")] 
    public class Contribution : RmType
    {
        static public ObjectRef ObjectRef(Contribution contribution, HierObjectId systemId)
        {
            return new ObjectRef(contribution.Uid, systemId.Value, contribution.RmTypeName);
        }

        static public ObjectRef ObjectRef(HierObjectId contributionUid, HierObjectId systemId)
        {
            return new ObjectRef(contributionUid, systemId.Value, RmFactory.GetRmTypeName(typeof(Contribution)));
        }

        public Contribution()
            : this(HierObjectId.NewObjectId())
        {
            Check.Invariant(this.Uid != null, "uid must not be null");
        }

        Contribution(HierObjectId uid)
        {
            Check.Require(uid != null, "uid must not be null");

            this.uid = uid;
        }

        public Contribution(HierObjectId uid, AuditDetails audit)
            : this(uid)
        {
            Check.Require(audit != null, "audit must not be null");

            this.audit = audit;
        }

        HierObjectId uid;

        public HierObjectId Uid
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return uid; }
        }

        Set<ObjectRef> versions = new Set<ObjectRef>();

        public Set<ObjectRef> Versions
        {
            get { return versions; }
        }

        AuditDetails audit;

        public AuditDetails Audit
        {
            get { return audit; }
            set { audit = value; }
        }

        protected void AddVersion(ObjectVersionId uid, HierObjectId ehrId, string rmTypeName)
        {
            ObjectRef version = new ObjectRef(uid, ehrId.Value, rmTypeName);
            versions.Add(version);
        }

        protected void RemoveVersion(ObjectVersionId uid, HierObjectId ehrId, string rmTypeName)
        {
            ObjectRef version = new ObjectRef(uid, ehrId.Value, rmTypeName);
            versions.Remove(version);
        }
    }
}
