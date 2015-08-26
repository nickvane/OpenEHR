using System;
using OpenEhr.AssumedTypes;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.DesignByContract;

namespace OpenEhr.RM.Common.Archetyped.Impl
{
    [Serializable]
    public abstract class DemographicLocatable
        : OpenEhr.RM.Common.Archetyped.Impl.ReflectivePathable, ILocatable
    {
        protected DemographicLocatable()
        { }

        protected DemographicLocatable(string archetypeNodeId, DvText name)
        {
            ArchetypeNodeId = archetypeNodeId;
            Name = name;

            CheckInvariants();
        }

        #region LOCATABLE

        public DvText Name
        {
            get { return NameBase; }
            set
            {
                Check.Require(value != null, "Name must not be null");

                NameBase = value;
            }
        }

        public string ArchetypeNodeId
        {
            get { return ArchetypeNodeIdBase; }
            set
            {
                Check.Require(!string.IsNullOrEmpty(value), "ArchetypeNodeId must not be null or empty");

                ArchetypeNodeIdBase = value;
            }
        }

        public UidBasedId Uid
        {
            get { return UidBase; }
            set
            {
                Check.Require(value != null, "Uid must not be null");
                UidBase = value;
            }
        }

        public List<Link> Links
        {
            get { return LinksBase; }
            //set;
        }

        public Archetyped ArchetypeDetails
        {
            get { return ArchetypeDetailsBase; }
            set { ArchetypeDetailsBase = value; }
        }

        public FeederAudit FeederAudit
        {
            get { return FeederAuditBase; }
            set { FeederAuditBase = value; }
        }

        public string Concept
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool IsArchetypeRoot
        {
            get { return ArchetypeId.IsValid(ArchetypeNodeId); }
        }

        #endregion

        DvText name;

        protected virtual DvText NameBase
        {
            get { return name; }
            set { name = value; }
        }

        string archetypeNodeId;

        protected virtual string ArchetypeNodeIdBase
        {
            get { return archetypeNodeId; }
            set { archetypeNodeId = value; }
        }

        UidBasedId uid;

        protected virtual UidBasedId UidBase
        {
            get { return this.uid; }
            set { this.uid = value; }
        }

        List<Link> links;

        protected virtual List<Link> LinksBase
        {
            get { return links; }
            set { links = value; }
        }

        Archetyped archetypeDetails;

        protected virtual Archetyped ArchetypeDetailsBase
        {
            get { return archetypeDetails; }
            set { archetypeDetails = value; }
        }

        FeederAudit feederAudit;

        protected virtual FeederAudit FeederAuditBase
        {
            get { return feederAudit; }
            set { feederAudit = value; }
        }

        void CheckInvariants()
        {
            Check.Ensure(Name != null, "Name must not be null");
            Check.Ensure(!string.IsNullOrEmpty(ArchetypeNodeId), "ArchetypeNodeId must not be null or empty");
        }
    }
}
