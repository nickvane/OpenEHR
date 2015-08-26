using System;
using System.Xml;
using OpenEhr.DesignByContract;
using OpenEhr.Serialisation;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Support.Identification;
using OpenEhr.RM.DataStructures.ItemStructure;
using OpenEhr.AM.Archetype.ConstraintModel;

namespace OpenEhr.RM.Common.Archetyped.Impl
{
    [Serializable]
    public abstract class Locatable : AttributeDictionaryPathable, ILocatable
    {
        protected Locatable()
            : base()
        { }

        protected Locatable(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
            AssumedTypes.List<Link> links, Archetyped archetypeDetails, FeederAudit feederAudit)
            : this()
        {
            Check.Require(name != null, "name must not be null");
            Check.Require(!string.IsNullOrEmpty(archetypeNodeId), "archetype_node_id must not be null or empty");

            this.name = name;
            this.archetypeNodeId = archetypeNodeId;
            this.uid = uid;
            this.links = links;
            this.archetypeDetails = archetypeDetails;
            this.feederAudit = feederAudit;
        }

        protected Locatable(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
           Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit)
            : this()
        {
            Check.Require(name != null, "name must not be null");
            Check.Require(!string.IsNullOrEmpty(archetypeNodeId), "archetype_node_id must not be null or empty");

            this.name = name;
            this.archetypeNodeId = archetypeNodeId;
            this.uid = uid;
            if (links != null)
                this.links = new OpenEhr.AssumedTypes.List<Link>(links);
            this.archetypeDetails = archetypeDetails;
            this.feederAudit = feederAudit;
        }

        protected void SetBaseData(string archetypeNodeId, DvText name)
        {
            Check.Invariant(attributesDictionary != null, "Attributes diction must not be null");
            
            Check.Require(!string.IsNullOrEmpty(archetypeNodeId), "archetype node id must not be null or empty");
            Check.Require(name != null, "name must not be null");
            Check.Require(!string.IsNullOrEmpty(name.Value), "name value must not be null or empty");

            Check.Assert(this.archetypeNodeId == null, "archetype node id attribute must be null");
            //Check.Assert(base.attributesDictionary["name"] == null, "archetype node id attributes item must be null");
            this.archetypeNodeId = archetypeNodeId;

            Check.Assert(this.name == null, "name attribute must be null");
            //Check.Assert(attributesDictionary["name"] == null, "name attributes item must be null");

            this.name = name;

            SetAttributeDictionary();
            CheckInvariants();
        }


        #region ILocatable Members

        private string archetypeNodeId;

        public string ArchetypeNodeId
        {
            get
            {
                if (this.archetypeNodeId == null)
                    this.archetypeNodeId = this.attributesDictionary["archetype_node_id"] as string;
                else
                    Check.Ensure(this.attributesDictionary["archetype_node_id"] as string == this.archetypeNodeId, "archetype_node_id not internally syncronised");

                Check.Ensure(this.archetypeNodeId != null, "archetype_node_id must not be null");

                return this.archetypeNodeId;
            }
            set
            {
                Check.Require(!string.IsNullOrEmpty(value), "archetype_node_id must not be null or empty");

                this.attributesDictionary["archetype_node_id"] = value;
                this.archetypeNodeId = value;

                Check.Ensure(this.ArchetypeNodeId == this.attributesDictionary["archetype_node_id"] as string, "archetype_node_id not internally syncronised");
            }
        }

        public bool IsArchetypeRoot
        {
            get { return ArchetypeId.IsValid(ArchetypeNodeId); }
        }

        public string Concept
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        private DvText name;

        public DvText Name
        {
            get
            {
                if (this.name == null)
                    this.name = this.attributesDictionary["name"] as DvText;
                else
                    Check.Ensure(this.name.Equals(this.attributesDictionary["name"]), "name not internally syncronised");

                Check.Ensure(this.name != null, "name must not be null or empty");

                return this.name;
            }
            set {
                Check.Require(value != null, "name must not be null");

                if (this.Constraint != null)
                {
                    CComplexObject objectConstraint = this.Constraint as CComplexObject;
                    CAttribute nameConstraint = objectConstraint.GetAttribute("name");
                    if (nameConstraint != null)
                        nameConstraint.ValidValue(value);
                }

                this.attributesDictionary["name"] = value;
                this.name = value;

                Check.Ensure(this.Name.Equals(this.attributesDictionary["name"]), "name not internally syncronised");
            }
        }

        private AssumedTypes.List<Link> links;

        public AssumedTypes.List<Link> Links
        {
            get
            {
                if (this.links == null)
                    this.links = this.attributesDictionary["links"] as AssumedTypes.List<Link>;

                return this.links;
            }
        }

        private Archetyped archetypeDetails;

        public Archetyped ArchetypeDetails
        {
            get
            {
                if (this.archetypeDetails == null)
                    this.archetypeDetails = this.attributesDictionary["archetype_details"] as Archetyped;
                return this.archetypeDetails;
            }
        }

