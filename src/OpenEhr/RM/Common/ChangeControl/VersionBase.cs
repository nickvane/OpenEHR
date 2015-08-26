using System;

using OpenEhr.DesignByContract;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Generic;
using OpenEhr.Serialisation;
using OpenEhr.Attributes;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Common.ChangeControl
{
    public abstract class Version<T>
        : IVersion<T>, IVersion where T : class
    {
        protected Version()
        { }

        protected Version(AuditDetails commitAudit, ObjectRef contribution)
        {
            Check.Require(commitAudit != null, "commitAudit must not be null.");

            // set local data
            this.contribution = contribution;
            this.commitAudit = commitAudit;
        }

        public override bool Equals(object obj)
        {
            bool result = false;

            Version<T> compareObj = obj as Version<T>;
            if (compareObj != null && this.GetType() == compareObj.GetType())
                result = this.Uid.Equals(compareObj.Uid);

            return result;
        }

        HierObjectId ownerId;

        public HierObjectId OwnerId
        {
            get
            {
                if (ownerId == null)
                    ownerId = new HierObjectId(this.Uid.ObjectId.Value);

                Check.Ensure(ownerId != null, "ownerId must not be null");
                return ownerId;
            }
        }

        [RmAttribute("data")]
        public abstract T Data
        { get; }

        [RmAttribute("uid", 1)]
        public abstract ObjectVersionId Uid
        {
            get;
            protected set;
        }

        [RmAttribute("preceding_version_uid")]
        public abstract ObjectVersionId PrecedingVersionUid
        {
            get;
            protected set;
        }

        [RmAttribute("lifecycle_state", 1)]
        [RmTerminology("version lifecycle state")]
        public abstract DvCodedText LifecycleState
        {
            get;
            set;
        }

        #region IVersion Members

        public string CanonicalForm
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool IsBranch
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        private AuditDetails commitAudit;

        [RmAttribute("commit_audit", 1)]
        public AuditDetails CommitAudit
        {
            get
            {
                return this.commitAudit;
            }
            protected set
            {
                Check.Require(value != null, "value must not be null");

                this.commitAudit = value;
            }
        }

        private Support.Identification.ObjectRef contribution;

        [RmAttribute("contribution", 1)]
        public ObjectRef Contribution
        {
            get
            {
                return this.contribution;
            }
            protected set
            {
                Check.Require(value != null, "contribution must not be null");
                this.contribution = value;
            }
        }

        private string signature;

        [RmAttribute("signature")]
        public string Signature
        {
            get
            {
                return this.signature;
            }
        }

        object IVersion.Data
        {
            get { return this.Data; }
        }

        #endregion

        #region serialization

        internal protected void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase(reader);

            Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement, "Expect an endElement");
            reader.ReadEndElement();
            reader.MoveToContent();

            this.CheckInvariants();
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            this.WriteXmlBase(writer);
        }

        protected virtual void ReadXmlBase(System.Xml.XmlReader reader)
        {
            Check.Assert(reader.LocalName == "contribution",
                "Expected LocalName is 'contribution' rather than " + reader.LocalName);
            string contributionType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
            if (contributionType != null)
                this.contribution = OpenEhr.RM.Support.Identification.ObjectRef.GetObjectRefByType(contributionType);
            else
                this.contribution = new ObjectRef();
            this.contribution.ReadXml(reader);

            Check.Assert(reader.LocalName == "commit_audit",
                "Expected LocalName is 'commit_audit' rather than " + reader.LocalName);
            this.commitAudit = new OpenEhr.RM.Common.Generic.AuditDetails();
            this.commitAudit.ReadXml(reader);

            if (reader.LocalName == "signature")
            {
                this.signature = reader.ReadElementString("signature", RmXmlSerializer.OpenEhrNamespace);
            }
            reader.MoveToContent();
        }

        protected virtual void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            this.CheckInvariants();

            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            writer.WriteStartElement("contribution", RmXmlSerializer.OpenEhrNamespace);
            if (this.Contribution.GetType() != typeof(ObjectRef))
            {
                string contriType = ((IRmType)this.Contribution).GetRmTypeName();
                if (!string.IsNullOrEmpty(prefix))
                    contriType = prefix + ":" + contriType;
                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, contriType);
            }
            this.Contribution.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("commit_audit", RmXmlSerializer.OpenEhrNamespace);
            this.CommitAudit.WriteXml(writer);
            writer.WriteEndElement();
        }

        #endregion

        protected virtual void CheckInvariants()
        {
            Check.Invariant(this.Contribution != null, "contribution must not be null");
            Check.Invariant(this.CommitAudit != null, "CommitAudit must not be null");
        }
    }
}
