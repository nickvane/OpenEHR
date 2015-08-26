using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.DataTypes.Quantity;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.DesignByContract;
using OpenEhr.AssumedTypes.Impl;
using OpenEhr.Attributes;

namespace OpenEhr.RM.Demographic
{
    [RmType("openEHR", "Demographic", "ROLE")]
    public abstract class Role 
        : Party
    {
        protected Role(string archetypeNodeId, DvText name, PartyRef performer)
            : base(archetypeNodeId, name)
        {
            Performer = performer;
        }

        protected Role() { }

        protected abstract DvInterval<DvDate> TimeValidityBase
        {
            get;
            set;
        }

        protected abstract PartyRef PerformerBase
        {
            get;
            set;
        }

        protected abstract LocatableSet<Capability> CapabilitiesBase
        {
            get;
            set;
        }

        #region ROLE

        public DvInterval<DvDate> TimeValidity
        {
            get { return TimeValidityBase; }
            set { TimeValidityBase = value; }
        }

        public PartyRef Performer
        {
            get { return PerformerBase; }
            set
            {
                Check.Require(value != null, "Performer must not be null");
                PerformerBase = value;
            }
        }

       public OpenEhr.AssumedTypes.Set<Capability> Capabilities
        {
            get { return CapabilitiesBase; }
            set
            {
                LocatableSet<Capability> locatableSet = value as LocatableSet<Capability>;
                Check.Require(value == null || locatableSet != null, "Capabilities must be of type LocatableSet, use RmFactory.Set");

                if (locatableSet != null)
                    locatableSet.Parent = this;
                CapabilitiesBase = locatableSet;
            }
        }
        #endregion
    }
}
