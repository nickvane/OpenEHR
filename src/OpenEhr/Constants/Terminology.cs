using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEhr.Constants
{
    /// <summary>
    /// Codifies the kinds of changes to data which are recorded in audit trails
    /// </summary>
    public enum AuditChangeType
    {
        /// <summary>Change type was creation.</summary>
        creation = 249,
        /// <summary>Change type was amendment, i.e. correction of the previous version.</summary>
        amendment = 250,
        /// <summary>Change type was update of the previous version.</summary>
        modification = 251,
        /// <summary>Change type was creation synthesis of data due to conversion process, typically a data importer.</summary>
        synthesis = 252,
        /// <summary>Change type was logical deletion.</summary>
        deleted = 523,
        /// <summary>Existing data were attested.</summary>
        attestation = 666,
        /// <summary>Type of change unknown.</summary>
        unknown = 253
    }

    /// <summary>
    /// Codifies lifecycle states of Compositions or other elements of the health record
    /// </summary>
    public enum VersionLifecycleState
    {
        /// <summary>Item is complete at time of committal.</summary>
        complete = 532,
        /// <summary>Item is incomplete at time of committal, in the view of the author. 
        /// Further editing or review needed before its status will be set to “finished”.</summary>
        incomplete = 553,
        /// <summary>Item has been logically deleted.</summary>
        deleted = 523
    }
}
