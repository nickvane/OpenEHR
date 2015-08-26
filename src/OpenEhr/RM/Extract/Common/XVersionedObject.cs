using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.Common.ChangeControl;
using OpenEhr.AssumedTypes;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Extract.Common
{
    [Serializable]
    public class XVersionedObject<T>
        : RmType where T : class
    {
        HierObjectId uid;
        ObjectRef ownerId;
        DvDateTime timeCreated;
        int totalVersionCount;
        List<OriginalVersion<T>> versions;

        protected XVersionedObject()
        { }

        protected XVersionedObject(HierObjectId uid, ObjectRef ownerId, DvDateTime timeCreated,
            int totalVersionCount, List<OriginalVersion<T>> versions) 
            : this(uid, ownerId, timeCreated, totalVersionCount)
        {
            Check.Require(totalVersionCount >= 1, "totalVersionCount must not be less than 1");

            this.versions = versions;
        }

        protected XVersionedObject(HierObjectId uid, ObjectRef ownerId, DvDateTime timeCreated,
            int totalVersionCount)
        {
            Check.Require(uid != null, "uid must not be null");
            Check.Require(ownerId != null, "ownerId must not be null");
            Check.Require(timeCreated != null, "uid must not be null");
            Check.Require(totalVersionCount >= 1, "totalVersionCount must not be less than 1");

            this.uid = uid;
            this.ownerId = ownerId;
            this.timeCreated = timeCreated;
            this.totalVersionCount = totalVersionCount;
        }

        public XVersionedObject(ObjectRef ownerId, DvDateTime timeCreated, int totalVersionCount, 
            OriginalVersion<T>[] versions)
        {
            Check.Require(versions != null, "versions must not be null");
            Check.Require(versions.Length > 0, "versions must not be empty");
            Check.Require(ownerId != null, "ownerId must not be null");
            Check.Require(timeCreated != null, "uid must not be null");
            Check.Require(totalVersionCount >= 1, "totalVersionCount must not be less than 1");


            this.uid = new HierObjectId(versions[0].OwnerId.Value);
            this.ownerId = ownerId;
            this.timeCreated = timeCreated;
            this.totalVersionCount = totalVersionCount;

            foreach (OriginalVersion<T> version in versions)
                this.Add(version);
        }

        /// <summary> Uid of original VERSIONED_OBJECT.
        /// Uid_valid: uid /= Void
        /// </summary>
        public HierObjectId Uid
        {
            get { return this.uid; }
            set { this.uid = value; }
        }

        /// <summary> Owner_id from original VERSIONED_OBJECT, which identifies source EHR.
        /// Owner_id_valid: owner_id /= Void
        /// </summary>
        public ObjectRef OwnerId
        {
            get { return this.ownerId; }
            set { this.ownerId = value; }
        }

        /// <summary> Creation time of original VERSIONED_OBJECT.
        /// Time_created_valid: time_created /= Void
        /// </summary>
        public DvDateTime TimeCreated
        {
            get { return this.timeCreated; }
            set { this.timeCreated = value; }
        }

        /// <summary> Total number of versions in original VERSIONED_OBJECT at time of creation of this
        /// X_VERSIONED_OBJECT
        /// Total_version_count_valid: total_version_count >= 1
        /// </summary>
        public int TotalVersionCount
        {
            get { return this.totalVersionCount; }
            set { this.totalVersionCount = value; }
        }

        /// <summary> The number of Versions in this extract for this Versioned object, i.e. the count of 
        /// items in the versions attribute. May be 0 if only revision history is requested.
        /// Extract_version_count_valid: extract_version_count >= 0
        /// </summary>
        public int ExtractVersionCount
        {
            get {
                if (this.versions != null)
                    return this.versions.Count;
                else
                    return 0;
            }
        }

        /// <summary> 0 or more Versions from the original VERSIONED_OBJECT, according to the Extract
        /// specification.
        /// Versions_valid: versions /= Void implies not versions.is_empty
        /// </summary>
        public List<OriginalVersion<T>> Versions
        {
            get { return this.versions; }
            set { this.versions = value; }
        }

        protected void Add(OriginalVersion<T> version)
        {
            Check.Require(version != null, "version must not be null");
            Check.Require(version.OwnerId.Value == this.Uid.Value, "version ownerID must be equal to uid");

            if (this.versions == null)
                this.versions = new List<OriginalVersion<T>>();

            this.versions.Add(version);
        }
    }
}
