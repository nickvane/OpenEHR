using System;
using System.Text;
using OpenEhr.AssumedTypes;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;

namespace OpenEhr.Futures.OperationalTemplate
{
    public class TAttribute
    {
        private string rmAttributeName;
        /// <summary>Name of reference model attribute being constrained by C_OBJECT at differential path</summary>
        public string RmAttributeName
        {
            get { return this.rmAttributeName; }
            set
            {
                Check.Require(!string.IsNullOrEmpty(value), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "RmAttributeName value"));
                this.rmAttributeName = value;
            }
        }

        private List<TComplexObject> children;
        /// <summary>Overlaid complex object constraints for attribute at differential path</summary>
        public List<TComplexObject> Children
        {
            get { return this.children; }
            set
            {
                Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "Children value"));
                this.children = value; 
            }
        }

        private string differentialPath;
        public string DifferentialPath
        {
            get { return this.differentialPath; }
            set
            {
                Check.Require(!string.IsNullOrEmpty(value), string.Format(CommonStrings.XMustNotBeNullOrEmpty, "DifferentialPath value"));
                this.differentialPath = value;
            }
        }
    }
}