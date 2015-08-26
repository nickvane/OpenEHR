using OpenEhr.DesignByContract;

using OpenEhr.Attributes;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.DataTypes.Quantity;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.RM.DataTypes.Text;

namespace OpenEhr.RM.Demographic
{
    [RmType("openEHR", "Demographic", "CAPABILITY")]
    public abstract class Capability 
        : DemographicLocatable
    {
        protected Capability(string archetypeNodeId, DvText name, ItemStructure credentials)
            : base(archetypeNodeId, name)
        {
            Credentials = credentials;
        }

        protected Capability() { }

        protected abstract ItemStructure CredentialsBase
        {
            get;
            set;
        }

        protected abstract DvInterval<DvDate> TimeValidityBase
        {
            get;
            set;
        }

        #region CAPABILITY

        public ItemStructure Credentials
        {
            get { return CredentialsBase; }
            set
            {
                Check.Require(value != null, "Credentials must not be null");

                CredentialsBase = value;
            }
        }

        public DvInterval<DvDate> TimeValidity
        {
            get { return TimeValidityBase; }
            set { TimeValidityBase = value; }
        }

        #endregion
    }
}
