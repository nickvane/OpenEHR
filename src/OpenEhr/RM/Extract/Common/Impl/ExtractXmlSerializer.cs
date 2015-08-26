using System;
using System.Xml;
using OpenEhr.RM.Extract.EhrExtract;
using OpenEhr.DesignByContract;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.Common.Generic;
using OpenEhr.Serialisation;
using OpenEhr.RM.Ehr;
using OpenEhr.RM.Common.ChangeControl;

namespace OpenEhr.RM.Extract.Common.Impl
{
    class ExtractXmlSerializer
    {
        #region ReadXml...

        public void ReadXml(XmlReader reader, Extract extract)
        {
            // <xs:extension base="LOCATABLE">
            this.ReadXmlBase(reader, extract as ExtractLocatable);

            // <xs:element name="request_id" type="OBJECT_REF" minOccurs="0"/>
            if (reader.LocalName == "request_id")
            {
                if(reader.HasAttributes)
                    throw new NotSupportedException("subtype of OBJECT_REF not supported in request_id");

                extract.RequestId = new ObjectRef();
                extract.RequestId.ReadXml(reader);
            }
            
            // <xs:element name="time_created" type="DV_DATE_TIME"/>
            Check.Require(reader.LocalName == "time_created", "time_created is mandatory");
            extract.TimeCreated = new DvDateTime();
            extract.TimeCreated.ReadXml(reader);

            // <xs:element name="system_id" type="HIER_OBJECT_ID"/>
            Check.Require(reader.LocalName == "system_id", "system_id is mandatory");
            extract.SystemId = new HierObjectId();
            extract.SystemId.ReadXml(reader);
            
            // <xs:element name="partitipations" type="PARTICIPATION" minOccurs="0" maxOccurs="unbounded"/>
            if (reader.Name == "partitipations")
                extract.Participations = new OpenEhr.AssumedTypes.List<Participation>();
            while (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name == "partitipations")
            {
                Participation participation = new Participation();

                participation.ReadXml(reader);
                extract.Participations.Add(participation);
            }
            
            // <xs:element name="sequence_nr" type="xs:long"/>
            extract.SequenceNr = reader.ReadElementContentAsInt("sequence_nr", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();

            // <xs:element name="chapters" type="EXTRACT_CHAPTER" minOccurs="0" maxOccurs="unbounded"/>
            if (reader.Name == "chapters")
                extract.Chapters = new OpenEhr.AssumedTypes.List<ExtractChapter>();
            while (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name == "chapters")
            {
                ExtractChapter chapter = new ExtractChapter();

                this.ReadXml(reader, chapter);
                extract.Chapters.Add(chapter);
            }

            // TODO: <xs:element name="specification" type="EXTRACT_SPEC" minOccurs="0"/>
            if (reader.Name == "specification")
                throw new NotImplementedException("specification not implemented");

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        void ReadXmlBase(XmlReader reader, ExtractLocatable locatable)
        {
            reader.MoveToContent();

            locatable.ArchetypeNodeId = reader.GetAttribute("archetype_node_id");
            Check.Require(!string.IsNullOrEmpty(locatable.ArchetypeNodeId), "archetype_node_id attribute is required");

            reader.ReadStartElement();
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "name", "Expected LocalName is 'name' rather than " + reader.LocalName);

            string nameType = RmXmlSerializer.ReadXsiType(reader);
            if (nameType != null && nameType == "DV_CODED_TEXT")
                locatable.Name = new DvCodedText();
            else
                locatable.Name = new DvText();
            locatable.Name.ReadXml(reader);

            if (reader.LocalName == "uid")
            {
                string uidType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                int i = uidType.IndexOf(":");
                if (i > 0)
                    uidType = uidType.Substring(i + 1);
                if (uidType == "OBJECT_VERSION_ID")
                    locatable.Uid = new ObjectVersionId();
                else if (uidType == "HIER_OBJECT_ID")
                    locatable.Uid = new HierObjectId();

                locatable.Uid.ReadXml(reader);
            }
            if (reader.LocalName == "links")
            {
                locatable.Links = new OpenEhr.AssumedTypes.List<Link>();
                do
                {
                    Link aLink = new Link();
                    aLink.ReadXml(reader);
                    locatable.Links.Add(aLink);
                } while (reader.LocalName == "links" && reader.NodeType == System.Xml.XmlNodeType.Element);
            }

            if (reader.LocalName == "archetype_details")
            {
                locatable.ArchetypeDetails = new Archetyped();
                locatable.ArchetypeDetails.ReadXml(reader);
            }

            if (reader.LocalName == "feeder_audit")
            {
                locatable.FeederAudit = new FeederAudit();
                locatable.FeederAudit.ReadXml(reader);
            }
        }

        void ReadXml(XmlReader reader, ExtractChapter chapter)
        {
            // <xs:extension base="LOCATABLE">
            this.ReadXmlBase(reader, chapter as ExtractLocatable);

            // TODO: <xs:element name="directory" type="EXTRACT_FOLDER" minOccurs="0"/>
            if (reader.Name == "directory")
                throw new NotImplementedException("directory not implemented");

            // <xs:element name="content" type="EXTRACT_CONTENT"/>
            Check.Require(reader.LocalName == "content", "content is mandatory");
            string contentType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
            Check.Require(!string.IsNullOrEmpty(contentType), "content type must be specified");
            if (!contentType.EndsWith("EHR_EXTRACT_CONTENT"))
                throw new NotSupportedException("content type " + contentType + " not supported");
            EhrExtractContent content = new EhrExtractContent();
            ReadXml(reader, content);
            chapter.Content = content;

            //<xs:element name="entity_identifier" type="EXTRACT_ENTITY_IDENTIFIER"/>
            Check.Require(reader.LocalName == "entity_identifier", "entity_identifier is mandatory");
            chapter.EntityIdentifier = new ExtractEntityIdentifier();
            ReadXml(reader, chapter.EntityIdentifier);

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        void ReadXml(XmlReader reader, EhrExtract.EhrExtractContent content)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            // TODO: <xs:element name="ehr_access" type="X_VERSIONED_OBJECT" minOccurs="0"/>
            if (reader.Name == "ehr_access")
                throw new NotImplementedException("ehr_access not implemented");
            
            // <xs:element name="ehr_status" type="X_VERSIONED_OBJECT" minOccurs="0"/>
            if (reader.Name == "ehr_status")
            {
                content.EhrStatus = new XmlSerializableXVersionedObject<EhrStatus>();
                ReadXml<EhrStatus>(reader, content.EhrStatus);
            }

            // TODO: <xs:element name="directory" type="X_VERSIONED_OBJECT" minOccurs="0"/>
            if (reader.Name == "directory")
                throw new NotImplementedException("directory not implemented");

            // <xs:element name="compositions" type="X_VERSIONED_OBJECT" minOccurs="0" maxOccurs="unbounded"/>
            if (reader.Name == "compositions")
                content.Compositions = new OpenEhr.AssumedTypes.List<XVersionedObject<OpenEhr.RM.Composition.Composition>>();

            while (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name == "compositions")
            {
                XmlSerializableXVersionedObject<OpenEhr.RM.Composition.Composition> versionedComposition
                    = new XmlSerializableXVersionedObject<OpenEhr.RM.Composition.Composition>();
                ReadXml<OpenEhr.RM.Composition.Composition>(reader, versionedComposition);
                content.Compositions.Add(versionedComposition);
            }

            // TODO: <xs:element name="demographics" type="X_VERSIONED_OBJECT" minOccurs="0" maxOccurs="unbounded"/>
            if (reader.Name == "demographics")
                throw new NotImplementedException("demographics not implemented");

            // <xs:element name="other_items" type="X_VERSIONED_OBJECT" minOccurs="0" maxOccurs="unbounded"/>
            if (reader.Name == "other_items")
                throw new NotImplementedException("other_items not implemented");

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        void ReadXml(XmlReader reader, ExtractEntityIdentifier entityIdentifier)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            //<xs:element name="entity_id" type="HIER_OBJECT_ID" minOccurs="0"/>
            if (reader.Name == "entity_id")
            {
                entityIdentifier.EntityId = new HierObjectId();
                entityIdentifier.EntityId.ReadXml(reader);
            }

            //<xs:element name="subject" type="PARTY_IDENTIFIED" minOccurs="0"/>
            if (reader.Name == "subject")
            {
                if (reader.HasAttributes)
                    throw new NotSupportedException("PARTY_RELATED subject not supporteed");

                entityIdentifier.Subject = new PartyIdentified();
                entityIdentifier.Subject.ReadXml(reader);
            }

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        public void ReadXml<T>(XmlReader reader, XVersionedObject<T> versionedObject) where T : class
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            // <xs:element name="uid" type="HIER_OBJECT_ID"/>
            versionedObject.Uid = new HierObjectId();
            ((System.Xml.Serialization.IXmlSerializable)versionedObject.Uid).ReadXml(reader);

            //<xs:element name="owner_id" type="OBJECT_REF"/>
            versionedObject.OwnerId = new ObjectRef();
            ((System.Xml.Serialization.IXmlSerializable)versionedObject.OwnerId).ReadXml(reader);

            // <xs:element name="time_created" type="DV_DATE_TIME"/>
            versionedObject.TimeCreated = new DvDateTime();
            ((System.Xml.Serialization.IXmlSerializable)versionedObject.TimeCreated).ReadXml(reader);

            //<xs:element name="total_version_count" type="xs:int"/>
            versionedObject.TotalVersionCount 
                = reader.ReadElementContentAsInt("total_version_count", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();

            //<xs:element name="extract_version_count" type="xs:int"/>
            int extractCount
                = reader.ReadElementContentAsInt("extract_version_count", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();

            // TODO: <xs:element name="revision_history" type="REVISION_HISTORY" minOccurs="0"/>

            //<xs:element name="versions" type="ORIGINAL_VERSION" minOccurs="0" maxOccurs="unbounded"/>
            if (reader.LocalName == "versions")
            {
                versionedObject.Versions = new AssumedTypes.List<OriginalVersion<T>>();

                while (reader.LocalName == "versions")
                {
                    OriginalVersion<T> version = new OriginalVersion<T>();
                    ((System.Xml.Serialization.IXmlSerializable)version).ReadXml(reader);
                    versionedObject.Versions.Add(version);
                }
            }

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        #endregion

        #region WriteXml

        public void WriteXml(XmlWriter writer, Extract extract)
        {
            RmXmlSerializer.UseOpenEhrPrefix(writer);
            RmXmlSerializer.UseXsiPrefix(writer);

            // <xs:extension base="LOCATABLE">
            this.WriteXmlBase(writer, extract as ExtractLocatable);

            // <xs:element name="request_id" type="OBJECT_REF" minOccurs="0"/>
            if (extract.RequestId != null)
            {
                writer.WriteStartElement("request_id", RmXmlSerializer.OpenEhrNamespace);
                ((System.Xml.Serialization.IXmlSerializable)extract.RequestId).WriteXml(writer);
                writer.WriteEndElement();
            }

            // <xs:element name="time_created" type="DV_DATE_TIME"/>
            writer.WriteStartElement("time_created", RmXmlSerializer.OpenEhrNamespace);
            ((System.Xml.Serialization.IXmlSerializable)extract.TimeCreated).WriteXml(writer);
            writer.WriteEndElement();

            // <xs:element name="system_id" type="HIER_OBJECT_ID"/>
            writer.WriteStartElement("system_id", RmXmlSerializer.OpenEhrNamespace);
            ((System.Xml.Serialization.IXmlSerializable)extract.SystemId).WriteXml(writer);
            writer.WriteEndElement();

            // <xs:element name="participations" type="PARTICIPATION" minOccurs="0" maxOccurs="unbounded"/>
            if (extract.Participations != null)
            {
                foreach (Participation participation in extract.Participations)
                {
                    writer.WriteStartElement("participations", RmXmlSerializer.OpenEhrNamespace);
                    ((System.Xml.Serialization.IXmlSerializable)participation).WriteXml(writer);
                    writer.WriteEndElement();
                }
            }

            // <xs:element name="sequence_nr" type="xs:long"/>
            writer.WriteStartElement("sequence_nr", RmXmlSerializer.OpenEhrNamespace);
            writer.WriteValue(extract.SequenceNr);
            writer.WriteEndElement();

            // <xs:element name="chapters" type="EXTRACT_CHAPTER" minOccurs="0" maxOccurs="unbounded"/>
            foreach (ExtractChapter chapter in extract.Chapters)
            {
                writer.WriteStartElement("chapters", RmXmlSerializer.OpenEhrNamespace);
                this.WriteXml(writer, chapter);
                writer.WriteEndElement();
            }

            // TODO:  <xs:element name="specification" type="EXTRACT_SPEC" minOccurs="0"/>
        }

        void WriteXml(XmlWriter writer, ExtractChapter chapter)
        {
            // <xs:extension base="LOCATABLE">
            this.WriteXmlBase(writer, chapter as ExtractLocatable);

            // TODO: <xs:element name="directory" type="EXTRACT_FOLDER" minOccurs="0"/>
            
            // <xs:element name="content" type="EXTRACT_CONTENT"/>
            writer.WriteStartElement("content", RmXmlSerializer.OpenEhrNamespace);
            this.WriteXml(writer, chapter.Content);
            writer.WriteEndElement();

            //<xs:element name="entity_identifier" type="EXTRACT_ENTITY_IDENTIFIER"/>
            writer.WriteStartElement("entity_identifier", RmXmlSerializer.OpenEhrNamespace);
            this.WriteXml(writer, chapter.EntityIdentifier);
            writer.WriteEndElement();
        }

        void WriteXml(XmlWriter writer, ExtractEntityIdentifier entityIdentifier)
        {
            //<xs:element name="entity_id" type="HIER_OBJECT_ID" minOccurs="0"/>
            if (entityIdentifier.EntityId != null)
            {
                writer.WriteStartElement("entity_id", RmXmlSerializer.OpenEhrNamespace);
                ((System.Xml.Serialization.IXmlSerializable)entityIdentifier.EntityId).WriteXml(writer);
                writer.WriteEndElement();
            }

            //<xs:element name="subject" type="PARTY_IDENTIFIED" minOccurs="0"/>
            if (entityIdentifier.Subject != null)
            {
                writer.WriteStartElement("subject", RmXmlSerializer.OpenEhrNamespace);
                ((System.Xml.Serialization.IXmlSerializable)entityIdentifier.Subject).WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        void WriteXml(XmlWriter writer, ExtractEntityContent content)
        {
            EhrExtract.EhrExtractContent ehrContent = content as EhrExtract.EhrExtractContent;
            if (ehrContent != null)
                WriteXml(writer, ehrContent);
            else
                throw new NotImplementedException("ExtractEntityContent sub-type not implemented: " + content.GetType().ToString());
        }

        void WriteXml(XmlWriter writer, EhrExtract.EhrExtractContent content)
        {
            WriteXmlBase(writer, content, "EHR_EXTRACT_CONTENT");

            // TODO: <xs:element name="ehr_access" type="X_VERSIONED_OBJECT" minOccurs="0"/>

            // <xs:element name="ehr_status" type="X_VERSIONED_OBJECT" minOccurs="0"/>
            if (content.EhrStatus != null)
            {
                writer.WriteStartElement("ehr_status", RmXmlSerializer.OpenEhrNamespace);
                this.WriteXml(writer, content.EhrStatus);
                writer.WriteEndElement();
            }

            // TODO: <xs:element name="directory" type="X_VERSIONED_OBJECT" minOccurs="0"/>

            // <xs:element name="compositions" type="X_VERSIONED_OBJECT" minOccurs="0" maxOccurs="unbounded"/>
            if (content.Compositions != null)
            {
                foreach (XVersionedObject<Composition.Composition> versionedComposition in content.Compositions)
                {
                    writer.WriteStartElement("compositions", RmXmlSerializer.OpenEhrNamespace);
                    this.WriteXml(writer, versionedComposition);
                    writer.WriteEndElement();
                }
            }

            // TODO: <xs:element name="demographics" type="X_VERSIONED_OBJECT" minOccurs="0" maxOccurs="unbounded"/>

            // <xs:element name="other_items" type="X_VERSIONED_OBJECT" minOccurs="0" maxOccurs="unbounded"/>
            if (content.OtherItems != null)
            {
                foreach (XVersionedObject<RM.Common.Archetyped.Impl.Locatable> versionedObject in content.OtherItems)
                {
                    writer.WriteStartElement("other_items", RmXmlSerializer.OpenEhrNamespace);
                    this.WriteXml(writer, versionedObject);
                    writer.WriteEndElement();
                }
            }
        }

        void WriteXmlBase(XmlWriter writer, ExtractEntityContent content, string typeName)
        {
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string oePrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            if (!string.IsNullOrEmpty(oePrefix))
                typeName = oePrefix + ":" + typeName;

            writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, typeName);
        }

        void WriteXmlBase(XmlWriter writer, ExtractLocatable locatable)
        {
            // <xs:attribute name="archetype_node_id" type="archetypeNodeId" use="required"/>
            writer.WriteAttributeString("archetype_node_id", locatable.ArchetypeNodeId);

            //<xs:element name="name" type="DV_TEXT"/>
            writer.WriteStartElement("name", RmXmlSerializer.OpenEhrNamespace);
            RmXmlSerializer.WriteXml(writer, locatable.Name);
            writer.WriteEndElement();

            //<xs:element name="uid" type="UID_BASED_ID" minOccurs="0"/>
            if (locatable.Uid != null)
            {
                writer.WriteStartElement("uid", RmXmlSerializer.OpenEhrNamespace);
                RmXmlSerializer.WriteXml(writer, locatable.Uid);
                writer.WriteEndElement();
            }

            //<xs:element name="links" type="LINK" minOccurs="0" maxOccurs="unbounded"/>
            if (locatable.Links != null)
            {
                foreach (Link link in locatable.Links)
                {
                    writer.WriteStartElement("links", RmXmlSerializer.OpenEhrNamespace);
                    ((System.Xml.Serialization.IXmlSerializable)link).WriteXml(writer);
                    writer.WriteEndElement();
                }
            }

            //<xs:element name="archetype_details" type="ARCHETYPED" minOccurs="0"/>
            if (locatable.ArchetypeDetails != null)
            {
                writer.WriteStartElement("archetype_details", RmXmlSerializer.OpenEhrNamespace);
                ((System.Xml.Serialization.IXmlSerializable)locatable.ArchetypeDetails).WriteXml(writer);
                writer.WriteEndElement();
            }

            //<xs:element name="feeder_audit" type="FEEDER_AUDIT" minOccurs="0"/>
            if (locatable.FeederAudit != null)
            {
                writer.WriteStartElement("feeder_audit", RmXmlSerializer.OpenEhrNamespace);
                ((System.Xml.Serialization.IXmlSerializable)locatable.FeederAudit).WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        public void WriteXml<T>(XmlWriter writer, XVersionedObject<T> versionedObject) where T:class 
        {
            // <xs:element name="uid" type="HIER_OBJECT_ID"/>
            writer.WriteStartElement("uid", RmXmlSerializer.OpenEhrNamespace);
            ((System.Xml.Serialization.IXmlSerializable)versionedObject.Uid).WriteXml(writer);
            writer.WriteEndElement();

            //<xs:element name="owner_id" type="OBJECT_REF"/>
            writer.WriteStartElement("owner_id", RmXmlSerializer.OpenEhrNamespace);
            RmXmlSerializer.WriteXml(writer, versionedObject.OwnerId);
            writer.WriteEndElement();

            // <xs:element name="time_created" type="DV_DATE_TIME"/>
            writer.WriteStartElement("time_created", RmXmlSerializer.OpenEhrNamespace);
            ((System.Xml.Serialization.IXmlSerializable)versionedObject.TimeCreated).WriteXml(writer);
            writer.WriteEndElement();

            //<xs:element name="total_version_count" type="xs:int"/>
            writer.WriteStartElement("total_version_count", RmXmlSerializer.OpenEhrNamespace);
            writer.WriteValue(versionedObject.TotalVersionCount);
            writer.WriteEndElement();

            //<xs:element name="extract_version_count" type="xs:int"/>
            writer.WriteStartElement("extract_version_count", RmXmlSerializer.OpenEhrNamespace);
            writer.WriteValue(versionedObject.ExtractVersionCount);
            writer.WriteEndElement();

            // TODO: <xs:element name="revision_history" type="REVISION_HISTORY" minOccurs="0"/>

            //<xs:element name="versions" type="ORIGINAL_VERSION" minOccurs="0" maxOccurs="unbounded"/>
            if (versionedObject.Versions != null)
            {
                foreach (OriginalVersion<T> version in versionedObject.Versions)
                {
                    writer.WriteStartElement("versions", RmXmlSerializer.OpenEhrNamespace);
                    ((System.Xml.Serialization.IXmlSerializable)version).WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }
        
        #endregion
    }
}
