using System;

using OpenEhr.RM.Extract.Common;
using OpenEhr.RM.Common.ChangeControl;
using OpenEhr.RM.Support.Identification;
using OpenEhr.Factories;

namespace OpenEhr.RM.Common.Directory
{
    [Serializable]
    public class VersionedFolder
        : VersionedObject<Folder>
    {
        static public ObjectRef ObjectRef(VersionedFolder directory, HierObjectId systemId)
        {
            return RmFactory.ObjectRef(directory.Uid, systemId.Value, typeof(Folder));
        }

        public VersionedFolder()
            : base()
        { }

        public VersionedFolder(ObjectRef ownerId, HierObjectId uid)
            : base(ownerId, uid)
        { }

        public VersionedFolder(XVersionedObject<Folder> xVersionedObject)
            : base(xVersionedObject)
        { }
    }
}
