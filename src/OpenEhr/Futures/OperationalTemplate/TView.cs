using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;

namespace OpenEhr.Futures.OperationalTemplate
{
    public class TView
    {
        private AssumedTypes.List<TViewConstraint> constraints;
        public AssumedTypes.List<TViewConstraint> Constraints
        {
            get { return this.constraints; }
            set
            {
                Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "Constraints value"));
                this.constraints = value;
            }
        }
    }
}