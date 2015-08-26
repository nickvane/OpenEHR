using System;
using OpenEhr.AssumedTypes;
using OpenEhr.AssumedTypes.Impl;
using OpenEhr.DesignByContract;
using OpenEhr.Paths;

namespace OpenEhr.RM.Common.Archetyped.Impl
{
    [Serializable]
    public abstract class AttributeDictionaryPathable : Pathable
    {
        private System.Collections.Generic.Dictionary<string, object> itemAtPathDictionary;

        protected AttributeDictionaryPathable()
        {
            this.attributesDictionary = new System.Collections.Generic.Dictionary<string, object>();
        }

        protected readonly System.Collections.Generic.Dictionary<string, object> attributesDictionary;

        public override bool PathExists(string path)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(path), "Path must not be null or empty.");

            object itemInDictionary = null;
            if (this.itemAtPathDictionary != null &&
                this.itemAtPathDictionary.ContainsKey(path))
                itemInDictionary = itemAtPathDictionary[path];
            else
                itemInDictionary = ItemAtPathUtil(path);
            if (itemInDictionary == null)
                return false;
            return true;
        }

        public override bool PathUnique(string path)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(path), "Path must not be null or empty.");

            object itemInDictionary = null;
            if (this.itemAtPathDictionary != null && this.itemAtPathDictionary.ContainsKey(path))
                itemInDictionary = itemAtPathDictionary[path];
            else
                itemInDictionary = ItemAtPathUtil(path);

            if (itemInDictionary == null)
                throw new PathNotExistException(path);

            AssumedTypes.IList list = itemInDictionary as AssumedTypes.IList;
            if (list != null)
            {
                if (list.Count > 1)
                    return false;
                else
                    return true;
            }
            else
                return true;
        }

        public override object ItemAtPath(string path)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(path), "Path must not be null or empty.");

            object itemInDictionary = null;
            if (this.itemAtPathDictionary != null && this.itemAtPathDictionary.ContainsKey(path))
                itemInDictionary = itemAtPathDictionary[path];
            else
                itemInDictionary = ItemAtPathUtil(path);

            if (itemInDictionary == null)
                throw new PathNotExistException(path);

            AssumedTypes.IList list = itemInDictionary as AssumedTypes.IList;
            if (list != null)
            {
                if (list.Count > 1)
                    throw new PathNotUniqueException(path);
                else
                    return list[0];
            }
            else
                return itemInDictionary;
        }

        public override List<object> ItemsAtPath(string path)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(path), "Path must not be null or empty.");

            object itemInDictionary = null;
            if (this.itemAtPathDictionary != null && this.itemAtPathDictionary.ContainsKey(path))
                itemInDictionary = itemAtPathDictionary[path];
            else
                itemInDictionary = ItemAtPathUtil(path);

            if (itemInDictionary == null)
                throw new PathNotExistException(path);

            //// CM: 07/07/09 the path must be unique path. Need to create a list and add the object to the list.
            //// Even though the spec shows the the precondition should be that the path must not be unique path, but this
            //// may be too restrictive. So for the moment, when the path is unique, then need to create a list instance
            //// and add the single object to the list rather than throw an exception.
            
            List<object> items = itemInDictionary as List<object>;
            if (items == null)
            {
                Check.Assert(!(itemInDictionary is IList), "itemInDiction must not be of type IList");

                items = new OpenEhr.AssumedTypes.List<object>();
                items.Add(itemInDictionary);
            }
            Check.Ensure(items != null, "items must not be null");

            return items;
        }

        protected void ClearItemAtPathCache()
        {
            itemAtPathDictionary = null;
        }

        private object ItemAtPathUtil(string path)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(path), "Path must not be null or empty.");

            object itemAtPath;

            if (path == "/")
            {
                Pathable pathableObject = this;

                while (pathableObject.Parent != null)
                    pathableObject = pathableObject.Parent;

                itemAtPath = pathableObject;
            }
            else
            {
                Path pathProcessor = new Path(path);

                //// The itemAtPath can be a single item which is a pathable, a locatable 
                //// or a non-pathable item.
                //// The itemAtPath can be an IList instance when the path points to 
                //// a locatable collection with multiple items or a single item; 
                //// or it can be an IList with one or more
                //// locatables when the path looks like //*[archetype ID] (i.e. the path is not
                //// named and is terminal)
                //// The itemAtPath can be null when the path doesn't exist.
                //object itemAtPath = this.ItemAtPathPathProcessorUtil(pathProcessor);
                itemAtPath = pathProcessor.ItemAtPath(this);

                if (this.itemAtPathDictionary == null)
                    itemAtPathDictionary = new System.Collections.Generic.Dictionary<string, object>();
                // this function is called only when the itemAtPathDictionary doesn't have the key - path,
                // therefore, don't need to check whether the dictionary has the key or not before
                // adding the key-value pair.      
                Check.Assert(!this.itemAtPathDictionary.ContainsKey(path), "itemAtPathDiction must not contain path");
                if (itemAtPath != null)
                    this.itemAtPathDictionary.Add(path, itemAtPath);
            }
            return itemAtPath;
        }

        /// <summary>
        /// In this method, it is assumed that the locatable must be the child of this class.
        /// </summary>
        /// <param name="locatable"></param>
        /// <returns></returns>
        private string PathOfItemSimple(Pathable item)
        {
            Check.Require(item != null);

            string path = null;
            string pathSeperator = "/";

            System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<string, object>> keyValueList
                = this.attributesDictionary as System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<string, object>>;
            foreach (System.Collections.Generic.KeyValuePair<string, object> listItem in keyValueList)
            {
                ILocatableList locatableList = listItem.Value as ILocatableList;
                Pathable locatableItem = listItem.Value as Pathable;

                if (locatableList != null)
                {
                    Locatable itemLocatable = item as Locatable;
                    if (locatableList.Contains(itemLocatable))
                    {
                        DesignByContract.Check.Assert(itemLocatable.Name != null);

                        return path = pathSeperator + listItem.Key + "[" + itemLocatable.ArchetypeNodeId +
                                      " and name/value='" + itemLocatable.Name.Value + "']";
                    }
                }
                else if (listItem.Value == item)
                {
                    // must be locatable
                    if (item is Locatable)
                        return path = pathSeperator + listItem.Key + "[" + ((Locatable)item).ArchetypeNodeId + "]";
                    else
                        return path = pathSeperator + listItem.Key;
                }
            }

            // raise exception?
            throw new InvalidOperationException("item not found in parent's items");
        }

        /// <summary>
        /// The path to an item relative to the root of this archetyped structure.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override string PathOfItem(Pathable item)
        {
            //throw new Exception("The method or operation is not implemented.");
            DesignByContract.Check.Require(item != null);

            //Locatable item = locatable as Locatable;
            DesignByContract.Check.Assert(item != null);

            AttributeDictionaryPathable itemParent = item.Parent as AttributeDictionaryPathable;
            Check.Assert(item.Parent == null || itemParent != null, "itemParent must not be null");

            // when the item is the top level (e.g. composition) or doesn't have a parent, 
            // the path is "/"
            if (itemParent == null)
                return "/";

            string path = "";

            while (!this.Equals(itemParent))
            {
                path = itemParent.PathOfItemSimple(item) + path;
                item = itemParent;
                if (item.Parent == null)
                    throw new ApplicationException("root of item not found, item is not a decendent of this object");

                itemParent = item.Parent as AttributeDictionaryPathable;
                Check.Assert(item.Parent == null || itemParent != null, "itemParent must not be null");
            }

            path = itemParent.PathOfItemSimple(item) + path;

            return path;
        }

        protected abstract void SetAttributeDictionary();

        protected override void SetAttributeValue(string attributeName, object value)
        {
            Check.Require(attributesDictionary.ContainsKey(attributeName), "RM type does not have attribute " + attributeName);

            base.SetAttributeValue(attributeName, value);
            this.attributesDictionary[attributeName] = value;
        }

        protected internal override object GetAttributeValue(string attributeName)
        {
            Check.Require(attributesDictionary.ContainsKey(attributeName), "RM type does not have attribute " + attributeName);

            return attributesDictionary[attributeName];
        }

        protected internal override System.Collections.IEnumerable GetAllAttributeValues()
        {
            return attributesDictionary.Values;
        }
    }
}