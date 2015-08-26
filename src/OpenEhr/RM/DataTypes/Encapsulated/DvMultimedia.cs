using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Encapsulated
{
    [Serializable]
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [RmType("openEHR", "DATA_TYPES", "DV_MULTIMEDIA")]
    public class DvMultimedia : DvEncapsulated, System.Xml.Serialization.IXmlSerializable
    {
        public DvMultimedia() 
        { }

        public DvMultimedia(Text.CodePhrase charset, Text.CodePhrase language,
            string alternateText, Uri.DvUri uri, byte[] data, Text.CodePhrase mediaType,
            Text.CodePhrase compressionAlgorithm, byte[] integrityCheck,
            Text.CodePhrase integrityCheckAlgorithm, int size, DvMultimedia thumbnail)
            : this()
        {
            this.SetBaseData(charset, language);
            this.alternateText = alternateText;
            this.uri = uri;
            this.data = data;
            this.mediaType = mediaType;
            this.compressionAlgorithm = compressionAlgorithm;
            this.integrityCheck = integrityCheck;
            this.integrityCheckAlgorithm = integrityCheckAlgorithm;
            this.size = size;

            this.CheckInvariants();
        }

        private Text.CodePhrase mediaType;

        [RmAttribute("media_type")]
        [RmCodeset("media types", "IANA_media-types")]
        public Text.CodePhrase MediaType
        {
            get
            {
                return this.mediaType;
            }
        }

        private byte[] data;

        public byte[] Data
        {
            get
            {
                return this.data;
            }
        }

        private Uri.DvUri uri;

        public Uri.DvUri Uri
        {
            get
            {
                return this.uri;
            }
        }

        private string alternateText;

        public string AlternateText
        {
            get
            {
                return this.alternateText;
            }
        }

        private Text.CodePhrase compressionAlgorithm;

        [RmAttribute("compression_algorithm")]
        [RmCodeset("compression algorithms", "openehr_compression_algorithms")]
        public Text.CodePhrase CompressionAlgorithm
        {
            get
            {
              return this.compressionAlgorithm;
            }
        }

        private byte[] integrityCheck;

        public byte[] IntegrityCheck
        {
            get
            {
                return this.integrityCheck;
            }
        }
        private Text.CodePhrase integrityCheckAlgorithm;

        [RmAttribute("integrity_check_algorithm")]
        [RmCodeset("integrity check algorithms", "openehr_integrity_check_algorithms")]
        public Text.CodePhrase IntegrityCheckAlgorithm
        {
            get
            {
              return this.integrityCheckAlgorithm;
            }
        }

        private int size;
        private bool sizeSet;

        public override int Size
        {
            get
            {
                return this.size;
            }
        }

        private DvMultimedia thumbnail;

        public DvMultimedia Thumbnail
        {
            get{
                return this.thumbnail;
            }
        }

        public override string ToString()
        {
            string rtfString = "";

            switch (this.MediaType.CodeString)
            {
                case "text/rtf":
                    System.Text.Encoding utf8Encoding = System.Text.Encoding.UTF8;
                    rtfString = utf8Encoding.GetString(this.Data);
                    break;
            }

            return rtfString;
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
            return new System.Xml.XmlQualifiedName("DV_MULTIMEDIA", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(System.Xml.XmlReader reader)
        {           
            base.ReadXmlBase(reader);

            if (reader.LocalName == "alternate_text")
                this.alternateText = reader.ReadElementString("alternate_text", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName == "uri")
            {
                string uriType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                if (uriType != null)
                {
                    Check.Assert(uriType.IndexOf("DV_EHR_URI")>0, "uriType must be DV_EHR_URI, but it is "+uriType);
                    this.uri = new OpenEhr.RM.DataTypes.Uri.DvEhrUri();
                }
                this.uri = new OpenEhr.RM.DataTypes.Uri.DvUri();
                this.uri.ReadXml(reader);
            }

            if (reader.LocalName == "data")
            {
                reader.MoveToContent();
                reader.ReadStartElement();
                byte[] buffer = new byte[1024];
                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream);
                int bytesRead = reader.ReadContentAsBase64(buffer, 0, 1024);
                while (bytesRead > 0)
                {
                    writer.Write(buffer, 0, bytesRead);
                    bytesRead = reader.ReadContentAsBase64(buffer, 0, 1024);
                }
                writer.Close();
                reader.MoveToContent();
                reader.ReadEndElement();
                this.data = stream.ToArray();
            }
            reader.MoveToContent();

            if (reader.LocalName == "media_type")
            {
                this.mediaType = new OpenEhr.RM.DataTypes.Text.CodePhrase();
                this.mediaType.ReadXml(reader);
            }

            if (reader.LocalName == "compression_algorithm")
            {
                this.compressionAlgorithm = new OpenEhr.RM.DataTypes.Text.CodePhrase();
                this.compressionAlgorithm.ReadXml(reader);
            }

            if (reader.LocalName == "integrity_check")
            {
                reader.ReadElementContentAsBase64(this.integrityCheck, 0, this.integrityCheck.Length);
            }
            reader.MoveToContent();

            if (reader.LocalName == "integrity_check_algorithm")
            {
                this.integrityCheckAlgorithm = new OpenEhr.RM.DataTypes.Text.CodePhrase();
                this.integrityCheckAlgorithm.ReadXml(reader);
            }

            Check.Assert(reader.LocalName=="size", "Expected LocalName is 'size', not " + reader.LocalName);
            this.size = reader.ReadElementContentAsInt("size", RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();

            if (reader.LocalName == "thumbnail")
            {
                this.thumbnail = new DvMultimedia();
                this.thumbnail.ReadXml(reader);
            }
        }

        protected override void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            if (this.AlternateText != null)
                writer.WriteElementString(openEhrPrefix, "alternate_text", RmXmlSerializer.OpenEhrNamespace, this.AlternateText);
            if (this.Uri != null)
            {
                writer.WriteStartElement(openEhrPrefix, "uri", RmXmlSerializer.OpenEhrNamespace);
                if (this.Uri.GetType() == typeof(OpenEhr.RM.DataTypes.Uri.DvEhrUri))
                {
                    string ehrUriType = "DV_EHR_URI";
                    if(!string.IsNullOrEmpty(openEhrPrefix))
                        ehrUriType = openEhrPrefix + ":" + ehrUriType;
                    writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, ehrUriType);
                }
                this.Uri.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.Data != null)
            {
                writer.WriteStartElement(openEhrPrefix, "data", RmXmlSerializer.OpenEhrNamespace);
                writer.WriteBase64(this.Data, 0, this.Data.Length);
                writer.WriteEndElement();
            }

            if (this.MediaType != null)
            {
                writer.WriteStartElement(openEhrPrefix, "media_type", RmXmlSerializer.OpenEhrNamespace);
                this.MediaType.WriteXml(writer);
                writer.WriteEndElement();
            }

            if(this.CompressionAlgorithm != null)
            {
                writer.WriteStartElement(openEhrPrefix, "compression_algorithm", RmXmlSerializer.OpenEhrNamespace);
                this.CompressionAlgorithm.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.IntegrityCheck != null)
            {
                writer.WriteStartElement(openEhrPrefix, "integrity_check", RmXmlSerializer.OpenEhrNamespace);
                writer.WriteBase64(this.IntegrityCheck, 0, this.IntegrityCheck.Length);
                writer.WriteEndElement();
            }

            if (this.IntegrityCheckAlgorithm != null)
            {
                writer.WriteStartElement(openEhrPrefix, "integrity_check_algorithm", RmXmlSerializer.OpenEhrNamespace);
                this.IntegrityCheckAlgorithm.WriteXml(writer);
                writer.WriteEndElement();
            }

            writer.WriteElementString(openEhrPrefix, "size", RmXmlSerializer.OpenEhrNamespace, this.Size.ToString());

            if (this.Thumbnail != null)
            {
                writer.WriteStartElement(openEhrPrefix, "thumbnail", RmXmlSerializer.OpenEhrNamespace);
                this.Thumbnail.WriteXml(writer);
                writer.WriteEndElement();
            }            
        }

        protected override void CheckInvariants()
        {
            base.CheckInvariants();

            // %HYYKA%
            //Check.Invariant(this.IntegrityCheck == null || this.IntegrityCheckAlgorithm != null, 
            //    "Integrity_check_validity: integrity_check /= Void implies integrity_check_algorithm /= Void");
        }
    }
}
