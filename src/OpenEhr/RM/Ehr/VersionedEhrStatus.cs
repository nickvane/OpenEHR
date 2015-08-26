using OpenEhr.RM.Common.ChangeControl;
using OpenEhr.RM.Extract.Common;


namespace OpenEhr.RM.Ehr
{
    public class VersionedEhrStatus
        : VersionedObject<EhrStatus>
    {
        public VersionedEhrStatus()
        { }

        public VersionedEhrStatus(XVersionedObject<EhrStatus> xVersionedObject)
            : base(xVersionedObject)
        { }
    }
}
