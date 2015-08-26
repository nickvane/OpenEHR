using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;

namespace OpenEhr.Futures.OperationalTemplate
{
    public class TViewConstraint
    {
        private string path;
        /// <summary>Path of this constraint</summary>
        public string Path
        {
            get { return this.path; }
            set
            {
                Check.Require(!string.IsNullOrEmpty(value), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "Path value"));
                if(this.path != value)
                    this.path = value;
            }
        }

        private AssumedTypes.Hash<string, string> items;
        /// <summary>Hash of keys (“pass_through”, “description” etc) and corresponding values.</summary>
        public AssumedTypes.Hash<string, string> Items
        {
            get { return this.items; }
            set 
            {
                Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "Items value"));
                this.items = value;
            }
        }
    }
}