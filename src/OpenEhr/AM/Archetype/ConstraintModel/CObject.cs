using System;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;
using OpenEhr.Factories;
using OpenEhr.Validation;
using OpenEhr.RM.Impl;

namespace OpenEhr.AM.Archetype.ConstraintModel
{
    /// <summary>
    /// Abstract model of constraint on any kind of object node.
    /// </summary>
    [Serializable]
    public abstract class CObject: ArchetypeConstraint
    {
        #region Constructors
        protected CObject(string rmTypeName, string nodeId, AssumedTypes.Interval<int> occurrences, 
            CAttribute parent)
        {
            this.RmTypeName = rmTypeName;
            this.Occurrences = occurrences;
            this.NodeId = nodeId;
            this.Parent = parent;
        }

        protected CObject() { }
        #endregion

        #region Class properties

        internal override ArchetypeConstraint ConstraintParent
        {
            get { return parent; }
        }

        private string rmTypename;
        /// <summary>
        /// Reference model type that this node corresponds to.
        /// </summary>
        public string RmTypeName
        {
            get { return this.rmTypename; }
            set
            {
                Check.Require(!string.IsNullOrEmpty(value),
                    string.Format(CommonStrings.XMustNotBeNullOrEmpty, "RmAttributeName value"));
                this.rmTypename = value;
            }
        }

        private AssumedTypes.Interval<int> occurrences;
        /// <summary>
        /// Occurrences of this object node in the data, under the owning attribute. 
        /// Upper limit can only be greater than 1 if owning attribute has a cardinality of more than 1).
        /// </summary>
        public AssumedTypes.Interval<int> Occurrences
        {
            get { return this.occurrences; }
            set
            {
                Check.Require(value != null,
                    string.Format(CommonStrings.XMustNotBeNull, "Occurrences value"));
                this.occurrences = value;
            }
        }

        private string nodeId;
        /// <summary>
        /// Semantic id of this node, used to differentiate sibling nodes of the same type. 
        /// [Previously called ‘meaning’]. 
        /// Each node_id must be defined in the archetype ontology as a term code.
        /// </summary>
        public string NodeId
        {
            get { return this.nodeId; }
            set
            {
                this.nodeId = value;
            }
        }

        internal virtual string ArchetypeNodeId
        {
            get { return this.nodeId; }
        }

        private CAttribute parent;
        /// <summary>
        /// C_ATTRIBUTE that owns this C_OBJECT.
        /// </summary>
        /// 
        public CAttribute Parent
        {
            get { return this.parent; }
            set { this.parent = value; }
        }

        #endregion

        #region Functions
        public override bool IsSubsetOf(ArchetypeConstraint other)
        {
            throw new Exception(string.Format(
                AmValidationStrings.IsSubsetNotImplementedInX, "CObject"));
        }

        public override bool IsValid()
        {
            return AmValidator.ValidateCObject(this,ValidationContext.TerminologyService);
        }

        protected override string GetPath()
        {
            if (this.Parent == null)
                return "/";

            if (!string.IsNullOrEmpty(this.NodeId) & this.NodeId != "at0000")
                return this.Parent.Path + "[" + this.NodeId + "]";
            else
                return this.Parent.Path;
        }

        string currentNodePath;
        internal string CurrentNodePath
        {
            get
            {
                if (this.currentNodePath == null)
                    this.currentNodePath = this.GetCurrentNodePath();

                return this.currentNodePath;
            }
        }
        protected abstract string GetCurrentNodePath();

        internal bool IsSameRmType(IRmType rmObject)
        {

            return IsSameRmType(this.rmTypename, rmObject);
        }

        public static bool IsSameRmType(string rmTypeName, IRmType rmObject)
        {
            Check.Require(rmObject != null, string.Format(CommonStrings.XMustNotBeNull, "rmObject"));
            Check.Require(!string.IsNullOrEmpty(rmTypeName), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "rmTypeName"));

            string actualTypeName = rmObject.GetRmTypeName();

            if (rmTypeName == actualTypeName)
                return true;

            Type actualRmType = rmObject.GetType();

            while (actualRmType != null && actualRmType != typeof(RmType))
            {
                actualTypeName = RmFactory.GetRmTypeName(actualRmType);

                if (actualTypeName == rmTypeName)
                    return true;

                if (actualTypeName != null)
                    actualRmType = actualRmType.BaseType;
                else
                    actualRmType = null;
            }

            return false;
        }

         #endregion

        #region Validation

        internal override ValidationContext ValidationContext
        {
            get 
            {
                ValidationContext result = ConstraintParent != null ? ConstraintParent.ValidationContext : new ValidationContext();

                Check.Ensure(result != null, string.Format(CommonStrings.XMustNotBeNull, "ValidationContext"));
                return result;
            }
        }

        #endregion

    }
}
