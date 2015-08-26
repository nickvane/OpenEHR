using System;
using OpenEhr.AssumedTypes.Impl;
using OpenEhr.RM.Common.Archetyped;


namespace OpenEhr.Factories
{
    public abstract class DemograpicFactory
    {
        static DemograpicFactory  instance = new OpenEhrV1DemographicFactory();

        /// <summary>
        /// DemographicFactory singleton instance
        /// </summary>
        public static DemograpicFactory Instance
        {
            get { return instance; }
        }

        public abstract OpenEhr.AssumedTypes.List<T> List<T>()  
            where T : Pathable;
        public abstract OpenEhr.AssumedTypes.List<T> List<T>(System.Collections.Generic.IEnumerable<T> items) 
            where T : Pathable;
        public abstract OpenEhr.AssumedTypes.Set<T> Set<T>()
            where T : Pathable, ILocatable;
        public abstract OpenEhr.AssumedTypes.Set<T> Set<T>(System.Collections.Generic.IEnumerable<T> items)
            where T : Pathable, ILocatable;
    }

    class OpenEhrV1DemographicFactory
        : DemograpicFactory
    {
        public override OpenEhr.AssumedTypes.List<T> List<T>()
        {
            return new PathableList<T>();
        }

        public override OpenEhr.AssumedTypes.List<T> List<T>(System.Collections.Generic.IEnumerable<T> items)
        {
            return new PathableList<T>(items);
        }

        public override OpenEhr.AssumedTypes.Set<T> Set<T>()
        {
            return new LocatableSet<T>();
        }

        public override OpenEhr.AssumedTypes.Set<T> Set<T>(System.Collections.Generic.IEnumerable<T> items)
        {
            return new LocatableSet<T>(items);
        }
    }
}
