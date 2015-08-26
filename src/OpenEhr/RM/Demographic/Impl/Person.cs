using OpenEhr.AssumedTypes.Impl;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.DesignByContract;

namespace OpenEhr.RM.Demographic.Impl
{
    public class Person
        : OpenEhr.RM.Demographic.Person
    {
        public Person(string archetypeNodeId)
            : base(archetypeNodeId)
        { }

        protected Person() { }

        LocatableSet<OpenEhr.RM.Demographic.PartyIdentity> identities;

        protected override LocatableSet<OpenEhr.RM.Demographic.PartyIdentity> IdentitiesBase
        {
            get { return identities; }
            set
            {
                Check.Require(value != null, "Identities must not be null");
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

        OpenEhr.AssumedTypes.List<PartyRef> roles;

        protected override OpenEhr.AssumedTypes.List<PartyRef> RolesBase
        {
            get { return roles; }
            set { roles = value; }
        }

        OpenEhr.AssumedTypes.List<DvText> languages;

        protected override OpenEhr.AssumedTypes.List<DvText> LanguagesBase
        {
            get { return languages; }
            set { languages = value; }
        }

    }
}
