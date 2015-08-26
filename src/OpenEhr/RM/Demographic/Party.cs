using OpenEhr.DesignByContract;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.AssumedTypes.Impl;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Demographic
{
    public abstract class Party 
        : DemographicLocatable
    {
        public static void SetVersionUid(Party party, ObjectVersionId versionUid)
        {
            Check.Require(party != null, "party must not be null");
            party.VersionUid = versionUid;
        }

        public static ObjectVersionId GetVersionUid(Party party)
        {
            Check.Require(party != null, "party must not be null");
            return party.VersionUid;
        }

        public static PartyRef PartyRef(Party party)
        {
            Check.Require(party != null, "party must not be null");

            PartyRef result = new PartyRef(party.Uid, "local", ((IRmType)party).GetRmTypeName());
            return result;
        }

        protected Party()
        { }

        protected Party(string archetypeNodeId, DvText name)
            : base(archetypeNodeId, name)
        { }

        private ObjectVersionId versionUid;

        protected virtual ObjectVersionId VersionUid
        {
            get { return this.versionUid; }
            set
            {
                Check.Require(value != null, "VersionUid must not be null");
                this.versionUid = value;
            }
        }

        public override int GetHashCode()
        {
            if (this.versionUid != null)
                return this.versionUid.Value.GetHashCode();
            else
                return base.GetHashCode();
        }

        protected abstract ItemStructure DetailsBase
        {
            get;
            set;
        }


        protected abstract OpenEhr.AssumedTypes.Set<PartyRef> ReverseRelationshipsBase
        {
            get;
            set;
        }

        protected abstract LocatableSet<PartyIdentity> IdentitiesBase
        {
            get;
            set;
        }

        protected abstract LocatableSet<Contact> ContactsBase
        {
            get;
            set;
        }

        protected abstract LocatableSet<PartyRelationship> RelationshipsBase
        {
            get;
            set;
        }

        #region PARTY
        
        public new HierObjectId Uid
        {
            get { return base.Uid as HierObjectId; }
            set { base.Uid = value; }
        }

        public ItemStructure Details
        {
            get { return DetailsBase; }
            set { DetailsBase = value; }
        }

        public OpenEhr.AssumedTypes.Set<PartyRef> ReverseRelationships
        {
            get { return ReverseRelationshipsBase; }
        }

        public OpenEhr.AssumedTypes.Set<PartyIdentity> Identities
        {
            get
            {
                if (IdentitiesBase == null)
                    IdentitiesBase = new LocatableSet<PartyIdentity>(this);

                return IdentitiesBase;
            }
            set
            {
                Check.Require(value != null, "Identities must not be null");

                LocatableSet<PartyIdentity> locatableSet = value as LocatableSet<PartyIdentity>;
                Check.Require(locatableSet != null, "Identities must be of type LocatableSet, use RmFactory.Set");

                locatableSet.Parent = this;
                IdentitiesBase = locatableSet;
            }
        }

        public OpenEhr.AssumedTypes.Set<Contact> Contacts
        {
            get
            {
                return ContactsBase;
            }
            set {
                LocatableSet<Contact> locatableSet = value as LocatableSet<Contact>;
                Check.Require(value == null || locatableSet != null, "Contacts must be of type LocatableSet, use RmFactory.Set");

                if (locatableSet != null)
                    locatableSet.Parent = this;
                ContactsBase = locatableSet;
            }
        }

        public OpenEhr.AssumedTypes.Set<PartyRelationship> Relationships
        {
            get
            {
                return RelationshipsBase;
            }
            set {
                LocatableSet<PartyRelationship> locatableSet
                    = value as LocatableSet<PartyRelationship>;
                Check.Require(value == null || locatableSet != null, "Relationships must be of type LocatableSet, use RmFactory.Set");

                if (locatableSet != null)
                    locatableSet.Parent = this;

                RelationshipsBase = locatableSet;
            }
        }

        public DvText Type
        {
            get { return this.Name; }
        }

        #endregion
    }
}
