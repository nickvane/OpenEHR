using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;
using OpenEhr.Attributes;
using OpenEhr.Factories;
using OpenEhr.Paths;

namespace OpenEhr.AM.Archetype.ConstraintModel
{
    /// <summary>
    /// A constraint defined by proxy, using a reference to an object constraint defined
    /// elsewhere in the same archetype. Note that since this object refers to another node, 
    /// there are two objects with available occurrences values. The local occurrences value on an
    /// ARCHETYPE_INTERNAL_REF should always be used; when setting this from a serialised
    /// form, if no occurrences is mentioned, the target occurrences should be used
    /// (not the standard default of {1..1}); otherwise the locally specified occurrences
    /// should be used as normal. When serialising out, if the occurrences is the same as
    /// that of the target, it can be left out.
    /// </summary>
    [Serializable]
    [AmType("ARCHETYPE_INTERNAL_REF")]
    public class ArchetypeInternalRef : CReferenceObject
    {
        #region Constructors
        public ArchetypeInternalRef(string rmTypeName, string nodeId, AssumedTypes.Interval<int> occurrences,
          CAttribute parent, string targetPath)
            : base(rmTypeName, nodeId, occurrences, parent)
        {
            this.TargetPath = targetPath;
        }

        public ArchetypeInternalRef() { }
        #endregion

        #region Class Properties
        private string targetPath;

        /// <summary>
        /// Reference to an object node using archetype path notation.
        /// </summary>
        public string TargetPath
        {
            get { return targetPath; }
            set
            {
                Check.Require(!string.IsNullOrEmpty(value), 
                    string.Format(CommonStrings.XMustNotBeNullOrEmpty, "TargetPath value"));
                targetPath = value;
            }
        }
        #endregion

        #region Functions
        public override bool IsSubsetOf(ArchetypeConstraint other)
        {
            throw new NotImplementedException(
                string.Format(AmValidationStrings.IsSubsetNotImplementedInX, "ArchetypeInternalRef"));
        }

        public override bool ValidValue(object aValue)
        {
            Check.Require(aValue != null, string.Format(CommonStrings.XMustNotBeNull, "aValue"));

            CComplexObject rootDefinition = AmFactory.GetRootDefinition(this);

            CObject cObjAtTargetPath = Archetype.GetCObjectAtTargetPath(rootDefinition, this.TargetPath);

            return cObjAtTargetPath.ValidValue(aValue);
        }

        protected override System.Collections.Generic.List<string> GetPhysicalPaths()
        {
            CComplexObject rootDefinition = AmFactory.GetRootDefinition(this);

            CObject cObjAtTargetPath = Archetype.GetCObjectAtTargetPath(rootDefinition, this.TargetPath);

            if (cObjAtTargetPath == null)
                throw new ApplicationException(string.Format(
                    AmValidationStrings.NoNodeMatchAtPath, this.TargetPath));

            return cObjAtTargetPath.PhysicalPaths;
        }

        protected override string GetPath()
        {
            Check.Require(string.IsNullOrEmpty(this.NodeId), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "NodeId"));
            Check.Require(this.Parent != null, string.Format(CommonStrings.XMustNotBeNull, "Parent"));

            return this.Parent.Path + this.GetCurrentNodePath();
        }       

        protected override string GetCurrentNodePath()
        {
            Check.Require(string.IsNullOrEmpty(this.NodeId), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "NodeId"));
            Check.Require(this.Parent != null, string.Format(CommonStrings.XMustNotBeNull, "Parent"));

            Path path = new Path(this.TargetPath);
            path.MoveLast();


            if (path.CurrentAttribute != this.Parent.RmAttributeName)
                throw new ApplicationException(
                    string.Format(AmValidationStrings.InternalRefTargetPathFinalPartNameIncorrect, 
                    this.Parent.RmAttributeName));

            return "[" + path.CurrentNodeId + "]";
        }
        #endregion

    }
}