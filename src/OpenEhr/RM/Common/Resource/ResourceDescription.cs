using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.Factories;
using OpenEhr.RM.Impl;

namespace OpenEhr.RM.Common.Resource
{
    /// <summary>
    /// Defines the descriptive meta-data of a resource.
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "COMMON", "RESOURCE_DESCRIPTION")]
    public class ResourceDescription : System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>
        /// A default constructor.
        /// </summary>
        public ResourceDescription() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalAuthor">Original author of this resource, with all relevant details, including organisation.</param>
        /// <param name="lifecycleState">Lifecycle state of the resource, typically including states such as: initial, submitted, experimental,
        /// awaiting_approval, approved, superseded, obsolete.</param>
        /// <param name="details">Details of all parts of resource description that are natural language-dependent, keyed by language code.</param>
        /// <param name="resourcePackageUri">URI of package to which this resource belongs.</param>
        /// <param name="otherContributors">Other contributors to the resource, probably listed in “name <email>” form.</param>
        /// <param name="otherDetails">Additional non language-senstive resource meta-data, as a list of name/value pairs.</param>
        /// <param name="parentResource">Reference to owning resource.</param>
        public ResourceDescription(AssumedTypes.Hash<string, string> originalAuthor, string lifecycleState,
            AssumedTypes.Hash<ResourceDescriptionItem, string> details, string resourcePackageUri, 
            AssumedTypes.List<string> otherContributors, AssumedTypes.Hash<string, string> otherDetails, 
            AuthoredResource parentResource):this(originalAuthor, lifecycleState, details)
        {
            this.resourcePackageUri = resourcePackageUri;
            this.otherContributors = otherContributors;
            this.otherDetails = otherDetails;
            this.parentResource = parentResource;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="originalAuthor">Original author of this resource, with all relevant details, including organisation.</param>
        /// <param name="lifecycleState">Lifecycle state of the resource, typically including states such as: initial, submitted, experimental,
        /// awaiting_approval, approved, superseded, obsolete.</param>
        /// <param name="details">Details of all parts of resource description that are natural language-dependent, keyed by language code.</param>
        public ResourceDescription(AssumedTypes.Hash<string, string> originalAuthor, string lifecycleState, 
            AssumedTypes.Hash<ResourceDescriptionItem, string> details)
        {
            Check.Require(originalAuthor!=null && !originalAuthor.IsEmpty(), "originalAuthor must not be null or empty.");
            Check.Require(!string.IsNullOrEmpty(lifecycleState), "lifecycleState must not be null or empty.");
            Check.Require(details!=null && !details.IsEmpty(), "details must not be null or empty.");

            this.originalAuthor = originalAuthor;
            this.lifecycleState = lifecycleState;
            this.details = details;
        }       

        private AssumedTypes.Hash<string, string> originalAuthor;
        /// <summary>
        /// Original author of this resource, with all relevant details, including organisation.
        /// </summary>
        public AssumedTypes.Hash<string, string> OriginalAuthor
        {
            get
            {
                return this.originalAuthor;
            }
        }

        private AssumedTypes.List<string> otherContributors;
        /// <summary>
        /// Other contributors to the resource, probably listed in “name <email>” form.
        /// </summary>
        public AssumedTypes.List<string> OtherContributors
        {
            get
            {
                return this.otherContributors;
            }
        }

        private string lifecycleState;
        /// <summary>
        /// Lifecycle state of the resource, typically including states such as: initial, submitted, 
        /// experimental, awaiting_approval, approved, superseded, obsolete.
        /// </summary>
        public string LifecycleState
        {
            get { return this.lifecycleState; }
        }

        private AssumedTypes.Hash<ResourceDescriptionItem, string> details;
        /// <summary>
        /// Details of all parts of resource description that are natural language-dependent, keyed
        /// by language code.
        /// </summary>
        public AssumedTypes.Hash<ResourceDescriptionItem, string> Details
        {
            get{return this.details;}
        }

        private string resourcePackageUri;
        /// <summary>
        /// URI of package to which this resource belongs.
        /// </summary>
        public string ResourcePackageUri
        {
            get { return this.resourcePackageUri; }
        }

        private AssumedTypes.Hash<string, string> otherDetails;
        /// <summary>
        /// Additional non language-senstive resource meta-data, as a list of name/value pairs.
        /// </summary>
        public  AssumedTypes.Hash<string, string> OtherDetails
        {
            get { return this.otherDetails; }
        }

        private AuthoredResource parentResource;
        /// <summary>
        /// Reference to owning resource.
        /// </summary>
        public AuthoredResource ParentResource
        {
            get{return this.parentResource;}
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
            return new System.Xml.XmlQualifiedName("TRANSLATION_DETAILS", RmXmlSerializer.OpenEhrNamespace);
        }

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            Check.Assert(reader.LocalName == "original_author", "Expected local name is 'original_author', not " + reader.LocalName);
            this.originalAuthor = GetHashData(reader, "original_author");
            reader.MoveToContent();

            string otherContributors = "other_contributors";
            if (reader.LocalName == otherContributors)
            {
                this.otherContributors = new OpenEhr.AssumedTypes.List<string>();
                do
                {
                    this.otherContributors.Add(reader.ReadElementString(otherContributors, RmXmlSerializer.OpenEhrNamespace));
                    reader.MoveToContent();
                } while (reader.LocalName == otherContributors);
            }

            string lifeCycle = "lifecycle_state";
            Check.Assert(reader.LocalName == lifeCycle, "Expected reader.LocalName is " + lifeCycle + ", not " + reader.LocalName);
            this.lifecycleState = reader.ReadElementString(lifeCycle, RmXmlSerializer.OpenEhrNamespace);
            reader.MoveToContent();

            string resourcePackageUriNodeName = "resource_package_uri";
            if (reader.LocalName == resourcePackageUriNodeName)
            {
                this.resourcePackageUri = reader.ReadElementString(resourcePackageUriNodeName, RmXmlSerializer.OpenEhrNamespace);
                reader.MoveToContent();
            }

            string otherDetailsNodeName = "other_details";
            if (reader.LocalName == otherDetailsNodeName)
            {
                this.otherDetails = GetHashData(reader, otherDetailsNodeName);
            }

            string detailsNodeName = "details";
            if (reader.LocalName == detailsNodeName)
            {
                System.Collections.Generic.Dictionary<string, ResourceDescriptionItem> detailsDic =
                    new System.Collections.Generic.Dictionary<string, ResourceDescriptionItem>();
                do
                {
                    ResourceDescriptionItem item = new ResourceDescriptionItem();
                    item.ReadXml(reader);

                    detailsDic.Add(item.Language.CodeString, item);

                } while (reader.LocalName == detailsNodeName);

                if (detailsDic.Count > 0)
                    this.details = new OpenEhr.AssumedTypes.Hash<ResourceDescriptionItem, string>(detailsDic);
            }

            string parentResourceNodeName = "parent_resource";
            if (reader.LocalName == parentResourceNodeName)
            {
                string resourceType = reader.GetAttribute("type", RmXmlSerializer.XsiNamespace);

                this.parentResource = RmFactory.Instance.AuthoredResource(resourceType);
                DesignByContract.Check.Assert(parentResource != null, "must have an object with the type of "+resourceType);

                this.parentResource.ReadXml(reader);
            }

            if (!reader.IsStartElement())
            {
                reader.ReadEndElement();
                reader.MoveToContent();
            }
        }

        private AssumedTypes.Hash<string, string> GetHashData(System.Xml.XmlReader reader, string nodeName)
        {
            if (reader.LocalName == nodeName)
            {
                System.Collections.Generic.Dictionary<string, string> dictionary 
                    = new System.Collections.Generic.Dictionary<string, string>();
                do
                {
                    string id = reader.GetAttribute("id");
                    string value = reader.ReadElementString(nodeName, RmXmlSerializer.OpenEhrNamespace);
                    reader.MoveToContent();

                    dictionary.Add(id, value);
                } while (reader.LocalName == nodeName);

                if (dictionary.Count > 0)
                   return new OpenEhr.AssumedTypes.Hash<string, string>(dictionary);
            }

            return null;

        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            Check.Require(this.OriginalAuthor != null, "OriginalAuthor must not be null.");
            Check.Require(!string.IsNullOrEmpty(this.LifecycleState), "ResourceDescription.LifecycleState must not be null or empty.");
            Check.Require(this.Details!=null && !this.Details.IsEmpty(), "ResourceDescription.Details must not be null or empty.");

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            foreach (string key in this.OriginalAuthor.Keys)
            {
                writer.WriteStartElement(openEhrPrefix, "original_author", RmXmlSerializer.OpenEhrNamespace);
                writer.WriteAttributeString("id", key);
                writer.WriteString(this.OriginalAuthor.Item(key));
                writer.WriteEndElement();
            }

            if (this.OtherContributors != null)
            {
                foreach (string contributor in this.OtherContributors)
                {
                    writer.WriteElementString("other_contributors", RmXmlSerializer.OpenEhrNamespace, contributor);
                }
            }

            writer.WriteElementString("lifecycle_state", RmXmlSerializer.OpenEhrNamespace, this.LifecycleState);

            if(!string.IsNullOrEmpty(this.ResourcePackageUri))
                writer.WriteElementString("resource_package_uri", RmXmlSerializer.OpenEhrNamespace, this.ResourcePackageUri);

            if (this.OtherDetails != null)
            {
                foreach (string id in this.OtherDetails.Keys)
                {
                    writer.WriteStartElement(openEhrPrefix, "other_details", RmXmlSerializer.OpenEhrNamespace);
                    writer.WriteAttributeString("id", id);
                    writer.WriteString(this.OtherDetails.Item(id).ToString());
                    writer.WriteEndElement();
                }
            }

            foreach (string id in this.Details.Keys)
            {
                writer.WriteStartElement(openEhrPrefix, "details", RmXmlSerializer.OpenEhrNamespace);
                this.Details.Item(id).WriteXml(writer);                
                writer.WriteEndElement();
            }

            if (this.ParentResource != null)
            {
                writer.WriteStartElement("parent_resource", RmXmlSerializer.OpenEhrNamespace);
                string parentResourcetype = ((IRmType)this.ParentResource).GetRmTypeName();
                writer.WriteAttributeString(xsiPrefix, "type", RmXmlSerializer.XsiNamespace, parentResourcetype);
                this.ParentResource.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

    }
}
