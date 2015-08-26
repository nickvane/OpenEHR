using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.RM.DataTypes.Text;

namespace OpenEhr.RM.Demographic.Impl
{
    public class PartyIdentity 
        : OpenEhr.RM.Demographic.PartyIdentity
    {
        public PartyIdentity(string archetypeNodeId, DvText name, ItemStructure details)
            : base(archetypeNodeId, name, details)
        { }

        protected PartyIdentity() { }

        ItemStructure details;

        protected override ItemStructure DetailsBase
        {
            get { return details; }
            set { details = value; }
        }
    }
}
