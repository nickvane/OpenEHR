using System;
using System.Xml;
using OpenEhr.Attributes;
using OpenEhr.Serialisation;
using OpenEhr.Factories;
using OpenEhr.RM.DataStructures.ItemStructure.Representation;
using OpenEhr.RM.DataTypes.Text;
using OpenEhr.RM.Common.Archetyped.Impl;
using OpenEhr.AssumedTypes.Impl;

namespace OpenEhr.RM.DataStructures.ItemStructure
{
    [System.Xml.Serialization.XmlSchemaProvider("GetXmlSchema")]
    [Serializable]
    [RmType("openEHR", "DATA_STRUCTURES", "ITEM_TABLE")]
    public class ItemTable : ItemStructure, System.Xml.Serialization.IXmlSerializable
    {
        public ItemTable() 
        { }

        public ItemTable(DvText name, string archetypeNodeId, Support.Identification.UidBasedId uid,
            Link[] links, Archetyped archetypeDetails, FeederAudit feederAudit, Cluster[] rows)
            : base(name, archetypeNodeId, uid, links, archetypeDetails, feederAudit)
        {
            if (rows != null)
            {
                this.rows = RmFactory.LocatableList<Cluster>(this, rows);
            }

            SetAttributeDictionary();
            CheckInvariants();
        }


        private AssumedTypes.List<Representation.Cluster> rows;
        /// <summary>
        /// Physical representation of the table as a list of CLUSTER, each containing the data of one row of the table
        /// </summary>
        [RmAttribute("rows")]
        public AssumedTypes.List<Representation.Cluster> Rows
        {
            get
            {
                if(this.rows == null)
                    this.rows = base.attributesDictionary["rows"] as LocatableList<Cluster>;
                return this.rows;

            }
        }

        #region class functions

        public override Item AsHierarchy()
        {
            if (this.Rows == null || this.Rows.Count == 0)
                return null;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return the number of rows. Return 0 when the Rows is null.
        /// </summary>
        /// <returns></returns>
        public int RowCount()
        {
            if (this.Rows == null)
                return 0;

            return this.Rows.Count;
        }

        /// <summary>
        /// Return the number of columns
        /// </summary>
        /// <returns></returns>
        public int ColumnCount()
        {
            if (this.RowCount() == 0)
                return 0;

            return this.Rows[0].Items.Count;
        }

        private AssumedTypes.List<DataTypes.Text.DvText> rowNames;
        /// <summary>
        /// Return the row names, i.e. the list of each cluster name. Returns an empty list of DvText when this.Rows is empty. 
        /// </summary>
        /// <returns></returns>
        public AssumedTypes.List<DataTypes.Text.DvText> RowNames()
        {          
            if (rowNames == null)
            {
                rowNames = new OpenEhr.AssumedTypes.List<OpenEhr.RM.DataTypes.Text.DvText>();
                if (this.RowCount() > 0)
                {
                    foreach (Cluster cluster in this.Rows)
                    {
                        rowNames.Add(cluster.Name);
                    }
                }
            }

            DesignByContract.Check.Ensure(rowNames != null, "rowNames must not be null.");
            return rowNames;
        }

        private AssumedTypes.List<DataTypes.Text.DvText> columnNames;
        /// <summary>
        /// Return the column names. Returns an empty list of DvText if the ColumnCount is 0
        /// </summary>
        /// <returns></returns>
        public AssumedTypes.List<DataTypes.Text.DvText> ColumnNames()
        {
            DesignByContract.Check.Require(this.ColumnCount() == 0 ^ this.RowCount()>0, 
                "If ColumnCount ==0, RowCount must be zero. If ColumnCount >0, RowCount must be > 0");

            if (columnNames == null)
            {
                columnNames = new AssumedTypes.List<OpenEhr.RM.DataTypes.Text.DvText>();
                if (this.ColumnCount() > 0)
                {
                    Cluster aRow = this.Rows[0];
                    DesignByContract.Check.Assert(aRow != null, "aRow must not be null.");

                    for (int i = 1; i < aRow.Items.Count; i++)
                    {
                        columnNames.Add(aRow.Items[i].Name);
                    }
                }
            }

            DesignByContract.Check.Ensure(columnNames != null, "columnNames must not be null.");
            return columnNames;
        }

        /// <summary>
        /// Return the i-th row
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Cluster IthRow(int i)
        {
            // put this precondition is because the specifications indicates that i<row_count
            DesignByContract.Check.Require(this.Rows != null && this.Rows.Count > 0, "Rows must not be null or empty.");
            DesignByContract.Check.Require(i >= 0 && i < this.RowCount(),
                "i must be greater than or equal to zero, but less than the total row counts. ");

            // TODO: Checking whether the i starts from 0 or 1.
            return this.Rows[i];
        }

        /// <summary>
        /// True if there is a row whose first column has the name 'aKey'
        /// </summary>
        /// <param name="aKey"></param>
        /// <returns></returns>
        public bool HasRowWithName(string aKey)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(aKey), "aKey must not be null or empty.");

