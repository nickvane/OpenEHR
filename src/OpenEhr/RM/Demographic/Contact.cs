using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.RM.DataTypes.Quantity;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.AssumedTypes.Impl;

namespace OpenEhr.RM.Demographic
{
    [RmType("openEHR", "Demographic", "CONTACT")]
    public abstract class Contact 
        : DemographicLocatable
    {
        protected Contact()
        { }

        protected Contact(string archetypeNodeId, DvText name, OpenEhr.RM.Demographic.Address[] addresses)
            : base(archetypeNodeId, name)
        {
            Addresses = new PathableList<Address>(this, addresses);
        }

        protected abstract DvInterval<DvDate> TimeValidityBase
        {
            get;
            set;
        }

        protected abstract PathableList<Address> AddressesBase
        {
            get;
            set;
        }

        #region CONTACT

        /// <summary>
        /// Valid time interval for this contact descriptor.
        /// </summary>
        public DvInterval<DvDate> TimeValidity
        {
            get { return TimeValidityBase; }
            set { TimeValidityBase = value; }
        }

        /// <summary>
        /// A set of address alternatives for this purpose and time validity.
        /// </summary>
        public OpenEhr.AssumedTypes.List<Address> Addresses
        {
            get { return AddressesBase; }
            set
            {
                Check.Require(value != null, "Addresses must not be null");

                PathableList<Address> pathableList = value as PathableList<Address>;
                Check.Require(pathableList != null, "Addresses must be of type PathableList");

                if (pathableList != null)
                    pathableList.Parent = this;
                AddressesBase = pathableList;
            }
        }

        /// <summary>
        /// Purpose for which this contact is used, e.g. “mail”, “daytime phone”, etc. Taken from value of inherited name attribute.
        /// </summary>
        public DvText Purpose
        {
            get { return this.Name; }
        }

        #endregion
    }
}
