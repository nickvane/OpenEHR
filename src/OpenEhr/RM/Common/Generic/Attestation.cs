using System;
using OpenEhr.DesignByContract;
using OpenEhr.Validation;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.Factories;


namespace OpenEhr.RM.Common.Generic
{
    [Serializable]
    [RmType("openEHR", "COMMON", "ATTESTATION")]
    public class Attestation : AuditDetails
    {
        public Attestation() 
        { }

        public Attestation(string systemId, DataTypes.Quantity.DateTime.DvDateTime timeCommitted, 
            DataTypes.Text.DvCodedText changedType, PartyProxy committer, DataTypes.Text.DvText description,
            DataTypes.Encapsulated.DvMultimedia attestedView, string proof, 
            AssumedTypes.List<DataTypes.Uri.DvEhrUri> items, DataTypes.Text.DvText reason, bool isPending)
            : base(systemId, timeCommitted, changedType, committer, description)
        {
            Check.Require(items == null | items.Count >0, "if items is not null, it must not be empty.");
            Check.Require(reason != null, "reason must not be null.");

            this.attestedView = attestedView;
            this.proof = proof;
            this.items = items;
            this.reason = reason;
            this.isPending = isPending;
            this.isPendingSet = true;

            this.CheckDefaultInvariants();
        }

        private string proof;

        public string Proof
        {
            get
            {
                return this.proof;
            }
           
        }

        private DataTypes.Encapsulated.DvMultimedia attestedView;

        public DataTypes.Encapsulated.DvMultimedia AttestedView
        {
            get
            {
                return this.attestedView;
            }
        }

        private AssumedTypes.List<DataTypes.Uri.DvEhrUri> items;

        public AssumedTypes.List<DataTypes.Uri.DvEhrUri> Items
        {
            get
            {
                return this.items;
            }
        }

        private OpenEhr.RM.DataTypes.Text.DvText reason;

        [RmAttribute("reason", 1)]
        [RmTerminology("attestation reason")]
        public OpenEhr.RM.DataTypes.Text.DvText Reason
        {
            get
            {
                return this.reason;
            }          
        }

        private bool isPendingSet;
        private bool isPending;

        public bool IsPending
        {
            get
            {
                return this.isPending;
            }
          
        }

        protected override void CheckDefaultInvariants()
        {
            base.CheckDefaultInvariants();

            DesignByContract.Check.Invariant(this.Reason != null, "Reason must not be null.");
            DesignByContract.Check.Invariant(this.Items == null | this.Items.Count>0, 
                "If Items is not null, it must not be empty."); 
        }

        #region serialization

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);

            if (reader.LocalName == "attested_view")
            {
                this.attestedView = new OpenEhr.RM.DataTypes.Encapsulated.DvMultimedia();
                this.attestedView.ReadXml(reader);
            }

            if (reader.LocalName == "proof")
            {
                this.proof = reader.ReadElementString("proof", RmXmlSerializer.OpenEhrNamespace);
                reader.MoveToContent();
            }

            if (reader.LocalName == "items")
            {
                this.items = new OpenEhr.AssumedTypes.List<OpenEhr.RM.DataTypes.Uri.DvEhrUri>();
                do
                {
                    DataTypes.Uri.DvEhrUri uri = new OpenEhr.RM.DataTypes.Uri.DvEhrUri();
                    uri.ReadXml(reader);

                    this.items.Add(uri);
                } while (reader.LocalName == "items");
            }

            if (reader.LocalName != "reason")
                throw new ValidationException("Excepted element name is reason, but it is: " + reader.LocalName);
            string reasonType = RmXmlSerializer.ReadXsiType(reader);
            if (!string.IsNullOrEmpty(reasonType))
            {
                this.reason = RmFactory.DataValue(reasonType) as DataTypes.Text.DvText;
            }
            else
                this.reason = new OpenEhr.RM.DataTypes.Text.DvText();
            this.reason.ReadXml(reader);

            if (reader.LocalName != "is_pending")
                throw new ValidationException("Excepted element name is is_pending, but it is: " + reader.LocalName);
            this.isPending = reader.ReadElementContentAsBoolean("is_pending", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();

        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);

            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            if (this.AttestedView != null)
            {
                writer.WriteStartElement(openEhrPrefix, "attested_view", RmXmlSerializer.OpenEhrNamespace);
                this.AttestedView.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.Proof != null)
            {
                writer.WriteElementString(openEhrPrefix, "proof", RmXmlSerializer.OpenEhrNamespace, this.Proof);
            }

            if (this.Items != null)
            {
                foreach (DataTypes.Uri.DvEhrUri uri in this.Items)
                {
                    writer.WriteStartElement(openEhrPrefix, "items", RmXmlSerializer.OpenEhrNamespace);
                    uri.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }

            writer.WriteStartElement(openEhrPrefix, "reason", RmXmlSerializer.OpenEhrNamespace);
            this.Reason.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteElementString(openEhrPrefix, "is_pending", RmXmlSerializer.OpenEhrNamespace, this.IsPending.ToString().ToLower());
        }      

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("ATTESTATION", RmXmlSerializer.OpenEhrNamespace);

        }
        #endregion
    }
}
