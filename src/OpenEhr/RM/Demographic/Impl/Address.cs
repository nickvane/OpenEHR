using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.RM.DataTypes.Text;

namespace OpenEhr.RM.Demographic.Impl
{
    public class Address 
        : OpenEhr.RM.Demographic.Address
    {
        public Address(string archetypeNodeId, DvText name, ItemStructure details)
            : base(archetypeNodeId, name, details)
        { }

        protected Address(): base() { }

        ItemStructure details;

        protected override ItemStructure DetailsBase
        {
            get { return details; }
            set { details = value; }
        }
    }
}
