using OpenEhr.Paths;

namespace OpenEhr.RM.Impl
{
    public interface IRmType
    {
        string GetRmTypeName();

        string GetXmlRmTypeName();

        void SetAttributeValue(string attributeName, object value);
        object GetAttributeValue(string attributeName);

        void BuildPath(Path path);

        AM.Archetype.ConstraintModel.CObject Constraint
        { get; set; }
    }
}
