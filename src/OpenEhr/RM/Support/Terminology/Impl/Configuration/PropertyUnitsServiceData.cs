using System;
using System.Configuration;

namespace OpenEhr.RM.Support.Terminology.Impl.Configuration
{
    public class PropertyUnitsServiceData
    {
        const string XmlFilePathProperty = "xmlFilePath";
        const string TerminologyServiceProviderProperty = "terminologyServiceProvider";

        public PropertyUnitsServiceData() { }
        public PropertyUnitsServiceData(string name, Type type) : this(name,type,string.Empty,string.Empty) {}
        public PropertyUnitsServiceData(string name, Type type, string xmlFilePath) : this(name, type, xmlFilePath, string.Empty) { }
        public PropertyUnitsServiceData(string name, Type type, string xmlFilePath, string terminologyServiceProvider)
        {
            XmlFilePath = xmlFilePath;
            TerminologyServiceProvider = terminologyServiceProvider;
        }

        [ConfigurationProperty(XmlFilePathProperty, IsRequired = false)]
        public string XmlFilePath
        {
            get { return (string)base[XmlFilePathProperty]; }
            set { base[XmlFilePathProperty] = value; }
        }

        [ConfigurationProperty(TerminologyServiceProviderProperty, IsRequired = false)]
        public string TerminologyServiceProvider
        {
            get { return (string)base[TerminologyServiceProviderProperty]; }
            set { base[TerminologyServiceProviderProperty] = value; }
        }
    }
}
