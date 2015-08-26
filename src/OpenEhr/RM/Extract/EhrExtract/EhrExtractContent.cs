using System;
using OpenEhr.RM.Extract.Common;
using OpenEhr.AssumedTypes;
using OpenEhr.Attributes;
using OpenEhr.RM.Common.Archetyped.Impl;

namespace OpenEhr.RM.Extract.EhrExtract
{
    /// <summary> 
    /// Form of EHR Extract content containing openEHR serialised VERSIONED_OBJECTs.
    /// </summary>
    [Serializable]
    [RmType("openEHR", "EXTRACT", "EXTRACT_ENTITY_CONTENT")]
    public class EhrExtractContent : ExtractEntityContent
    {
        XVersionedObject<Ehr.EhrStatus> ehrStatus;
        List<XVersionedObject<Composition.Composition>> compositions;
        List<XVersionedObject<Locatable>> otherItems;

        internal EhrExtractContent()
        { }

        public EhrExtractContent(string archetypeNodeId, DataTypes.Text.DvText name)
            :base(archetypeNodeId, name)
        {
            // TODO: implement SetAttributeDictionary and CheckInvariant overrides
            SetAttributeDictionary();
            CheckInvariants();
        }

        public XVersionedObject<Ehr.EhrStatus> EhrStatus
        {
            get { return this.ehrStatus; }
            set { this.ehrStatus = value; }
        }

        /// <summary> Compositions from source EHR
        /// Compositions_valid: compositions /= Void implies not compositions.is_empty
        /// </summary>
        public List<XVersionedObject<Composition.Composition>> Compositions
        {
            get { return this.compositions; }
            set { this.compositions = value; }
        }

        /// <summary> Other items from source EHR.
        /// Other_items_valid: other_items /= Void implies not other_items.is_empty
        /// </summary>
        public List<XVersionedObject<Locatable>> OtherItems
        {
            get { return this.otherItems; }
            set { this.otherItems = value; }
        }

        /// <summary>
        /// Add versioned composition extract object to compositions collection
        /// </summary>
        /// <param name="versionedComposition"></param>
        public void AddComposition(XVersionedObject<Composition.Composition> versionedComposition)
        {
            DesignByContract.Check.Require(versionedComposition != null, "VersionedComposition must not be null");

            if (this.compositions == null)
                this.compositions = new List<XVersionedObject<Composition.Composition>>();

            this.compositions.Add(versionedComposition);
        }
    }
}
