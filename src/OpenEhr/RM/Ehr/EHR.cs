using System;

using OpenEhr.DesignByContract;

using OpenEhr.AssumedTypes;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.Attributes;

namespace OpenEhr.RM.Ehr
{
    [Serializable]
    [RmType("openEHR", "EHR", "EHR")]
    public class EHR
    {
        protected EHR() 
        { }

        public EHR(string systemId, HierObjectId ehrId, DvDateTime timeCreated, ObjectRef ehrAccess, ObjectRef ehrStatus, 
            ObjectRef directory, List<ObjectRef> compositions, List<ObjectRef> contributions)
        {
            Check.Require(!string.IsNullOrEmpty(systemId), "systemId must not be null or empty");
            Check.Require(ehrId != null, "ehrId must not be null");
            Check.Require(timeCreated != null, "timeCreated must not be null");
            Check.Require(ehrAccess != null, "ehrAccess must not be null");   
            Check.Require(ehrStatus != null, "ehrStatus must not be null");

            this.systemId = systemId;
            this.ehrId = ehrId;
            this.timeCreated = timeCreated;
            this.ehrAccess = ehrAccess;
            this.ehrStatus = ehrStatus;
            this.directory = directory;
            this.compositions = compositions;
            this.contributions = contributions;
        }

        string systemId;

        public string SystemId
        {
            get { return systemId; }
            set 
            {
                Check.Require(!string.IsNullOrEmpty(value), "SystemId must not be null or empty.");
                systemId = value; 
            }
        }

        HierObjectId ehrId;

        public HierObjectId EhrId
        {
            get { return ehrId; }
            set 
            {
                Check.Require(value != null, "EhrId must not be null.");
                ehrId = value; 
            }
        }

        DvDateTime timeCreated;

        public DvDateTime TimeCreated
        {
            get { return timeCreated; }
            set 
            {
                Check.Require(value != null, "TimeCreated must not be null.");
                timeCreated = value; 
            }
        }

        ObjectRef ehrAccess;

        public ObjectRef EhrAccess
        {
            get { return ehrAccess; }
            set 
            {
                Check.Require(value != null, "EhrAccess must not be null.");
                ehrAccess = value; 
            }
        }

        ObjectRef ehrStatus;

        public ObjectRef EhrStatus
        {
            get { return ehrStatus; }
            set 
            {
                Check.Require(value != null, "EhrStatus must not be null.");
                ehrStatus = value; 
            }
        }

        List<ObjectRef> compositions;

        public List<ObjectRef> Compositions
        {
            get { return compositions; }
            set 
            {
                Check.Require(value != null, "Compositions must not be null.");
                compositions = value; 
            }
        }

        ObjectRef directory;

        public ObjectRef Directory
        {
            get { return directory; }
            set { directory = value; }
        }

        List<ObjectRef> contributions;

        public List<ObjectRef> Contributions
        {
            get { return contributions; }
            set 
            {
                Check.Require(value != null, "Contributions must not be null.");
                contributions = value; 
            }
        }
    }
}
