using OpenEhr.RM.Support.Terminology.Impl.Configuration;

namespace OpenEhr.RM.Support.Terminology.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public class CodeSetAccessProviderCustomFactory : AssemblerBasedObjectFactory<ICodeSetAccessProvider, CodeSetAccessProviderData>
    {
        /// <summary>
        /// 
        /// </summary>
        public static CodeSetAccessProviderCustomFactory Instance = new CodeSetAccessProviderCustomFactory();
    }
}