            if (this.RowCount() == 0)
                return false;

            AssumedTypes.List<DataTypes.Text.DvText> rowNames = this.RowNames();
            
            foreach (DataTypes.Text.DvText name in rowNames)
            {
                if (name.Value == aKey)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// True if there is a column with name aKey
        /// </summary>
        /// <param name="aKey"></param>
        /// <returns></returns>
        public bool HasColumnWithName(string aKey)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(aKey), "aKey must not be null or empty.");
            
            if (this.ColumnCount() == 0)
                return false;

            AssumedTypes.List<DataTypes.Text.DvText> columnNames = this.ColumnNames();

            foreach (DataTypes.Text.DvText name in columnNames)
            {
                if (name.Value == aKey)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Return the row whose first column has the name aKey
        /// </summary>
        /// <param name="aKey"></param>
        /// <returns></returns>
        public Cluster NamedRow(string aKey)
        {
            DesignByContract.Check.Require(this.HasRowWithName(aKey), "Must have a row with the name aKey");

            foreach (Cluster row in this.Rows)
            {
                if (row.Items[0].Name.Value == aKey)
                    return row;
            }

            return null;
        }


        /// <summary>
        /// True if there is a row whose first n columns have the names in keys
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        
        public bool HasRowWithKey(AssumedTypes.Set<string> keys)
        {
            throw new NotImplementedException("HasRowWithKey function has not been implemented.");
        }

        /// <summary>
        /// Return the row whose first n columns have names equal to the values in 'Keys'
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public Cluster RowWithKey(AssumedTypes.Set<string> keys)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return the element at the column i, row j
        /// </summary>
        /// <param name="i">column index</param>
        /// <param name="j">row index</param>
        /// <returns></returns>
        public Element ElementAtCellIJ(int i, int j)
        {
            DesignByContract.Check.Require(this.RowCount() >0, "RowCount must be greater than zero.");
            DesignByContract.Check.Require(this.ColumnCount() > 1, "ColumnCount must be greater than 1.");
            DesignByContract.Check.Require(i >=1 && i<=this.ColumnCount(), "i must be >=1 and <= ColumnCount.");
            DesignByContract.Check.Require(j>=1 && j<= RowCount(), "j must be >=1 and <= RowCount.");

            return (this.Rows[j].Items[i]) as Element;            
        }

        /// <summary>
        /// Return the element at the row whose first column has the name rowKey and column has the name columnKey
        /// </summary>
        /// <param name="rowKey"></param>
        /// <param name="columnKey"></param>
        /// <returns></returns>
        public Element ElementAtNamedCell(string rowKey, string columnKey)
        {           
            DesignByContract.Check.Require(this.HasRowWithName(rowKey), "Must have row with name of rowKey.");
            DesignByContract.Check.Require(this.HasColumnWithName(columnKey), "Must have column with name columnKey");

            foreach (Cluster row in this.Rows)
            {
                if (row.Name.Value == rowKey)
                {
                    foreach (Element element in row.Items)
                    {
                        if (element.Name.Value == columnKey)
                            return element;
                    }
                }
            }

            throw new ApplicationException("There must be an element at the named cell with row of " + rowKey + " and column of " + columnKey);
        }

        #endregion

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

        public static XmlQualifiedName GetXmlSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            RmXmlSerializer.LoadCompositionSchema(xs);
            return new XmlQualifiedName("ITEM_TABLE", RmXmlSerializer.OpenEhrNamespace);
        }

        protected override void ReadXmlBase(XmlReader reader)
        {
            base.ReadXmlBase(reader);

            if (reader.LocalName == "rows")
            {
                LocatableList<Representation.Cluster> rows =
                    new LocatableList<OpenEhr.RM.DataStructures.ItemStructure.Representation.Cluster>();

                while (reader.LocalName == "rows")
                {
                    OpenEhr.RM.DataStructures.ItemStructure.Representation.Cluster aCluster =
                       new OpenEhr.RM.DataStructures.ItemStructure.Representation.Cluster();
                    aCluster.ReadXml(reader);
                    aCluster.Parent = this;
                    rows.Add(aCluster);
                }
                this.rows = rows;
            }

        }

        protected override void WriteXmlBase(XmlWriter writer)
        {
            base.WriteXmlBase(writer);

            string xsiPrefix = RmXmlSerializer.UseXsiPrefix(writer);
            string openEhrPrefix = RmXmlSerializer.UseOpenEhrPrefix(writer);

            if (this.Rows != null && this.Rows.Count > 0)
            {
                foreach (OpenEhr.RM.DataStructures.ItemStructure.Representation.Cluster aCluster 
                    in this.rows)
                {
                    writer.WriteStartElement(openEhrPrefix, "rows", RmXmlSerializer.OpenEhrNamespace);
                    aCluster.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }

        protected override void SetAttributeDictionary()
        {
            base.SetAttributeDictionary();
            base.attributesDictionary["rows"]= this.rows;
        }
    }
}
