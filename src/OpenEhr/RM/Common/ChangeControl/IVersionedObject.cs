using System.Collections.Generic;

namespace OpenEhr.RM.Common.ChangeControl
{
    public interface IVersionedObject
    {
        Support.Identification.HierObjectId Uid { get; }

        Support.Identification.ObjectRef OwnerId { get; }

        DataTypes.Quantity.DateTime.DvDateTime TimeCreated { get; }

        int VersionCount { get; }

        // %HYYKA%
        // CM: 22/10/07 
        //List<Support.Identification.ObjectId> AllVersionIds();
        List<Support.Identification.ObjectVersionId> AllVersionIds();

        List<IVersion> AllVersions();

        bool HasVersionAtTime(DataTypes.Quantity.DateTime.DvDateTime time);
        bool HasVersionId(Support.Identification.ObjectVersionId Id);

        IVersion VersionWithId(Support.Identification.ObjectVersionId Id);
        IVersion VersionAtTime(DataTypes.Quantity.DateTime.DvDateTime time);

        IVersion LatestVersion();
        IVersion LatestTrunkVersion();

        DataTypes.Text.DvCodedText TrunkLifecycleState { get; }
    }

    public interface IVersionedObject<T>
    {
        Support.Identification.HierObjectId Uid { get; }

        Support.Identification.ObjectRef OwnerId { get; }

        DataTypes.Quantity.DateTime.DvDateTime TimeCreated { get; }

        int VersionCount { get; }

        // %HYYKA%
        // CM: 22/10/07 
        //List<Support.Identification.ObjectId> AllVersionIds();
        List<Support.Identification.ObjectVersionId> AllVersionIds();

        List<IVersion<T>> AllVersions();

        bool HasVersionAtTime(DataTypes.Quantity.DateTime.DvDateTime time);
        bool HasVersionId(Support.Identification.ObjectVersionId Id);

        IVersion<T> VersionWithId(Support.Identification.ObjectVersionId Id);
        IVersion<T> VersionAtTime(DataTypes.Quantity.DateTime.DvDateTime time);

        IVersion<T> LatestVersion();
        IVersion<T> LatestTrunkVersion();

        DataTypes.Text.DvCodedText TrunkLifecycleState { get; }
    }
}
