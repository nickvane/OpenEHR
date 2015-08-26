using System;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;
using OpenEhr.Validation;

namespace OpenEhr.AM.Archetype.ConstraintModel
{
    /// <summary>
    /// Archetype equivalent to LOCATABLE class in openEHR Common reference model. 
    /// Defines common constraints for any inheritor of LOCATABLE in any reference model.
    /// </summary>
    [Serializable]
    public abstract class ArchetypeConstraint
    {               
        #region Functions
        /// <summary>
        /// True if constraints represented by other are narrower than this node. Note: not easily
        /// evaluatable for CONSTRAINT_REF nodes.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract bool IsSubsetOf(ArchetypeConstraint other);
        /// <summary>
        /// True if this node (and all its sub-nodes) is a valid archetype node for its type. 
        /// </summary>
        /// <returns></returns>
        public abstract bool IsValid();

        internal abstract ArchetypeConstraint ConstraintParent { get;}


        private string path;
        /// <summary>
        /// Path of this node relative to root of archetype.
        /// </summary>
        public string Path
        {
            get
            {
                if (string.IsNullOrEmpty(this.path))
                    this.path = this.GetPath();

                return this.path;
            }
        }

        protected abstract string GetPath();

        /// <summary>
        /// True if the relative path a_path exists at this node.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool HasPath(string aPath)
        {
            Check.Require(!string.IsNullOrEmpty(aPath), 
                string.Format(CommonStrings.XMustNotBeNullOrEmpty, "aPath"));

            return this.PhysicalPaths.Contains(aPath);
        }

        private System.Collections.Generic.List<string> _physicalPaths;
        internal System.Collections.Generic.List<string> PhysicalPaths
        {
            get { return this._physicalPaths ?? (this._physicalPaths = GetPhysicalPaths()); }
        }
        protected abstract System.Collections.Generic.List<string> GetPhysicalPaths();
        #endregion

        #region Validation

        internal abstract ValidationContext ValidationContext { get; }
 
        public abstract bool ValidValue(object value);

        #endregion
    }
}