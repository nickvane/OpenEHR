using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEhr.AM.Archetype.ConstraintModel
{
    [Serializable]
    public abstract class CReferenceObject : CObject
    {
        protected CReferenceObject(string rmTypeName, string nodeId, AssumedTypes.Interval<int> occurrences,
          CAttribute parent)
            : base(rmTypeName, nodeId, occurrences, parent) { }

        protected CReferenceObject() { }

    }
}
