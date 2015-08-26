using System;
using System.Collections.Generic;

namespace OpenEhr.RM.Support.Terminology.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public class PropertyUnitsService : IPropertyUnitsService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="terminologyServiceProvider"></param>
        public PropertyUnitsService(string terminologyServiceProvider) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="terminologyServiceProvider"></param>
        /// <param name="xmlFilenamePath"></param>
        public PropertyUnitsService(string terminologyServiceProvider, string xmlFilenamePath) { }

        #region IPropertyUnitsService Members

        public IList<Unit> Units(string openEhrPropertyCode)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public IList<Unit> Units()
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #endregion
    }
}
