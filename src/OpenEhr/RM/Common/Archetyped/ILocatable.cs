using OpenEhr.AssumedTypes;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Support.Identification;

namespace OpenEhr.RM.Common.Archetyped
{
    public interface ILocatable
    {
        string ArchetypeNodeId
        { get; }

        UidBasedId Uid
        { get; }

        DvText Name
        { get; }

        Pathable Parent
        { get; }

        object ItemAtPath(string path);
        List<object> ItemsAtPath(string path);       
        bool PathExists(string path);
        bool PathUnique(string path);
        string PathOfItem(Pathable item);        
        
        string Concept { get;}
        bool IsArchetypeRoot { get;}
    }
}
