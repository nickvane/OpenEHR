using OpenEhr.DesignByContract;

using OpenEhr.RM.DataTypes.Quantity;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.Attributes;

namespace OpenEhr.RM.Demographic
{
    [RmType("openEHR", "Demographic", "PARTY_RELATIONSHIP")]
    public abstract class PartyRelationship 
        : DemographicLocatable
    {
        protected PartyRelationship(string archetypeNodeId, DvText name, PartyRef target)
            : base(archetypeNodeId, name)
        {
            Target = target;
        }

        protected PartyRelationship() { }

        protected abstract ItemStructure DetailsBase
        {
            get;
            set;
        }

        protected abstract DvInterval<DvDate> TimeValidityBase
        {
            get;
            set;
        }

        protected abstract PartyRef SourceBase
        {
            get;
            //set;
        }

        protected abstract PartyRef TargetBase
        {
            get;
            set;
        }

        #region PARTY_RELATIONSHIP

        public ItemStructure Details
        {
            get { return DetailsBase; }
            set { DetailsBase = value; }
        }

        public DvInterval<DvDate> TimeValidity
        {
            get { return TimeValidityBase; }
            set { TimeValidityBase = value; }
        }

        public PartyRef Source
        {
            get { return SourceBase; }
        }

        public PartyRef Target
        {
            get { return TargetBase; }
            set
            {
                Check.Require(value != null, "Target must not be null");
                TargetBase = value;
            }
        }

        public DvText Type
        {
            get { return this.Name; }
        }

        #endregion
    }
}
