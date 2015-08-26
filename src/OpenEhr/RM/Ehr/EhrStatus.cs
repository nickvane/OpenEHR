using System;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.Common.Generic;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.DesignByContract;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;
using System.Xml;

namespace OpenEhr.RM.Ehr
{
    public interface IEhrStatus : OpenEhr.RM.Common.Archetyped.ILocatable
    {
        PartySelf Subject { get; }
        bool IsQueryable { get; }
        bool IsModifiable { get; }
        ItemStructure OtherDetails { get; }
    }

    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "EHR", "EHR_STATUS")]
    public class EhrStatus : Locatable, System.Xml.Serialization.IXmlSerializable, IEhrStatus
    {
        const string defaultArchetypeId = "openEHR-EHR-EHR_STATUS.default.v1";
        const string defaultName = "EHR Status";

        public EhrStatus()
            : this(null, new PartySelf(), true, true, null)
        { }

        public EhrStatus(Support.Identification.ObjectId subjectId, string subjectNamespace)
            : this(null, new PartySelf(new PartyRef(subjectId, subjectNamespace, "PERSON")), true, true, null)
        { }

        public EhrStatus(PartySelf subject, bool isQueryable, bool isModifiable, ItemStructure otherDetails)
            : this(null, subject, isQueryable, isModifiable, otherDetails)
        { }

        public EhrStatus(ObjectVersionId uid, PartySelf subject, bool isQueryable, bool isModifiable, ItemStructure otherDetails)
            : this(new ArchetypeId(defaultArchetypeId), new DvText(defaultName), uid, subject, isQueryable, isModifiable, otherDetails)
        { }

        public EhrStatus(ArchetypeId archetypeId, DvText name, ObjectVersionId uid, PartySelf subject, 
            bool isQueryable, bool isModifiable, ItemStructure otherDetails)
        {
            if (uid != null)
            this.Uid = uid;

            this.subject = subject;

            this.isQueryable = isQueryable;
            this.isQueryableSet = true;

            this.isModifiable = isModifiable;
            this.isModifiableSet = true;

            this.otherDetails = otherDetails;

            base.SetBaseData(archetypeId.Value, name);
        }

        private PartySelf subject;

        public PartySelf Subject
        {
            get
            {
                if(this.subject == null)
                    this.subject = base.attributesDictionary["subject"] as PartySelf;

                Check.Ensure(base.attributesDictionary["subject"] == this.subject, "subject in attribute dictionary must be same as class attribute");
                return this.subject;
            }
            set
            {
                this.subject = value;

                base.attributesDictionary["subject"] = this.subject;
            }
        }

        private bool isQueryable;
        private bool isQueryableSet;

        public bool IsQueryable
        {
            get
            {
                if (!isQueryableSet)
                    this.isQueryable = bool.Parse(base.attributesDictionary["is_queryable"].ToString());

                Check.Ensure(bool.Parse(base.attributesDictionary["is_queryable"].ToString()) == this.isQueryable, "isQueryable in attribute dictionary must equal class attribute");
                return this.isQueryable;
            }

            set
            {
                this.isQueryable = value;
                this.isQueryableSet = true;

                base.attributesDictionary["is_queryable"] = this.isQueryable;
            }
        }

        private bool isModifiableSet;
        private bool isModifiable;

        public bool IsModifiable
        {
            get
            {
                if (!this.isModifiableSet)
                    this.isModifiable = bool.Parse(base.attributesDictionary["is_modifiable"].ToString());

                Check.Ensure(bool.Parse(base.attributesDictionary["is_modifiable"].ToString()) == this.isModifiable, "isModifiable in attribute dictionary must equal class attribute");
                return this.isModifiable;
            }
            set
            {
                this.isModifiable = value;
                isModifiableSet = true;

                base.attributesDictionary["is_modifiable"] = this.isModifiable;
            }
        }

        private ItemStructure otherDetails;

        public ItemStructure OtherDetails
        {
            get
            {
                if(this.otherDetails == null)
                    this.otherDetails = base.attributesDictionary["other_details"] as ItemStructure;

                Check.Ensure(base.attributesDictionary["other_details"] == this.otherDetails,
                    "other_details in attribute dictionary must be same as class attribute");
                return this.otherDetails;
            }

            internal set
            {
                Check.Require(value != null, "value must not be null.");

                this.otherDetails = value;

                base.attributesDictionary["other_details"] = this.otherDetails;
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

        public static XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadEhrStatusSchema(xs);
            return new XmlQualifiedName("EHR_STATUS", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(XmlReader reader)
        {
            base.ReadXmlBase(reader);

            DesignByContract.Check.Assert(reader.LocalName == "subject",
                "Expected LocalName is 'subject', but it is " + reader.LocalName);
            this.subject = new PartySelf();
            this.subject.ReadXml(reader);

            DesignByContract.Check.Assert(reader.LocalName == "is_queryable",
                "Expected LocalName is 'is_queryable', but it is " + reader.LocalName);
            this.isQueryable = reader.ReadElementContentAsBoolean("is_queryable", RmXmlSerializer.OpenEhrNamespace);
            //this.isQueryableSet = true;
            reader.MoveToContent();

            DesignByContract.Check.Assert(reader.LocalName == "is_modifiable",
               "Expected LocalName is 'is_modifiable', but it is " + reader.LocalName);
            this.isModifiable = reader.ReadElementContentAsBoolean("is_modifiable", RmXmlSerializer.OpenEhrNamespace);
            //this.isModifiableSet = true;
            reader.MoveToContent();

            if (reader.LocalName == "other_details")
            {
                string otherDetailsType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                DesignByContract.Check.Assert(!string.IsNullOrEmpty(otherDetailsType), "otherDetailsType must not be null or empty.");
                this.otherDetails = Locatable.GetLocatableObjectByType(otherDetailsType) as ItemStructure;
                this.otherDetails.ReadXml(reader);
            }
        }

        protected override void WriteXmlBase(XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            writer.WriteStartElement(openEhrPrefix, "subject", RmXmlSerializer.OpenEhrNamespace);
            this.Subject.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteElementString(openEhrPrefix, "is_queryable", RmXmlSerializer.OpenEhrNamespace, this.IsQueryable.ToString().ToLower());

            writer.WriteElementString(openEhrPrefix, "is_modifiable", RmXmlSerializer.OpenEhrNamespace, this.IsModifiable.ToString().ToLower());

            if (this.OtherDetails != null)
            {
                writer.WriteStartElement(openEhrPrefix, "other_details", RmXmlSerializer.OpenEhrNamespace);
                string otherDetailsType = ((IRmType)this.OtherDetails).GetRmTypeName();
                if (!string.IsNullOrEmpty(openEhrPrefix))
                    otherDetailsType = openEhrPrefix + ":" + otherDetailsType;

                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, otherDetailsType);
                this.OtherDetails.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        #endregion

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();

            base.attributesDictionary["subject"] = this.subject;
            base.attributesDictionary["is_queryable"] = this.isQueryable;
            base.attributesDictionary["is_modifiable"] = this.isModifiable;
            base.attributesDictionary["other_details"] = this.otherDetails;
        }

        protected override void CheckInvariants()
        {
            base.CheckInvariants();
            Check.Invariant(this.Parent == null, "EhrStatus must not have a parent");
            Check.Invariant(this.IsArchetypeRoot, "EhrStatus must be archetype root.");
            Check.Invariant(this.Subject != null, "Subject must not e null.");

            Check.Invariant(base.attributesDictionary["subject"] == this.Subject, "subject in attribute dictionary must be same as class attribute");
            Check.Invariant(bool.Parse(base.attributesDictionary["is_modifiable"].ToString()) == this.IsModifiable, "isModifiable in attribute dictionary must equal class attribute");
            Check.Invariant(bool.Parse(base.attributesDictionary["is_queryable"].ToString()) == this.IsQueryable, "isQueryable in attribute dictionary must equal class attribute");
            Check.Invariant(base.attributesDictionary["other_details"] == this.OtherDetails,
                "other_details in attribute dictionary must be same as class attribute");
        }

        const string RmTypeName = "EHR_STATUS";
    }
}
