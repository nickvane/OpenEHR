namespace OpenEhr.RM.Impl
{
    public interface IVisitable
    {
        void Accept(IVisitor visitor);
    }
}
