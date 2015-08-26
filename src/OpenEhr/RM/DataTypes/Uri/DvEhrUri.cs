using System;
using System.Text.RegularExpressions;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.DataTypes.Uri
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_TYPES", "DV_EHR_URI")]
    public class DvEhrUri : DvUri, System.Xml.Serialization.IXmlSerializable
    {

        private static string DateTimePattern = @"\d{4}\-\d{2}-\d{2}T\d{2}\:\d{2}\:\d{2}"; //format is yyyy-MM-ddTHH:mm:ss
        private static string DomainOrIdPattern = @"[0-9a-zA-Z\-\._]+";
        private static string GuidPattern = @"[0-9a-zA-Z\-]+";  // TODO: this must support OID and NETWORK_ID as weel
        private static string RevisionNamePattern = @"latest_trunk_version|latest_version";
        private static string VersionTreeIdPattern = @"[0-9\.]+";

        private static string EhrLocatorPattern = @"//(?<ehrUid>" + GuidPattern + @")(?:@(?<ehrLocation>" + DomainOrIdPattern + "))?";
        private static string VersionUidPattern = @"(?<versionedObjectUid>" + GuidPattern + @")::(?<creatingSystemId>" + DomainOrIdPattern + @")::(?<versionTreeId>" + VersionTreeIdPattern + ")";
        private static string VersionAtRevisionIdPattern = @"(?<versionedObjectUid>" + GuidPattern + @")(@(?:(?<revisionTime>" + DateTimePattern + @")|(?<revisionName>" + RevisionNamePattern + "))?)";
        private static string StructureLocatorPattern = @"/(?:(?<versionUid>" + VersionUidPattern + @")|(?<versionLocator>" + VersionAtRevisionIdPattern + "))";
        private static string EhrPathPattern = @"(?<ehrPath>/.*)";

        public static string EhrUriPattern = @"^ehr:(?:(?:" + EhrLocatorPattern + ")?" + StructureLocatorPattern + EhrPathPattern + "?)|(?:" + EhrLocatorPattern + "/?)$";


        public DvEhrUri()
        { }

        public DvEhrUri(string uriValue)
            : this()
        {
            DesignByContract.Check.Require(IsValidEhrUri(uriValue), "Wrong EhrUri pattern: " + uriValue);

            this.SetBaseData(uriValue);
            this.CheckInvariants();
        }

        public static bool IsValidEhrUri(string ehrUri)
        {
            return Regex.IsMatch(ehrUri, EhrUriPattern, RegexOptions.Compiled | RegexOptions.Singleline);
        }

        public static System.Xml.XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadBaseTypesSchema(xs);
            return new System.Xml.XmlQualifiedName("DV_EHR_URI", RmXmlSerializer.OpenEhrNamespace);
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

        public override bool Equals(object obj)
        {
            // If parameter is null or not a DvUri return false.
            DvEhrUri uri = obj as DvEhrUri;
            if ((object)uri == null)
                return false;

            // support inexact value equality, e.g. no authority (ehr/systemId)
            if (string.IsNullOrEmpty(uri.Authority))
            {
                if (string.IsNullOrEmpty(this.Authority))
                    return this.Value == uri.Value;
                else
                {
                    string path = this.Value.Substring(this.Authority.Length + 6);
                    return (path == uri.Value.Substring(4));
    }
            }
            else if (string.IsNullOrEmpty(this.Authority))
            {
                string path =  uri.Value.Substring(uri.Authority.Length + 6);
                return (path == this.Value.Substring(4));
            }
            else
                return this.Value == uri.Value;
        }
    }
}
