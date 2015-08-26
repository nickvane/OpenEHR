using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ObjectBuilder;
using System.Collections;

namespace OpenEhr.RM.Support.Terminology.Impl
{
    public class TerminologyServiceFactory : LocatorNameTypeFactoryBase<ITerminologyServiceProvider>
    {        
        private TerminologyServiceFactory(IConfigurationSource configurationSource)
            : base(configurationSource) { }

        public static ITerminologyService Create(IConfigurationSource configurationSource)
        {
            return GetFactory(configurationSource).CreateDefault();
        }

        public static ITerminologyService Create(IConfigurationSource configurationSource, string name)
        {
            return GetFactory(configurationSource).Create(name);
        }

        private static Hashtable factories = Hashtable.Synchronized(new Hashtable());

        private static TerminologyServiceFactory GetFactory(IConfigurationSource configurationSource)
        {
            if (!factories.Contains(configurationSource))
                factories.Add(configurationSource, new TerminologyServiceFactory(configurationSource));

            return (TerminologyServiceFactory)factories[configurationSource];
        }
    }
}
