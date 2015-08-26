using System;
using System.Text.RegularExpressions;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;

namespace OpenEhr.RM.Support.Identification
{
    [Serializable]
    [RmType("openEHR", "SUPPORT", "UUID")]
    public class Uuid: Uid
    {
        public Uuid(string value)
            : base(value)
        {
            Check.Ensure(IsValid(Value));
        }       

        const string pattern = @"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$";

        static Regex uuidRegEx = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline);

        protected override bool IsValidValue(string value)
        {
            return IsValid(value);
        }

        public static bool IsValid(string value)
        {
            Check.Require(value != null, "value must not be null");

            return uuidRegEx.IsMatch(value);
        }
    }
}
