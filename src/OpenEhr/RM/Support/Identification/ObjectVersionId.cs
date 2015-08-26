using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Support.Identification
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "SUPPORT", "OBJECT_VERSION_ID")]
    public class ObjectVersionId : UidBasedId, System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>
        /// Create first version of a new object version ID with specified creating system ID
        /// </summary>
        /// <param name="creatingSystemId">Creating system ID</param>
        /// <returns>First version of an object version ID</returns>
        public static ObjectVersionId CreateNew(HierObjectId creatingSystemId)
        {
            return new ObjectVersionId(HierObjectId.NewObjectId(), creatingSystemId.Value, new VersionTreeId());
        }

        /// <summary>
        /// Create new ObjectVersionId that is the next trunk version of the precedingVersionUid 
        /// with the same CreatingSystemId
        /// </summary>
        /// <param name="precedingVersionUid">Preceding version UID</param>
        /// <returns>Next trunk object version of the preceding version UID</returns>
        public static ObjectVersionId CreateNew(ObjectVersionId precedingVersionUid)
        {
            long trunkVersion;
            if (!long.TryParse(precedingVersionUid.VersionTreeId.Value, out trunkVersion))
                throw new NotSupportedException("Branched version tree IDs not supported");
            trunkVersion++;

            ObjectVersionId result = new ObjectVersionId(precedingVersionUid.ObjectId, 
                precedingVersionUid.CreatingSystemId, new VersionTreeId(trunkVersion.ToString()));

            Check.Ensure(precedingVersionUid.ObjectId.Equals(result.ObjectId),
                "result objectId must equal preceding objectId");
            Check.Ensure(precedingVersionUid.CreatingSystemId == result.CreatingSystemId,
                "result creatingSystemId must equal preceding creatingSystemId");
            Check.Ensure(long.Parse(result.VersionTreeId.Value) > long.Parse(precedingVersionUid.VersionTreeId.Value), 
                "result VersionTreeId must be greater than preceding versionTreeId");
            return result;
        }

        public ObjectVersionId()
        { }

        public ObjectVersionId(string value)
            : this()
        {
            SetBaseData(value);
        }

        public ObjectVersionId(Uid objectId, HierObjectId creatingSystemId, VersionTreeId versionTreeId)
            : this(objectId.Value + "::" + creatingSystemId.Value + "::" + versionTreeId.Value)
        { }

        public ObjectVersionId(HierObjectId objectId, string creatingSystemId, VersionTreeId versionTreeId)
            : this(objectId.Value + "::" + creatingSystemId + "::" + versionTreeId.Value)
        { }

        Uid objectId;

        public Uid ObjectId
        {
            get
            {
                if (objectId == null)
                {
                    string value = this.Value;
                    int i = value.IndexOf("::");

                    // CM: 16/04/2008 Uid is abstract
                    objectId = Uid.Create(value.Substring(0, i));
                }
                return objectId;
            }
        }

        public VersionTreeId VersionTreeId
        {
            get
            {
                string value = this.Value;
                int i = value.LastIndexOf("::") + 2;

                return new VersionTreeId(value.Substring(i, value.Length - i));
            }
        }

        public HierObjectId CreatingSystemId
        {
            get
            {
                string value = this.Value;
                int i = value.IndexOf("::") + 2;
                int j = value.IndexOf("::", i);

                return new HierObjectId( value.Substring(i, j-i));
            }
        }

        public bool IsBranch()
        {
            return (VersionTreeId.BranchNumber != null);
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

        const string RmTypeName = "OBJECT_VERSION_ID";

        public static bool IsValid(string value)
        {
            Check.Require(value != null, "value must not be null");

            string[] strings = value.Split(new string[] { "::" }, 3, StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length == 0)
                return false;

            if (strings.Length != 3)
                return false;

            if (!Uid.IsValid(strings[0]))
                return false;

            if (!Uid.IsValid(strings[1]))
                return false;

            if (!VersionTreeId.IsValid(strings[2]))
                return false;

            return true;
        }

        protected override bool IsValidValue(string value)
        {
            return IsValid(value);
        }
    }
}
