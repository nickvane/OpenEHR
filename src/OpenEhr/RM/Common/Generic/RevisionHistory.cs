using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;
using OpenEhr.Validation;
using OpenEhr.RM.Impl;
using OpenEhr.Serialisation;

namespace OpenEhr.RM.Common.Generic
{
    /// <summary>
    /// Defines the notion of a revision history of audit items, each associated with the
    /// version for which that audit was committed. The list is in most-recent-first order.
    /// </summary>
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "COMMON", "REVISION_HISTORY")]
    public class RevisionHistory : RmType, System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public RevisionHistory() { }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="items">The items in this history in most-recent-last order.</param>
        public RevisionHistory(AssumedTypes.List<RevisionHistoryItem> items)
        {
            DesignByContract.Check.Require(items!= null, "items must not be null.");
            // %HYYKA%
            // TODO: need to sort the order according to the spec
            //this.items = SortItemsInMostRecentLastOrder(items);
            this.items = items;
        }

        private AssumedTypes.List<RevisionHistoryItem> items;
        /// <summary>
        /// The items in this history in most-recent-last order.
        /// </summary>
        public AssumedTypes.List<RevisionHistoryItem> Items
        {
            get { return this.items; }
        }

        /// <summary>
        /// The version id of the most recent item, as a String.
        /// </summary>
        /// <returns></returns>
        public string MostRecentVersion()
        {
            DesignByContract.Check.Require(this.Items!= null, "RevisionHistory.Items must not be null.");
            DesignByContract.Check.Require(this.Items.Count >0, "RevisionHistory.Items must not be empty.");
           
            return this.Items.Last.VersionId.Value;
        }

        /// <summary>
        /// The commit date/time of the most recent item, as a String.
        /// </summary>
        /// <returns></returns>
        public string MostRecentVersionTimeCommitted()
        {
            DesignByContract.Check.Require(this.Items != null, "RevisionHistory.Items must not be null.");
            DesignByContract.Check.Require(this.Items.Count > 0, "RevisionHistory.Items must not be empty.");

            return this.Items.Last.Audits.First.TimeCommitted.Value;
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
            return new System.Xml.XmlQualifiedName("REVISION_HISTORY", RmXmlSerializer.OpenEhrNamespace);
        }

        internal void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            if (reader.LocalName != "items")
                throw new ValidationException("Expected local name is items, but it is " + reader.LocalName);
           
            this.items = new OpenEhr.AssumedTypes.List<RevisionHistoryItem>();
           
            while (reader.LocalName == "items")
            {
                RevisionHistoryItem item = new RevisionHistoryItem();
                item.ReadXml(reader);
                this.items.Add(item);
            }

            // TODO: sort the items
        }

        internal void WriteXml(System.Xml.XmlWriter writer)
        {
            Check.Require(this.Items != null, "RevisionHistory.Items must not be null.");
         
            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            foreach (RevisionHistoryItem item in this.Items)
            {
                writer.WriteStartElement(openEhrPrefix, "items", RmXmlSerializer.OpenEhrNamespace);
                item.WriteXml(writer);
                writer.WriteEndElement();
            }           
        }
    }
}
