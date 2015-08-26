using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.DesignByContract;

namespace OpenEhr.AM.Archetype.ConstraintModel.Primitive
{
    [Serializable]
    public abstract class CPrimitive//: IRmType
    {
        /// <summary>
        /// Generate a default value from this constraint object
        /// </summary>
        public abstract object DefaultValue { get; }

        /// <summary>
        /// True if there is an assumed value
        /// </summary>
        /// <returns></returns>
        public abstract bool HasAssumedValue();

        /// <summary>
        /// Value to be assumed if none sent in data
        /// </summary>
        public abstract object AssumedValue { get; set;}

        internal abstract string ValidValue(object aValue);

        /// <summary>
        /// True if the current node constraints is narrower than other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        internal abstract bool IsSubsetOf(CPrimitive other);

    }
}
