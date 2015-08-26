using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEhr.AM.Archetype.ConstraintModel
{
    /// <summary>
    /// Abstract parent type of domain-specific constrainer types, to be defined in external packages.
    /// </summary>
    [Serializable]
    public abstract class CDomainType : CDefinedObject
    {
        #region Constructors
        protected CDomainType(string rmTypeName, string nodeId, AssumedTypes.Interval<int> occurrences,
            CAttribute parent, object assumedValue)
            : base(rmTypeName, nodeId, occurrences, parent, assumedValue)
        { }

        protected CDomainType() : base() { }
        #endregion

        /// <summary>
        /// Standard (i.e. C_OBJECT) form of constraint.
        /// </summary>
        /// <returns></returns>
        public abstract CComplexObject StandardEquivalent();    

        protected override List<string> GetPhysicalPaths()
        {
            return null;
        }

        protected override string GetCurrentNodePath()
        {
            return null;
        }

    }
}
