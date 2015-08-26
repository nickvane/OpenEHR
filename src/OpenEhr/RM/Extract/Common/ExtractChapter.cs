using System;
using OpenEhr.Attributes;
using OpenEhr.RM.Common.Archetyped.Impl;

namespace OpenEhr.RM.Extract.Common
{
    /// <summary>
    /// One content chapter of an Extract; contains information relating to only one entity.
    /// </summary>
    [Serializable]
    [RmType("openEHR", "EXTRACT", "EXTRACT_CHAPTER")]
    public class ExtractChapter: ExtractLocatable
    {
        ExtractEntityContent content;
        ExtractEntityIdentifier entityIdentifier;

        internal ExtractChapter()
        { }

        public ExtractChapter(string archetypeNodeId, DataTypes.Text.DvText name, 
            ExtractEntityIdentifier entityIdentifer, ExtractEntityContent content)
            : base(archetypeNodeId, name)
        {
            // TODO: Set attribute values
            this.entityIdentifier = entityIdentifer;
            this.content = content;

            // TODO: implement SetAttributeDictionary and CheckInvariant overrides
            SetAttributeDictionary();
            CheckInvariants();

            DesignByContract.Check.Ensure(EntityIdentifier != null, "EntityIdentifier must not be null");
            DesignByContract.Check.Ensure(Content != null, "Content must not be null");
        }

        /// <summary> The information content of this chapter. 
        /// Void if the requested entity does not exist in the repository.
        /// Content_valid: content /= Void
        /// </summary>
        public ExtractEntityContent Content
        {
            get { return this.content; }
            set { this.content = value; }
        }

        /// <summary> Reference to causing Request, if any.
        /// Entity_identifier_valid: entity_identifier /= Void
        /// </summary>
        public ExtractEntityIdentifier EntityIdentifier
        {
            get { return this.entityIdentifier; }
            set { this.entityIdentifier = value; }
        }
    }
}
