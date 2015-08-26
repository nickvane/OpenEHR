using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEhr.Attributes
{
    public class AmTypeAttribute : Attribute
    {
        public AmTypeAttribute(string amTypeName)
        {
            this.amTypeName = amTypeName;
        }

        private string amTypeName;
        public string AmTypeName
        {
            get { return amTypeName; }
        }
    }
}
