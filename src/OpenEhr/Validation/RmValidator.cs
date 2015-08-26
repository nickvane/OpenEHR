using System;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.RM.Composition.Content.Entry;
using OpenEhr.RM.DataTypes.Basic;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.DataTypes.Encapsulated;
using OpenEhr.RM.Common.Generic;
using OpenEhr.RM.DataStructures.ItemStructure.Representation;
using OpenEhr.RM.Support.Terminology.Impl;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.DataTypes.Uri;
using OpenEhr.RM.DataTypes.Quantity;
using OpenEhr.RM.Support.Terminology;
using OpenEhr.Attributes;
using System.Reflection;
using OpenEhr.RM.Common.ChangeControl;
using OpenEhr.RM.Common.Directory;
using OpenEhr.RM.Composition;
using OpenEhr.RM.Composition.Content;
using OpenEhr.RM.Ehr;
using OpenEhr.RM.DataStructures.History;
using OpenEhr.RM.DataStructures;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.Common.Resource;

namespace OpenEhr.Validation
{
    public class RmValidator
    {
        private readonly ITerminologyService _terminologyService;

        protected RmValidator(ITerminologyService terminologyService)
        {
            Check.Require(terminologyService != null, "terminologyService must not be null.");
            _terminologyService = terminologyService;
        }

        private MethodInfo lastMethod = null;
        private object lastObjectRead = null;

