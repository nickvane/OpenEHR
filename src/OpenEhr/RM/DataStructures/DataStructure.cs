using System;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Attributes;

namespace OpenEhr.RM.DataStructures
{
    [Serializable]
    [RmType("openEHR", "DATA_STRUCTURES", "DATA_STRUCTURE")]
    public abstract class DataStructure : Locatable
    {
        protected DataStructure() 
        { }

        protected DataStructure(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
            Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit)
        {
            
        }

        public abstract ItemStructure.Representation.Item AsHierarchy();
    }
}
