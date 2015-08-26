using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEhr.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RmTerminologyAttribute : Attribute
    {
        public RmTerminologyAttribute(string groupName)
        {
            this.groupName = groupName;
        }

        string groupName;

        public string GroupName
        {
            get { return groupName; }
            set { groupName = value; }
        }

        const string terminologyId = "openehr";

        public string TerminologyId
        {
            get { return terminologyId; }
        }
    }
}