        #region ****** Conment parckage ******
        private MethodInfo lastMethodReadValidVersion = null;
        private IVersion lastVersionRead = null;
        private void CallValidate(IVersion version)
        {
            if (version == null) throw new ArgumentNullException("version must not be null.");
            
            const string methodName = "ValidateVersion";

            try
            {
                MethodInfo method = this.GetType().GetMethod(methodName,
                     BindingFlags.NonPublic | BindingFlags.Instance, 
                     Type.DefaultBinder, new Type[] {version.GetType() }, new ParameterModifier[0]);

                if (method != null)
                {
                    // Avoid StackOverflow exceptions by executing only if the method and visitable  
                    // are different from the last parameters used.
                    if (method != lastMethodReadValidVersion || version != lastVersionRead)
                    {
                        lastMethodReadValidVersion = method;
                        lastVersionRead = version;

                        method.Invoke(this, new Object[] {version });

                    }
                    else
                    {
                        string message = "The method '" + methodName + "' with parameter type '"
                            + version.GetType().ToString() + "' is looping and is terminated.";
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = "The method '" + methodName + "' with parameter type '"
                        + version.GetType().ToString() + "' is not implemented.";
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    Exception innerException = ex.InnerException.InnerException;
                    if (innerException != null)
                        throw new ApplicationException(ex.InnerException.Message, innerException);
                    else
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                }
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }


        void ValidateVersion<T>(OriginalVersion<T> version)
            where T : OpenEhr.RM.Common.Archetyped.Impl.Locatable
        {
            // uid
            this.Invariant(version.Uid != null, "version uid must not be null.");
            Validate(version.Uid);
            if (version.Data != null)
                this.Invariant(version.Data.Uid == version.Uid, "version.Uid must equal version.Data.Uid");

            // ownerId
            this.Invariant(version.OwnerId != null, "version ownerId must not be null.");
            Validate(version.OwnerId);
            this.Invariant(version.OwnerId.Value == version.Uid.ObjectId.Value,
                "version ownerId value must be equal to version uid.objectId.value.");

            // commitAudit
            this.Invariant(version.CommitAudit != null, "version commitAudit must not be null.");
            Validate(version.CommitAudit);

            // contribution
            this.Invariant(version.Contribution != null, "version contribution must not be null.");
            // TODO: when the version is not committed, the contribution.Id could be an empty string.
            if (!string.IsNullOrEmpty(version.Contribution.Id.Value))
            {
                Validate(version.Contribution);
                this.Invariant(version.Contribution.Type == "CONTRIBUTION", "version contribution type must be 'CONTRIBUTION'");
            }

            // preceding version uid validity
            this.Invariant(version.Uid.VersionTreeId.TrunkVersion == "1" || version.PrecedingVersionUid != null,
                "either version uid.version_tree_id.is_first or version preceding_version_uid is not null.");
            if (version.PrecedingVersionUid != null)
            {
                this.Invariant(version.Uid.ObjectId.Value == version.PrecedingVersionUid.ObjectId.Value,
                    "version.PrecedingVersionUid and version.Uid must share the same versionedObjectUid value");
                Validate(version.PrecedingVersionUid);
            }

            // lifecycle state
            this.Invariant(version.LifecycleState != null, "version lifecycle state must not be null");
            Validate(version.LifecycleState);
            this.Invariant(version.LifecycleState.DefiningCode.TerminologyId.Value == "openehr",
                "version lifecycle state defining code terminology id value must be 'openehr'");

            //has_code_for_group_id(Group_id_version_lifecycle_state,
            //lifecycle_state.defining_code)
            ValidateRmGroupCode(typeof(OriginalVersion<T>),
                "lifecycle_state", version.LifecycleState.DefiningCode);

            // attestation
            this.Invariant(version.Attestations == null || version.Attestations.Count > 0,
                "if version.Attestations is not null, it must not be empty");
            if (version.Attestations != null)
            {
                foreach (Attestation a in version.Attestations)
                    this.Validate(a);
            }

            this.Invariant(version.OtherInputVersionUids == null || version.OtherInputVersionUids.Count > 0,
                "if version.OtherInputVersionUids is not null, it must not be empty");
            if (version.OtherInputVersionUids != null)
            {
                foreach (ObjectVersionId id in version.OtherInputVersionUids)
                    this.Validate(id);
            }

            if (version.Data != null)
                CallValidate(version.Data);

        }

        protected void Validate(Attestation attestation)
        {
            this.Validate(attestation as AuditDetails);
            this.Invariant(attestation.Reason != null, "attestation.reason must not be null.");

            this.Invariant(attestation.Items == null || attestation.Items.Count > 0,
                "if attestations.items is not null, it must not be empty.");

            if (attestation.AttestedView != null)
                this.Validate(attestation.AttestedView);

            if (attestation.Items != null)
            {
                foreach (DvEhrUri uri in attestation.Items)
                    this.Validate(uri);
            }

            this.Validate(attestation.Reason);

            DvCodedText codedReason = attestation.Reason as DvCodedText;
            if (codedReason != null)
                ValidateRmGroupCode(typeof(Attestation), "reason", codedReason.DefiningCode);

        }

        public static void Validate(IVersion version, ITerminologyService terminologyService)
        {
            RmValidator instance = new RmValidator(terminologyService);
            instance.CallValidate(version);

            // TODO: attestations /= Void implies not attestations.is_empty
            // TODO: Is_merged_validity: other_input_version_ids = Void xor is_merged
            // TODO: Other_input_version_uids_valid: other_input_version_uids /= Void implies not other_input_version_uids.is_empty
        }

        protected void ValidateVersion(OriginalVersion<Composition> version)
        {
            this.ValidateVersion<Composition>(version);
        }

        protected void ValidateVersion(OriginalVersion<Folder> version)
        {
            this.ValidateVersion<Folder>(version);
        }

        protected void ValidateVersion(OriginalVersion<EhrStatus> version)
        {
            this.ValidateVersion<EhrStatus>(version);
        }
       
       private void Validate(PartyProxy partyProxy)
        {
            if (partyProxy == null) throw new ArgumentNullException("partyProxy must not be null.");
           
            const string methodName = "Validate";

            try
            {
                MethodInfo method = this.GetType().GetMethod(methodName,
                    BindingFlags.ExactBinding | BindingFlags.NonPublic
                    | BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { partyProxy.GetType() },
                               new ParameterModifier[0]);

                if (method != null)
                {
                    // Avoid StackOverflow exceptions by executing only if the method and visitable  
                    // are different from the last parameters used.
                    if (method != lastMethod || partyProxy != lastObjectRead)
                    {
                        lastMethod = method;
                        lastObjectRead = partyProxy;

                        method.Invoke(this, new Object[] { partyProxy });

                    }
                    else
                    {
                        string message = "The method '" + methodName + "' with parameter type '"
                            + partyProxy.GetType().ToString() + "' is looping and is terminated.";
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = "The method '" + methodName + "' with parameter type '"
                        + partyProxy.GetType().ToString() + "' is not implemented.";
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    Exception innerException = ex.InnerException.InnerException;
                    if (innerException != null)
                        throw new ApplicationException(ex.InnerException.Message, innerException);
                    else
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                }
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        protected void ValidateBase(PartyProxy partyProxy)
        {
            if (partyProxy.ExternalRef != null)
                this.Validate(partyProxy.ExternalRef);
        }

        protected void Validate(PartyIdentified partyIdentified)
        {
            this.ValidateBase((PartyProxy)partyIdentified);

            this.Invariant(partyIdentified.Name != null || partyIdentified.Identifiers != null ||
               partyIdentified.ExternalRef != null, "partyIdentifier.Name /= Void or partyIdentifier.Identifiers /= Void or partyIdentifier.External_ref /= Void");
            this.Invariant(partyIdentified.Name == null || partyIdentified.Name.Length > 0,
                "When partyIdentified.Name is not null, it must not be empty string.");
            // TODO: need this check
            //this.Invariant(partyIdentified.Identifiers == null || partyIdentified.Identifiers.Count > 0,
            //    "When partyIdentified.Identifiers is not null, it must not be empty.");
            if (partyIdentified.Identifiers != null)
            {
                foreach (DvIdentifier anId in partyIdentified.Identifiers)
                    Validate(anId);
            }
        }

        protected void Validate(PartyRelated partyRelated)
        {
            this.ValidateBase((PartyIdentified)partyRelated);

            this.Invariant(partyRelated.Relationship != null,
                "When partyRelated.Relationship must not be null");
            this.Validate(partyRelated.Relationship);

            ValidateRmGroupCode(typeof(PartyRelated), "relationship", partyRelated.Relationship.DefiningCode);
        }

        protected void Validate(PartySelf partySelf)
        {
            this.ValidateBase((PartyProxy)partySelf);           
        }

        protected void Validate(AuditDetails auditDetails)
        {
            // systemId
            this.Invariant(auditDetails.SystemId != null, "auditDetails systemId must not be null.");
            this.Invariant(auditDetails.SystemId.Length > 0, "auditDetails systemId must not be empty.");

            // committer
            this.Invariant(auditDetails.Committer != null, "auditDetails committer must not be null");
            Validate(auditDetails.Committer);

            // time committed
            this.Invariant(auditDetails.TimeCommitted != null, "auditDetails time committed must not be null.");
            Validate(auditDetails.TimeCommitted);

            // change type
            this.Invariant(auditDetails.ChangeType != null, "auditDetails change type must not be null.");
            Validate(auditDetails.ChangeType);
            this.Invariant(auditDetails.ChangeType.DefiningCode.TerminologyId.Value == "openehr",
                "auditDetails change type terminologyId value must be 'openehr'.");

            //has_code_for_group_id(Group_id_audit_change_type, change_type.defining_code)
            ValidateRmGroupCode(typeof(AuditDetails), "change_type", auditDetails.ChangeType.DefiningCode);
        }

        private void Validate(RevisionHistory revisionHistory)
        {
            Invariant(revisionHistory.Items != null, "revisionHistory.Items must not be null.");

            foreach (RevisionHistoryItem item in revisionHistory.Items)
                Validate(item);
        }

        private void Validate(RevisionHistoryItem revisionHistoryItem)
        {
            Invariant(revisionHistoryItem.Audits != null && revisionHistoryItem.Audits.Count>0, 
                "revisionHistoryItem.Audit must not be null or empty..");
            Invariant(revisionHistoryItem.VersionId != null, "revisionHistoryItem.VersionId must not be null.");

            foreach (AuditDetails eachItem in revisionHistoryItem.Audits)
                Validate(eachItem);

            Validate(revisionHistoryItem.VersionId);
        }

        #endregion

        #region ****** Validate Locatable ******
        protected void ValidateBase(OpenEhr.RM.Common.Archetyped.Impl.Locatable locatable)
        {
            this.Invariant(locatable.Name != null, "name must not be null.");

            Validate(locatable.Name);

            this.Invariant(!string.IsNullOrEmpty(locatable.ArchetypeNodeId),
                "archetypeNodeId must not be null or empty.");

            this.Invariant(locatable.Links == null || locatable.Links.Count > 0,
                "Links_valid: links /= Void implies not links.empty");
            if (locatable.Links != null)
            {
                foreach (OpenEhr.RM.Common.Archetyped.Impl.Link link in locatable.Links)
                    Validate(link);
            }

            if (locatable.ArchetypeDetails != null)
                Validate(locatable.ArchetypeDetails);

            if (locatable.FeederAudit != null)
                Validate(locatable.FeederAudit);

        }

        protected void Validate(OpenEhr.RM.Common.Archetyped.Impl.Link link)
        {
            this.Invariant(link.Meaning != null, "link meaning must not be null.");
            Validate(link.Meaning);

            this.Invariant(link.Type != null, "link type must not be null.");
            Validate(link.Type);

            this.Invariant(link.Target != null, "link target must not be null.");
            Validate(link.Target);
        }

        protected void Validate(OpenEhr.RM.Common.Archetyped.Impl.Archetyped archetyped)
        {
            this.Invariant(archetyped.ArchetypeId != null, "archetyped archetypeId must not be null.");
            Validate(archetyped.ArchetypeId);

            this.Invariant(archetyped.RmVersion != null, "archetyped rmVersion must not be null.");
            this.Invariant(archetyped.RmVersion.Length>0, "archetyped rmVersion must not be empty.");

            if (archetyped.TemplateId != null)
                Validate(archetyped.TemplateId);
        }

        protected void Validate(OpenEhr.RM.Common.Archetyped.Impl.FeederAudit feederAudit)
        {
            this.Invariant(feederAudit.OriginatingSystemAudit != null, "feederAudit.OriginatingSystemAudit must not be null.");
            Validate(feederAudit.OriginatingSystemAudit);

            if (feederAudit.OriginatingSystemItemIds != null)
            {
                foreach (DvIdentifier dvId in feederAudit.OriginatingSystemItemIds)
                    Validate(dvId);
            }

            if (feederAudit.FeederSystemItemIds != null)
            {
                foreach (DvIdentifier dvId in feederAudit.FeederSystemItemIds)
                    Validate(dvId);
            }

            if (feederAudit.OriginalContent != null)
            {
                Validate(feederAudit.OriginalContent);
            }

            if (feederAudit.FeederSystemAudit != null)
                Validate(feederAudit.FeederSystemAudit);
        }

        protected void Validate(OpenEhr.RM.Common.Archetyped.FeederAuditDetails feederAuditDetails)
        {
            this.Invariant(feederAuditDetails.SystemId != null, "feederAuditDetails.SystemId must not be null.");
            this.Invariant(feederAuditDetails.SystemId.Length >0, "feederAuditDetails.SystemId must not be empty.");

            if (feederAuditDetails.Location != null)
                Validate(feederAuditDetails.Location);

            if (feederAuditDetails.Provider != null)
                Validate(feederAuditDetails.Provider);

            if (feederAuditDetails.Subject != null)
                Validate(feederAuditDetails.Subject);

            if (feederAuditDetails.Time != null)
                Validate(feederAuditDetails.Time);
        }

        protected void Validate(Folder folder)
        {
            this.ValidateBase((OpenEhr.RM.Common.Archetyped.Impl.Locatable)folder);

            this.Invariant(folder.Folders == null || folder.Folders.Count > 0,
                "folders /= Void implies not folders.is_empty");

            if (folder.Folders != null)
            {
                foreach (Folder subfolder in folder.Folders)
                    Validate(subfolder);
            }

            if (folder.Items != null)
            {
                foreach (ObjectRef item in folder.Items)
                    CallValidate(item);
            }
        }

        protected void Validate(Composition composition)
        {
            ValidateBase((OpenEhr.RM.Common.Archetyped.Impl.Locatable)composition);

            this.Invariant(composition.Uid != null, "composition.Uid must not be null.");

            this.Invariant(composition.Category != null, "composition.Category must not be null.");
            Validate(composition.Category);

            Type compType = typeof(Composition);
            ValidateRmGroupCode(compType, "category", composition.Category.DefiningCode);

            this.Invariant(composition.Territory != null, "composition.Territory must not be null.");
            this.Validate(composition.Territory);
            // TODO: need to uncomment this validation once territory case difference is resolved
            ValidateRmCodeSet(compType, "territory", composition.Territory);

            this.Invariant(composition.Language != null, "composition.Language must not be null.");
            this.Validate(composition.Language);
            ValidateRmCodeSet(compType, "language", composition.Language);

            this.Invariant(composition.Composer != null, "Composition.Composer must not be null.");
            this.Validate(composition.Composer);
            this.Invariant(composition.IsArchetypeRoot, "composition must be archetype root.");
            this.Invariant(composition.Parent == null, "composition must not have parent.");
            this.Invariant(!composition.IsPersistent() || (composition.IsPersistent() && composition.Context == null),
                "When composition is persistent, the composition.Context must be null.");

            if (composition.Context != null)
                this.Validate(composition.Context);

            if (composition.Content != null)
            {
                foreach (ContentItem anItem in composition.Content)
                    CallValidate(anItem);
            }
            
        }     

        protected void Validate(EhrStatus ehrStatus)
        {
            ValidateBase((OpenEhr.RM.Common.Archetyped.Impl.Locatable)ehrStatus);

            this.Invariant(ehrStatus.Subject != null, "ehrStatus.Subject must not be null.");
            Validate(ehrStatus.Subject);

            this.Invariant(ehrStatus.Parent == null, "ehrStatus.Parent must be null.");

            if (ehrStatus.OtherDetails != null)
                Validate(ehrStatus.OtherDetails);         
        }

        protected void Validate(OpenEhr.RM.Composition.Content.Navigation.Section section)
        {
            this.ValidateBase((OpenEhr.RM.Common.Archetyped.Impl.Locatable)section);

            this.Invariant(section.Items == null || section.Items.Count > 0,
                "If Section items is not null, it must not be empty");

            if (section.Items != null)
            {
                foreach (ContentItem anItem in section.Items)
                    CallValidate(anItem);
            }
        }

        protected void Validate(OpenEhr.RM.Composition.Content.Entry.Entry entry)
        {
            this.ValidateBase(((OpenEhr.RM.Common.Archetyped.Impl.Locatable)entry));

            this.Invariant(entry.Language != null, "entry language must not be null");
            Validate(entry.Language);

            Type entryType = typeof(Entry);
            //Language_valid: language /= Void and then code_set(Code_set_id_languages).has_code(language)            
            ValidateRmCodeSet(entryType, "language", entry.Language);

            this.Invariant(entry.Encoding != null, "entry.Encoding must not be null");
            Validate(entry.Encoding);
            // Encoding_valid: encoding /= Void and then code_set(Code_set_id_character
            // sets).has_code(encoding)
            ValidateRmCodeSet(entryType, "encoding", entry.Encoding);

            this.Invariant(entry.Subject != null, "entry.Subject must not be null");

            // TODO: Subject_validity: subject_is_self implies subject.generating_type =
            //“PARTY_SELF”  

            Validate(entry.Subject);

            this.Invariant(entry.OtherParticipations == null || entry.OtherParticipations.Count > 0,
                "if entry Other_participations is not null, it must not be empty.");

            // validate participations
            if (entry.OtherParticipations != null)
            {
                foreach (Participation p in entry.OtherParticipations)
                {
                    this.Validate(p);
                }
            }

            this.Invariant(entry.IsArchetypeRoot, "entry is_archetype_root must be true.");

            if (entry.Provider != null)
                Validate(entry.Provider);

            if (entry.WorkflowId != null)
                Validate(entry.WorkflowId);
        }

        protected void Validate(CareEntry careEntry)
        {
            this.Validate(((OpenEhr.RM.Composition.Content.Entry.Entry)careEntry));

            if (careEntry.Protocol != null)
                Validate(careEntry.Protocol);
            if (careEntry.GuidelineId != null)
                Validate(careEntry.GuidelineId);
        }

        protected void Validate(AdminEntry adminEntry)
        {
            this.Validate(((OpenEhr.RM.Composition.Content.Entry.Entry)adminEntry));

            this.Invariant(adminEntry.Data != null, "adminEntry.Data must not be null.");
            CallValidate(adminEntry.Data);
        }

        protected void Validate(Event<ItemStructure> @event)
        {
            this.ValidateBase(((OpenEhr.RM.Common.Archetyped.Impl.Locatable)@event));

            this.Invariant(@event.Time != null, "Time must not be null.");
            Validate(@event.Time);
            this.Invariant(@event.Data != null, "Data must not be null.");
            CallValidate(@event.Data);

            if (@event.State != null)
                CallValidate(@event.State);
        }

        protected void Validate(PointEvent<ItemStructure> @event)
        {
            this.Validate(((Event<ItemStructure>)@event));

        }

        protected void Validate(IntervalEvent<ItemStructure> @event)
        {
            this.Validate(((Event<ItemStructure>)@event));

            this.Invariant(@event.Width != null, "intervalEvent.width must not be null.");
            Validate(@event.Width);

            this.Invariant(@event.MathFunction != null, "intervalEvent.MathFunction must not be null.");
            Validate(@event.MathFunction);

            //Math_function_validity: terminology(Terminology_id_openehr).
            //has_code_for_group_id(Group_id_event_math_function, math_function.defining_code)
            ValidateRmGroupCode(typeof(IntervalEvent<ItemStructure>),
                "math_function", @event.MathFunction.DefiningCode);
        }

        protected void Validate(DataStructure dataStructure)
        {
            this.ValidateBase((OpenEhr.RM.Common.Archetyped.Impl.Locatable)dataStructure);
        }      

        protected void Validate(ItemList itemList)
        {
            this.Validate((DataStructure)itemList);

            if (itemList.Items != null)
            {

                foreach (Item item in itemList.Items)
                {
                    this.Invariant(item.GetType() == typeof(Element), "item must be type of element in ItemList");
                    Validate((Element)item);
                }
            }
        }

        protected void Validate(ItemSingle itemSingle)
        {
            this.Validate((DataStructure)itemSingle);

            this.Invariant(itemSingle.Item != null, 
                "ItemSingle.item must be null.");
            Validate(itemSingle.Item);
        }

        protected void Validate(ItemTable itemTable)
        {
            this.Validate((DataStructure)itemTable);

            foreach (Item row in itemTable.Rows)
            {
                this.Invariant(row.GetType() == typeof(Cluster), 
                    "EachRow must be type of Cluster in ItemRow");
                Validate((Cluster)row);
            }
        }

        protected void Validate(ItemTree itemTree)
        {
            this.Validate((DataStructure)itemTree);

            if (itemTree.Items != null)
            {
                foreach (Item item in itemTree.Items)
                {
                    CallValidate(item);
                }
            }
        }

        protected void Validate(Cluster cluster)
        {
            this.ValidateBase((OpenEhr.RM.Common.Archetyped.Impl.Locatable)cluster);
            this.Invariant(cluster.Items != null && cluster.Items.Count > 0,
                "cluster items must not be null or empty.");

            foreach (Item item in cluster.Items)
            {
                CallValidate(item);
            }
        }

        protected void Validate(Element element)
        {
            this.ValidateBase((OpenEhr.RM.Common.Archetyped.Impl.Locatable)element);
            this.Invariant(element.Value == null ^ element.NullFlavour== null,
                "Element ("+element.Name.Value+") value is null xor element nullFlavour is null");

            if (element.Value != null)
                Validate(element.Value);
            if (element.NullFlavour != null)
            {
                Validate(element.NullFlavour);
                ValidateRmGroupCode(typeof(Element), "null_flavour", element.NullFlavour.DefiningCode);
            }
        }

        protected void Validate(Instruction instruction)
        {
            this.Validate(((OpenEhr.RM.Composition.Content.Entry.CareEntry)instruction));

            this.Invariant(instruction.Narrative != null, "instruction data must not be null.");
            Validate(instruction.Narrative);
            this.Invariant(instruction.Activities == null || instruction.Activities.Count > 0,
                "If instruction activities is not null, it must not be empty.");
            if (instruction.Activities != null)
            {
                foreach (Activity anActivity in instruction.Activities)
                {
                    Validate(anActivity);
                }
            }

            if (instruction.ExpiryTime != null)
                Validate(instruction.ExpiryTime);

            if (instruction.WfDefinition != null)
                Validate(instruction.WfDefinition);
        }

        protected void Validate(Observation observation)
        {
            this.Validate(((OpenEhr.RM.Composition.Content.Entry.CareEntry)observation));

            this.Invariant(observation.Data != null, "observation data must not be null.");
            CallValidate(observation.Data);

            if (observation.State != null)
                CallValidate(observation.State);
        }

        protected void Validate(Evaluation evaluation)
        {
            this.Validate(((OpenEhr.RM.Composition.Content.Entry.CareEntry)evaluation));

            this.Invariant(evaluation.Data != null, "evaluation data must not be null.");
            CallValidate(evaluation.Data);
        }

        protected void Validate(OpenEhr.RM.Composition.Content.Entry.Action action)
        {
            this.Validate(((OpenEhr.RM.Composition.Content.Entry.CareEntry)action));

            this.Invariant(action.Time != null, "action.Time must not be null.");
            Validate(action.Time);
           
            this.Invariant(action.Description != null, "action.Description must not be null.");
            CallValidate(action.Description);
            
            this.Invariant(action.IsmTransition != null, "action.IsmTransition must not be null.");
            Validate(action.IsmTransition);

            if (action.InstructionDetails != null)
                Validate(action.InstructionDetails);
        }

        protected void Validate(Activity activity)
        {
            this.ValidateBase(((OpenEhr.RM.Common.Archetyped.Impl.Locatable)activity));

            this.Invariant(activity.Description != null, "activity.Description must not be null.");
            CallValidate(activity.Description);
            
            this.Invariant(activity.Timing != null, "aactivity.Timing must not be null.");
            Validate(activity.Timing);

            this.Invariant(!string.IsNullOrEmpty(activity.ActionArchetypeId),
               "activity.ActionArchetypeId must not be null or empty.");
        }

        protected void Validate(History<ItemStructure> history)
        {
            this.Validate(((OpenEhr.RM.DataStructures.DataStructure)history));

            DvDateTime origin = history.Origin;
            this.Invariant(origin!=null, "history origin must not be null.");
            Validate(origin);

            this.Invariant(history.Events == null || history.Events.Count > 0,
                "When history.Events is not null, it must not be empty.");
            if (history.Events != null)
            {
                foreach (OpenEhr.RM.DataStructures.History.Event<ItemStructure>
                    anEvent in history.Events)
                {
                    CallValidate(anEvent);

                    DvDateTime eventTime = anEvent.Time;
                    this.Invariant(origin <= eventTime, "origin must be less than or equal to an event time.");

                    OpenEhr.RM.DataStructures.History.IntervalEvent<ItemStructure>
                       intervalEvent = anEvent as OpenEhr.RM.DataStructures.History.IntervalEvent<ItemStructure>;
                    if (intervalEvent != null)
                    {
                        DvDateTime intervalStartTime = intervalEvent.IntervalStartTime();
                        this.Invariant(origin <= intervalStartTime, "origin must be less than or equal to intervalStartTime.");
                    }
                }
            }

            if (history.Period != null)
                Validate(history.Period);

            if (history.Duration != null)
                Validate(history.Duration);

            if (history.Summary != null)
                CallValidate(history.Summary);
        }

        public static void Validate(OpenEhr.RM.Common.Archetyped.Impl.Locatable locatable, ITerminologyService terminologyService)
        {
            DesignByContract.Check.Require(locatable != null, "locatable must not be null.");
            Check.Require(terminologyService != null, "terminologyService must not be null.");

            RmValidator instance = new RmValidator(terminologyService);
            instance.CallValidate(locatable);
        }
       
        private void CallValidate(OpenEhr.RM.Common.Archetyped.Impl.Locatable locatable)
        {
            if (locatable == null) throw new ArgumentNullException("locatable");           
            
            const string methodName = "Validate";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] {locatable.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    // Avoid StackOverflow exceptions by executing only if the method and visitable  
                    // are different from the last parameters used.
                    if (method != lastMethod || locatable != lastObjectRead)
                    {
                        lastMethod = method;
                        lastObjectRead = locatable;

                        method.Invoke(this, new Object[] {locatable });

                    }
                    else
                    {
                        string message = "The method '" + methodName + "' with parameter type '"
                            + locatable.GetType().ToString() + "' is looping and is terminated.";
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = "The method '" + methodName + "' with parameter type '"
                        + locatable.GetType().ToString() + "' is not implemented.";
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    Exception innerException = ex.InnerException.InnerException;
                    if (innerException != null)
                        throw new ApplicationException(ex.InnerException.Message, innerException);
                    else
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                }
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }
#endregion

        #region Validate data value

        private void Validate(DataValue dataValue)
        {
            if (dataValue == null) throw new ArgumentNullException("datavalue");

            const string methodName = "Validate";

            try
            {
                System.Reflection.MethodInfo method = this.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.ExactBinding | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { dataValue.GetType() },
                               new System.Reflection.ParameterModifier[0]);

                if (method != null)
                {
                    // Avoid StackOverflow exceptions by executing only if the method and visitable  
                    // are different from the last parameters used.
                    if (method != lastMethod || dataValue != lastObjectRead)
                    {
                        lastMethod = method;
                        lastObjectRead = dataValue;

                        method.Invoke(this, new Object[] { dataValue });

                    }
                    else
                    {
                        string message = "The method '" + methodName + "' with parameter type '"
                            + dataValue.GetType().ToString() + "' is looping and is terminated.";
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = "The method '" + methodName + "' with parameter type '"
                        + dataValue.GetType().ToString() + "' is not implemented.";
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    if (ex.InnerException is ApplicationException && ex.InnerException.InnerException != null
                            && ex.InnerException.Message == ex.InnerException.InnerException.Message)
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException.InnerException);
                    else
                        throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        protected void Validate(OpenEhr.RM.DataTypes.Text.DvText dvText)
        {
            this.Invariant(dvText.Value != null, "dvText.Value must not be null.");
            //dvText value must not be an empty string
            this.Invariant(dvText.Value.Length >0, "dvText value must not be empty");

            this.Invariant(dvText.Mappings == null || dvText.Mappings.Count > 0, 
                "When dvText.Mappings is not null, it must not be empty.");
            if (dvText.Mappings != null)
            {
                foreach (TermMapping mapping in dvText.Mappings)
                    Validate(mapping);
            }

            this.Invariant(dvText.Formatting == null || dvText.Formatting.Length > 0,
                "When dvText.Formatting is not null, it must not be empty.");

            if (dvText.Hyperlink != null)
                Validate(dvText.Hyperlink);

            if (dvText.Language != null)
            {
                Validate(dvText.Language);
                ValidateRmCodeSet(typeof(DvText), "language", dvText.Language);
            }

            if (dvText.Encoding != null)
            {
                Validate(dvText.Encoding);
                ValidateRmCodeSet(typeof(DvText), "encoding", dvText.Encoding);
            }

        }

        protected void Validate(OpenEhr.RM.DataTypes.Text.DvCodedText dvText)
        {
            this.Validate((OpenEhr.RM.DataTypes.Text.DvText)dvText);

            this.Invariant(dvText.DefiningCode != null, "dvCodedText.DefiningCode must not be null.");
            this.Validate(dvText.DefiningCode);
        }

        protected void Validate(OpenEhr.RM.DataTypes.Text.CodePhrase codePhrase)
        {
           this.Invariant(codePhrase.TerminologyId!=null, "codePhrase.TerminologyId must not be null");
           
           Validate(codePhrase.TerminologyId);

           this.Invariant(!string.IsNullOrEmpty(codePhrase.CodeString), "codePhrase.CodeString must not be null or empty.");
        }

        private void ValidateRmGroupCode(Type type, string rmAttributeName, CodePhrase codePhrase)
        {
            Check.Require(type != null, "type must not be null.");
            Check.Require(!string.IsNullOrEmpty(rmAttributeName), "rmAttributeName must not be null or empty.");
            Check.Require(codePhrase != null, "codePhrase must not be null.");

            System.Reflection.PropertyInfo property = GetProperty(type, rmAttributeName);
            this.Invariant(property != null, string.Format("rmAttribute {0} not existing in class {1}",
               rmAttributeName, type.Name));

            RmTerminologyAttribute rmTermAttribute
                        = Attribute.GetCustomAttribute(property, typeof(RmTerminologyAttribute))
                        as RmTerminologyAttribute;

            if (rmTermAttribute == null)
                throw new ApplicationException("rmTermAttribute must not be null for this rmAttribute: " + rmAttributeName);

            string terminologyId = codePhrase.TerminologyId.Value;

			
            this.Invariant(TerminologyService.HasTerminology(terminologyId), "Not valid terminologyId: " + terminologyId);

            this.Invariant(terminologyId == rmTermAttribute.TerminologyId, string.Format("codePhrase.TerminologyId.Value must be {0}, but it is {1}",
                rmTermAttribute.TerminologyId, terminologyId));

            string groupName = rmTermAttribute.GroupName;

			
            ITerminologyAccess termAccess = this.TerminologyService.Terminology(terminologyId);
            this.Invariant(termAccess.HasCodeForGroupId(groupName, codePhrase),
                string.Format("Invalid code string ({0}) within this group ({1})", codePhrase.CodeString, groupName));

        }

        private void ValidateRmCodeSet(Type type, string rmAttributeName, CodePhrase codePhrase)
        {
            Check.Require(type != null, "type must not be null.");
            Check.Require(!string.IsNullOrEmpty(rmAttributeName), "rmAttributeName must not be null or empty.");
            Check.Require(codePhrase != null, "codePhrase must not be null.");

            PropertyInfo property = GetProperty(type, rmAttributeName);
            this.Invariant(property != null, string.Format("rmAttribute {0} not existing in class {1}",
                rmAttributeName, type.Name));

            RmCodesetAttribute rmCodeSetAttribute
                    = Attribute.GetCustomAttribute(property, typeof(RmCodesetAttribute)) as RmCodesetAttribute;
            if (rmCodeSetAttribute == null)
                throw new ApplicationException("Must have RmCodesetAttribute for this rmAttribute: " + rmAttributeName);

            string terminologyId = codePhrase.TerminologyId.Value;
            string rmExternalTermId = rmCodeSetAttribute.ExternalId;

            this.Invariant(terminologyId == rmExternalTermId, string.Format("codePhrase.TerminologyId.Value must be {0}, but it is {1} for attribute ({2})",
               rmExternalTermId, terminologyId, rmAttributeName));

            string codeSetId = rmCodeSetAttribute.CodesetId;


            ICodeSetAccess codeSetAccess = this.TerminologyService.CodeSet(terminologyId);
            this.Invariant(codeSetAccess != null, "terminology not existing: " + terminologyId);

            this.Invariant(codeSetAccess.Id == terminologyId, "terminology id value should be " + codeSetAccess.Id);

            this.Invariant(codeSetAccess.HasCode(codePhrase),
                string.Format("Invalid code string ({0}) within this codeSet ({1})", codePhrase.CodeString, codeSetId));
        }

        protected void Validate(OpenEhr.RM.DataTypes.Text.TermMapping mapping)
        {
            this.Invariant(mapping.Match != null, "Match must not be null");
            this.Invariant(mapping.Target != null, "target must not be null");
            Validate(mapping.Target);

            if (mapping.Purpose != null)
            {
                Validate(mapping.Purpose);
                ValidateRmGroupCode(typeof(TermMapping), "purpose", mapping.Purpose.DefiningCode);
            }
        }

        protected void Validate<T>(DvOrdered<T> dvOrdered) 
            where T : DvOrdered<T>
        {
            this.Invariant(dvOrdered.OtherReferenceRanges == null ||
                    dvOrdered.OtherReferenceRanges.Count > 0, 
                    "When dvOrdered.OtherReferenceRanges is not null, it must not be empty.");

            this.Invariant(!dvOrdered.IsSimple() ^
             (dvOrdered.NormalRange == null && dvOrdered.OtherReferenceRanges == null),
              "When dvOrdered.IsSimple is true, its NormalRange and OtherReferenceRanges must be null.");
            
            //Normal_range_and_status_consistency: (normal_range /= Void and
            //normal_status /= Void) implies (normal_status.code_string.is_equal(“N”) xor
            //not normal_range.has(Current))
            Check.Assert(dvOrdered.NormalRange == null || dvOrdered.NormalStatus == null ||
                (dvOrdered.NormalStatus.CodeString=="N" ^ !dvOrdered.NormalRange.Has(dvOrdered as T)),
                "When dvOrdered.NormalRange is not null and NormalStatus is not null, it implies that "+
                "either the NormalStatus.CodeString must be 'N' or NormalRange.Has(Current) is true.");

            if (dvOrdered.NormalStatus != null)
            {
                Validate(dvOrdered.NormalStatus);

                //Normal_status_validity: normal_status /= Void implies
                //code_set(Code_set_id_normal_statuses).has_code(normal_status)
                ValidateRmCodeSet(typeof(DvOrdered<T>), "normal_status",
                    dvOrdered.NormalStatus);
            }

            if (dvOrdered.NormalRange != null)
            {
                Validate(dvOrdered.NormalRange);                
            }

            if (dvOrdered.OtherReferenceRanges != null)
            {
                foreach (ReferenceRange<T> aRange in dvOrdered.OtherReferenceRanges)
                {
                    Validate(aRange);
                }
            }
        }

        protected void ValidateInterval<T>(OpenEhr.RM.DataTypes.Quantity.DvInterval<T> interval) 
            where T : DvOrdered<T>
        {
            this.Invariant(!interval.LowerUnbounded || !interval.LowerIncluded,
                "If dvInterval is lowerUnbounded, lowerIncluded must be false.");

            this.Invariant(!interval.UpperUnbounded || !interval.UpperIncluded,
                "If dvInterval is UpperUnbounded, UpperIncluded must be false.");

            this.Invariant(interval.LowerUnbounded || interval.UpperUnbounded
                || (interval.Upper > interval.Lower || interval.Upper == interval.Lower),
                "When dvInterval not lowerUnbounded and not upperUnbounded, it implies its upper must be >= lower");
            this.Invariant(interval.LowerUnbounded || interval.UpperUnbounded
                || interval.Lower.IsStrictlyComparableTo(interval.Upper),
                "When dvInterval not lowerUnbounded and not upperUnbounded, it implies its lower.IsStrictlyComparableTo(upper)");

            if(interval.Upper!= null)
                Validate(((DataValue)interval.Upper));

            if(interval.Lower!= null)
                Validate(((DataValue)interval.Lower));
        }

        protected void Validate(OpenEhr.RM.DataTypes.Quantity.DvInterval<DvQuantity> interval)
        {
            ValidateInterval<DvQuantity>(interval);
        }

        protected void Validate(OpenEhr.RM.DataTypes.Quantity.DvInterval<DvCount> interval)
        {
            ValidateInterval<DvCount>(interval);
        }

        protected void Validate(OpenEhr.RM.DataTypes.Quantity.DvInterval<DvProportion> interval)
        {
            ValidateInterval<DvProportion>(interval);
        }

        protected void Validate(OpenEhr.RM.DataTypes.Quantity.DvInterval<DvOrdinal> interval)
        {
            ValidateInterval<DvOrdinal>(interval);
        }

        protected void Validate(OpenEhr.RM.DataTypes.Quantity.DvInterval<DvDate> interval)
        {
            ValidateInterval<DvDate>(interval);
        }

        protected void Validate(OpenEhr.RM.DataTypes.Quantity.DvInterval<DvDateTime> interval)
        {
            ValidateInterval<DvDateTime>(interval);
        }

        protected void Validate(OpenEhr.RM.DataTypes.Quantity.DvInterval<DvTime> interval)
        {
            ValidateInterval<DvTime>(interval);
        }

        protected void Validate(OpenEhr.RM.DataTypes.Quantity.DvInterval<DvDuration> interval)
        {
            ValidateInterval<DvDuration>(interval);
        }

        protected void Validate<T>(OpenEhr.RM.DataTypes.Quantity.ReferenceRange<T> referenceRange)
            where T : DvOrdered<T>
        {
            this.Invariant(referenceRange.Meaning != null, "referenceRange.Meaning must not be null.");
            Validate(referenceRange.Meaning);
            
            this.Invariant(referenceRange.Range != null, "referenceRange.Range must not be null.");
            Validate(referenceRange.Range);
            
            //TODO: Range_is_simple: (range.lower_unbounded or else range.lower.is_simple) and
            //(range.upper_unbounded or else range.upper.is_simple)
        }

        protected void Validate<T>(DvQuantified<T> dvQuantified)
            where T : DvQuantified<T>
        {
            Validate((DvOrdered<T>)dvQuantified);

            this.Invariant(dvQuantified.MagnitudeStatus == null ||
                   DvQuantified<T>.ValidMagnitudeStatus(dvQuantified.MagnitudeStatus),
                   "When dvQuantified.MagnitudeStatus is not null, its MagnitudeStatus must be valid.");

        }

        protected void Validate(DvOrdinal dataValue)
        {
            Validate((DvOrdered<DvOrdinal>)dataValue);

            this.Invariant(dataValue.Symbol != null,
                   "dvOrdinal.Symbol must not be null.");
            Validate(dataValue.Symbol);

            // TODO: this.Invariant(dataValue.Limits != null && dataValue.Limits.Meaning.Value == "limits",
            //    "dvOrdinal.Limits is not null and it's meaning value is 'limits'");
           
            //TODO: Reference_range_valid: other_reference_ranges /= Void and then
            //other_reference_ranges.has(limits)
        }

        protected void Validate<T>(DvAmount<T> dataValue)
            where T : DvAmount<T>
        {
            Validate((DvQuantified<T>)dataValue);

            this.Invariant(dataValue.Accuracy !=0 || (dataValue.Accuracy == 0 && !dataValue.AccuracyIsPercent),
                "dvAmount.Accuracy is 0 means dvAmount.AccuracyIsPercent must be false.");

            this.Invariant(!dataValue.AccuracyIsPercent || dataValue.ValidPercentage(),
                "dvAmount.AccuracyIsPercent is true implies ValidPercentage(accuracy) must be true.");
        }

        protected void Validate(DvProportion dataValue)
        {
            Validate<DvProportion>((DvAmount<DvProportion>)dataValue);

            // Type_validity
            this.Invariant(dataValue.ValidProportionKind((int)dataValue.Type), "dvProportion.Type must be valid proportionKind.");

            // Is_integral_validity
            this.Invariant(!dataValue.IsIntegral() ||
                ((Math.Floor(dataValue.Numerator) == dataValue.Numerator)
                 && (Math.Floor(dataValue.Denominator) == dataValue.Denominator)), 
                 "If dvProportion.IsIntegral, it requires dvProportion numerator.floor == numerator"+
                 " and dvProportion denominator.floor == denominator.");

            // Fraction_validity
            this.Invariant((dataValue.Type != OpenEhr.RM.DataTypes.Quantity.ProportionKind.pkFraction &&
            dataValue.Type != OpenEhr.RM.DataTypes.Quantity.ProportionKind.pkIntegerFraction) || dataValue.IsIntegral(), 
            "If dvProportion.Type is pk_fraction or pk_integer_fraction, it implies dvProportion.IsIntegral must be true.");

            // Unitary_validity
            this.Invariant(dataValue.Type != OpenEhr.RM.DataTypes.Quantity.ProportionKind.pkUnitary ||
                dataValue.Denominator == 1, "If dvProportion.Type is pk_unitary, it implies dvProportion.Denominator = 1.");

            // Percent_validity
            this.Invariant(dataValue.Type != OpenEhr.RM.DataTypes.Quantity.ProportionKind.pkPercent ||
                dataValue.Denominator == 100, "id dvProportion.Type = pk_percent, it implies dvProportion.denominator must be 100.");
        }

        protected void Validate(DvQuantity dataValue)
        {
            Validate<DvQuantity>((DvAmount<DvQuantity>)dataValue);

            this.Invariant(dataValue.Units != null, "dvQuantity.Units must not be null.");
        }

        protected void Validate(DvCount dataValue)
        {
            Validate<DvCount>((DvAmount<DvCount>)dataValue);

        }

        protected void Validate(DvDuration dataValue)
        {
            Validate<DvDuration>((DvAmount<DvDuration>)dataValue);

            this.Invariant(AssumedTypes.Iso8601Duration.ValidIso8601Duration(dataValue.Value),
                "dvDuration.Value must be valid iso8601 duration string");

        }

        protected void Validate(DvDateTime dateTime)
        {
            Validate < DvDateTime>((DvQuantified<DvDateTime>)dateTime);

            this.Invariant(AssumedTypes.Iso8601DateTime.ValidIso8601DateTime(dateTime.Value),
                "dvDateTime.Value must be valid iso8601 datetime string");

        }

        protected void Validate(DvDate date)
        {
            Validate < DvDate>((DvQuantified<DvDate>)date);

            this.Invariant(AssumedTypes.Iso8601Date.ValidIso8601Date(date.Value),
                 "dvDate.Value must be valid iso8601 date string");

        }

        protected void Validate(DvTime time)
        {
            Validate < DvTime>((DvQuantified<DvTime>)time);

            this.Invariant(AssumedTypes.Iso8601Time.ValidIso8601Time(time.Value),
                "time.Value must be valid iso8601 time string");

        }

        protected void Validate(DvIdentifier dvIdentifier)
        {
            this.Invariant(dvIdentifier.Issuer != null, "dvIdentifier.Issuer must not be null.");
            this.Invariant(dvIdentifier.Issuer.Length >0, "dvIdentifier.Issuer must not be empty.");

            this.Invariant(dvIdentifier.Assigner != null, "dvIdentifier.Assigner must not be null.");
            this.Invariant(dvIdentifier.Assigner.Length > 0, "dvIdentifier.Assigner must not be empty.");

            this.Invariant(dvIdentifier.Id != null, "dvIdentifier.Id must not be null.");
            this.Invariant(dvIdentifier.Id.Length > 0, "dvIdentifier.Id must not be empty.");

            this.Invariant(dvIdentifier.Type != null, "dvIdentifier.Type must not be null.");
            this.Invariant(dvIdentifier.Type.Length > 0, "dvIdentifier.Type must not be empty.");
        }

        protected void Validate(DvBoolean dvBoolean)
        {
            this.Invariant(dvBoolean.Value != null, "dvBoolean value must not be null.");            
        }

        protected void Validate(DvEncapsulated encapsulated)
        {
            // TODO: DvParsable.Size has not been implemented.

            if (encapsulated.Language != null)
            {
                Validate(encapsulated.Language);
                //Language_valid: language /= Void implies code_set(Code_set_id_languages).has_code(language)
                ValidateRmCodeSet(typeof(DvEncapsulated), "language", encapsulated.Language);
            }
            if (encapsulated.Charset != null)
            {
                Validate(encapsulated.Charset);

                //Charset_valid: charset /= Void implies code_set(Code_set_id_character_sets).has_code(charset)
                ValidateRmCodeSet(typeof(DvEncapsulated), "charset", encapsulated.Charset);
            }
        }

        protected void Validate(DvMultimedia encapsulated)
        {
            Validate((DvEncapsulated)encapsulated);

            //TODO: Not_empty: is_inline or is_external            

            this.Invariant(encapsulated.IntegrityCheck == null || encapsulated.IntegrityCheckAlgorithm != null,
                "encapsulated.Integrity_check_validity: integrity_check /= Void implies integrity_check_algorithm /= Void");

            if (encapsulated.Uri != null)
                Validate(encapsulated.Uri);

            Type multimediaType = typeof(DvMultimedia);

            if (encapsulated.MediaType != null)
            {
                Validate(encapsulated.MediaType);
                //Media_type_validity: media_type /= Void and then code_set(Code_set_id_media_types).has_code(media_type)
                ValidateRmCodeSet(multimediaType, "media_type", encapsulated.MediaType);
            }

            if (encapsulated.CompressionAlgorithm != null)
            {
                Validate(encapsulated.CompressionAlgorithm);

                //Compression_algorithm_validity: compression_algorithm /= Void implies code_set(Code_set_id_compression_algorithms).
                //has_code(compression_algorithm)
                ValidateRmCodeSet(multimediaType, "compression_algorithm", encapsulated.CompressionAlgorithm);
            }

            if (encapsulated.IntegrityCheckAlgorithm != null)
            {
                Validate(encapsulated.IntegrityCheckAlgorithm);

                //Integrity_check_algorithm_validity: integrity_check_algorithm /= Void implies
                //code_set(Code_set_id_integrity_check_algorithms).has_code(integrity_check_algorithm)
                ValidateRmCodeSet(multimediaType, "integrity_check_algorithm", encapsulated.IntegrityCheckAlgorithm);
            }

        }

        protected void Validate(DvParsable encapsulated)
        {
            Validate((DvEncapsulated)encapsulated);

            //TODO: Not_empty: is_inline or is_external           

            this.Invariant(encapsulated.Value != null, "dvParsable.Value must not be null.");
            this.Invariant(encapsulated.Formalism != null, "dvParsable.Formalism must not be null.");
            this.Invariant(encapsulated.Formalism.Length > 0, "dvParsable.Formalism must not be empty.");
        }

        protected void Validate(DvUri uri)
        {
            this.Invariant(uri.Value != null, "uri value must not be null.");
            this.Invariant(uri.Value.Length > 0, "uri value must not be empty.");
        }

        protected void Validate(DvEhrUri uri)
        {
            Validate((DvUri)uri);
        }
        #endregion

        #region Validate Pathable
        protected void Validate(EventContext context)
        {
            this.Invariant(context.StartTime != null, "context.StartTime must not be null.");
            this.Validate(context.StartTime);
            this.Invariant(context.Setting != null, "context.Setting must not be null");
            this.Validate(context.Setting);
            ValidateRmGroupCode(typeof(EventContext), "setting", context.Setting.DefiningCode);

            this.Invariant(context.Participations == null || context.Participations.Count > 0,
               "When context.Participations is not null, it must not be empty.");
            this.Invariant(context.Location == null || context.Location.Length > 0,           
                "When context.Location is not null, it must not be empty.");

            if (context.EndTime != null)
                Validate(context.EndTime);

            if (context.OtherContext != null)
                CallValidate(context.OtherContext);

            if (context.HealthCareFacility != null)
                Validate(context.HealthCareFacility);

            if (context.Participations != null)
            {
                foreach (Participation aParticipation in context.Participations)
                    Validate(aParticipation);
            }
        }

        protected void Validate(Participation participation)
        {
            this.Invariant(participation.Performer != null, "participation.performer must not be null.");
            this.Validate(participation.Performer);

            this.Invariant(participation.Function != null, "participation.function must not be null.");
            this.Validate(participation.Function);
            DvCodedText function = participation.Function as DvCodedText;

            Type participationType = typeof(Participation);
            if (function != null && function.DefiningCode.TerminologyId.Value!="local")
                ValidateRmGroupCode(participationType, "function", function.DefiningCode);

            this.Invariant(participation.Mode != null, "participation.mode must not be null.");
            this.Validate(participation.Mode);
            ValidateRmGroupCode(participationType, "mode", participation.Mode.DefiningCode);

            if (participation.Time != null)
                this.Validate(participation.Time);
        }

        protected void Validate(IsmTransition ismTransition)
        {
            this.Invariant(ismTransition.CurrentState != null, "ismTransition.CurrentState must not be null.");
            Validate(ismTransition.CurrentState);

            Type ismTransitionType = typeof(IsmTransition);
            //terminology(Terminology_id_openehr).has_code_for_group_id
            // (Group_id_instruction_states, current_state.defining_code)
            ValidateRmGroupCode(ismTransitionType, "current_state", ismTransition.CurrentState.DefiningCode);

            //Transition_valid: transition /= Void implies
            //terminology(Terminology_id_openehr).
            //has_code_for_group_id(Group_id_instruction_transitions, transition.
            //defining_code)

            if (ismTransition.Transition != null)
            {
                Validate(ismTransition.Transition);
                ValidateRmGroupCode(ismTransitionType, "transition", ismTransition.Transition.DefiningCode);
            }

            if (ismTransition.CareflowStep != null)
                Validate(ismTransition.CareflowStep);
        }

        protected void Validate(InstructionDetails instructionDetails)
        {
            this.Invariant(instructionDetails.InstructionId != null, 
                "InstructionId must not be null.");
            Validate(instructionDetails.InstructionId);

            this.Invariant(instructionDetails.ActivityId != null 
                && instructionDetails.ActivityId.Length > 0,
                "ActivityId must not be null or empty.");

            if (instructionDetails.WfDetails != null)
                CallValidate(instructionDetails.WfDetails);
        }

        #endregion
        
        #region Validate Identification package

        private void Validate(ObjectId objectId)
        {
            if (objectId == null) throw new ArgumentNullException("locatable");

            const string methodName = "Validate";

            try
            {
                MethodInfo method = this.GetType().GetMethod(methodName,
                    BindingFlags.ExactBinding | BindingFlags.NonPublic
                    | BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { objectId.GetType() },
                               new ParameterModifier[0]);

                if (method != null)
                {
                    // Avoid StackOverflow exceptions by executing only if the method and visitable  
                    // are different from the last parameters used.
                    if (method != lastMethod || !ObjectId.ReferenceEquals(objectId, lastObjectRead))
                    {
                        lastMethod = method;
                        lastObjectRead = objectId;

                        method.Invoke(this, new Object[] { objectId });
                        lastObjectRead = null;
                    }
                    else
                    {
                        string message = "The method '" + methodName + "' with parameter type '"
                            + objectId.GetType().ToString() + "' is looping and is terminated.";
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = "The method '" + methodName + "' with parameter type '"
                        + objectId.GetType().ToString() + "' is not implemented.";
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        protected void ValidateBase(ObjectId objectId)
        {
            // TODO: checking value must not be empty.
            this.Invariant(objectId.Value != null, "objectId.value must not be null.");
        }

        protected void Validate(TerminologyId objectId)
        {
            ValidateBase((ObjectId)objectId);
            
            this.Invariant(objectId.Name!= null && objectId.Name.Length>0, 
                "TerminologyId.Name must not be null or emtpy.");
            this.Invariant(objectId.VersionId != null, "terminologyId.VersionId must not be null.");
        }

        protected void Validate(UidBasedId objectId)
        {
            ValidateBase((ObjectId)objectId);

            Check.Assert(objectId.Root != null, "UidBasedId.Root must not be null");
            Validate(objectId.Root);

            Check.Assert(objectId.Extension != null, "UidBasedId.Extension must not be null");
            Check.Assert(objectId.Extension== "" ^ objectId.HasExtension,
                "When objectId extention is not null, HasExtension must be true.");
        }

        protected void Validate(ObjectVersionId objectId)
        {
            Validate((UidBasedId)objectId);

            this.Invariant(objectId.ObjectId != null, "objectVersionId.ObjectId must not be null.");
            Validate(objectId.ObjectId);

            this.Invariant(objectId.VersionTreeId != null, "objectVersionId.VersionTreeId must not be null.");
            Validate(objectId.VersionTreeId);

            this.Invariant(objectId.CreatingSystemId != null, "objectVersionId.CreatingSystemId must not be null.");
        }

        protected void Validate(VersionTreeId versionTreeId)
        {
            this.Invariant(versionTreeId.Value != null, "versionTreeId.value must not be null.");
            this.Invariant(versionTreeId.Value.Length > 0, "versionTreeId.value must not be empty.");

            this.Invariant(versionTreeId.TrunkVersion != null, "versionTreeId.TrunkVersion must not be null.");
            int trunkVersion = -1;
            this.Invariant(int.TryParse(versionTreeId.TrunkVersion, out trunkVersion), 
                "versionTreeId.TrunkVersion must be integer");
            this.Invariant(trunkVersion >= 1, "versionTreeId.TrunkVersion must be >=1");

            // TODO: uncomment these when BranchNumber is supported.
            //int branchNumber = -1;
            //this.Invariant(versionTreeId.BranchNumber == null ^ 
            //    (int.TryParse(versionTreeId.BranchNumber, out branchNumber) && branchNumber>=1),
            //    "versionTreeId branch number is not null, implies it must be an integer and value is >=1");

            //int branchVersion = -1;
            //this.Invariant(versionTreeId.BranchVersion == null ^ 
            //    (int.TryParse(versionTreeId.BranchVersion, out branchVersion) && branchVersion >= 1),
            //    "versionTreeId branch version is not null, implies it must be an integer and value is >=1");

            //// Branch_valid
            //this.Invariant((versionTreeId.BranchNumber == null && versionTreeId.BranchVersion == null) ^
            //    (versionTreeId.BranchNumber != null && versionTreeId.BranchVersion != null),
            //    "(branch_number = Void and branch_version = Void ) xor (branch_number /= Void and branch_version /= Void )");

            //// Is_branch_validity
            //this.Invariant(versionTreeId.IsBranch() ^ versionTreeId.BranchNumber== null, 
            //    "versionTreeId isBranch() is true, implies it's branchNumber must not be null.");

            // IsFirstValidity
            this.Invariant(!versionTreeId.IsFirst() ^ trunkVersion == 1, 
                "versionTreeId.IsFirst() is false xor trunkVersion is 1.");
        }

        protected void Validate(HierObjectId objectId)
        {
            Validate((UidBasedId)objectId);

        }

        protected void Validate(GenericId objectId)
        {
            ValidateBase((ObjectId)objectId);

            this.Invariant(objectId.Scheme != null && objectId.Scheme.Length > 0,
                "genericId.Scheme must not be null or emtpy.");
        }

        protected void Validate(TemplateId objectId)
        {
            ValidateBase((ObjectId)objectId);

        }

        protected void Validate(ArchetypeId objectId)
        {
            ValidateBase((ObjectId)objectId);
            
            this.Invariant(objectId.QualifiedRmEntity != null && objectId.QualifiedRmEntity.Length > 0,
                "archetypeId.QualifiedRmEntity must not be null or emtpy.");
            this.Invariant(objectId.DomainConcept != null && objectId.DomainConcept.Length > 0,
                "archetypeId.DomainConcept must not be null or emtpy.");
            this.Invariant(objectId.RmOriginator != null && objectId.RmOriginator.Length > 0,
                "archetypeId.RmOriginator must not be null or emtpy.");
            this.Invariant(objectId.RmName != null && objectId.RmName.Length > 0,
               "archetypeId.RmName must not be null or emtpy.");
            this.Invariant(objectId.RmEntity != null && objectId.RmEntity.Length > 0,
               "archetypeId.RmEntity must not be null or emtpy.");
            this.Invariant(objectId.Specialisation == null || objectId.Specialisation.Length > 0,
               "when archetypeId.Specialisation is not null, it must not be emtpy.");
            this.Invariant(objectId.VersionId != null && objectId.VersionId.Length > 0,
               "archetypeId.VersionId must not be null or emtpy.");
        }

        protected void Validate(Uid uid)
        {
            this.Invariant(uid.Value != null, "UID value must not be null.");
            this.Invariant(uid.Value.Length > 0, "UID value must not be empty.");
        }

        private void CallValidate(ObjectRef objectRef)
        {
            if (objectRef == null) throw new ArgumentNullException("locatable");          

            const string methodName = "Validate";

            try
            {
                MethodInfo method = this.GetType().GetMethod(methodName,
                    BindingFlags.ExactBinding | BindingFlags.NonPublic
                    | BindingFlags.Instance, Type.DefaultBinder,
                               new Type[] { objectRef.GetType() },
                               new ParameterModifier[0]);

                if (method != null)
                {
                    // Avoid StackOverflow exceptions by executing only if the method and visitable  
                    // are different from the last parameters used.
                    if (method != lastMethod || objectRef != lastObjectRead)
                    {
                        lastMethod = method;
                        lastObjectRead = objectRef;

                        method.Invoke(this, new Object[] { objectRef });

                    }
                    else
                    {
                        string message = "The method '" + methodName + "' with parameter type '"
                            + objectRef.GetType().ToString() + "' is looping and is terminated.";
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new ApplicationException(message);
                    }
                }
                else
                {
                    string message = "The method '" + methodName + "' with parameter type '"
                        + objectRef.GetType().ToString() + "' is not implemented.";
                    System.Diagnostics.Debug.WriteLine(message);
                    throw new ApplicationException(message);
                }
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw new ApplicationException(ex.InnerException.Message, ex.InnerException);
                else
                    throw new ApplicationException(ex.Message, ex);
            }
        }

        protected void Validate(ObjectRef objectRef)
        {
            this.Invariant(objectRef.Id != null, "objectRef.Id must not be null.");
            Validate(objectRef.Id);

            this.Invariant(!string.IsNullOrEmpty(objectRef.Namespace),
                "objectRef.Namespace must not be null or empty.");
            this.Invariant(!string.IsNullOrEmpty(objectRef.Type),
                "objectRef.Type must not be null or empty.");

        }

        protected void Validate(AccessGroupRef objectRef)
        {
            Validate((ObjectRef)objectRef);

            this.Invariant(objectRef.Type == "ACCESS_GROUP", "objectRef.Type must be ACCESS_GROUP.");

        }

        protected void Validate(PartyRef objectRef)
        {
            Validate((ObjectRef)objectRef);
        }

        protected void Validate(LocatableRef objectRef)
        {
            Validate((ObjectRef)objectRef);

            this.Invariant(objectRef.Path == null ^ objectRef.Path.Length>0,
                "locatableRef.Path is not null implies that is must not be empty.");

        }
        #endregion

        #region Validate Common.Resource classes
        protected void Validate(AuthoredResource authoredResource)
        {
            Invariant(authoredResource.OriginalLanguage != null, "authoredResource.OriginalLanguage must not be null");
            Validate(authoredResource.OriginalLanguage);

            Invariant(authoredResource.IsControlled ^ authoredResource.RevisionHistory == null,
                "authoredResource.IsControlled is true or RevisionHistory is null, cannot be both.");
            if (authoredResource.RevisionHistory != null)
                Validate(authoredResource.RevisionHistory);

            Invariant(authoredResource.Translations == null ^
                (!authoredResource.Translations.IsEmpty() && !authoredResource.Translations.HasKey(authoredResource.OriginalLanguage.CodeString)),
                "if authoredResource.Translations is not null, implies that it must not be empty and it doesn't constains originalLanguage");
            if(authoredResource.Translations != null)
            {
                foreach (string key in authoredResource.Translations.Keys)
                    Validate(authoredResource.Translations.Item(key));
            }

            Invariant(authoredResource.Description == null ^ authoredResource.Translations != null,
                "authoredResource.Description is not null implieds authoredResource.Translations must not be null.");

            if (authoredResource.Description != null)
            {
                Validate(authoredResource.Description);
                foreach(string langugage in authoredResource.Description.Details.Keys) 
                {
                    authoredResource.Translations.HasKey(langugage);
                }
            }

        }

        protected void Validate(TranslationDetails translationDetails)
        {
            Invariant(translationDetails.Language != null, "translationDetails.Languge must not be null.");
            Validate(translationDetails.Language);

            Invariant(translationDetails.Author != null, "translationDetails.Author must not be null.");          

        }

        protected void Validate(ResourceDescription resourceDescription)
        {
            Invariant(resourceDescription.OriginalAuthor != null && !resourceDescription.OriginalAuthor.IsEmpty(), 
                "resourceDescription.OriginalAuthor must not be null or empty.");
            Invariant(!string.IsNullOrEmpty(resourceDescription.LifecycleState), "resourceDescription.LifecycleState must not be null or empty");
            Invariant(resourceDescription.Details != null && !resourceDescription.Details.IsEmpty(),
               "resourceDescription.OriginalAuthor must not be null or empty.");
            foreach (string key in resourceDescription.Details.Keys)
                Validate(resourceDescription.Details.Item(key));

            if (resourceDescription.ParentResource != null)
            {
                Validate(resourceDescription.ParentResource);

                foreach (string key in resourceDescription.Details)
                    Invariant(resourceDescription.ParentResource.LanguagesAvailable().Has(key), 
                        "when resourceDescription.ParentResource is not null, implies resourceDescription.ParentResource.LanguagesAvailable contains each language included in resourceDescription.Details");

                Invariant(resourceDescription.ParentResource.Description == resourceDescription,
                    "when resourceDescription.ParentResource is not null, it implies its parentResource.Description = Current");
            }
        }

        protected void Validate(ResourceDescriptionItem resourceDescriptionItem)
        {
            Invariant(resourceDescriptionItem.Language != null, "resourceDescriptionItem.Language must not be null");
            Validate(resourceDescriptionItem.Language);

            Invariant(!string.IsNullOrEmpty(resourceDescriptionItem.Purpose), "resourceDescriptionItem.Purpose must not be null or empty.");
            Invariant(resourceDescriptionItem.Use == null ^ resourceDescriptionItem.Use != string.Empty,
                "if resourceDescriptionItem.Use is not null, it must not be empty");
            Invariant(resourceDescriptionItem.Misuse == null ^ resourceDescriptionItem.Misuse != string.Empty,
                "if resourceDescriptionItem.Misuse is not null, it must not be empty");
            Invariant(resourceDescriptionItem.Copyright == null || resourceDescriptionItem.Copyright != string.Empty,
                "if resourceDescriptionItem.Copyright is not null, it must not be empty");

        }
        #endregion

        #region Validate AssumedTypes classes
        protected void Validate<T>(AssumedTypes.Interval<T> interval) where T: IComparable
        {
            Invariant(!interval.LowerUnbounded || (interval.LowerUnbounded && !interval.LowerIncluded), "lower_unbounded implies not lower_included");
            Invariant(!interval.UpperUnbounded || ( interval.UpperUnbounded && !interval.UpperIncluded), "upper_unbounded implies not upper_included");
            Invariant((interval.LowerUnbounded || interval.UpperUnbounded) || (interval.Lower.CompareTo(interval.Upper)<=0),
                "(not upper_unbounded and not lower_unbounded) implies lower <= upper");
            Invariant((interval.LowerUnbounded || interval.LowerUnbounded) || (interval.Lower.GetType() == interval.Upper.GetType()),
               "(not upper_unbounded and not lower_unbounded) implies lower.strictly_comparable_to(upper)");
        }
        #endregion

        protected void Invariant(bool assertion, string message)
        {
            if (!assertion)
                throw new RmInvariantException(message);
        }

        protected void Invariant(bool assertion, string message, Exception inner)
        {
            if (!assertion)
                throw new RmInvariantException(message, inner);
        }

        #region Helper functions

        protected ITerminologyService TerminologyService
        {
            get { return _terminologyService; }
        }
      
	
        private PropertyInfo GetProperty(Type type, string rmAttributeName)
        {
            PropertyInfo[] allProperties = type.GetProperties();

            foreach (PropertyInfo property in allProperties)
            {
                RmAttributeAttribute rmAttri 
                    = Attribute.GetCustomAttribute(property, typeof(RmAttributeAttribute)) as RmAttributeAttribute;

                if (rmAttri != null)
                {
                    if (rmAttri.AttributeName == rmAttributeName)
                    {
                        return property;
                    }
                }
            }

            return null;
        }
        #endregion
    }
}
