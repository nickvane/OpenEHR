using System;
using OpenEhr.DesignByContract;
using OpenEhr.Attributes;

namespace OpenEhr.RM.Support.Identification
{
    [Serializable]
    [RmType("openEHR", "COMMON", "UID_BASED_ID")]
    public abstract class UidBasedId : ObjectId
    {
        private const string seperator = "::";

        string[] strings;

        private string[] Strings
        {
            get
            {
                if (strings == null)
                    strings = this.Value.Split(new string[] { seperator }, 2, StringSplitOptions.RemoveEmptyEntries);

                Check.Ensure(strings.Length > 0, "strings must not be empty"); 
                return strings;
            }
        }

        private Uid root;

        public Uid Root
        {
            get
            {
                if (this.root == null)
                {                    
                    root = Uid.Create(Strings[0]);
                }
                return root;
            }
        }

        public string Extension
        {
            get
            {
                if (Strings.Length > 1)
                    return Strings[1];
                else
                    return string.Empty;
            }
        }

        public bool HasExtension
        {
            get { return Strings.Length > 1; }
        }
    }
}
