using System;
using OpenEhr.Attributes;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Composition.Content
{
    [Serializable]
    [RmType("openEHR", "EHR", "CONTENT_ITEM")]
    public abstract class ContentItem : Locatable, IVisitable
    {
        protected ContentItem()
        { }

        protected ContentItem(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
           Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit)
        { }

        #region IVisitable Members

        protected virtual void Accept(IVisitor visitor)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void IVisitable.Accept(IVisitor visitor)
        {
            this.Accept(visitor);
        }

        #endregion
    }
}
