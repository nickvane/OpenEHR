using System;
using OpenEhr.DesignByContract;
using System.Runtime.InteropServices;
using OpenEhr.Attributes;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.Common.Generic;
using OpenEhr.RM.Common.ChangeControl;
using OpenEhr.AssumedTypes;
using OpenEhr.RM.Composition.Content.Entry;
using OpenEhr.RM.DataTypes.Quantity.DateTime;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.RM.DataStructures.History;
using OpenEhr.Utilities;
using OpenEhr.RM.Common.Archetyped;
using OpenEhr.AssumedTypes.Impl;
using OpenEhr.RM.Common.Resource;
using OpenEhr.Constants;
using OpenEhr.RM.Impl;
using OpenEhr.RM.DataTypes.Quantity;
using OpenEhr.RM.Ehr;
using OpenEhr.RM.Composition;
using OpenEhr.RM.DataTypes.Basic;
using OpenEhr.RM.DataTypes.Encapsulated;

namespace OpenEhr.Factories
{
    public static class RmFactory
    {
        # region RmFactory Instance Singleton

        private static OpenEhrFactoryInstance instance;
        public static OpenEhrFactoryInstance Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new OpenEhrFactoryInstance();
                }
                return instance;
            }
        }

        #endregion

        [Obsolete("Use RmType.GetRmTypeName")]
        public static string GetRmTypeName(Type rmType)
        {
            Check.Require(rmType != null, "rmType must not be null");

            object[] rmTypeAttributes = rmType.GetCustomAttributes(typeof(RmTypeAttribute), true);
            if (rmTypeAttributes == null)
                throw new ApplicationException("rmTypeAttributes must not be null");
            if (rmTypeAttributes.Length < 1)
                return null;

            return ((RmTypeAttribute)rmTypeAttributes[0]).RmEntity;
        }

        [Obsolete("Use RmType.GetRmTypeName")]
        public static string GetRmTypeName(IRmType rmType)
        {
            Check.Require(rmType != null, "rmType must not be null");
            return rmType.GetRmTypeName();
        }

        public static OriginalVersion<T> Version<T>(HierObjectId contributionUid, AuditDetails commitAudit, T data)
            where T : class
        {
            Check.Require(contributionUid != null, "contributionUid must not be null");

            ObjectVersionId uid = ObjectVersionId.CreateNew(new HierObjectId(commitAudit.SystemId));

            ObjectRef contribution 
                = new ObjectRef(contributionUid, commitAudit.SystemId, GetRmTypeName(typeof(Contribution)));

            OriginalVersion<T> version = new OriginalVersion<T>(uid, null,
                Instance.DvCodedText(VersionLifecycleState.complete), commitAudit, contribution, data);

            return version;
        }

        internal static T Clone<T>(T obj)
            where T : class
        {
            if (obj == null) 
                return obj;

            T clone = null;
            using (System.IO.MemoryStream memory = new System.IO.MemoryStream())
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter
                    = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(memory, obj);
                memory.Position = 0;
                clone = formatter.Deserialize(memory) as T;
            }

            Check.Ensure(clone != null, "clone must not be null");
            return clone;
        }

        #region Create EHR types

        static public EhrStatus EhrStatus(string subjectId, string subjectIdScheme, string subjectNamespace)
        {
            return Instance.EhrStatus(subjectId, subjectIdScheme, subjectNamespace);
        }

        #endregion

        static public void SetObservationHistoryOriginFromEvents(Composition composition)
        {
            Check.Require(composition != null, "composition must not be null");

            string observationPath = @"//*[{/openEHR-EHR-OBSERVATION\.\w+(-\w+)*\.[vV]\d*/}]";

            // get all observations
            // TODO: use composition.ItemsAtPath once it supports the path above
            Paths.ArchetypedPathProcessor archetypePathProcessor
                = new Paths.ArchetypedPathProcessor(composition);

            // need to call PathExist to ensure observationPath existing. Otherwise, 
            // will get an exception
            if (archetypePathProcessor.PathExists(observationPath))
            {
                List<object> observations = archetypePathProcessor.ItemsAtPath(observationPath);
                if (observations == null || observations.Count == 0)
                    throw new ApplicationException("observations must not be null or empty.");

                foreach (Observation observation in observations)
                {
                    if (observation.State != null)
                        throw new NotSupportedException("Setting OBSERVATION state origin from events not supported");

                    if (observation.Data.Origin == null)
                    {
                        observation.Data.Origin = History<ItemStructure>.CalculateOrigin(observation.Data);
                    }
                    Check.Ensure(observation.Data.Origin != null, "origin must not be null");
                }
            }
        }


        #region Create Generic types

        static public PartyProxy PartyProxy(string rmTypeName)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(rmTypeName), "rmTypeName must not be null or empty.");

            switch (rmTypeName)
            {
                case "PARTY_IDENTIFIED":
                    return new PartyIdentified();
                case "PARTY_SELF":
                    return new PartySelf();
                case "PARTY_RELATED":
                    return new PartyRelated();
            }
            throw new ApplicationException("Party proxy type name not recognised: " + rmTypeName);
        }

        static public AuditDetails AuditDetails(string committerName, AuditChangeType changeType)
        {
            return Instance.AuditDetails(committerName, changeType);
        }

        static public PartySelf PartySelf(string subjectId, string subjectIdScheme, string subjectIdNamespace)
        {
            return new PartySelf(new PartyRef(new GenericId(subjectId, subjectIdScheme), subjectIdNamespace, "PERSON"));
        }

        static public PartyIdentified PartyIdentified(string name)
        {
            return Instance.PartyIdentified(name);
        }

        static public PartyIdentified PersonIdentified(string name, string externalRefNamespace,
            ObjectId externalRefId)
        {
            return Instance.PersonIdentified(name, externalRefNamespace, externalRefId);
        }

        #endregion

        #region Create Identification types

        static public GenericId GenericId(string value, string scheme)
        {
            return new GenericId(value, scheme);
        }

        static public HierObjectId HierObjectId(string value)
        {
            return new HierObjectId(value);
        }

        static public ArchetypeId ArchetypeId(string value)
        {
            return Instance.ArchetypeId(value);
        }

        static public UidBasedId UidBasedId(string value)
        {
            return Instance.UidBasedId(value);
        }

        static public Uid Uid(string value)
        {
            return RM.Support.Identification.Uid.Create(value);
        }

        static public ObjectRef ObjectRef(ObjectId objectId, string @namespace, Type rmType)
        {
            return new ObjectRef(objectId, @namespace, GetRmTypeName(rmType));
        }

        #endregion

        #region DataTypes

        static public DataValue DataValue(string typeName)
        {
            Check.Require(!string.IsNullOrEmpty(typeName), "type must not be null or empty.");

            OpenEhr.RM.DataTypes.Basic.DataValue dataValue = null;

            switch (typeName)
            {
                case "DV_QUANTITY":
                    dataValue = new OpenEhr.RM.DataTypes.Quantity.DvQuantity();
                    break;
                case "DV_COUNT":
                    dataValue = new OpenEhr.RM.DataTypes.Quantity.DvCount();
                    break;
                case "DV_ORDINAL":
                    dataValue = new OpenEhr.RM.DataTypes.Quantity.DvOrdinal();
                    break;
                case "DV_PROPORTION":
                    dataValue = new OpenEhr.RM.DataTypes.Quantity.DvProportion();
                    break;
                case "DV_TEXT":
                    dataValue = new OpenEhr.RM.DataTypes.Text.DvText();
                    break;
                case "DV_CODED_TEXT":
                    dataValue = new OpenEhr.RM.DataTypes.Text.DvCodedText();
                    break;
                case "DV_BOOLEAN":
                    dataValue = new OpenEhr.RM.DataTypes.Basic.DvBoolean();
                    break;
                case "DV_IDENTIFIER":
                    dataValue = new OpenEhr.RM.DataTypes.Basic.DvIdentifier();
                    break;
                case "DV_DATE_TIME":
                    dataValue = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvDateTime();
                    break;
                case "DV_DATE":
                    dataValue = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvDate();
                    break;
                case "DV_TIME":
                    dataValue = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvTime();
                    break;
                case "DV_DURATION":
                    dataValue = new OpenEhr.RM.DataTypes.Quantity.DateTime.DvDuration();
                    break;
                case "DV_URI":
                    dataValue = new OpenEhr.RM.DataTypes.Uri.DvUri();
                    break;
                case "DV_EHR_URI":
                    dataValue = new OpenEhr.RM.DataTypes.Uri.DvEhrUri();
                    break;
                case "DV_PARSABLE":
                    dataValue = new OpenEhr.RM.DataTypes.Encapsulated.DvParsable();
                    break;
                case "DV_MULTIMEDIA":
                    dataValue = new OpenEhr.RM.DataTypes.Encapsulated.DvMultimedia();
                    break;
                default:
                    throw new NotSupportedException("type not implemented: " + typeName);
            }
            return dataValue;
        }

        static public DvCodedText DvCodedText(AuditChangeType changeType)
        {
            return Instance.DvCodedText(changeType);
        }

        static public DvCodedText DvCodedText(VersionLifecycleState lifecycleState)
        {
            return Instance.DvCodedText(lifecycleState);
        }

        static public DvCodedText DvCodedText(string value, string codeString, string terminologyIdValue)
        {
            return Instance.DvCodedText(value, codeString, terminologyIdValue);
        }

        static public DvOrdinal DvOrdinal(int value, string symbolValue, string symbolCodeString)
        {
            return new DvOrdinal(value, Instance.DvCodedText(symbolValue, symbolCodeString, "local"), null, null);
        }
        #endregion

        #region feeder audit

        public static FeederAudit FeederAudit
            (string systemId, DvEncapsulated originalContent)
        {
            FeederAuditDetails originatingSystemAudit = new FeederAuditDetails(systemId);
            return new RM.Common.Archetyped.Impl.FeederAudit(originatingSystemAudit, null, originalContent);
        }

        public static FeederAudit FeederAudit
           (FeederAuditDetails originatingSystemAudit,
            System.Collections.Generic.List<DvIdentifier> originatingSystemItemIds,
            FeederAuditDetails feederSystemAudit,
            System.Collections.Generic.List<DvIdentifier> feederSystemItemIds,
            DvEncapsulated originalContent)
        {
            List<OpenEhr.RM.DataTypes.Basic.DvIdentifier> origSystemItemIds = null;
            if (originatingSystemItemIds != null)
                origSystemItemIds = new List<OpenEhr.RM.DataTypes.Basic.DvIdentifier>(originatingSystemItemIds);

            List<OpenEhr.RM.DataTypes.Basic.DvIdentifier> feederSystemItemIdsOpenehrList = null;
            if (feederSystemItemIds != null)
                feederSystemItemIdsOpenehrList = new List<OpenEhr.RM.DataTypes.Basic.DvIdentifier>(feederSystemItemIds);

            return new OpenEhr.RM.Common.Archetyped.Impl.FeederAudit(originatingSystemAudit, 
                origSystemItemIds, feederSystemAudit, 
                feederSystemItemIdsOpenehrList, originalContent);
        }

        #endregion

        #region Support types

        static public Set<T> Set<T>()
            where T : OpenEhr.RM.Common.Archetyped.Impl.DemographicLocatable
        {
            return new LocatableSet<T>();
        }

        static public List<T> List<T>(Pathable parent)
            where T: OpenEhr.RM.Common.Archetyped.Impl.Locatable
        {
            return new LocatableList<T>(parent, null);
        }

        static internal List<T> List<T>(Pathable parent, System.Collections.Generic.IEnumerable<T> items)
           where T : OpenEhr.RM.Common.Archetyped.Impl.Locatable
        {
            LocatableList<T> locatableList = new LocatableList<T>(parent, items);

            return locatableList;
        }

        internal static AssumedTypes.Impl.LocatableList<T> LocatableList<T>(Pathable parent, T[] items) 
            where T : OpenEhr.RM.Common.Archetyped.Impl.Locatable
        {
            Check.Require(items != null, "items must not be null");

            return new LocatableList<T>(parent, items);
        }

        #endregion


        /// <summary>
        /// Get the attribute name or class name used in our implementation with a given RM atrribute name
        /// </summary>
        /// <param name="referenceModelAttribute"></param>
        /// <returns></returns>
        internal static string GetOpenEhrV1RmName(string rmName)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(rmName),
                "rmName must not be null or empty.");

            switch (rmName)
            {
                case "null_flavor":
                    rmName = "null_flavour";
                    break;
            }
            
            if (rmName.IndexOf("_") >= 0)
            {
                string rmAttribute = null;

                string[] stringArray = rmName.Split(new char[] { '_' }, StringSplitOptions.None);
                foreach (string aString in stringArray)
                {
                    rmAttribute += aString[0].ToString().ToUpper() + aString.Substring(1).ToLower();
                }

                DesignByContract.Check.Ensure(rmAttribute != null, "rmAttribute must not be null.");

                return rmAttribute;
            }
            else
            {
                rmName = rmName[0].ToString().ToUpper() + rmName.Substring(1).ToLower();
            }

            return rmName;
        }

        static Lazy<System.Collections.Generic.Dictionary<string, Type>> openEhrV1Types =
            new Lazy<System.Collections.Generic.Dictionary<string, Type>>(delegate()
            {
                System.Collections.Generic.Dictionary<string, Type> innerDictionary = 
                    new System.Collections.Generic.Dictionary<string, Type>();

                foreach (Type type in typeof(RmFactory).Assembly.GetTypes())
                {
                    object[] attributes = type.GetCustomAttributes(typeof(RmTypeAttribute), false);
                    if (attributes.Length > 0)
                    {
                        string key = (attributes[0] as RmTypeAttribute).RmEntity;
                        Check.Assert(!innerDictionary.ContainsKey(key),
                            key + " key already exists.");
                        innerDictionary.Add(key, type);
                    }
                }
                return innerDictionary;
            });


        internal static Type GetOpenEhrV1Type(string rmName)
        {

            Type rmType;
            if (!openEhrV1Types.Value.TryGetValue(rmName, out rmType))
                throw new ArgumentException( "The RM Type " + rmName + " is not supported.");

            return rmType;   

        }

        public static RmType CreateRmType(string rmName)
        {
            Type rmType = GetOpenEhrV1Type(rmName);
            RmType rmObject = Activator.CreateInstance(rmType) as RmType;

            return rmObject;
        }

        public static bool IsRmType(string typeName)
        {
            return openEhrV1Types.Value.ContainsKey(typeName);
        }
    }

    public class OpenEhrFactoryInstance
    {
        public OpenEhrFactoryInstance()
        { }

        #region Create EHR types

        public EhrStatus EhrStatus(string subjectId, string subjectIdScheme, string subjectNamespace)
        {
            return new EhrStatus(new GenericId(subjectId, subjectIdScheme), subjectNamespace);
        }

        #endregion

        #region Create Generic types

        public AuditDetails AuditDetails(string committerName, AuditChangeType changeType)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(committerName),
                "Committer name must not be null or empty");

            DvCodedText codedChangeType = DvCodedText(changeType);

            PartyIdentified committer
                = new PartyIdentified(committerName);

            // %HYYKA%
            //return new Common.Generic.AuditDetails(systemId, changeType, committer);
            // Expects system ID to be set by server
            return new AuditDetails(codedChangeType, committer);
        }

        public PartyIdentified PartyIdentified(string name)
        {
            return new PartyIdentified(name);
        }

        public PartyIdentified PersonIdentified(string name, string externalRefNamespace,
            ObjectId externalRefId)
        {
            return new PartyIdentified(name, new PartyRef(externalRefId, externalRefNamespace, "PERSON"));
        }

        #endregion

        #region Create Identification types

        public PartyRef PartyRef()
        {
            return new PartyRef();
        }

        public ArchetypeId ArchetypeId(string value)
        {
            return new ArchetypeId(value);
        }

        public UidBasedId UidBasedId(string value)
        {
            UidBasedId uid;
            if (value.Contains("::"))
                uid = new ObjectVersionId(value);
            else
                uid = new HierObjectId(value);

            return uid;
        }
        
        public ObjectRef ObjectRef(string rmTypeName)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(rmTypeName), "rmTypeName must not be null or empty.");

            switch (rmTypeName)
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
                    throw new NotSupportedException(rmTypeName +" type is not supported");
            }
            
        }


        #endregion

        #region Create DataTypes

        public DvCodedText DvCodedText(string value, string codeString, string terminologyIdValue)
        {
            return new DvCodedText(value, codeString, terminologyIdValue);
        }

        public DvCodedText DvCodedText(AuditChangeType changeType)
        {
            string value = changeType.ToString();
            string codeString = ((int)changeType).ToString();

            return DvCodedText(value, codeString, "openehr");
        }

        public DvCodedText DvCodedText(VersionLifecycleState lifecycleState)
        {
            string value = lifecycleState.ToString();
            string codeString = ((int)lifecycleState).ToString();

            return DvCodedText(value, codeString, "openehr");
        }
        #endregion

        #region Create Assumed Types

        public AssumedTypes.Iso8601DateTime Iso8601DateTime(string value)
        {
            return new AssumedTypes.Iso8601DateTime(value);
        }
        #endregion

        #region
        public AuthoredResource AuthoredResource(string typeName)
        {
            Check.Require(!string.IsNullOrEmpty(typeName), "type must not be null or empty.");

            AuthoredResource resource = null;
           
            switch (typeName)
            {
                case "ARCHETYPE":
                    resource = new AM.Archetype.Archetype();
                    break;             
                default:
                    throw new NotSupportedException("type not implemented: " + typeName);
            }
            return resource;
        }
        #endregion
    }
}
