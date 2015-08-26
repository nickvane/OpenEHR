using System;
using System.Text.RegularExpressions;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;

namespace OpenEhr.RM.Support.Identification
{
    [Serializable]
    [RmType("openEHR", "SUPPORT", "INTERNET_ID")]
    public class InternetId: Uid
    {
        public InternetId(string value)
            : base(value)
        {
            Check.Ensure(IsValid(Value));
        }

        private const string pattern = @"^[a-zA-Z]([a-zA-Z0-9-]*[a-zA-Z0-9])?(\.[a-zA-Z]([a-zA-Z0-9-]*[a-zA-Z0-9]))*$";

        protected override bool IsValidValue(string value)
        {
            return IsValidValue(value);
        }

        public static bool IsValid(string value)
        {
            Check.Require(value != null, "value must not be null or empty");

            return Regex.IsMatch(value, pattern, RegexOptions.Compiled | RegexOptions.Singleline);
        }
    }
}
