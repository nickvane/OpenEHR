using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.RM.Support.Identification;
using OpenEhr.AssumedTypes.Impl;
using OpenEhr.RM.DataTypes.Quantity;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.DesignByContract;

namespace OpenEhr.RM.Demographic.Impl
{
    public class Role 
        : OpenEhr.RM.Demographic.Role
    {
        public Role(string archetypeNodeId, DvText name, PartyRef performer)
            : base(archetypeNodeId, name, performer)
        { }

        protected Role() { }

        LocatableSet<OpenEhr.RM.Demographic.PartyIdentity> identities;

        protected override LocatableSet<OpenEhr.RM.Demographic.PartyIdentity> IdentitiesBase
        {
            get { return identities; }
            set
            {
                Check.Require(value != null, "Identities must not ben null");
                identities = value;
            }
        }

        LocatableSet<OpenEhr.RM.Demographic.Contact> contacts;

        protected override LocatableSet<OpenEhr.RM.Demographic.Contact> ContactsBase
        {
            get { return contacts; }
            set { contacts = value; }
        }

        LocatableSet<OpenEhr.RM.Demographic.PartyRelationship> relationships;

        protected override LocatableSet<OpenEhr.RM.Demographic.PartyRelationship> RelationshipsBase
        {
            get { return relationships; }
            set { relationships = value; }
        }

        ItemStructure details;

        protected override ItemStructure DetailsBase
        {
            get { return details; }
            set { details = value; }
        }

        OpenEhr.AssumedTypes.Set<PartyRef> reverseRelationships;

        protected override OpenEhr.AssumedTypes.Set<PartyRef> ReverseRelationshipsBase
        {
            get { return reverseRelationships; }
            set { reverseRelationships = value; }
        }

        PartyRef performer;

        protected override PartyRef PerformerBase
        {
            get { return performer; }
            set { performer = value; }
        }

        DvInterval<DvDate> timeValidity;

        protected override DvInterval<DvDate> TimeValidityBase
        {
            get { return timeValidity; }
            set { timeValidity = value; }
        }

        LocatableSet<OpenEhr.RM.Demographic.Capability> capabilities;

        protected override LocatableSet<OpenEhr.RM.Demographic.Capability> CapabilitiesBase
        {
            get { return capabilities; }
            set { capabilities = value; }
        }
    }
}