        private UidBasedId uid;

        public UidBasedId Uid
        {
            get
            {
                if (this.uid == null)
                    this.uid = this.attributesDictionary["uid"] as Support.Identification.UidBasedId;
                return this.uid;
            }
            set
            {
                Check.Require(value != null, "value must not be null");

                this.attributesDictionary["uid"] = value;
                this.uid = value;

                ClearItemAtPathCache();

                Check.Ensure(this.attributesDictionary["uid"].ToString() == this.uid.Value, "uid in attributeDictyionary must equal uid attribute");
                Check.Ensure(this.ItemAtPath("/uid/value").ToString() == this.Uid.Value, "ItemAtPath(/uid/value) must equal Uid.Value property");
            }
        }

        private FeederAudit feederAudit;

        public FeederAudit FeederAudit
        {
            get
            {
                if (this.feederAudit == null)
                    this.feederAudit = this.attributesDictionary["feeder_audit"] as FeederAudit;
                return this.feederAudit;
            }
        }

        #endregion

        protected virtual void CheckInvariants()
        {
            Check.Invariant(this.Name != null, "name must not be null.");
            // HKF: 7 Aug 2009 - need to allow name value to be empty for EhrGateDataObjects transformation prior to OperationalTemplate augmentation
            Check.Invariant(this.Name.Value != null, "name value must not be null");

            // HKF: 7 Aug 2009 - need to allow archetypeNodeId to be empty for EhrGateDataObjects transformation prior to OperationalTemplate augmentation
            Check.Invariant(this.ArchetypeNodeId != null, "archetype node id must not be null");

            DesignByContract.Check.Invariant(this.Links == null || this.Links.Count > 0,
                "Links_valid: links /= Void implies not links.empty");
        }

        protected virtual void CheckInvariantsDefault()
        {
            DesignByContract.Check.Invariant(this.Name != null, "name must not be null for " + this.GetType().Name + "(" + this.ArchetypeNodeId + ").");
            DesignByContract.Check.Invariant(this.Name.Value != null,
                "name value must not be null");
            
            DesignByContract.Check.Invariant(!string.IsNullOrEmpty(this.ArchetypeNodeId), 
                "archetypeNodeId must not be null or empty.");
            DesignByContract.Check.Invariant(this.Links == null || this.Links.Count > 0,
                "Links_valid: links /= Void implies not links.empty");

            Check.Invariant(base.attributesDictionary["archetype_node_id"] == this.ArchetypeNodeId,
                "archetype_node_id in attributes item must be same as ArchetypeNodeId property");
            Check.Invariant(base.attributesDictionary["name"] == this.Name, 
                "name in attributes item must be same as name property");
        }

        private System.Collections.Hashtable pathItemPair;

        internal System.Collections.Hashtable PathItemPair
        {
            get
            {
                if (pathItemPair == null)
                    pathItemPair = new System.Collections.Hashtable();

                return this.pathItemPair;
            }

        }

        internal protected virtual void ReadXml(XmlReader reader)
        {
            if (reader.NodeType == System.Xml.XmlNodeType.None)
                reader.MoveToContent();

            this.archetypeNodeId = reader.GetAttribute("archetype_node_id");
            Check.Require(this.archetypeNodeId != null, "archetype_node_id attribute must exist and not empty");

            reader.ReadStartElement();
            reader.MoveToContent();

            this.ReadXmlBase(reader);

            Check.Assert(reader.NodeType == XmlNodeType.EndElement, "Expected endElement");
            reader.ReadEndElement();
            reader.MoveToContent();

            this.SetAttributeDictionary();
            
            this.CheckInvariants();
        }

        protected virtual void ReadXmlBase(System.Xml.XmlReader reader)
        {
            Check.Assert(reader.LocalName == "name", "Expected LocalName is 'name' rather than " + reader.LocalName);

            // %HYYKA%
            // CM: 12/11/09 need to take into account prefix
            //string nameType = reader.GetAttribute("type", XmlSerializer.XsiNamespace);
            string nameType = RmXmlSerializer.ReadXsiType(reader);
            if (nameType != null && nameType == "DV_CODED_TEXT")
                this.name = new DvCodedText();
            else
                this.name = new DvText();
            this.name.ReadXml(reader);

            if (reader.LocalName == "uid")
            {
                string uidType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);
                int i = uidType.IndexOf(":");
                if (i > 0)
                    uidType = uidType.Substring(i + 1);
                if (uidType == "OBJECT_VERSION_ID")
                    this.uid = new ObjectVersionId();
                else if (uidType == "HIER_OBJECT_ID")
                    this.uid = new HierObjectId();

                this.uid.ReadXml(reader);
            }
            if (reader.LocalName == "links")
            {
                this.links = new OpenEhr.AssumedTypes.List<Link>();
                do
                {
                    Link aLink = new Link();
                    aLink.ReadXml(reader);
                    this.links.Add(aLink);
                } while (reader.LocalName == "links" && reader.NodeType == System.Xml.XmlNodeType.Element);
            }

