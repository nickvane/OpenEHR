using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEhr.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RmAttributeAttribute : Attribute
    {
        public RmAttributeAttribute(string attributeName)
            :this (attributeName, 0)
        { }

        public RmAttributeAttribute(string attributeName, int lowerExistence)
        {
            this.attributeName = attributeName;
            this.lowerExistence = lowerExistence;
        }

        private string attributeName;

        public string AttributeName
        {
            get { return this.attributeName; }
            set { this.attributeName = value; }
        }

        private int lowerExistence;

        public int LowerExistence
        {
            get { return this.lowerExistence; }
            set { this.lowerExistence = value; }
        }
    }
}
