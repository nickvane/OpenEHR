using System.Configuration;

namespace OpenEhr.RM.Support.Terminology.Impl.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class TerminologyServiceSettings : SerializableConfigurationSection
    {
        public const string SectionName = "terminologyServiceConfiguration";

        private const string defaultProviderProperty = "defaultProvider";
        private const string terminologyServiceProvidersProperty = "terminologyServiceProviders";

        public TerminologyServiceSettings() : this(string.Empty) { }

        public TerminologyServiceSettings(string defaultProvider)
        {
            this.DefaultProvider = defaultProvider;
        }

        public static TerminologyServiceSettings GetTerminologyServiceSettings(IConfigurationSource configurationSource)
        {
            return (TerminologyServiceSettings)configurationSource.GetSection(TerminologyServiceSettings.SectionName);
        }

        [ConfigurationProperty(defaultProviderProperty, IsRequired = true)]
        public string DefaultProvider
        {
            get { return (string)base[defaultProviderProperty]; }
            set { base[defaultProviderProperty] = value; }
        }

        [ConfigurationProperty(terminologyServiceProvidersProperty, IsRequired = true)]
        public NameTypeConfigurationElementCollection<TerminologyServiceProviderData> TerminologyServiceProviders
        {
            get { return (NameTypeConfigurationElementCollection<TerminologyServiceProviderData>)base[terminologyServiceProvidersProperty]; }
        }
    }
}
