using System;
using OpenEhr.DesignByContract;
using OpenEhr.Serialisation;
using OpenEhr.Factories;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Common.Archetyped
{
    /// <summary>
    /// Adit details for any system in a feeder system chain. Audit details here means
    /// the genral notion of who/where/when the information item to which the audit is
    /// attached was created. None of the attributes is defined as mandatory, however,
    /// in different scenarios, various combinations of attributes will usually be mandatory.
    /// This can be controlled by specifying feeder audit details in legacy archetypes.
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    public class FeederAuditDetails : System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>
        /// constructor
        /// </summary>
        public FeederAuditDetails()
        { }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="systemId"></param>
        public FeederAuditDetails(string systemId)
            : this()
        {
            this.systemId = systemId;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="systemId"></param>
        /// <param name="location"></param>
        /// <param name="provider"></param>
        /// <param name="subject"></param>
        /// <param name="time"></param>
        /// <param name="versionId"></param>
        public FeederAuditDetails(string systemId, Common.Generic.PartyIdentified location,
            Common.Generic.PartyIdentified provider, Common.Generic.PartyProxy subject,
            DataTypes.Quantity.DateTime.DvDateTime time, string versionId)
            : this()
        {
            this.systemId = systemId;
            this.location = location;
            this.provider = provider;
            this.subject = subject;
            this.time = time;
            this.versionId = versionId;
        }

        private string systemId;

        public string SystemId
        {
            get
            {
                return this.systemId;
            }
        }

        private string versionId;

        public string VersionId
        {
            get
            {
                return this.versionId;
            }
        }

        private Common.Generic.PartyIdentified location;

        public Common.Generic.PartyIdentified Location
        {
            get
            {
                return this.location;
            }
        }

        private Common.Generic.PartyIdentified provider;

        public Common.Generic.PartyIdentified Provider
        {
            get
            {
                return this.provider;
            }
        }

        private Common.Generic.PartyProxy subject;

        public Common.Generic.PartyProxy Subject
        {
            get
            {
                return this.subject;
            }
        }

        private DataTypes.Quantity.DateTime.DvDateTime time;

        public DataTypes.Quantity.DateTime.DvDateTime Time
        {
            get
            {
                return this.time;
            }
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
            return new System.Xml.XmlQualifiedName("FEEDER_AUDIT_DETAILS", RmXmlSerializer.OpenEhrNamespace);
        }

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            Check.Require(reader.LocalName == "system_id", "Expected LocalName is 'system_id' rather than "
                + reader.LocalName);
            this.systemId = reader.ReadElementString("system_id", RmXmlSerializer.OpenEhrNamespace);

            if (reader.LocalName == "location")
            {
                this.location = new OpenEhr.RM.Common.Generic.PartyIdentified();
                this.location.ReadXml(reader);
            }

            if (reader.LocalName == "provider")
            {
                this.provider = new OpenEhr.RM.Common.Generic.PartyIdentified();
                this.provider.ReadXml(reader);
            }

            if (reader.LocalName == "subject")
            {
                string subjectType = RmXmlSerializer.ReadXsiType(reader);
                Check.Assert(!string.IsNullOrEmpty(subjectType), "the type of subject must not be null or empty.");
                this.subject = RmFactory.PartyProxy(subjectType);
                this.subject.ReadXml(reader);
            }

            if (reader.LocalName == "time")
            {
                this.time = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime();
                this.time.ReadXml(reader);
            }

            if (reader.LocalName == "version_id")
            {
                this.versionId = reader.ReadElementString("version_id", RmXmlSerializer.OpenEhrNamespace);
            }

            if (!reader.IsStartElement())
            {
                reader.ReadEndElement();
            }

            reader.MoveToContent();
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            writer.WriteElementString(openEhrPrefix, "system_id", RmXmlSerializer.OpenEhrNamespace, this.SystemId);
            if (this.Location != null)
            {
                writer.WriteStartElement(openEhrPrefix, "location", RmXmlSerializer.OpenEhrNamespace);
                this.Location.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.provider != null)
            {
                writer.WriteStartElement(openEhrPrefix, "provider", RmXmlSerializer.OpenEhrNamespace);
                this.Provider.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.subject != null)
            {
                writer.WriteStartElement(openEhrPrefix, "subject", RmXmlSerializer.OpenEhrNamespace);
                string subjectType = ((IRmType)this.Subject).GetRmTypeName();
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    subjectType = openEhrPrefix + ":" + subjectType;
                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, subjectType);
                this.Subject.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.Time != null)
            {
                writer.WriteStartElement(openEhrPrefix, "time", RmXmlSerializer.OpenEhrNamespace);
                this.Time.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.VersionId != null)
                writer.WriteElementString(openEhrPrefix, "version_id", RmXmlSerializer.OpenEhrNamespace, this.VersionId);
        }
    }
}
