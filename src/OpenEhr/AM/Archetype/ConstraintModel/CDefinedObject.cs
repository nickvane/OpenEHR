using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEhr.AM.Archetype.ConstraintModel
{
    /// <summary>
    /// Abstract parent type of C_OBJECT subtypes that are defined by value, i.e. whose
    /// definitions are actually in the archetype rather than being by reference.
    /// </summary>
    [Serializable]
    public abstract class CDefinedObject : CObject
    {
        #region Constructors
        protected CDefinedObject(string rmTypeName, string nodeId, AssumedTypes.Interval<int> occurrences, 
            CAttribute parent, object assumedValue)
            : base(rmTypeName, nodeId, occurrences, parent)
        {
            this.AssumedValue = assumedValue;
        }

        protected CDefinedObject():base() { }
        #endregion

        #region Properties
        /// <summary>
        /// Generate a default value from this constraint object
        /// </summary>
        public abstract object DefaultValue { get;}      

        private object assumedValue;
        /// <summary>
        /// Value to be assumed if none sent in data
        /// </summary>
        public object AssumedValue
        {
            get { return this.assumedValue; }
            set { this.assumedValue = value; }
        }
        #endregion

        #region Functions
        
        /// <summary>
        /// True if any value (i.e. instance) of the reference model type would be allowed. 
        /// Redefined in descedants.
        /// </summary>
        /// <returns></returns>
        public abstract bool AnyAllowed();
        /// <summary>
        /// True if there is an assumed value
        /// </summary>
        /// <returns></returns>
        public bool HasAssumedValue()
        {
            return this.AssumedValue != null;
        }

        #endregion
    }
}
