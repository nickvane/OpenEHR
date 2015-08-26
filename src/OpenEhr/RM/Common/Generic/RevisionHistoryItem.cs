using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Common.Generic
{
    /// <summary>
    /// An entry in a revision history, corresponding to a version from a versioned container.
    /// Consists of AUDIT_DETAILS instances with revision identifier of the revision 
    /// to which the AUDIT_DETAILS intance belongs.
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "COMMON", "REVISION_HISTORY_ITEM")]
    public class RevisionHistoryItem : RmType, System.Xml.Serialization.IXmlSerializable
    {
        public RevisionHistoryItem()
        {
        }

        public RevisionHistoryItem(AssumedTypes.List<AuditDetails> audits,
            OpenEhr.RM.Support.Identification.ObjectVersionId versionId)
        {
            Check.Require(versionId != null, "version_id must not be null");

            this.audits = audits;
            this.versionId = versionId;
        }

        private AssumedTypes.List<AuditDetails> audits;
        /// <summary>
        /// The audits for this revision; there will always be at least one commit audit (which may itself
        /// be an ATTESTATION), there may also be further attestations.
        /// </summary>
        public AssumedTypes.List<AuditDetails> Audits
        {
            get { return this.audits; }
        }

        private OpenEhr.RM.Support.Identification.ObjectVersionId versionId;
        /// <summary>
        /// Version identifier for this revision.
        /// </summary>
        public OpenEhr.RM.Support.Identification.ObjectVersionId VersionId
        {
            get { return this.versionId; }
        }



        #region IXmlSerializable Members

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            this.ReadXml(reader);
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            this.WriteXml(writer);
        }

        #endregion

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("REVISION_HISTORY_ITEM", RmXmlSerializer.OpenEhrNamespace);
        }

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "version_id", "Expected local name is 'version_id', not " + reader.LocalName);
            this.versionId = new OpenEhr.RM.Support.Identification.ObjectVersionId();
            this.versionId.ReadXml(reader);

            Check.Assert(reader.LocalName == "audits", "Expected local name is 'audits', not " + reader.LocalName);
            while (reader.LocalName == "audits")
            {
                if (this.audits == null)
                    this.audits = new OpenEhr.AssumedTypes.List<AuditDetails>();

                AuditDetails auditDetails = new AuditDetails();
                auditDetails.ReadXml(reader);

                this.audits.Add(auditDetails);
            }
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            Check.Require(this.VersionId != null, "VersionId must not be null.");
            Check.Require(this.Audits != null, "Audits must not be null.");
            Check.Require(this.Audits.Count > 0, "Audits must not be empty.");

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "version_id", RmXmlSerializer.OpenEhrNamespace);
            this.VersionId.WriteXml(writer);
            writer.WriteEndElement();

            foreach (AuditDetails auditDetails in this.Audits)
            {
                writer.WriteStartElement(openEhrPrefix, "audits", RmXmlSerializer.OpenEhrNamespace);
                auditDetails.WriteXml(writer);
                writer.WriteEndElement();
            }
        }
    }
}
