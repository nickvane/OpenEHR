using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEhr.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    class RmTypeAttribute : Attribute
    {
        public RmTypeAttribute(string rmOriginator, string rmName, string rmEntity)
        {
            this.rmEntity = rmEntity;
            this.rmName = rmName;
            this.rmOriginator = rmOriginator;
        }

        private string rmEntity;

        public string RmEntity
        {
            get { return this.rmEntity; }
            set { this.rmEntity = value; }
        }

        private string rmName;

        public string RmName
        {
            get { return this.rmName; }
            set { this.rmName = value; }
        }

        private string rmOriginator;

        public string RmOriginator
        {
            get { return this.rmOriginator; }
            set { this.rmOriginator = value; }
        }
    }
}
