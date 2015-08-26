using System;
using OpenEhr.AssumedTypes;
using OpenEhr.Attributes;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.DesignByContract;

namespace OpenEhr.RM.Common.ChangeControl
{
    [RmType("openEHR", "COMMON", "VERSIONED_OBJECT")]
    public class VersionedObject<T>
        : IVersionedObject<T> where T : class
    {
        private List<Version<T>> versions;

        private ObjectRef ownerId;
        private DvDateTime timeCreated;
        private HierObjectId uid;

        public VersionedObject()
        {
            this.timeCreated = new DvDateTime();
            this.versions = new List<Version<T>>();
        }

        public VersionedObject(ObjectRef ownerId, HierObjectId uid)
            : this()
        {
            Check.Require(ownerId != null, "ownerId must not be null");
            Check.Require(uid != null, "uid must not be null");

            this.ownerId = ownerId;
            this.uid = uid;
        }

        public VersionedObject(Extract.Common.XVersionedObject<T> xVersionedObject)
        {
            this.versions = new List<Version<T>>();
            foreach (OriginalVersion<T> version in xVersionedObject.Versions)
                this.versions.Add(version);

            this.ownerId = xVersionedObject.OwnerId;
            this.timeCreated = xVersionedObject.TimeCreated;
            this.uid = xVersionedObject.Uid;
        }

        public VersionedObject(HierObjectId uid, ObjectRef ownerId, DvDateTime timeCreated, 
            System.Collections.Generic.IEnumerable<Version<T>> versions)
        {
            this.uid = uid;
            this.ownerId = ownerId;
            this.timeCreated = timeCreated;

            versions = new List<Version<T>>(versions);
        }

        public override bool Equals(object obj)
        {
            VersionedObject<T> compareObj = obj as VersionedObject<T>;
            if (compareObj == null)
                return false;

            return this.Uid.Equals(compareObj.Uid);
        }

        protected void Add(Version<T> version)
        {
            Check.Require(version != null, "version must not be null");

            versions.Add(version);
        }

        protected void Remove(Version<T> version)
        {
            Check.Require(version != null, "version must not be null");

            versions.Remove(version);
        }

        public List<Version<T>> AllVersions()
        {            
            return this.versions;
        }

        public Version<T> LatestVersion()
        {
            if (versions.Count > 0)
                return versions[versions.Count - 1];

            throw new InvalidOperationException("No versions in Versioned Object");
        }

        public Version<T> LatestTrunkVersion()
        {
            return LatestVersion();
        }

        public ObjectRef OwnerId
        {
            get
            {
                return this.ownerId;
            }
            set
            {
                this.ownerId = value;
            }
        }

        public DvDateTime TimeCreated
        {
            get
            {
                return this.timeCreated;
            }
            set
            {
                this.timeCreated = value;
            }
        }

        public HierObjectId Uid
        {
            get
            {
                return this.uid;
            }
            set
            {
                this.uid = value;
            }
        }

        public int VersionCount
        {
            get
            {
                return this.versions.Count;
            }
        }

        public List<ObjectVersionId> AllVersionIds()
        {
            List<ObjectVersionId> allVersionIds = new List<ObjectVersionId>();
            foreach (Version<T> eachVersion in versions)
            {
                allVersionIds.Add(eachVersion.Uid);
            }

            if (allVersionIds.Count == 0)
                throw new InvalidOperationException("No versions in Versioned Object");

            return allVersionIds;
        }

        public bool HasVersionAtTime(DvDateTime time)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool HasVersionId(ObjectVersionId uid)
        {
            Check.Require(uid != null && uid.Value != null && uid.Value.Length > 0, "Version Id cannot be null or empty.");

            foreach (Version<T> v in this.versions)
            {
                if (v.Uid.Value == uid.Value)
                    return true;
            }
            return false;
        }

        public Version<T> VersionWithId(ObjectVersionId uid)
        {
            Check.Require(uid != null && uid.Value != null && uid.Value.Length > 0, "Version Id cannot be null or empty.");

            foreach (Version<T> v in this.versions)
            {
                if (v.Uid.Value == uid.Value)
                    return v;
            }
            throw new InvalidOperationException("Version Id is not existing in this versionedObjectProxy instance: " + uid.Value);
        }

        public Version<T> VersionAtTime(DvDateTime time)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public DvCodedText TrunkLifecycleState
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }


        #region IVersionedObject<T> Members

        System.Collections.Generic.List<IVersion<T>> IVersionedObject<T>.AllVersions()
        {
            System.Collections.Generic.List<IVersion<T>> allVersions
                = new System.Collections.Generic.List<IVersion<T>>(this.versions.Count);

            foreach (Version<T> version in versions)
            {
                allVersions.Add(version);
            }
            return allVersions;
        }

        IVersion<T> IVersionedObject<T>.LatestVersion()
        {
            if (versions.Count > 0)
                return versions[versions.Count - 1];

            throw new InvalidOperationException("No versions in Versioned Object");
        }

        System.Collections.Generic.List<ObjectVersionId> IVersionedObject<T>.AllVersionIds()
        {
            System.Collections.Generic.List<ObjectVersionId> allVersionIds
                = new System.Collections.Generic.List<ObjectVersionId>(this.AllVersionIds());

            return allVersionIds;
        }

        IVersion<T> IVersionedObject<T>.VersionWithId(ObjectVersionId uid)
        {
            return VersionWithId(uid);
        }

        IVersion<T> IVersionedObject<T>.VersionAtTime(DvDateTime time)
        {
            return VersionAtTime(time);
        }

        IVersion<T> IVersionedObject<T>.LatestTrunkVersion()
        {
            return LatestVersion();
        }

        #endregion
    }
}
