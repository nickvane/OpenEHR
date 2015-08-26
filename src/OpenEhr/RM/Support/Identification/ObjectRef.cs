using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Support.Identification
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "SUPPORT", "OBJECT_REF")]
    public class ObjectRef : RmType, System.Xml.Serialization.IXmlSerializable 
    {
        public ObjectRef() 
        { }

        public ObjectRef(ObjectId objectId, string namespaceValue, string typeValue)
            : this()
        {
            Check.Require(!string.IsNullOrEmpty(namespaceValue), "namespaceValue must not be null or empty");
            Check.Require(objectId != null, "objectId must not be null");
            Check.Require(!string.IsNullOrEmpty(typeValue), "typeValue must not be null or empty");

            this.id = objectId;
            this.namespaceValue = namespaceValue;
            this.type = typeValue;

            CheckInvariants();
        }

        private ObjectId id;

        public ObjectId Id
        {
            get
            {
                return this.id;
            }
        }
       
        private string namespaceValue;

        public string Namespace
        {
            get
            {
                return namespaceValue;
            }
        }

        private string type;

        public string Type
        {
            get
            {
                return this.type;
            }
        }

        public override bool Equals(object obj)
        {
            ObjectRef objectRef = obj as ObjectRef;
            if (objectRef == null)
                return false;

            return this.Equals(objectRef);
        }

        public bool Equals(ObjectRef objectRef)
        {
            // If parameter is null return false:
            if ((object)objectRef == null)
                return false;

            if (this.GetType() != objectRef.GetType())
                return false;

            // Return true if the attribute values match
            if (!this.Id.Equals(objectRef.Id))
                return false;

            if (!this.Namespace.Equals(objectRef.Namespace))
                return false;

            return this.Type.Equals(objectRef.Type);
        }

        public override int GetHashCode()
        {
            string hashString = ((IRmType)this).GetRmTypeName() + "::" + this.Id.Value + "::" + this.Namespace + "::" + this.Type;
            return hashString.GetHashCode();
        }

        protected void CheckInvariants()
        {
            DesignByContract.Check.Invariant(this.Id !=null);

            // %HYYKA%
            // TODO: uncomments this invariant checking
            //DesignByContract.Check.Invariant(!string.IsNullOrEmpty(this.Namespace));
            //DesignByContract.Check.Invariant(!string.IsNullOrEmpty(this.Type));
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
            return new System.Xml.XmlQualifiedName(RmTypeName, RmXmlSerializer.OpenEhrNamespace);

        }

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            Check.Require(!reader.IsEmptyElement, reader.LocalName + " element must not be empty.");

            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase(reader);
           
            reader.ReadEndElement();
            reader.MoveToContent();

            this.CheckInvariants();
        }

        internal virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            this.WriteXmlBase(writer);
        }

        protected virtual void ReadXmlBase(System.Xml.XmlReader reader)
        {
            Check.Assert(reader.LocalName == "id", "reader localName must be 'id'");
            if (this.id == null)
            {
                string objectIdType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                this.id = ObjectId.GetObjectIdByType(objectIdType);
            }
            this.id.ReadXml(reader);

            Check.Assert(reader.LocalName == "namespace", "reader localName must be 'namespace'");
            this.namespaceValue = reader.ReadElementContentAsString();

            if (!reader.IsStartElement())
                reader.ReadEndElement();
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "type", "reader localName must be 'type'");
            this.type = reader.ReadElementContentAsString();
        }

        internal static ObjectRef GetObjectRefByType(string objectRefType)
        {
            Check.Require(!string.IsNullOrEmpty(objectRefType), "objectIdType must not be null or empty.");

            if (objectRefType.IndexOf(":") > 0 && objectRefType.IndexOf("http") < 0)
                objectRefType = objectRefType.Substring(objectRefType.IndexOf(":") + 1);

            switch (objectRefType)
            {
                case "OBJECT_REF":
                    return new ObjectRef();
                case "PARTY_REF":
                    return new PartyRef();
                case "LOCATABLE_REF":
                    return new LocatableRef();
                case "ACCESS_GROUP_REF":
                    return new AccessGroupRef();
                default:
                    throw new NotImplementedException("type not subtype of ObjectRef: " + objectRefType);

            }
        }

        protected virtual void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            writer.WriteStartElement("id", RmXmlSerializer.OpenEhrNamespace);

            string objectIdType = ((IRmType)this.Id).GetRmTypeName();
            if (!string.IsNullOrEmpty(openEhrPrefix))
                objectIdType = openEhrPrefix + ":" + objectIdType;
            writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, objectIdType);

            this.Id.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteElementString("namespace", RmXmlSerializer.OpenEhrNamespace, this.Namespace);
            writer.WriteElementString("type", RmXmlSerializer.OpenEhrNamespace, this.Type);
        }

        protected void SetBaseData(ObjectId objectId, string @namespace, string type)
        {
            Check.Require(objectId != null, "objectId must not be null");
            Check.Require(!string.IsNullOrEmpty(@namespace), "namespace must not be null or empty");
            Check.Require(!string.IsNullOrEmpty(type), "type must not be null or empty");

            this.id = objectId;
            this.namespaceValue = @namespace;
            this.type = type;
        }

        const string RmTypeName = "OBJECT_REF";
    }
}
