using System;
using System.Xml;
using OpenEhr.DesignByContract;

using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.Factories;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Common.ChangeControl
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "COMMON", "ORIGINAL_VERSION")]
    public class OriginalVersion<T> 
        : Version<T>, System.Xml.Serialization.IXmlSerializable where T : class 
    {
        public OriginalVersion() 
        { }

        public OriginalVersion(Support.Identification.ObjectVersionId uid,
            Support.Identification.ObjectVersionId precedingVersionUid,
            DataTypes.Text.DvCodedText lifecycleState, Common.Generic.AuditDetails commitAudit,
            Support.Identification.ObjectRef contribution, T data)
            : base(commitAudit, contribution)
        {
            Check.Require(uid != null, "uid must not be null");
            Check.Require(data != null, "data must not be null");

            // set local data
            this.data = data;
            this.uid = uid;
            this.precedingVersionUid = precedingVersionUid;
            this.lifecycleState = lifecycleState;
        }

        private T data;

        protected T MutableData
        {
            get
            {
                return data;
            }
        }

        public override T Data
        {
            get { return RmFactory.Clone<T>(MutableData); }
        }

        private AssumedTypes.List<Generic.Attestation> attestations;

        [RmAttribute("attestations")]
        public AssumedTypes.List<Generic.Attestation> Attestations
        {
            get
            {
                return this.attestations;
            }
        }

        private ObjectVersionId uid;

        public override ObjectVersionId Uid
        {
            get
            {
                return this.uid;
            }
            protected set
            {
                Check.Require(value != null, "Uid must not be null");
                this.uid = value;
            }
        }

        private ObjectVersionId precedingVersionUid;

        public override ObjectVersionId PrecedingVersionUid
        {
            get 
            {
                return this.precedingVersionUid;
            }
            protected set
            {
                Check.Require(value != null, "PrecedingVersionUid must not be null");
                this.precedingVersionUid = value;
            }
        }

        private DvCodedText lifecycleState;

        public override DvCodedText LifecycleState
        {
            get
            {
                return this.lifecycleState;
            }
            set
            {
                Check.Require(value != null, "LifecycleState must not be null");

                this.lifecycleState = value;
            }
        }

        AssumedTypes.Set<ObjectVersionId> otherInputVersionUids;

        public AssumedTypes.Set<ObjectVersionId> OtherInputVersionUids
        {
            get
            {
                return this.otherInputVersionUids;
            }
        }

        #region openEhrV1 serialization
        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {           
            base.ReadXmlBase(reader);

            // read for the attributes in this local class
            Check.Assert(reader.LocalName == "uid", "Expected LocalName as uid rather than " + reader.LocalName);
            this.uid = new OpenEhr.RM.Support.Identification.ObjectVersionId();
            this.uid.ReadXml(reader);

            if (reader.LocalName == "data")
            {
                string dataType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);

                OpenEhr.RM.Common.Archetyped.Impl.Locatable locatableData =
                    OpenEhr.RM.Common.Archetyped.Impl.Locatable.GetLocatableObjectByType(dataType);
                locatableData.ReadXml(reader);
                this.data = locatableData as T;
            }

            if (reader.LocalName == "preceding_version_uid")
            {
                this.precedingVersionUid = new OpenEhr.RM.Support.Identification.ObjectVersionId();
                this.precedingVersionUid.ReadXml(reader);
            }

            if (reader.LocalName == "other_input_version_uids")
            {
                this.otherInputVersionUids = new AssumedTypes.Set<ObjectVersionId>();
                do
                {
                    ObjectVersionId objectVersionId = new ObjectVersionId();
                    objectVersionId.ReadXml(reader);

                    this.otherInputVersionUids.Add(objectVersionId);
                } while (reader.LocalName == "other_input_version_uids");
            }

            if (reader.LocalName == "attestations")
            {
                this.attestations = new OpenEhr.AssumedTypes.List<OpenEhr.RM.Common.Generic.Attestation>();
                do
                {
                    Generic.Attestation a = new OpenEhr.RM.Common.Generic.Attestation();
                    a.ReadXml(reader);

                    this.attestations.Add(a);
                } while (reader.LocalName == "attestations");
            }

            Check.Assert(reader.LocalName == "lifecycle_state",
                "Expected LocalName is lifecycle_state not " + reader.LocalName);
            this.lifecycleState = new OpenEhr.RM.DataTypes.Text.DvCodedText();
            this.lifecycleState.ReadXml(reader);
        }        

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            writer.WriteStartElement("uid", RmXmlSerializer.OpenEhrNamespace);            
            this.Uid.WriteXml(writer);
            writer.WriteEndElement();

            if (this.Data != null)
            {
                writer.WriteStartElement("data", RmXmlSerializer.OpenEhrNamespace);

                OpenEhr.RM.Common.Archetyped.Impl.Locatable dataLocatable = this.Data 
                    as OpenEhr.RM.Common.Archetyped.Impl.Locatable;
                if (dataLocatable== null)
                    throw new NotSupportedException("Only support locatable as version data: "
                        + Data.GetType().Name);
                string dataType = ((IRmType)dataLocatable).GetRmTypeName();
                if (!string.IsNullOrEmpty(prefix))
                    dataType = prefix + ":" + dataType;
                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, dataType);
                Composition.Composition com = this.Data as Composition.Composition;
                com.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.PrecedingVersionUid != null)
            {
                writer.WriteStartElement("preceding_version_uid", RmXmlSerializer.OpenEhrNamespace);
                this.PrecedingVersionUid.WriteXml(writer);
                writer.WriteEndElement();
            }

            // otherInputVersionUids
            if (this.OtherInputVersionUids != null)
            {
                foreach (ObjectVersionId objectVersionId in this.OtherInputVersionUids)
                {
                    writer.WriteStartElement("other_input_version_uids", RmXmlSerializer.OpenEhrNamespace);
                    objectVersionId.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
           
            // attestation
            if (this.Attestations != null)
            {
                foreach (Generic.Attestation a in this.Attestations)
                {
                    writer.WriteStartElement("attestations", RmXmlSerializer.OpenEhrNamespace);
                    a.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }

            writer.WriteStartElement("lifecycle_state", RmXmlSerializer.OpenEhrNamespace);
            this.LifecycleState.WriteXml(writer);
            writer.WriteEndElement();
            
        }

        protected override void CheckInvariants()
        {
            base.CheckInvariants();
            Check.Invariant(this.Uid != null, "Uid must not be null");
            Check.Invariant(this.LifecycleState != null, "LifecycleState must not be null");
        }

        public static XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadVersionSchema(xs);
            return new XmlQualifiedName("ORIGINAL_VERSION", RmXmlSerializer.OpenEhrNamespace);
        }

        #region IXmlSerializable Members

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(XmlReader reader)
        {
            this.ReadXml(reader);
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(XmlWriter writer)
        {
            this.WriteXml(writer);
        }
        #endregion

        #endregion
    }
}
