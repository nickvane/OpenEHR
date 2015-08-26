using System;
using System.Configuration;

namespace OpenEhr.RM.Support.Terminology.Impl.Configuration
{
    public class TerminologyServiceData : TerminologyServiceProviderData
    {
        private const string terminologyAccessProvidersProperty = "terminologyAccessProviders";
        private const string codeSetAccessProvidersProperty = "codeSetAccessProviders";

        public TerminologyServiceData()
        { }

        public TerminologyServiceData(string name, Type type)
            : base(name, type)
        { }

        [ConfigurationProperty(terminologyAccessProvidersProperty, IsRequired = true)]
        public NameTypeConfigurationElementCollection<TerminologyAccessProviderData> TerminologyAccessProviders
        {
            get
            {
                return base[terminologyAccessProvidersProperty] as
                    NameTypeConfigurationElementCollection<TerminologyAccessProviderData>;
            }
        }

        [ConfigurationProperty(codeSetAccessProvidersProperty, IsRequired = true)]
        public NameTypeConfigurationElementCollection<CodeSetAccessProviderData> codeSetAccessProviders
        {
            get
            {
                return base[codeSetAccessProvidersProperty] as
                    NameTypeConfigurationElementCollection<CodeSetAccessProviderData>;
            }
        }
    }
}