            if (reader.LocalName == "archetype_details")
            {
                this.archetypeDetails = new Archetyped();
                this.archetypeDetails.ReadXml(reader);
            }

            if (reader.LocalName == "feeder_audit")
            {
                this.feederAudit = new FeederAudit();
                this.feederAudit.ReadXml(reader);
            }

        }

        protected virtual void WriteXmlBase(System.Xml.XmlWriter writer)
        {            
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);

            // %HYYKA%
            // HKF: Not valid against schema when attribute has prefix and namespace
            //writer.WriteAttributeString(openEhrPrefix, "archetype_node_id", 
            //    XmlSerializer.OpenEhrNamespace, this.ArchetypeNodeId);
            writer.WriteAttributeString("archetype_node_id", this.ArchetypeNodeId);

            writer.WriteStartElement(openEhrPrefix, "name", RmXmlSerializer.OpenEhrNamespace);
            string nameType = openEhrPrefix;
            if (!string.IsNullOrEmpty(nameType))
                nameType += ":";
            if (this.Name.GetType() == typeof(DvCodedText))
                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace,
                    nameType + "DV_CODED_TEXT");
            this.Name.WriteXml(writer);
            writer.WriteEndElement();

            if (this.Uid != null)
            {
                writer.WriteStartElement(openEhrPrefix, "uid", RmXmlSerializer.OpenEhrNamespace);
                string uidType = openEhrPrefix;
                if (!string.IsNullOrEmpty(uidType))
                    uidType += ":";
                
                if (this.Uid.GetType() == typeof(ObjectVersionId))
                    writer.WriteAttributeString(xsiPrefix, "type", 
                        RmXmlSerializer.XsiNamespace, uidType + "OBJECT_VERSION_ID");
                else
                    writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, uidType + "HIER_OBJECT_ID");

                this.Uid.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.Links != null && this.Links.Count > 0)
            {
                foreach (Link aLink in this.Links)
                {
                    writer.WriteStartElement(openEhrPrefix, "links", RmXmlSerializer.OpenEhrNamespace);
                    aLink.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }

            if (this.ArchetypeDetails != null)
            {
                writer.WriteStartElement(openEhrPrefix, "archetype_details", RmXmlSerializer.OpenEhrNamespace);
                this.ArchetypeDetails.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (this.FeederAudit != null)
            {
                writer.WriteStartElement(openEhrPrefix, "feeder_audit", RmXmlSerializer.OpenEhrNamespace);
                this.FeederAudit.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            this.CheckInvariants();
            this.WriteXmlBase(writer);
        }

        internal static Locatable GetLocatableObjectByType(string type)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(type), "type must not be null or empty");

            int i = type.IndexOf(":");
            if (i > 0)
                type = type.Substring(i + 1);
            switch (type)
            {
                case "COMPOSITION":
                    return new Composition.Composition();
                case "SECTION":
                    return new OpenEhr.RM.Composition.Content.Navigation.Section();
                case "ADMIN_ENTRY":
                    return new OpenEhr.RM.Composition.Content.Entry.AdminEntry();
                case "OBSERVATION":
                    return new OpenEhr.RM.Composition.Content.Entry.Observation();
                case "EVALUATION":
                    return new OpenEhr.RM.Composition.Content.Entry.Evaluation();
                case "INSTRUCTION":
                    return new OpenEhr.RM.Composition.Content.Entry.Instruction();
                case "ACTION":
                    return new OpenEhr.RM.Composition.Content.Entry.Action();
                case "ITEM_TREE":
                    return new ItemTree();
                case "ITEM_LIST":
                    return new ItemList();
                case "ITEM_SINGLE":
                    return new ItemSingle();
                case "ITEM_TABLE":
                    return new ItemTable();
                case "POINT_EVENT":
                    return new DataStructures.History.PointEvent<ItemStructure>();
                case "ELEMENT":
                    return new OpenEhr.RM.DataStructures.ItemStructure.Representation.Element();
                case "CLUSTER":
                    return new OpenEhr.RM.DataStructures.ItemStructure.Representation.Cluster();  
                case "INTERVAL_EVENT":
                    return new DataStructures.History.IntervalEvent<ItemStructure>();
                default:
                    throw new InvalidOperationException("type is not supported: " + type);
            }
        }


        protected override void SetAttributeDictionary() 
        {
            DesignByContract.Check.Require(this.attributesDictionary != null, "attributeDictionary must not be null.");

            this.attributesDictionary["name"] = this.name; 
            this.attributesDictionary["archetype_node_id"] = this.archetypeNodeId;
            this.attributesDictionary["uid"] = this.uid;
            this.attributesDictionary["links"] = this.links;
            this.attributesDictionary["archetype_details"]= this.archetypeDetails;

            this.attributesDictionary["feeder_audit"]= this.feederAudit;

        }
    }    
}
