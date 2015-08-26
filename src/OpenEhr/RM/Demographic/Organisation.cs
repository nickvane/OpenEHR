using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Attributes;

namespace OpenEhr.RM.Demographic
{
    [RmType("openEHR", "Demographic", "ORGANISATION")]
    public abstract class Organisation : Actor
    {
        protected Organisation()
        { }

        protected Organisation(string archetypeNodeId)
            : base(archetypeNodeId, new DvText("ORGANISATION"))
        { }
    }
}
