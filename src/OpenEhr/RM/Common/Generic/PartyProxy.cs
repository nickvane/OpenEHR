using System;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.RM.Impl;
using OpenEhr.RM.Support.Identification;

namespace OpenEhr.RM.Common.Generic
{
    [Serializable]
    [RmType("openEHR", "COMMON", "PARTY_PROXY")]
    public abstract class PartyProxy : RmType
    {
        private PartyRef externalRef;

        public PartyRef ExternalRef
        {
            get { return this.externalRef; }
        }

        protected virtual void SetBaseData(PartyRef externalRef)
        {
            this.externalRef = externalRef;
        }

        protected virtual void ReadXmlBase(System.Xml.XmlReader reader)
        {
            if (reader.LocalName == "external_ref")
            {
                if (this.externalRef == null)
                    this.externalRef = new PartyRef();

                this.externalRef.ReadXml(reader);
            }
        }

        protected virtual void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            if (this.ExternalRef != null)
            {
                writer.WriteStartElement("external_ref", RmXmlSerializer.OpenEhrNamespace);
                this.ExternalRef.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            if (!reader.IsEmptyElement)
            {
                reader.ReadStartElement();
                reader.MoveToContent();

                this.ReadXmlBase(reader);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            else
            {
                reader.ReadStartElement();
                reader.MoveToContent();
            }
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            this.WriteXmlBase(writer);
        }

        protected virtual void CheckInvariants()
        {
            throw new NotImplementedException();
        }
    }
}
