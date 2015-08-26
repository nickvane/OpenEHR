using System;
using OpenEhr.Attributes;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Archetyped.Impl;

namespace OpenEhr.RM.DataStructures.ItemStructure
{
    [Serializable]
    [RmType("openEHR", "DATA_STRUCTURES", "ITEM_STRUCTURE")]
    public abstract class ItemStructure : DataStructure
    {
        protected ItemStructure() 
        { }

        protected ItemStructure(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
            Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit)
        { }
    }
}
