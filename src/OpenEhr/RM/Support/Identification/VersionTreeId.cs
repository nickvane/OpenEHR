using System;
using OpenEhr.DesignByContract;
using System.Text.RegularExpressions;
using OpenEhr.Attributes;

namespace OpenEhr.RM.Support.Identification
{
    [Serializable]
    [RmType("openEHR", "SUPPORT", "VERSION_TREE_ID")]
    public class VersionTreeId
    {
        const string pattern = @"^(?<trunk_version>\d+)(\.(?<branch_number>\d+)\.(?<branch_version>\d+))?$";

        public static bool IsValid(string value)
        {
            Check.Require(value != null, "value must not be null");

            return Regex.IsMatch(value, pattern, RegexOptions.Compiled | RegexOptions.Singleline);
        }

        public VersionTreeId()
            : this("1")
        { }

        public VersionTreeId(string value)
        {
            this.Value = value; 
        }

        private string value;
        private Match valueMatch;

        public String Value
        {
            get { return this.value; }
            set
            {
                Check.Require(IsValid(value), "value must be valid VERSION_TREE_ID");

                this.value = value;
                valueMatch = Regex.Match(value, pattern, RegexOptions.Compiled | RegexOptions.Singleline);

                Check.Ensure(valueMatch != null, "valueMatch must not be null");
                Check.Ensure(valueMatch.Success, "valueMatch success must be true");
                Check.Ensure(valueMatch.Groups["trunk_version"] != null, "valueMatch must contain trunk_version group");
                Check.Ensure(!string.IsNullOrEmpty(valueMatch.Groups["trunk_version"].Value), "trunk_version group value must not be null or empty");
            }
        }

        public string TrunkVersion
        {
            get { return valueMatch.Groups["trunk_version"].Value; }
        }

        public string BranchNumber
        {
            get
            {
                Group group = valueMatch.Groups["branch_number"];

                if (group.Success)
                    return group.Value;
                else
                    return null;
            }
        }

        public string BranchVersion
        {
            get
            {
                Group group = valueMatch.Groups["branch_version"];

                if (group.Success)
                    return group.Value;
                else
                    return null;
            }
        }

        public bool IsBranch()
        {
            return (BranchVersion != null);
        }

        public bool IsFirst()
        {
            return int.Parse(this.TrunkVersion, System.Globalization.NumberStyles.Integer) == 1;
        }
    }
}
