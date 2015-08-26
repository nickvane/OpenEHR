using System;
using System.Configuration;

namespace OpenEhr.RM.Support.Terminology.Impl.Configuration
{
    public class CodeSetAccessData : CodeSetAccessProviderData
    {
        const string xmlFilePathProperty = "xmlFilePath";

        public CodeSetAccessData() { }
        public CodeSetAccessData(string name, Type type) : this(name, type, string.Empty) { }
        public CodeSetAccessData(string name, Type type, string xmlFilePath)
            : base(name, type)
        {
            xmlFilePath = xmlFilePath;
        }

        [ConfigurationProperty(xmlFilePathProperty, IsRequired = false)]
        public string XmlFilePath
        {
            get { return (string)base[xmlFilePathProperty]; }
            set { base[xmlFilePathProperty] = value; }
        }
    }
}
