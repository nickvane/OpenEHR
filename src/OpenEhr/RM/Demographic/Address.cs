using OpenEhr.DesignByContract;

using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.Attributes;

namespace OpenEhr.RM.Demographic
{
    [RmType("openEHR", "Demographic", "ADDRESS")]
    public abstract class Address 
        : DemographicLocatable
    {
        protected Address() 
        { }

        protected Address(string archetypeNodeId, DvText name, ItemStructure details)
            : base(archetypeNodeId, name)
        {
            Details = details;
        }

        protected abstract ItemStructure DetailsBase
        {
            get;
            set;
        }

        #region ADDRESS

        public ItemStructure Details
        {
            get { return DetailsBase; }
            set
            {
                Check.Require(value != null, "Details must not be null");
                DetailsBase = value;
            }
        }

        public DvText Type
        {
            get { return this.Name; }
        }

        #endregion
    }
}
