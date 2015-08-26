using System;
using System.ComponentModel;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Support.Identification
{
    [TypeConverter(typeof(ObjectIdTypeConverter))]
    [Serializable]
    [RmType("openEHR", "SUPPORT", "OBJECT_ID")]
    public abstract class ObjectId : RmType
    {
        private string value;        
        
        public string Value
        {
            get { return this.value; }
        }

        protected void SetBaseData(string value)
        {
            Check.Require(IsValidValue(value), string.Format("Not valid value ({0}) for type of {1}", 
                value, this.GetType().Name));

            this.value = value;
        }

        protected abstract bool IsValidValue(string value);

        public override string ToString()
        {
            return this.Value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            ObjectId objectId = obj as ObjectId;
            return this.Equals(objectId);
        }

        public bool Equals(ObjectId objectId)
        {
            // If parameter is null return false:
            if (objectId == null)
                return false;

            return this.Value.Equals(objectId.Value);
        }

        public static bool operator ==(ObjectId a, ObjectId b)
        {
            if ((object)a != null)
                return a.Equals(b);

            // a is null, check if b is null
            if ((object)b != null)
                return false;
            else
                return true;
        }

        public static bool operator !=(ObjectId a, ObjectId b)
        {
            if ((object)a != null)
                return !a.Equals(b);

            // a is null, check if b is null
            if ((object)b != null)
                return true;
            else
                return false;
        }

        protected virtual void CheckInvariants()
        {
            Check.Invariant(this.Value != null, "value must not be null.");
        }

        #region internal functions
        
        internal virtual void ReadXml(System.Xml.XmlReader reader)
        {
            DesignByContract.Check.Require(!reader.IsEmptyElement, reader.LocalName + " element must not be empty.");

            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase(reader);
           
            reader.ReadEndElement();
            reader.MoveToContent();

            this.CheckInvariants();
        }

        protected virtual void ReadXmlBase(System.Xml.XmlReader reader)
        {
            this.value = reader.ReadElementString("value", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();
        }

        protected virtual void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            string openEhrNamespace = RmXmlSerializer.OpenEhrNamespace;
            string prefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            writer.WriteElementString(prefix, "value", openEhrNamespace, this.Value);
        }

        internal virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            this.CheckInvariants();
            this.WriteXmlBase(writer);
        }    

       
        #endregion

        internal static ObjectId GetObjectIdByType(string objectIdType)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(objectIdType), "objectIdType must not be null or empty.");

            if (objectIdType.IndexOf(":") > 0 && objectIdType.IndexOf("http") < 0)
                objectIdType = objectIdType.Substring(objectIdType.IndexOf(":") + 1);

            switch (objectIdType)
            {
                case "TERMINOLOGY_ID":
                    return new TerminologyId();
                case "OBJECT_VERSION_ID":
                    return new ObjectVersionId();
                case "HIER_OBJECT_ID":
                    return new HierObjectId();
                case "GENERIC_ID":
                    return new GenericId();
                default:
                    throw new NotImplementedException("type not supported yet: " + objectIdType);

            }
        }
    }
}
