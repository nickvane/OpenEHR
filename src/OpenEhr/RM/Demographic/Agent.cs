using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Attributes;

namespace OpenEhr.RM.Demographic
{
    [RmType("openEHR", "Demographic", "AGENT")]
    public abstract class Agent 
        : Actor
    {
        protected Agent(string archetypeNodeId)
            : base(archetypeNodeId, new DvText("AGENT"))
        { }
    }
}
