using System;
using System.Text.RegularExpressions;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;

namespace OpenEhr.RM.Support.Identification
{
    [Serializable]
    [RmType("openEHR", "SUPPORT", "ISO_OID")]
    public class IsoOid: Uid
    {
        public IsoOid(string value)
            : base(value)
        {
            Check.Ensure(IsValid(Value));
        }

        private const string pattern = @"^^(\d{1}\.)(\d*\.)*";

        protected override bool IsValidValue(string value)
        {
            return IsValid(value);
        }

        public static bool IsValid(string value)
        {
            Check.Require(value != null, "value must not be null");

            return Regex.IsMatch(value, pattern, RegexOptions.Compiled | RegexOptions.Singleline);
        }
    }
}
