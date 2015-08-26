using System;
using OpenEhr.RM.DataTypes.Basic;
using OpenEhr.DesignByContract;
using OpenEhr.Resources;
using OpenEhr.Attributes;

namespace OpenEhr.Futures.OperationalTemplate
{
    [AmType("T_COMPLEX_OBJECT")]
    public class TComplexObject : AM.Archetype.ConstraintModel.CComplexObject
    {
        private DataValue defaultValue;
        public DataValue DefaultValue
        {
            get { return this.defaultValue; }
            set
            {
                Check.Require(value != null, string.Format(CommonStrings.XMustNotBeNull, "DefaultValue value"));
                this.defaultValue = value;
            }
        }
    }
}