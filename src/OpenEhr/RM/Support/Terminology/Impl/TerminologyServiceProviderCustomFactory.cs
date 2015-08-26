using System.Collections.Generic;
using OpenEhr.RM.Support.Terminology.Impl.Configuration;
using OpenEhr.DesignByContract;
using System.Configuration;

namespace OpenEhr.RM.Support.Terminology.Impl
{
    public class TerminologyServiceProviderCustomFactory : ICustomFactory  
    {
        #region ICustomFactory Members

        public object CreateObject(Microsoft.Practices.ObjectBuilder.IBuilderContext context, string name, IConfigurationSource configurationSource, ConfigurationReflectionCache reflectionCache)
        {
            TerminologyServiceSettings settings =
                configurationSource.GetSection(TerminologyServiceSettings.SectionName) as TerminologyServiceSettings;

            Check.Assert(settings != null, "settings must not be null.");
            TerminologyServiceData data = settings.TerminologyServiceProviders.Get(name) as TerminologyServiceData;
            if (data == null)
                throw new ConfigurationErrorsException("Unable to find Terminology service provider. " + name);

            Dictionary<string, ICodeSetAccess> codeSetAccessDictionary = new Dictionary<string, ICodeSetAccess>();

            foreach (CodeSetAccessProviderData codeSetAccessProviderData in data.codeSetAccessProviders)
            {
                codeSetAccessDictionary.Add(codeSetAccessProviderData.Name,
                    (ICodeSetAccess)CodeSetAccessProviderCustomFactory.Instance.Create
                    (context, codeSetAccessProviderData, configurationSource, reflectionCache));
            }

            Dictionary<string, ITerminologyAccess> terminologyAccessDictionary = new Dictionary<string, ITerminologyAccess>();

            foreach (TerminologyAccessProviderData terminologyAccessProviderData in data.TerminologyAccessProviders)
            {
                terminologyAccessDictionary.Add(terminologyAccessProviderData.Name,
                    (ITerminologyAccess)TerminologyAccessProviderCustomFactory.Instance.Create
                    (context, terminologyAccessProviderData, configurationSource, reflectionCache));
            }

            ITerminologyService terminologyService = new TerminologyService(terminologyAccessDictionary, codeSetAccessDictionary);

            if (context.Locator != null)
            {
                ILifetimeContainer lifetime = context.Locator.Get<ILifetimeContainer>(typeof(ILifetimeContainer), SearchMode.Local);
                if (lifetime != null)
                {
                    context.Locator.Add(new DependencyResolutionLocatorKey(typeof(ITerminologyServiceProvider), name), terminologyService);
                    lifetime.Add(terminologyService);
                }
            }

            return terminologyService;
        }

        #endregion
    }
}
