using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ObjectBuilder;
using OpenEhr.RM.Support.Terminology.Impl.Configuration;

namespace OpenEhr.RM.Support.Terminology.Impl
{
    public class TerminologyAccessProviderCustomFactory 
        : AssemblerBasedObjectFactory<ITerminologyAccessProvider,TerminologyAccessProviderData>
    {
        public static TerminologyAccessProviderCustomFactory Instance = new TerminologyAccessProviderCustomFactory();
    }
}
