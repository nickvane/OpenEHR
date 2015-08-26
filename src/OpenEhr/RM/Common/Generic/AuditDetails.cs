using System;
using OpenEhr.Attributes;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Impl;
using OpenEhr.Factories;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Common.Generic
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "COMMON", "AUDIT_DETAILS")]
    public class AuditDetails : RmType, System.Xml.Serialization.IXmlSerializable
    {
        public AuditDetails()
        {
        }

        // System ID is not set here, expecting it top be set by the server
        public AuditDetails(DataTypes.Text.DvCodedText changeType, PartyProxy committer)
            : this()
        {
            DesignByContract.Check.Require(changeType != null, "changeType must not be null");
            DesignByContract.Check.Require(committer != null, "committer must not be null");

            this.changeType = changeType;
            this.committer = committer;

            this.CheckDefaultInvariants();
        }

        public AuditDetails(string systemId, DataTypes.Text.DvCodedText changeType)
            : this()
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(systemId), "systemId must not be null or empty");
            DesignByContract.Check.Require(changeType != null, "changeType must not be null");

            this.systemId = systemId;
            this.timeCommitted = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime();
            this.changeType = changeType;
        }


        public AuditDetails(string systemId, DataTypes.Quantity.DateTime.DvDateTime timeCommitted, 
            DataTypes.Text.DvCodedText changeType, PartyProxy committer, DataTypes.Text.DvText description)
            : this()
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(systemId), "systemId must not be null or empty");
            DesignByContract.Check.Require(timeCommitted != null, "timeCommitted must not be null");
            DesignByContract.Check.Require(changeType != null, "changeType must not be null");
            DesignByContract.Check.Require(committer != null, "committer must not be null");

            this.systemId = systemId;
            this.timeCommitted = timeCommitted;
            this.changeType = changeType;
            this.committer = committer;
            this.description = description;

            this.CheckDefaultInvariants();
        }

        private string systemId;

        public string SystemId
        {
            get
            {
                return this.systemId;
            }
            set
            {
                Check.Require(!string.IsNullOrEmpty(value), "SystemId must not be null or empty");
                this.systemId = value;
            }
        }

        private DataTypes.Quantity.DateTime.DvDateTime timeCommitted;

        public DataTypes.Quantity.DateTime.DvDateTime TimeCommitted
        {
            get
            {
                return timeCommitted;
            }
            set
            {
                Check.Require(value != null, "TimeCommitted must not be null");
                this.timeCommitted = value;
            }
        }

        private DataTypes.Text.DvCodedText changeType;

        [RmAttribute("change_type", 1)]
        [RmTerminology("audit change type")]
        public DataTypes.Text.DvCodedText ChangeType
        {
            get
            {
                return changeType;
            }
        }

        private DataTypes.Text.DvText description;

        public DataTypes.Text.DvText Description
        {
            get { return description; }
            set { description = value; }
        }

        private PartyProxy committer;

        public PartyProxy Committer
        {
            get
            {
                return committer;
            }
            set
            {
                Check.Require(value != null, "Committer must not be null");

                committer = value;
            }
        }

        #region serialization

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

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase(reader);

            DesignByContract.Check.Assert(reader.NodeType == System.Xml.XmlNodeType.EndElement,
              "Expected endElement of PartyIdentified.");
            reader.ReadEndElement();
            reader.MoveToContent();

            this.CheckDefaultInvariants();
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            this.CheckStrictInvariants();
            this.WriteXmlBase(writer);
        }

        protected virtual void ReadXmlBase(System.Xml.XmlReader reader)
        {
            Check.Assert(reader.LocalName == "system_id",
                "Expected LocalName is 'system_id' not " + reader.LocalName);
            this.systemId = reader.ReadElementString("system_id", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "committer",
                "Expected LocalName is 'committer' not " + reader.LocalName);
            string committerType = RmXmlSerializer.ReadXsiType(reader);
            this.committer = RmFactory.PartyProxy(committerType);

            this.committer.ReadXml(reader);

            Check.Assert(reader.LocalName == "time_committed",
                "Expected LocalName is 'time_committed' not " + reader.LocalName);
            this.timeCommitted = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime();
            this.timeCommitted.ReadXml(reader);

            Check.Assert(reader.LocalName == "change_type",
                            "Expected LocalName is 'change_type' not " + reader.LocalName);
            this.changeType = new OpenEhr.RM.DataTypes.Text.DvCodedText();
            this.changeType.ReadXml(reader);

            if (reader.LocalName == "description")
            {
                string descriptionType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                if (descriptionType == null)
                    this.description = new OpenEhr.RM.DataTypes.Text.DvText();
                else
                    this.description = new OpenEhr.RM.DataTypes.Text.DvCodedText();

                this.description.ReadXml(reader);
            }
        }

        protected virtual void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            writer.WriteElementString(openEhrPrefix, "system_id", RmXmlSerializer.OpenEhrNamespace, this.SystemId);

            writer.WriteStartElement(openEhrPrefix, "committer", RmXmlSerializer.OpenEhrNamespace);
            string committerType = ((IRmType)this.Committer).GetRmTypeName();
            if (!string.IsNullOrEmpty(openEhrPrefix))
                committerType = openEhrPrefix + ":" + committerType;
            writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, committerType);
            this.Committer.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement(openEhrPrefix, "time_committed", RmXmlSerializer.OpenEhrNamespace);
            if (this.TimeCommitted != null)
                this.TimeCommitted.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement(openEhrPrefix, "change_type", RmXmlSerializer.OpenEhrNamespace);
            this.ChangeType.WriteXml(writer);
            writer.WriteEndElement();

            if (this.Description != null)
            {
                writer.WriteStartElement(openEhrPrefix, "description", RmXmlSerializer.OpenEhrNamespace);
                if (this.Description.GetType() == typeof(OpenEhr.RM.DataTypes.Text.DvCodedText))
                {
                    string descriptionType = "DV_CODED_TEXT";
                    if (!string.IsNullOrEmpty(committerType))
                        descriptionType = openEhrPrefix + ":" + descriptionType;
                    writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, descriptionType);
                }
                this.Description.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("AUDIT_DETAILS", RmXmlSerializer.OpenEhrNamespace);

        }

        #endregion

        private void CheckStrictInvariants()
        {
            Check.Invariant(!string.IsNullOrEmpty(this.SystemId), "system_id must not be null or empty.");
            Check.Invariant(this.Committer != null, "committer must not be null");
            Check.Invariant(this.TimeCommitted != null, "time_committed must not be null");
            Check.Invariant(this.ChangeType != null, "ChangeType must not be null");
        }

        protected virtual void CheckDefaultInvariants()
        {
            Check.Invariant(this.ChangeType != null, "ChangeType must not be null");
        }
    }
}
