using System;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;
using OpenEhr.Validation;
using System.Collections.Generic;

namespace OpenEhr.AM.Archetype.ConstraintModel
{
    /// <summary>
    /// Abstract model of constraint on any kind of attribute node.
    /// </summary>   
    [Serializable]
    public abstract class CAttribute : ArchetypeConstraint
    {
        #region Constructors
        protected CAttribute(string rmAttributeName, AssumedTypes.Interval<int> existence,
            AssumedTypes.List<CObject> children)
        {
            this.RmAttributeName = rmAttributeName;
            this.Existence = existence;
            this.Children = children;
        }

        protected CAttribute() { }
        #endregion

        #region Class properties

        internal override ArchetypeConstraint ConstraintParent
        {
            get { return this.parent; }
        }


        private string rmAttributeName;
        /// <summary>
        /// Reference model attribute within the enclosing type represented by a C_OBJECT.
        /// </summary>
        public string RmAttributeName
        {
            get { return this.rmAttributeName; }
            set
            {
                Check.Require(!string.IsNullOrEmpty(value),
                    string.Format(CommonStrings.XMustNotBeNullOrEmpty, "RmAttributeName value"));
                this.rmAttributeName = value;
            }
        }

        private AssumedTypes.Interval<int> existence;
        /// <summary>
        /// Constraint on every attribute, regardless of whether it is singular or of a container type,
        /// which indicates whether its target object exists or not (i.e. is mandatory or not).
        /// </summary>
        public AssumedTypes.Interval<int> Existence
        {
            get { return existence; }
            set
            {
                DesignByContract.Check.Require(value!= null,
                    string.Format(CommonStrings.XMustNotBeNull, "Existence value"));
                this.existence = value;
            }
        }

        private AssumedTypes.List<CObject> children;
        /// <summary>
        /// Child C_OBJECT nodes. Each such node represents a constraint on the type of this attribute in its reference model. 
        /// Multiples occur both for multiple items in the case of container attributes, 
        /// and alternatives in the case of singular attributes.
        /// </summary>
        public AssumedTypes.List<CObject> Children
        {
            get { return this.children; }
            set { this.children = value; }
        }

        internal CComplexObject parent;

        #endregion

        #region Functions

        protected override string GetPath()
        {
            Check.Require(!string.IsNullOrEmpty(RmAttributeName),
                string.Format(CommonStrings.XMustNotBeNullOrEmpty, "RmAttributeName"));
            Check.Require(parent!= null, string.Format(CommonStrings.XMustNotBeNull, "parent"));

            string currentPath = "/" + this.rmAttributeName;
            string parentPath = this.parent.Path;
            if (parentPath != "/")
                currentPath = parentPath + currentPath;

            return currentPath;            
        }

        protected override System.Collections.Generic.List<string> GetPhysicalPaths()
        {
            if (this.Children == null)
                return null;

            System.Collections.Generic.List<string> paths = new System.Collections.Generic.List<string>();
            
            foreach (CObject item in this.Children)
            {
                string currentPath = "/" + this.rmAttributeName;

                currentPath += item.CurrentNodePath;

                paths.Add(currentPath);

                List<string> itemPysicalPath = item.PhysicalPaths;
                if (itemPysicalPath != null)
                {
                    foreach (string aPath in itemPysicalPath)
                    {
                        string combinedPath = currentPath + aPath;
                        paths.Add(combinedPath);
                    }
                }                
                    
            }

            Check.Ensure(paths.Count>0, "paths must not be empty.");

            return paths;
        }

        public override bool IsValid()
        {
            return AmValidator.ValidateCAttribute(this, ValidationContext.TerminologyService);
        }

       
        #endregion

        #region Validation

        internal override ValidationContext ValidationContext
        {
            get
            {
                Check.Require(ConstraintParent != null, string.Format(CommonStrings.XMustNotBeNullOrEmpty, "ConstraintParent"));

                ValidationContext result = ConstraintParent.ValidationContext;

                Check.Ensure(result != null, string.Format(CommonStrings.XMustNotBeNull, "ValidationContext"));
                return result;
            }
        }

        #endregion
    }
}
