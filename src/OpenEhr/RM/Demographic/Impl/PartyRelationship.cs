using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.DataTypes.Quantity;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.DesignByContract;

namespace OpenEhr.RM.Demographic.Impl
{
    public class PartyRelationship 
        : OpenEhr.RM.Demographic.PartyRelationship
    {
        public PartyRelationship(string archetypeNodeId, DvText name, PartyRef target)
            : base(archetypeNodeId, name, target)
        { }

        protected PartyRelationship() { }

        ItemStructure details;

        protected override ItemStructure DetailsBase
        {
            get { return details; }
            set { details = value; }
        }

        DvInterval<DvDate> timeValidity;

        protected override DvInterval<DvDate> TimeValidityBase
        {
            get { return timeValidity; }
            set { timeValidity = value; }
        }

        PartyRef source;

        protected override PartyRef SourceBase
        {
            get {

                if (Parent == null)
                    this.source = null;
                else
                {
                    OpenEhr.RM.Demographic.Party party
                        = Parent as OpenEhr.RM.Demographic.Party;

                    Check.Assert(party != null, "parent must be type of Actor or Role");

                    if (party.Uid != null)
                    {
                        string type = OpenEhr.RM.Demographic.Party.GetRmTypeName(party);
                        string @namespace = "local";    // TODO: derive @namespace from parent?

                        this.source = new PartyRef(party.Uid, @namespace, type);
                    }
                }
                return source; 
            }
        }

        PartyRef target;

        protected override PartyRef TargetBase
        {
            get { return target; }
            set { target = value; }
        }
    }
}
