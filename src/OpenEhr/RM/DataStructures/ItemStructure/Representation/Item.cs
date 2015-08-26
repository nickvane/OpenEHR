using System;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataTypes.Text;

namespace OpenEhr.RM.DataStructures.ItemStructure.Representation
{
    [Serializable]
    public abstract class Item : Locatable
    {
        protected Item()
        { }

        protected Item(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
            Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit) 
        { }
    }
}
