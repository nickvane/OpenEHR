namespace OpenEhr.RM.Common.Directory
{
    public interface IFolder : Common.Archetyped.ILocatable
    {
        AssumedTypes.List<Support.Identification.ObjectRef> Items
        { get;}

        AssumedTypes.List<IFolder> Folders
        { get;}
    }
}
