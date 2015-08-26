using System;

using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Support.Identification
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "SUPPORT", "LOCATABLE_REF")]
    public sealed class LocatableRef : ObjectRef, System.Xml.Serialization.IXmlSerializable
    {

        public LocatableRef() 
        { }

        public LocatableRef(UidBasedId objectId, string @namespace, string type, string path)
            :this()           
        {
            SetBaseData(objectId, @namespace, type);
            this.path = path;
        }

        private UidBasedId id;

        public UidBasedId Id
        {
            get
            {
                return this.id;
            }
        }

        private string path;

        public string Path
        {
            get
            {
                return this.path;
            }
        }

        public string AsUri()
        {
            return "ehr://" + Id.Value + "/" + this.Path;
        }   

       
        private void CheckInvariants()
        {
            if (this.Path != null)
                DesignByContract.Check.Invariant(!string.IsNullOrEmpty(this.Path));
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
            return new System.Xml.XmlQualifiedName("LOCATABLE_REF", RmXmlSerializer.OpenEhrNamespace);

        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {
            base.ReadXmlBase(reader);
            if (reader.LocalName == "path")
                this.path = reader.ReadElementString("path", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);
            if (this.Path != null || this.Path.Length > 0)
            {
                writer.WriteElementString("path", RmXmlSerializer.OpenEhrNamespace, this.Path);
                
            }
        }
    }
}
