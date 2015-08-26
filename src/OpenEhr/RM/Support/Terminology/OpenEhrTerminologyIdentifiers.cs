using System.Collections.Generic;

namespace OpenEhr.RM.Support.Terminology
{
    public abstract class OpenEhrTerminologyIdentifiers
    {
        #region OpenehrTerminologyGroupIdentifiers

        public const string TerminologyIdOpenehr = "openehr";
        public const string TerminologyIdMediaTypes = "IANA_media-types";
        public const string GroupIdAuditChangeType = "audit change type";
        public const string GroupIdAttestationReason = "attestation reason";
        public const string GroupIdCompositionCategory = "composition category";
        public const string GroupIdEventMathFunction = "event math function";
        public const string GroupIdInstructionStates = "instruction states";
        public const string GroupIdInstructionTransitions = "instruction transitions";
        public const string GroupIdNullFlavours = "null flavours";
        public const string GroupIdProperty = "property";
        public const string GroupIdParticipationFunction = "participation function";
        public const string GroupIdParticipationMode = "participation mode";
        public const string GroupIdSetting = "setting";
        public const string GroupIdTermMappingPurpose = "term mapping purpose";
        public const string GroupIdSubjectRelationship = "subject relationship";
        public const string GroupIdVersionLifecycleState = "version lifecycle state";

        private List<string> groupIds = new List<string>(new string[] {
            TerminologyIdOpenehr,
            GroupIdAuditChangeType,
            GroupIdAttestationReason,
            GroupIdCompositionCategory,
            GroupIdEventMathFunction,
            GroupIdInstructionStates,
            GroupIdInstructionTransitions,
            GroupIdNullFlavours,
            GroupIdProperty,
            GroupIdParticipationFunction,
            GroupIdParticipationMode,
            GroupIdSetting,
            GroupIdTermMappingPurpose,
            GroupIdSubjectRelationship,
            GroupIdSubjectRelationship,
            GroupIdVersionLifecycleState});

        public bool ValidGroupId(string id)
        {
            return groupIds.Contains(id);
        } 
        #endregion

        #region OpenehrCodeSetIdentifiers


        public const string CodeSetIdCharacterSets = "character sets";
        public const string CodeSetIdCompressionAlgorithms = "compression algorithms";
        public const string CodeSetIdCountries = "countries";
        public const string CodeSetIdIntegrityCheckAlgorithms = "integrity check algorithms";
        public const string CodeSetIdLanguages = "languages";
        public const string CodeSetIdMediaTypes = "media types";
        public const string CodeSetIdNormalStatuses = "normal statuses";

        private List<string> codeSetIds = new List<string>(new string[] {
            CodeSetIdCharacterSets,
            CodeSetIdCompressionAlgorithms,
            CodeSetIdCountries,
            CodeSetIdIntegrityCheckAlgorithms,
            CodeSetIdLanguages,
            CodeSetIdMediaTypes,
            CodeSetIdNormalStatuses });

        public bool ValidCodeSetId(string id)
        {
            return codeSetIds.Contains(id);
        } 
        #endregion
    }
}
