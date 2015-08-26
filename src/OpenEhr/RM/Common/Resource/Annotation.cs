using System;
using System.Collections.Generic;
using OpenEhr.DesignByContract;

namespace OpenEhr.RM.Common.Resource
{
    [Serializable]
    public class Annotation
    {
        public Annotation(Dictionary<string, string> itemsDictionary, string path)
        {
            Check.Require(!string.IsNullOrEmpty(path), "path should not be null or empty");
            Check.Require(itemsDictionary != null, "itemsDictionary should not be null");

            this.path = path;
            this.items = new OpenEhr.AssumedTypes.Hash<string, string>(itemsDictionary);
        }

        private string path;
        public string Path
        {
            get { return this.path; }
            set 
            {
                Check.Require(!string.IsNullOrEmpty(value), "path should not be null or empty");
                this.path = value; 
            }
        }

        private readonly AssumedTypes.Hash<string, string> items;
        public AssumedTypes.Hash<string, string> Items
        {
            get { return this.items; }
        }
    }
}
