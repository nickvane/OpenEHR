using System;
using OpenEhr.DesignByContract;

namespace OpenEhr.RM.Common.Archetyped.Impl
{
    [Serializable]
    public abstract class ExtractLocatable: AttributeDictionaryPathable, ILocatable
    {
        private Archetyped archetypeDetails;
        string archetypeNodeId;
        Support.Identification.UidBasedId uid;
        DataTypes.Text.DvText name;
        private AssumedTypes.List<Link> links;
        private FeederAudit feederAudit;

        #region Attribute Dictionary ...

        /// <summary> Initialises AttributeDictionary using member attributes
        /// Called by Locatable base class constructor with member attribute inital value parameters
        /// Overridden in sub-class, which calls base
        /// </summary>
        protected override void SetAttributeDictionary()
        {
            DesignByContract.Check.Require(this.attributesDictionary != null, "attributeDictionary must not be null.");

            this.attributesDictionary["name"] = this.name;
            this.attributesDictionary["archetype_node_id"] = this.archetypeNodeId;
            this.attributesDictionary["uid"] = this.uid;
            this.attributesDictionary["links"] = this.links;
            this.attributesDictionary["archetype_details"] = this.archetypeDetails;

            this.attributesDictionary["feeder_audit"] = this.feederAudit;
        }

        #endregion

        protected ExtractLocatable()
        { }

        protected ExtractLocatable(string archetypeNodeId, DataTypes.Text.DvText name)
            : this()
        {
            Check.Require(!string.IsNullOrEmpty(archetypeNodeId), "archetype node id must not be null or empty");
            this.archetypeNodeId = archetypeNodeId;

            Check.Require(name != null, "name must not be null");
            Check.Require(!string.IsNullOrEmpty(name.Value), "name value must not be null or empty");
            this.name = name;

            Check.Invariant(attributesDictionary != null, "Attributes diction must not be null");
        }

        /// <summary>
        /// Checks RM Invariants
        /// Called from sub-type constructor
        /// </summary>
        protected virtual void CheckInvariants()
        {
            DesignByContract.Check.Invariant(this.Name != null, "name must not be null.");
            DesignByContract.Check.Invariant(!string.IsNullOrEmpty(this.Name.Value),
                "name value must not be null or empty.");

            DesignByContract.Check.Invariant(!string.IsNullOrEmpty(this.ArchetypeNodeId), "archetypeNodeId must not be null or empty.");
            DesignByContract.Check.Invariant(this.Links == null || this.Links.Count > 0,
                "Links_valid: links /= Void implies not links.empty");
        }

        /// <summary>Design-time archetype id of this node taken from its generating archetype;
        /// used to build archetype paths. Always in the form of an “at” code, e.g. “at0005”.
        /// This value enables a "standardised" name for this node to be generated, by
        /// referring to the generating archetype local ontology.
        /// 
        /// At an archetype root point, the value of this attribute is always the stringified
        /// form of the archetype_id found in the archetype_details object.
        /// </summary>
        public string ArchetypeNodeId
        {
            get { return this.archetypeNodeId; }
            set { this.archetypeNodeId = value; }
        }

        /// <summary>Optional globally unique object identifier
        /// </summary>
        public Support.Identification.UidBasedId Uid
        {
            get { return this.uid; }
            set { this.uid = value; }
        }

        /// <summary>Runtime name of this fragment, used to build runtime paths. This is the term provided
        /// via a clinical application or batch process to name this EHR construct: its
        /// retention in the EHR faithfully preserves the original label by which this entry
        /// was known to end users.
        /// </summary>
        public OpenEhr.RM.DataTypes.Text.DvText Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>Details of archetyping used on this node.
        /// </summary>
        public Archetyped ArchetypeDetails
        {
            get
            {
                return this.archetypeDetails;
            }
            set
            {
                this.archetypeDetails = value;
            }
        }

        /// <summary>Audit trail from non-openEHR system of original commit of information forming
        /// the content of this node, or from a conversion gateway which has synthesised
        /// this node.
        /// </summary>
        public FeederAudit FeederAudit
        {
            get
            {
                return this.feederAudit;
            }
            set
            {
                this.feederAudit = value;
            }
        }

        /// <summary>Links to other archetyped structures (data whose root object inherits from
        /// ARCHETYPED, such as ENTRY, SECTION and so on). Links may be to structures in
        /// other compositions. 
        /// </summary>
        public AssumedTypes.List<Link> Links
        {
            get
            {
                return this.links;
            }
            set
            {
                this.links = value;
            }
        }

        /// <summary>True if this node is the root of an archetyped structure.
        /// </summary>
        public bool IsArchetypeRoot
        {
            get
            {
                if (!this.ArchetypeNodeId.StartsWith("at"))
                    return true;
                else
                    return false;
            }
        }

        #region ILocatable Members ...

        Support.Identification.UidBasedId ILocatable.Uid
        {
            get { return this.uid; }
        }

        /// <summary>Clinical concept of the archetype as a whole 
        /// (= derived from the ‘archetype_node_id’ of the root node)
        /// </summary>
        string ILocatable.Concept
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion
    }
}
