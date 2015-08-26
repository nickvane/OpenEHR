using System.Collections.Generic;

namespace OpenEhr.RM.Support.Terminology.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPropertyUnitsService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="openEhrPropertyCode"></param>
        /// <returns></returns>
        IList<Unit> Units(string openEhrPropertyCode);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IList<Unit> Units();
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class Unit
    {
        private readonly string _name = null;
        private readonly string _symbol = null;

        internal Unit(string name, string symbol)
        {
            this._name = name;
            this._symbol = symbol;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Symbol
        { get { return this._symbol; } }

        /// <summary>
        /// 
        /// </summary>
        public string Name
        { get { return this._name; } }

    }
}
