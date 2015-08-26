using System;
using OpenEhr.RM.Support.Identification;
using OpenEhr.AssumedTypes;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.Common.Generic;

namespace OpenEhr.RM.Extract.Common
{
    /// <summary>
    /// Generic model of an Extract of some information from a repository; the generic
    /// parameters select which kind of specification and content the Extract carries.
    /// </summary>
    [Serializable]
    public abstract class Extract : ExtractLocatable
    {
        ObjectRef requestId;
        DataTypes.Quantity.DateTime.DvDateTime timeCreated;
        HierObjectId systemId;
        List<Participation> participations;
        int sequenceNr;
        List<ExtractChapter> chapters;

        protected Extract()
        { }

        /// <summary>Extract basic constructor</summary>
        /// <param name="systemId">System ID</param>
        protected Extract(string archetypeNodeId, DataTypes.Text.DvText name, HierObjectId systemId)
            : base(archetypeNodeId, name)
        {
            DesignByContract.Check.Require(systemId != null, "system ID must not be null");

            this.systemId = systemId;

            base.Uid = new HierObjectId(Guid.NewGuid().ToString());
            this.timeCreated = new DataTypes.Quantity.DateTime.DvDateTime(DateTime.Now);
            this.sequenceNr = 1;

            this.chapters = new List<ExtractChapter>();

            // TODO: implement SetAttributeDictionary and CheckInvariant overrides
            SetAttributeDictionary();
            CheckInvariants();
        }

        /// <summary>Identifier of this Extract.</summary>
        /// <remarks>Uid_exists: uid /= Void</remarks>
        public new HierObjectId Uid
        {
            get { return base.Uid as HierObjectId; }
            set { base.Uid = value; }
        }

        ///<summary>Reference to causing Request, if any.</summary>
        ///<remarks>Request_id_valid: request_id /= Void implies request_id.type.is_equal(“EXTRACT_REQUEST”)</remarks>
        public ObjectRef RequestId
        {
            get { return this.requestId; }
            set { this.requestId = value; }
        }

        public DataTypes.Quantity.DateTime.DvDateTime TimeCreated
        {
            get { return this.timeCreated; }
            set { this.timeCreated = value; }
        }

        public HierObjectId SystemId
        {
            get { return this.systemId; }
            set { this.systemId = value; }
        }

        /// <summary>Participations relevant to the creation of this Extract.</summary>
        ///<remarks>Participations_valid: participations /= Void implies not participations.is_empty</remarks> 
        public List<Participation> Participations
        {
            get { return this.participations; }
            set { this.participations = value; }
        }

        
        /// <summary> Number of this Extract response in sequence of responses to Extract request identified by
        /// request_id. If this is the sole response, or there was no request, value is 1.
        /// </summary>
        /// <remarks>Sequence_nr_valid: sequence_nr >= 1</remarks>
        public int SequenceNr
        {
            get { return this.sequenceNr; }
            set { this.sequenceNr = value; }
        }

        
        /// <summary>The content extracted and serialised from the source repository for this Extract.</summary>
        /// <remarks>Chapters_valid: chapters /= Void</remarks>
        public List<ExtractChapter> Chapters
        {
            get { return this.chapters; }
            set { this.chapters = value; }
        }

        // TODO: Specification
    }
}
