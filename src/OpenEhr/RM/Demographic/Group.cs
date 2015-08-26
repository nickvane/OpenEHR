using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Attributes;

namespace OpenEhr.RM.Demographic
{
    [RmType("openEHR", "Demographic", "GROUP")]
    public abstract class Group : Actor
    {
        protected Group(string archetypeNodeId)
            : base(archetypeNodeId, new DvText("GROUP"))
        { }

        protected Group() { }
    }
}
