using OpenEhr.RM.DataTypes.Quantity;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.AssumedTypes.Impl;

namespace OpenEhr.RM.Demographic.Impl
{
    public class Contact 
        : OpenEhr.RM.Demographic.Contact
    {
        public Contact(string archetypeNodeId, DvText name)
            : base(archetypeNodeId, name, new OpenEhr.RM.Demographic.Address[] { })
        { }

        public Contact(string archetypeNodeId, DvText name, OpenEhr.RM.Demographic.Address[] addresses)
            : base(archetypeNodeId, name, addresses)
        { }

        protected Contact() { }

        PathableList<OpenEhr.RM.Demographic.Address> addresses;

        protected override PathableList<OpenEhr.RM.Demographic.Address> AddressesBase
        {
            get { return addresses; }
            set { addresses = value; }
        }

        DvInterval<DvDate> timeValidity;

        protected override DvInterval<DvDate> TimeValidityBase
        {
            get { return timeValidity; }
            set { timeValidity = value; }
        }
    }
}
