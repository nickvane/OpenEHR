using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;

namespace OpenEhr.Futures.OperationalTemplate
{
    public class TConstraint
    {
        private AssumedTypes.List<TAttribute> attributes;
        public AssumedTypes.List<TAttribute> Attributes
        {
            get { return this.attributes; }
            set 
            {
                Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "Attributes value"));
                this.attributes = value; 
            }
        }
    }
}