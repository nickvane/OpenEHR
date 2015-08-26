using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Attributes;

namespace OpenEhr.RM.Demographic
{
    [RmType("openEHR", "Demographic", "PERSON")]
    public abstract class Person 
        : Actor
    {
        protected Person(string archetypeNodeId)
            : base(archetypeNodeId, new DvText("PERSON"))
        { }

        protected Person()
        { }
    }
}
