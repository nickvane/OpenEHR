using System;
using OpenEhr.DesignByContract;

namespace OpenEhr.RM.Support.Terminology.Impl.Configuration
{
    public class TerminologyServiceProviderData : NameTypeConfigurationElement
    {
        public TerminologyServiceProviderData() { }

        public TerminologyServiceProviderData(string name, Type type) : base(name, type) { }
    }

    internal class TerminologyServiceDataRetriever : IConfigurationNameMapper
    {
        #region IConfigurationNameMapper Members

        public string MapName(string name, IConfigurationSource configSource)
        {
            if (string.IsNullOrEmpty(name))
            {
                TerminologyServiceSettings settings =
                    configSource.GetSection(TerminologyServiceSettings.SectionName) as TerminologyServiceSettings;

                if (settings == null)
                    throw new ApplicationException(TerminologyServiceSettings.SectionName + " configuration section not found");
                name = settings.DefaultProvider;
            }

            Check.Ensure(!string.IsNullOrEmpty(name));
            return name;
        }

        #endregion
    }
}
