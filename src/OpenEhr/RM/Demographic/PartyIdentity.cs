using OpenEhr.DesignByContract;

using OpenEhr.Attributes;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataStructures.ItemStructure;

namespace OpenEhr.RM.Demographic
{
    [RmType("openEHR", "Demographic", "PARTY_IDENTITY")]
    public abstract class PartyIdentity 
        : DemographicLocatable
    {
        protected PartyIdentity()
        { }

        protected PartyIdentity(string archetypeNodeId, DvText name, ItemStructure details)
            : base(archetypeNodeId, name)
        {
            Details = details;
        }

        protected abstract ItemStructure DetailsBase
        {
            get;
            set;
        }

        #region PARTY_IDENTITY

        public DvText Purpose
        {
            get { return this.Name; }
        }

        public ItemStructure Details
        {
            get { return DetailsBase; }
            set
            {
                Check.Require(value != null, "Details must not be null");
                DetailsBase = value;
            }
        }

        #endregion
    }
}
