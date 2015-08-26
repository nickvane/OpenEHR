using System;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.Common.Generic;
using OpenEhr.Attributes;

namespace OpenEhr.RM.Extract.Common
{
    /// <summary>
    /// Identifier for a single demographic entity or record thereof. Because identification
    /// is poorly standardised and also heavily dependent on existing systems, this class
    /// provides two possibilities for identification: an id for the record, or an id for the
    /// demographic entity. These are not always 1:1 anyway, due to errors that occur in
    /// systems causing duplicate records for a given entity.
    /// </summary>
    [Serializable]
    [RmType("openEHR", "EXTRACT", "EXTRACT_ENTITY_IDENTIFIER")]
    public class ExtractEntityIdentifier
    {
        HierObjectId entityId;
        PartyIdentified subject;

        public ExtractEntityIdentifier()
        { }

        public ExtractEntityIdentifier(HierObjectId entityId, PartyIdentified subject)
        {
            this.entityId = entityId;
            this.subject = subject;
        }

        /// <summary>Identifies a record for a demographic entity. 
        /// </summary>
        public HierObjectId EntityId
        {
            get { return this.entityId; }
            set { this.entityId = value; }
        }

        /// <summary> Identifies a demographic entity for which there is a record.
        /// </summary>
        public PartyIdentified Subject
        {
            get { return this.subject; }
            set { this.subject = value; }
        }
    }
}
