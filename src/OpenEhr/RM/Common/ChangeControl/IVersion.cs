namespace OpenEhr.RM.Common.ChangeControl
{
    public interface IVersion
    {
        Support.Identification.ObjectVersionId Uid
        {
            get;
        }

        Support.Identification.ObjectVersionId PrecedingVersionUid
        {
            get;
        }

        DataTypes.Text.DvCodedText LifecycleState
        {
            get;
        }

        object Data
        {
            get;
        }

        string CanonicalForm { get; }
        string Signature { get; }

        Support.Identification.HierObjectId OwnerId { get; }

        bool IsBranch { get; }

        Support.Identification.ObjectRef Contribution { get; }

        Generic.AuditDetails CommitAudit { get; }
    }

    public interface IVersion<T>
    {
        Support.Identification.ObjectVersionId Uid
        {
            get;
        }

        Support.Identification.ObjectVersionId PrecedingVersionUid
        {
            get;
        }

        DataTypes.Text.DvCodedText LifecycleState
        {
            get;
        }

        T Data
        {
            get;
        }

        string CanonicalForm { get; }
        string Signature { get; }

        Support.Identification.HierObjectId OwnerId { get; }

        bool IsBranch { get; }

        Support.Identification.ObjectRef Contribution { get; }

        Generic.AuditDetails CommitAudit { get; }
    }
}
