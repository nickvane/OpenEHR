using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEhr.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RmCodesetAttribute : Attribute
    {

        public RmCodesetAttribute(string codesetId, string externalId)
        {
            this.codesetId = codesetId;
            this.externalId = externalId;
        }

        string codesetId;

        public string CodesetId
        {
            get { return codesetId; }
            set { codesetId = value; }
        }

        string externalId;

        public string ExternalId
        {
            get { return externalId; }
            set { externalId = value; }
        }
    }
}
