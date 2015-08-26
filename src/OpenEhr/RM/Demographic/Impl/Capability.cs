using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.RM.DataTypes.Quantity;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.DataTypes.Text;

namespace OpenEhr.RM.Demographic.Impl
{
    public class Capability 
        : OpenEhr.RM.Demographic.Capability
    {
        public Capability(string archetypeNodeId, DvText name, ItemStructure credentials)
            : base(archetypeNodeId, name, credentials)
        { }

        protected Capability()
        { }

        ItemStructure credentials;

        protected override ItemStructure CredentialsBase
        {
            get { return credentials; }
            set { credentials = value; }
        }

        DvInterval<DvDate> timeValidity;

        protected override DvInterval<DvDate> TimeValidityBase
        {
            get { return timeValidity; }
            set { timeValidity = value; }
        }
    }
}
